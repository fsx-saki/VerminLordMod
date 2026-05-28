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
    [AutoloadHead]
    public class MengQiuZhen : GuMasterBase
    {
        private int _attackTimer;
        private int _illusionTimer;
        private int _illusionCount;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/FangYuan/MengQiuZhen_Head";

        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.Zhuan6_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Cunning;

        public override string GuMasterDisplayName => "梦求真";
        public override int GuMasterDamage => 250;
        public override int GuMasterLife => 40000;
        public override int GuMasterDefense => 120;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 900;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 30;
            NPCID.Sets.AttackAverageChance[Type] = 20;
            NPCID.Sets.HatOffsetY[Type] = 4;
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
            NPC.knockBackResist = 0.2f;
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
            _illusionTimer = 0;
            _illusionCount = 0;
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
                ExecuteDreamCombatAI();
            }

            DreamAuraEffect();
        }

        private void DreamAuraEffect()
        {
            if (Main.rand.NextBool(5))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat() * 40f;
                Vector2 offset = angle.ToRotationVector2() * dist;
                Dust.NewDust(NPC.Center + offset, 4, 4, DustID.PinkFairy,
                    0f, -1f, 100, Color.Pink, 0.8f);
            }
        }

        private void ExecuteDreamCombatAI()
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

            if (dist > 400f)
                NPC.velocity.X = dir * 3f;
            else if (dist > 150f)
                NPC.velocity.X = dir * 1.5f;
            else
                NPC.velocity.X = dir * 0.8f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -7f;

            _attackTimer++;
            if (_attackTimer >= 50)
            {
                _attackTimer = 0;
                ExecuteDreamAttack(target);
            }

            _illusionTimer++;
            if (_illusionTimer >= 180 && _illusionCount < 3)
            {
                _illusionTimer = 0;
                _illusionCount++;
                SpawnDreamIllusion(target);
            }
        }

        private void ExecuteDreamAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 5;

            int pattern = Main.rand.Next(3);

            switch (pattern)
            {
                case 0:
                    target.AddBuff(BuffID.Confused, 180);
                    CombatText.NewText(NPC.getRect(), Color.Pink, "梦蝶蛊——虚实难辨！", true);

                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 5f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.WaterBolt, damage, 1f, Main.myPlayer);
                    }
                    break;

                case 1:
                    for (int i = -2; i <= 2; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.25f;
                        Vector2 velocity = angle.ToRotationVector2() * 8f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.CultistBossLightningOrb, damage, 0f, Main.myPlayer);
                    }
                    break;

                case 2:
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 spawnPos = target.Center + new Vector2(
                            Main.rand.Next(-200, 200),
                            Main.rand.Next(-300, -100));
                        Vector2 velocity = (target.Center - spawnPos).SafeNormalize(Vector2.UnitY) * 6f;
                        Projectile.NewProjectile(source, spawnPos, velocity,
                            ProjectileID.Fireball, damage, 2f, Main.myPlayer);
                    }
                    break;
            }
        }

        private void SpawnDreamIllusion(Player target)
        {
            CombatText.NewText(NPC.getRect(), Color.Pink, "梦蝶——幻影！", true);

            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.PinkFairy,
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-3f, 3f),
                    100, Color.Pink, 1.5f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                Vector2 offset = new Vector2(Main.rand.Next(-150, 150), Main.rand.Next(-100, 100));
                Vector2 spawnPos = NPC.Center + offset;

                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi / 4f * i;
                    Vector2 velocity = angle.ToRotationVector2() * 4f;
                    Projectile.NewProjectile(source, spawnPos, velocity,
                        ProjectileID.WaterBolt, NPC.damage / 6, 1f, Main.myPlayer);
                }
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "你分得清……梦境与现实吗？",
                GuAttitude.Wary => "你在看我？还是你在看自己的梦？",
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => "在梦中，一切皆有可能。你的尊重……我收到了。",
                GuAttitude.Contemptuous => "你的轻蔑……不过是一场梦。梦醒之后，什么都不会留下。",
                GuAttitude.Fearful => "恐惧？在梦里，恐惧也是一种力量。",
                _ => "梦……是另一种真实。"
            };
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "梦求真微微一笑：\"梦道，是蛊世界最年轻的道。而我，是第一个梦道蛊仙。\"",
                "梦求真：\"你以为这是现实？也许……这只是方源做的一个梦。\"",
                "梦求真：\"梦蝶蛊可以创造幻象，让敌人分不清真假。在战场上，这是最致命的能力。\"",
                "梦求真：\"我的粉色头发？这是梦道的印记。每一个梦道蛊师，都会被梦境染色。\"",
                "梦求真：\"在梦中，没有不可能的事。唯一的限制，是你的想象力。\"",
                "梦求真：\"方源创造了我来探索梦道。而我……已经超越了他的期望。\"",
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
            var b = new DialogueTreeBuilder("FangYuan_MengQiuZhen", "greeting");

            b.StartNode("greeting",
                "梦求真周身飘散着粉色光点，仿佛身处梦境之中。")
                .AddOption("询问梦道", "ask_dream_dao", DialogueOptionType.Informative)
                .AddOption("关于梦蝶蛊", "ask_dream_butterfly", DialogueOptionType.Informative)
                .AddOption("关于方源", "ask_fangyuan", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_dream_dao",
                "梦求真：\"梦道，操控梦境之道。在梦中，一切规则都可以被改写。现实中的不可能，在梦中都是可能。\"")
                .AddOption("梦能变成现实吗？", "ask_dream_reality")
                .AddOption("梦道的危险", "ask_dream_danger")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_dream_reality",
                "梦求真：\"高深的梦道蛊师，确实可以将梦境的一部分带入现实。但这需要极大的真元和精确的控制。稍有不慎，就会迷失在梦与现实的夹缝中。\"")
                .AddOption("你迷失过吗？", "ask_lost")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_lost",
                "梦求真沉默了片刻：\"……迷失过。在成为蛊仙之前，我曾在梦境中沉睡了三年。那三年里，我分不清哪个是梦，哪个是现实。直到方源唤醒了我。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_dream_danger",
                "梦求真：\"梦道最大的危险，是迷失自我。当你能创造任何梦境时，你也会开始怀疑——自己是否也在别人的梦中。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_dream_butterfly",
                "梦求真：\"梦蝶蛊，六转仙蛊，梦道核心蛊虫。它可以创造幻象，让敌人陷入梦境无法自拔。庄周梦蝶，蝶梦庄周——虚实之间，便是梦蝶蛊的领域。\"")
                .AddOption("庄周梦蝶是什么意思？", "ask_zhuangzi")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_zhuangzi",
                "梦求真：\"庄周梦蝶，是上古的一个寓言——庄周梦见自己变成了蝴蝶，醒来后不知道是庄周梦蝶，还是蝴蝶梦庄周。梦蝶蛊的名字，就来源于此。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_fangyuan",
                "梦求真：\"方源……他是我的创造者，也是我的本体。但梦求真不只是方源的影子——我有自己的思考，自己的情感。虽然……这些也许都是方源设计好的。\"")
                .AddOption("你是独立的个体吗？", "ask_independence")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_independence",
                "梦求真苦笑：\"独立？也许吧。在梦中，我确实是自由的。但梦醒之后……我依然是方源的分身。这种矛盾，就是梦道蛊师的宿命。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "梦求真：\"你想在梦中交易？有趣……梦中的货币是想象力。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "梦求真：\"愿你今晚……做一个好梦。\"")
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
            return new List<string> { "梦求真" };
        }
    }
}
