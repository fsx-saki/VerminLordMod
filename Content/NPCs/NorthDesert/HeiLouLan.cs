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
    public class HeiLouLan : GuMasterBase
    {
        private int _attackTimer;
        private bool _baTiActive;
        private int _baTiCooldown;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.ChangShengTian;
        public override GuRank GetRank() => GuRank.Zhuan7_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Ambitious;

        public override string GuMasterDisplayName => "黑楼兰";
        public override int GuMasterDamage => 380;
        public override int GuMasterLife => 55000;
        public override int GuMasterDefense => 170;

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
            NPC.knockBackResist = 0.1f;
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
            _baTiActive = false;
            _baTiCooldown = 0;
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

            if (_baTiCooldown > 0) _baTiCooldown--;

            if (CurrentAttitude == GuAttitude.Hostile || (HasBeenHitByPlayer && AggroTimer > 0))
            {
                ExecuteCombatAI();
            }
        }

        private void ExecuteCombatAI()
        {
            NPC.TargetClosest(true);
            var target = Main.player[NPC.target];
            if (!target.active || target.dead) { NPC.velocity *= 0.95f; return; }

            float dist = Vector2.Distance(NPC.Center, target.Center);
            float dir = target.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = (int)dir;

            if (dist > 400f)
                NPC.velocity.X = dir * 3.5f;
            else
                NPC.velocity.X = dir * 2f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -8f;

            _attackTimer++;
            if (_attackTimer >= 40)
            {
                _attackTimer = 0;
                ExecuteAttack(target);
            }

            if (!_baTiActive && NPC.life < NPC.lifeMax * 0.4f && _baTiCooldown <= 0)
            {
                ActivateBaTi();
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
                    for (int i = -2; i <= 2; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.12f;
                        Vector2 vel = angle.ToRotationVector2() * 9f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.Meteor1, damage, 3f, Main.myPlayer);
                    }
                    break;
                case 1:
                    NPC.velocity.X += Math.Sign(target.Center.X - NPC.Center.X) * 6f;
                    NPC.velocity.Y = -5f;
                    CombatText.NewText(NPC.getRect(), Color.OrangeRed, "王庭之力——霸拳！", true);
                    break;
                case 2:
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 vel = angle.ToRotationVector2() * 6f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.BoulderStaffOfEarth, damage, 2f, Main.myPlayer);
                    }
                    break;
            }
        }

        private void ActivateBaTi()
        {
            _baTiActive = true;
            _baTiCooldown = 600;
            NPC.defense += 100;
            CombatText.NewText(NPC.getRect(), Color.Orange, "霸体——！", true);

            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.Torch,
                    Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f),
                    100, Color.Orange, 2f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi / 12f * i;
                    Vector2 vel = angle.ToRotationVector2() * 7f;
                    Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.CultistBossFireBall, NPC.damage / 5, 2f, Main.myPlayer);
                }
            }

            DelayedResetBaTi();
        }

        private void DelayedResetBaTi()
        {
            if (_baTiActive)
            {
                _baTiActive = false;
                NPC.defense = GuMasterDefense + (int)GetRank();
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => NumberOfTimesTalkedTo switch
                {
                    1 => "王庭之主，岂是浪得虚名？",
                    2 => "你敢挡我的路？不知死活！",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "哼，你在打什么主意？我黑楼兰可不是好骗的。",
                GuAttitude.Friendly => "你是巨阳大人的客人？那便以礼相待。",
                GuAttitude.Respectful => "有实力的人，值得我尊重。但北原的王座，我绝不会让。",
                GuAttitude.Contemptuous => "就凭你？也配与我一战？",
                GuAttitude.Fearful => "你……比我想象的更强……",
                _ => "王庭之主，岂是浪得虚名。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "我是巨阳仙尊的后裔，岂是你能挑衅的？",
                "力道碾压一切，你不过是螳臂当车！",
                "王庭之主的力量，你承受不起！",
                "霸体之下，万法皆破！",
                "北原的王者，绝不退缩！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("HeiLouLan", "greeting");

            b.StartNode("greeting", "黑楼兰傲然而立，浑身散发着力道强者的气息。")
                .AddOption("谈论北原局势", "north_desert", DialogueOptionType.Informative)
                .AddOption("关于巨阳仙尊", "ju_yang", DialogueOptionType.Informative)
                .AddOption("力道修炼", "li_dao", DialogueOptionType.Teach)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("north_desert", "黑楼兰：\"北原的局势？哼，长生天一统北原，这是巨阳大人的遗愿。任何敢于反抗的势力，都将被碾碎。\"")
                .AddOption("你与冰塞川的关系？", "bing_saichuan")
                .AddOption("北原的威胁？", "north_threats")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bing_saichuan", "黑楼兰面色微变：\"冰塞川……他是长生天的领袖，我尊重他的地位。但北原的王座，终将由我来坐。\"")
                .AddOption("你有野心取代他？", "ambition")
                .AddOption("回到北原局势", "north_desert");

            b.StartNode("ambition", "黑楼兰冷笑：\"野心？这不是野心，这是实力。巨阳大人的血脉在我体内流淌，王庭之主的位置，非我莫属。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("north_threats", "黑楼兰：\"北原的威胁？天庭一直觊觎北原，影宗也在暗中活动。但在我的霸体面前，一切威胁都不足为惧。\"")
                .AddOption("天庭的阴谋？", "heaven_plot")
                .AddOption("回到北原局势", "north_desert");

            b.StartNode("heaven_plot", "黑楼兰：\"天庭……他们想用宿命蛊控制一切。但宿命已被方源击碎，天庭的好日子到头了。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ju_yang", "黑楼兰肃然起敬：\"巨阳仙尊……他是运道尊者，创建了长生天。作为他的后裔，我肩负着延续荣耀的使命。\"")
                .AddOption("运道的力量？", "luck_power")
                .AddOption("你的血脉传承", "bloodline")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("luck_power", "黑楼兰：\"运道……祸福相依，吉凶同体。巨阳大人能将大凶化为大吉，这是运道的极致。而我，继承了这份力量的冰山一角。\"")
                .AddOption("回到巨阳仙尊", "ju_yang");

            b.StartNode("bloodline", "黑楼兰：\"巨阳大人的后裔遍布北原，这是'家天下'的体现。但在我心中，只有最强的后裔才配坐上王座。\"")
                .AddOption("回到巨阳仙尊", "ju_yang");

            b.StartNode("li_dao", "黑楼兰：\"力道？你想了解力道？力道是最直接的道——以力破万法，以力证大道。我的霸体，便是力道的极致体现之一。\"")
                .AddOption("霸体是什么？", "ba_ti")
                .AddOption("力道与其他道的区别？", "li_vs_others")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ba_ti", "黑楼兰：\"霸体是力道高阶修士的特殊状态，激活后肉身坚不可摧，万法难侵。但维持霸体消耗极大，不能持久。\"")
                .AddOption("回到力道修炼", "li_dao");

            b.StartNode("li_vs_others", "黑楼兰：\"力道不讲花哨，不像音道、幻道那般虚虚实实。力道就是——你一拳打过来，我硬接；我一拳打回去，你死。简单，直接，有效。\"")
                .AddOption("回到力道修炼", "li_dao");

            b.StartNode("trade", "黑楼兰：\"交易？可以。长生天不缺资源，但你必须拿出等价的东西。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "黑楼兰微微点头：\"去吧。记住，北原的王座，只能有一个主人。\"")
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

        public override List<string> SetNPCNameList() => new List<string> { "黑楼兰" };
    }
}
