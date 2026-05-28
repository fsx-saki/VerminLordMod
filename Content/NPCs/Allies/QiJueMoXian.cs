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

namespace VerminLordMod.Content.NPCs.Allies
{
    [AutoloadHead]
    public class QiJueMoXian : GuMasterBase
    {
        private int _attackTimer;
        private int _qiPressureTimer;
        private bool _qiJueZoneActive;
        private int _qiJueCooldown;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.Zhuan8_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Fierce;

        public override string GuMasterDisplayName => "气绝魔仙";
        public override int GuMasterDamage => 400;
        public override int GuMasterLife => 62000;
        public override int GuMasterDefense => 190;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 1500;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 20;
            NPCID.Sets.AttackAverageChance[Type] = 10;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Suffocation] = true;

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
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(3, 20, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _qiPressureTimer = 0;
            _qiJueZoneActive = false;
            _qiJueCooldown = 0;
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

            if (_qiJueCooldown > 0) _qiJueCooldown--;

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

            NPC.velocity.X = dir * 2.5f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -7f;

            _attackTimer++;
            if (_attackTimer >= 30)
            {
                _attackTimer = 0;
                ExecuteAttack(target);
            }

            _qiPressureTimer++;
            if (_qiPressureTimer >= 180)
            {
                _qiPressureTimer = 0;
                QiPressureAttack(target);
            }

            if (!_qiJueZoneActive && NPC.life < NPC.lifeMax * 0.4f && _qiJueCooldown <= 0)
            {
                ActivateQiJueZone();
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
                    for (int i = -4; i <= 4; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.07f;
                        Vector2 vel = angle.ToRotationVector2() * 11f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.DD2BetsyFireball, damage, 2f, Main.myPlayer);
                    }
                    break;
                case 1:
                    for (int i = 0; i < 16; i++)
                    {
                        float angle = MathHelper.TwoPi / 16f * i;
                        Vector2 vel = angle.ToRotationVector2() * 6f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.CultistBossLightningOrb, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 2:
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 14f,
                        ProjectileID.DeathLaser, damage * 2, 3f, Main.myPlayer);
                    break;
            }
        }

        private void QiPressureAttack(Player target)
        {
            CombatText.NewText(NPC.getRect(), Color.Purple, "气压迫压！", true);

            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            var source = NPC.GetSource_FromAI();
            int damage = NPC.damage / 5;

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8f * i;
                Vector2 spawnPos = target.Center + angle.ToRotationVector2() * 200f;
                Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.UnitY) * 5f;
                Projectile.NewProjectile(source, spawnPos, vel, ProjectileID.CultistBossLightningOrb, damage, 2f, Main.myPlayer);
            }

            target.AddBuff(BuffID.Slow, 150);
        }

        private void ActivateQiJueZone()
        {
            _qiJueZoneActive = true;
            _qiJueCooldown = 900;
            CombatText.NewText(NPC.getRect(), Color.Purple, "气绝——！", true);

            for (int i = 0; i < 40; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.PurpleTorch,
                    Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f),
                    100, Color.DarkViolet, 2.5f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                int damage = NPC.damage / 3;

                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi / 20f * i;
                    Vector2 vel = angle.ToRotationVector2() * 7f;
                    Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.ShadowFlame, damage, 2f, Main.myPlayer);
                }
            }

            foreach (Player p in Main.player)
            {
                if (p.active && !p.dead && Vector2.Distance(NPC.Center, p.Center) < 500f)
                {
                    p.AddBuff(BuffID.Suffocation, 180);
                    p.AddBuff(BuffID.Slow, 240);
                    p.AddBuff(BuffID.Weak, 180);
                }
            }

            _qiJueZoneActive = false;
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => NumberOfTimesTalkedTo switch
                {
                    1 => "无极大人的护道人，岂是等闲？",
                    2 => "你竟敢冒犯无极大人？找死！",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "你的气息……让我警惕。但如果你不是无极大人的敌人，我不会为难你。",
                GuAttitude.Friendly => "你似乎不是坏人。但在我面前，最好坦诚相待。",
                GuAttitude.Respectful => "你的实力……值得我认真对待。",
                GuAttitude.Contemptuous => "弱者不配与我对话。",
                GuAttitude.Fearful => "你……竟比我还强……无极大人，恕我无能……",
                _ => "无极大人的遗志，由我来守护。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "气绝之下，万物窒息！",
                "我是无极大人的护道人，谁敢来犯！",
                "气道之力，碾压一切！",
                "你连呼吸的权利，都在我的掌控之中！",
                "气绝魔仙的名号，是用无数尸骨堆出来的！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("QiJueMoXian", "greeting");

            b.StartNode("greeting", "气绝魔仙浑身散发着无形的气压，令人呼吸都变得沉重。")
                .AddOption("谈论气道", "qi_dao", DialogueOptionType.Teach)
                .AddOption("关于无极魔尊", "wu_ji", DialogueOptionType.Informative)
                .AddOption("护道人的职责", "guardian_duty", DialogueOptionType.Informative)
                .AddOption("关于西帝", "xi_di", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("qi_dao", "气绝魔仙：\"气道？操控天地之气，便是气道。气无处不在——空气、灵气、真元……都在气道的范畴之内。而我的'气绝'，便是剥夺一切气。\"")
                .AddOption("气绝的原理？", "qi_jue_principle")
                .AddOption("气道与战斗？", "qi_combat")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("qi_jue_principle", "气绝魔仙：\"气绝的原理很简单——抽空目标周围的一切气体。没有空气，就无法呼吸；没有灵气，就无法施展蛊术；没有真元，就无法战斗。这就是气绝的恐怖之处。\"")
                .AddOption("回到气道", "qi_dao");

            b.StartNode("qi_combat", "气绝魔仙：\"气道在战斗中的优势？气压！以庞大的气压碾压敌人，让他们的身体承受不住。一气大手爆，足以毁天灭地。\"")
                .AddOption("回到气道", "qi_dao");

            b.StartNode("wu_ji", "气绝魔仙目光柔和了一瞬：\"无极大人……他是我的主人，也是我唯一敬重的人。我愿为他赴汤蹈火，在所不辞。\"")
                .AddOption("你为何如此忠诚？", "loyalty_reason")
                .AddOption("无极魔尊现在何处？", "wu_ji_now")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("loyalty_reason", "气绝魔仙：\"为何忠诚？因为无极大人给了我新生。在我最绝望的时候，是他收留了我，传授我气道。这份恩情，我以命相报。\"")
                .AddOption("回到无极魔尊", "wu_ji");

            b.StartNode("wu_ji_now", "气绝魔仙沉默片刻：\"无极大人……他已经不在了。但他的遗志，由我来守护。西帝是他的传人，我会守护西帝，就像守护无极大人一样。\"")
                .AddOption("回到无极魔尊", "wu_ji");

            b.StartNode("xi_di", "气绝魔仙：\"西帝……无极大人的传人。我受无极大人之托，守护西帝成长。虽然西帝的性子有些……特别，但他的天赋毋庸置疑。\"")
                .AddOption("西帝的实力？", "xi_di_power")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("xi_di_power", "气绝魔仙：\"西帝的实力？他还年轻，但潜力无限。假以时日，他必能超越无极大人。而我的职责，就是确保他能活到那一天。\"")
                .AddOption("回到西帝", "xi_di");

            b.StartNode("guardian_duty", "气绝魔仙：\"护道人？这是蛊世界中一种特殊的关系。强者护道弱者成长，直到弱者能够独当一面。我是无极大人的护道人，现在也是西帝的护道人。\"")
                .AddOption("护道人的风险？", "guardian_risk")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("guardian_risk", "气绝魔仙：\"风险？当然有。护道人往往要面对比被护者更强的敌人。但这就是护道人的使命——用自己的命，换被护者的生。\"")
                .AddOption("回到护道人", "guardian_duty");

            b.StartNode("trade", "气绝魔仙：\"交易？可以。但别想从我这里占便宜。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "气绝魔仙冷冷地点头：\"去吧。别让我后悔放你走。\"")
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

        public override List<string> SetNPCNameList() => new List<string> { "气绝魔仙" };
    }
}
