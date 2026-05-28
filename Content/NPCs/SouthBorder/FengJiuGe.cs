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
    [AutoloadHead]
    public class FengJiuGe : GuMasterBase
    {
        private int _attackTimer;
        private int _songPhase;
        private bool _daFengGeActive;
        private int _daFengGeCooldown;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.Heaven;
        public override GuRank GetRank() => GuRank.Zhuan8_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Proud;

        public override string GuMasterDisplayName => "凤九歌";
        public override int GuMasterDamage => 480;
        public override int GuMasterLife => 75000;
        public override int GuMasterDefense => 160;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 2000;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 15;
            NPCID.Sets.AttackAverageChance[Type] = 8;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Silenced] = true;

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
            _songPhase = 0;
            _daFengGeActive = false;
            _daFengGeCooldown = 0;
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

            if (_daFengGeCooldown > 0) _daFengGeCooldown--;

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

            if (dist > 600f)
                NPC.velocity.X = dir * 2.5f;
            else
                NPC.velocity.X = dir * 1f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -7f;

            _attackTimer++;
            if (_attackTimer >= 25)
            {
                _attackTimer = 0;
                _songPhase = (_songPhase + 1) % 4;
                ExecuteAttack(target);
            }

            if (!_daFengGeActive && NPC.life < NPC.lifeMax * 0.35f && _daFengGeCooldown <= 0)
            {
                ActivateDaFengGe(target);
            }
        }

        private void ExecuteAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 4;

            switch (_songPhase)
            {
                case 0:
                    CombatText.NewText(NPC.getRect(), Color.Magenta, "音波——！", true);
                    for (int i = -5; i <= 5; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.06f;
                        Vector2 vel = angle.ToRotationVector2() * 12f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.HarpyFeather, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 1:
                    for (int i = 0; i < 16; i++)
                    {
                        float angle = MathHelper.TwoPi / 16f * i;
                        Vector2 vel = angle.ToRotationVector2() * 5f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.CultistBossLightningOrb, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 2:
                    Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 15f,
                        ProjectileID.DD2BetsyFireball, damage * 2, 3f, Main.myPlayer);
                    break;
                case 3:
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 spawnPos = target.Center + new Vector2(Main.rand.Next(-250, 250), -400);
                        Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), 10f);
                        Projectile.NewProjectile(source, spawnPos, vel, ProjectileID.HarpyFeather, damage, 2f, Main.myPlayer);
                    }
                    break;
            }
        }

        private void ActivateDaFengGe(Player target)
        {
            _daFengGeActive = true;
            _daFengGeCooldown = 1200;
            CombatText.NewText(NPC.getRect(), Color.Magenta, "大风歌——！", true);

            for (int i = 0; i < 50; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.PinkTorch,
                    Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f),
                    100, Color.Magenta, 3f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var source = NPC.GetSource_FromAI();
                int damage = NPC.damage / 2;

                for (int wave = 0; wave < 3; wave++)
                {
                    for (int i = 0; i < 24; i++)
                    {
                        float angle = MathHelper.TwoPi / 24f * i + wave * 0.13f;
                        Vector2 vel = angle.ToRotationVector2() * (8f + wave * 2f);
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.DD2BetsyFireball, damage, 3f, Main.myPlayer);
                    }
                }

                foreach (Player p in Main.player)
                {
                    if (p.active && !p.dead && Vector2.Distance(NPC.Center, p.Center) < 800f)
                    {
                        p.AddBuff(BuffID.Slow, 300);
                        p.AddBuff(BuffID.Silenced, 180);
                    }
                }
            }

            _daFengGeActive = false;
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => NumberOfTimesTalkedTo switch
                {
                    1 => "一人一琴，纵横三千万里。",
                    2 => "你竟敢挑战我？凤九歌的名号，你听过没有？",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "你的眼神很锐利……但还不够。",
                GuAttitude.Friendly => "难得遇到懂音律之人，坐下来听一曲如何？",
                GuAttitude.Respectful => "能让我正视的对手，你是少数之一。",
                GuAttitude.Contemptuous => "你连听我琴音的资格都没有。",
                GuAttitude.Fearful => "你……竟能抵挡我的大风歌……",
                _ => "一人一琴，纵横三千万里！"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "音道之下，万物震颤！",
                "大风歌起，无人可挡！",
                "我的琴音，就是你的丧钟！",
                "亚仙尊的力量，你承受不起！",
                "巴仙楚度也败在我的音下，你算什么？",
                "命运歌——你的命运，由我来决定！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("FengJiuGe", "greeting");

            b.StartNode("greeting", "凤九歌抚琴而坐，琴音悠扬，似有千军万马在其中奔腾。")
                .AddOption("谈论音道", "yin_dao", DialogueOptionType.Teach)
                .AddOption("大风歌的传说", "dafengge", DialogueOptionType.Special)
                .AddOption("关于天庭", "heaven", DialogueOptionType.Informative)
                .AddOption("南疆往事", "south_past", DialogueOptionType.Informative)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("yin_dao", "凤九歌：\"音道？这是最被低估的道。世人只知音道能奏乐，却不知音道可杀人于无形。一曲大风歌，足以灭杀八转蛊仙。\"")
                .AddOption("音道的极致？", "yin_limit")
                .AddOption("音道攻击的原理？", "yin_principle")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("yin_limit", "凤九歌：\"音道的极致？命运歌。那是一首能触及命运本身的曲子，弹奏之时，亚仙尊的力量尽现。但命运歌的代价……我不愿多提。\"")
                .AddOption("回到音道", "yin_dao");

            b.StartNode("yin_principle", "凤九歌：\"音道的攻击原理很简单——以音波震荡万物。但简单的原理，做到极致便是无敌。音波可以穿透防御，震碎内腑，甚至扰乱神魂。\"")
                .AddOption("回到音道", "yin_dao");

            b.StartNode("dafengge", "凤九歌微微一笑：\"大风歌……这是我的成名绝技。一曲大风歌，杀巴仙楚度于三千万里之外。那一战，是我此生最辉煌的时刻。\"")
                .AddOption("巴仙楚度是谁？", "ba_xian_chu_du")
                .AddOption("大风歌的威力？", "dafengge_power")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("ba_xian_chu_du", "凤九歌：\"巴仙楚度……南疆的强者，七转巅峰。他曾不可一世，直到遇到我。大风歌一响，他便化为了飞灰。这就是音道的力量。\"")
                .AddOption("回到大风歌", "dafengge");

            b.StartNode("dafengge_power", "凤九歌：\"大风歌的威力？三千万里之内，无人可挡。音波所过之处，山崩地裂，江河倒流。这不是夸张，这是事实。\"")
                .AddOption("回到大风歌", "dafengge");

            b.StartNode("heaven", "凤九歌：\"天庭……我曾经是天庭的人。但天庭的规矩太多，容不下我凤九歌的自由。所以我离开了，但天庭的恩情，我记着。\"")
                .AddOption("你为何离开天庭？", "leave_heaven")
                .AddOption("天庭的现状？", "heaven_now")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("leave_heaven", "凤九歌：\"为何离开？因为天庭要的是服从，而我要的是自由。一人一琴，纵横天下，这才是我的道。天庭的枷锁，我戴不了。\"")
                .AddOption("回到天庭", "heaven");

            b.StartNode("heaven_now", "凤九歌：\"天庭的现状？宿命破碎后，天庭的根基动摇了。但百足之虫死而不僵，天庭的底蕴依然深厚。只是……再也无法像从前那样掌控一切了。\"")
                .AddOption("回到天庭", "heaven");

            b.StartNode("south_past", "凤九歌：\"南疆……那是我的故乡。南疆蛊师好战，家族林立，争斗不断。我在南疆成长，也在南疆成名。\"")
                .AddOption("南疆最强的势力？", "south_factions")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("south_factions", "凤九歌：\"南疆最强的势力？曾经是各大古族——白家、熊家、武家。但如今，南疆已经四分五裂，没有谁能一统南疆。\"")
                .AddOption("回到南疆往事", "south_past");

            b.StartNode("trade", "凤九歌：\"交易？可以。但我只和有趣的人交易。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "凤九歌轻拨琴弦：\"去吧，愿音律护佑你。\"")
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

        public override List<string> SetNPCNameList() => new List<string> { "凤九歌" };
    }
}
