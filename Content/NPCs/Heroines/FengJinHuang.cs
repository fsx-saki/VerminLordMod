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
    public class FengJinHuang : GuMasterBase
    {
        public override FactionID GetFaction() => FactionID.Heaven;
        public override GuRank GetRank() => GuRank.Zhuan6_Gao;
        public override GuPersonality GetPersonality() => GuPersonality.Ambitious;

        public override string GuMasterDisplayName => "凤金煌";
        public override int GuMasterDamage => 200;
        public override int GuMasterLife => 35000;
        public override int GuMasterDefense => 100;

        private int _attackTimer;
        private int _dreamZoneTimer;
        private bool _dreamZoneActive;
        private bool _treeInitialized;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 700;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 35;
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
            NPC.knockBackResist = 0.3f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(0, 3, 0, 0);
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

            if (_dreamZoneActive)
            {
                _dreamZoneTimer++;
                var player = Main.LocalPlayer;
                if (Vector2.Distance(NPC.Center, player.Center) < 350f)
                {
                    if (!player.HasBuff(BuffID.Slow) && !player.HasBuff(BuffID.Suffocation))
                    {
                        player.AddBuff(BuffID.Slow, 120);
                        if (Main.rand.NextBool(60))
                        {
                            player.AddBuff(BuffID.Suffocation, 60);
                        }
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    Dust.NewDust(NPC.Center + new Vector2(Main.rand.Next(-350, 350), Main.rand.Next(-200, 200)),
                        10, 10, DustID.PurpleTorch, 0, -1f, 100, Color.MediumPurple, 1.2f);
                }
                if (_dreamZoneTimer > 300)
                {
                    _dreamZoneActive = false;
                    _dreamZoneTimer = 0;
                }
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
                npc.velocity.X = fleeDir * 1.8f;
            }
            else if (dist > 350f)
            {
                float dir = target.Center.X > npc.Center.X ? 1 : -1;
                npc.velocity.X = dir * 1.5f;
            }
            else
            {
                npc.velocity.X *= 0.9f;
            }

            if (npc.collideX && npc.velocity.Y == 0)
                npc.velocity.Y = -6f;

            _attackTimer++;

            if (_attackTimer % 60 == 0 && dist < 450f)
            {
                Vector2 dir = Vector2.Normalize(target.Center - npc.Center);
                float speed = 7f;
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * speed,
                    ProjectileID.DD2OgreSpit, npc.damage / 3, 3f, Main.myPlayer);
            }

            if (_attackTimer % 40 == 0 && dist < 300f)
            {
                DreamBiteAttack(npc, target);
            }

            if (!_dreamZoneActive && _attackTimer % 180 == 0 && dist < 400f)
            {
                _dreamZoneActive = true;
                _dreamZoneTimer = 0;
                CombatText.NewText(npc.getRect(), Color.MediumPurple, "大梦领域！", true);
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(npc.Center, 10, 10, DustID.PurpleTorch,
                        Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f),
                        100, Color.MediumPurple, 2f);
                }
            }

            if (Main.rand.NextBool(120))
            {
                string[] lines = { "我终将成为大梦仙尊！", "在梦中，一切都是我的！", "大梦一场，万法皆空。" };
                CombatText.NewText(npc.getRect(), Color.MediumPurple, lines[Main.rand.Next(lines.Length)], true);
            }
        }

        private void DreamBiteAttack(NPC npc, Player target)
        {
            Vector2 dir = Vector2.Normalize(target.Center - npc.Center);
            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 5f,
                ProjectileID.DD2BetsyFireball, npc.damage / 4, 2f, Main.myPlayer);
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "凤金煌高傲地说：\"你竟敢与天庭为敌？我终将成为大梦仙尊！\"",
                GuAttitude.Wary => "凤金煌审视着你：\"你是什么人？不要妨碍我修炼梦道。\"",
                GuAttitude.Friendly => NumberOfTimesTalkedTo <= 1
                    ? "凤金煌自信地说：\"我终将成为大梦仙尊！你，愿意见证吗？\""
                    : "凤金煌微笑道：\"又来了？看来你也对我的梦道感兴趣。\"",
                GuAttitude.Respectful => "凤金煌点头道：\"你的实力值得尊重。天庭需要这样的强者。\"",
                GuAttitude.Contemptuous => "凤金煌不屑道：\"你？也配与我说话？\"",
                GuAttitude.Fearful => "凤金煌咬牙道：\"大梦种子不会输给你！\"",
                _ => "凤金煌看了你一眼，眼中满是野心。"
            };
        }

        private void InitializeDialogueTree()
        {
            var b = new DialogueTreeBuilder("FengJinHuang", "greeting");

            b.StartNode("greeting", "凤金煌自信地说：\"我终将成为大梦仙尊！你，愿意见证吗？\"")
                .AddOption("你是谁？", "who_are_you", DialogueOptionType.Informative)
                .AddOption("大梦仙尊？", "about_dream_venerable", DialogueOptionType.Informative)
                .AddOption("关于天庭", "about_heaven", DialogueOptionType.Informative)
                .AddOption("大梦种子是什么？", "dream_seed", DialogueOptionType.Special)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("who_are_you",
                "凤金煌骄傲地说：\"我是凤金煌，天庭六转蛊师，梦道修行的天才！\n\n更重要的是——我是大梦种子的拥有者，未来的大梦仙尊！\"")
                .AddOption("大梦种子？", "dream_seed")
                .AddOption("六转就敢称仙尊？", "challenge_her", DialogueOptionType.Risky)
                .AddOption("我了解了", "greeting");

            b.StartNode("challenge_her",
                "凤金煌冷笑：\"六转？这只是开始！大梦种子赋予了我超越境界的潜力。\n\n总有一天，我会成为九转仙尊，让整个蛊师世界都在我的梦中！\"")
                .AddOption("有志气", "respect_ambition")
                .AddOption("太狂妄了", "greeting");

            b.StartNode("respect_ambition",
                "凤金煌微微一笑：\"你倒是有眼光。在这个弱肉强食的世界里，没有野心的人注定平庸。\n\n记住我的名字——凤金煌，未来的大梦仙尊。\"")
                .AddOption("继续聊", "greeting")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("about_dream_venerable",
                "凤金煌眼中闪烁着光芒：\"大梦仙尊，是传说中梦道的巅峰存在。据说大梦仙尊能将整个世界化为梦境，让一切生灵都在梦中沉浮。\n\n而我，就是下一个大梦仙尊！\"")
                .AddOption("大梦种子是什么？", "dream_seed")
                .AddOption("天庭支持你吗？", "about_heaven")
                .AddOption("我了解了", "greeting");

            b.StartNode("dream_seed",
                "凤金煌神秘地说：\"大梦种子，是天庭秘传的至宝。拥有它的人，天生亲和梦道，修炼速度是常人的数倍。\n\n但大梦种子也有代价——它会让你的梦境与现实交织，分不清哪里是梦，哪里是现实。\"")
                .AddOption("你不怕迷失吗？", "fear_lost")
                .AddOption("我了解了", "greeting");

            b.StartNode("fear_lost",
                "凤金煌坚定地说：\"迷失？不。梦与现实的边界，本就是我想要打破的。\n\n当大梦仙尊降临时，梦就是现实，现实就是梦。那才是真正的自由！\"")
                .AddOption("你的野心令人敬畏", "respect_ambition")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("about_heaven",
                "凤金煌认真地说：\"天庭是蛊师世界最强大的势力。我能在天庭修行，是天赐的机缘。\n\n但天庭也有它的局限——它太注重秩序，太害怕变革。而我，注定要改变一切。\"")
                .AddOption("你要颠覆天庭？", "overthrow_heaven", DialogueOptionType.Risky)
                .AddOption("我了解了", "greeting");

            b.StartNode("overthrow_heaven",
                "凤金煌摇头：\"不是颠覆，是超越。天庭给了我成长的土壤，但我的目标比天庭更远大。\n\n大梦仙尊不属于任何势力，只属于她自己。\"")
                .AddOption("我理解了", "respect_ambition")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("bye", "凤金煌转身离去：\"记住，我终将成为大梦仙尊！\"")
                .EndsDialogue();

            DialogueTreeManager.Instance.RegisterTree<FengJinHuang>(b.Build());
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qiRealm = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel <= 0) return 0f;
            return 0.004f;
        }
    }
}
