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
    [AutoloadHead]
    public class QingChou : GuMasterBase
    {
        private int _attackTimer;
        private float _rageMultiplier;
        private bool _rageMode;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.AncientLegendary;
        public override GuPersonality GetPersonality() => GuPersonality.Vengeful;

        public override string GuMasterDisplayName => "青仇";
        public override int GuMasterDamage => 300;
        public override int GuMasterLife => 50000;
        public override int GuMasterDefense => 180;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 1200;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 22;
            NPCID.Sets.AttackAverageChance[Type] = 12;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
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
            NPC.knockBackResist = 0.1f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(2, 0, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _rageMultiplier = 1f;
            _rageMode = false;
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

            float healthPercent = (float)NPC.life / NPC.lifeMax;
            _rageMultiplier = 1f + (1f - healthPercent) * 1.5f;

            if (!_rageMode && healthPercent < 0.3f)
            {
                ActivateRageMode();
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

            float speed = 2.5f * _rageMultiplier;
            NPC.velocity.X = dir * Math.Min(speed, 5f);

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -8f;

            _attackTimer++;
            int attackInterval = Math.Max(15, (int)(40 / _rageMultiplier));
            if (_attackTimer >= attackInterval)
            {
                _attackTimer = 0;
                ExecuteAttack(target);
            }
        }

        private void ExecuteAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = (int)(NPC.damage / 4 * _rageMultiplier);

            int pattern = Main.rand.Next(3);
            switch (pattern)
            {
                case 0:
                    CombatText.NewText(NPC.getRect(), Color.DarkGreen, "青仇杀招——仇恨之击！", true);
                    for (int i = -3; i <= 3; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.09f;
                        Vector2 vel = angle.ToRotationVector2() * (8f * _rageMultiplier);
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.DD2BetsyFireball, damage, 2f, Main.myPlayer);
                    }
                    break;
                case 1:
                    for (int i = 0; i < (int)(8 * _rageMultiplier); i++)
                    {
                        float angle = MathHelper.TwoPi / (8f * _rageMultiplier) * i;
                        Vector2 vel = angle.ToRotationVector2() * (5f * _rageMultiplier);
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.CultistBossLightningOrb, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 2:
                    NPC.velocity.X += Math.Sign(target.Center.X - NPC.Center.X) * 4f * _rageMultiplier;
                    NPC.velocity.Y = -6f;
                    CombatText.NewText(NPC.getRect(), Color.DarkGreen, "仇恨冲锋！", true);
                    break;
            }
        }

        private void ActivateRageMode()
        {
            _rageMode = true;
            CombatText.NewText(NPC.getRect(), Color.DarkRed, "仇恨……永不止息！", true);

            NPC.damage = (int)(NPC.defDamage * 2f);
            NPC.defense = (int)(NPC.defDefense * 0.5f);

            for (int i = 0; i < 40; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.GreenTorch,
                    Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f),
                    100, Color.DarkGreen, 2.5f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                int damage = NPC.damage / 3;
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi / 20f * i;
                    Vector2 vel = angle.ToRotationVector2() * 9f;
                    Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.ShadowFlame, damage, 2f, Main.myPlayer);
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
                    1 => "仇恨……永不止息！",
                    2 => "青家的仇恨……我要用血来偿还！",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "你……你是什么人？不要妨碍我的仇恨！",
                GuAttitude.Friendly => "你……你不恨我？奇怪……但我的仇恨不会消减。",
                GuAttitude.Respectful => "你很强……但我的仇恨比你的力量更强。",
                GuAttitude.Contemptuous => "你不懂仇恨……你什么都不懂。",
                GuAttitude.Fearful => "你……你竟让我的仇恨动摇……不可能！",
                _ => "仇恨……永不止息！"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "青家的血债，必须用血来偿还！",
                "仇恨让我变强……你的攻击只会增加我的仇恨！",
                "越痛……越恨……越强！",
                "我生于仇恨，也将死于仇恨！",
                "太古传奇的仇恨，岂是凡人能理解的？",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("QingChou", "greeting");

            b.StartNode("greeting", "青仇浑身散发着幽绿色的光芒，眼中燃烧着永恒的仇恨之火。")
                .AddOption("谈论仇恨道", "chouhen_dao", DialogueOptionType.Teach)
                .AddOption("青家的悲剧", "qing_family", DialogueOptionType.Informative)
                .AddOption("你的诞生", "origin", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("chouhen_dao", "青仇：\"仇恨道……这是我的道。仇恨让我诞生，仇恨让我变强，仇恨是我存在的全部。越是被伤害，我就越强大——这就是仇恨道的真谛。\"")
                .AddOption("仇恨道的极限？", "chouhen_limit")
                .AddOption("仇恨能消解吗？", "chouhen_resolve")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("chouhen_limit", "青仇：\"极限？没有极限。仇恨是无穷的——只要有人被伤害，就会有仇恨。只要仇恨存在，我就会越来越强。\"")
                .AddOption("回到仇恨道", "chouhen_dao");

            b.StartNode("chouhen_resolve", "青仇沉默良久：\"消解？……我不知道。也许有一天，当青家的血债被偿还，我的仇恨会消解。但那一天……可能永远不会到来。\"")
                .AddOption("回到仇恨道", "chouhen_dao");

            b.StartNode("qing_family", "青仇的仇恨之火燃烧得更旺：\"青家……曾经是南疆的大家族。但有人灭了青家满门，只为了夺取青家的传承。那场屠杀……诞生了我。\"")
                .AddOption("谁灭了青家？", "qing_destroyer")
                .AddOption("青家的传承是什么？", "qing_legacy")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("qing_destroyer", "青仇：\"谁灭了青家？……我不知道。我只知道仇恨，不知道仇人。但我会找到他们——一个一个地找，一个一个地杀。\"")
                .AddOption("回到青家", "qing_family");

            b.StartNode("qing_legacy", "青仇：\"青家的传承？仇恨道。青家世代修炼仇恨道，最终……他们的仇恨凝聚成了我。我是青家仇恨的化身，也是青家最后的传承。\"")
                .AddOption("回到青家", "qing_family");

            b.StartNode("origin", "青仇：\"我的诞生？我是青家灭门时，所有族人的仇恨凝聚而成的太古传奇。我不是人，也不是兽——我是纯粹的仇恨。\"")
                .AddOption("你有自己的意志吗？", "own_will")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("own_will", "青仇沉默了：\"意志？……我不确定。我的行动由仇恨驱动，但有时候……我也会思考，仇恨之外是否还有别的什么。但每次思考，都会被更强烈的仇恨淹没。\"")
                .AddOption("回到你的诞生", "origin");

            b.StartNode("trade", "青仇：\"交易？……可以。但别指望我会友善。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "青仇转身离去：\"仇恨……永不止息。\"")
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

        public override List<string> SetNPCNameList() => new List<string> { "青仇" };
    }
}
