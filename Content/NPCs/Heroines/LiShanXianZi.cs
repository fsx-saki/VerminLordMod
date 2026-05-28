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
    public class LiShanXianZi : GuMasterBase
    {
        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.Zhuan7_Gao;
        public override GuPersonality GetPersonality() => GuPersonality.Gentle;

        public override string GuMasterDisplayName => "黎山仙子";
        public override int GuMasterDamage => 280;
        public override int GuMasterLife => 48000;
        public override int GuMasterDefense => 160;

        private int _attackTimer;
        private int _healTimer;
        private bool _treeInitialized;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 600;
            NPCID.Sets.AttackType[Type] = 0;
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
            NPC.knockBackResist = 0.35f;
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

            _healTimer++;
            if (_healTimer % 180 == 0)
            {
                HealingAura();
            }
        }

        private void HealingAura()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var other = Main.npc[i];
                if (other.active && other.ModNPC is GuMasterBase ally)
                {
                    if (ally.GetFaction() == GetFaction() && Vector2.Distance(NPC.Center, other.Center) < 400f)
                    {
                        other.life = Math.Min(other.life + other.lifeMax / 50, other.lifeMax);
                        CombatText.NewText(other.getRect(), Color.Green, "+", false);
                    }
                }
            }

            NPC.life = Math.Min(NPC.life + NPC.lifeMax / 80, NPC.lifeMax);

            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(NPC.Center, 10, 10, DustID.Poisoned,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 0f),
                    100, Color.Green, 1.2f);
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

            if (_attackTimer % 50 == 0 && dist < 450f)
            {
                VineAttack(npc, target);
            }

            if (_attackTimer % 80 == 0 && dist < 350f)
            {
                NatureTrapAttack(npc, target);
            }

            if (_attackTimer % 120 == 0 && dist < 400f)
            {
                VineEntangleAttack(npc, target);
            }

            if (Main.rand.NextBool(130))
            {
                string[] lines = { "山有木兮木有枝。", "大自然的力量，你无法抗拒。", "请不要再逼我了……" };
                CombatText.NewText(npc.getRect(), Color.Green, lines[Main.rand.Next(lines.Length)], true);
            }
        }

        private void VineAttack(NPC npc, Player target)
        {
            Vector2 dir = Vector2.Normalize(target.Center - npc.Center);
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.ToRadians(-15 + i * 15);
                Vector2 rotatedDir = dir.RotatedBy(angle);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, rotatedDir * 8f,
                    ProjectileID.PoisonDart, npc.damage / 4, 3f, Main.myPlayer);
            }
        }

        private void NatureTrapAttack(NPC npc, Player target)
        {
            Vector2 trapPos = target.Center + new Vector2(Main.rand.Next(-80, 80), 0);
            Projectile.NewProjectile(npc.GetSource_FromAI(), trapPos, Vector2.Zero,
                ProjectileID.SporeCloud, npc.damage / 5, 2f, Main.myPlayer, 0, 120);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(trapPos, 10, 10, DustID.Poisoned,
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
                    100, Color.Green, 1.5f);
            }
        }

        private void VineEntangleAttack(NPC npc, Player target)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = i * MathHelper.TwoPi / 6;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 80f;
                Vector2 spawnPos = target.Center + offset;
                Vector2 dir = Vector2.Normalize(target.Center - spawnPos);
                Projectile.NewProjectile(npc.GetSource_FromAI(), spawnPos, dir * 4f,
                    ProjectileID.PoisonDart, npc.damage / 5, 2f, Main.myPlayer);
            }
            target.AddBuff(BuffID.Slow, 120);
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "黎山仙子叹了口气：\"请不要再逼我了……山有木兮木有枝，我不想伤害你。\"",
                GuAttitude.Wary => "黎山仙子温婉但警惕地看着你：\"你是谁？如果你没有恶意，我不会为难你。\"",
                GuAttitude.Friendly => NumberOfTimesTalkedTo <= 1
                    ? "黎山仙子温柔地说：\"山有木兮木有枝。我是黎山仙子，北荒的木道蛊师。\""
                    : "黎山仙子微笑道：\"又见面了。山中的花草，都还记得你。\"",
                GuAttitude.Respectful => "黎山仙子恭敬地说：\"大人仁义，黎山佩服。若需要疗伤，我随时可以帮忙。\"",
                GuAttitude.Contemptuous => "黎山仙子摇头道：\"暴力不是解决问题的唯一方式。\"",
                GuAttitude.Fearful => "黎山仙子后退一步：\"我……不想与人为敌。\"",
                _ => "黎山仙子看了你一眼，微微点头。"
            };
        }

        private void InitializeDialogueTree()
        {
            var b = new DialogueTreeBuilder("LiShanXianZi", "greeting");

            b.StartNode("greeting", "黎山仙子温柔地说：\"山有木兮木有枝。我是黎山仙子，北荒的木道蛊师。\"")
                .AddOption("你是谁？", "who_are_you", DialogueOptionType.Informative)
                .AddOption("关于木道", "about_wood", DialogueOptionType.Informative)
                .AddOption("你和焚天魔女？", "about_fentian", DialogueOptionType.Social)
                .AddOption("你能治疗吗？", "about_healing", DialogueOptionType.Social)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("who_are_you",
                "黎山仙子温婉地说：\"我是黎山仙子，北荒七转木道蛊师。擅长治疗和自然之术。\n\n在北荒这个以火和冰为主的地方，木道蛊师很少见。但正因为稀少，才更显珍贵。\"")
                .AddOption("关于木道", "about_wood")
                .AddOption("北荒的生活？", "about_north")
                .AddOption("我了解了", "greeting");

            b.StartNode("about_wood",
                "黎山仙子伸出手，一株小草从掌心生长：\"木道，是生机的道。木道蛊师能催生万物，治愈伤痛，也能让藤蔓化为利器。\n\n木道看似温柔，但大自然的力量，远比想象中强大。\"")
                .AddOption("你能治疗吗？", "about_healing")
                .AddOption("木道也能战斗？", "wood_combat")
                .AddOption("我了解了", "greeting");

            b.StartNode("wood_combat",
                "黎山仙子认真地说：\"当然。藤蔓可以束缚敌人，毒草可以致命，荆棘可以刺穿铠甲。\n\n木道的战斗方式，是让敌人在不知不觉中陷入绝境。就像森林中的藤蔓，无声无息地将猎物缠绕。\"")
                .AddOption("你和焚天魔女？", "about_fentian")
                .AddOption("我了解了", "greeting");

            b.StartNode("about_healing",
                "黎山仙子微笑道：\"当然。木道蛊师最擅长的就是治疗。我的治疗术可以加速伤口愈合，恢复真元。\n\n如果你受伤了，靠近我就好。我的治愈光环会自动生效。\"")
                .AddOption("谢谢", "thank_you")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("thank_you",
                "黎山仙子温和地说：\"不客气。帮助他人，是木道蛊师的本分。\"")
                .AddOption("继续聊", "greeting")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("about_fentian",
                "黎山仙子叹了口气：\"焚天魔女……她是我最重要的朋友。虽然我们的性格截然不同——她如火，我如木——但我们之间的羁绊，比任何东西都坚固。\n\n在北荒，我们互相扶持，才能走到今天。\"")
                .AddOption("火和木不是相克吗？", "fire_and_wood")
                .AddOption("你和黑楼兰？", "about_heiloulan")
                .AddOption("我了解了", "greeting");

            b.StartNode("fire_and_wood",
                "黎山仙子微笑道：\"相克？不，是相辅相成。火焰燃烧后的灰烬，是植物最好的养料。\n\n她保护我免受强敌侵害，我为她疗伤恢复。这就是我们的关系。\"")
                .AddOption("很美好的友情", "thank_you")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("about_heiloulan",
                "黎山仙子表情变得复杂：\"黑楼兰……他是我和焚天魔女之间的那个人。\n\n有些事情，我不愿多提。山有木兮木有枝，心悦君兮君不知。\"")
                .AddOption("我理解", "thank_you")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("about_north",
                "黎山仙子回忆道：\"北荒是一个残酷而美丽的地方。那里有茫茫雪原，也有炽热的火山。\n\n在北荒生存，需要刚烈，也需要温柔。刚烈让我们战斗，温柔让我们不迷失自我。\"")
                .AddOption("继续聊", "greeting")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("bye", "黎山仙子温柔地说：\"愿山间的清风，带给你安宁。\"")
                .EndsDialogue();

            DialogueTreeManager.Instance.RegisterTree<LiShanXianZi>(b.Build());
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qiRealm = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel <= 0) return 0f;
            return 0.003f;
        }
    }
}
