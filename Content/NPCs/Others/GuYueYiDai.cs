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

namespace VerminLordMod.Content.NPCs.Others
{
    public class GuYueYiDai : GuMasterBase
    {
        private int _attackTimer;
        private bool _hiddenPowerActive;
        private int _bloodSkullCooldown;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.GuYue;
        public override GuRank GetRank() => GuRank.Zhuan5_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Hidden;

        public override string GuMasterDisplayName => "古月一代";
        public override int GuMasterDamage => 200;
        public override int GuMasterLife => 25000;
        public override int GuMasterDefense => 90;

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
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Bleeding] = true;

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
            NPC.value = Item.buyPrice(0, 40, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _hiddenPowerActive = false;
            _bloodSkullCooldown = 0;
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

            if (_bloodSkullCooldown > 0) _bloodSkullCooldown--;

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
                NPC.velocity.Y = -5f;

            _attackTimer++;
            if (_attackTimer >= 45)
            {
                _attackTimer = 0;
                ExecuteAttack(target);
            }

            if (!_hiddenPowerActive && NPC.life < NPC.lifeMax * 0.3f)
            {
                ActivateHiddenPower();
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
                    if (_bloodSkullCooldown <= 0)
                    {
                        CombatText.NewText(NPC.getRect(), Color.DarkRed, "血颅蛊！", true);
                        Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 10f,
                            ProjectileID.BloodShot, damage * 2, 2f, Main.myPlayer);

                        int healAmount = damage;
                        NPC.life = Math.Min(NPC.life + healAmount, NPC.lifeMax);
                        NPC.HealEffect(healAmount, true);

                        _bloodSkullCooldown = 180;
                    }
                    break;
                case 1:
                    for (int i = -2; i <= 2; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.12f;
                        Vector2 vel = angle.ToRotationVector2() * 8f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.VampireKnife, damage, 1f, Main.myPlayer);
                    }
                    break;
            }
        }

        private void ActivateHiddenPower()
        {
            _hiddenPowerActive = true;
            CombatText.NewText(NPC.getRect(), Color.DarkRed, "千年布局——隐藏之力！", true);

            NPC.damage = (int)(NPC.defDamage * 1.8f);
            NPC.defense -= 20;

            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.Blood,
                    Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f),
                    100, Color.DarkRed, 2f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi / 12f * i;
                    Vector2 vel = angle.ToRotationVector2() * 7f;
                    Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.BloodShot, NPC.damage / 4, 2f, Main.myPlayer);
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
                    1 => "千年布局，只为今日。",
                    2 => "你知道得太多了……不能留你！",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "你……你是什么人？不要妨碍我的计划。",
                GuAttitude.Friendly => "古月家的朋友？呵，那就好。不过……你知道的越少越好。",
                GuAttitude.Respectful => "你很聪明……但聪明人往往死得最快。",
                GuAttitude.Contemptuous => "蝼蚁……你不知道自己面对的是什么。",
                GuAttitude.Fearful => "你……你竟然看穿了我的布局！",
                _ => "千年布局，只为今日。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "血道之下，你的生命就是我的养分！",
                "千年的谋划，不是你能破坏的！",
                "血颅蛊——吸干你的精血！",
                "古月家的秘密，你永远猜不到！",
                "我等了千年，不差这一时！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("GuYueYiDai", "greeting");

            b.StartNode("greeting", "古月一代看似普通的老者，但眼中的精光透露出千年的算计。")
                .AddOption("谈论血道", "xue_dao", DialogueOptionType.Teach)
                .AddOption("古月家的秘密", "guyue_secret", DialogueOptionType.Risky)
                .AddOption("千年的布局", "thousand_year", DialogueOptionType.Risky)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("xue_dao", "古月一代：\"血道？哼，血道是最被误解的道。世人只知血道残忍，却不知血道的本质是——生命的转化。以血为媒，可以治愈，也可以杀戮。\"")
                .AddOption("血颅蛊是什么？", "blood_skull")
                .AddOption("血道的禁忌？", "xue_taboo")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("blood_skull", "古月一代：\"血颅蛊……这是我的本命蛊。它可以吸取敌人的精血，转化为我的生命力。杀敌一千，自补八百——这就是血颅蛊的妙处。\"")
                .AddOption("回到血道", "xue_dao");

            b.StartNode("xue_taboo", "古月一代：\"血道的禁忌？以血道伤害同族，是最大的禁忌。但……呵，规则是给弱者定的。强者，不受规则束缚。\"")
                .AddOption("回到血道", "xue_dao");

            b.StartNode("guyue_secret", "古月一代眼中闪过一丝危险的光芒：\"古月家的秘密？你确定你想知道？有些秘密，知道了就再也活不了了。\"")
                .AddOption("我不怕", "secret_reveal")
                .AddOption("还是算了", "greeting");

            b.StartNode("secret_reveal", "古月一代低声说：\"古月家……不仅仅是青茅山的一个小家族。我们的血脉中，流淌着上古血道大能的传承。千年来，每一代家主都在暗中布局……你真的以为方源是唯一在算计的人吗？\"")
                .AddOption("你在布局什么？", "thousand_year")
                .AddOption("我听够了", "greeting");

            b.StartNode("thousand_year", "古月一代：\"千年的布局……你以为方源五百年的算计已经很惊人了？呵，那不过是小巫见大巫。古月家的千年布局，才是真正的棋局。\"")
                .AddOption("棋局的目的是什么？", "chess_goal")
                .AddOption("方源知道吗？", "fang_yuan_knows")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("chess_goal", "古月一代：\"目的？……复兴古月家的上古荣光。让古月家重新成为血道的至高家族。这需要时间，需要牺牲，需要……布局。\"")
                .AddOption("回到千年布局", "thousand_year");

            b.StartNode("fang_yuan_knows", "古月一代冷笑：\"方源？他以为自己是棋手，殊不知……他也在我的棋盘上。不过，他确实是个变数——一个我没能完全算到的变数。\"")
                .AddOption("回到千年布局", "thousand_year");

            b.StartNode("trade", "古月一代：\"交易？可以。但我只和有价值的人交易。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "古月一代微微点头：\"去吧。记住，你知道的越少，活得越久。\"")
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

        public override List<string> SetNPCNameList() => new List<string> { "古月一代" };
    }
}
