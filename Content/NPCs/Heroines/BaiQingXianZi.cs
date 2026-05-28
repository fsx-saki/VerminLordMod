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
    public class BaiQingXianZi : GuMasterBase
    {
        public override FactionID GetFaction() => FactionID.LingYuanZhai;
        public override GuRank GetRank() => GuRank.Zhuan7_Gao;
        public override GuPersonality GetPersonality() => GuPersonality.Devoted;

        public override string GuMasterDisplayName => "白晴仙子";
        public override int GuMasterDamage => 260;
        public override int GuMasterLife => 42000;
        public override int GuMasterDefense => 140;

        private int _attackTimer;
        private int _loveBuffTimer;
        private bool _treeInitialized;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 600;
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
            NPC.value = Item.buyPrice(0, 4, 0, 0);
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

            _loveBuffTimer++;
            if (_loveBuffTimer % 150 == 0)
            {
                ApplyLoveBuffs();
            }
        }

        private void ApplyLoveBuffs()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var other = Main.npc[i];
                if (other.active && other.ModNPC is GuMasterBase ally)
                {
                    if (ally.GetFaction() == GetFaction() && Vector2.Distance(NPC.Center, other.Center) < 400f)
                    {
                        other.life = Math.Min(other.life + other.lifeMax / 40, other.lifeMax);
                        other.damage = (int)(other.damage * 1.05f);
                        CombatText.NewText(other.getRect(), Color.Pink, "♥", false);
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(NPC.Center, 10, 10, DustID.PinkTorch,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 0f),
                    100, Color.Pink, 1.2f);
            }
        }

        public override void ExecuteCombatAI(NPC npc)
        {
            var target = Main.player[npc.target];
            float dist = Vector2.Distance(npc.Center, target.Center);

            npc.spriteDirection = target.Center.X > npc.Center.X ? 1 : -1;

            if (dist < 120f)
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

            if (_attackTimer % 55 == 0 && dist < 450f)
            {
                Vector2 dir = Vector2.Normalize(target.Center - npc.Center);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 7f,
                    ProjectileID.EmeraldBolt, npc.damage / 3, 3f, Main.myPlayer);
            }

            if (_attackTimer % 90 == 0 && dist < 350f)
            {
                CharmAttack(npc, target);
            }

            if (_attackTimer % 130 == 0 && dist < 400f)
            {
                LoveBondAttack(npc, target);
            }

            if (Main.rand.NextBool(120))
            {
                string[] lines = { "九歌……我等你回来。", "爱是最强大的力量。", "为了他，我不会输！" };
                CombatText.NewText(npc.getRect(), Color.Pink, lines[Main.rand.Next(lines.Length)], true);
            }
        }

        private void CharmAttack(NPC npc, Player target)
        {
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.ToRadians(Main.rand.Next(-40, 40));
                Vector2 dir = Vector2.Normalize(target.Center - npc.Center).RotatedBy(angle);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 6f,
                    ProjectileID.EmeraldBolt, npc.damage / 4, 2f, Main.myPlayer);
            }

            if (Main.rand.NextBool(3))
            {
                target.AddBuff(BuffID.Lovestruck, 120);
            }

            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(npc.Center, 10, 10, DustID.PinkTorch,
                    Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f),
                    100, Color.Pink, 1.8f);
            }
        }

        private void LoveBondAttack(NPC npc, Player target)
        {
            Vector2 dir = Vector2.Normalize(target.Center - npc.Center);
            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 5f,
                ProjectileID.EmeraldBolt, npc.damage / 2, 5f, Main.myPlayer);

            for (int i = 0; i < 8; i++)
            {
                float angle = i * MathHelper.TwoPi / 8;
                Vector2 circleDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Dust.NewDust(target.Center + circleDir * 50f, 5, 5, DustID.PinkTorch,
                    circleDir.X * 2f, circleDir.Y * 2f, 100, Color.HotPink, 2f);
            }

            target.AddBuff(BuffID.Slow, 90);
            CombatText.NewText(npc.getRect(), Color.HotPink, "情丝缠绕！", true);
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "白晴仙子眼中含泪：\"九歌……我等你回来。你却来伤害我？\"",
                GuAttitude.Wary => "白晴仙子警惕地看着你：\"你是谁？如果你是灵缘斋的敌人，我不会手软。\"",
                GuAttitude.Friendly => NumberOfTimesTalkedTo <= 1
                    ? "白晴仙子温柔地说：\"九歌……我等你回来。你呢？在等谁？\""
                    : "白晴仙子微笑道：\"又来了。灵缘斋的茶，永远为你准备着。\"",
                GuAttitude.Respectful => "白晴仙子恭敬地说：\"大人仁义。灵缘斋愿与您交好。\"",
                GuAttitude.Contemptuous => "白晴仙子摇头道：\"不懂情之人，可悲。\"",
                GuAttitude.Fearful => "白晴仙子后退一步：\"九歌……如果你在就好了……\"",
                _ => "白晴仙子看了你一眼，眼中似乎有远方的思念。"
            };
        }

        private void InitializeDialogueTree()
        {
            var b = new DialogueTreeBuilder("BaiQingXianZi", "greeting");

            b.StartNode("greeting", "白晴仙子温柔地说：\"九歌……我等你回来。你呢？在等谁？\"")
                .AddOption("你是谁？", "who_are_you", DialogueOptionType.Informative)
                .AddOption("灵缘斋是什么？", "about_lingyuan", DialogueOptionType.Informative)
                .AddOption("九歌是谁？", "about_fengjiuge", DialogueOptionType.Social)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("who_are_you",
                "白晴仙子温柔地说：\"我是白晴仙子，灵缘斋七转蛊师。擅长情道和缘道之术。\n\n不过……我的修行，都是为了一个人。\"")
                .AddOption("那个人是谁？", "about_fengjiuge")
                .AddOption("灵缘斋？", "about_lingyuan")
                .AddOption("我了解了", "greeting");

            b.StartNode("about_lingyuan",
                "白晴仙子解释道：\"灵缘斋，是以情缘为核心的势力。我们相信，世间万物皆有缘法，情缘是最强大的力量。\n\n灵缘斋的蛊师，擅长牵线搭桥、缔结良缘，也擅长以情制敌。\"")
                .AddOption("以情制敌？", "love_combat")
                .AddOption("九歌是谁？", "about_fengjiuge")
                .AddOption("我了解了", "greeting");

            b.StartNode("love_combat",
                "白晴仙子认真地说：\"情道之术，能让人心神恍惚、行动迟缓。情丝缠绕，比任何锁链都牢固。\n\n不过，我更喜欢用情道来帮助他人——治愈伤痛、缔结良缘。战斗……只是不得已的手段。\"")
                .AddOption("你的情道很温柔", "gentle_power")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("gentle_power",
                "白晴仙子微笑道：\"温柔也是一种力量。九歌教会了我这一点。\n\n真正的强大，不是毁灭一切，而是在拥有力量之后，依然选择温柔。\"")
                .AddOption("九歌是谁？", "about_fengjiuge")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("about_fengjiuge",
                "白晴仙子的眼中闪过一丝温柔和哀伤：\"九歌……凤九歌，他是我的道侣。灵缘斋最出色的蛊师之一。\n\n他为了天庭的任务离开了……我一直在等他回来。九歌……我等你回来。\"")
                .AddOption("他去了哪里？", "where_is_he")
                .AddOption("你很痴情", "devotion")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("where_is_he",
                "白晴仙子叹息道：\"天庭的任务……他不能拒绝。九歌是天庭的人，也是灵缘斋的人。\n\n他说过会回来的。我相信他。无论等多久，我都会等。\"")
                .AddOption("你很痴情", "devotion")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("devotion",
                "白晴仙子微笑着，眼中含着泪光：\"痴情？也许吧。在灵缘斋，情是最重要的修行。\n\n我的情道，因他而起，也因他而强。这份等待，就是我最强大的力量。\"")
                .AddOption("我理解了", "understand")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("understand",
                "白晴仙子擦了擦眼角：\"谢谢你的理解。这个世界上，能理解等待之苦的人不多。\n\n九歌……我等你回来。\"")
                .AddOption("继续聊", "greeting")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("bye", "白晴仙子温柔地说：\"愿你也能找到值得等待的人。九歌……我等你回来。\"")
                .EndsDialogue();

            DialogueTreeManager.Instance.RegisterTree<BaiQingXianZi>(b.Build());
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qiRealm = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel <= 0) return 0f;
            return 0.003f;
        }
    }
}
