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
    public class GuYueFangYuan : GuMasterBase
    {
        private bool _hasUsedSpringAutumnCicada;
        private int _attackTimer;
        private int _refineTimer;
        private int _currentAttackPattern;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/FangYuan/GuYueFangYuan_Head";

        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.Zhuan9_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Cunning;

        public override string GuMasterDisplayName => "方源";
        public override int GuMasterDamage => 600;
        public override int GuMasterLife => 150000;
        public override int GuMasterDefense => 300;

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
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Ichor] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Slow] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Weak] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Bleeding] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.BrokenArmor] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Silenced] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Darkness] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Blackout] = true;

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
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(10, 0, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _hasUsedSpringAutumnCicada = false;
            _attackTimer = 0;
            _refineTimer = 0;
            _currentAttackPattern = 0;
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
                ExecuteBossCombatAI();
            }

            SpringAutumnCicadaCheck();
            RefineHeavenDemonCheck();
            SupremeImmortalFetusCheck();
        }

        private void SpringAutumnCicadaCheck()
        {
            if (!_hasUsedSpringAutumnCicada && NPC.life <= NPC.lifeMax * 0.1f)
            {
                _hasUsedSpringAutumnCicada = true;
                NPC.life = NPC.lifeMax;
                NPC.HealEffect(NPC.lifeMax, true);
                CombatText.NewText(NPC.getRect(), Color.Gold, "春秋蝉——时光倒流！", true);

                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.Center, 20, 20, DustID.GoldFlame,
                        Main.rand.NextFloat(-6f, 6f),
                        Main.rand.NextFloat(-6f, 6f),
                        100, Color.Gold, 2.5f);
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    var source = NPC.GetSource_FromAI();
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi / 12f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 8f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.CultistBossLightningOrb, NPC.damage / 4, 0f, Main.myPlayer);
                    }
                }

                Say("春秋蝉……我回来了。", Color.Gold);
            }
        }

        private void RefineHeavenDemonCheck()
        {
            if (CurrentAttitude != GuAttitude.Hostile && !(HasBeenHitByPlayer && AggroTimer > 0))
                return;

            _refineTimer++;
            if (_refineTimer < 60) return;
            _refineTimer = 0;

            int absorbedCount = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                var proj = Main.projectile[i];
                if (proj.active && proj.hostile && Vector2.Distance(NPC.Center, proj.Center) < 200f)
                {
                    absorbedCount++;
                    proj.Kill();
                }
            }

            if (absorbedCount > 0)
            {
                int healAmount = absorbedCount * 500;
                NPC.life = Math.Min(NPC.life + healAmount, NPC.lifeMax);
                NPC.HealEffect(healAmount, true);
                CombatText.NewText(NPC.getRect(), Color.Purple, "炼天魔尊——万物皆可炼！", true);

                for (int i = 0; i < absorbedCount * 3; i++)
                {
                    Dust.NewDust(NPC.Center, 20, 20, DustID.PurpleTorch,
                        Main.rand.NextFloat(-4f, 4f),
                        Main.rand.NextFloat(-4f, 4f),
                        100, Color.Purple, 1.5f);
                }
            }
        }

        private void SupremeImmortalFetusCheck()
        {
            for (int i = 0; i < NPC.buffType.Length; i++)
            {
                if (NPC.buffType[i] > 0 && NPC.buffTime[i] > 0)
                {
                    if (NPC.buffType[i] != BuffID.WellFed && NPC.buffType[i] != BuffID.SugarRush)
                    {
                        NPC.DelBuff(i);
                        i--;
                    }
                }
            }
        }

        private void ExecuteBossCombatAI()
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
            {
                NPC.velocity.X = dir * 4f;
            }
            else if (dist > 200f)
            {
                NPC.velocity.X = dir * 2.5f;
            }
            else
            {
                NPC.velocity.X = dir * 1.5f;
            }

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -10f;

            _attackTimer++;
            if (_attackTimer >= 30)
            {
                _attackTimer = 0;
                _currentAttackPattern = (_currentAttackPattern + 1) % 5;
                ExecuteAttackPattern(target);
            }
        }

        private void ExecuteAttackPattern(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            Vector2 normalized = toTarget.SafeNormalize(Vector2.UnitY);
            int damage = NPC.damage / 4;

            switch (_currentAttackPattern)
            {
                case 0:
                    for (int i = -2; i <= 2; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.15f;
                        Vector2 velocity = angle.ToRotationVector2() * 10f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.Fireball, damage, 3f, Main.myPlayer);
                    }
                    break;

                case 1:
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 7f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.IceBlock, damage, 2f, Main.myPlayer);
                    }
                    break;

                case 2:
                    Projectile.NewProjectile(source, NPC.Center, normalized * 12f,
                        ProjectileID.CultistBossLightningOrb, damage, 0f, Main.myPlayer);
                    for (int i = -1; i <= 1; i += 2)
                    {
                        float angle = toTarget.ToRotation() + i * 0.3f;
                        Vector2 velocity = angle.ToRotationVector2() * 10f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.CultistBossLightningOrb, damage, 0f, Main.myPlayer);
                    }
                    break;

                case 3:
                    for (int i = 0; i < 16; i++)
                    {
                        float angle = MathHelper.TwoPi / 16f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 6f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.WaterBolt, damage, 2f, Main.myPlayer);
                    }
                    break;

                case 4:
                    Vector2 above = target.Center - new Vector2(0, 300f);
                    for (int i = -3; i <= 3; i++)
                    {
                        Vector2 spawnPos = above + new Vector2(i * 60, 0);
                        Vector2 velocity = new Vector2(0, 12f);
                        Projectile.NewProjectile(source, spawnPos, velocity,
                            ProjectileID.Meteor1, damage * 2, 5f, Main.myPlayer);
                    }
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
                    1 => "你挡了我的路。在永生面前，一切皆是虚妄。",
                    2 => "你以为能阻止我？可笑。",
                    3 => "既然你执意找死，我便成全你。",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => NumberOfTimesTalkedTo switch
                {
                    1 => "有趣……你比我想象的要谨慎。",
                    2 => "你在试探我？还是你在害怕？",
                    3 => "保持警惕是好事，但过度警惕只会错失良机。",
                    _ => WaryDialogue()
                },
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => NumberOfTimesTalkedTo switch
                {
                    1 => "你让我想起了……曾经的自己。",
                    2 => "能走到今天，你也不容易。但这条路，远比你想象的要长。",
                    3 => "强者之间的尊重，不需要言语。你我心知肚明。",
                    _ => RespectfulDialogue()
                },
                GuAttitude.Contemptuous => "你不值得我浪费时间。",
                GuAttitude.Fearful => "……有意思。能让我感到威胁的人，这五百年来不超过三个。",
                _ => "永生，是我唯一的追求。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "在永生的道路上，你不过是一粒尘埃。",
                "我活了五百年，见过太多像你这样的人。",
                "你以为你在战斗？你只是在浪费时间。",
                "死亡对我来说，不过是另一次开始。",
                "你的挣扎毫无意义。",
                "我曾在青茅山从零开始，你算什么？",
                "义天山上万人围攻，我亦从容而退。",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        private string WaryDialogue()
        {
            var lines = new List<string>
            {
                "你很聪明，但聪明不等于智慧。",
                "我在观察你，就像观察棋盘上的一枚棋子。",
                "你的每一步，都在我的计算之中。",
                "谨慎？不，我只是在评估你的价值。",
                "你让我想起了三王传承时的那些对手……",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "交易？我从不做亏本的买卖。",
                "你有我需要的东西，我有你需要的东西。这就是交易的本质。",
                "在这个世界上，没有永远的敌人，只有永远的利益。",
                "我方源活了五百年，最明白一个道理——一切皆可交易。",
                "你想要什么？力量？长生？还是……永生？",
                "我可以给你想要的一切，但你必须付出等价的代价。",
                "聪明人之间的合作，总是令人愉快的。",
                "你比大多数人都有价值。这是赞美，不是威胁。",
            };
            return dialogues[NumberOfTimesTalkedTo % dialogues.Count];
        }

        private string RespectfulDialogue()
        {
            var lines = new List<string>
            {
                "你让我想起了……曾经的自己。那时候我也像你一样，满怀希望。",
                "强者之间的尊重，不需要言语。你我心知肚明。",
                "能走到今天，你也不容易。但这条路，远比你想象的要长。",
                "你是我五百年来遇到的少数值得尊重的人之一。",
                "宿命大战时，我也曾遇到过像你这样的人……可惜，他们都已经不在了。",
            };
            return lines[Main.rand.Next(lines.Count)];
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
            var b = new DialogueTreeBuilder("FangYuan_GuYueFangYuan", "greeting");

            b.StartNode("greeting",
                "方源平静地看着你，眼中深不见底。")
                .AddOption("谈论永生", "immortality", DialogueOptionType.Special)
                .AddOption("询问你的过去", "past_lives", DialogueOptionType.Informative)
                .AddOption("关于蛊世界", "gu_world", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("哲学讨论", "philosophy", DialogueOptionType.Informative)
                .AddOption("关于分身", "avatars", DialogueOptionType.Informative)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("immortality",
                "方源：\"永生……这是世间唯一的真理。在永生面前，一切道德、情感、善恶，不过是过眼云烟。\"")
                .AddOption("永生真的值得一切代价吗？", "immortality_cost")
                .AddOption("你离永生还有多远？", "immortality_distance")
                .AddOption("如果永生是假的呢？", "immortality_doubt")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("immortality_cost",
                "方源淡淡一笑：\"代价？你问一个活了五百年的人代价是什么？我失去了一切——朋友、爱人、尊严、甚至人性。但与永生相比，这些不过是旅途中的落叶。\"")
                .AddOption("你后悔过吗？", "immortality_regret")
                .AddOption("你失去了什么？", "immortality_loss")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("immortality_regret",
                "方源沉默了片刻：\"后悔？……后悔是弱者的情感。我从不后悔，因为后悔毫无意义。走过的路不能回头，只能向前。\"")
                .AddOption("即使牺牲了所有人？", "immortality_sacrifice")
                .AddOption("我理解了", "greeting");

            b.StartNode("immortality_sacrifice",
                "方源：\"你问我是否后悔牺牲了所有人？让我告诉你——在青茅山，我亲手杀死了收留我的族人。在三王传承中，我利用了每一个信任我的人。在义天山上，我背叛了所有盟友。但我不后悔，因为这一切都是通往永生的阶梯。\"")
                .AddOption("……你真的没有心吗？", "immortality_heartless")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("immortality_heartless",
                "方源：\"心？我有心。但我的心只为一件事跳动——永生。你可以称之为无情，但我称之为专注。在这个世界上，只有偏执狂才能生存。\"")
                .AddOption("我无法认同你的方式", "greeting")
                .AddOption("也许你是对的", "greeting");

            b.StartNode("immortality_loss",
                "方源：\"我失去了什么？第一世，我失去了家族。第二世，我失去了爱情。第三世，我失去了自由。第四世……我失去了对生命的敬畏。但每一次失去，都让我离永生更近一步。\"")
                .AddOption("值得吗？", "immortality_regret")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("immortality_distance",
                "方源：\"我离永生……只差一步。但这一步，可能需要又一个五百年。宿命蛊曾经是唯一的障碍，如今宿命已破，但永生的道路依然漫长。\"")
                .AddOption("宿命蛊是什么？", "past_fate_battle")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("immortality_doubt",
                "方源看了你一眼：\"如果永生是假的？那我这五百年的执念就是一场笑话。但即便如此，我也不会停下。因为——追求本身，就是意义。\"")
                .AddOption("追求本身就是意义？", "philosophy_meaning")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("past_lives",
                "方源：\"我的过去？你确定你想知道？那些故事里，没有英雄，只有算计。\"")
                .AddOption("青茅山的往事", "past_qingmao")
                .AddOption("三王传承", "past_three_kings")
                .AddOption("义天山之战", "past_yitian")
                .AddOption("宿命大战", "past_fate_battle")
                .AddOption("你最初的五百年", "past_first_500")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("past_qingmao",
                "方源：\"青茅山……那是我一切的起点。古月山寨收留了我，而我亲手将它毁灭。你也许觉得残忍，但那是生存的法则——弱肉强食，天经地义。\"")
                .AddOption("古月山寨的人恨你吗？", "past_qingmao_hate")
                .AddOption("你从青茅山学到了什么？", "past_qingmao_learn")
                .AddOption("回到过去", "past_lives");

            b.StartNode("past_qingmao_hate",
                "方源：\"恨？当然恨。但死人是不会恨的。而活着的人……他们最终会明白，我做的选择是正确的。在蛊的世界里，仁慈是最大的弱点。\"")
                .AddOption("继续", "past_lives");

            b.StartNode("past_qingmao_learn",
                "方源：\"我学到了最重要的一课——在这个世界上，没有人会无条件地帮助你。一切关系都是交易，一切善意都有代价。明白了这一点，你就不会再被任何人欺骗。\"")
                .AddOption("令人心寒", "past_lives")
                .AddOption("但这是事实", "past_lives");

            b.StartNode("past_three_kings",
                "方源：\"三王传承……那是我第一次真正展露实力。三王的宝藏，无数蛊师争夺，而我——一个散修，最终得到了最大的好处。你知道为什么吗？因为其他人都在争，而我在算。\"")
                .AddOption("你是怎么做到的？", "past_three_kings_how")
                .AddOption("三王是谁？", "past_three_kings_who")
                .AddOption("回到过去", "past_lives");

            b.StartNode("past_three_kings_how",
                "方源：\"很简单——让别人替我打，我在最后收割成果。蛊师世界的法则就是如此：最聪明的猎人，往往不是最先出手的那个。\"")
                .AddOption("继续", "past_lives");

            b.StartNode("past_three_kings_who",
                "方源：\"三王？那是南疆的三位强者——熊家、白家、和另一位。他们的传承之争，不过是棋盘上的一局。而我，是那个下棋的人。\"")
                .AddOption("继续", "past_lives");

            b.StartNode("past_yitian",
                "方源：\"义天山……那一战，我以一己之力对抗天下群雄。不是为了正义，不是为了荣耀，只是为了——活着。在义天山上，我证明了一个人可以对抗整个世界，只要你足够冷酷，足够聪明。\"")
                .AddOption("你不怕死吗？", "past_yitian_death")
                .AddOption("义天山上发生了什么？", "past_yitian_what")
                .AddOption("回到过去", "past_lives");

            b.StartNode("past_yitian_death",
                "方源：\"怕死？我曾经死过。春秋蝉让我重生，而重生之后，我再也不怕死了。因为——死亡不过是另一场开始。\"")
                .AddOption("春秋蝉是什么？", "past_fate_battle")
                .AddOption("回到过去", "past_lives");

            b.StartNode("past_yitian_what",
                "方源：\"义天山上，天下蛊师齐聚，围攻于我。但我早已布下天罗地网，每一步都在我的算计之中。最终，他们以为困住了我，却不知——那正是我计划的一部分。\"")
                .AddOption("继续", "past_lives");

            b.StartNode("past_fate_battle",
                "方源：\"宿命大战……那是我人生中最重要的战役。宿命蛊，掌控一切命运的至高蛊虫。我方源，以凡人之躯，逆天改命，最终击碎了宿命。从此——命运不再束缚任何人。\"")
                .AddOption("击碎宿命之后呢？", "past_after_fate")
                .AddOption("宿命蛊真的存在吗？", "past_fate_exist")
                .AddOption("回到过去", "past_lives");

            b.StartNode("past_after_fate",
                "方源：\"宿命破碎后，世界变得……混乱。没有了命运的指引，每个人都必须自己选择道路。这很好——因为只有在混乱中，像我这样的人才能找到机会。\"")
                .AddOption("你利用了混乱？", "past_lives")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("past_fate_exist",
                "方源：\"宿命蛊当然存在。它曾经决定了每一个人的命运——你出生在哪里，你会遇到谁，你会怎么死。而我……我亲手杀了它。\"")
                .AddOption("继续", "past_lives");

            b.StartNode("past_first_500",
                "方源：\"最初的五百年……我从一个凡人，一步步修炼到九转。期间经历了无数生死，无数背叛，无数绝望。但我从未放弃，因为——永生就在前方。\"")
                .AddOption("你曾经有过同伴吗？", "past_companions")
                .AddOption("你曾经爱过谁吗？", "past_love")
                .AddOption("回到过去", "past_lives");

            b.StartNode("past_companions",
                "方源：\"同伴？有过。但最终，他们都成了我通往永生的垫脚石。不是我不念旧情，而是——在永生面前，一切情感都是暂时的。\"")
                .AddOption("你真的这么认为吗？", "past_companions_doubt")
                .AddOption("回到过去", "past_lives");

            b.StartNode("past_companions_doubt",
                "方源沉默良久：\"……也许吧。也许在某个深夜，我也会想起那些曾经并肩的人。但天亮之后，我依然会继续前行。因为——停下来，就意味着死亡。\"")
                .AddOption("……", "past_lives");

            b.StartNode("past_love",
                "方源的表情第一次出现了波动：\"爱？……我曾经爱过一个人。但那已经是几百年前的事了。在追求永生的路上，爱情是最先被抛弃的东西。\"")
                .AddOption("你还记得她吗？", "past_love_remember")
                .AddOption("回到过去", "past_lives");

            b.StartNode("past_love_remember",
                "方源：\"记得？当然记得。五百年的一切，我都记得清清楚楚。但记住不代表留恋。记忆只是工具，用来避免重蹈覆辙。\"")
                .AddOption("我理解了", "past_lives");

            b.StartNode("gu_world",
                "方源：\"蛊世界？你想了解这个世界的真相？\"")
                .AddOption("蛊虫的本质是什么？", "gu_world_nature")
                .AddOption("蛊师的等级体系", "gu_world_ranks")
                .AddOption("各大势力", "gu_world_factions")
                .AddOption("蛊世界的法则", "gu_world_laws")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("gu_world_nature",
                "方源：\"蛊虫是天地精华的凝聚，是规则的具现。每一只蛊虫都蕴含着一条天地法则。蛊师修炼的本质，就是通过蛊虫来驾驭天地法则。\"")
                .AddOption("那九转蛊虫呢？", "gu_world_nine_turn")
                .AddOption("回到蛊世界", "gu_world");

            b.StartNode("gu_world_nine_turn",
                "方源：\"九转蛊虫……那是接近天道本身的存在。每一只九转蛊，都足以改写世界格局。而春秋蝉——那是唯一能逆转时间的九转蛊。\"")
                .AddOption("春秋蝉在你手中？", "past_fate_battle")
                .AddOption("回到蛊世界", "gu_world");

            b.StartNode("gu_world_ranks",
                "方源：\"蛊师分为一转到九转，每一转都是天堑。一转开辟空窍，二转炼蛊，三转凝蛊，四转蛊师已是一方之主。五转以上便是蛊仙，超脱生死轮回。而我——九转，站在这个世界的巅峰。\"")
                .AddOption("九转是什么概念？", "gu_world_ranks_nine")
                .AddOption("回到蛊世界", "gu_world");

            b.StartNode("gu_world_ranks_nine",
                "方源：\"九转……就是尊者。整个蛊世界，同时存在的尊者不超过五位。每一位尊者，都是足以改写世界规则的存在。而我，是其中最危险的那个。\"")
                .AddOption("回到蛊世界", "gu_world");

            b.StartNode("gu_world_factions",
                "方源：\"蛊世界的势力？中洲天庭，南疆散修，北原兽族，西漠佛门，东海龙宫……每个势力都有九转尊者坐镇。而我——不属于任何势力。我只属于我自己。\"")
                .AddOption("天庭是什么？", "gu_world_heaven")
                .AddOption("回到蛊世界", "gu_world");

            b.StartNode("gu_world_heaven",
                "方源：\"天庭……蛊世界最强大的势力，掌控宿命蛊数万年。他们自诩正义，实则不过是维护自身统治的工具。而我击碎了宿命，也就击碎了天庭的根基。\"")
                .AddOption("继续", "gu_world");

            b.StartNode("gu_world_laws",
                "方源：\"蛊世界的法则？弱肉强食，这是唯一的法则。什么道德、正义、仁慈，不过是强者用来约束弱者的枷锁。真正的强者，只遵循自己的法则。\"")
                .AddOption("这就是你的道？", "philosophy_fangyuan_dao")
                .AddOption("回到蛊世界", "gu_world");

            b.StartNode("trade",
                "方源：\"交易？说吧，你想要什么。但记住——我方源从不做亏本的买卖。\"")
                .AddOption("购买炼蛊材料", "trade_materials", DialogueOptionType.Trade)
                .AddOption("购买蛊虫", "trade_gu", DialogueOptionType.Trade)
                .AddOption("请求修炼指导", "trade_training", DialogueOptionType.Trade)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade_materials",
                "方源：\"炼蛊材料？我这里有些存货。价格公道，童叟无欺——当然，'公道'的定义由我来定。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade_gu",
                "方源：\"蛊虫？你想买我的蛊虫？可以。但有些蛊虫，你未必付得起代价。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade_training",
                "方源：\"修炼指导？我可以指点你一二。但记住——我的指导不是免费的，也不是仁慈的。我会告诉你真相，而真相往往是残酷的。\"")
                .AddOption("我愿意承受", "trade_training_accept")
                .AddOption("还是算了", "greeting");

            b.StartNode("trade_training_accept",
                "方源：\"好。第一条——永远不要相信任何人。第二条——永远为自己留一条后路。第三条——在生死关头，只有实力才是真理。这三条，足够你活很久了。\"")
                .AddOption("多谢指教", "greeting");

            b.StartNode("philosophy",
                "方源：\"哲学？你想和我讨论人生的意义？有趣。\"")
                .AddOption("生命的意义是什么？", "philosophy_meaning")
                .AddOption("善与恶的本质", "philosophy_good_evil")
                .AddOption("你的道是什么？", "philosophy_fangyuan_dao")
                .AddOption("自由意志是否存在？", "philosophy_freewill")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("philosophy_meaning",
                "方源：\"生命的意义？这个问题困扰了无数人。但我的答案很简单——生命的意义，就是追求永生。因为只有永生，才能超越一切有限的意义。\"")
                .AddOption("如果追求本身没有终点呢？", "philosophy_infinite_pursuit")
                .AddOption("其他的意义呢？", "philosophy_other_meaning")
                .AddOption("回到哲学讨论", "philosophy");

            b.StartNode("philosophy_infinite_pursuit",
                "方源：\"没有终点？那又如何。追求的过程本身就是意义。我活了五百年，每一天都在追求永生。这五百年的每一刻，都是充实的。比起那些浑浑噩噩度过一生的人，我活得比他们真实一万倍。\"")
                .AddOption("继续", "philosophy");

            b.StartNode("philosophy_other_meaning",
                "方源：\"其他的意义？爱情、友情、亲情、荣耀、权力……这些都是暂时的。人死了，一切归零。只有永生，才能让意义永远延续。\"")
                .AddOption("但过程也很重要", "philosophy_process")
                .AddOption("回到哲学讨论", "philosophy");

            b.StartNode("philosophy_process",
                "方源：\"过程？你说得对，过程确实重要。但只有永生者才能真正享受过程，因为凡人的过程终将结束。而我——我要让这个过程永远持续下去。\"")
                .AddOption("继续", "philosophy");

            b.StartNode("philosophy_good_evil",
                "方源：\"善与恶？这是凡人发明的概念。在蛊师的世界里，没有善恶，只有强弱。所谓的'善'，不过是强者对弱者的施舍；所谓的'恶'，不过是弱者对强者的嫉妒。\"")
                .AddOption("那正义呢？", "philosophy_justice")
                .AddOption("回到哲学讨论", "philosophy");

            b.StartNode("philosophy_justice",
                "方源冷笑：\"正义？天庭以正义之名统治了数万年，但那不过是强者制定规则、弱者遵守规则的另一种说法。真正的正义只有一个——实力即正义。\"")
                .AddOption("继续", "philosophy");

            b.StartNode("philosophy_fangyuan_dao",
                "方源：\"我的道？炼道。万物皆可炼——蛊虫可炼，人心可炼，天地可炼，甚至命运本身，亦可炼之。炼天魔尊，这就是我的称号。\"")
                .AddOption("炼道的本质是什么？", "philosophy_refine_nature")
                .AddOption("回到哲学讨论", "philosophy");

            b.StartNode("philosophy_refine_nature",
                "方源：\"炼道的本质是——转化。将一切不属于自己的东西，转化为自己的力量。将一切障碍，转化为阶梯。将一切敌人，转化为养分。这就是炼道，这就是我方源的道。\"")
                .AddOption("令人敬畏", "greeting")
                .AddOption("令人恐惧", "greeting");

            b.StartNode("philosophy_freewill",
                "方源：\"自由意志？在宿命蛊存在的时候，没有。每个人的命运都被写好了。但现在——宿命已被我击碎。从今以后，每个人都拥有真正的自由意志。包括选择自己死法的自由。\"")
                .AddOption("你给了世界自由？", "philosophy_freedom_gift")
                .AddOption("回到哲学讨论", "philosophy");

            b.StartNode("philosophy_freedom_gift",
                "方源：\"给？不，我从不'给'任何人任何东西。我击碎宿命，是因为它阻碍了我追求永生。至于其他人获得了自由……那不过是副产品。\"")
                .AddOption("继续", "philosophy");

            b.StartNode("avatars",
                "方源：\"分身？你想了解我的分身？每个分身都是我，又都不是我。他们各自修炼不同的道，但最终——都服务于同一个目标。\"")
                .AddOption("何春秋——宙道分身", "avatar_hechunqiu")
                .AddOption("梦求真——梦道分身", "avatar_mengqiuzhen")
                .AddOption("吴帅——奴道分身", "avatar_wushuai")
                .AddOption("气海老祖——气道分身", "avatar_qihailaozu")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("avatar_hechunqiu",
                "方源：\"何春秋……那是我的本体制成的分身，修炼宙道。他掌握着春秋蝉的力量，可以操控时间的流转。在所有分身中，他与我最为接近。\"")
                .AddOption("春秋蝉的力量？", "past_fate_battle")
                .AddOption("回到分身", "avatars");

            b.StartNode("avatar_mengqiuzhen",
                "方源：\"梦求真……梦道分身，蛊世界第一个梦道蛊仙。他的能力是操控梦境，让敌人分不清现实与虚幻。梦蝶蛊在他手中，可以创造无数幻象。\"")
                .AddOption("梦道是什么？", "avatar_dream_dao")
                .AddOption("回到分身", "avatars");

            b.StartNode("avatar_dream_dao",
                "方源：\"梦道是蛊世界最神秘的道之一。在梦中，一切规则都可以被改写。梦道蛊师可以让人永远沉睡，也可以让梦境化为现实。\"")
                .AddOption("回到分身", "avatars");

            b.StartNode("avatar_wushuai",
                "方源：\"吴帅……奴道分身，龙人之躯。他掌控龙宫，可以号令万龙。如梦令在他手中，可以让任何生灵臣服。\"")
                .AddOption("龙宫是什么？", "avatar_dragon_palace")
                .AddOption("回到分身", "avatars");

            b.StartNode("avatar_dragon_palace",
                "方源：\"龙宫是东海最强大的势力之一，拥有无数龙族。吴帅以龙人之身入主龙宫，以奴道驾驭万龙——这就是我的手段。\"")
                .AddOption("回到分身", "avatars");

            b.StartNode("avatar_qihailaozu",
                "方源：\"气海老祖……气道分身，东海气海之主。他的气海无量，真元无穷无尽。一气大手爆，足以毁天灭地。\"")
                .AddOption("气海是什么？", "avatar_qi_sea")
                .AddOption("回到分身", "avatars");

            b.StartNode("avatar_qi_sea",
                "方源：\"气海是东海的一处奇地，蕴含无穷真元。气海老祖以气道修炼，将气海化为己用，真元永不枯竭。\"")
                .AddOption("回到分身", "avatars");

            b.StartNode("bye",
                "方源微微点头：\"去吧。记住——在这个世界上，只有永生是真实的。\"")
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

        public override bool CanChat()
        {
            return CurrentAttitude != GuAttitude.Hostile || NPC.life > NPC.lifeMax * 0.5f;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return 0f;
        }

        public override List<string> SetNPCNameList()
        {
            return new List<string> { "方源" };
        }

        public override void OnKill()
        {
            if (!_hasUsedSpringAutumnCicada)
            {
                Say("春秋蝉……我还会回来的……", Color.Gold);
            }
            base.OnKill();
        }
    }
}
