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
    public class BingSaiChuan : GuMasterBase
    {
        private int _attackTimer;
        private int _freezeZoneTimer;
        private bool _timeFreezeActive;
        private int _timeFreezeCooldown;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.ChangShengTian;
        public override GuRank GetRank() => GuRank.Zhuan8_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Cold;

        public override string GuMasterDisplayName => "冰塞川";
        public override int GuMasterDamage => 400;
        public override int GuMasterLife => 70000;
        public override int GuMasterDefense => 200;

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
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frozen] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Chilled] = true;
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
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(3, 0, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _freezeZoneTimer = 0;
            _timeFreezeActive = false;
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

            if (dist > 500f)
                NPC.velocity.X = dir * 3f;
            else if (dist > 200f)
                NPC.velocity.X = dir * 1.5f;
            else
                NPC.velocity.X = dir * 0.8f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -7f;

            _attackTimer++;
            if (_attackTimer >= 35)
            {
                _attackTimer = 0;
                ExecuteAttack(target);
            }

            _freezeZoneTimer++;
            if (_freezeZoneTimer >= 180)
            {
                _freezeZoneTimer = 0;
                SpawnFrozenZone(target);
            }

            if (!_timeFreezeActive && NPC.life < NPC.lifeMax * 0.3f && _timeFreezeCooldown <= 0)
            {
                ActivateTimeFreeze();
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
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.IceBolt, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 1:
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi / 12f * i;
                        Vector2 vel = angle.ToRotationVector2() * 5f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.FrostDaggerfish, damage, 2f, Main.myPlayer);
                    }
                    break;
                case 2:
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 14f,
                        ProjectileID.Blizzard, damage * 2, 4f, Main.myPlayer);
                    break;
            }
        }

        private void SpawnFrozenZone(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            var source = NPC.GetSource_FromAI();
            Vector2 zoneCenter = target.Center + new Vector2(Main.rand.Next(-200, 200), Main.rand.Next(-100, 100));
            int damage = NPC.damage / 6;

            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi / 6f * i;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                Projectile.NewProjectile(source, zoneCenter, vel, ProjectileID.IceBlock, damage, 1f, Main.myPlayer);
            }

            target.AddBuff(BuffID.Chilled, 180);
            CombatText.NewText(NPC.getRect(), Color.LightCyan, "冰封领域！", true);

            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(zoneCenter, 20, 20, DustID.Ice,
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
                    100, Color.LightCyan, 1.5f);
            }
        }

        private void ActivateTimeFreeze()
        {
            _timeFreezeActive = true;
            _timeFreezeCooldown = 900;
            CombatText.NewText(NPC.getRect(), Color.Cyan, "宙道——时间冻结！", true);

            for (int i = 0; i < 40; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.IceRod,
                    Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f),
                    100, Color.Cyan, 2.5f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                for (int i = 0; i < 16; i++)
                {
                    float angle = MathHelper.TwoPi / 16f * i;
                    Vector2 vel = angle.ToRotationVector2() * 8f;
                    Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.CultistBossIceMist, NPC.damage / 3, 2f, Main.myPlayer);
                }
            }

            foreach (Player p in Main.player)
            {
                if (p.active && !p.dead && Vector2.Distance(NPC.Center, p.Center) < 600f)
                {
                    p.AddBuff(BuffID.Frozen, 120);
                    p.AddBuff(BuffID.Slow, 180);
                }
            }

            _timeFreezeActive = false;
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => NumberOfTimesTalkedTo switch
                {
                    1 => "时间……终将冻结一切。",
                    2 => "在宙道面前，你的挣扎毫无意义。",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "你的存在，不过是时间长河中的一瞬。",
                GuAttitude.Friendly => "长生天不拒绝合作，但前提是你有价值。",
                GuAttitude.Respectful => "能让我正视的对手不多，你算一个。",
                GuAttitude.Contemptuous => "蝼蚁般的存在，不值得我浪费时间。",
                GuAttitude.Fearful => "你……竟能撼动时间的流转……",
                _ => "时间……终将冻结一切。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "宙道冰道，双道合一，你无处可逃。",
                "时间会证明，你的一切反抗都是徒劳。",
                "冰封万物，时间凝固——这是我的道。",
                "长生天的领袖，岂是你能挑衅的？",
                "在绝对的力量面前，一切计谋都是笑话。",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("BingSaiChuan", "greeting");

            b.StartNode("greeting", "冰塞川冷冷地看着你，周围空气似乎都在降温。")
                .AddOption("谈论长生天", "changshengtian", DialogueOptionType.Informative)
                .AddOption("宙道与冰道", "dao_discussion", DialogueOptionType.Teach)
                .AddOption("关于北原", "north_desert", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("changshengtian", "冰塞川：\"长生天是巨阳仙尊创建的势力，我接手后，将其发展壮大。北原的一切，都在长生天的掌控之中。\"")
                .AddOption("你如何成为领袖？", "leadership")
                .AddOption("长生天的目标？", "cst_goal")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("leadership", "冰塞川：\"实力。在长生天，实力决定一切。八转修为，宙道冰道双修，这就是我坐在这个位置上的理由。\"")
                .AddOption("回到长生天", "changshengtian");

            b.StartNode("cst_goal", "冰塞川：\"目标？维持北原的秩序，抵御外敌入侵。天庭、影宗，都觊觎北原的资源。我不会让任何人染指。\"")
                .AddOption("天庭的威胁？", "heaven_threat")
                .AddOption("回到长生天", "changshengtian");

            b.StartNode("heaven_threat", "冰塞川：\"天庭……他们自诩正道，实则想控制一切。宿命虽已破碎，但天庭的野心从未消减。\"")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("dao_discussion", "冰塞川：\"宙道操控时间，冰道冻结万物。两道结合，便是让时间本身凝固——这是我的道。\"")
                .AddOption("宙道的本质？", "zhou_dao")
                .AddOption("冰道的极致？", "bing_dao")
                .AddOption("双道合一的难度？", "dual_dao")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("zhou_dao", "冰塞川：\"宙道是操控时间的道。快、慢、停、逆——时间的一切维度，都在宙道的范畴之内。但逆转时间，那是九转才能触及的领域。\"")
                .AddOption("回到道论", "dao_discussion");

            b.StartNode("bing_dao", "冰塞川：\"冰道是北原最普遍的道。但大多数冰道蛊师只知冰冻，不知冰的本质是——静止。将一切运动停止，这才是冰道的真谛。\"")
                .AddOption("回到道论", "dao_discussion");

            b.StartNode("dual_dao", "冰塞川：\"双道合一？这需要极高的天赋和无数的资源。宙道与冰道有天然的亲和性——时间可以冻结，冰可以永恒。这就是我选择双修的原因。\"")
                .AddOption("回到道论", "dao_discussion");

            b.StartNode("north_desert", "冰塞川：\"北原是蛊世界五大域之一，以冰道和运道闻名。长生天是北原最大的势力，任何外来者都必须遵守我们的规矩。\"")
                .AddOption("北原的强者？", "north_masters")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("north_masters", "冰塞川：\"北原的强者……雪胡老祖、黑楼兰、五行大法师，都是长生天的核心战力。而我，是他们的领袖。\"")
                .AddOption("回到北原", "north_desert");

            b.StartNode("trade", "冰塞川：\"交易可以。但记住，长生天的资源不是白给的。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "冰塞川转过身去：\"时间……终将冻结一切。\"")
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

        public override List<string> SetNPCNameList() => new List<string> { "冰塞川" };
    }
}
