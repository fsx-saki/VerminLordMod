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
    public class LuWeiYin : GuMasterBase
    {
        private int _attackTimer;
        private int _mazeTimer;
        private bool _renHealingActive;
        private int _renCooldown;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.Zhuan8_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Compassionate;

        public override string GuMasterDisplayName => "陆畏因";
        public override int GuMasterDamage => 350;
        public override int GuMasterLife => 68000;
        public override int GuMasterDefense => 240;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 1200;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 25;
            NPCID.Sets.AttackAverageChance[Type] = 12;
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
            NPC.knockBackResist = 0.05f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(3, 50, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _mazeTimer = 0;
            _renHealingActive = false;
            _renCooldown = 0;
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

            if (_renCooldown > 0) _renCooldown--;

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

            NPC.velocity.X = dir * 2f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -6f;

            _attackTimer++;
            if (_attackTimer >= 35)
            {
                _attackTimer = 0;
                ExecuteAttack(target);
            }

            _mazeTimer++;
            if (_mazeTimer >= 240)
            {
                _mazeTimer = 0;
                SpawnIllusionMaze(target);
            }

            if (!_renHealingActive && NPC.life < NPC.lifeMax * 0.5f && _renCooldown <= 0)
            {
                ActivateRenHealing();
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
                    CombatText.NewText(NPC.getRect(), Color.Brown, "幻沙转影！", true);
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi / 12f * i;
                        Vector2 vel = angle.ToRotationVector2() * 5f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.SandBallGun, damage, 2f, Main.myPlayer);
                    }
                    break;
                case 1:
                    for (int i = -3; i <= 3; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.08f;
                        Vector2 vel = angle.ToRotationVector2() * 9f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.BoulderStaffOfEarth, damage, 3f, Main.myPlayer);
                    }
                    break;
                case 2:
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 11f,
                        ProjectileID.BoulderStaffOfEarth, damage * 2, 4f, Main.myPlayer);
                    break;
            }
        }

        private void SpawnIllusionMaze(Player target)
        {
            CombatText.NewText(NPC.getRect(), Color.Brown, "胎土迷宫！", true);

            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            var source = NPC.GetSource_FromAI();
            int damage = NPC.damage / 5;

            for (int i = 0; i < 20; i++)
            {
                Vector2 spawnPos = target.Center + new Vector2(Main.rand.Next(-350, 350), Main.rand.Next(-300, 300));
                Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.UnitY) * 3f;
                Projectile.NewProjectile(source, spawnPos, vel, ProjectileID.SandBallGun, damage, 1f, Main.myPlayer);
            }

            target.AddBuff(BuffID.Slow, 180);
            target.AddBuff(BuffID.Blackout, 120);

            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(target.Center, 20, 20, DustID.Sand,
                    Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f),
                    100, Color.Brown, 1.5f);
            }
        }

        private void ActivateRenHealing()
        {
            _renHealingActive = true;
            _renCooldown = 600;
            CombatText.NewText(NPC.getRect(), Color.Green, "仁——！", true);

            int healAmount = NPC.lifeMax / 4;
            NPC.life = Math.Min(NPC.life + healAmount, NPC.lifeMax);
            NPC.HealEffect(healAmount, true);

            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.GreenTorch,
                    Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f),
                    100, Color.Green, 2f);
            }

            _renHealingActive = false;
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => NumberOfTimesTalkedTo switch
                {
                    1 => "乐土之道，在于与万物共存。",
                    2 => "我不愿与你为敌，但你若执意……",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "你的气息……让我不安。但我愿意相信你的善意。",
                GuAttitude.Friendly => "施主有礼，陆畏因愿与您结善缘。",
                GuAttitude.Respectful => "施主心怀慈悲，令畏因敬佩。",
                GuAttitude.Contemptuous => "你……为何如此？慈悲之心，人皆有之。",
                GuAttitude.Fearful => "我……虽败，但师父的遗愿不会断绝……",
                _ => "乐土之道，在于与万物共存。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "土道阵道，双道合一，你逃不出我的迷宫！",
                "幻沙转影战场——这里是我的领域！",
                "仁者无敌，我的慈悲不是软弱！",
                "胎土迷宫，困住你的不仅是身体，还有心！",
                "师父，请赐予我力量！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("LuWeiYin", "greeting");

            b.StartNode("greeting", "陆畏因静坐于地，身上散发着泥土与蘑菇的气息，面容慈悲。")
                .AddOption("谈论土道与阵道", "dao_discussion", DialogueOptionType.Teach)
                .AddOption("关于乐土仙尊", "le_tu", DialogueOptionType.Informative)
                .AddOption("蘑菇人的身世", "mushroom_origin", DialogueOptionType.Informative)
                .AddOption("仁的力量", "ren_power", DialogueOptionType.Special)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("dao_discussion", "陆畏因：\"土道与阵道，是我修炼的两条道。土道操控大地，阵道布设陷阱。两道结合，便是'胎土迷宫'——让敌人迷失在无尽的土之迷宫中。\"")
                .AddOption("土道的本质？", "tu_dao")
                .AddOption("阵道的精髓？", "zhen_dao")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("tu_dao", "陆畏因：\"土道的本质是——承载。大地承载万物，不择善恶。土道修士可以操控泥土、岩石、沙尘，甚至可以改变地形。\"")
                .AddOption("回到道论", "dao_discussion");

            b.StartNode("zhen_dao", "陆畏因：\"阵道的精髓是——困。阵法可以困住敌人，也可以保护友军。我的胎土迷宫，便是阵道的极致体现。\"")
                .AddOption("回到道论", "dao_discussion");

            b.StartNode("le_tu", "陆畏因双手合十：\"师父……乐土仙尊，他是我的恩师，也是我的一切。他教会了我慈悲，教会了我'仁'。\"")
                .AddOption("乐土仙尊的遗愿？", "le_tu_wish")
                .AddOption("乐土仙尊是怎样的人？", "le_tu_person")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("le_tu_wish", "陆畏因：\"师父的遗愿……是建立一个没有痛苦的世界。虽然这个愿望太过宏大，但我会尽我所能，朝着这个方向努力。\"")
                .AddOption("回到乐土仙尊", "le_tu");

            b.StartNode("le_tu_person", "陆畏因：\"师父……他是一个真正慈悲的人。他从不杀生，即使面对敌人，也以感化为主。他相信，每个人都有善的一面。\"")
                .AddOption("回到乐土仙尊", "le_tu");

            b.StartNode("mushroom_origin", "陆畏因微微一笑：\"蘑菇人？是的，我不是人类。我是师父用土道和生命道培育出的蘑菇人。虽然外形不同，但我的心，和人类一样。\"")
                .AddOption("蘑菇人有何特殊？", "mushroom_special")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("mushroom_special", "陆畏因：\"蘑菇人的特殊之处？我们可以与大地沟通，感受泥土中的信息。这对土道修炼有极大的帮助。而且……我们不需要进食，只需要阳光和水分。\"")
                .AddOption("回到蘑菇人身世", "mushroom_origin");

            b.StartNode("ren_power", "陆畏因：\"仁……这是师父传授给我的最高道义。仁者爱人，仁者无敌。'仁'的力量不在于攻击，而在于——治愈。以仁心治愈一切伤痛。\"")
                .AddOption("仁能治愈什么？", "ren_heal")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ren_heal", "陆畏因：\"仁可以治愈身体的伤痛，也可以治愈心灵的创伤。但……仁无法治愈那些不愿被治愈的人。这是仁的局限，也是仁的慈悲——它尊重每个人的选择。\"")
                .AddOption("回到仁的力量", "ren_power");

            b.StartNode("trade", "陆畏因：\"交易？可以。我这里有些土道和阵道的材料，或许对您有用。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "陆畏因双手合十：\"施主保重，愿仁心永驻。\"")
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

        public override List<string> SetNPCNameList() => new List<string> { "陆畏因" };
    }
}
