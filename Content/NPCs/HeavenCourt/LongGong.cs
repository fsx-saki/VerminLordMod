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

namespace VerminLordMod.Content.NPCs.HeavenCourt
{
    public class LongGong : GuMasterBase
    {
        private int _attackTimer;
        private int _dragonFormTimer;
        private bool _inDragonForm;
        private bool _hasUsedDragonExtinction;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/HeavenCourt/LongGong_Head";

        public override FactionID GetFaction() => FactionID.Heaven;
        public override GuRank GetRank() => GuRank.Zhuan8_DianFeng;
        public override GuPersonality GetPersonality() => GuPersonality.Righteous;

        public override string GuMasterDisplayName => "龙公";
        public override int GuMasterDamage => 450;
        public override int GuMasterLife => 85000;
        public override int GuMasterDefense => 220;

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
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Slow] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Weak] = true;

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
            _dragonFormTimer = 0;
            _inDragonForm = false;
            _hasUsedDragonExtinction = false;
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
                ExecuteDragonCombatAI();
            }

            DragonFormUpdate();
            DragonExtinctionCheck();
        }

        private void DragonFormUpdate()
        {
            if (_inDragonForm)
            {
                _dragonFormTimer++;
                NPC.defense = GuMasterDefense + 100;

                if (_dragonFormTimer > 600)
                {
                    _inDragonForm = false;
                    _dragonFormTimer = 0;
                    NPC.defense = GuMasterDefense;
                    CombatText.NewText(NPC.getRect(), Color.Silver, "龙形解除", false);
                }
            }
        }

        private void DragonExtinctionCheck()
        {
            if (!_hasUsedDragonExtinction && NPC.life <= NPC.lifeMax * 0.15f)
            {
                _hasUsedDragonExtinction = true;
                _inDragonForm = true;
                _dragonFormTimer = 0;

                CombatText.NewText(NPC.getRect(), Color.Gold, "龙人寂灭！", true);

                for (int i = 0; i < 50; i++)
                {
                    Dust.NewDust(NPC.Center, 20, 20, DustID.GoldFlame,
                        Main.rand.NextFloat(-8f, 8f),
                        Main.rand.NextFloat(-8f, 8f),
                        100, Color.Gold, 3f);
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    var source = NPC.GetSource_FromAI();
                    int damage = NPC.damage / 2;
                    for (int i = 0; i < 24; i++)
                    {
                        float angle = MathHelper.TwoPi / 24f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 12f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.Fireball, damage, 3f, Main.myPlayer);
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 6f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.CultistBossLightningOrb, damage, 0f, Main.myPlayer);
                    }
                }

                Say("龙人寂灭——天庭之怒！", Color.Gold);
            }
        }

        private void ExecuteDragonCombatAI()
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

            float speed = _inDragonForm ? 5f : 3f;
            if (dist > 500f)
                NPC.velocity.X = dir * speed;
            else if (dist > 150f)
                NPC.velocity.X = dir * speed * 0.7f;
            else
                NPC.velocity.X = dir * speed * 0.4f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = _inDragonForm ? -12f : -8f;

            _attackTimer++;
            int attackInterval = _inDragonForm ? 20 : 40;
            if (_attackTimer >= attackInterval)
            {
                _attackTimer = 0;
                ExecuteDragonAttack(target);
            }
        }

        private void ExecuteDragonAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 5;

            if (_inDragonForm)
            {
                int pattern = Main.rand.Next(3);
                switch (pattern)
                {
                    case 0:
                        for (int i = -3; i <= 3; i++)
                        {
                            float angle = toTarget.ToRotation() + i * 0.15f;
                            Vector2 velocity = angle.ToRotationVector2() * 11f;
                            Projectile.NewProjectile(source, NPC.Center, velocity,
                                ProjectileID.Fireball, damage, 2f, Main.myPlayer);
                        }
                        break;
                    case 1:
                        for (int i = 0; i < 12; i++)
                        {
                            float angle = MathHelper.TwoPi / 12f * i;
                            Vector2 velocity = angle.ToRotationVector2() * 8f;
                            Projectile.NewProjectile(source, NPC.Center, velocity,
                                ProjectileID.CultistBossLightningOrb, damage, 0f, Main.myPlayer);
                        }
                        break;
                    case 2:
                        Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 14f,
                            ProjectileID.Meteor1, damage * 2, 4f, Main.myPlayer);
                        break;
                }
            }
            else
            {
                int pattern = Main.rand.Next(3);
                switch (pattern)
                {
                    case 0:
                        if (NPC.life < NPC.lifeMax * 0.5f)
                        {
                            _inDragonForm = true;
                            _dragonFormTimer = 0;
                            CombatText.NewText(NPC.getRect(), Color.Gold, "龙化！", true);
                            for (int i = 0; i < 20; i++)
                            {
                                Dust.NewDust(NPC.Center, 20, 20, DustID.GoldFlame,
                                    Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f),
                                    100, Color.Gold, 2f);
                            }
                        }
                        else
                        {
                            for (int i = -1; i <= 1; i++)
                            {
                                float angle = toTarget.ToRotation() + i * 0.2f;
                                Vector2 velocity = angle.ToRotationVector2() * 9f;
                                Projectile.NewProjectile(source, NPC.Center, velocity,
                                    ProjectileID.Fireball, damage, 1f, Main.myPlayer);
                            }
                        }
                        break;
                    case 1:
                        for (int i = 0; i < 6; i++)
                        {
                            float angle = MathHelper.TwoPi / 6f * i;
                            Vector2 velocity = angle.ToRotationVector2() * 6f;
                            Projectile.NewProjectile(source, NPC.Center, velocity,
                                ProjectileID.WaterBolt, damage, 1f, Main.myPlayer);
                        }
                        break;
                    case 2:
                        Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 10f,
                            ProjectileID.CultistBossIceMist, damage, 0f, Main.myPlayer);
                        break;
                }
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => HostileDialogue(),
                GuAttitude.Wary => "天庭之规，不可逾越。你若执意试探，后果自负。",
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => "你的实力值得尊重，但天庭的秩序不可动摇。",
                GuAttitude.Contemptuous => "蝼蚁也敢仰望天庭？",
                GuAttitude.Fearful => "恐惧？不……我只是不愿滥杀无辜。",
                _ => "天庭之规，不可逾越。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "天庭之规，不可逾越！",
                "你竟敢挑战天庭三公？",
                "龙族的创造者，岂是你能撼动的？",
                "洪亭……你为何要背叛？",
                "变化之道，在于万变不离其宗！",
                "我龙公一生，从未退缩！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "龙公：\"天庭历经万载，靠的不是蛮力，而是秩序。\"",
                "龙公：\"我创造了龙族，却无法阻止他们的命运。这是天道，非人力可改。\"",
                "龙公：\"红莲曾是我的学生……但他选择了毁灭宿命。我不后悔教导他，只遗憾他走错了路。\"",
                "龙公：\"气道与变化道，看似不同，实则相通。气为万物之始，变为万物之终。\"",
                "龙公：\"天庭三公，各司其职。我主变化，铜公主金，眉公主魅。三道合一，天庭永固。\"",
                "龙公：\"你若真心向善，天庭不会亏待你。但若心怀不轨……\"",
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
            var b = new DialogueTreeBuilder("HeavenCourt_LongGong", "greeting");

            b.StartNode("greeting",
                "龙公身姿挺拔，周身隐隐有龙气环绕，目光如炬。")
                .AddOption("询问气道与变化道", "ask_dao", DialogueOptionType.Informative)
                .AddOption("关于龙族", "ask_dragon_race", DialogueOptionType.Informative)
                .AddOption("关于红莲魔尊", "ask_honglian", DialogueOptionType.Informative)
                .AddOption("关于天庭三公", "ask_three_dukes", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_dao",
                "龙公：\"气道为万物之源，变化道为万法之归。我修两道，以气化龙，以变应万。这是天庭赋予我的使命。\"")
                .AddOption("龙化是什么感觉？", "ask_dragon_transform")
                .AddOption("气道与变化道如何兼修？", "ask_dual_dao")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_dragon_transform",
                "龙公：\"龙化……是我变化道的极致应用。化身龙形，力量暴增，但也会失去理智的边缘。每一次龙化，都是在与本能搏斗。\"")
                .AddOption("龙人寂灭呢？", "ask_dragon_extinction")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_dragon_extinction",
                "龙公沉默片刻：\"龙人寂灭……是我最后的手段。以龙族之力，化为毁灭之光。使用之后，龙族便会走向衰亡。这是我最大的罪孽。\"")
                .AddOption("你后悔创造龙族吗？", "ask_dragon_regret")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_dragon_regret",
                "龙公：\"后悔？……不。龙族的存在有其意义。但龙人寂灭的代价，我永远背负。这就是天庭三公的责任——为秩序付出一切。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_dual_dao",
                "龙公：\"兼修两道？天底下能做到的人寥寥无几。关键在于——找到两道之间的共通之处。气道讲'化'，变化道讲'变'，化变合一，便是我的道。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_dragon_race",
                "龙公：\"龙族……是我一生最大的创造，也是最大的遗憾。我以变化道创造了龙人，让他们拥有龙的力量。但龙人的命运，却早已被宿命写好。\"")
                .AddOption("龙人的命运是什么？", "ask_dragon_fate")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_dragon_fate",
                "龙公：\"龙人注定为奴。这是宿命蛊的安排。我创造了他们，却无法改变他们的命运。直到……红莲击碎了宿命。\"")
                .AddOption("红莲击碎宿命后呢？", "ask_honglian")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_honglian",
                "龙公的表情变得复杂：\"红莲……他曾经是我最得意的学生。我教他气道，教他变化道，希望他成为天庭的栋梁。但他选择了另一条路——摧毁宿命。\"")
                .AddOption("你恨他吗？", "ask_hate_honglian")
                .AddOption("他为什么要摧毁宿命？", "ask_honglian_reason")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_hate_honglian",
                "龙公：\"恨？……不。我理解他。他的爱人因宿命而死，他恨宿命，恨天庭，恨这个不公的世界。但理解不代表认同。天庭的秩序，必须有人守护。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_honglian_reason",
                "龙公：\"红莲的爱人，是被宿命蛊安排去死的。他亲眼看着她走向注定的毁灭，却无能为力。这种痛苦……我无法想象。但摧毁宿命的代价，是整个世界的混乱。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_three_dukes",
                "龙公：\"天庭三公——龙公、铜公、眉公。我主变化，铜公主金，眉公主魅。我们三人辅佐天庭之主，维护蛊世界的秩序。这是万载不变的格局。\"")
                .AddOption("三公之间有分歧吗？", "ask_duke_conflict")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_duke_conflict",
                "龙公：\"分歧？当然有。铜公主张铁腕镇压，眉公主张以柔克刚，而我……我主张在秩序与自由之间寻找平衡。但无论分歧多大，我们三公始终忠于天庭。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "龙公：\"交易？天庭的资源，可以与你分享。但记住——一切都有代价。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "龙公微微颔首：\"去吧。记住——天庭的秩序，不可动摇。\"")
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
            return new List<string> { "龙公" };
        }
    }
}
