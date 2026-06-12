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
    public class GuYueFangZheng : GuMasterBase
    {
        private int _attackTimer;
        private int _bloodColdTimer;
        private bool _bloodColdActive;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/HeavenCourt/GuYueFangZheng_Head";

        public override FactionID GetFaction() => FactionID.Heaven;
        public override GuRank GetRank() => GuRank.Zhuan7_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.DualFaced;

        public override string GuMasterDisplayName => "古月方正";
        public override int GuMasterDamage => 380;
        public override int GuMasterLife => 50000;
        public override int GuMasterDefense => 150;

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
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Bleeding] = true;
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
            NPC.knockBackResist = 0.15f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(3, 0, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _bloodColdTimer = 0;
            _bloodColdActive = false;
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
                ExecuteBloodCombatAI();
            }

            BloodColdUpdate();
        }

        private void BloodColdUpdate()
        {
            if (_bloodColdActive)
            {
                _bloodColdTimer++;
                if (_bloodColdTimer > 240)
                {
                    _bloodColdActive = false;
                    _bloodColdTimer = 0;
                    CombatText.NewText(NPC.getRect(), Color.LightBlue, "血渐冷消散", false);
                }
                else
                {
                    var target = Main.player[NPC.target];
                    if (target.active && !target.dead)
                    {
                        float dist = Vector2.Distance(NPC.Center, target.Center);
                        if (dist < 350f)
                        {
                            target.AddBuff(BuffID.Frostburn, 60);
                            target.AddBuff(BuffID.Chilled, 60);
                        }
                    }
                }
            }
        }

        private void ExecuteBloodCombatAI()
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
                NPC.velocity.X = dir * 3f;
            else if (dist > 150f)
                NPC.velocity.X = dir * 2f;
            else
                NPC.velocity.X = dir * 1.2f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -7f;

            _attackTimer++;
            if (_attackTimer >= 35)
            {
                _attackTimer = 0;
                ExecuteBloodAttack(target);
            }
        }

        private void ExecuteBloodAttack(Player target)
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
                        float angle = toTarget.ToRotation() + i * 0.2f;
                        Vector2 velocity = angle.ToRotationVector2() * 9f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.Fireball, damage, 1f, Main.myPlayer);
                    }
                    CombatText.NewText(NPC.getRect(), Color.Crimson, "血亲心仇！", true);
                    break;
                case 1:
                    _bloodColdActive = true;
                    _bloodColdTimer = 0;
                    CombatText.NewText(NPC.getRect(), Color.LightBlue, "血渐冷——冻结一切！", true);
                    for (int i = 0; i < 15; i++)
                    {
                        float angle = MathHelper.TwoPi / 15f * i;
                        Dust.NewDust(NPC.Center + angle.ToRotationVector2() * 150f, 10, 10,
                            DustID.IceTorch, 0, -2f, 100, Color.LightBlue, 1.5f);
                    }
                    break;
                case 2:
                    for (int i = 0; i < 10; i++)
                    {
                        float angle = MathHelper.TwoPi / 10f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 6f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.IceBlock, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 3:
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 12f,
                        ProjectileID.VampireKnife, damage * 2, 2f, Main.myPlayer);
                    break;
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => HostileDialogue(),
                GuAttitude.Wary => "你让我想起了……那个人。不，你不是他。",
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => "你……和哥哥不同。你选择了另一条路。",
                GuAttitude.Contemptuous => "弱者……就像曾经的我。",
                GuAttitude.Fearful => "……我不想再失去任何人了。",
                _ => "哥哥……我们为何走上了不同的道路？"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "血亲心仇——你的血，将为我所用！",
                "哥哥……为什么你要走上那条路？！",
                "我恨你，但我也……理解你。",
                "血渐冷……你的心也会冷却的。",
                "天庭给了我力量，让我能与他抗衡！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "古月方正：\"哥哥……我们终究走上了不同的路。他选择了永生，而我……选择了复仇。\"",
                "古月方正：\"血道……是仇恨赋予我的力量。每一次使用，我都能感受到血液中的愤怒。\"",
                "古月方正：\"天庭收留了我，培养了我，让我有了与哥哥抗衡的资本。但代价是……我不再是原来的我。\"",
                "古月方正：\"你知道吗？我曾经也想过，如果当初走的是哥哥的路，会怎样？但……没有如果。\"",
                "古月方正：\"血亲心仇——这是血道的禁忌之术。对血亲使用，威力倍增。天庭就是为了这个才培养我的。\"",
                "古月方正：\"血渐冷……不只是攻击，也是我的心境。仇恨燃烧殆尽后，剩下的只有冰冷。\"",
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
            var b = new DialogueTreeBuilder("HeavenCourt_GuYueFangZheng", "greeting");

            b.StartNode("greeting",
                "古月方正的眼神中交织着恨意与迷茫，周身隐隐有血色光芒。")
                .AddOption("询问血道", "ask_blood_dao", DialogueOptionType.Informative)
                .AddOption("关于方源", "ask_fangyuan", DialogueOptionType.Informative)
                .AddOption("关于天庭", "ask_heaven", DialogueOptionType.Informative)
                .AddOption("关于古月家族", "ask_guyue_family", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_blood_dao",
                "古月方正：\"血道……以血为媒，操控生命之力。血亲之间，血脉相连，血道的威力可以成倍增长。这就是'血亲心仇'的原理。\"")
                .AddOption("血亲心仇是什么？", "ask_blood_grudge")
                .AddOption("血渐冷呢？", "ask_blood_cold")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_blood_grudge",
                "古月方正的声音变得冰冷：\"血亲心仇——对血亲使用血道攻击，威力倍增。天庭培养我，就是为了让我成为克制方源的武器。因为……我是他唯一的血亲。\"")
                .AddOption("你甘心做武器吗？", "ask_willing_weapon")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_willing_weapon",
                "古月方正沉默良久：\"甘心？……一开始不甘心。但后来我想通了——如果做武器能让我接近哥哥，能让我亲手了结这一切，那我甘心。\"")
                .AddOption("了结？你想杀他？", "ask_kill_him")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_kill_him",
                "古月方正：\"杀他？……我不知道。有时候我想杀他，有时候我又想问他——为什么？为什么你要抛弃我？为什么你选择了永生，而不是我们？\"")
                .AddOption("也许他有自己的理由", "ask_his_reason")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_his_reason",
                "古月方正苦笑：\"理由？他的理由永远是——永生。在永生面前，亲情、友情、爱情，都是可以抛弃的。我……我无法理解，也无法接受。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_blood_cold",
                "古月方正：\"血渐冷……不只是杀招，也是我的心境。仇恨燃烧了太久，连血液都变得冰冷。有时候我觉得，我已经不像一个人了。\"")
                .AddOption("你还记得从前的自己吗？", "ask_old_self")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_old_self",
                "古月方正的眼神恍惚了一瞬：\"从前？……从前我也是古月山寨的少年，也曾经天真过。但哥哥离开后，一切都变了。天庭找到了我，告诉我——我是唯一能克制方源的人。从那一刻起，我就不再是古月方正了。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_fangyuan",
                "古月方正的拳头握紧：\"方源……我的双生哥哥。我们本是一体，却走上了截然相反的道路。他追求永生，我追求……复仇？不，也许我追求的只是——一个答案。\"")
                .AddOption("什么答案？", "ask_answer")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_answer",
                "古月方正：\"为什么？为什么他可以毫不犹豫地抛弃一切？为什么他可以如此冷酷？为什么……他从来不在乎我？这个答案，只有他本人能给我。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_heaven",
                "古月方正：\"天庭……他们救了我，也利用了我。但我不恨他们。在这个世界上，每个人都在利用别人，也被别人利用。至少天庭给了我力量，让我不再是那个无力的少年。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_guyue_family",
                "古月方正的表情变得痛苦：\"古月家族……已经不存在了。被哥哥亲手毁灭。那些曾经收留我们的人，那些曾经照顾我们的人……都死了。而我，却活了下来。\"")
                .AddOption("你恨他吗？", "ask_hate")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_hate",
                "古月方正：\"恨？……我不知道。有时候我恨他入骨，有时候我又……想念他。他是我的哥哥，我的另一半。恨他，就像恨自己一样。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "古月方正：\"交易？……可以。但我只交换有价值的东西。感情，不在交易范围内。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "古月方正转过身去：\"去吧。也许下次见面，我们就是敌人了。\"")
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
            return new List<string> { "古月方正" };
        }
    }
}
