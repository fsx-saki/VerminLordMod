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
    public class DiCangSheng : GuMasterBase
    {
        private int _attackTimer;
        private int _stompCooldown;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.AncientBeast;
        public override GuPersonality GetPersonality() => GuPersonality.Ferocious;

        public override string GuMasterDisplayName => "帝藏生";
        public override int GuMasterDamage => 450;
        public override int GuMasterLife => 65000;
        public override int GuMasterDefense => 200;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 1500;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 18;
            NPCID.Sets.AttackAverageChance[Type] = 8;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;

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
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(2, 50, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _stompCooldown = 0;
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

            if (_stompCooldown > 0) _stompCooldown--;

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

            NPC.velocity.X = dir * 3f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -10f;

            _attackTimer++;
            if (_attackTimer >= 25)
            {
                _attackTimer = 0;
                ExecuteAttack(target);
            }

            if (_stompCooldown <= 0 && dist < 300f)
            {
                EarthquakeStomp(target);
                _stompCooldown = 360;
            }
        }

        private void ExecuteAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 4;

            int pattern = Main.rand.Next(3);
            switch (pattern)
            {
                case 0:
                    CombatText.NewText(NPC.getRect(), Color.Orange, "吼——！", true);
                    for (int i = -4; i <= 4; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.08f;
                        Vector2 vel = angle.ToRotationVector2() * 11f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.DD2BetsyFireball, damage, 3f, Main.myPlayer);
                    }
                    break;
                case 1:
                    for (int i = 0; i < 16; i++)
                    {
                        float angle = MathHelper.TwoPi / 16f * i;
                        Vector2 vel = angle.ToRotationVector2() * 6f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.CultistBossFireBall, damage, 2f, Main.myPlayer);
                    }
                    break;
                case 2:
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 spawnPos = target.Center + new Vector2(Main.rand.Next(-250, 250), -500);
                        Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), 12f);
                        Projectile.NewProjectile(source, spawnPos, vel, ProjectileID.Meteor1, damage * 2, 5f, Main.myPlayer);
                    }
                    break;
            }
        }

        private void EarthquakeStomp(Player target)
        {
            CombatText.NewText(NPC.getRect(), Color.Orange, "孽龙之怒！", true);

            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.Torch,
                    Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f),
                    100, Color.Orange, 2f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                int damage = NPC.damage / 3;

                for (int i = 0; i < 24; i++)
                {
                    float angle = MathHelper.TwoPi / 24f * i;
                    Vector2 vel = angle.ToRotationVector2() * 8f;
                    Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.BoulderStaffOfEarth, damage, 3f, Main.myPlayer);
                }
            }

            foreach (Player p in Main.player)
            {
                if (p.active && !p.dead && Vector2.Distance(NPC.Center, p.Center) < 400f)
                {
                    p.AddBuff(BuffID.Slow, 180);
                    p.AddBuff(BuffID.Weak, 120);
                }
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "吼——！",
                GuAttitude.Wary => "吼……",
                GuAttitude.Friendly => "吼……吼。",
                GuAttitude.Respectful => "吼！",
                GuAttitude.Contemptuous => "吼……",
                GuAttitude.Fearful => "吼……！？",
                _ => "吼——！"
            };
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("DiCangSheng", "greeting");

            b.StartNode("greeting", "帝藏生——太古荒兽孽龙，发出低沉的咆哮。它的智慧有限，但你能感受到它体内蕴含的恐怖力量。")
                .AddOption("尝试沟通", "communicate", DialogueOptionType.Risky)
                .AddOption("关于龙宫", "dragon_palace", DialogueOptionType.Informative)
                .AddOption("太古荒兽", "ancient_beast", DialogueOptionType.Informative)
                .AddOption("交易（如果它理解的话）", "trade", DialogueOptionType.Trade)
                .AddOption("离开", "bye", DialogueOptionType.Exit);

            b.StartNode("communicate", "帝藏生歪了歪巨大的头颅，似乎在努力理解你的话语。它发出一声低吼——你隐约感觉到，它在说'不……伤害……'")
                .AddOption("你被龙宫控制了？", "controlled")
                .AddOption("你想要自由？", "freedom")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("controlled", "帝藏生发出痛苦的嘶吼：'龙宫……控制……帝藏生……不想……'它的眼中闪过一丝清明，但很快又被狂暴取代。")
                .AddOption("谁能解除控制？", "free_method")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("free_method", "帝藏生低吼：'龙宫……主人……解除……'它似乎在说，只有龙宫的主人才能解除对它的控制。")
                .AddOption("回到沟通", "communicate");

            b.StartNode("freedom", "帝藏生的眼中闪过一丝渴望：'自由……帝藏生……想要……'但随即，它的眼神又变得狂暴，龙宫的控制再次占据了上风。")
                .AddOption("回到沟通", "communicate");

            b.StartNode("dragon_palace", "帝藏生听到'龙宫'二字，浑身颤抖：'龙宫……主人……命令……'它似乎被龙宫的指令所束缚，无法违抗。")
                .AddOption("龙宫的主人是谁？", "dragon_palace_master")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("dragon_palace_master", "帝藏生：'吴帅……龙宫……主人……'它似乎在告诉你，龙宫的主人名叫吴帅。")
                .AddOption("回到龙宫", "dragon_palace");

            b.StartNode("ancient_beast", "帝藏生发出一声长啸，仿佛在回忆远古的岁月。太古荒兽——蛊世界最古老的存在之一，拥有毁天灭地的力量。")
                .AddOption("太古荒兽有多强？", "beast_power")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("beast_power", "帝藏生骄傲地昂起头：'太古……最强……'虽然它的语言能力有限，但你能感受到它对自己力量的自信——太古荒兽，确实是蛊世界最强大的存在之一。")
                .AddOption("回到太古荒兽", "ancient_beast");

            b.StartNode("trade", "帝藏生困惑地看着你——它似乎不理解'交易'的概念。但它从身上抖落了一些鳞片，似乎算是'礼物'。")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "帝藏生发出一声低吼，似乎在告别。")
                .AddOption("离开", "bye", DialogueOptionType.Exit);

            var tree = b.Build();
            tree.NPCType = Type;
            DialogueTreeManager.Instance.RegisterTree(tree);
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            if (DialogueTreeManager.Instance.HasTree(NPC))
            {
                button = "对话";
                button2 = "";
                return;
            }
            button = "对话";
            button2 = "";
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
        }

        public override bool CanChat() => CurrentAttitude != GuAttitude.Hostile || NPC.life > NPC.lifeMax * 0.5f;

        public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0f;

        public override List<string> SetNPCNameList() => new List<string> { "帝藏生" };
    }
}
