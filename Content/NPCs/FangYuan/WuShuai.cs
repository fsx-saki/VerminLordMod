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

namespace VerminLordMod.Content.NPCs.FangYuan
{
    [AutoloadHead]
    public class WuShuai : GuMasterBase
    {
        private int _attackTimer;
        private int _dragonSummonTimer;
        private int _dragonCount;
        private bool _dreamCommandActive;
        private int _dreamCommandTimer;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/FangYuan/WuShuai_Head";

        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.Zhuan8_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Cunning;

        public override string GuMasterDisplayName => "吴帅";
        public override int GuMasterDamage => 380;
        public override int GuMasterLife => 75000;
        public override int GuMasterDefense => 220;

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
            _dragonSummonTimer = 0;
            _dragonCount = 0;
            _dreamCommandActive = false;
            _dreamCommandTimer = 0;
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

            DreamCommandUpdate();
            DragonAuraEffect();
        }

        private void DragonAuraEffect()
        {
            if (Main.rand.NextBool(8))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat() * 50f;
                Vector2 offset = angle.ToRotationVector2() * dist;
                Dust.NewDust(NPC.Center + offset, 4, 4, DustID.Shadowflame,
                    0f, -0.5f, 100, Color.RoyalBlue, 0.6f);
            }
        }

        private void DreamCommandUpdate()
        {
            if (_dreamCommandActive)
            {
                _dreamCommandTimer++;
                if (_dreamCommandTimer > 120)
                {
                    _dreamCommandActive = false;
                    _dreamCommandTimer = 0;
                }
                else
                {
                    var target = Main.player[NPC.target];
                    if (target.active && !target.dead)
                    {
                        target.AddBuff(BuffID.Confused, 30);
                        target.AddBuff(BuffID.Slow, 30);
                    }
                }
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

            if (dist > 500f)
                NPC.velocity.X = dir * 3.5f;
            else if (dist > 200f)
                NPC.velocity.X = dir * 2f;
            else
                NPC.velocity.X = dir * 1.2f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -9f;

            _attackTimer++;
            if (_attackTimer >= 40)
            {
                _attackTimer = 0;
                ExecuteDragonAttack(target);
            }

            _dragonSummonTimer++;
            if (_dragonSummonTimer >= 300 && _dragonCount < 4)
            {
                _dragonSummonTimer = 0;
                _dragonCount++;
                SummonDragonMinion();
            }
        }

        private void ExecuteDragonAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 5;

            int pattern = Main.rand.Next(4);

            switch (pattern)
            {
                case 0:
                    for (int i = -1; i <= 1; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.15f;
                        Vector2 velocity = angle.ToRotationVector2() * 10f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.DD2BetsyFireball, damage, 2f, Main.myPlayer);
                    }
                    break;

                case 1:
                    _dreamCommandActive = true;
                    _dreamCommandTimer = 0;
                    CombatText.NewText(NPC.getRect(), Color.MediumPurple, "如梦令——臣服于我！", true);

                    for (int i = 0; i < 10; i++)
                    {
                        float angle = MathHelper.TwoPi / 10f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 5f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.WaterBolt, damage, 1f, Main.myPlayer);
                    }
                    break;

                case 2:
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 spawnPos = target.Center + new Vector2(
                            Main.rand.Next(-300, 300),
                            -400);
                        Vector2 velocity = new Vector2(0, 8f);
                        Projectile.NewProjectile(source, spawnPos, velocity,
                            ProjectileID.Meteor1, damage, 3f, Main.myPlayer);
                    }
                    break;

                case 3:
                    Vector2 dashDir = toTarget.SafeNormalize(Vector2.UnitX);
                    NPC.velocity = dashDir * 15f;
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 spawnPos = NPC.Center + Main.rand.NextVector2Circular(30f, 30f);
                        Projectile.NewProjectile(source, spawnPos, dashDir * 12f,
                            ProjectileID.DD2BetsyFireball, damage, 2f, Main.myPlayer);
                    }
                    break;
            }
        }

        private void SummonDragonMinion()
        {
            CombatText.NewText(NPC.getRect(), Color.RoyalBlue, "龙宫——召唤龙族！", true);

            for (int i = 0; i < 25; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.Shadowflame,
                    Main.rand.NextFloat(-4f, 4f),
                    Main.rand.NextFloat(-4f, 4f),
                    100, Color.RoyalBlue, 1.8f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                Vector2 spawnPos = NPC.Center + new Vector2(
                    Main.rand.Next(-200, 200),
                    -100);

                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi / 6f * i;
                    Vector2 velocity = angle.ToRotationVector2() * 4f;
                    Projectile.NewProjectile(source, spawnPos, velocity,
                        ProjectileID.CultistBossLightningOrb, NPC.damage / 6, 0f, Main.myPlayer);
                }
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "龙宫之主，岂是你能冒犯的？",
                GuAttitude.Wary => "你在打量我？龙人的直觉告诉我，你不简单。",
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => "龙族尊重强者。你……值得尊重。",
                GuAttitude.Contemptuous => "在龙宫面前，你的傲慢毫无意义。",
                GuAttitude.Fearful => "龙人的字典里没有恐惧……好吧，也许有一点。",
                _ => "龙宫深处的秘密，不是谁都能知晓的。"
            };
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "吴帅：\"龙宫，东海最伟大的势力之一。而我——以龙人之躯，成为了它的主人。\"",
                "吴帅：\"奴道的精髓在于——让万物为你所用。龙族虽强，但在奴道面前，也要低头。\"",
                "吴帅：\"如梦令，可以控制任何生灵的心智。这是奴道最恐怖的力量。\"",
                "吴帅：\"我的龙人之躯，让我可以与龙族沟通。这是方源选择我掌控龙宫的原因。\"",
                "吴帅：\"东海龙族有万条之众，每一条龙都是我的棋子。这就是龙宫的力量。\"",
                "吴帅：\"方源的算计从不出错。他选择我入主龙宫，是因为只有我能驾驭龙族。\"",
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
            var b = new DialogueTreeBuilder("FangYuan_WuShuai", "greeting");

            b.StartNode("greeting",
                "吴帅身披龙鳞战甲，周身萦绕着淡淡的龙威。")
                .AddOption("询问奴道", "ask_slave_dao", DialogueOptionType.Informative)
                .AddOption("关于龙宫", "ask_dragon_palace", DialogueOptionType.Informative)
                .AddOption("关于如梦令", "ask_dream_command", DialogueOptionType.Informative)
                .AddOption("关于方源", "ask_fangyuan", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_slave_dao",
                "吴帅：\"奴道，驭使万物之道。无论是人、兽、还是龙，都可以成为奴道蛊师的仆从。关键不在于力量，而在于——控制。\"")
                .AddOption("奴道的极限是什么？", "ask_slave_limit")
                .AddOption("奴道和自由矛盾吗？", "ask_slave_freedom")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_slave_limit",
                "吴帅：\"极限？理论上，奴道可以控制一切有意识的生灵。但控制越强的存在，消耗的真元越多。控制一条龙和控制一个人，完全是两个概念。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_slave_freedom",
                "吴帅：\"自由？在蛊师的世界里，自由是奢侈品。每个人都在被什么控制——被欲望、被恐惧、被命运。奴道只是让这种控制变得显性而已。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_dragon_palace",
                "吴帅：\"龙宫，东海龙族的圣地。龙宫之中有万龙守护，更有无数龙族传承。方源命我入主龙宫，是为了获取龙族的力量。\"")
                .AddOption("你是怎么征服龙宫的？", "ask_conquer_palace")
                .AddOption("龙宫有什么宝物？", "ask_palace_treasure")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_conquer_palace",
                "吴帅：\"征服？不，不是征服。是——驾驭。龙族尊重强者，而我的龙人之躯让我拥有了龙族血脉。再加上奴道的力量，龙宫自然归我所有。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_palace_treasure",
                "吴帅：\"龙宫的宝物？太多了。龙珠、龙鳞、龙髓……还有最重要的——龙族传承。这些，都是方源计划的一部分。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_dream_command",
                "吴帅：\"如梦令，八转仙蛊，奴道至宝。它可以让任何生灵陷入梦境，在梦中接受我的命令。醒来后，他们会不自觉地执行我的意志。\"")
                .AddOption("如梦令能控制尊者吗？", "ask_control_venerable")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_control_venerable",
                "吴帅摇头：\"尊者？不能。尊者的意志远超常人，如梦令对尊者只能造成短暂的干扰。但即使是短暂的干扰，在战斗中也足以致命。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_fangyuan",
                "吴帅：\"方源……他是我的本体，我的创造者。吴帅只是他众多分身之一，但——我是唯一拥有龙人之躯的分身。这让他在东海的计划中，我不可或缺。\"")
                .AddOption("你忠于方源吗？", "ask_loyalty")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_loyalty",
                "吴帅：\"忠诚？不，这不是忠诚。这是——利益一致。方源追求永生，而我作为他的分身，他的永生就是我的永生。我们不需要忠诚，因为我们的命运已经绑定。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "吴帅：\"龙宫的宝物，可以与你交易。但记住——龙族的东西，不便宜。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "吴帅微微颔首：\"龙宫的大门，随时为你敞开。\"")
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
            return new List<string> { "吴帅" };
        }
    }
}
