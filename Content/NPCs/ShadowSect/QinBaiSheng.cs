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

namespace VerminLordMod.Content.NPCs.ShadowSect
{
    [AutoloadHead]
    public class QinBaiSheng : GuMasterBase
    {
        private int _attackTimer;
        private int _stealTimer;
        private bool _hasUsedDaTongWind;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/ShadowSect/QinBaiSheng_Head";

        public override FactionID GetFaction() => FactionID.ShadowSect;
        public override GuRank GetRank() => GuRank.Zhuan7_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Reckless;

        public override string GuMasterDisplayName => "秦百胜";
        public override int GuMasterDamage => 350;
        public override int GuMasterLife => 48000;
        public override int GuMasterDefense => 140;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 800;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 30;
            NPCID.Sets.AttackAverageChance[Type] = 20;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;

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
            NPC.knockBackResist = 0.2f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(2, 0, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _stealTimer = 0;
            _hasUsedDaTongWind = false;
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
                ExecuteStealCombatAI();
            }

            DaTongWindCheck();
        }

        private void DaTongWindCheck()
        {
            if (!_hasUsedDaTongWind && NPC.life <= NPC.lifeMax * 0.2f)
            {
                _hasUsedDaTongWind = true;

                CombatText.NewText(NPC.getRect(), Color.Cyan, "大同风——！", true);

                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.Center, 20, 20, DustID.WaterCandle,
                        Main.rand.NextFloat(-10f, 10f),
                        Main.rand.NextFloat(-10f, 10f),
                        100, Color.Cyan, 3f);
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    var source = NPC.GetSource_FromAI();
                    int damage = NPC.damage;
                    for (int i = 0; i < 24; i++)
                    {
                        float angle = MathHelper.TwoPi / 24f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 14f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.WaterBolt, damage, 3f, Main.myPlayer);
                    }
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi / 12f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 8f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.CultistBossLightningOrb, damage / 2, 0f, Main.myPlayer);
                    }
                }

                int selfDamage = NPC.lifeMax / 4;
                NPC.life = Math.Max(NPC.life - selfDamage, 1);
                NPC.HealEffect(-selfDamage);

                Say("大同风——盗天真传，我已参悟！", Color.Cyan);
            }
        }

        private void ExecuteStealCombatAI()
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

            if (dist > 400f)
                NPC.velocity.X = dir * 3.5f;
            else if (dist > 150f)
                NPC.velocity.X = dir * 2.5f;
            else
                NPC.velocity.X = dir * 1.5f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -9f;

            _attackTimer++;
            _stealTimer++;

            if (_attackTimer >= 30)
            {
                _attackTimer = 0;
                ExecuteStealAttack(target);
            }

            if (_stealTimer >= 150)
            {
                _stealTimer = 0;
                StealBuff(target);
            }
        }

        private void ExecuteStealAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 5;

            int pattern = Main.rand.Next(4);
            switch (pattern)
            {
                case 0:
                    for (int i = -2; i <= 2; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.18f;
                        Vector2 velocity = angle.ToRotationVector2() * 10f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.DemonScythe, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 1:
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 6f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.WaterBolt, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 2:
                    int[] projTypes = { ProjectileID.CultistBossIceMist, ProjectileID.CultistBossLightningOrb, ProjectileID.Fireball };
                    int projType = projTypes[Main.rand.Next(projTypes.Length)];
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 12f,
                        projType, damage * 2, 2f, Main.myPlayer);
                    CombatText.NewText(NPC.getRect(), Color.DarkViolet, "盗取！", true);
                    break;
                case 3:
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = toTarget.ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
                        Vector2 velocity = angle.ToRotationVector2() * (7f + Main.rand.NextFloat(3f));
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.ShadowFlame, damage, 1f, Main.myPlayer);
                    }
                    break;
            }
        }

        private void StealBuff(Player target)
        {
            int stolenCount = 0;
            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                int buffType = target.buffType[i];
                if (buffType > 0 && !Main.debuff[buffType])
                {
                    target.DelBuff(i);
                    stolenCount++;
                    i--;
                    if (stolenCount >= 2) break;
                }
            }

            if (stolenCount > 0)
            {
                int healAmount = stolenCount * 300;
                NPC.life = Math.Min(NPC.life + healAmount, NPC.lifeMax);
                NPC.HealEffect(healAmount);

                if (Main.netMode != NetmodeID.Server)
                {
                    CombatText.NewText(target.Hitbox, Color.DarkViolet, $"盗取了{stolenCount}个增益！", false);
                    for (int i = 0; i < 10; i++)
                    {
                        Dust.NewDustDirect(target.position, target.width, target.height,
                            DustID.Shadowflame, 0, -3f, Scale: 1.5f);
                    }
                }
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => HostileDialogue(),
                GuAttitude.Wary => "你在防备我？聪明。但偷道面前，防备没有用。",
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => "能让我偷不走的东西……不多了。",
                GuAttitude.Contemptuous => "你有什么值得我偷的？",
                GuAttitude.Fearful => "……你的力量，让我不敢轻举妄动。",
                _ => "纵然身死，也要掀起大同风！"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "盗天真传，我已参悟！",
                "偷道——万物皆可盗！",
                "你的力量，你的蛊虫，我全都要！",
                "大同风——不惜一切代价！",
                "盗天魔尊的真传，不是谁都能继承的！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "秦百胜：\"盗天真传，我已参悟！虽然不如盗天魔尊本人，但七转的偷道，足以让天下人头疼。\"",
                "秦百胜：\"偷道，盗取一切之力。蛊虫、修为、运气……只要存在，就可以被偷。这就是偷道的可怕。\"",
                "秦百胜：\"大同风……是偷道的禁忌杀招。以自身为代价，释放毁灭性的风暴。用了之后，我可能活不了。但——那又如何？\"",
                "秦百胜：\"影宗给了我盗天真传，这是我的底牌。但方源……他连盗天真传都能算计到。那个人太可怕了。\"",
                "秦百胜：\"你问我怕不怕死？当然怕。但有些时候，孤注一掷比畏缩不前更有价值。这就是我的道——赌！\"",
                "秦百胜：\"偷道和赌道，看似不同，实则相通。每一次偷窃都是一场赌博，每一次赌博都是一次偷窃。赢者通吃，输者一无所有。\"",
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
            var b = new DialogueTreeBuilder("ShadowSect_QinBaiSheng", "greeting");

            b.StartNode("greeting",
                "秦百胜目光锐利，浑身散发着一种孤注一掷的气势。")
                .AddOption("询问偷道", "ask_steal_dao", DialogueOptionType.Informative)
                .AddOption("关于盗天真传", "ask_dao_tian", DialogueOptionType.Informative)
                .AddOption("关于大同风", "ask_datong_wind", DialogueOptionType.Risky)
                .AddOption("关于影宗", "ask_shadow_sect", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_steal_dao",
                "秦百胜：\"偷道，盗取一切之力。蛊虫可以偷，修为可以偷，运气可以偷，甚至——寿命也可以偷。只要存在，就可以被偷。\"")
                .AddOption("偷道有什么限制？", "ask_steal_limit")
                .AddOption("偷道和盗天魔尊的区别？", "ask_vs_daotian")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_steal_limit",
                "秦百胜：\"限制？当然有。偷道需要消耗真元，偷的东西越珍贵，消耗越大。而且——偷来的东西不一定是永久的。有些东西，偷来之后会慢慢流失。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_vs_daotian",
                "秦百胜：\"盗天魔尊？……他是偷道的开创者，九转尊者。我只是继承了他的真传，但实力远不及他。他可以偷天换日，我只能偷些小东西。但——总有一天，我会达到他的高度！\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_dao_tian",
                "秦百胜的眼中闪过狂热：\"盗天真传！这是盗天魔尊留下的至高传承。我秦百胜，是盗天真传的持有者！虽然只有七转修为，但有了真传，我的偷道威力远超同阶！\"")
                .AddOption("真传是怎么得到的？", "ask_get_inheritance")
                .AddOption("真传有什么能力？", "ask_inheritance_power")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_get_inheritance",
                "秦百胜：\"怎么得到的？……影宗给我的。影宗拥有无数真传，分发给成员使用。我得到了盗天真传，代价是——永远忠于影宗。\"")
                .AddOption("你甘心吗？", "ask_willing")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_willing",
                "秦百胜大笑：\"甘心？当然甘心！盗天真传给了我力量，影宗给了我方向。在这个弱肉强食的世界里，有人给你力量和方向，你还有什么不甘心的？\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_inheritance_power",
                "秦百胜：\"盗天真传的能力？太多了。偷取蛊虫、偷取修为、偷取运气……最厉害的是——大同风。那是盗天魔尊的绝杀，以自身为代价，释放毁天灭地的风暴。\"")
                .AddOption("大同风？", "ask_datong_wind")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_datong_wind",
                "秦百胜的表情变得严肃：\"大同风——盗天魔尊的终极杀招。以自身全部真元为代价，释放一道足以毁灭一切的风暴。用完之后，施术者必死无疑。\"")
                .AddOption("你会用吗？", "ask_will_use")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_will_use",
                "秦百胜：\"会不会用？……如果到了万不得已的时候，我会用。孤注一掷，这就是我的道。与其苟活，不如轰轰烈烈地燃烧殆尽！\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_shadow_sect",
                "秦百胜：\"影宗？影宗给了我一切——盗天真传、修炼资源、战斗伙伴。没有影宗，我秦百胜不过是一个普通的七转蛊师。有了影宗，我才能站在这里。\"")
                .AddOption("影宗的目标是什么？", "ask_sect_goal")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_sect_goal",
                "秦百胜：\"影宗的目标？击碎宿命蛊。这个目标已经实现了——红莲魔尊做到了。但新的目标是……在混乱中建立新的秩序。至于这个秩序是什么样的，那是上面的人操心的事。我只需要执行。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "秦百胜：\"交易？我可以偷一些你需要的东西……当然，需要等价交换。偷道不是白用的。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "秦百胜挥了挥手：\"去吧。记住——孤注一掷，有时候比稳扎稳打更有效。\"")
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
            return new List<string> { "秦百胜" };
        }
    }
}
