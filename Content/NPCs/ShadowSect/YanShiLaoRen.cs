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
    public class YanShiLaoRen : GuMasterBase
    {
        private int _attackTimer;
        private int _formationTimer;
        private bool _trapArrayActive;
        private int _trapArrayTimer;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/ShadowSect/YanShiLaoRen_Head";

        public override FactionID GetFaction() => FactionID.ShadowSect;
        public override GuRank GetRank() => GuRank.Zhuan7_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Calculating;

        public override string GuMasterDisplayName => "砚石老人";
        public override int GuMasterDamage => 300;
        public override int GuMasterLife => 42000;
        public override int GuMasterDefense => 160;

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
            NPC.knockBackResist = 0.15f;
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
            _formationTimer = 0;
            _trapArrayActive = false;
            _trapArrayTimer = 0;
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
                ExecuteFormationCombatAI();
            }

            TrapArrayUpdate();
        }

        private void TrapArrayUpdate()
        {
            if (_trapArrayActive)
            {
                _trapArrayTimer++;
                if (_trapArrayTimer > 360)
                {
                    _trapArrayActive = false;
                    _trapArrayTimer = 0;
                    CombatText.NewText(NPC.getRect(), Color.Orange, "阵法消散", false);
                }
                else
                {
                    var target = Main.player[NPC.target];
                    if (target.active && !target.dead)
                    {
                        float dist = Vector2.Distance(NPC.Center, target.Center);
                        if (dist < 400f)
                        {
                            target.AddBuff(BuffID.Slow, 30);
                            target.AddBuff(BuffID.Webbed, 30);
                        }
                    }
                }
            }
        }

        private void ExecuteFormationCombatAI()
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
                NPC.velocity.X = dir * 2.5f;
            else if (dist > 150f)
                NPC.velocity.X = dir * 1.5f;
            else
                NPC.velocity.X = dir * 0.8f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -7f;

            _attackTimer++;
            _formationTimer++;

            if (_attackTimer >= 40)
            {
                _attackTimer = 0;
                ExecuteFormationAttack(target);
            }

            if (_formationTimer >= 200)
            {
                _formationTimer = 0;
                DeployTrapArray(target);
            }
        }

        private void ExecuteFormationAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 5;

            int pattern = Main.rand.Next(4);
            switch (pattern)
            {
                case 0:
                    CombatText.NewText(NPC.getRect(), Color.Orange, "阵法——困！", true);
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 pos = target.Center + angle.ToRotationVector2() * 150f;
                        Vector2 velocity = (target.Center - pos).SafeNormalize(Vector2.UnitY) * 5f;
                        Projectile.NewProjectile(source, pos, velocity,
                            ProjectileID.WaterBolt, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 1:
                    for (int i = -2; i <= 2; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.2f;
                        Vector2 velocity = angle.ToRotationVector2() * 8f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.DemonScythe, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 2:
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi / 6f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 5f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.CultistBossIceMist, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 3:
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 10f,
                        ProjectileID.CultistBossLightningOrb, damage * 2, 2f, Main.myPlayer);
                    target.AddBuff(BuffID.Slow, 120);
                    break;
            }
        }

        private void DeployTrapArray(Player target)
        {
            _trapArrayActive = true;
            _trapArrayTimer = 0;

            CombatText.NewText(NPC.getRect(), Color.Orange, "阵法——困敌之阵！", true);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                int damage = NPC.damage / 6;
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi / 4f * i;
                    Vector2 pos = target.Center + angle.ToRotationVector2() * 200f;
                    Projectile.NewProjectile(source, pos, Vector2.Zero,
                        ProjectileID.WaterBolt, damage, 1f, Main.myPlayer);
                }
            }

            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi / 20f * i;
                Dust.NewDust(NPC.Center + angle.ToRotationVector2() * 200f, 10, 10,
                    DustID.OrangeTorch, 0, -2f, 100, Color.Orange, 1.5f);
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => HostileDialogue(),
                GuAttitude.Wary => "你在观察我？有趣。但阵法已经布好，你走不了了。",
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => "能看穿阵法之人……值得认真对待。",
                GuAttitude.Contemptuous => "棋子就该有棋子的觉悟，不要试图越界。",
                GuAttitude.Fearful => "……你的实力超出了我的计算。需要重新布局。",
                _ => "棋子就该有棋子的觉悟。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "棋子就该有棋子的觉悟！",
                "阵法已成，你无处可逃！",
                "你以为你在和我战斗？你只是在阵法中挣扎！",
                "算无遗策——这就是阵道的可怕！",
                "每一步都在我的计算之中！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "砚石老人：\"棋子就该有棋子的觉悟。这不是侮辱，这是事实。在蛊世界，每个人都是别人的棋子。\"",
                "砚石老人：\"阵道，讲究的是布局。好的阵法，不需要强大的攻击力，只需要让敌人走进你设好的陷阱。\"",
                "砚石老人：\"白凝冰……她曾经是我手中最完美的棋子。可惜，方源把她夺走了。\"",
                "砚石老人：\"阵法的精髓在于——以弱胜强。只要阵法布置得当，七转也能困住八转。这就是阵道的价值。\"",
                "砚石老人：\"影宗的每一步行动，都经过精密的计算。我们不是在赌博，我们是在下棋。而天庭……还不知道自己已经输了。\"",
                "砚石老人：\"你问我会不会算错？当然会。但阵道修心，就是要接受计算的错误，并随时调整布局。\"",
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
            var b = new DialogueTreeBuilder("ShadowSect_YanShiLaoRen", "greeting");

            b.StartNode("greeting",
                "砚石老人目光如棋盘般深邃，每一个眼神都仿佛在计算着什么。")
                .AddOption("询问阵道", "ask_formation_dao", DialogueOptionType.Informative)
                .AddOption("关于白凝冰", "ask_bai_ningbing", DialogueOptionType.Informative)
                .AddOption("关于影宗的布局", "ask_shadow_plan", DialogueOptionType.Risky)
                .AddOption("关于棋局", "ask_chess", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_formation_dao",
                "砚石老人：\"阵道，以天地为棋盘，以万物为棋子。好的阵法，可以让弱者胜强，让少数胜多数。这就是阵道的精髓——以布局取胜。\"")
                .AddOption("阵法有哪些类型？", "ask_formation_types")
                .AddOption("困敌之阵的原理？", "ask_trap_formation")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_formation_types",
                "砚石老人：\"阵法分为攻阵、守阵、困阵、幻阵四大类。攻阵主杀伐，守阵主防御，困阵主控制，幻阵主迷惑。我擅长困阵——让敌人进得来，出不去。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_trap_formation",
                "砚石老人：\"困敌之阵——以阵法之力封锁一片区域，让敌人无法逃脱。阵法越精密，困敌效果越强。七转的困阵，足以困住八转半个时辰。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_bai_ningbing",
                "砚石老人的眼神闪过一丝遗憾：\"白凝冰……她曾经是我手中最完美的棋子。冰道天才，性格单纯，易于操控。但方源……他把她从我手中夺走了。\"")
                .AddOption("你是怎么控制她的？", "ask_control_bnb")
                .AddOption("方源是怎么夺走她的？", "ask_fangyuan_bnb")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_control_bnb",
                "砚石老人：\"控制？不，我更愿意称之为——引导。白凝冰的体质特殊，需要特殊的蛊虫维持。我提供了她需要的一切，而她……为我所用。这是交易，不是控制。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_fangyuan_bnb",
                "砚石老人：\"方源……他看穿了我的布局。他用更高级的蛊虫替代了我提供的蛊虫，让白凝冰不再依赖我。这是我一生中最大的失误——低估了方源。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_shadow_plan",
                "砚石老人微微眯眼：\"影宗的布局？你确定你想知道？……好吧。影宗的目标只有一个——击碎宿命蛊。为此，我们潜伏了数万年，在天庭、在南疆、在北原，到处都有我们的棋子。\"")
                .AddOption("宿命蛊已经被击碎了吧？", "ask_fate_broken")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_fate_broken",
                "砚石老人：\"是的，宿命蛊已经被红莲击碎了。但这不代表影宗的使命结束了。宿命破碎后的混乱，需要有人来收拾。而影宗……将在混乱中建立新的秩序。\"")
                .AddOption("新的秩序？", "ask_new_order")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_new_order",
                "砚石老人：\"新的秩序……不是由天庭主导的秩序，而是由影宗主导的秩序。天庭的秩序建立在宿命之上，注定不公。而影宗的秩序……建立在自由意志之上。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_chess",
                "砚石老人：\"棋局？蛊世界就是一盘大棋。天庭是执白者，影宗是执黑者。而其他人……都是棋子。包括你。但有趣的是——有时候，棋子也能改变棋局。\"")
                .AddOption("我是棋子还是棋手？", "ask_chess_role")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_chess_role",
                "砚石老人：\"你？……现在还是棋子。但如果你足够聪明，也许有一天，你能成为棋手。不过——那需要付出巨大的代价。你准备好了吗？\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "砚石老人：\"交易？可以。但我只交换有价值的东西——情报、蛊虫、或者……棋子的位置。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "砚石老人微微点头：\"去吧。记住——每一步都是棋局，每一步都要算计。\"")
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
            return new List<string> { "砚石老人" };
        }
    }
}
