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
    public class MeiGong : GuMasterBase
    {
        private int _attackTimer;
        private int _inkBarrierTimer;
        private bool _paintTrapActive;
        private int _paintTrapTimer;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/HeavenCourt/MeiGong_Head";

        public override FactionID GetFaction() => FactionID.Heaven;
        public override GuRank GetRank() => GuRank.Zhuan8_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Wise;

        public override string GuMasterDisplayName => "眉公";
        public override int GuMasterDamage => 370;
        public override int GuMasterLife => 68000;
        public override int GuMasterDefense => 210;

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
            NPC.knockBackResist = 0.15f;
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
            _inkBarrierTimer = 0;
            _paintTrapActive = false;
            _paintTrapTimer = 0;
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
                ExecutePaintingCombatAI();
            }

            PaintTrapUpdate();
        }

        private void PaintTrapUpdate()
        {
            if (_paintTrapActive)
            {
                _paintTrapTimer++;
                if (_paintTrapTimer > 300)
                {
                    _paintTrapActive = false;
                    _paintTrapTimer = 0;
                    CombatText.NewText(NPC.getRect(), Color.DarkSlateBlue, "画地为牢消散", false);
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

        private void ExecutePaintingCombatAI()
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
            _inkBarrierTimer++;

            if (_attackTimer >= 40)
            {
                _attackTimer = 0;
                ExecutePaintingAttack(target);
            }

            if (_inkBarrierTimer >= 200)
            {
                _inkBarrierTimer = 0;
                DeployInkBarrier(target);
            }
        }

        private void ExecutePaintingAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 5;

            int pattern = Main.rand.Next(4);
            switch (pattern)
            {
                case 0:
                    _paintTrapActive = true;
                    _paintTrapTimer = 0;
                    CombatText.NewText(NPC.getRect(), Color.DarkSlateBlue, "画地为牢！", true);
                    for (int i = 0; i < 20; i++)
                    {
                        float angle = MathHelper.TwoPi / 20f * i;
                        Dust.NewDust(NPC.Center + angle.ToRotationVector2() * 200f, 10, 10,
                            DustID.Shadowflame, 0, -2f, 100, Color.DarkSlateBlue, 1.5f);
                    }
                    break;
                case 1:
                    for (int i = -2; i <= 2; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.2f;
                        Vector2 velocity = angle.ToRotationVector2() * 8f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.WaterBolt, damage, 1f, Main.myPlayer);
                    }
                    CombatText.NewText(NPC.getRect(), Color.DarkSlateBlue, "墨弹！", true);
                    break;
                case 2:
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 5f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.WaterBolt, damage, 1f, Main.myPlayer);
                    }
                    CombatText.NewText(NPC.getRect(), Color.DarkSlateBlue, "泼墨成阵！", true);
                    break;
                case 3:
                    target.AddBuff(BuffID.Confused, 180);
                    target.AddBuff(BuffID.Slow, 180);
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 10f,
                        ProjectileID.CultistBossLightningOrb, damage, 0f, Main.myPlayer);
                    CombatText.NewText(NPC.getRect(), Color.DarkSlateBlue, "一笔定乾坤！", true);
                    break;
            }
        }

        private void DeployInkBarrier(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            var source = NPC.GetSource_FromAI();
            int damage = NPC.damage / 6;

            CombatText.NewText(NPC.getRect(), Color.DarkSlateBlue, "墨壁！", true);

            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi / 6f * i;
                Vector2 pos = NPC.Center + angle.ToRotationVector2() * 120f;
                Vector2 velocity = (target.Center - pos).SafeNormalize(Vector2.UnitY) * 4f;
                Projectile.NewProjectile(source, pos, velocity,
                    ProjectileID.WaterBolt, damage, 1f, Main.myPlayer);
            }

            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.Shadowflame,
                    Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f),
                    100, Color.DarkSlateBlue, 1.5f);
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => HostileDialogue(),
                GuAttitude.Wary => "你的画意……比我想象的深沉。但画道面前，万物皆为墨。",
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => "能看破画中真意之人，必有非凡之心。",
                GuAttitude.Contemptuous => "不懂画道之人，不过是纸上的一抹败笔。",
                GuAttitude.Fearful => "……你的意志，让我也感到了一丝不安。",
                _ => "一笔画天下，一墨定乾坤。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "一笔画天下，一墨定乾坤！",
                "画道之下，你的命运已被我画下！",
                "你以为能逃出我的画卷？天真。",
                "天庭三公的威严，不容亵渎！",
                "画地为牢——你已无处可逃！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "眉公：\"画道，以笔墨绘天地。一笔可定乾坤，一墨可画牢笼。画道修心，心若止水，方能落笔无悔。\"",
                "眉公：\"一笔画天下，一墨定乾坤——这是我一生的追求。画道不只是攻击，更是对天地万物的理解。\"",
                "眉公：\"三公之中，我最为从容。龙公有他的遗憾，铜公有他的执念，而我——只求画中真意。\"",
                "眉公：\"画地为牢，不只是杀招，更是画道的哲学。将万物困于画中，让现实与虚幻交织，这才是画道的极致。\"",
                "眉公：\"墨弹看似简单，实则蕴含画道真意。每一滴墨，都是一幅画；每一幅画，都是一个世界。\"",
                "眉公：\"天庭需要的不是暴君，而是能看透世间万象的智者。这就是我修画道的原因。\"",
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
            var b = new DialogueTreeBuilder("HeavenCourt_MeiGong", "greeting");

            b.StartNode("greeting",
                "眉公手持画笔，目光从容深邃，周身隐隐有墨香浮动。")
                .AddOption("询问画道", "ask_painting_dao", DialogueOptionType.Informative)
                .AddOption("关于画地为牢", "ask_paint_trap", DialogueOptionType.Informative)
                .AddOption("关于三公", "ask_three_dukes", DialogueOptionType.Informative)
                .AddOption("关于墨法", "ask_ink_art", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_painting_dao",
                "眉公：\"画道，以笔墨绘天地之力。一笔可定乾坤，一墨可画牢笼。画道的精髓，在于将虚幻化为现实，将现实困于虚幻。\"")
                .AddOption("画道和幻术有什么区别？", "ask_painting_vs_illusion")
                .AddOption("画道能画出活物吗？", "ask_painting_life")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_painting_vs_illusion",
                "眉公：\"幻术是欺骗感官，画道是创造现实。幻术的幻象一戳即破，画道的画作却真实存在。墨弹可以伤人，画地为牢可以困人——这些都是真实的。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_painting_life",
                "眉公微微一笑：\"画出活物？……理论上可以，但需要九转的修为。我目前只能画出墨弹和牢笼。但也许有一天，我画出的龙，能真正飞翔于天际。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_paint_trap",
                "眉公：\"画地为牢——以画道之力，在地面画出一个牢笼。被画入牢笼者，行动受限，如同被困画中。这是画道最实用的杀招。\"")
                .AddOption("天庭是以画道治理吗？", "ask_heaven_painting")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_heaven_painting",
                "眉公：\"……天庭？天庭有时以画道治理，有时以力服人。画地为牢不只是攻击，更是一种秩序——将混乱困于画中，让世界回归安宁。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_three_dukes",
                "眉公：\"三公各有所长。龙公刚毅，铜公坚定，而我……我追求的是从容。三公如鼎之三足，缺一不可。\"")
                .AddOption("三公之间有分歧吗？", "ask_duke_harmony")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_duke_harmony",
                "眉公：\"分歧？当然有。但和谐不是没有分歧，而是在分歧中找到共识。这就是画道的智慧——黑白相映，方成画卷。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_ink_art",
                "眉公：\"墨法是画道的根基。墨弹、墨壁、泼墨成阵……每一种墨法，都是一幅画。在画道高手眼中，战场就是画卷，敌人不过是画中的一笔。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "眉公：\"交易？当然可以。天庭的资源，愿意与有缘者共享。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "眉公微微颔首：\"去吧。记住——一笔画天下，一墨定乾坤。\"")
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
            return new List<string> { "眉公" };
        }
    }
}
