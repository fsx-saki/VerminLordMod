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
    public class MiaoYinXianZi : GuMasterBase
    {
        private static readonly Color DarkPurpleColor = new Color(48, 0, 96);
        public override FactionID GetFaction() => FactionID.ShadowSect;
        public override GuRank GetRank() => GuRank.Zhuan7_Gao;
        public override GuPersonality GetPersonality() => GuPersonality.Mysterious;

        public override string GuMasterDisplayName => "妙音仙子";
        public override int GuMasterDamage => 320;
        public override int GuMasterLife => 45000;
        public override int GuMasterDefense => 110;

        private int _attackTimer;
        private int _resonanceTimer;
        private bool _treeInitialized;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 700;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 30;
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
            NPC.knockBackResist = 0.25f;
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

            _resonanceTimer++;
            if (CurrentAIState == GuMasterAIState.Combat && _resonanceTimer % 60 == 0)
            {
                var player = Main.LocalPlayer;
                if (Vector2.Distance(NPC.Center, player.Center) < 500f)
                {
                    player.AddBuff(BuffID.Confused, 60);
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
                npc.velocity.X = fleeDir * 2f;
            }
            else if (dist > 400f)
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

            if (_attackTimer % 40 == 0 && dist < 500f)
            {
                SoundWaveAttack(npc, target);
            }

            if (_attackTimer % 100 == 0 && dist < 400f)
            {
                MiaoShouXuanYin(npc, target);
            }

            if (_attackTimer % 150 == 0 && dist < 350f)
            {
                ResonanceAttack(npc, target);
            }

            if (Main.rand.NextBool(110))
            {
                string[] lines = { "此音只应天上有。", "听好了……这是你最后的旋律。", "音波之中，无处可逃。" };
                CombatText.NewText(npc.getRect(), Color.MediumPurple, lines[Main.rand.Next(lines.Length)], true);
            }
        }

        private void SoundWaveAttack(NPC npc, Player target)
        {
            Vector2 dir = Vector2.Normalize(target.Center - npc.Center);
            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 8f,
                ProjectileID.HarpyFeather, npc.damage / 4, 3f, Main.myPlayer);

            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(npc.Center, 10, 10, DustID.PurpleTorch,
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
                    100, Color.MediumPurple, 1.5f);
            }
        }

        private void MiaoShouXuanYin(NPC npc, Player target)
        {
            int trueDamage = Math.Max(npc.damage / 2, npc.damage - target.statDefense / 2);

            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.ToRadians(-40 + i * 20);
                Vector2 dir = Vector2.Normalize(target.Center - npc.Center).RotatedBy(angle);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 6f,
                    ProjectileID.HarpyFeather, trueDamage / 5, 2f, Main.myPlayer);
            }

            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(npc.Center, 10, 10, DustID.PurpleTorch,
                    Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f),
                    150, Color.Purple, 2f);
            }

            CombatText.NewText(npc.getRect(), Color.Purple, "妙手玄音！", true);
        }

        private void ResonanceAttack(NPC npc, Player target)
        {
            for (int i = 0; i < 12; i++)
            {
                float angle = i * MathHelper.TwoPi / 12;
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 5f,
                    ProjectileID.HarpyFeather, npc.damage / 5, 2f, Main.myPlayer);
            }

            target.AddBuff(BuffID.Slow, 120);
            target.AddBuff(BuffID.Darkness, 90);

            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(npc.Center, 20, 20, DustID.PurpleTorch,
                    Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f),
                    200, DarkPurpleColor, 2.5f);
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "妙音仙子冷冷地说：\"此音只应天上有……你却偏要来听。\"",
                GuAttitude.Wary => "妙音仙子神秘地微笑：\"你对我很好奇？小心，好奇心会害死猫。\"",
                GuAttitude.Friendly => NumberOfTimesTalkedTo <= 1
                    ? "妙音仙子轻声说：\"此音只应天上有。你……听得见吗？\""
                    : "妙音仙子微微一笑：\"又来了。看来你被我的旋律吸引了。\"",
                GuAttitude.Respectful => "妙音仙子点头道：\"你的实力不错。南疆三仙，不是浪得虚名。\"",
                GuAttitude.Contemptuous => "妙音仙子轻哼一声：\"你听不懂这天籁之音。\"",
                GuAttitude.Fearful => "妙音仙子皱眉道：\"这种情况……不在我的乐谱之中。\"",
                _ => "妙音仙子看了你一眼，嘴角带着神秘的微笑。"
            };
        }

        private void InitializeDialogueTree()
        {
            var b = new DialogueTreeBuilder("MiaoYinXianZi", "greeting");

            b.StartNode("greeting", "妙音仙子轻声说：\"此音只应天上有。你……听得见吗？\"")
                .AddOption("你是谁？", "who_are_you", DialogueOptionType.Informative)
                .AddOption("关于音道", "about_sound", DialogueOptionType.Informative)
                .AddOption("南疆三仙？", "about_three_fairies", DialogueOptionType.Informative)
                .AddOption("你和影宗……", "about_shadow", DialogueOptionType.Risky)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("who_are_you",
                "妙音仙子神秘地微笑：\"我是妙音仙子，南疆三仙之一，七转音道蛊师。\n\n我的音律能治愈伤痛，也能取人性命。全凭心意。\"")
                .AddOption("南疆三仙？", "about_three_fairies")
                .AddOption("音道是什么？", "about_sound")
                .AddOption("我了解了", "greeting");

            b.StartNode("about_sound",
                "妙音仙子轻轻弹指，一段旋律在空气中回荡：\"音道，是以声音为核心的修行之道。\n\n声音可以穿透一切防御，直达灵魂。这就是音道的可怕之处——你无法用铠甲抵挡声音。\"")
                .AddOption("妙手玄音是什么？", "about_miaoshou")
                .AddOption("我了解了", "greeting");

            b.StartNode("about_miaoshou",
                "妙音仙子眼中闪过一丝精光：\"妙手玄音，是我的绝技。以音波穿透敌人的防御，直接攻击灵魂。\n\n再坚固的铠甲，也挡不住声音的穿透。这就是音道的霸道之处。\"")
                .AddOption("太厉害了", "respect_sound")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("respect_sound",
                "妙音仙子微微一笑：\"音道虽然强大，但也有弱点——音道蛊师通常防御较低。\n\n所以，我们更擅长在暗处行动。\"")
                .AddOption("你和影宗……", "about_shadow", DialogueOptionType.Risky)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("about_three_fairies",
                "妙音仙子回忆道：\"南疆三仙，是南疆最著名的三位女蛊师——我、还有另外两位姐妹。\n\n我们各有所长，但都擅长音道。三仙合奏，威力倍增。\"")
                .AddOption("三仙合奏有多强？", "trio_power")
                .AddOption("我了解了", "greeting");

            b.StartNode("trio_power",
                "妙音仙子神秘地说：\"三仙合奏，足以让八转蛊师也感到棘手。不过……这种机会不多。\n\n每个人都有自己的秘密和立场。\"")
                .AddOption("你和影宗……", "about_shadow", DialogueOptionType.Risky)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("about_shadow",
                "妙音仙子表情不变，但周围的空气似乎凝固了一瞬：\"影宗？你为什么这么问？\n\n……有些事情，知道太多对你没有好处。我只是一个普通的音道蛊师。\"")
                .AddOption("你不必隐瞒", "shadow_press", DialogueOptionType.Risky)
                .AddOption("我不会追问", "respect_secret")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("shadow_press",
                "妙音仙子冷冷地看着你：\"你很执着。但执着的人，往往死得最快。\n\n记住，影宗的暗棋，不是你能窥探的。\"")
                .AddOption("我明白了", "respect_secret")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("respect_secret",
                "妙音仙子恢复了神秘的微笑：\"你很聪明。在这个世界上，有些秘密最好永远埋在心底。\n\n此音只应天上有……你听到的，只是冰山一角。\"")
                .AddOption("继续聊", "greeting")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("bye", "妙音仙子转身离去，一段旋律在空气中回荡：\"此音只应天上有……\"")
                .EndsDialogue();

            DialogueTreeManager.Instance.RegisterTree<MiaoYinXianZi>(b.Build());
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qiRealm = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel <= 0) return 0f;
            return 0.003f;
        }
    }
}
