using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.NPCs.GuMasters;
using VerminLordMod.Content.Projectiles;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Content.NPCs.Heroines
{
    [AutoloadHead]
    public class BaiNingBing : GuMasterBase
    {
        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.Zhuan7_Gao;
        public override GuPersonality GetPersonality() => GuPersonality.Proud;

        public override string GuMasterDisplayName => "白凝冰";
        public override int GuMasterDamage => 350;
        public override int GuMasterLife => 55000;
        public override int GuMasterDefense => 150;

        private int _attackTimer;
        private int _selfDestructTimer;
        private bool _selfDestructActive;
        private bool _treeInitialized;

        private const int IceBurstCooldown = 90;
        private const int IceShardCooldown = 45;
        private const float SelfDestructThreshold = 0.2f;
        private const int SelfDestructHPDrain = 50;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 800;
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
            NPC.knockBackResist = 0.2f;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath7;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(0, 5, 0, 0);
            NPC.townNPC = false;
            NPC.friendly = false;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
        }

        public override void AI()
        {
            if (!_treeInitialized)
            {
                InitializeDialogueTree();
                _treeInitialized = true;
            }

            base.AI();

            if (_selfDestructActive)
            {
                _selfDestructTimer++;
                if (_selfDestructTimer % 30 == 0)
                {
                    NPC.life -= SelfDestructHPDrain;
                    if (NPC.life <= 0)
                    {
                        NPC.life = 1;
                        IceExplode();
                    }
                }
                for (int i = 0; i < 3; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.IceTorch,
                        Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, 0f),
                        100, Color.LightCyan, 1.5f);
                }
            }
        }

        public override void ExecuteCombatAI(NPC npc)
        {
            var target = Main.player[npc.target];
            float dist = Vector2.Distance(npc.Center, target.Center);

            npc.spriteDirection = target.Center.X > npc.Center.X ? 1 : -1;

            if (!_selfDestructActive && (float)npc.life / npc.lifeMax < SelfDestructThreshold)
            {
                _selfDestructActive = true;
                _selfDestructTimer = 0;
                npc.damage = (int)(npc.damage * 2.5f);
                npc.defense = (int)(npc.defense * 0.5f);
                CombatText.NewText(npc.getRect(), Color.Cyan, "北冥冰魄——爆！", true);
                Say("北冥冰魄体，全力爆发！", Color.Cyan);
            }

            if (dist < 120f)
            {
                float fleeDir = npc.Center.X > target.Center.X ? 1 : -1;
                npc.velocity.X = fleeDir * 2.5f;
            }
            else if (dist > 350f)
            {
                float dir = target.Center.X > npc.Center.X ? 1 : -1;
                npc.velocity.X = dir * 2f;
            }
            else
            {
                npc.velocity.X *= 0.9f;
            }

            if (npc.collideX && npc.velocity.Y == 0)
                npc.velocity.Y = -7f;

            _attackTimer++;
            int shardCD = _selfDestructActive ? IceShardCooldown / 2 : IceShardCooldown;
            int burstCD = _selfDestructActive ? IceBurstCooldown / 2 : IceBurstCooldown;

            if (_attackTimer % shardCD == 0 && dist < 500f)
            {
                Vector2 dir = Vector2.Normalize(target.Center - npc.Center);
                float speed = _selfDestructActive ? 12f : 9f;
                Vector2 vel = dir * speed;
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, vel,
                    ProjectileID.FrostShard, npc.damage / 4, 3f, Main.myPlayer);
            }

            if (_attackTimer % burstCD == 0 && dist < 300f)
            {
                IceBurstAttack(npc, target);
            }

            if (_selfDestructActive && _attackTimer % 60 == 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 dir = new Vector2((float)Math.Cos(i * MathHelper.PiOver4), (float)Math.Sin(i * MathHelper.PiOver4));
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 6f,
                        ProjectileID.FrostShard, npc.damage / 5, 2f, Main.myPlayer);
                }
            }

            if (Main.rand.NextBool(150))
            {
                string[] lines = { "冰魄碎裂！", "精彩……才刚刚开始！", "北冥之力，你承受不住！" };
                CombatText.NewText(npc.getRect(), Color.Cyan, lines[Main.rand.Next(lines.Length)], true);
            }
        }

        private void IceBurstAttack(NPC npc, Player target)
        {
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.ToRadians(Main.rand.Next(-30, 30));
                Vector2 dir = Vector2.Normalize(target.Center - npc.Center);
                dir = dir.RotatedBy(angle);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * 8f,
                    ProjectileID.FrostShard, npc.damage / 3, 4f, Main.myPlayer);
            }
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(npc.Center, 10, 10, DustID.IceTorch,
                    Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f),
                    100, Color.LightCyan, 2f);
            }
        }

        private void IceExplode()
        {
            for (int i = 0; i < 16; i++)
            {
                Vector2 dir = new Vector2((float)Math.Cos(i * MathHelper.TwoPi / 16), (float)Math.Sin(i * MathHelper.TwoPi / 16));
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dir * 10f,
                    ProjectileID.FrostShard, NPC.damage / 2, 5f, Main.myPlayer);
            }
            for (int i = 0; i < 40; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.IceTorch,
                    Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f),
                    150, Color.White, 2.5f);
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => "白凝冰冷笑：\"想杀我？先问问我的冰魄答不答应！\"",
                GuAttitude.Wary => "白凝冰警惕地看着你：\"你是什么人？别挡我的路。\"",
                GuAttitude.Friendly => NumberOfTimesTalkedTo <= 1
                    ? "白凝冰微微扬起下巴：\"我白凝冰，从不走寻常路。精彩，才是人生的意义。\""
                    : "白凝冰淡淡地说：\"又见面了。这世间有趣的事不多，你算一个。\"",
                GuAttitude.Respectful => "白凝冰难得露出微笑：\"你有些本事。能让我白凝冰认可的人，不多。\"",
                GuAttitude.Contemptuous => "白凝冰不屑地瞥了你一眼：\"哼，平庸之辈。\"",
                GuAttitude.Fearful => "白凝冰咬牙道：\"就算死，我也要精彩地死！\"",
                _ => "白凝冰看了你一眼：\"……\""
            };
        }

        private void InitializeDialogueTree()
        {
            var b = new DialogueTreeBuilder("BaiNingBing", "greeting");

            b.StartNode("greeting", "白凝冰微微扬起下巴：\"我白凝冰，从不走寻常路。精彩，才是人生的意义。\"")
                .AddOption("你是谁？", "who_are_you", DialogueOptionType.Informative)
                .AddOption("十绝体是什么？", "ten_extreme", DialogueOptionType.Informative)
                .AddOption("关于方源……", "about_fangyuan", DialogueOptionType.Social)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("who_are_you",
                "白凝冰冷冷地说：\"我是白凝冰，北冥冰魄体的拥有者。曾经是白家的天才，如今……不过是一个追求精彩的散修罢了。\"")
                .AddOption("你为何离开白家？", "left_bai")
                .AddOption("十绝体很厉害吗？", "ten_extreme")
                .AddOption("我了解了", "greeting");

            b.StartNode("left_bai",
                "白凝冰眼中闪过一丝复杂：\"白家……呵。他们只看到了十绝体的威胁，却看不到其中的精彩。我选择自己的路，不需要任何人施舍。\"")
                .AddOption("你变女身……", "gender_change")
                .AddOption("我理解你", "understand")
                .AddOption("回到之前的话题", "greeting");

            b.StartNode("gender_change",
                "白凝冰坦然地说：\"不错，十绝体让我从男变女。但这又如何？身体不过是皮囊，精彩的人生不在于外表，而在于选择。我白凝冰，从不后悔。\"")
                .AddOption("你很洒脱", "understand")
                .AddOption("回到之前的话题", "greeting");

            b.StartNode("understand",
                "白凝冰微微一愣，随即露出罕见的微笑：\"能理解我的人……很少。你，有点意思。\"")
                .AddOption("继续聊", "greeting")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("ten_extreme",
                "白凝冰解释道：\"十绝体，天地间最极端的十种体质。北冥冰魄体是其中之一，拥有者天生亲和冰道，但也因此被宿命束缚。\n\n十绝体的拥有者，要么在挣扎中绽放光彩，要么在平庸中消亡。我选择前者。\"")
                .AddOption("其他十绝体呢？", "other_extreme")
                .AddOption("回到之前的话题", "greeting");

            b.StartNode("other_extreme",
                "白凝冰想了想：\"我只知道几个——影宗的巨阳体、天庭的大梦仙姿……每一个都是惊才绝艳之辈。但十绝体的命运，从来都不由自己掌控。\"")
                .AddOption("关于方源……", "about_fangyuan")
                .AddOption("我了解了", "greeting");

            b.StartNode("about_fangyuan",
                "白凝冰的表情变得复杂：\"方源……他是我见过的最特别的人。不是因为强大，而是因为……他从不走寻常路。这一点，我们很像。\n\n但我不信任他。没有人应该被完全信任。\"")
                .AddOption("你们之间发生了什么？", "fangyuan_detail")
                .AddOption("我了解了", "greeting");

            b.StartNode("fangyuan_detail",
                "白凝冰沉默了片刻：\"在青茅山，我们曾并肩作战，也曾互相算计。方源这个人……他让我看到了什么叫真正的'不择手段'。\n\n但不得不说，和他在一起的日子，确实精彩。\"")
                .AddOption("精彩才是人生的意义", "understand")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("bye", "白凝冰转身离去，留下一句话：\"记住，精彩才是人生的意义。\"")
                .EndsDialogue();

            DialogueTreeManager.Instance.RegisterTree<BaiNingBing>(b.Build());
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qiRealm = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel <= 0) return 0f;
            return Main.rand.NextBool(800) ? 0.01f : 0f;
        }

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);
            tag["selfDestructActive"] = _selfDestructActive;
            tag["selfDestructTimer"] = _selfDestructTimer;
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            _selfDestructActive = tag.GetBool("selfDestructActive");
            _selfDestructTimer = tag.GetInt("selfDestructTimer");
        }
    }
}
