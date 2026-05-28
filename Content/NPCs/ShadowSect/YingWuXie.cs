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
    public class YingWuXie : GuMasterBase
    {
        private int _attackTimer;
        private int _shadowCloneTimer;
        private int _soulDrainTimer;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/ShadowSect/YingWuXie_Head";

        public override FactionID GetFaction() => FactionID.ShadowSect;
        public override GuRank GetRank() => GuRank.Zhuan7_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Cunning;

        public override string GuMasterDisplayName => "影无邪";
        public override int GuMasterDamage => 340;
        public override int GuMasterLife => 45000;
        public override int GuMasterDefense => 130;

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
            _shadowCloneTimer = 0;
            _soulDrainTimer = 0;
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
                ExecuteShadowCombatAI();
            }
        }

        private void ExecuteShadowCombatAI()
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
                NPC.velocity.X = dir * 2f;
            else
                NPC.velocity.X = dir * 1.2f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -8f;

            _attackTimer++;
            _shadowCloneTimer++;
            _soulDrainTimer++;

            if (_attackTimer >= 35)
            {
                _attackTimer = 0;
                ExecuteShadowAttack(target);
            }

            if (_shadowCloneTimer >= 180)
            {
                _shadowCloneTimer = 0;
                SpawnShadowClones(target);
            }

            if (_soulDrainTimer >= 120)
            {
                _soulDrainTimer = 0;
                SoulDrain(target);
            }
        }

        private void ExecuteShadowAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 5;

            int pattern = Main.rand.Next(3);
            switch (pattern)
            {
                case 0:
                    for (int i = -2; i <= 2; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.2f;
                        Vector2 velocity = angle.ToRotationVector2() * 9f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.ShadowFlame, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 1:
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 6f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.ShadowBeamHostile, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 2:
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 11f,
                        ProjectileID.ShadowFlame, damage * 2, 2f, Main.myPlayer);
                    break;
            }
        }

        private void SpawnShadowClones(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            var source = NPC.GetSource_FromAI();
            int damage = NPC.damage / 6;

            CombatText.NewText(NPC.getRect(), Color.DarkViolet, "影分身！", true);

            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi / 3f * i + target.Center.X - NPC.Center.X > 0 ? 0 : (float)Math.PI;
                Vector2 spawnOffset = angle.ToRotationVector2() * 150f;
                Vector2 velocity = (target.Center - (NPC.Center + spawnOffset)).SafeNormalize(Vector2.UnitY) * 7f;
                Projectile.NewProjectile(source, NPC.Center + spawnOffset, velocity,
                    ProjectileID.ShadowFlame, damage, 1f, Main.myPlayer);
            }

            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.Shadowflame,
                    Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f),
                    100, Color.DarkViolet, 1.5f);
            }
        }

        private void SoulDrain(Player target)
        {
            float dist = Vector2.Distance(NPC.Center, target.Center);
            if (dist < 500f)
            {
                int drainAmount = target.statLifeMax2 / 20;
                target.statLife = Math.Max(target.statLife - drainAmount, 1);
                target.HealEffect(-drainAmount);

                int healAmount = drainAmount / 2;
                NPC.life = Math.Min(NPC.life + healAmount, NPC.lifeMax);
                NPC.HealEffect(healAmount);

                if (Main.netMode != NetmodeID.Server)
                {
                    CombatText.NewText(target.Hitbox, Color.DarkViolet, "魂吸！", true);
                    for (int i = 0; i < 8; i++)
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
                GuAttitude.Wary => "你在窥探影宗？小心，影子也在窥探你。",
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => "能看穿影宗布局之人……不简单。",
                GuAttitude.Contemptuous => "你以为你看到了真相？那不过是我想让你看到的。",
                GuAttitude.Fearful => "……有意思，你比我想象的要强。",
                _ => "影宗的棋局，你不过是一颗棋子。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "影宗的布局，你永远看不透！",
                "你以为你在和我战斗？你只是在和影子搏斗！",
                "魂道之下，你的灵魂无处可藏！",
                "影分身——你永远不知道哪个是真的！",
                "幽魂魔尊的意志，由我继承！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "影无邪：\"影宗的布局，你永远看不透。但既然你站在我面前……也许，你可以成为棋盘上的一枚棋子。\"",
                "影无邪：\"魂道……是操控灵魂的力量。灵魂是一切存在的根本，掌握了灵魂，就掌握了一切。\"",
                "影无邪：\"我是幽魂魔尊的分魂，继承了主人的意志。影宗的目标，从未改变——击碎宿命。\"",
                "影无邪：\"纯梦求真体……那曾经是我的身体。但现在，它属于另一个人了。这就是影宗的牺牲。\"",
                "影无邪：\"影分身不只是战斗手段，更是情报网络。每一个影子，都是我的耳目。\"",
                "影无邪：\"天庭以为他们掌控了一切，但他们不知道——影宗的暗子，早已渗透到天庭的每一个角落。\"",
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
            var b = new DialogueTreeBuilder("ShadowSect_YingWuXie", "greeting");

            b.StartNode("greeting",
                "影无邪的身影若隐若现，仿佛随时会融入黑暗之中。")
                .AddOption("询问魂道", "ask_soul_dao", DialogueOptionType.Informative)
                .AddOption("关于影宗", "ask_shadow_sect", DialogueOptionType.Informative)
                .AddOption("关于幽魂魔尊", "ask_youhun", DialogueOptionType.Informative)
                .AddOption("关于纯梦求真体", "ask_dream_body", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_soul_dao",
                "影无邪：\"魂道，操控灵魂之力。灵魂是万物之本，肉身不过是容器。掌握了魂道，就掌握了生死的钥匙。\"")
                .AddOption("魂吸是什么？", "ask_soul_drain")
                .AddOption("影分身的原理？", "ask_shadow_clone")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_soul_drain",
                "影无邪：\"魂吸——抽取他人的灵魂之力，化为己用。这是魂道最基础也是最阴损的手段。被魂吸之人，会感到灵魂被撕裂的痛苦。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_shadow_clone",
                "影无邪：\"影分身——以魂道真元凝聚影子，制造出能模仿攻击的分身。它们没有实体，但攻击是真实的。你永远不知道，下一击来自哪个影子。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_shadow_sect",
                "影无邪：\"影宗……是幽魂魔尊留下的遗产。我们的目标只有一个——击碎宿命蛊，解放所有人的命运。为此，我们潜伏了数万年。\"")
                .AddOption("影宗有多少人？", "ask_sect_members")
                .AddOption("影宗的渗透有多深？", "ask_infiltration")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_sect_members",
                "影无邪：\"多少人？……不多。但每一个都是幽魂魔尊的分魂，每一个都潜伏在不同的势力中。你以为天庭是铁板一块？不，天庭里到处都是我们的影子。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_infiltration",
                "影无邪冷笑：\"渗透有多深？……你见过正元老人吗？天庭的人道大师，深受信任。但他——是影宗的人。这就是影宗的力量。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_youhun",
                "影无邪：\"幽魂魔尊……我们的主人，影宗的缔造者。他将自己的灵魂分裂成无数碎片，每一片都寄宿在不同的身体中，潜伏在蛊世界的每一个角落。\"")
                .AddOption("灵魂分裂有什么代价？", "ask_soul_split_cost")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_soul_split_cost",
                "影无邪：\"代价？……每一片分魂都是独立的个体，有自己的思想、自己的情感。有时候，连我自己都分不清——我是幽魂魔尊，还是影无邪？\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_dream_body",
                "影无邪的表情变得复杂：\"纯梦求真体……那曾经是我的身体。梦道仙蛊'梦蝶'寄宿其中，可以操控梦境。但现在……它被方源夺走了。\"")
                .AddOption("方源是怎么夺走的？", "ask_fangyuan_stole")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_fangyuan_stole",
                "影无邪：\"方源……他利用了影宗的布局，反过来算计了我们。纯梦求真体落入他手中，成为了他梦道分身'梦求真'的载体。这是我最大的失败。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "影无邪：\"交易？影宗有各种……不为人知的资源。只要你付得起代价。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "影无邪的身影开始消散：\"去吧。记住——影子无处不在。\"")
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
            return new List<string> { "影无邪" };
        }
    }
}
