using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.Boss
{
    [AutoloadBossHead]
    public class DiMaiGuardian : ModNPC
    {
        public bool SecondStage { get => NPC.ai[0] == 1f; set => NPC.ai[0] = value ? 1f : 0f; }
        public float AttackTimer { get => NPC.localAI[0]; set => NPC.localAI[0] = value; }
        public float ShieldActive { get => NPC.localAI[1]; set => NPC.localAI[1] = value; }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.width = 70;
            NPC.height = 80;
            NPC.damage = 50;
            NPC.defense = 30;
            NPC.lifeMax = 12000;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath43;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.value = Item.buyPrice(gold: 15);
            NPC.SpawnWithHigherTime(30);
            NPC.npcSlots = 10f;
            NPC.boss = true;
            NPC.netAlways = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                new FlavorTextBestiaryInfoElement("地脉守护者——守护青茅山地脉的远古存在。")
            });
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.TargetClosest();
                target = Main.player[NPC.target];
                if (!target.active || target.dead)
                {
                    NPC.velocity.Y -= 0.3f;
                    if (NPC.timeLeft > 10) NPC.timeLeft = 10;
                    return;
                }
            }

            AttackTimer++;

            if (!SecondStage)
            {
                // 一阶段：缓慢移动，高防御，岩石攻击
                NPC.damage = 50;
                NPC.defense = 30;

                // 缓慢追踪
                Vector2 moveDir = target.Center - NPC.Center;
                moveDir.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, moveDir * 3f, 0.02f);

                // 岩石弹幕
                if (AttackTimer % 80 == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = MathHelper.TwoPi / 5 * i + AttackTimer * 0.01f;
                        Vector2 shootDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 6f;
                        Projectile.NewProjectile(null, NPC.Center, shootDir, ProjectileID.BoulderStaffOfEarth, 40, 4f);
                    }
                }

                // 地震波
                if (AttackTimer % 120 == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 dustVel = new Vector2(Main.rand.Next(-5, 5), Main.rand.Next(-8, -2));
                        Terraria.Dust.NewDust(NPC.Bottom, 20, 10, DustID.Stone, dustVel.X, dustVel.Y);
                    }
                    // 震退玩家
                    if (Vector2.Distance(NPC.Center, target.Center) < 400f)
                    {
                        Vector2 knockback = target.Center - NPC.Center;
                        knockback.Normalize();
                        target.velocity += knockback * 8f;
                    }
                }

                // 进入二阶段
                if (NPC.life < NPC.lifeMax * 0.5f)
                {
                    SecondStage = true;
                    AttackTimer = 0;
                    NPC.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                    NPC.defense = 15;
                }
            }
            else
            {
                // 二阶段：护盾消失，攻击更猛烈
                NPC.damage = 65;
                NPC.defense = 15;

                // 快速冲撞
                if (AttackTimer % 150 < 40)
                {
                    Vector2 dashDir = target.Center - NPC.Center;
                    dashDir.Normalize();
                    NPC.velocity = Vector2.Lerp(NPC.velocity, dashDir * 12f, 0.15f);
                }
                else
                {
                    NPC.velocity *= 0.95f;
                }

                // 岩石弹幕（更密集）
                if (AttackTimer % 50 == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8 * i + AttackTimer * 0.02f;
                        Vector2 shootDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 7f;
                        Projectile.NewProjectile(null, NPC.Center, shootDir, ProjectileID.BoulderStaffOfEarth, 45, 4f);
                    }
                }

                // 地刺
                if (AttackTimer % 90 == 0)
                {
                    Vector2 spikeDir = target.Center - NPC.Center;
                    spikeDir.Normalize();
                    for (int i = 0; i < 3; i++)
                    {
                        float delay = i * 0.15f;
                        Vector2 offset = spikeDir * (50 + i * 80);
                        Projectile.NewProjectile(null, NPC.Center + offset, spikeDir * 2f, ProjectileID.Stinger, 35, 3f);
                    }
                }
            }

            NPC.spriteDirection = NPC.Center.X < target.Center.X ? 1 : -1;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1;
            if (NPC.frameCounter >= 8)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= Main.npcFrameCount[Type] * frameHeight)
                    NPC.frame.Y = 0;
            }
        }

        public override void OnKill()
        {
            DownBossSystem.defeatedDiMaiGuardian = true;
            Main.NewText("[蛊世界] 地脉守护者已被击败！青茅山的封印打破了！", new Color(200, 150, 50));

            Item.NewItem(null, NPC.getRect(), ModContent.ItemType<Content.Items.Consumables.YuanS>(), Main.rand.Next(30, 50));
        }

        public override bool CheckActive() => false;

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }
    }
}
