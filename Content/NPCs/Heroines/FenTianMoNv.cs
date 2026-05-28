using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.NPCs.GuMasters;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Content.NPCs.Heroines
{
    [AutoloadHead]
    public class FenTianMoNv : GuMasterBase
    {
        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.Zhuan7_Gao;
        public override GuPersonality GetPersonality() => GuPersonality.Fierce;

        public override string GuMasterDisplayName => "焚天魔女";
        public override int GuMasterDamage => 360;
        public override int GuMasterLife => 50000;
        public override int GuMasterDefense => 130;

        private int _attackTimer;
        private bool _hasUsedFenTian;
        private bool _treeInitialized;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 800;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 20;
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
            NPC.knockBackResist = 0.15f;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(0, 5, 0, 0);
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

            if (CurrentAIState == GuMasterAIState.Combat)
            {
                Lighting.AddLight(NPC.Center, 1f, 0.4f, 0.1f);
            }
        }

        public override void ExecuteCombatAI(NPC npc)
        {
            var target = Main.player[npc.target];
            float dist = Vector2.Distance(npc.Center, target.Center);

            npc.spriteDirection = target.Center.X > npc.Center.X ? 1 : -1;

            if (dist < 100f)
            {
                float fleeDir = npc.Center.X > target.Center.X ? 1 : -1;
                npc.velocity.X = fleeDir * 2.5f;
            }
            else if (dist > 300f)
            {
                float dir = target.Center.X > npc.Center.X ? 1 : -1;
                npc.velocity.X = dir * 2.2f;
            }
            else
            {
                npc.velocity.X *= 0.9f;
            }

            if (npc.collideX && npc.velocity.Y == 0)
                npc.velocity.Y = -7f;

            _attackTimer++;

            if (_attackTimer % 35 == 0 && dist < 500f)
            {
                Vector2 dir = Vector2.Normalize(target.Center - npc.Center);
                float speed = 9f;
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * speed,
                    ProjectileID.Fireball, npc.damage / 3, 4f, Main.myPlayer);
            }

            if (_attackTimer % 90 == 0 && dist < 400f)
            {
                FirestormAttack(npc, target);
            }

            if (_attackTimer % 150 == 0 && dist < 350f)
            {
                BurningZoneAttack(npc, target);
            }

            if (!_hasUsedFenTian && (float)npc.life / npc.lifeMax < 0.3f)
            {
                FenTianExplosion(npc);
                _hasUsedFenTian = true;
            }

            if (Main.rand.NextBool(100))
            {
                string[] lines = { "烈焰焚天，寸草不留！", "在烈火中化为灰烬吧！", "北荒的火焰，你承受不住！" };
                CombatText.NewText(npc.getRect(), Color.OrangeRed, lines[Main.rand.Next(lines.Length)], true);
            }
        }

        private void FirestormAttack(NPC npc, Player target)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.ToRadians(i * 45 + Main.rand.Next(-10, 10));
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 7f,
                    ProjectileID.MolotovFire, npc.damage / 4, 3f, Main.myPlayer);
            }
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(npc.Center, 10, 10, DustID.Torch,
                    Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f),
                    100, Color.OrangeRed, 2f);
            }
        }

        private void BurningZoneAttack(NPC npc, Player target)
        {
            Vector2 zoneCenter = target.Center;
            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = new Vector2(Main.rand.Next(-100, 100), Main.rand.Next(-50, 50));
                Projectile.NewProjectile(npc.GetSource_FromAI(), zoneCenter + offset, Vector2.Zero,
                    ProjectileID.MolotovFire2, npc.damage / 5, 2f, Main.myPlayer, 0, 180);
            }
        }

        private void FenTianExplosion(NPC npc)
        {
            CombatText.NewText(npc.getRect(), Color.Red, "焚天！", true);
            Say("烈焰焚天，寸草不留！", Color.OrangeRed);

            for (int i = 0; i < 24; i++)
            {
                float angle = i * MathHelper.TwoPi / 24;
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 12f,
                    ProjectileID.Fireball, npc.damage / 2, 6f, Main.myPlayer);
            }

            for (int i = 0; i < 60; i++)
            {
                Dust.NewDust(npc.Center, 20, 20, DustID.Torch,
                    Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f),
                    200, Color.Red, 3f);
            }

            npc.life -= npc.lifeMax / 10;
            if (npc.life <= 0) npc.life = 1;
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "焚天魔女怒喝：\"烈焰焚天，寸草不留！你选错了对手！\"",
                GuAttitude.Wary => "焚天魔女警惕地看着你：\"你是什么人？别靠近我，否则烧成灰！\"",
                GuAttitude.Friendly => NumberOfTimesTalkedTo <= 1
                    ? "焚天魔女豪迈地说：\"烈焰焚天，寸草不留！这就是我——北荒焚天魔女！\""
                    : "焚天魔女咧嘴一笑：\"又来了？看来你还没被我的火焰吓退。\"",
                GuAttitude.Respectful => "焚天魔女点头道：\"你有些本事。北荒之人，敬重强者。\"",
                GuAttitude.Contemptuous => "焚天魔女不屑道：\"弱者，连做我火焰燃料的资格都没有！\"",
                GuAttitude.Fearful => "焚天魔女咬牙道：\"就算死，也要拉你一起！\"",
                _ => "焚天魔女看了你一眼，周身火焰微微跳动。"
            };
        }

        private void InitializeDialogueTree()
        {
            var b = new DialogueTreeBuilder("FenTianMoNv", "greeting");

            b.StartNode("greeting", "焚天魔女豪迈地说：\"烈焰焚天，寸草不留！这就是我——北荒焚天魔女！\"")
                .AddOption("你是谁？", "who_are_you", DialogueOptionType.Informative)
                .AddOption("关于北荒", "about_north", DialogueOptionType.Informative)
                .AddOption("你的火道很强？", "about_fire", DialogueOptionType.Informative)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("who_are_you",
                "焚天魔女骄傲地说：\"我是焚天魔女，北荒七转火道蛊师！\n\n在北荒，没有人敢小看我的火焰。那些胆敢挑战我的人，都化成了灰烬！\"")
                .AddOption("北荒是什么样的？", "about_north")
                .AddOption("焚天是什么招式？", "about_fentian")
                .AddOption("我了解了", "greeting");

            b.StartNode("about_north",
                "焚天魔女回忆道：\"北荒，是一片冰与火交织的土地。那里有最凶猛的妖兽，最刚烈的蛊师。\n\n在北荒，弱者没有生存的资格。只有像烈火一样刚烈的人，才能活下来。\"")
                .AddOption("你和黑楼兰？", "about_heiloulan")
                .AddOption("我了解了", "greeting");

            b.StartNode("about_heiloulan",
                "焚天魔女表情复杂：\"黑楼兰……那个男人。我们之间的事，说来话长。\n\n他是北荒最强的蛊师之一，而我……曾经是他的盟友，也是他的对手。\"")
                .AddOption("你们之间发生了什么？", "heiloulan_detail")
                .AddOption("我了解了", "greeting");

            b.StartNode("heiloulan_detail",
                "焚天魔女沉默了片刻：\"在北荒的争斗中，我们曾经并肩作战，也曾经互相算计。\n\n黑楼兰是个复杂的人——他既有温柔的一面，也有残忍的一面。就像北荒的风雪，美丽而致命。\"")
                .AddOption("你还想着他？", "still_think")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("still_think",
                "焚天魔女哼了一声：\"想？我只是……记得。北荒的刚烈之人，不会沉溺于过去。\n\n但有些记忆，就像火焰一样，越烧越旺。\"")
                .AddOption("继续聊", "greeting")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("about_fire",
                "焚天魔女伸出手，一团火焰在掌心跳跃：\"火道，是最刚烈的道。火焰不会妥协，不会退让。\n\n修火道的人，也要像火焰一样——宁折不弯，焚尽一切！\"")
                .AddOption("焚天是什么招式？", "about_fentian")
                .AddOption("我了解了", "greeting");

            b.StartNode("about_fentian",
                "焚天魔女眼中燃起烈焰：\"焚天，是我的绝招。将全部火道真元凝聚于一点，然后瞬间爆发。\n\n这一招的代价很大——会消耗我大量的生命力。但只要能焚尽敌人，一切代价都值得！\"")
                .AddOption("太刚烈了", "respect_fierce")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("respect_fierce",
                "焚天魔女大笑：\"刚烈？这就是北荒的生存之道！软弱的人，在北荒活不过三天！\"")
                .AddOption("继续聊", "greeting")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("bye", "焚天魔女转身离去，身后留下一串火焰：\"记住，烈焰焚天，寸草不留！\"")
                .EndsDialogue();

            DialogueTreeManager.Instance.RegisterTree<FenTianMoNv>(b.Build());
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qiRealm = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel <= 0) return 0f;
            return 0.003f;
        }

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);
            tag["hasUsedFenTian"] = _hasUsedFenTian;
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            _hasUsedFenTian = tag.GetBool("hasUsedFenTian");
        }
    }
}
