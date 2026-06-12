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
    public class ZhengYuanLaoRen : GuMasterBase
    {
        private int _attackTimer;
        private int _surveillanceTimer;
        private bool _surveillanceActive;
        private int _surveillanceDuration;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";
        public override string HeadTexture => "VerminLordMod/Content/NPCs/ShadowSect/ZhengYuanLaoRen_Head";

        public override FactionID GetFaction() => FactionID.ShadowSect;
        public override GuRank GetRank() => GuRank.Zhuan8_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Calculating;

        public override string GuMasterDisplayName => "正元老人";
        public override int GuMasterDamage => 330;
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
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
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
            _surveillanceTimer = 0;
            _surveillanceActive = false;
            _surveillanceDuration = 0;
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
                ExecuteHumanCombatAI();
            }

            SurveillanceUpdate();
        }

        private void SurveillanceUpdate()
        {
            if (_surveillanceActive)
            {
                _surveillanceDuration++;
                if (_surveillanceDuration > 360)
                {
                    _surveillanceActive = false;
                    _surveillanceDuration = 0;
                    CombatText.NewText(NPC.getRect(), Color.Yellow, "众目睽睽消散", false);
                }
                else
                {
                    var target = Main.player[NPC.target];
                    if (target.active && !target.dead)
                    {
                        float dist = Vector2.Distance(NPC.Center, target.Center);
                        if (dist < 500f)
                        {
                            target.AddBuff(BuffID.Slow, 30);
                            target.AddBuff(BuffID.Weak, 30);
                            target.AddBuff(BuffID.BrokenArmor, 30);
                        }
                    }
                }
            }
        }

        private void ExecuteHumanCombatAI()
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
            _surveillanceTimer++;

            if (_attackTimer >= 40)
            {
                _attackTimer = 0;
                ExecuteHumanAttack(target);
            }

            if (_surveillanceTimer >= 240)
            {
                _surveillanceTimer = 0;
                ActivateSurveillance(target);
            }
        }

        private void ExecuteHumanAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 5;

            int pattern = Main.rand.Next(4);
            switch (pattern)
            {
                case 0:
                    CombatText.NewText(NPC.getRect(), Color.Yellow, "千夫所指！", true);
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi / 12f * i;
                        Vector2 velocity = angle.ToRotationVector2() * 7f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.CultistBossLightningOrb, damage, 1f, Main.myPlayer);
                    }
                    target.AddBuff(BuffID.Cursed, 120);
                    break;
                case 1:
                    for (int i = -3; i <= 3; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.15f;
                        Vector2 velocity = angle.ToRotationVector2() * 10f;
                        Projectile.NewProjectile(source, NPC.Center, velocity,
                            ProjectileID.HolyWater, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 2:
                    _surveillanceActive = true;
                    _surveillanceDuration = 0;
                    CombatText.NewText(NPC.getRect(), Color.Yellow, "众目睽睽——无处遁形！", true);
                    for (int i = 0; i < 20; i++)
                    {
                        float angle = MathHelper.TwoPi / 20f * i;
                        Dust.NewDust(NPC.Center + angle.ToRotationVector2() * 250f, 10, 10,
                            DustID.YellowTorch, 0, -2f, 100, Color.Yellow, 1.5f);
                    }
                    break;
                case 3:
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 12f,
                        ProjectileID.CultistBossIceMist, damage * 2, 2f, Main.myPlayer);
                    target.AddBuff(BuffID.Slow, 180);
                    break;
            }
        }

        private void ActivateSurveillance(Player target)
        {
            _surveillanceActive = true;
            _surveillanceDuration = 0;

            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(target.Hitbox, Color.Yellow, "众目睽睽！", true);
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDustDirect(target.position, target.width, target.height,
                        DustID.YellowTorch, 0, -3f, Scale: 1.5f);
                }
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => HostileDialogue(),
                GuAttitude.Wary => "你似乎看出了什么？不，不可能。我的伪装天衣无缝。",
                GuAttitude.Friendly => FriendlyDialogue(),
                GuAttitude.Respectful => "你的眼力不错……但看破不说破，才是聪明人。",
                GuAttitude.Contemptuous => "蝼蚁之辈，不配与我对话。",
                GuAttitude.Fearful => "……你比我想象的要危险。也许我该重新评估你。",
                _ => "人心所向，便是天道。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "正道……呵，不过是另一种伪装！",
                "千夫所指——你的罪行，天下皆知！",
                "你以为天庭是正义的？天真！",
                "众目睽睽之下，你无处遁形！",
                "人道之力，在于人心！而人心，最容易被操控！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        private string FriendlyDialogue()
        {
            var dialogues = new List<string>
            {
                "正元老人：\"正道……呵，不过是另一种伪装。在天庭，我以人道大师的身份行走，谁也不知道我的真面目。\"",
                "正元老人：\"人道，操控人心之力。千夫所指，可以毁掉任何人的名誉；众目睽睽，可以让任何人无处遁形。这就是人道的可怕之处。\"",
                "正元老人：\"我在天庭潜伏了数百年，深受信任。天庭的每一次决策，我都有参与。而每一次，我都在暗中为影宗谋利。\"",
                "正元老人：\"你问我会不会愧疚？……不会。天庭的'正义'本就是虚伪的。我只是以虚伪对虚伪，以假面换假面。\"",
                "正元老人：\"人道最厉害的地方，不是攻击，而是——操控舆论。当所有人都说你错了，你就真的错了。这就是千夫所指的力量。\"",
                "正元老人：\"天庭以为我是他们的人，影宗知道我是他们的人。但真正的问题是——我是谁的人？也许……我只忠于自己。\"",
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
            var b = new DialogueTreeBuilder("ShadowSect_ZhengYuanLaoRen", "greeting");

            b.StartNode("greeting",
                "正元老人面容慈祥，目光温和，但眼底深处隐藏着不可捉摸的深意。")
                .AddOption("询问人道", "ask_human_dao", DialogueOptionType.Informative)
                .AddOption("关于天庭", "ask_heaven", DialogueOptionType.Risky)
                .AddOption("关于影宗", "ask_shadow_sect", DialogueOptionType.Risky)
                .AddOption("关于伪装", "ask_disguise", DialogueOptionType.Risky)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ask_human_dao",
                "正元老人：\"人道，操控人心之力。人心是世间最复杂的东西，也是最容易被操控的东西。千夫所指，可以毁掉任何人；众目睽睽，可以让任何人无处遁形。\"")
                .AddOption("千夫所指是什么？", "ask_crowd_condemn")
                .AddOption("众目睽睽呢？", "ask_surveillance")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_crowd_condemn",
                "正元老人：\"千夫所指——以人道之力，操控众人的意志，将矛头指向一人。被千夫所指者，不仅会遭受精神上的打击，更会被众人的力量所压制。这就是人道的可怕——杀人不用刀。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_surveillance",
                "正元老人：\"众目睽睽——以人道之力，创造无数'眼睛'监视目标。被监视者会感到无处不在的压力，力量减弱，防御降低。在众目睽睽之下，没有人能发挥全部实力。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_heaven",
                "正元老人露出慈祥的微笑：\"天庭？天庭是蛊世界的守护者，正义的化身。我在天庭服务数百年，深受信任……\"他的眼神闪过一丝不易察觉的冷意。")
                .AddOption("你在天庭做什么？", "ask_heaven_role")
                .AddOption("你真的忠于天庭吗？", "ask_loyalty", DialogueOptionType.Risky)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_heaven_role",
                "正元老人：\"我在天庭负责人道事务，为天庭出谋划策。天庭的许多决策，都有我的参与。这也是我……的价值所在。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_loyalty",
                "正元老人的笑容不变，但眼中的温度骤降：\"忠于天庭？当然。我正元老人，一生忠于天庭，天地可鉴。\"他的语气平静得令人不寒而栗。")
                .AddOption("……我明白了", "greeting")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_shadow_sect",
                "正元老人微微一怔，随即恢复平静：\"影宗？那是天庭的敌人，蛊世界的毒瘤。我正元老人与影宗势不两立。\"他说话时的表情，完美得像是在演戏。")
                .AddOption("你知道影宗的渗透吗？", "ask_infiltration", DialogueOptionType.Risky)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_infiltration",
                "正元老人：\"渗透？天庭的防御固若金汤，影宗怎么可能渗透进来？你多虑了。\"他的目光在你身上停留了一瞬，似乎在评估你知道多少。")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_disguise",
                "正元老人沉默了片刻，然后低声说：\"伪装？……你很敏锐。在这个世界上，每个人都在伪装。天庭伪装正义，影宗伪装邪恶。而我……我伪装成他们需要的样子。\"")
                .AddOption("你的真面目是什么？", "ask_true_face")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ask_true_face",
                "正元老人苦笑：\"真面目？……我已经不记得了。伪装了太久，连自己都分不清哪个是真的。也许……这就是人道的代价——当你能操控所有人的心时，你已经失去了自己的心。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("trade",
                "正元老人：\"交易？当然可以。天庭的资源，我都可以为你调配……当然，需要等价交换。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye",
                "正元老人微微颔首：\"去吧。记住——在这个世界上，看到的未必是真的，看不到的未必是假的。\"")
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
            return new List<string> { "正元老人" };
        }
    }
}
