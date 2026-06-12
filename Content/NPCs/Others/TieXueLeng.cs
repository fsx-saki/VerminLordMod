using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Common.UI.DialogueTreeUI;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Content.NPCs.Others
{
    public class TieXueLeng : GuMasterBase
    {
        private int _attackTimer;
        private int _markTimer;
        private bool _playerMarked;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.Tie;
        public override GuRank GetRank() => GuRank.Zhuan4_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Righteous;

        public override string GuMasterDisplayName => "铁血冷";
        public override int GuMasterDamage => 100;
        public override int GuMasterLife => 15000;
        public override int GuMasterDefense => 60;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 600;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 40;
            NPCID.Sets.AttackAverageChance[Type] = 30;
            NPCID.Sets.HatOffsetY[Type] = 4;

            if (!NPCID.Sets.NPCBestiaryDrawOffset.ContainsKey(Type))
            {
                NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers { Velocity = 1f, Direction = 1 });
            }
        }

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = GuMasterDamage;
            NPC.lifeMax = GuMasterLife;
            NPC.defense = GuMasterDefense;
            NPC.knockBackResist = 0.3f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(0, 20, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _markTimer = 0;
            _playerMarked = false;
        }

        public override void AI()
        {
            base.AI();

            if (!_dialogueTreeRegistered)
            {
                _dialogueTreeRegistered = true;
                if (!RegisteredDialogueTreeTypes.Contains(Type))
                {
                    RegisteredDialogueTreeTypes.Add(Type);
                    RegisterDialogueTree();
                }
            }

            if (CurrentAttitude == GuAttitude.Hostile || (HasBeenHitByPlayer && AggroTimer > 0))
            {
                ExecuteCombatAI();
            }
        }

        private void ExecuteCombatAI()
        {
            NPC.TargetClosest(true);
            var target = Main.player[NPC.target];
            if (!target.active || target.dead) { NPC.velocity *= 0.95f; return; }

            float dist = Vector2.Distance(NPC.Center, target.Center);
            float dir = target.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = (int)dir;

            NPC.velocity.X = dir * 1.8f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -5f;

            _attackTimer++;
            if (_attackTimer >= 50)
            {
                _attackTimer = 0;
                ExecuteAttack(target);
            }

            if (!_playerMarked)
            {
                _markTimer++;
                if (_markTimer >= 120)
                {
                    _markTimer = 0;
                    MarkTarget(target);
                }
            }
        }

        private void ExecuteAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 3;

            int pattern = Main.rand.Next(2);
            switch (pattern)
            {
                case 0:
                    for (int i = -2; i <= 2; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.1f;
                        Vector2 vel = angle.ToRotationVector2() * 9f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.IchorBullet, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 1:
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 11f,
                        ProjectileID.ChlorophyteBullet, damage, 1f, Main.myPlayer);
                    break;
            }
        }

        private void MarkTarget(Player target)
        {
            _playerMarked = true;
            CombatText.NewText(NPC.getRect(), Color.Yellow, "律道——标记！", true);

            target.AddBuff(BuffID.OnFire, 300);

            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(target.Center, 20, 20, DustID.IchorTorch,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f),
                    100, Color.Yellow, 1.2f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                int damage = NPC.damage / 4;

                for (int i = 0; i < 4; i++)
                {
                    Vector2 spawnPos = target.Center + new Vector2(Main.rand.Next(-150, 150), -250);
                    Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.UnitY) * 8f;
                    Projectile.NewProjectile(source, spawnPos, vel, ProjectileID.IchorBullet, damage, 1f, Main.myPlayer);
                }
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => NumberOfTimesTalkedTo switch
                {
                    1 => "真相只有一个。",
                    2 => "你犯下的罪行，铁家一一记录在案！",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "你的行为……我会持续关注。",
                GuAttitude.Friendly => "铁家的神捕，愿与正义之士为友。",
                GuAttitude.Respectful => "你的正义之心，令铁血冷敬佩。",
                GuAttitude.Contemptuous => "罪犯就是罪犯，无论你如何狡辩。",
                GuAttitude.Fearful => "你……你竟有如此实力……但律法不会屈服！",
                _ => "真相只有一个。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "律道之下，罪行无所遁形！",
                "铁家的神捕，绝不会放过任何罪犯！",
                "你已被标记，逃不掉的！",
                "天网恢恢，疏而不漏！",
                "正义或许迟到，但绝不会缺席！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("TieXueLeng", "greeting");

            b.StartNode("greeting", "铁血冷目光如炬，仿佛能看穿一切罪行。")
                .AddOption("谈论律道", "lv_dao", DialogueOptionType.Teach)
                .AddOption("铁家神捕", "tie_family", DialogueOptionType.Informative)
                .AddOption("追捕罪犯", "hunting", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("lv_dao", "铁血冷：\"律道？律道是秩序的道。律道修士可以制定规则、执行规则、惩罚违反规则之人。天网恢恢，疏而不漏——这就是律道的精髓。\"")
                .AddOption("律道如何战斗？", "lv_combat")
                .AddOption("标记之术？", "mark_skill")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("lv_combat", "铁血冷：\"律道的战斗方式？标记敌人，追踪到底。一旦被律道标记，无论你逃到天涯海角，都会被追踪弹找到。这就是律道的可怕之处。\"")
                .AddOption("回到律道", "lv_dao");

            b.StartNode("mark_skill", "铁血冷：\"标记之术是律道的基础。标记目标后，可以追踪其位置，也可以让攻击自动追踪。被标记的人，无处可逃。\"")
                .AddOption("回到律道", "lv_dao");

            b.StartNode("tie_family", "铁血冷：\"铁家……南疆的执法家族。我们世代替天行道，追捕罪犯，维护秩序。虽然铁家的实力不是最强，但我们的律道传承，让所有罪犯闻风丧胆。\"")
                .AddOption("铁家的传承？", "tie_legacy")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("tie_legacy", "铁血冷：\"铁家的律道传承可以追溯到上古时期。据说铁家先祖曾与天庭合作，制定了蛊世界的第一部律法。虽然如今律法已经名存实亡，但铁家的传承从未断绝。\"")
                .AddOption("回到铁家", "tie_family");

            b.StartNode("hunting", "铁血冷：\"追捕罪犯？这是我的职责。无论罪犯逃到哪里，我都会追到底。天网恢恢，疏而不漏——这不是空话，这是我的信念。\"")
                .AddOption("你追捕过最强的罪犯？", "strongest_criminal")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("strongest_criminal", "铁血冷：\"最强的罪犯……方源。他的罪行罄竹难书，但我只是四转修士，根本无力追捕他。但总有一天，我会亲手将他绳之以法。\"")
                .AddOption("回到追捕罪犯", "hunting");

            b.StartNode("trade", "铁血冷：\"交易？可以。铁家虽然不富裕，但律道的材料还是有些的。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "铁血冷拱手：\"道友保重，愿正义永存。\"")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            var tree = b.Build();
            tree.NPCType = Type;
            DialogueTreeManager.Instance.RegisterTree(tree);
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            if (DialogueTreeManager.Instance.HasTree(NPC))
            {
                button = "对话";
                button2 = CurrentAttitude != GuAttitude.Hostile ? "商店" : "";
                return;
            }
            button = "对话";
            button2 = CurrentAttitude != GuAttitude.Hostile ? "商店" : "";
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton)
            {
                if (DialogueTreeManager.Instance.HasTree(NPC))
                {
                    var mgr = DialogueTreeManager.Instance;
                    if (!mgr.HasActiveSession(Main.LocalPlayer))
                        mgr.StartDialogue(NPC, Main.LocalPlayer);

                    var currentText = mgr.GetCurrentNPCText(Main.LocalPlayer);
                    var options = mgr.GetCurrentOptions(Main.LocalPlayer);

                    if (options != null && options.Count > 0)
                    {
                        DialogueTreeUI.Instance.Open(NPC.GivenName, NPCHeadLoader.GetHeadSlot(HeadTexture), currentText ?? "", options);
                    }
                    else
                    {
                        mgr.EndDialogue(Main.LocalPlayer);
                        Main.npcChatText = currentText ?? GetDialogue(NPC, CurrentAttitude);
                    }
                    return;
                }
                Main.npcChatText = GetDialogue(NPC, CurrentAttitude);
            }
            else
            {
                shop = ShopName;
            }
        }

        public override bool CanChat() => CurrentAttitude != GuAttitude.Hostile || NPC.life > NPC.lifeMax * 0.5f;

        public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0f;

        public override List<string> SetNPCNameList() => new List<string> { "铁血冷" };
    }
}
