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
    public class TianHeShangRenBoss : ModNPC
    {
        public bool SecondStage { get => NPC.ai[0] == 1f; set => NPC.ai[0] = value ? 1f : 0f; }
        public float AttackTimer { get => NPC.localAI[0]; set => NPC.localAI[0] = value; }
        public float PhaseTimer { get => NPC.localAI[1]; set => NPC.localAI[1] = value; }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
        }

        public override void SetDefaults()
        {
            NPC.width = 80;
            NPC.height = 90;
            NPC.damage = 45;
            NPC.defense = 15;
            NPC.lifeMax = 8000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 10);
            NPC.SpawnWithHigherTime(30);
            NPC.npcSlots = 10f;
            NPC.boss = true;
            NPC.netAlways = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("天鹤上人——驾驭白鹤的强者，曾袭击青茅山。")
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
            PhaseTimer++;

            float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);

            if (!SecondStage)
            {
                // 一阶段：骑鹤飞行，风刃攻击
                NPC.damage = 45;

                // 环绕飞行
                float angle = PhaseTimer * 0.02f;
                Vector2 circleCenter = target.Center + new Vector2(0, -200);
                Vector2 desiredPos = circleCenter + new Vector2((float)Math.Cos(angle) * 300, (float)Math.Sin(angle) * 100);
                Vector2 moveDir = desiredPos - NPC.Center;
                moveDir.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, moveDir * 6f, 0.05f);

                // 风刃攻击
                if (AttackTimer % 60 == 0)
                {
                    Vector2 shootDir = target.Center - NPC.Center;
                    shootDir.Normalize();
                    float speed = 8f;
                    Projectile.NewProjectile(null, NPC.Center, shootDir * speed, ProjectileID.DemonSickle, 30, 3f);
                }

                // 召唤鹤群
                if (AttackTimer % 180 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 spawnPos = NPC.Center + new Vector2(Main.rand.Next(-100, 100), Main.rand.Next(-50, 50));
                        NPC.NewNPC(null, (int)spawnPos.X, (int)spawnPos.Y, NPCID.Bird);
                    }
                }

                // 进入二阶段
                if (NPC.life < NPC.lifeMax * 0.4f)
                {
                    SecondStage = true;
                    AttackTimer = 0;
                    PhaseTimer = 0;
                    NPC.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                }
            }
            else
            {
                // 二阶段：鹤被击落，人形战斗，更猛烈的攻击
                NPC.damage = 55;
                NPC.defense = 10;

                // 快速冲向玩家
                if (AttackTimer % 120 < 60)
                {
                    Vector2 dashDir = target.Center - NPC.Center;
                    dashDir.Normalize();
                    NPC.velocity = Vector2.Lerp(NPC.velocity, dashDir * 10f, 0.1f);
                }
                else
                {
                    // 悬停
                }

                // 连续风刃
                if (AttackTimer % 30 == 0)
                {
                    Vector2 shootDir = target.Center - NPC.Center;
                    shootDir.Normalize();
                    for (int i = -1; i <= 1; i++)
                    {
                        float angle2 = shootDir.ToRotation() + i * 0.2f;
                        Vector2 spread = new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * 9f;
                        Projectile.NewProjectile(null, NPC.Center, spread, ProjectileID.DemonSickle, 35, 3f);
                    }
                }

                // 雷击
                if (AttackTimer % 90 == 0)
                {
                    Vector2 lightningPos = target.Center + new Vector2(Main.rand.Next(-200, 200), -400);
                    Projectile.NewProjectile(null, lightningPos, new Vector2(0, 12f), ProjectileID.ThunderSpearShot, 50, 5f);
                }
            }

            // 面向玩家
            NPC.spriteDirection = NPC.Center.X < target.Center.X ? 1 : -1;
        }

        public override void FindFrame(int frameHeight)
        {
            int frameSpeed = 6;
            NPC.frameCounter += 1;
            if (NPC.frameCounter >= frameSpeed)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= Main.npcFrameCount[Type] * frameHeight)
                    NPC.frame.Y = 0;
            }
        }

        public override void OnKill()
        {
            DownBossSystem.defeatedTianHeShangRen = true;
            Main.NewText("[蛊世界] 天鹤上人已被击败！", new Color(100, 200, 255));

            if (!Main.expertMode)
            {
                Item.NewItem(null, NPC.getRect(), ModContent.ItemType<Content.Items.Consumables.YuanS>(), Main.rand.Next(20, 40));
            }
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            if (SecondStage) modifiers.FinalDamage *= 1.2f;
        }

        public override bool CheckActive() => false;

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.HealingPotion;
        }
    }
}
