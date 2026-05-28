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
    public class TaiBaiYunSheng : GuMasterBase
    {
        private int _attackTimer;
        private int _healTimer;
        private bool _timeFreezeDefense;
        private int _timeFreezeCooldown;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.Zhuan6_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Benevolent;

        public override string GuMasterDisplayName => "太白云生";
        public override int GuMasterDamage => 200;
        public override int GuMasterLife => 35000;
        public override int GuMasterDefense => 130;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 800;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 35;
            NPCID.Sets.AttackAverageChance[Type] = 25;
            NPCID.Sets.HatOffsetY[Type] = 4;
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
            NPC.knockBackResist = 0.25f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(0, 80, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _healTimer = 0;
            _timeFreezeDefense = false;
            _timeFreezeCooldown = 0;
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

            if (_timeFreezeCooldown > 0) _timeFreezeCooldown--;

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

            NPC.velocity.X = dir * 1.5f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -5f;

            _attackTimer++;
            if (_attackTimer >= 50)
            {
                _attackTimer = 0;
                ExecuteAttack(target);
            }

            _healTimer++;
            if (_healTimer >= 120)
            {
                _healTimer = 0;
                TimeHeal();
            }

            if (!_timeFreezeDefense && NPC.life < NPC.lifeMax * 0.25f && _timeFreezeCooldown <= 0)
            {
                ActivateTimeFreezeDefense();
            }
        }

        private void ExecuteAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 3;

            int pattern = Main.rand.Next(2);
            switch (pattern)
            {
                case 0:
                    for (int i = -2; i <= 2; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.12f;
                        Vector2 vel = angle.ToRotationVector2() * 8f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.WaterBolt, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 1:
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 10f,
                        ProjectileID.CultistBossIceMist, damage, 2f, Main.myPlayer);
                    break;
            }
        }

        private void TimeHeal()
        {
            int healAmount = NPC.lifeMax / 15;
            NPC.life = Math.Min(NPC.life + healAmount, NPC.lifeMax);
            NPC.HealEffect(healAmount, true);

            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.BlueTorch,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f),
                    100, Color.LightBlue, 1.2f);
            }
        }

        private void ActivateTimeFreezeDefense()
        {
            _timeFreezeDefense = true;
            _timeFreezeCooldown = 900;
            CombatText.NewText(NPC.getRect(), Color.LightBlue, "山如故——江如故！", true);

            NPC.defense += 80;

            for (int i = 0; i < 25; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.IceRod,
                    Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f),
                    100, Color.LightBlue, 2f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi / 8f * i;
                    Vector2 vel = angle.ToRotationVector2() * 4f;
                    Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.IceBlock, NPC.damage / 5, 1f, Main.myPlayer);
                }
            }

            _timeFreezeDefense = false;
            NPC.defense = GuMasterDefense + (int)GetRank();
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => NumberOfTimesTalkedTo switch
                {
                    1 => "治世救人，方为正道。",
                    2 => "师弟……你到底在追求什么？",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "你的眼神……让我想起了某些人。希望你不是那种人。",
                GuAttitude.Friendly => "道友请了，太白云生愿与您论道救人。",
                GuAttitude.Respectful => "道友心怀天下，令云生敬佩。",
                GuAttitude.Contemptuous => "你……唉，何必如此？",
                GuAttitude.Fearful => "我……不是你的对手……但我的信念不会动摇……",
                _ => "治世救人，方为正道。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "宙道之力，亦可救人，亦可退敌！",
                "山如故，江如故——时间不会因你而停！",
                "我不想伤害你，但你也不要逼我！",
                "为何要互相残杀？和平共处不好吗？",
                "我的医术，也能化为攻击……可惜了。",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("TaiBaiYunSheng", "greeting");

            b.StartNode("greeting", "太白云生面带微笑，温和的目光中透着一丝悲伤。")
                .AddOption("谈论宙道", "zhou_dao", DialogueOptionType.Teach)
                .AddOption("医道救人", "healing", DialogueOptionType.Teach)
                .AddOption("关于方源", "fang_yuan", DialogueOptionType.Informative)
                .AddOption("你的命运", "my_fate", DialogueOptionType.Special)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("zhou_dao", "太白云生：\"宙道？我修炼宙道，主要用于救人。时间可以加速伤口愈合，也可以让毒发作变慢。宙道的本质是——掌控时间的流速。\"")
                .AddOption("宙道如何救人？", "zhou_heal")
                .AddOption("山如故江如故？", "mountain_river")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("zhou_heal", "太白云生：\"宙道救人的原理很简单——加速身体的时间流速，让伤口以十倍百倍的速度愈合。但这也有代价，会消耗大量真元。\"")
                .AddOption("回到宙道", "zhou_dao");

            b.StartNode("mountain_river", "太白云生：\"山如故、江如故……这是我的防御绝招。以宙道冻结时间，让一切攻击无法触及我。山还是那座山，江还是那条江——时间在我面前，静止了。\"")
                .AddOption("回到宙道", "zhou_dao");

            b.StartNode("fang_yuan", "太白云生叹了口气：\"方源……我曾经以为他是朋友。他救过我，我也救过他。但最终……他利用了我。他利用了所有人的善意。\"")
                .AddOption("他怎么利用你的？", "fang_yuan_used")
                .AddOption("你还恨他吗？", "fang_yuan_hate")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("fang_yuan_used", "太白云生：\"他……在需要我的时候对我百般友好，在不需要的时候……呵。我太天真了，以为善意可以感化任何人。但方源……他没有心。\"")
                .AddOption("回到方源", "fang_yuan");

            b.StartNode("fang_yuan_hate", "太白云生沉默良久：\"恨？……也许吧。但更多的是悲哀。为他的冷酷悲哀，也为自己的天真悲哀。但我不后悔——善良不是弱点，是选择。\"")
                .AddOption("回到方源", "fang_yuan");

            b.StartNode("my_fate", "太白云生的眼中闪过一丝哀伤：\"我的命运？……我知道，我的结局不会好。但即使如此，我也不会改变自己的道。治世救人，这是我的信念。\"")
                .AddOption("你知道自己的结局？", "know_fate")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("know_fate", "太白云生苦笑：\"宙道修士对时间敏感，我隐约看到了自己的结局……被最信任的人所害。但我不愿相信，也不愿改变。因为——善良不应该因为恐惧而放弃。\"")
                .AddOption("……保重", "greeting");

            b.StartNode("healing", "太白云生：\"医道？我虽非专修医道，但宙道与医道结合，可以做到许多医道蛊师做不到的事。比如——让时间倒流，消除伤口。\"")
                .AddOption("你能治疗我吗？", "heal_me")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("heal_me", "太白云生微笑：\"当然可以。来，让我为你疗伤。\"")
                .AddOption("多谢", "greeting");

            b.StartNode("trade", "太白云生：\"交易？可以。我这里有些宙道和医道的材料，或许对你有用。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "太白云生拱手：\"道友保重。愿天下无病，世间无灾。\"")
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

        public override List<string> SetNPCNameList() => new List<string> { "太白云生" };
    }
}
