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
    public class LongGongBoss : ModNPC
    {
        // 阶段控制：0=一阶段，1=二阶段（龙公强化），2=三阶段（最终）
        public float Phase { get => NPC.ai[0]; set => NPC.ai[0] = value; }
        public float AttackTimer { get => NPC.localAI[0]; set => NPC.localAI[0] = value; }
        public float SpecialTimer { get => NPC.localAI[1]; set => NPC.localAI[1] = value; }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
        }

        public override void SetDefaults()
        {
            NPC.width = 60;
            NPC.height = 80;
            NPC.damage = 80;
            NPC.defense = 40;
            NPC.lifeMax = 50000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 50);
            NPC.SpawnWithHigherTime(30);
            NPC.npcSlots = 15f;
            NPC.boss = true;
            NPC.netAlways = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement("龙公——天庭最强战力之一，宿命的守护者。")
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
                    NPC.velocity.Y -= 0.5f;
                    if (NPC.timeLeft > 10) NPC.timeLeft = 10;
                    return;
                }
            }

            AttackTimer++;
            SpecialTimer++;

            if (Phase == 0)
            {
                // 一阶段：龙公·常态
                NPC.damage = 80;
                NPC.defense = 40;

                // 环绕飞行
                float angle = SpecialTimer * 0.015f;
                Vector2 circleCenter = target.Center + new Vector2(0, -250);
                Vector2 desiredPos = circleCenter + new Vector2((float)Math.Cos(angle) * 350, (float)Math.Sin(angle) * 150);
                Vector2 moveDir = desiredPos - NPC.Center;
                moveDir.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, moveDir * 5f, 0.04f);

                // 宿命之丝
                if (AttackTimer % 45 == 0)
                {
                    Vector2 shootDir = target.Center - NPC.Center;
                    shootDir.Normalize();
                    for (int i = -2; i <= 2; i++)
                    {
                        float spreadAngle = shootDir.ToRotation() + i * 0.15f;
                        Vector2 spread = new Vector2((float)Math.Cos(spreadAngle), (float)Math.Sin(spreadAngle)) * 10f;
                        Projectile.NewProjectile(null, NPC.Center, spread, ProjectileID.CultistBossLightningOrbArc, 55, 4f);
                    }
                }

                // 天庭神雷
                if (AttackTimer % 120 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 lightningPos = target.Center + new Vector2(Main.rand.Next(-300, 300), -500);
                        Projectile.NewProjectile(null, lightningPos, new Vector2(0, 15f), ProjectileID.CultistBossIceMist, 70, 5f);
                    }
                }

                // 进入二阶段
                if (NPC.life < NPC.lifeMax * 0.6f)
                {
                    Phase = 1;
                    AttackTimer = 0;
                    SpecialTimer = 0;
                    NPC.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);
                    Main.NewText("[龙公] \"宿命不可违！\"", new Color(255, 215, 0));
                }
            }
            else if (Phase == 1)
            {
                // 二阶段：龙公·宿命之力
                NPC.damage = 100;
                NPC.defense = 50;

                // 快速冲撞
                if (AttackTimer % 180 < 50)
                {
                    Vector2 dashDir = target.Center - NPC.Center;
                    dashDir.Normalize();
                    NPC.velocity = Vector2.Lerp(NPC.velocity, dashDir * 14f, 0.12f);
                }
                else if (AttackTimer % 180 < 80)
                {
                    NPC.velocity *= 0.9f;
                }
                else
                {
                    float angle = SpecialTimer * 0.02f;
                    Vector2 circleCenter = target.Center + new Vector2(0, -200);
                    Vector2 desiredPos = circleCenter + new Vector2((float)Math.Cos(angle) * 250, (float)Math.Sin(angle) * 100);
                    Vector2 moveDir = desiredPos - NPC.Center;
                    moveDir.Normalize();
                    NPC.velocity = Vector2.Lerp(NPC.velocity, moveDir * 6f, 0.05f);
                }

                // 宿命之丝（密集）
                if (AttackTimer % 30 == 0)
                {
                    Vector2 shootDir = target.Center - NPC.Center;
                    shootDir.Normalize();
                    for (int i = -3; i <= 3; i++)
                    {
                        float spreadAngle = shootDir.ToRotation() + i * 0.12f;
                        Vector2 spread = new Vector2((float)Math.Cos(spreadAngle), (float)Math.Sin(spreadAngle)) * 11f;
                        Projectile.NewProjectile(null, NPC.Center, spread, ProjectileID.CultistBossLightningOrbArc, 60, 4f);
                    }
                }

                // 宿命锁定（追踪弹）
                if (AttackTimer % 90 == 0)
                {
                    Vector2 homingDir = target.Center - NPC.Center;
                    homingDir.Normalize();
                    Projectile.NewProjectile(null, NPC.Center, homingDir * 6f, ProjectileID.CultistBossFireBall, 80, 5f);
                }

                // 进入三阶段
                if (NPC.life < NPC.lifeMax * 0.25f)
                {
                    Phase = 2;
                    AttackTimer = 0;
                    SpecialTimer = 0;
                    NPC.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);
                    Main.NewText("[龙公] \"天意不可逆！你……竟敢逆天而行！\"", new Color(255, 50, 50));
                }
            }
            else
            {
                // 三阶段：龙公·天意降临（最终）
                NPC.damage = 120;
                NPC.defense = 30;

                // 疯狂冲撞
                if (AttackTimer % 120 < 40)
                {
                    Vector2 dashDir = target.Center - NPC.Center;
                    dashDir.Normalize();
                    NPC.velocity = Vector2.Lerp(NPC.velocity, dashDir * 18f, 0.2f);
                }
                else
                {
                    NPC.velocity *= 0.92f;
                }

                // 全方位弹幕
                if (AttackTimer % 20 == 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi / 12 * i + AttackTimer * 0.03f;
                        Vector2 shootDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                        Projectile.NewProjectile(null, NPC.Center, shootDir, ProjectileID.CultistBossLightningOrbArc, 65, 4f);
                    }
                }

                // 天意审判（大范围AOE）
                if (AttackTimer % 150 == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 lightningPos = target.Center + new Vector2(Main.rand.Next(-400, 400), -600);
                        Projectile.NewProjectile(null, lightningPos, new Vector2(0, 18f), ProjectileID.CultistBossIceMist, 90, 6f);
                    }
                }
            }

            NPC.spriteDirection = NPC.Center.X < target.Center.X ? 1 : -1;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1;
            if (NPC.frameCounter >= 5)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= Main.npcFrameCount[Type] * frameHeight)
                    NPC.frame.Y = 0;
            }
        }

        public override void OnKill()
        {
            DownBossSystem.defeatedLongGong = true;
            Main.NewText("[蛊世界] 龙公已被击败！宿命……碎了！", new Color(255, 215, 0));

            Item.NewItem(null, NPC.getRect(), ModContent.ItemType<Content.Items.Consumables.YuanS>(), Main.rand.Next(50, 100));
            Item.NewItem(null, NPC.getRect(), ItemID.GoldCoin, Main.rand.Next(10, 30));
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            if (Phase >= 1) modifiers.FinalDamage *= 1.3f;
            if (Phase >= 2) modifiers.FinalDamage *= 1.2f;
        }

        public override bool CheckActive() => false;

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.SuperHealingPotion;
        }
    }
}
