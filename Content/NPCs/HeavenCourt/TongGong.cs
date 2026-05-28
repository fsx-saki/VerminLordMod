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
    public class TongGong : GuMasterBase
    {
        private int _attackTimer;
        private int _barrierCooldown;
        private bool _barrierActive;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/HeavenCourt/TongGong_Head";

        public override FactionID GetFaction() => FactionID.Heaven;
        public override GuRank GetRank() => GuRank.Zhuan8_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Steadfast;

        public override string GuMasterDisplayName => "铜公";
        public override int GuMasterDamage => 400;
        public override int GuMasterLife => 70000;
        public override int GuMasterDefense => 260;

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
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Ichor] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Slow] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Weak] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.BrokenArmor] = true;

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
            NPC.knockBackResist = 0f;
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
            _barrierCooldown = 0;
            _barrierActive = false;
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
                ExecuteMetalCombatAI();
            }

            BarrierUpdate();
        }

        private void BarrierUpdate()
        {
            if (_barrierCooldown > 0)
                _barrierCooldown--;

            if (_barrierActive && _barrierCooldown <= 0)
            {
                _barrierActive = false;
                NPC.defense = GuMasterDefense;
                CombatText.NewText(NPC.getRect(), Color.Silver, "金壁消散", false);
            }
        }

        private void ExecuteMetalCombatAI()
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
                NPC.velocity.Y = -6f;

            _attackTimer++;
            if (_attackTimer >= 45)
            {
                _attackTimer = 0;
                ExecuteMetalAttack(target);
            }
        }

        private void ExecuteMetalAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 5;

            int pattern = Main.rand.Next(4);
            switch (pattern)
            {
                case 0:
                    if (!_barrierActive)
                    {
                        _barrierActive = true;
                        _barrierCooldown = 300;
                        NPC.defense = GuMasterDefense + 200;
                        CombatText.NewText(NPC.getRect(), Color.Silver, "铜墙铁壁！", true);
                        for (int i = 0; i < 20; i++)
                        {
                            float angle = MathHelper.TwoPi / 20f * i;
                            Dust.NewDust(NPC.Center + angle.ToRotationVector2() * 80f, 10, 10,
                                DustID.SilverFlame, 0, 0, 100, Color.Silver, 2f);
                        }
                    }
                    else
                    {
                        for (int i = -2; i <= 2; i++)
                        {
                            float angle = toTarget.ToRotation() + i * 0.15f;
                            Vector2 velocity = angle.ToRotationVector2() * 10f;
                            Projectile.NewProjectile(source, NPC.Center, velocity,
                                ProjectileID.IchorSplash, damage, 2f, Main.myPlayer);
                        }
                    }
                    break;
                case 1:
                    CombatText.NewText(NPC.getRect(), Color.Silver, "金刃风暴！", true);
                    for (int i = 0; i < 16; i++)
                    {
                        float angle = MathHelper.TwoPi / 16f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 7f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.DemonScythe, damage, 2f, Main.myPlayer);
                    }
                    break;
                case 2:
                    for (int i = -3; i <= 3; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.12f;
                        Vector2 velocity = angle.ToRotationVector2() * 11f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.CultistBossIceMist, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 3:
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 8f,
                        ProjectileID.Meteor1, damage * 2, 4f, Main.myPlayer);
                    CombatText.NewText(NPC.getRect(), Color.Gray, "金锤破甲！", true);
                    target.AddBuff(BuffID.BrokenArmor, 300);
                    break;
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => HostileDialogue(),
                GuAttitude.Wary => "你的气息……不属于天庭。保持距离。",
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => "坚定的意志，如同金石。你值得尊重。",
                GuAttitude.Contemptuous => "软弱的意志，连铜壁都击不破。",
                GuAttitude.Fearful => "……不，天庭三公不会恐惧。",
                _ => "天庭之铜，坚不可摧。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "铜墙铁壁，天庭永固！",
                "金道之力，坚不可摧！",
                "你攻不破我的防线！",
                "天庭三公之名，岂是虚传？",
                "来吧！让我用金壁碾碎你！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "铜公：\"金道，讲究的是坚不可摧。无论是攻击还是防御，金道都是最可靠的力量。\"",
                "铜公：\"铜墙铁壁——这不只是杀招，更是我的信念。天庭的防线，永远不会被攻破。\"",
                "铜公：\"三公之中，我最为刚直。龙公有他的遗憾，眉公有他的算计，而我——只有坚守。\"",
                "铜公：\"金刃风暴，铜墙铁壁……金道的攻防一体，是天庭最稳固的基石。\"",
                "铜公：\"你问我会不会动摇？不会。金道修心，心如金石，万载不移。\"",
                "铜公：\"天庭历经万载风雨，靠的就是这份坚守。只要三公还在，天庭就不会倒。\"",
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
            var b = new DialogueTreeBuilder("HeavenCourt_TongGong", "greeting");

            b.StartNode("greeting",
                "铜公如同一座铁塔，浑身散发着金属般的光泽，目光坚定如钢。")
                .AddOption("询问金道", "ask_metal_dao", DialogueOptionType.Informative)
                .AddOption("关于天庭防御", "ask_heaven_defense", DialogueOptionType.Informative)
                .AddOption("关于三公", "ask_three_dukes", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_metal_dao",
                "铜公：\"金道，天地间最坚固的大道。金之刚猛，可破万物；金之坚韧，可御万法。攻防一体，这就是金道的精髓。\"")
                .AddOption("铜墙铁壁是怎么做到的？", "ask_barrier")
                .AddOption("金刃风暴的原理？", "ask_blade_storm")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_barrier",
                "铜公：\"铜墙铁壁——以金道真元凝聚成壁，防御力远超常理。但代价是行动受限，无法移动。攻守之间，必须做出选择。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_blade_storm",
                "铜公：\"金刃风暴——以金道真元化为无数利刃，向四面八方射出。这是金道最强的范围攻击，但消耗极大。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_heaven_defense",
                "铜公：\"天庭的防御，由我全权负责。三十六道金壁阵，覆盖天庭全境。任何入侵者，都要先过我这关。\"")
                .AddOption("有人攻破过吗？", "ask_breached")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_breached",
                "铜公的表情凝重：\"……红莲。宿命大战时，红莲以时间回溯之力，找到了金壁阵的破绽。那是我一生中唯一的失败。\"")
                .AddOption("你恨红莲吗？", "ask_hate_honglian")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_hate_honglian",
                "铜公：\"恨？不。红莲是一个值得尊敬的对手。他找到了我的破绽，这说明我的金壁还有不足。失败是进步的阶梯——这也是金道的修行。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_three_dukes",
                "铜公：\"三公——龙公、铜公、眉公。我们三人各修一道，共同辅佐天庭。龙公善变，我善守，眉公善柔。三道互补，天庭才能万载不衰。\"")
                .AddOption("三公之间有矛盾吗？", "ask_duke_conflict")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_duke_conflict",
                "铜公：\"矛盾？当然有。龙公有时过于激进，眉公有时过于柔和。但无论分歧多大，我们三公始终团结。因为——天庭的利益高于一切。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "铜公：\"交易？天庭的金属资源，可以与你交换。但记住——等价交换，这是金道的原则。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "铜公抱拳行礼：\"去吧。铜墙铁壁，天庭永固。\"")
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
            return new List<string> { "铜公" };
        }
    }
}
