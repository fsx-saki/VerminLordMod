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
    [AutoloadHead]
    public class QinDingLing : GuMasterBase
    {
        private int _attackTimer;
        private int _fortuneZoneTimer;
        private bool _fortuneZoneActive;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/HeavenCourt/QinDingLing_Head";

        public override FactionID GetFaction() => FactionID.Heaven;
        public override GuRank GetRank() => GuRank.Zhuan8_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Cunning;

        public override string GuMasterDisplayName => "秦鼎菱";
        public override int GuMasterDamage => 350;
        public override int GuMasterLife => 65000;
        public override int GuMasterDefense => 200;

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
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Slow] = true;

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
            NPC.knockBackResist = 0.1f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(4, 0, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _fortuneZoneTimer = 0;
            _fortuneZoneActive = false;
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
                ExecuteFortuneCombatAI();
            }

            FortuneZoneUpdate();
        }

        private void FortuneZoneUpdate()
        {
            if (_fortuneZoneActive)
            {
                _fortuneZoneTimer++;
                if (_fortuneZoneTimer > 300)
                {
                    _fortuneZoneActive = false;
                    _fortuneZoneTimer = 0;
                    CombatText.NewText(NPC.getRect(), Color.Pink, "运道领域消散", false);
                }
                else
                {
                    var target = Main.player[NPC.target];
                    if (target.active && !target.dead)
                    {
                        float dist = Vector2.Distance(NPC.Center, target.Center);
                        if (dist < 400f)
                        {
                            target.AddBuff(BuffID.Cursed, 60);
                        }
                    }

                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        var other = Main.npc[i];
                        if (other.active && other.ModNPC is GuMasterBase ally && ally.GetFaction() == FactionID.Heaven)
                        {
                            if (Vector2.Distance(NPC.Center, other.Center) < 400f)
                            {
                                other.AddBuff(BuffID.Lucky, 30);
                            }
                        }
                    }
                }
            }
        }

        private void ExecuteFortuneCombatAI()
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

            if (dist > 500f)
                NPC.velocity.X = dir * 2.5f;
            else if (dist > 200f)
                NPC.velocity.X = dir * 1.5f;
            else
                NPC.velocity.X = dir * 0.8f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -7f;

            _attackTimer++;
            if (_attackTimer >= 50)
            {
                _attackTimer = 0;
                ExecuteFortuneAttack(target);
            }
        }

        private void ExecuteFortuneAttack(Player target)
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
                        Vector2 velocity = angle.ToRotationVector2() * 8f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.WaterBolt, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 1:
                    _fortuneZoneActive = true;
                    _fortuneZoneTimer = 0;
                    CombatText.NewText(NPC.getRect(), Color.Pink, "运道领域——厄运降临！", true);
                    for (int i = 0; i < 20; i++)
                    {
                        float angle = MathHelper.TwoPi / 20f * i;
                        Dust.NewDust(NPC.Center + angle.ToRotationVector2() * 200f, 10, 10,
                            DustID.RainbowTorch, 0, -2f, 100, Color.Pink, 1.5f);
                    }
                    break;
                case 2:
                    target.AddBuff(BuffID.Cursed, 120);
                    target.AddBuff(BuffID.Darkness, 180);
                    CombatText.NewText(target.Hitbox, Color.DarkRed, "运势逆转！", true);
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 10f,
                        ProjectileID.CultistBossIceMist, damage, 0f, Main.myPlayer);
                    break;
                case 3:
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 5f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.WaterBolt, damage, 1f, Main.myPlayer);
                    }
                    break;
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => HostileDialogue(),
                GuAttitude.Wary => "你的运势……让我看到了变数。有趣。",
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => "能被运道认可的人，必有非凡之处。",
                GuAttitude.Contemptuous => "运势低微之人，不值一提。",
                GuAttitude.Fearful => "……你的运势中，我看到了天庭的阴影。",
                _ => "天庭的秩序，由我来守护。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "天庭虽遭重创，但绝不会倒下！",
                "你以为命运站在你那边？天真。",
                "运道在我手中，你的命运由我书写！",
                "宿命虽碎，天庭的气运犹在！",
                "逆天而行者，终将自食其果。",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "秦鼎菱：\"运道……是世间最玄妙的大道。它看不见，摸不着，却决定着一切。\"",
                "秦鼎菱：\"宿命大战后天庭元气大伤，但气运未绝。只要运道尚在，天庭就不会灭亡。\"",
                "秦鼎菱：\"我接手天庭，不是因为野心，而是因为责任。这个烂摊子，总得有人收拾。\"",
                "秦鼎菱：\"方源……他的运势如同黑洞，吞噬一切好运。这样的人，是运道的天敌。\"",
                "秦鼎菱：\"你以为运气是随机的？不。运气是道，是可以计算、可以操控的力量。\"",
                "秦鼎菱：\"天庭的未来？我看到了无数条线，每一条都通向不同的结局。但我会选择对天庭最有利的那条。\"",
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
            var b = new DialogueTreeBuilder("HeavenCourt_QinDingLing", "greeting");

            b.StartNode("greeting",
                "秦鼎菱目光深邃，仿佛在审视你身上流转的运势。")
                .AddOption("询问运道", "ask_fortune_dao", DialogueOptionType.Informative)
                .AddOption("关于天庭的现状", "ask_heaven_status", DialogueOptionType.Informative)
                .AddOption("关于方源", "ask_fangyuan", DialogueOptionType.Informative)
                .AddOption("关于宿命大战", "ask_fate_war", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_fortune_dao",
                "秦鼎菱：\"运道，操控命运之力。宿命蛊掌控的是'必然'，而运道操控的是'或然'。宿命破碎后，运道成了影响世界走向的关键。\"")
                .AddOption("你能看到未来吗？", "ask_see_future")
                .AddOption("运势可以被改变吗？", "ask_change_fortune")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_see_future",
                "秦鼎菱：\"看到未来？不。我只能看到无数条可能的线。哪一条会成为现实，取决于无数微小的选择。这就是运道的本质——概率。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_change_fortune",
                "秦鼎菱：\"当然可以。运道蛊师可以增加好运的概率，降低厄运的概率。但代价是——你必须在别处偿还。运道守恒，这是铁律。\"")
                .AddOption("运道守恒？", "ask_fortune_conservation")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_fortune_conservation",
                "秦鼎菱：\"运道守恒——好运不会凭空产生，厄运也不会凭空消失。你得到的好运，必然来自他人的厄运。这就是为什么运道蛊师……总是孤独的。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_heaven_status",
                "秦鼎菱：\"天庭的现状？……一言难尽。宿命大战后，天庭损失惨重。多位八转陨落，九转尊者元气大伤。但天庭的根基还在，气运未绝。\"")
                .AddOption("你是如何接手天庭的？", "ask_takeover")
                .AddOption("天庭还能恢复吗？", "ask_recovery")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_takeover",
                "秦鼎菱：\"接手天庭？……不是我主动争取的。宿命大战后，天庭群龙无首，三公各有顾虑。最终，是我以运道推演，选择了最有利于天庭的道路——由我来领导。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_recovery",
                "秦鼎菱：\"恢复？需要时间，需要资源，更需要……运气。我会用运道为天庭争取最大的概率。但方源的存在，让一切变得不确定。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_fangyuan",
                "秦鼎菱的眼神变得锐利：\"方源……我运道推演中最大的变数。他的运势如同黑洞，吞噬一切好运。任何与他为敌的人，运势都会下降。这就是他最可怕的地方。\"")
                .AddOption("如何对付他？", "ask_counter_fangyuan")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_counter_fangyuan",
                "秦鼎菱：\"对付方源？……不能硬来。他的实力已经超越八转，接近九转。唯一的办法，是利用运道——让他的运势降到最低，让天庭的运势升到最高。然后，在他最虚弱的时候，一击致命。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_fate_war",
                "秦鼎菱：\"宿命大战……那是天庭最大的失败。红莲魔尊联合影宗，击碎了宿命蛊。天庭失去了最大的底牌，整个世界的格局都改变了。\"")
                .AddOption("宿命蛊真的碎了吗？", "ask_fate_gu_broken")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_fate_gu_broken",
                "秦鼎菱：\"碎了。彻底碎了。宿命不再束缚任何人，但这也意味着——天庭再也无法通过宿命来控制世界。我们只能靠实力说话了。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "秦鼎菱：\"交易？我可以为你调整运势……当然，需要等价交换。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "秦鼎菱微微点头：\"去吧。记住——运势无常，唯有算计永恒。\"")
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
            return new List<string> { "秦鼎菱" };
        }
    }
}
