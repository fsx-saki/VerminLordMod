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
    public class WuYong : GuMasterBase
    {
        private int _attackTimer;
        private int _warCryCooldown;
        private bool _combatAuraActive;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.Zhuan7_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Bold;

        public override string GuMasterDisplayName => "武庸";
        public override int GuMasterDamage => 350;
        public override int GuMasterLife => 48000;
        public override int GuMasterDefense => 150;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 1000;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 20;
            NPCID.Sets.AttackAverageChance[Type] = 15;
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
            NPC.knockBackResist = 0.15f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(1, 50, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _warCryCooldown = 0;
            _combatAuraActive = false;
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

            if (_warCryCooldown > 0) _warCryCooldown--;

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

            float speed = _combatAuraActive ? 3.5f : 2.5f;
            NPC.velocity.X = dir * speed;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -8f;

            _attackTimer++;
            int attackInterval = _combatAuraActive ? 20 : 35;
            if (_attackTimer >= attackInterval)
            {
                _attackTimer = 0;
                ExecuteAttack(target);
            }

            if (!_combatAuraActive && NPC.life < NPC.lifeMax * 0.6f && _warCryCooldown <= 0)
            {
                ActivateWarCry();
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
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.Meteor1, damage, 3f, Main.myPlayer);
                    }
                    break;
                case 1:
                    NPC.velocity.X += Math.Sign(target.Center.X - NPC.Center.X) * 5f;
                    CombatText.NewText(NPC.getRect(), Color.OrangeRed, "武道连环——冲锋！", true);
                    break;
                case 2:
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 vel = angle.ToRotationVector2() * 6f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.CultistBossFireBall, damage, 2f, Main.myPlayer);
                    }
                    break;
            }
        }

        private void ActivateWarCry()
        {
            _combatAuraActive = true;
            _warCryCooldown = 900;
            CombatText.NewText(NPC.getRect(), Color.Red, "战嚎——！", true);

            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.Torch,
                    Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f),
                    100, Color.Red, 2f);
            }

            NPC.damage = (int)(NPC.defDamage * 1.4f);
            NPC.defense -= 30;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi / 12f * i;
                    Vector2 vel = angle.ToRotationVector2() * 8f;
                    Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.DD2BetsyFireball, NPC.damage / 5, 2f, Main.myPlayer);
                }
            }

            foreach (Player p in Main.player)
            {
                if (p.active && !p.dead && Vector2.Distance(NPC.Center, p.Center) < 500f)
                {
                    p.AddBuff(BuffID.Weak, 180);
                }
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => NumberOfTimesTalkedTo switch
                {
                    1 => "南疆武者，从不退缩！",
                    2 => "来吧！让我看看你的战意！",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "你身上有战场的气息……但还不够浓。",
                GuAttitude.Friendly => "有战意的人，我武庸交你这个朋友！",
                GuAttitude.Respectful => "强者！来，与我痛饮一杯！",
                GuAttitude.Contemptuous => "没有战意的人，不配站在我面前。",
                GuAttitude.Fearful => "你……比战场上的任何敌人都强……",
                _ => "南疆武者，从不退缩！"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "战道不灭，我武庸不败！",
                "南疆男儿，绝不退缩！",
                "来吧！这正是我渴望的战斗！",
                "战嚎一响，万军辟易！",
                "在战场上，只有强者才能活下来！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("WuYong", "greeting");

            b.StartNode("greeting", "武庸浑身散发着战意，仿佛随时准备投入战斗。")
                .AddOption("谈论战道", "zhan_dao", DialogueOptionType.Teach)
                .AddOption("南疆的战场", "south_battle", DialogueOptionType.Informative)
                .AddOption("关于武家", "wu_family", DialogueOptionType.Informative)
                .AddOption("切磋", "spar", DialogueOptionType.Combat)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("zhan_dao", "武庸：\"战道？这是最纯粹的道——战斗、战斗、再战斗！在战斗中领悟，在战斗中突破，在战斗中证道！\"")
                .AddOption("战道的核心？", "zhan_core")
                .AddOption("战嚎是什么？", "war_cry")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("zhan_core", "武庸：\"战道的核心？战意！没有战意，就没有战道。战意越强，力量越大。当你不怕死的时候，就是你最强的时候！\"")
                .AddOption("回到战道", "zhan_dao");

            b.StartNode("war_cry", "武庸：\"战嚎？这是战道高阶修士的绝技。一声战嚎，可以激发自身潜能，削弱敌人斗志。在战场上，战嚎一响，万军辟易！\"")
                .AddOption("回到战道", "zhan_dao");

            b.StartNode("south_battle", "武庸：\"南疆的战场？南疆从来不缺战争。家族争斗、资源争夺、恩怨清算……在南疆，战斗就是生活。\"")
                .AddOption("南疆最大的战役？", "south_war")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("south_war", "武庸：\"最大的战役？三王传承之战！那场大战，整个南疆都被卷入。无数蛊师陨落，但也诞生了无数传说。\"")
                .AddOption("回到南疆战场", "south_battle");

            b.StartNode("wu_family", "武庸：\"武家……南疆的古族之一。以战道闻名，族中高手如云。但如今武家已经没落，只剩下我这样的散修。\"")
                .AddOption("武家为何没落？", "wu_decline")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("wu_decline", "武庸：\"没落？内斗。武家的人太好战了，不仅对外打，对内也打。打到最后，家族四分五裂，再也无法凝聚。\"")
                .AddOption("回到武家", "wu_family");

            b.StartNode("spar", "武庸大笑：\"好！有胆量！来吧，让我看看你的实力！\"")
                .TriggersCombat()
                .AddOption("等等，我还没准备好", "greeting");

            b.StartNode("trade", "武庸：\"交易？行，但别跟我讨价还价，我不喜欢磨叽。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "武庸拍拍你的肩膀：\"去吧！记住，只有战斗才能让你变强！\"")
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

        public override List<string> SetNPCNameList() => new List<string> { "武庸" };
    }
}
