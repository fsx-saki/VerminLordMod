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
    public class MaoLiQiu : GuMasterBase
    {
        private int _attackTimer;
        private int _transformTimer;
        private bool _mirroredAttack;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.ChangShengTian;
        public override GuRank GetRank() => GuRank.AncientLegendary;
        public override GuPersonality GetPersonality() => GuPersonality.Wild;

        public override string GuMasterDisplayName => "毛里球";
        public override int GuMasterDamage => 360;
        public override int GuMasterLife => 60000;
        public override int GuMasterDefense => 250;

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
            NPC.knockBackResist = 0.05f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(2, 50, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _transformTimer = 0;
            _mirroredAttack = false;
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
                NPC.velocity.Y = -9f;

            _attackTimer++;
            if (_attackTimer >= 35)
            {
                _attackTimer = 0;
                ExecuteAttack(target);
            }

            _transformTimer++;
            if (_transformTimer >= 600)
            {
                _transformTimer = 0;
                ExecuteTransformation(target);
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
                    for (int i = -3; i <= 3; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.1f;
                        Vector2 vel = angle.ToRotationVector2() * 10f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.DD2PhoenixBowShot, damage, 2f, Main.myPlayer);
                    }
                    if (_mirroredAttack)
                    {
                        for (int i = -3; i <= 3; i++)
                        {
                            float angle = toTarget.ToRotation() + i * 0.1f + 0.05f;
                            Vector2 vel = angle.ToRotationVector2() * 10f;
                            Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.DD2PhoenixBowShot, damage, 2f, Main.myPlayer);
                        }
                        _mirroredAttack = false;
                    }
                    break;
                case 1:
                    for (int i = 0; i < 10; i++)
                    {
                        float angle = MathHelper.TwoPi / 10f * i;
                        Vector2 vel = angle.ToRotationVector2() * 7f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.SaucerMissile, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 2:
                    CombatText.NewText(NPC.getRect(), Color.Purple, "成双入对！", true);
                    _mirroredAttack = true;
                    Vector2 mirrorVel = toTarget.SafeNormalize(Vector2.UnitY) * 12f;
                    Projectile.NewProjectile(source, NPC.Center, mirrorVel, ProjectileID.CultistBossLightningOrb, damage * 2, 3f, Main.myPlayer);
                    Projectile.NewProjectile(source, NPC.Center, mirrorVel.RotatedBy(0.15f), ProjectileID.CultistBossLightningOrb, damage * 2, 3f, Main.myPlayer);
                    break;
            }
        }

        private void ExecuteTransformation(Player target)
        {
            CombatText.NewText(NPC.getRect(), Color.Purple, "变化道——化形！", true);

            for (int i = 0; i < 25; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.PurpleTorch,
                    Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f),
                    100, Color.Purple, 1.8f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                int damage = NPC.damage / 3;
                for (int i = 0; i < 16; i++)
                {
                    float angle = MathHelper.TwoPi / 16f * i;
                    Vector2 vel = angle.ToRotationVector2() * 6f;
                    Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.ShadowFlame, damage, 2f, Main.myPlayer);
                }
            }

            NPC.damage = (int)(NPC.defDamage * 1.3f);
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => NumberOfTimesTalkedTo switch
                {
                    1 => "（兽语，不可对话，但可交互）",
                    2 => "（毛里球发出愤怒的咆哮！）",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "（毛里球警惕地注视着你……）",
                GuAttitude.Friendly => "（毛里球似乎接受了你的存在。）",
                GuAttitude.Respectful => "（毛里球对你表示了敬意。）",
                GuAttitude.Contemptuous => "（毛里球不屑地哼了一声。）",
                GuAttitude.Fearful => "（毛里球发出了痛苦的哀鸣……）",
                _ => "（兽语，不可对话，但可交互）"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "变化万千，你永远猜不到我的真身！",
                "成双入对，你的攻击我加倍奉还！",
                "巨阳大人的荣光，由我来守护！",
                "太古传奇的力量，岂是凡人能抗衡的？",
                "我毛里球，绝不让巨阳大人蒙羞！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("MaoLiQiu", "greeting");

            b.StartNode("greeting", "毛里球傲然而立，身上散发着太古传奇的气息。")
                .AddOption("谈论变化道", "bianhua_dao", DialogueOptionType.Teach)
                .AddOption("关于巨阳仙尊", "ju_yang", DialogueOptionType.Informative)
                .AddOption("成双入对之术", "mirror_skill", DialogueOptionType.Special)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("bianhua_dao", "毛里球：\"变化道？这是我修炼的道。变化万千，化形无数——这就是变化道的精髓。\"")
                .AddOption("变化道的极限？", "bianhua_limit")
                .AddOption("你可以变成什么？", "bianhua_forms")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bianhua_limit", "毛里球：\"变化道的极限？传说中，变化道的极致可以化为天地万物，甚至化为另一个人的模样，连修为都能模仿。但那需要九转变化道蛊虫。\"")
                .AddOption("回到变化道", "bianhua_dao");

            b.StartNode("bianhua_forms", "毛里球：\"我能化为飞禽走兽，也能化为人形。但无论变成什么，我的忠诚不会改变——我永远是巨阳大人的坐骑。\"")
                .AddOption("回到变化道", "bianhua_dao");

            b.StartNode("ju_yang", "毛里球目光柔和：\"巨阳大人……他是运道尊者，创建了长生天。我追随他无数岁月，见证了北原的兴衰。他的意志，就是我行动的方向。\"")
                .AddOption("你追随他多久了？", "follow_time")
                .AddOption("巨阳仙尊现在何处？", "ju_yang_now")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("follow_time", "毛里球：\"多久？……久到我已经记不清了。从巨阳大人还是凡人的时候，我就跟在他身边。这份忠诚，跨越了无数岁月。\"")
                .AddOption("回到巨阳仙尊", "ju_yang");

            b.StartNode("ju_yang_now", "毛里球沉默片刻：\"巨阳大人……他已经不在了。但他的遗志，由长生天继承。而我，会守护他的遗产，直到最后一刻。\"")
                .AddOption("回到巨阳仙尊", "ju_yang");

            b.StartNode("mirror_skill", "毛里球：\"成双入对？这是我的独门绝技。每一击都能镜像复制，双倍攻击，让敌人防不胜防。这是变化道与战斗结合的产物。\"")
                .AddOption("如何修炼此术？", "mirror_learn")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("mirror_learn", "毛里球：\"修炼此术需要变化道的天赋，以及……一双能看穿万物本质的眼睛。不是所有人都能学会的。\"")
                .AddOption("回到成双入对", "mirror_skill");

            b.StartNode("trade", "毛里球：\"交易？可以。太古传奇的收藏，可不是寻常货色。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "毛里球点头：\"去吧。记住，巨阳大人的荣光，永世不灭。\"")
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

        public override List<string> SetNPCNameList() => new List<string> { "毛里球" };
    }
}
