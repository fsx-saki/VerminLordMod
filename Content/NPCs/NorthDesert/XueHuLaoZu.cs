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

namespace VerminLordMod.Content.NPCs.NorthDesert
{
    [AutoloadHead]
    public class XueHuLaoZu : GuMasterBase
    {
        private int _attackTimer;
        private int _iceArmorRegenTimer;
        private int _iceArmorStacks;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.ChangShengTian;
        public override GuRank GetRank() => GuRank.Zhuan7_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Arrogant;

        public override string GuMasterDisplayName => "雪胡老祖";
        public override int GuMasterDamage => 340;
        public override int GuMasterLife => 52000;
        public override int GuMasterDefense => 190;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 1000;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 30;
            NPCID.Sets.AttackAverageChance[Type] = 20;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frozen] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Chilled] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;

            if (!NPCID.Sets.NPCBestiaryDrawOffset.ContainsKey(Type))
            {
                NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers { Velocity = 1f, Direction = 1 });
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
            NPC.value = Item.buyPrice(1, 50, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _iceArmorRegenTimer = 0;
            _iceArmorStacks = 5;
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

            _iceArmorRegenTimer++;
            if (_iceArmorRegenTimer >= 300 && _iceArmorStacks < 5)
            {
                _iceArmorStacks++;
                _iceArmorRegenTimer = 0;
                NPC.defense = GuMasterDefense + (int)GetRank() + _iceArmorStacks * 15;
                CombatText.NewText(NPC.getRect(), Color.LightCyan, "冰甲再生！", false);
            }

            if (CurrentAttitude == GuAttitude.Hostile || (HasBeenHitByPlayer && AggroTimer > 0))
            {
                ExecuteCombatAI();
            }
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (_iceArmorStacks > 0)
            {
                _iceArmorStacks--;
                NPC.defense = GuMasterDefense + (int)GetRank() + _iceArmorStacks * 15;
                _iceArmorRegenTimer = 0;

                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(NPC.Center, 20, 20, DustID.Ice,
                        Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
                        100, Color.LightCyan, 1.2f);
                }
            }
            base.ModifyHitByProjectile(projectile, ref modifiers);
        }

        private void ExecuteCombatAI()
        {
            NPC.TargetClosest(true);
            var target = Main.player[NPC.target];
            if (!target.active || target.dead) { NPC.velocity *= 0.95f; return; }

            float dist = Vector2.Distance(NPC.Center, target.Center);
            float dir = target.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = (int)dir;

            NPC.velocity.X = dir * 2f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -6f;

            _attackTimer++;
            if (_attackTimer >= 45)
            {
                _attackTimer = 0;
                ExecuteAttack(target);
            }
        }

        private void ExecuteAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 4;

            int pattern = Main.rand.Next(3);
            switch (pattern)
            {
                case 0:
                    CombatText.NewText(NPC.getRect(), Color.LightBlue, "暴风雪！", true);
                    for (int i = 0; i < 20; i++)
                    {
                        Vector2 spawnPos = target.Center + new Vector2(Main.rand.Next(-300, 300), -400);
                        Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), 8f);
                        Projectile.NewProjectile(source, spawnPos, vel, ProjectileID.Blizzard, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 1:
                    for (int i = -4; i <= 4; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.08f;
                        Vector2 vel = angle.ToRotationVector2() * 11f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.IceBolt, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 2:
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 vel = angle.ToRotationVector2() * 5f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.FrostDaggerfish, damage, 2f, Main.myPlayer);
                    }
                    target.AddBuff(BuffID.Chilled, 120);
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
                    1 => "北原之雪，永不停歇。",
                    2 => "你竟敢冒犯我？不知天高地厚！",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "哼，你最好别打什么歪主意。",
                GuAttitude.Friendly => "你是长生天的朋友？那便好说。",
                GuAttitude.Respectful => "有实力的人，我雪胡老祖也高看一眼。",
                GuAttitude.Contemptuous => "区区蝼蚁，也配与我说话？",
                GuAttitude.Fearful => "这不可能……我的冰甲竟被击碎……",
                _ => "北原之雪，永不停歇。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "冰道之下，万物凋零！",
                "北原的暴风雪，将吞噬你的一切！",
                "我雪胡老祖活了数百年，什么场面没见过？",
                "冰甲护体，你伤不了我！",
                "在北原，我就是寒风的主人！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("XueHuLaoZu", "greeting");

            b.StartNode("greeting", "雪胡老祖傲然挺立，胡须上挂着冰霜。")
                .AddOption("谈论冰道", "bing_dao", DialogueOptionType.Teach)
                .AddOption("北原的往事", "north_past", DialogueOptionType.Informative)
                .AddOption("关于长生天", "changshengtian", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("bing_dao", "雪胡老祖：\"冰道？哼，北原的冰道蛊师多如牛毛，但真正领悟冰道真谛的，不过寥寥数人。\"")
                .AddOption("冰道的真谛是什么？", "bing_truth")
                .AddOption("冰甲之术？", "ice_armor")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bing_truth", "雪胡老祖：\"冰的真谛不是寒冷，而是——永恒。冰可以保存万物，让时间在冰中停止。这才是冰道的最高境界。\"")
                .AddOption("回到冰道", "bing_dao");

            b.StartNode("ice_armor", "雪胡老祖：\"冰甲是我独创的防御之术。以冰道蛊虫凝聚冰甲，可抵御万法。冰甲破碎后还能再生，这就是冰道的持久之力。\"")
                .AddOption("回到冰道", "bing_dao");

            b.StartNode("north_past", "雪胡老祖：\"北原的往事？我在北原修炼了数百年，见证了无数势力的兴衰。唯有长生天，屹立不倒。\"")
                .AddOption("北原最危险的时刻？", "north_danger")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("north_danger", "雪胡老祖：\"最危险的时刻？天庭曾联合南疆势力入侵北原，那一战，我以暴风雪覆盖千里，将入侵者尽数冻杀。\"")
                .AddOption("回到北原往事", "north_past");

            b.StartNode("changshengtian", "雪胡老祖：\"长生天是巨阳仙尊的遗产，冰塞川是领袖，我是核心战力之一。我们守护北原，不容外敌侵犯。\"")
                .AddOption("你与冰塞川的关系？", "with_bing")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("with_bing", "雪胡老祖：\"冰塞川……他虽然冷酷，但确实有领袖之才。我尊重他的决策，但我雪胡老祖也不是任人驱使之辈。\"")
                .AddOption("回到长生天", "changshengtian");

            b.StartNode("trade", "雪胡老祖：\"交易？可以。但我只和有实力的人交易。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "雪胡老祖冷哼一声：\"去吧，别在北原迷路了。\"")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            var tree = b.Build();
            tree.NPCType = Type;
            DialogueTreeManager.Instance.RegisterTree(tree);
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
                        mgr.StartDialogue(NPC, Main.LocalPlayer);

                    var currentText = mgr.GetCurrentNPCText(Main.LocalPlayer);
                    var options = mgr.GetCurrentOptions(Main.LocalPlayer);

                    if (options != null && options.Count > 0)
                    {
                        DialogueTreeUI.Instance.Open(NPC.GivenName, NPCHeadLoader.GetHeadSlot(HeadTexture), currentText ?? "", options);
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

        public override bool CanChat() => CurrentAttitude != GuAttitude.Hostile || NPC.life > NPC.lifeMax * 0.5f;

        public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0f;

        public override List<string> SetNPCNameList() => new List<string> { "雪胡老祖" };
    }
}
