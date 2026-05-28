using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Items.Materials;
using System.Collections.Generic;

namespace VerminLordMod.Content.NPCs.Venerables
{
    [AutoloadBossHead]
    public class JuYangXianZun : ModNPC
    {
        public bool SecondStage
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value ? 1f : 0f;
        }

        public ref float LuckZoneTimer => ref NPC.localAI[0];
        public ref float FortuneAttackTimer => ref NPC.localAI[1];
        public ref float DescendantTimer => ref NPC.localAI[2];
        public ref float GeneralTimer => ref NPC.localAI[3];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Ichor] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 90;
            NPC.height = 100;
            NPC.damage = 470;
            NPC.defense = 185;
            NPC.lifeMax = 95000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 47);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement("运道尊者，创建长生天，后裔最多。运道加持，祸福相依。")
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 18, 48));
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownBossSystem.downedJuYangXianZun, -1);
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 20f, 6f, 20, 1000f, FullName);
                Main.instance.CameraModifiers.Add(modifier);
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            return true;
        }

        public override void FindFrame(int frameHeight)
        {
            int startFrame = 0;
            int finalFrame = 2;
            if (SecondStage)
            {
                startFrame = 3;
                finalFrame = Main.npcFrameCount[NPC.type] - 1;
                if (NPC.frame.Y < startFrame * frameHeight)
                    NPC.frame.Y = startFrame * frameHeight;
            }
            int frameSpeed = 5;
            NPC.frameCounter += 0.5f;
            NPC.frameCounter += NPC.velocity.Length() / 10f;
            if (NPC.frameCounter > frameSpeed)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y > finalFrame * frameHeight)
                    NPC.frame.Y = startFrame * frameHeight;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.netMode == NetmodeID.Server) return;
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GoldFlame, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5));
                }
            }
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];
            if (player.dead)
            {
                NPC.velocity.Y -= 0.04f;
                NPC.EncourageDespawn(10);
                return;
            }

            CheckSecondStage();
            GeneralTimer++;

            if (SecondStage)
                DoSecondStage(player);
            else
                DoFirstStage(player);
        }

        private void CheckSecondStage()
        {
            if (SecondStage) return;
            if (NPC.life < NPC.lifeMax * 0.5f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                SecondStage = true;
                NPC.netUpdate = true;
                if (Main.netMode != NetmodeID.Server)
                {
                    CombatText.NewText(NPC.Hitbox, Color.Gold, "运道加持——家天下！", true);
                }
            }
        }

        private void DoFirstStage(Player player)
        {
            Vector2 hoverPos = player.Center + new Vector2(Math.Sign(NPC.Center.X - player.Center.X) * 260, -200);
            Vector2 toHover = hoverPos - NPC.Center;
            Vector2 toHoverNorm = toHover.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(6f, toHover.Length() / 25f);
            NPC.velocity = (NPC.velocity * 25f + toHoverNorm * speed) / 26f;

            LuckZoneTimer++;
            if (LuckZoneTimer > 200 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                LuckZoneTimer = 0;
                SpawnLuckZone(player);
            }

            FortuneAttackTimer++;
            if (FortuneAttackTimer > 120 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                FortuneAttackTimer = 0;
                SpawnFortuneAttack(player);
            }

            NPC.damage = NPC.defDamage;
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void DoSecondStage(Player player)
        {
            float orbitAngle = GeneralTimer * 0.016f;
            Vector2 orbitPos = player.Center + new Vector2((float)Math.Cos(orbitAngle) * 280, -180 + (float)Math.Sin(orbitAngle * 2) * 70);
            Vector2 toOrbit = orbitPos - NPC.Center;
            Vector2 toOrbitNorm = toOrbit.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(9f, toOrbit.Length() / 18f);
            NPC.velocity = (NPC.velocity * 20f + toOrbitNorm * speed) / 21f;

            LuckZoneTimer++;
            if (LuckZoneTimer > 140 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                LuckZoneTimer = 0;
                SpawnLuckZone(player);
            }

            FortuneAttackTimer++;
            if (FortuneAttackTimer > 80 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                FortuneAttackTimer = 0;
                SpawnFortuneAttack(player);
            }

            DescendantTimer++;
            if (DescendantTimer > 300 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                DescendantTimer = 0;
                SpawnDescendants(player);
            }

            NPC.damage = (int)(NPC.defDamage * 1.25f);
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void SpawnLuckZone(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            Vector2 zoneCenter = player.Center + new Vector2(Main.rand.Next(-300, 300), Main.rand.Next(-200, 200));
            bool isGoodLuck = Main.rand.NextBool();
            if (isGoodLuck)
            {
                NPC.life = Math.Min(NPC.life + NPC.lifeMax / 20, NPC.lifeMax);
                NPC.HealEffect(NPC.lifeMax / 20);
                NPC.AddBuff(BuffID.Sharpened, 180);
                if (Main.netMode != NetmodeID.Server)
                {
                    CombatText.NewText(NPC.Hitbox, Color.Gold, "大吉！", true);
                    for (int i = 0; i < 15; i++)
                    {
                        Dust.NewDustDirect(zoneCenter, 20, 20, DustID.GoldFlame, Main.rand.NextFloat(-2, 2), -2f, Scale: 2f);
                    }
                }
            }
            else
            {
                int damage = NPC.damage / 3;
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi / 8 * i;
                    Vector2 vel = angle.ToRotationVector2() * 6f;
                    Projectile.NewProjectile(entitySource, zoneCenter, vel, ProjectileID.GoldenShowerFriendly, damage, 1f, Main.myPlayer);
                }
                player.AddBuff(BuffID.Cursed, 120);
                if (Main.netMode != NetmodeID.Server)
                {
                    CombatText.NewText(player.Hitbox, Color.Red, "大凶！", true);
                }
            }
        }

        private void SpawnFortuneAttack(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 4;
            int luck = Main.rand.Next(4);
            switch (luck)
            {
                case 0:
                    for (int i = 0; i < 16; i++)
                    {
                        float angle = MathHelper.TwoPi / 16 * i;
                        Vector2 vel = angle.ToRotationVector2() * 5f;
                        Projectile.NewProjectile(entitySource, NPC.Center, vel, ProjectileID.GoldenShowerFriendly, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 1:
                    Vector2 toPlayer = player.Center - NPC.Center;
                    float baseAngle = toPlayer.ToRotation();
                    for (int i = -5; i <= 5; i++)
                    {
                        float angle = baseAngle + i * 0.08f;
                        Vector2 vel = angle.ToRotationVector2() * 12f;
                        Projectile.NewProjectile(entitySource, NPC.Center, vel, ProjectileID.Meteor1, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 2:
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 spawnPos = player.Center + new Vector2(Main.rand.Next(-300, 300), -500);
                        Vector2 vel = new Vector2(Main.rand.NextFloat(-1, 1), 6f);
                        Projectile.NewProjectile(entitySource, spawnPos, vel, ProjectileID.CultistBossLightningOrb, damage, 1f, Main.myPlayer);
                    }
                    break;
                default:
                    NPC.life = Math.Min(NPC.life + NPC.lifeMax / 25, NPC.lifeMax);
                    NPC.HealEffect(NPC.lifeMax / 25);
                    break;
            }
        }

        private void SpawnDescendants(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int count = Main.rand.Next(3, 6);
            for (int i = 0; i < count; i++)
            {
                Vector2 spawnPos = NPC.Center + new Vector2(Main.rand.Next(-100, 100), Main.rand.Next(-50, 50));
                int damage = NPC.damage / 6;
                float angle = (player.Center - spawnPos).ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 vel = angle.ToRotationVector2() * 8f;
                Projectile.NewProjectile(entitySource, spawnPos, vel, ProjectileID.CultistBossIceMist, damage, 1f, Main.myPlayer);
            }
            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(NPC.Hitbox, Color.Gold, "家天下——子孙万代！", true);
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GoldFlame, Main.rand.NextFloat(-3, 3), -2f, Scale: 1.5f);
                }
            }
        }
    }
}
