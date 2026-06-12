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
    public class HanHuiXianGu : GuMasterBase
    {
        private int _attackTimer;
        private int _soulFreezeTimer;
        private bool _soulPacifyActive;
        private int _soulPacifyTimer;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/ShadowSect/HanHuiXianGu_Head";

        public override FactionID GetFaction() => FactionID.ShadowSect;
        public override GuRank GetRank() => GuRank.Zhuan8_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Cold;

        public override string GuMasterDisplayName => "寒灰仙姑";
        public override int GuMasterDamage => 360;
        public override int GuMasterLife => 60000;
        public override int GuMasterDefense => 180;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 1000;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 25;
            NPCID.Sets.AttackAverageChance[Type] = 15;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Chilled] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frozen] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;

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
            _soulFreezeTimer = 0;
            _soulPacifyActive = false;
            _soulPacifyTimer = 0;
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
                ExecuteSoulCombatAI();
            }

            SoulPacifyUpdate();
        }

        private void SoulPacifyUpdate()
        {
            if (_soulPacifyActive)
            {
                _soulPacifyTimer++;
                if (_soulPacifyTimer > 300)
                {
                    _soulPacifyActive = false;
                    _soulPacifyTimer = 0;
                    CombatText.NewText(NPC.getRect(), Color.LightCyan, "灵魂安抚消散", false);
                }
                else
                {
                    var target = Main.player[NPC.target];
                    if (target.active && !target.dead)
                    {
                        float dist = Vector2.Distance(NPC.Center, target.Center);
                        if (dist < 400f)
                        {
                            target.AddBuff(BuffID.Frozen, 30);
                            target.AddBuff(BuffID.Chilled, 60);
                        }
                    }
                }
            }
        }

        private void ExecuteSoulCombatAI()
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
            _soulFreezeTimer++;

            if (_attackTimer >= 45)
            {
                _attackTimer = 0;
                ExecuteSoulAttack(target);
            }

            if (_soulFreezeTimer >= 200)
            {
                _soulFreezeTimer = 0;
                SoulFreeze(target);
            }
        }

        private void ExecuteSoulAttack(Player target)
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
                        Vector2 velocity = angle.ToRotationVector2() * 9f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.IceBlock, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 1:
                    _soulPacifyActive = true;
                    _soulPacifyTimer = 0;
                    CombatText.NewText(NPC.getRect(), Color.LightCyan, "灵魂安抚——万物寂灭！", true);
                    for (int i = 0; i < 20; i++)
                    {
                        float angle = MathHelper.TwoPi / 20f * i;
                        Dust.NewDust(NPC.Center + angle.ToRotationVector2() * 200f, 10, 10,
                            DustID.IceTorch, 0, -2f, 100, Color.LightCyan, 1.5f);
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
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 11f,
                        ProjectileID.CultistBossIceMist, damage * 2, 2f, Main.myPlayer);
                    target.AddBuff(BuffID.Frostburn, 180);
                    break;
            }
        }

        private void SoulFreeze(Player target)
        {
            float dist = Vector2.Distance(NPC.Center, target.Center);
            if (dist < 600f)
            {
                target.AddBuff(BuffID.Frozen, 120);
                target.AddBuff(BuffID.Chilled, 180);

                if (Main.netMode != NetmodeID.Server)
                {
                    CombatText.NewText(target.Hitbox, Color.LightCyan, "灵魂冻结！", true);
                    for (int i = 0; i < 15; i++)
                    {
                        Dust.NewDustDirect(target.position, target.width, target.height,
                            DustID.IceTorch, 0, -3f, Scale: 1.5f);
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
                GuAttitude.Wary => "你的灵魂……在颤抖。是因为恐惧，还是因为寒冷？",
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => "你的灵魂……比大多数人都要坚韧。",
                GuAttitude.Contemptuous => "脆弱的灵魂，连寒意都无法承受。",
                GuAttitude.Fearful => "……你的灵魂中，有我无法冻结的东西。",
                _ => "灵魂……不过是另一种形式的灰烬。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "灵魂的安宁，才是永恒！",
                "在寒灰面前，你的灵魂将化为冰晶！",
                "灵魂安抚——让你永远沉睡！",
                "冷……你会习惯的。习惯了，就不再痛苦。",
                "魂安洞的寒意，足以冻结一切！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "寒灰仙姑：\"灵魂的安宁，才是永恒。肉体会腐朽，但灵魂……可以被永远保存。\"",
                "寒灰仙姑：\"魂安洞是我修炼的地方。那里的寒意，足以冻结灵魂。但只有在这种极寒中，灵魂才能得到真正的安宁。\"",
                "寒灰仙姑：\"我是幽魂魔尊的分魂之一。与其他分魂不同，我追求的不是胜利，而是——安宁。\"",
                "寒灰仙姑：\"灵魂冻结……不只是攻击。它是一种慈悲。被冻结的灵魂，不再痛苦，不再挣扎，只有永恒的平静。\"",
                "寒灰仙姑：\"影宗的使命是击碎宿命。但宿命破碎后呢？混乱中的灵魂更需要安抚。这就是我的职责。\"",
                "寒灰仙姑：\"你问我会不会感到孤独？……不会。在灵魂的世界里，没有孤独，只有安宁。\"",
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
            var b = new DialogueTreeBuilder("ShadowSect_HanHuiXianGu", "greeting");

            b.StartNode("greeting",
                "寒灰仙姑周身寒气缭绕，目光冷若冰霜，却透着一种超然的宁静。")
                .AddOption("询问魂道", "ask_soul_dao", DialogueOptionType.Informative)
                .AddOption("关于魂安洞", "ask_soul_cave", DialogueOptionType.Informative)
                .AddOption("关于幽魂魔尊", "ask_youhun", DialogueOptionType.Informative)
                .AddOption("关于灵魂安宁", "ask_soul_peace", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_soul_dao",
                "寒灰仙姑：\"魂道，操控灵魂之力。灵魂是存在的本质，肉体不过是过客。真正的强者，追求的不是肉体的永生，而是灵魂的永恒。\"")
                .AddOption("灵魂冻结是什么？", "ask_soul_freeze")
                .AddOption("灵魂安抚呢？", "ask_soul_pacify")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_soul_freeze",
                "寒灰仙姑：\"灵魂冻结——以极寒之力冻结敌人的灵魂。被冻结者无法行动，无法思考，只能感受无尽的寒冷。这是……一种慈悲。\"")
                .AddOption("慈悲？", "ask_mercy")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_mercy",
                "寒灰仙姑：\"是的，慈悲。在这个充满痛苦的世界里，灵魂的冻结意味着痛苦的终结。不再挣扎，不再恐惧，只有永恒的平静。这不是慈悲，是什么？\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_soul_pacify",
                "寒灰仙姑：\"灵魂安抚——比冻结更温和的手段。让灵魂进入安宁的状态，不再痛苦，不再挣扎。被安抚者甚至无法使用物品——因为他们已经不需要了。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_soul_cave",
                "寒灰仙姑：\"魂安洞，我的修炼之地。洞中寒气足以冻结八转以下蛊师的灵魂。在那里修炼，可以让灵魂更加坚韧，更加安宁。\"")
                .AddOption("你在那里修炼了多久？", "ask_cultivation_time")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_cultivation_time",
                "寒灰仙姑：\"多久？……我已经不记得了。在魂安洞中，时间没有意义。只有灵魂的安宁，才是唯一的度量。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_youhun",
                "寒灰仙姑：\"幽魂魔尊……我的主人。他将灵魂分裂成无数碎片，每一片都是独立的个体。我继承了主人对'安宁'的追求。\"")
                .AddOption("你不追求击碎宿命吗？", "ask_no_fate")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_no_fate",
                "寒灰仙姑：\"击碎宿命？……那是影宗的使命，不是我的追求。我只想要安宁。宿命碎与不碎，与我无关。但如果主人的意志是击碎宿命，那我就执行。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_soul_peace",
                "寒灰仙姑：\"灵魂的安宁……是超越生死的状态。不喜不悲，不怒不惧。只有达到这种境界，灵魂才能永恒。这就是我追求的道。\"")
                .AddOption("你达到了吗？", "ask_achieved")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_achieved",
                "寒灰仙姑沉默良久：\"……没有。我依然会感到寒冷，依然会感到孤独。也许……安宁本身就是一个永远无法到达的彼岸。但追求的过程，就是意义。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "寒灰仙姑：\"交易？我可以提供灵魂方面的……服务。但代价是——你必须体验一次灵魂冻结。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "寒灰仙姑微微点头：\"去吧。愿你的灵魂，找到安宁。\"")
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
            return new List<string> { "寒灰仙姑" };
        }
    }
}
