using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Content.NPCs.Heroines
{
    [AutoloadHead]
    public class ShangXinCi : GuMasterBase
    {
        public override FactionID GetFaction() => FactionID.Shang;
        public override GuRank GetRank() => GuRank.Zhuan5_Gao;
        public override GuPersonality GetPersonality() => GuPersonality.Benevolent;

        public override string GuMasterDisplayName => "商心慈";
        public override int GuMasterDamage => 120;
        public override int GuMasterLife => 25000;
        public override int GuMasterDefense => 80;

        private int _luckTimer;
        private int _buffTimer;
        private bool _treeInitialized;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 600;
            NPCID.Sets.AttackType[Type] = 0;
            NPCID.Sets.AttackTime[Type] = 40;
            NPCID.Sets.AttackAverageChance[Type] = 10;
            NPCID.Sets.HatOffsetY[Type] = 4;

            if (!NPCID.Sets.NPCBestiaryDrawOffset.ContainsKey(Type))
            {
                NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers
                {
                    Velocity = 1f,
                    Direction = 1
                });
            }
        }

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = GuMasterDamage;
            NPC.lifeMax = GuMasterLife;
            NPC.defense = GuMasterDefense;
            NPC.knockBackResist = 0.4f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(0, 2, 0, 0);
            NPC.townNPC = false;
            NPC.friendly = false;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
        }

        public override void AI()
        {
            if (!_treeInitialized)
            {
                InitializeDialogueTree();
                _treeInitialized = true;
            }

            base.AI();

            _luckTimer++;
            _buffTimer++;

            if (_buffTimer % 120 == 0)
            {
                ApplyLuckAura();
            }

            if (_luckTimer % 300 == 0 && Main.rand.NextBool(3))
            {
                TriggerLuckyEvent();
            }
        }

        private void ApplyLuckAura()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var other = Main.npc[i];
                if (other.active && other.ModNPC is GuMasterBase ally)
                {
                    if (ally.GetFaction() == GetFaction() && Vector2.Distance(NPC.Center, other.Center) < 400f)
                    {
                        other.defense += 5;
                    }
                }
            }

            if (CurrentAttitude == GuAttitude.Friendly || CurrentAttitude == GuAttitude.Respectful)
            {
                var player = Main.LocalPlayer;
                if (Vector2.Distance(NPC.Center, player.Center) < 400f)
                {
                    player.AddBuff(BuffID.Lucky, 180);
                }
            }
        }

        private void TriggerLuckyEvent()
        {
            int eventType = Main.rand.Next(3);
            switch (eventType)
            {
                case 0:
                    int coinAmount = Main.rand.Next(50, 200);
                    Item.NewItem(NPC.GetSource_FromAI(), NPC.Center, ItemID.SilverCoin, coinAmount);
                    Say("运气不错呢，掉落了一些元石。", Color.Gold);
                    break;
                case 1:
                    NPC.life = Math.Min(NPC.life + NPC.lifeMax / 20, NPC.lifeMax);
                    CombatText.NewText(NPC.getRect(), Color.Green, "运道庇佑！", true);
                    break;
                case 2:
                    for (int i = 0; i < 8; i++)
                    {
                        Dust.NewDust(NPC.Center, 10, 10, DustID.Enchanted_Gold,
                            Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
                            100, Color.Gold, 1.5f);
                    }
                    break;
            }
        }

        public override void ExecuteCombatAI(NPC npc)
        {
            var target = Main.player[npc.target];
            float dist = Vector2.Distance(npc.Center, target.Center);

            npc.spriteDirection = target.Center.X > npc.Center.X ? 1 : -1;

            if (dist < 150f)
            {
                float fleeDir = npc.Center.X > target.Center.X ? 1 : -1;
                npc.velocity.X = fleeDir * 1.5f;
            }
            else if (dist > 300f)
            {
                float dir = target.Center.X > npc.Center.X ? 1 : -1;
                npc.velocity.X = dir * 1.2f;
            }
            else
            {
                npc.velocity.X *= 0.9f;
            }

            if (npc.collideX && npc.velocity.Y == 0)
                npc.velocity.Y = -5f;

            if (Main.rand.NextBool(90) && dist < 400f)
            {
                Vector2 dir = Vector2.Normalize(target.Center - npc.Center);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 6f,
                    ProjectileID.HallowStar, npc.damage / 3, 3f, Main.myPlayer);
            }

            if (Main.rand.NextBool(180))
            {
                string[] lines = { "菩萨心肠，金刚手段！", "我不想伤害你……", "运道无常，你好自为之。" };
                CombatText.NewText(npc.getRect(), Color.Gold, lines[Main.rand.Next(lines.Length)], true);
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "商心慈叹了口气：\"我不想伤害你，但你逼我出手。菩萨心肠，金刚手段。\"",
                GuAttitude.Wary => "商心慈温和但警惕地看着你：\"你是谁？如果你没有恶意，我不会为难你。\"",
                GuAttitude.Friendly => NumberOfTimesTalkedTo <= 1
                    ? "商心慈微笑着说：\"菩萨心肠，金刚手段。这是我行事的准则。你呢？\""
                    : "商心慈温和地说：\"又见面了。最近运气如何？\"",
                GuAttitude.Respectful => "商心慈恭敬地说：\"大人仁义，心慈佩服。若有什么需要帮忙的，尽管开口。\"",
                GuAttitude.Contemptuous => "商心慈摇头道：\"人若没有善心，再强大也不过是一具空壳。\"",
                GuAttitude.Fearful => "商心慈后退一步：\"我……不想与你为敌。\"",
                _ => "商心慈看了你一眼，微微点头。"
            };
        }

        private void InitializeDialogueTree()
        {
            var b = new DialogueTreeBuilder("ShangXinCi", "greeting");

            b.StartNode("greeting", "商心慈微笑着说：\"菩萨心肠，金刚手段。这是我行事的准则。你呢？\"")
                .AddOption("你是谁？", "who_are_you", DialogueOptionType.Informative)
                .AddOption("什么是运道？", "about_luck", DialogueOptionType.Informative)
                .AddOption("关于白家和商家", "about_factions", DialogueOptionType.Informative)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("who_are_you",
                "商心慈温和地说：\"我叫商心慈，原本是白家的人，后来嫁入商家。运道蛊师，五转修为。\n\n虽然实力不算顶尖，但我相信——善良和运气，也是一种力量。\"")
                .AddOption("白家和商家？", "about_factions")
                .AddOption("你真的很善良吗？", "kind_or_naive")
                .AddOption("我了解了", "greeting");

            b.StartNode("kind_or_naive",
                "商心慈认真地说：\"善良不等于天真。菩萨心肠，金刚手段——我愿意帮助他人，但绝不会任人欺负。\n\n在这个弱肉强食的世界里，只有善良是不够的。\"")
                .AddOption("说得对", "agree")
                .AddOption("你遇到过危险吗？", "dangers")
                .AddOption("回到之前的话题", "greeting");

            b.StartNode("agree",
                "商心慈微笑道：\"你能理解，我很高兴。这个世界上，理解比力量更珍贵。\"")
                .AddOption("继续聊", "greeting")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("dangers",
                "商心慈回忆道：\"在青茅山的时候，我见过太多阴谋诡计。方源……他让我明白了一个道理——\n\n善良不是软弱，而是在看清世界真相后，依然选择帮助他人。\"")
                .AddOption("方源？", "about_fangyuan")
                .AddOption("我了解了", "greeting");

            b.StartNode("about_fangyuan",
                "商心慈表情复杂：\"方源……他是一个让人无法定义的人。他救过我，也利用过我。\n\n但我不恨他。因为正是他，让我明白了善良的真正含义。\"")
                .AddOption("你很坚强", "agree")
                .AddOption("回到之前的话题", "greeting");

            b.StartNode("about_luck",
                "商心慈解释道：\"运道，是天地间最玄妙的道之一。运气好的时候，一切都会顺遂；运气差的时候，喝凉水都会塞牙。\n\n我的运道蛊虫能感知气运的流动，在关键时刻趋吉避凶。\"")
                .AddOption("能帮我提升运气吗？", "boost_luck")
                .AddOption("回到之前的话题", "greeting");

            b.StartNode("boost_luck",
                "商心慈微笑道：\"当然可以。靠近我的人，运气都会变好一些。这是我运道蛊虫的被动效果。\n\n不过，真正的运气，还是要靠自己去争取。\"")
                .AddOption("谢谢", "agree")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("about_factions",
                "商心慈叹了口气：\"白家是我的娘家，商家是我嫁入的家族。两家之间有着千丝万缕的联系。\n\n但在我心中，家族不是最重要的。重要的是——你选择做一个什么样的人。\"")
                .AddOption("你很独立", "agree")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("bye", "商心慈温和地说：\"愿你前路顺遂，好运常伴。\"")
                .EndsDialogue();

            DialogueTreeManager.Instance.RegisterTree<ShangXinCi>(b.Build());
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qiRealm = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel <= 0) return 0f;
            return 0.005f;
        }
    }
}
