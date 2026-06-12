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

namespace VerminLordMod.Content.NPCs.SouthBorder
{
    public class TianHeShangRen : GuMasterBase
    {
        private int _attackTimer;
        private int _craneSummonCooldown;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.Zhuan5_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Arrogant;

        public override string GuMasterDisplayName => "天鹤上人";
        public override int GuMasterDamage => 150;
        public override int GuMasterLife => 20000;
        public override int GuMasterDefense => 80;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 600;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 40;
            NPCID.Sets.AttackAverageChance[Type] = 30;
            NPCID.Sets.HatOffsetY[Type] = 4;

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
            NPC.knockBackResist = 0.3f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(0, 30, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _craneSummonCooldown = 0;
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

            if (_craneSummonCooldown > 0) _craneSummonCooldown--;

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

            NPC.velocity.X = dir * 1.8f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -6f;

            _attackTimer++;
            if (_attackTimer >= 50)
            {
                _attackTimer = 0;
                ExecuteAttack(target);
            }

            if (_craneSummonCooldown <= 0 && NPC.life < NPC.lifeMax * 0.5f)
            {
                SummonCranes(target);
                _craneSummonCooldown = 600;
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
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.HarpyFeather, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 1:
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 10f,
                        ProjectileID.Typhoon, damage, 2f, Main.myPlayer);
                    break;
            }
        }

        private void SummonCranes(Player target)
        {
            CombatText.NewText(NPC.getRect(), Color.White, "鹤鸣九天！", true);

            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            var source = NPC.GetSource_FromAI();
            int damage = NPC.damage / 4;

            for (int i = 0; i < 4; i++)
            {
                Vector2 spawnPos = NPC.Center + new Vector2(Main.rand.Next(-200, 200), -300);
                Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.UnitY) * 7f;
                Projectile.NewProjectile(source, spawnPos, vel, ProjectileID.HarpyFeather, damage, 1f, Main.myPlayer);
            }

            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.Cloud,
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
                    100, Color.White, 1.5f);
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => NumberOfTimesTalkedTo switch
                {
                    1 => "区区青茅山，也敢与我仙鹤门作对？",
                    2 => "你走上了邪路，我不得不出手！",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "你的气息……不太对劲。希望你不是邪修。",
                GuAttitude.Friendly => "道友请了，仙鹤门天鹤，愿与你论道。",
                GuAttitude.Respectful => "道友修为不凡，令人敬佩。",
                GuAttitude.Contemptuous => "哼，邪魔外道，不值一提。",
                GuAttitude.Fearful => "你的力量……远超我所能抗衡……",
                _ => "区区青茅山，也敢与我仙鹤门作对？"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "风道之下，邪魔无所遁形！",
                "我天鹤虽只是五转，但正道之心不可夺！",
                "鹤鸣九天，驱邪除魔！",
                "你若回头，尚有出路！",
                "仙鹤门的弟子，绝不向邪恶低头！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("TianHeShangRen", "greeting");

            b.StartNode("greeting", "天鹤上人白衣飘飘，仙风道骨，宛如一只白鹤立于人间。")
                .AddOption("谈论风道", "feng_dao", DialogueOptionType.Teach)
                .AddOption("关于方正", "fang_zheng", DialogueOptionType.Informative)
                .AddOption("仙鹤门", "xianhe_gate", DialogueOptionType.Informative)
                .AddOption("正邪之辩", "righteousness", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("feng_dao", "天鹤上人：\"风道？风无形无相，却能穿山越岭。风道的精髓在于——顺势而为，借力打力。\"")
                .AddOption("风道的攻击手段？", "feng_attack")
                .AddOption("鹤的修炼？", "crane_training")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("feng_attack", "天鹤上人：\"风刃是最基本的风道攻击，但高阶风道可以操控气流，形成风暴、龙卷。我的鹤鸣九天，便是以风道召唤灵鹤攻击。\"")
                .AddOption("回到风道", "feng_dao");

            b.StartNode("crane_training", "天鹤上人：\"鹤是风道的灵兽，与风道蛊师有天然的亲和力。修炼鹤法，可以增强风道修为，也能获得鹤的灵性。\"")
                .AddOption("回到风道", "feng_dao");

            b.StartNode("fang_zheng", "天鹤上人叹了口气：\"方正……我曾在青茅山救过他。那孩子本性不坏，却被方源害得家破人亡。我只恨自己修为不够，无法为他讨回公道。\"")
                .AddOption("方源做了什么？", "fang_yuan_evil")
                .AddOption("方正现在如何？", "fang_zheng_now")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("fang_yuan_evil", "天鹤上人：\"方源……那个恶魔。他利用了所有人，包括自己的族人。青茅山的惨剧，都是他一手策划的。\"")
                .AddOption("回到方正", "fang_zheng");

            b.StartNode("fang_zheng_now", "天鹤上人：\"方正后来被天庭收留，修炼了奴道。但他的心中，始终充满了对方源的恨意。我只希望他不要被仇恨吞噬。\"")
                .AddOption("回到方正", "fang_zheng");

            b.StartNode("xianhe_gate", "天鹤上人：\"仙鹤门是南疆的小门派，以风道和鹤法闻名。虽然实力不强，但我们坚守正道，从不与邪修为伍。\"")
                .AddOption("仙鹤门的传承？", "xianhe_legacy")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("xianhe_legacy", "天鹤上人：\"仙鹤门的传承可以追溯到上古时期，据说创派祖师曾得到一只九转鹤蛊的指点。但那只是传说，如今的仙鹤门，只有我这一个五转修士撑着。\"")
                .AddOption("回到仙鹤门", "xianhe_gate");

            b.StartNode("righteousness", "天鹤上人正色道：\"正邪之辩？正道以苍生为念，邪道以私欲为先。这不是空话——正道修士守护弱者，邪修只知掠夺。这是亘古不变的道理。\"")
                .AddOption("但方源说他只追求永生？", "fang_yuan_immortality")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("fang_yuan_immortality", "天鹤上人摇头：\"永生？以牺牲他人为代价的永生，不过是另一种形式的邪恶。真正的长生，应该是与天地同寿，而非踩着尸骨前行。\"")
                .AddOption("回到正邪之辩", "righteousness");

            b.StartNode("trade", "天鹤上人：\"交易？可以。仙鹤门虽小，但风道材料还是有些的。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "天鹤上人拱手：\"道友保重，愿正道永存。\"")
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

        public override List<string> SetNPCNameList() => new List<string> { "天鹤上人" };
    }
}
