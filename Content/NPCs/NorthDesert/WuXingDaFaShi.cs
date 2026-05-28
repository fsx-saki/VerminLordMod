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

namespace VerminLordMod.Content.NPCs.NorthDesert
{
    [AutoloadHead]
    public class WuXingDaFaShi : GuMasterBase
    {
        private int _attackTimer;
        private int _elementCycle;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.ChangShengTian;
        public override GuRank GetRank() => GuRank.Zhuan8_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Wise;

        public override string GuMasterDisplayName => "五行大法师";
        public override int GuMasterDamage => 420;
        public override int GuMasterLife => 65000;
        public override int GuMasterDefense => 210;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 1500;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 20;
            NPCID.Sets.AttackAverageChance[Type] = 10;
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
            NPC.knockBackResist = 0.05f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(3, 0, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _elementCycle = 0;
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

            NPC.velocity.X = dir * 2f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -6f;

            _attackTimer++;
            if (_attackTimer >= 30)
            {
                _attackTimer = 0;
                _elementCycle = (_elementCycle + 1) % 5;
                ExecuteAttack(target);
            }
        }

        private void ExecuteAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 4;

            switch (_elementCycle)
            {
                case 0:
                    CombatText.NewText(NPC.getRect(), Color.OrangeRed, "火——！", true);
                    for (int i = -3; i <= 3; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.1f;
                        Vector2 vel = angle.ToRotationVector2() * 10f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.DD2BetsyFireball, damage, 2f, Main.myPlayer);
                    }
                    target.AddBuff(BuffID.OnFire, 180);
                    break;
                case 1:
                    CombatText.NewText(NPC.getRect(), Color.Blue, "水——！", true);
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi / 12f * i;
                        Vector2 vel = angle.ToRotationVector2() * 6f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.WaterBolt, damage, 1f, Main.myPlayer);
                    }
                    target.AddBuff(BuffID.Wet, 300);
                    break;
                case 2:
                    CombatText.NewText(NPC.getRect(), Color.Brown, "土——！", true);
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 spawnPos = target.Center + new Vector2(Main.rand.Next(-300, 300), -400);
                        Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), 10f);
                        Projectile.NewProjectile(source, spawnPos, vel, ProjectileID.BoulderStaffOfEarth, damage * 2, 3f, Main.myPlayer);
                    }
                    break;
                case 3:
                    CombatText.NewText(NPC.getRect(), Color.Silver, "金——！", true);
                    for (int i = -4; i <= 4; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.06f;
                        Vector2 vel = angle.ToRotationVector2() * 14f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.IchorBullet, damage, 1f, Main.myPlayer);
                    }
                    target.AddBuff(BuffID.Ichor, 180);
                    break;
                case 4:
                    CombatText.NewText(NPC.getRect(), Color.Green, "木——！", true);
                    for (int i = 0; i < 16; i++)
                    {
                        float angle = MathHelper.TwoPi / 16f * i;
                        Vector2 vel = angle.ToRotationVector2() * 5f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.PoisonFang, damage, 1f, Main.myPlayer);
                    }
                    target.AddBuff(BuffID.Poisoned, 240);
                    int healAmount = NPC.lifeMax / 20;
                    NPC.life = Math.Min(NPC.life + healAmount, NPC.lifeMax);
                    NPC.HealEffect(healAmount, true);
                    break;
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => NumberOfTimesTalkedTo switch
                {
                    1 => "五行相生相克，天地之道尽在其中。",
                    2 => "你竟敢挑战五行之道？不自量力。",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "你的气息……让我想起了一些旧事。不过，五行之道讲究平衡，我不会轻易出手。",
                GuAttitude.Friendly => "有缘人，五行大法师愿与你论道五行。",
                GuAttitude.Respectful => "你的修为……让我想起了年轻时的自己。",
                GuAttitude.Contemptuous => "不懂五行之道的人，不过是井底之蛙。",
                GuAttitude.Fearful => "你……竟能打破五行的平衡……",
                _ => "五行相生相克，天地至理。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "五行轮转，你逃不出我的手掌！",
                "火克金，金克木，木克土，土克水，水克火——你的弱点，我一清二楚！",
                "五行相生，力量无穷！",
                "长生天的核心战力，岂是等闲？",
                "五行之道，包罗万象，你不过是其中一粒尘埃！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("WuXingDaFaShi", "greeting");

            b.StartNode("greeting", "五行大法师端坐如山，周身隐隐有五色光芒流转——火红、水蓝、土黄、金白、木绿。")
                .AddOption("谈论五行道", "wuxing_dao", DialogueOptionType.Teach)
                .AddOption("五行相生相克", "wuxing_cycle", DialogueOptionType.Teach)
                .AddOption("关于长生天", "changshengtian", DialogueOptionType.Informative)
                .AddOption("五行与修炼", "wuxing_cultivation", DialogueOptionType.Teach)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("wuxing_dao", "五行大法师：\"五行道？金、木、水、火、土——天地万物，皆由五行构成。五行道修士可以操控五种元素，相生相克，变化无穷。\"")
                .AddOption("五行的本质？", "wuxing_nature")
                .AddOption("五行道的极致？", "wuxing_limit")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("wuxing_nature", "五行大法师：\"五行的本质？是天地的基本法则。金主杀伐，木主生长，水主流动，火主变化，土主承载。五者相生相克，构成了世界的平衡。\"")
                .AddOption("回到五行道", "wuxing_dao");

            b.StartNode("wuxing_limit", "五行大法师：\"五行道的极致？五行合一。当五种元素完美融合，便能产生超越任何单一元素的力量。这就是我追求的境界——五行归一。\"")
                .AddOption("回到五行道", "wuxing_dao");

            b.StartNode("wuxing_cycle", "五行大法师：\"相生：木生火，火生土，土生金，金生水，水生木。相克：木克土，土克水，水克火，火克金，金克木。这就是五行相生相克的基本法则。\"")
                .AddOption("如何利用相生？", "use_generate")
                .AddOption("如何利用相克？", "use_overcome")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("use_generate", "五行大法师：\"利用相生？比如木生火——先施展木道攻击，再顺势转为火道攻击，威力倍增。这就是相生的妙用——借力打力，顺势而为。\"")
                .AddOption("回到五行相生相克", "wuxing_cycle");

            b.StartNode("use_overcome", "五行大法师：\"利用相克？比如水克火——当敌人施展火道攻击时，我以水道克制，不仅化解攻击，还能反击。这就是相克的妙用——以敌之道，还施彼身。\"")
                .AddOption("回到五行相生相克", "wuxing_cycle");

            b.StartNode("changshengtian", "五行大法师：\"长生天？我是长生天的核心战力之一。冰塞川是领袖，但论战斗力，我五行大法师不输任何人。\"")
                .AddOption("你与冰塞川的关系？", "with_bing")
                .AddOption("长生天的未来？", "cst_future")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("with_bing", "五行大法师：\"冰塞川……他是个好领袖，虽然冷酷，但决策英明。我尊重他的领导，也会全力支持他。\"")
                .AddOption("回到长生天", "changshengtian");

            b.StartNode("cst_future", "五行大法师：\"长生天的未来？只要五行不灭，长生天就不会倒。五行之道讲究平衡——只要我们保持内部的平衡，外部的威胁不足为惧。\"")
                .AddOption("回到长生天", "changshengtian");

            b.StartNode("wuxing_cultivation", "五行大法师：\"五行与修炼？五行道是最难修炼的道之一，因为需要同时精通五种元素。但一旦精通，便是五倍的力量。这就是五行道的代价与回报。\"")
                .AddOption("修炼五行道的窍门？", "wuxing_tips")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("wuxing_tips", "五行大法师：\"窍门？先精通一种元素，再逐步扩展到其他四种。切忌贪多嚼不烂——五行道最忌讳的就是样样通、样样松。\"")
                .AddOption("回到五行与修炼", "wuxing_cultivation");

            b.StartNode("trade", "五行大法师：\"交易？可以。五行道的材料，我这里最为齐全。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "五行大法师微微颔首：\"去吧。记住，五行相生相克，天地至理。\"")
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

        public override List<string> SetNPCNameList() => new List<string> { "五行大法师" };
    }
}
