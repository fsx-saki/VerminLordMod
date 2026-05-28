using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Content.NPCs.Heroines
{
    [AutoloadHead]
    public class BaiTuGuNiang : GuMasterBase
    {
        private static readonly Color DarkPurpleColor = new Color(48, 0, 96);

        public override FactionID GetFaction() => FactionID.ShadowSect;
        public override GuRank GetRank() => IsBlackTuForm ? GuRank.Zhuan7_Gao : GuRank.Zhuan6_Gao;
        public override GuPersonality GetPersonality() => IsBlackTuForm ? GuPersonality.DualFaced : GuPersonality.Benevolent;

        public override string GuMasterDisplayName => IsBlackTuForm ? "黑菟" : "白兔姑娘";
        public override int GuMasterDamage => IsBlackTuForm ? 340 : 220;
        public override int GuMasterLife => IsBlackTuForm ? 55000 : 40000;
        public override int GuMasterDefense => IsBlackTuForm ? 150 : 100;

        private bool _isBlackTuForm;
        private bool _hasTransformed;
        private int _attackTimer;
        private int _transformTimer;
        private bool _treeInitialized;

        public bool IsBlackTuForm
        {
            get => _isBlackTuForm;
            private set
            {
                if (_isBlackTuForm != value)
                {
                    _isBlackTuForm = value;
                    OnFormChanged();
                }
            }
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 700;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 30;
            NPCID.Sets.AttackAverageChance[Type] = 10;
            NPCID.Sets.HatOffsetY[Type] = 4;

            if (!NPCID.Sets.NPCBestiaryDrawOffset.ContainsKey(Type))
            {
                NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers
                {
                    Velocity = 1f,
                    Direction = 1
                });
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
            NPC.value = Item.buyPrice(0, 4, 0, 0);
            NPC.townNPC = false;
            NPC.friendly = false;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
        }

        private void OnFormChanged()
        {
            NPC.damage = GuMasterDamage;
            NPC.defense = GuMasterDefense;
        }

        public override void AI()
        {
            if (!_treeInitialized)
            {
                InitializeDialogueTree();
                _treeInitialized = true;
            }

            base.AI();

            if (!_hasTransformed && (float)NPC.life / NPC.lifeMax <= 0.5f)
            {
                _hasTransformed = true;
                _transformTimer = 0;
                TransformToBlackTu();
            }

            if (_hasTransformed && _transformTimer < 60)
            {
                _transformTimer++;
                NPC.velocity.X *= 0.5f;
                NPC.dontTakeDamage = true;

                for (int i = 0; i < 5; i++)
                {
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                        IsBlackTuForm ? DustID.Shadowflame : DustID.PinkTorch,
                        Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
                        100, IsBlackTuForm ? DarkPurpleColor : Color.Pink, 2f);
                    dust.noGravity = true;
                }

                if (_transformTimer == 30)
                {
                    IsBlackTuForm = true;
                    NPC.lifeMax = 55000;
                    int lifeDeficit = NPC.lifeMax - NPC.life;
                    NPC.life += lifeDeficit / 2;
                }
            }
            else
            {
                NPC.dontTakeDamage = false;
            }
        }

        private void TransformToBlackTu()
        {
            CombatText.NewText(NPC.getRect(), DarkPurpleColor, "嘻嘻……该换一面了~", true);
            Say("黑暗……才是真实的。", DarkPurpleColor);

            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.Shadowflame,
                    Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f),
                    200, DarkPurpleColor, 3f);
            }
        }

        public override void ExecuteCombatAI(NPC npc)
        {
            if (_hasTransformed && _transformTimer < 60) return;

            var target = Main.player[npc.target];
            float dist = Vector2.Distance(npc.Center, target.Center);

            npc.spriteDirection = target.Center.X > npc.Center.X ? 1 : -1;

            float moveSpeed = IsBlackTuForm ? 2.5f : 1.8f;
            if (dist < 120f)
            {
                float fleeDir = npc.Center.X > target.Center.X ? 1 : -1;
                npc.velocity.X = fleeDir * moveSpeed;
            }
            else if (dist > 300f)
            {
                float dir = target.Center.X > npc.Center.X ? 1 : -1;
                npc.velocity.X = dir * moveSpeed;
            }
            else
            {
                npc.velocity.X *= 0.9f;
            }

            if (npc.collideX && npc.velocity.Y == 0)
                npc.velocity.Y = (IsBlackTuForm ? -8f : -6f);

            _attackTimer++;

            if (IsBlackTuForm)
            {
                ExecuteBlackTuCombat(npc, target, dist);
            }
            else
            {
                ExecuteWhiteRabbitCombat(npc, target, dist);
            }
        }

        private void ExecuteWhiteRabbitCombat(NPC npc, Player target, float dist)
        {
            if (_attackTimer % 50 == 0 && dist < 450f)
            {
                Vector2 dir = Vector2.Normalize(target.Center - npc.Center);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 7f,
                    ProjectileID.AmethystBolt, npc.damage / 3, 3f, Main.myPlayer);
            }

            if (_attackTimer % 80 == 0 && dist < 350f)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.ToRadians(Main.rand.Next(-30, 30));
                    Vector2 dir = Vector2.Normalize(target.Center - npc.Center).RotatedBy(angle);
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 6f,
                        ProjectileID.AmethystBolt, npc.damage / 4, 2f, Main.myPlayer);
                }
            }

            if (Main.rand.NextBool(100))
            {
                string[] lines = { "嘻嘻，来玩呀~", "不要欺负我哦~", "兔子急了也会咬人的！" };
                CombatText.NewText(npc.getRect(), Color.Pink, lines[Main.rand.Next(lines.Length)], true);
            }
        }

        private void ExecuteBlackTuCombat(NPC npc, Player target, float dist)
        {
            if (_attackTimer % 35 == 0 && dist < 500f)
            {
                Vector2 dir = Vector2.Normalize(target.Center - npc.Center);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 10f,
                    ProjectileID.ShadowFlame, npc.damage / 3, 4f, Main.myPlayer);
            }

            if (_attackTimer % 70 == 0 && dist < 400f)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * MathHelper.TwoPi / 8;
                    Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 6f,
                        ProjectileID.ShadowFlame, npc.damage / 5, 3f, Main.myPlayer);
                }
            }

            if (_attackTimer % 120 == 0 && dist < 350f)
            {
                Vector2 dir = Vector2.Normalize(target.Center - npc.Center);
                for (int i = 0; i < 5; i++)
                {
                    Projectile.NewProjectile(npc.GetSource_FromAI(),
                        npc.Center + new Vector2(Main.rand.Next(-50, 50), Main.rand.Next(-50, 50)),
                        dir * 8f, ProjectileID.ShadowFlame, npc.damage / 3, 4f, Main.myPlayer);
                }
                target.AddBuff(BuffID.Darkness, 120);
            }

            if (Main.rand.NextBool(90))
            {
                string[] lines = { "黑暗……才是真实的。", "在暗影中颤抖吧！", "白兔已经死了，现在……是黑菟。" };
                CombatText.NewText(npc.getRect(), DarkPurpleColor, lines[Main.rand.Next(lines.Length)], true);
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;

            if (IsBlackTuForm)
            {
                return attitude switch
                {
                    GuAttitude.Hostile => "黑菟冷冷地说：\"黑暗……才是真实的。你，不配站在我面前。\"",
                    GuAttitude.Wary => "黑菟审视着你：\"你察觉到了？呵，敏锐的蝼蚁。\"",
                    GuAttitude.Friendly => "黑菟冷笑：\"友好？在黑暗中，没有友好，只有利益。\"",
                    GuAttitude.Respectful => "黑菟微微点头：\"你有些实力。影宗需要这样的人。\"",
                    GuAttitude.Contemptuous => "黑菟不屑道：\"白兔的温柔？那不过是伪装。\"",
                    GuAttitude.Fearful => "黑菟咬牙道：\"就算死，也要拉着你们一起坠入黑暗！\"",
                    _ => "黑菟看了你一眼，眼中满是黑暗。"
                };
            }

            return attitude switch
            {
                GuAttitude.Hostile => "白兔姑娘害怕地说：\"呜……你为什么要欺负我？\"",
                GuAttitude.Wary => "白兔姑娘警惕地看着你：\"嘻嘻，你不会是坏人吧？\"",
                GuAttitude.Friendly => NumberOfTimesTalkedTo <= 1
                    ? "白兔姑娘笑嘻嘻地说：\"嘻嘻，来玩呀~\""
                    : "白兔姑娘开心地说：\"又见面了！嘻嘻~\"",
                GuAttitude.Respectful => "白兔姑娘崇拜地看着你：\"你好厉害！能教我吗？\"",
                GuAttitude.Contemptuous => "白兔姑娘嘟着嘴：\"哼，不理你了！\"",
                GuAttitude.Fearful => "白兔姑娘颤抖着说：\"呜呜……不要伤害我……\"",
                _ => "白兔姑娘歪着头看你：\"嗯？\""
            };
        }

        private void InitializeDialogueTree()
        {
            var b = new DialogueTreeBuilder("BaiTuGuNiang", "greeting");

            b.StartNode("greeting", "白兔姑娘笑嘻嘻地说：\"嘻嘻，来玩呀~\"")
                .AddOption("你是谁？", "who_are_you", DialogueOptionType.Informative)
                .AddOption("你看起来很天真", "innocent", DialogueOptionType.Social)
                .AddOption("关于影宗……", "about_shadow", DialogueOptionType.Risky)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("who_are_you",
                "白兔姑娘歪着头：\"我是白兔姑娘呀！六转蛊师，嘻嘻~\n\n我……我没有什么特别的，就是喜欢玩，喜欢交朋友！\"")
                .AddOption("你真的这么天真吗？", "innocent")
                .AddOption("六转？不错了", "about_rank")
                .AddOption("我了解了", "greeting");

            b.StartNode("innocent",
                "白兔姑娘眨了眨眼，表情一瞬间变得诡异，又立刻恢复了笑容：\"天真？嘻嘻，也许吧。\n\n不过……天真的人，活得比较开心哦。你说对不对？\"")
                .AddOption("你刚才的表情……", "dark_hint", DialogueOptionType.Risky)
                .AddOption("说得对", "agree")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("dark_hint",
                "白兔姑娘的笑容消失了片刻，低声说：\"你看到了？……嘻嘻，什么都没看到哦。\n\n白兔是白兔，黑菟是黑菟。我们是两个不同的人……或者说，是同一个人的两面。\"")
                .AddOption("黑菟？", "about_heitu", DialogueOptionType.Risky)
                .AddOption("我不会追问", "respect_secret")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("about_heitu",
                "白兔姑娘的声音变低了：\"黑菟……是我的另一面。暗道七转蛊师，影宗的成员。\n\n当你看到黑菟的时候……说明我已经不需要伪装了。黑暗……才是真实的。\"")
                .AddOption("影宗？", "about_shadow", DialogueOptionType.Risky)
                .AddOption("我了解了", "respect_secret");

            b.StartNode("about_shadow",
                "白兔姑娘的表情变得复杂：\"影宗……是我的归属。在那里，我不需要伪装天真。\n\n影宗给了我力量，也给了我……另一个自己。\"")
                .AddOption("你愿意为影宗做什么？", "shadow_dedication")
                .AddOption("我了解了", "respect_secret");

            b.StartNode("shadow_dedication",
                "白兔姑娘/黑菟的声音交替出现：\"影宗的任务……就是我的使命。无论是白兔的伪装，还是黑菟的暗杀——\n\n都是影宗的一部分。嘻嘻……黑暗，才是真实的。\"")
                .AddOption("我理解了", "respect_secret")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("respect_secret",
                "白兔姑娘恢复了笑容：\"嘻嘻，你是个好人！不会把我的秘密说出去的，对吧？\n\n来玩呀~\"")
                .AddOption("继续聊", "greeting")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("about_rank",
                "白兔姑娘骄傲地说：\"六转！虽然不是最强的，但也不弱了！\n\n而且……嘻嘻，我还有更强的形态哦。不过那个……你不会想看到的。\"")
                .AddOption("更强的形态？", "dark_hint", DialogueOptionType.Risky)
                .AddOption("我了解了", "greeting");

            b.StartNode("agree",
                "白兔姑娘开心地笑了：\"对吧对吧！开开心心的多好~\n\n嘻嘻，来玩呀~\"")
                .AddOption("继续聊", "greeting")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("bye", "白兔姑娘挥手：\"嘻嘻，下次再来玩呀~\"")
                .EndsDialogue();

            DialogueTreeManager.Instance.RegisterTree<BaiTuGuNiang>(b.Build());
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qiRealm = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel <= 0) return 0f;
            return 0.003f;
        }

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);
            tag["isBlackTuForm"] = _isBlackTuForm;
            tag["hasTransformed"] = _hasTransformed;
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            _isBlackTuForm = tag.GetBool("isBlackTuForm");
            _hasTransformed = tag.GetBool("hasTransformed");
        }
    }
}
