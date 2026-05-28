using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
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
    public class QiHaiLaoZu : GuMasterBase
    {
        private int _attackTimer;
        private int _qiRegenTimer;
        private bool _hasUsedBigExplosion;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/FangYuan/QiHaiLaoZu_Head";

        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.Zhuan8_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Cunning;

        public override string GuMasterDisplayName => "气海老祖";
        public override int GuMasterDamage => 420;
        public override int GuMasterLife => 70000;
        public override int GuMasterDefense => 180;

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
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Silenced] = true;
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
            _qiRegenTimer = 0;
            _hasUsedBigExplosion = false;
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
                ExecuteQiCombatAI();
            }

            QiSeaRegeneration();
            QiAuraEffect();
        }

        private void QiAuraEffect()
        {
            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat() * 60f;
                Vector2 offset = angle.ToRotationVector2() * dist;
                Dust.NewDust(NPC.Center + offset, 4, 4, DustID.AncientLight,
                    0f, -0.8f, 100, Color.LightCyan, 0.7f);
            }
        }

        private void QiSeaRegeneration()
        {
            _qiRegenTimer++;
            if (_qiRegenTimer >= 120)
            {
                _qiRegenTimer = 0;
                int regenAmount = NPC.lifeMax / 50;
                if (NPC.life < NPC.lifeMax)
                {
                    NPC.life = Math.Min(NPC.life + regenAmount, NPC.lifeMax);
                    NPC.HealEffect(regenAmount, false);
                }
            }
        }

        private void ExecuteQiCombatAI()
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

            if (dist > 600f)
                NPC.velocity.X = dir * 3.5f;
            else if (dist > 250f)
                NPC.velocity.X = dir * 2f;
            else
                NPC.velocity.X = dir * 1.2f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -8f;

            _attackTimer++;
            if (_attackTimer >= 35)
            {
                _attackTimer = 0;
                ExecuteQiAttack(target);
            }

            if (!_hasUsedBigExplosion && NPC.life <= NPC.lifeMax * 0.3f)
            {
                _hasUsedBigExplosion = true;
                ExecuteBigExplosion(target);
            }
        }

        private void ExecuteQiAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 5;

            int pattern = Main.rand.Next(4);

            switch (pattern)
            {
                case 0:
                    CombatText.NewText(NPC.getRect(), Color.LightCyan, "气海无量！", true);
                    for (int i = 0; i < 20; i++)
                    {
                        float angle = MathHelper.TwoPi / 20f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 6f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.WaterBolt, damage, 2f, Main.myPlayer);
                    }
                    break;

                case 1:
                    for (int i = -3; i <= 3; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.12f;
                        Vector2 velocity = angle.ToRotationVector2() * 11f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.CultistBossLightningOrb, damage, 1f, Main.myPlayer);
                    }
                    break;

                case 2:
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 spawnPos = target.Center + new Vector2(
                            Main.rand.Next(-250, 250),
                            -350);
                        Vector2 velocity = new Vector2(0, 7f);
                        Projectile.NewProjectile(source, spawnPos, velocity,
                            ProjectileID.Meteor1, damage, 3f, Main.myPlayer);
                    }
                    break;

                case 3:
                    Vector2 dashDir = toTarget.SafeNormalize(Vector2.UnitX);
                    NPC.velocity = dashDir * 18f;
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 5f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.CultistBossLightningOrb, damage, 0f, Main.myPlayer);
                    }
                    break;
            }
        }

        private void ExecuteBigExplosion(Player target)
        {
            CombatText.NewText(NPC.getRect(), Color.OrangeRed, "一气大手爆——！", true);

            for (int i = 0; i < 60; i++)
            {
                Dust.NewDust(NPC.Center, 30, 30, DustID.Torch,
                    Main.rand.NextFloat(-8f, 8f),
                    Main.rand.NextFloat(-8f, 8f),
                    100, Color.OrangeRed, 2.5f);
            }

            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                int damage = NPC.damage / 3;

                for (int i = 0; i < 24; i++)
                {
                    float angle = MathHelper.TwoPi / 24f * i;
                    Vector2 velocity = angle.ToRotationVector2() * 10f;
                    Projectile.NewProjectile(source, NPC.Center, velocity,
                        ProjectileID.Fireball, damage, 4f, Main.myPlayer);
                }

                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi / 12f * i;
                    Vector2 velocity = angle.ToRotationVector2() * 7f;
                    Projectile.NewProjectile(source, NPC.Center, velocity,
                        ProjectileID.CultistBossLightningOrb, damage, 2f, Main.myPlayer);
                }

                for (int ring = 0; ring < 3; ring++)
                {
                    float baseAngle = MathHelper.TwoPi / 8f * ring;
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi / 8f * i;
                        Vector2 velocity = angle.ToRotationVector2() * (5f + ring * 3f);
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.WaterBolt, damage / 2, 1f, Main.myPlayer);
                    }
                }
            }

            Say("气海无量……一气爆天！", Color.OrangeRed);
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "气海之下，万法皆空。你——也不例外。",
                GuAttitude.Wary => "你感受到了吗？这是气海的压力。",
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => "能被你尊重，说明气海之名不是虚传。",
                GuAttitude.Contemptuous => "轻蔑？在气海面前，轻蔑只会加速你的灭亡。",
                GuAttitude.Fearful => "恐惧？气海之中，恐惧是最无用的情感。",
                _ => "气海无量，真元无穷。"
            };
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "气海老祖：\"气海，东海最神秘的修炼圣地。真元如海，永无穷尽。\"",
                "气海老祖：\"气道修炼到极致，便是——气海无量。真元不再枯竭，法力永无止境。\"",
                "气海老祖：\"一气大手爆，是我最得意的杀招。一气之下，山崩地裂。\"",
                "气海老祖：\"方源选择我掌控气海，是因为只有我能驾驭这无穷真元。\"",
                "气海老祖：\"东海的修士都叫我'老祖'，但我知道——在方源面前，我不过是棋盘上的一枚棋子。\"",
                "气海老祖：\"气海的真元可以滋养万物，也可以毁灭万物。全看使用者的心意。\"",
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
            var b = new DialogueTreeBuilder("FangYuan_QiHaiLaoZu", "greeting");

            b.StartNode("greeting",
                "气海老祖周身真元流转，如海潮般起伏不定。")
                .AddOption("询问气道", "ask_qi_dao", DialogueOptionType.Informative)
                .AddOption("关于气海", "ask_qi_sea", DialogueOptionType.Informative)
                .AddOption("关于一气大手爆", "ask_big_explosion", DialogueOptionType.Informative)
                .AddOption("关于方源", "ask_fangyuan", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_qi_dao",
                "气海老祖：\"气道，修炼真元之道。真元是蛊师的根本，而气道蛊师——可以将真元修炼到极致。气海无量，便是气道的最高境界。\"")
                .AddOption("真元和蛊虫的关系？", "ask_qi_gu_relation")
                .AddOption("气道如何战斗？", "ask_qi_combat")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_qi_gu_relation",
                "气海老祖：\"真元是驱动蛊虫的燃料。没有真元，再强的蛊虫也无法使用。气道修炼真元，就是从根本上增强蛊师的战斗力。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_qi_combat",
                "气海老祖：\"气道战斗？简单——用真元碾压一切。当你的真元无穷无尽时，不需要花哨的技巧，只需要——一力降十会。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_qi_sea",
                "气海老祖：\"气海，东海的一处奇地。这里真元浓郁如海，修炼速度是外界的十倍。而我——以气道入主气海，将这无穷真元化为己用。\"")
                .AddOption("气海是怎么形成的？", "ask_qi_sea_origin")
                .AddOption("其他人能进入气海吗？", "ask_qi_sea_entry")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_qi_sea_origin",
                "气海老祖：\"气海的形成……据说是一位远古气道尊者的道场。尊者陨落后，他的真元化为了这片气海，至今仍未消散。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_qi_sea_entry",
                "气海老祖：\"进入气海？没有我的允许，任何人都无法进入。气海有天然的真元屏障，只有气道蛊师才能穿越。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_big_explosion",
                "气海老祖：\"一气大手爆，八转杀招。将气海中的真元瞬间压缩，然后——引爆。威力足以毁天灭地，但代价是消耗大量真元。\"")
                .AddOption("你会经常使用吗？", "ask_use_frequency")
                .AddOption("有没有更强的招式？", "ask_stronger_move")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_use_frequency",
                "气海老祖：\"经常使用？不。一气大手爆的消耗太大，即使是我，也不能随意使用。只有在生死关头，才会动用这一招。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_stronger_move",
                "气海老祖：\"更强的招式？九转气道杀招——气吞天下。但那是方源本体的招式，我作为分身，还无法使用。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_fangyuan",
                "气海老祖：\"方源……他是我的本体。气海老祖只是他众多分身之一。但——我是唯一掌控气海的分身，这让我的地位与众不同。\"")
                .AddOption("你觉得自己是方源还是气海老祖？", "ask_identity")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_identity",
                "气海老祖沉思片刻：\"……两者都是。我拥有方源的记忆和智慧，但也有气海老祖的经历和感悟。这种双重身份，让我比其他分身更加……完整。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "气海老祖：\"气海的修炼资源，可以与你交换。真元丹、气海精华……你想要什么？\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "气海老祖微微颔首：\"去吧。气海之门，永远为有价值的人敞开。\"")
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
            return new List<string> { "气海老祖" };
        }
    }
}
