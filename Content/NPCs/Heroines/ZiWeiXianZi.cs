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
    public class ZiWeiXianZi : GuMasterBase
    {
        public override FactionID GetFaction() => FactionID.Heaven;
        public override GuRank GetRank() => GuRank.Zhuan8_Gao;
        public override GuPersonality GetPersonality() => GuPersonality.Cunning;

        public override string GuMasterDisplayName => "紫薇仙子";
        public override int GuMasterDamage => 380;
        public override int GuMasterLife => 70000;
        public override int GuMasterDefense => 200;

        private int _attackTimer;
        private int _predictionTimer;
        private Vector2 _predictedPlayerPos;
        private bool _treeInitialized;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 1000;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 25;
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
            NPC.knockBackResist = 0.1f;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath7;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(0, 10, 0, 0);
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

            _predictionTimer++;
            if (_predictionTimer % 10 == 0)
            {
                var player = Main.player[NPC.target];
                _predictedPlayerPos = player.Center + player.velocity * 30f;
            }
        }

        public override void ExecuteCombatAI(NPC npc)
        {
            var target = Main.player[npc.target];
            float dist = Vector2.Distance(npc.Center, target.Center);

            npc.spriteDirection = target.Center.X > npc.Center.X ? 1 : -1;

            if (dist < 200f)
            {
                float fleeDir = npc.Center.X > target.Center.X ? 1 : -1;
                npc.velocity.X = fleeDir * 2f;
            }
            else if (dist > 400f)
            {
                float dir = target.Center.X > npc.Center.X ? 1 : -1;
                npc.velocity.X = dir * 1.8f;
            }
            else
            {
                npc.velocity.X *= 0.9f;
            }

            if (npc.collideX && npc.velocity.Y == 0)
                npc.velocity.Y = -7f;

            _attackTimer++;

            if (_attackTimer % 50 == 0 && dist < 600f)
            {
                Vector2 aimDir = Vector2.Normalize(_predictedPlayerPos - npc.Center);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, aimDir * 10f,
                    ProjectileID.StarCloakStar, npc.damage / 4, 4f, Main.myPlayer);
            }

            if (_attackTimer % 120 == 0 && dist < 500f)
            {
                StarFormationAttack(npc, target);
            }

            if (_attackTimer % 200 == 0 && dist < 400f)
            {
                StrategicBarrage(npc, target);
            }

            if (Main.rand.NextBool(120))
            {
                string[] lines = { "棋局已定。", "你的一举一动，尽在我掌握。", "天庭的棋局，远比你想象的复杂。" };
                CombatText.NewText(npc.getRect(), Color.Purple, lines[Main.rand.Next(lines.Length)], true);
            }
        }

        private void StarFormationAttack(NPC npc, Player target)
        {
            int starCount = 7;
            float radius = 150f;
            for (int i = 0; i < starCount; i++)
            {
                float angle = i * MathHelper.TwoPi / starCount;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 spawnPos = target.Center + offset;
                Vector2 dir = Vector2.Normalize(target.Center - spawnPos);
                Projectile.NewProjectile(npc.GetSource_FromAI(), spawnPos, dir * 5f,
                    ProjectileID.StarCloakStar, npc.damage / 5, 3f, Main.myPlayer);
            }
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(target.Center, 10, 10, DustID.PurpleTorch,
                    Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f),
                    100, Color.Purple, 2f);
            }
        }

        private void StrategicBarrage(NPC npc, Player target)
        {
            Vector2 predictedPos = target.Center + target.velocity * 20f;
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = new Vector2(Main.rand.Next(-100, 100), -300 - i * 50);
                Vector2 spawnPos = predictedPos + offset;
                Vector2 dir = Vector2.Normalize(predictedPos - spawnPos);
                Projectile.NewProjectile(npc.GetSource_FromAI(), spawnPos, dir * 8f,
                    ProjectileID.Meteor1, npc.damage / 3, 5f, Main.myPlayer);
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "紫薇仙子冷冷地说：\"天庭的棋局，远比你想象的复杂。你不过是一枚棋子。\"",
                GuAttitude.Wary => "紫薇仙子审视着你：\"你有些不寻常……让我仔细看看。\"",
                GuAttitude.Friendly => NumberOfTimesTalkedTo <= 1
                    ? "紫薇仙子微微一笑：\"天庭的棋局，远比你想象的复杂。不过，你或许能成为一枚有趣的棋子。\""
                    : "紫薇仙子淡淡地说：\"又来了？看来你对我很感兴趣。\"",
                GuAttitude.Respectful => "紫薇仙子点头道：\"你的实力不俗。在天庭，强者才能获得尊重。\"",
                GuAttitude.Contemptuous => "紫薇仙子不屑道：\"蝼蚁也想撼天？不自量力。\"",
                GuAttitude.Fearful => "紫薇仙子皱眉道：\"这种情况……不在我的计算之中。\"",
                _ => "紫薇仙子看了你一眼，似乎在计算什么。"
            };
        }

        private void InitializeDialogueTree()
        {
            var b = new DialogueTreeBuilder("ZiWeiXianZi", "greeting");

            b.StartNode("greeting", "紫薇仙子微微一笑：\"天庭的棋局，远比你想象的复杂。不过，你或许能成为一枚有趣的棋子。\"")
                .AddOption("你是谁？", "who_are_you", DialogueOptionType.Informative)
                .AddOption("天庭是什么？", "about_heaven", DialogueOptionType.Informative)
                .AddOption("关于智道", "about_wisdom", DialogueOptionType.Informative)
                .AddOption("我听说你有双重身份……", "dual_identity", DialogueOptionType.Risky)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("who_are_you",
                "紫薇仙子优雅地说：\"我是紫薇，天庭八转蛊师，智道修行的佼佼者。\n\n在天庭，我负责推演天机、制定战略。每一步棋，都关乎天庭的兴衰。\"")
                .AddOption("天庭是什么？", "about_heaven")
                .AddOption("八转……很强吗？", "about_rank")
                .AddOption("我了解了", "greeting");

            b.StartNode("about_heaven",
                "紫薇仙子缓缓道来：\"天庭，是蛊师世界最强大的势力之一。由历代天尊创立，掌控着无数资源和传承。\n\n天庭的目标只有一个——维护天地秩序，延续天庭的统治。为此，我们不惜一切代价。\"")
                .AddOption("不惜一切代价？", "heaven_cost")
                .AddOption("关于影宗", "about_shadow")
                .AddOption("我了解了", "greeting");

            b.StartNode("heaven_cost",
                "紫薇仙子眼中闪过一丝复杂：\"天庭的代价……是无数蛊师的生命和自由。但这是必要的牺牲。\n\n至少，天庭是这样认为的。\"")
                .AddOption("你真的这么认为吗？", "dual_identity")
                .AddOption("我了解了", "greeting");

            b.StartNode("about_shadow",
                "紫薇仙子表情不变，但语气微妙地变了：\"影宗……天庭的宿敌。他们潜伏在暗处，伺机而动。\n\n不过，敌人的敌人未必是朋友。这个道理，你应该懂。\"")
                .AddOption("你似乎很了解影宗", "dual_identity")
                .AddOption("我了解了", "greeting");

            b.StartNode("dual_identity",
                "紫薇仙子沉默了片刻，然后低声说：\"你很敏锐……不过，有些事情，知道太多对你没有好处。\n\n天庭的棋局，影宗的暗棋……我站在哪里，连我自己有时都不确定。\"")
                .AddOption("你在影宗？", "shadow_reveal", DialogueOptionType.Risky)
                .AddOption("我不会追问", "respect_secret")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("shadow_reveal",
                "紫薇仙子冷冷地看着你：\"这个问题，你不该问。在天庭，知道太多的人，往往活不长。\n\n但既然你已经猜到了……我会记住你的。\"")
                .AddOption("我不会说出去", "respect_secret")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("respect_secret",
                "紫薇仙子微微点头：\"你很聪明。在这个世界上，聪明人才能活得长久。\n\n记住，天庭的棋局远比你想象的复杂。而棋子……有时候也能成为棋手。\"")
                .AddOption("继续聊", "greeting")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("about_wisdom",
                "紫薇仙子解释道：\"智道，是以智慧为核心的修行之道。智道蛊师擅长推演、算计、预判。\n\n在战斗中，智道蛊师能预测对手的每一步行动，从而先发制人。这不是读心术，而是基于逻辑和概率的推演。\"")
                .AddOption("你能预测我的行动？", "predict_me")
                .AddOption("我了解了", "greeting");

            b.StartNode("predict_me",
                "紫薇仙子微微一笑：\"你已经向右移动了三次，每次遇到障碍都会跳跃。你的战斗模式……我已经掌握了七成。\n\n这就是智道的可怕之处。\"")
                .AddOption("太厉害了", "respect_secret")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("about_rank",
                "紫薇仙子淡淡地说：\"八转，已经站在了蛊师世界的顶端。再往上，就是九转——天尊的领域。\n\n不过，修为不是一切。真正的强者，是能以弱胜强的智者。\"")
                .AddOption("继续聊", "greeting")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("bye", "紫薇仙子转身离去：\"棋局还在继续，我们还会再见。\"")
                .EndsDialogue();

            DialogueTreeManager.Instance.RegisterTree<ZiWeiXianZi>(b.Build());
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qiRealm = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel <= 0) return 0f;
            return 0.002f;
        }
    }
}
