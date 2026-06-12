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

namespace VerminLordMod.Content.NPCs.FangYuan
{
    public class HeChunQiu : GuMasterBase
    {
        private int _attackTimer;
        private int _timeSlowTimer;
        private bool _timeSlowActive;
        private bool _hasUsedTimeReversal;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/FangYuan/HeChunQiu_Head";

        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.Zhuan8_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Cunning;

        public override string GuMasterDisplayName => "何春秋";
        public override int GuMasterDamage => 400;
        public override int GuMasterLife => 80000;
        public override int GuMasterDefense => 200;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 1200;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 25;
            NPCID.Sets.AttackAverageChance[Type] = 15;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Slow] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;

            if (!NPCID.Sets.NPCBestiaryDrawOffset.ContainsKey(Type))
            {
                var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers
                {
                    Velocity = 1f,
                    Direction = 1
                };
                NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
            }
        }

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = GuMasterDamage;
            NPC.lifeMax = GuMasterLife;
            NPC.defense = GuMasterDefense;
            NPC.knockBackResist = 0.05f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(5, 0, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _timeSlowTimer = 0;
            _timeSlowActive = false;
            _hasUsedTimeReversal = false;
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
                ExecuteTimeCombatAI();
            }

            TimeSlowUpdate();
            TimeReversalCheck();
        }

        private void TimeSlowUpdate()
        {
            if (_timeSlowActive)
            {
                _timeSlowTimer++;
                if (_timeSlowTimer > 180)
                {
                    _timeSlowActive = false;
                    _timeSlowTimer = 0;
                    CombatText.NewText(NPC.getRect(), Color.SkyBlue, "时间恢复正常", false);
                }
                else
                {
                    var target = Main.player[NPC.target];
                    if (target.active && !target.dead)
                    {
                        target.AddBuff(BuffID.Slow, 30);
                    }
                }
            }
        }

        private void TimeReversalCheck()
        {
            if (!_hasUsedTimeReversal && NPC.life <= NPC.lifeMax * 0.2f)
            {
                _hasUsedTimeReversal = true;
                NPC.life = (int)(NPC.lifeMax * 0.6f);
                NPC.HealEffect((int)(NPC.lifeMax * 0.4f), true);
                CombatText.NewText(NPC.getRect(), Color.Cyan, "春秋蝉——时光逆转！", true);

                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(NPC.Center, 20, 20, DustID.Shadowflame,
                        Main.rand.NextFloat(-5f, 5f),
                        Main.rand.NextFloat(-5f, 5f),
                        100, Color.Cyan, 2f);
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    var source = NPC.GetSource_FromAI();
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 6f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.CultistBossLightningOrb, NPC.damage / 5, 0f, Main.myPlayer);
                    }
                }

                Say("时间……在我手中流转。", Color.Cyan);
            }
        }

        private void ExecuteTimeCombatAI()
        {
            NPC.TargetClosest(true);
            var target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.velocity *= 0.95f;
                return;
            }

            float dist = Vector2.Distance(NPC.Center, target.Center);
            float dir = target.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = (int)dir;

            float speed = _timeSlowActive ? 6f : 3f;
            if (dist > 500f)
                NPC.velocity.X = dir * speed;
            else if (dist > 150f)
                NPC.velocity.X = dir * speed * 0.7f;
            else
                NPC.velocity.X = dir * speed * 0.4f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -8f;

            _attackTimer++;
            if (_attackTimer >= 45)
            {
                _attackTimer = 0;
                ExecuteTimeAttack(target);
            }
        }

        private void ExecuteTimeAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 5;

            int pattern = Main.rand.Next(3);

            switch (pattern)
            {
                case 0:
                    for (int i = -1; i <= 1; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.2f;
                        Vector2 velocity = angle.ToRotationVector2() * 9f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.CultistBossLightningOrb, damage, 0f, Main.myPlayer);
                    }
                    break;

                case 1:
                    _timeSlowActive = true;
                    _timeSlowTimer = 0;
                    CombatText.NewText(NPC.getRect(), Color.Cyan, "时间减速！", true);

                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi / 6f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 5f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.WaterBolt, damage, 1f, Main.myPlayer);
                    }
                    break;

                case 2:
                    NPC.velocity *= 2f;
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi / 12f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 7f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.IceBlock, damage, 2f, Main.myPlayer);
                    }
                    break;
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "时间不会为任何人停留……包括你。",
                GuAttitude.Wary => "你在观察我？明智。但时间不等人。",
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => "能被你尊重，说明我的时间没有白费。",
                GuAttitude.Contemptuous => "你的时间……已经不多了。",
                GuAttitude.Fearful => "恐惧？不……我只是在等待正确的时机。",
                _ => "时间如流水，永不停歇。"
            };
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "何春秋：\"宙道的奥义在于——过去、现在、未来，皆在掌中。\"",
                "何春秋：\"春秋蝉让我看到了时间的尽头。那里……什么都没有。\"",
                "何春秋：\"方源的本体比我更了解时间的价值。我只是他的一面。\"",
                "何春秋：\"你想了解宙道？先学会等待。等待是最被低估的力量。\"",
                "何春秋：\"时间加速，时间减速，时间停止……这些都是表象。真正的宙道，是理解因果。\"",
                "何春秋：\"我曾在时间的长河中逆流而上，看到了无数种可能。但最终——只有一种结局。\"",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        protected virtual void RegisterDialogueTree()
        {
            var tree = BuildDialogueTree();
            if (tree != null)
            {
                tree.NPCType = Type;
                DialogueTreeManager.Instance.RegisterTree(tree);
            }
        }

        protected virtual DialogueTree BuildDialogueTree()
        {
            var b = new DialogueTreeBuilder("FangYuan_HeChunQiu", "greeting");

            b.StartNode("greeting",
                "何春秋周身萦绕着淡淡的时间气息，目光深邃如星空。")
                .AddOption("询问宙道", "ask_time_dao", DialogueOptionType.Informative)
                .AddOption("关于春秋蝉", "ask_cicada", DialogueOptionType.Informative)
                .AddOption("关于方源", "ask_fangyuan", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_time_dao",
                "何春秋：\"宙道，掌控时间之道。过去已成定局，未来尚未书写，唯有现在——是可以改变的。\"")
                .AddOption("时间可以逆转吗？", "ask_time_reverse")
                .AddOption("因果是什么？", "ask_cause_effect")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_time_reverse",
                "何春秋：\"时间逆转……春秋蝉可以做到。但代价极大。每一次逆转，都在消耗我存在的根基。\"")
                .AddOption("你不怕消失吗？", "ask_disappear")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_disappear",
                "何春秋：\"消失？我本就是方源的分身。即使消失，也只是回归本体。这不算恐惧，只是……遗憾。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_cause_effect",
                "何春秋：\"因果是时间的骨架。种什么因，得什么果。但宙道的高深处——可以斩断因果，改写结局。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_cicada",
                "何春秋：\"春秋蝉……六转仙蛊，宙道至宝。它能让时光倒流，让一切重来。方源凭借此蛊，从青茅山重生，开始了他的五百年传奇。\"")
                .AddOption("春秋蝉的力量有限制吗？", "ask_cicada_limit")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_cicada_limit",
                "何春秋：\"限制？当然有。每一次使用春秋蝉，都需要消耗大量的宙道真元。而且——逆转的时间越长，代价越大。这不是无本之木。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_fangyuan",
                "何春秋：\"方源……他就是我，我就是他。但我们的视角不同。我专注于宙道，而他——追求的是永生本身。\"")
                .AddOption("你们会冲突吗？", "ask_conflict")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_conflict",
                "何春秋：\"冲突？不会。因为我们的目标一致——永生。分身之间的分歧只是手段的不同，目的是相同的。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "何春秋：\"交易？时间就是金钱，而我是时间的主人。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "何春秋：\"去吧。时间不会等你，也不会等我。\"")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            return b.Build();
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
                    {
                        mgr.StartDialogue(NPC, Main.LocalPlayer);
                    }

                    var currentText = mgr.GetCurrentNPCText(Main.LocalPlayer);
                    var options = mgr.GetCurrentOptions(Main.LocalPlayer);

                    if (options != null && options.Count > 0)
                    {
                        DialogueTreeUI.Instance.Open(
                            NPC.GivenName,
                            NPCHeadLoader.GetHeadSlot(HeadTexture),
                            currentText ?? "",
                            options);
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

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return 0f;
        }

        public override List<string> SetNPCNameList()
        {
            return new List<string> { "何春秋" };
        }
    }
}
