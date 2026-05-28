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
    public class LeTuXianZun : ModNPC
    {
        public bool SecondStage
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value ? 1f : 0f;
        }

        public ref float BarrierTimer => ref NPC.localAI[0];
        public ref float EarthquakeTimer => ref NPC.localAI[1];
        public ref float DomainTimer => ref NPC.localAI[2];
        public ref float GeneralTimer => ref NPC.localAI[3];

        private bool domainActive = false;
        private int domainDuration = 0;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Ichor] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 90;
            NPC.height = 110;
            NPC.damage = 420;
            NPC.defense = 280;
            NPC.lifeMax = 115000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 56);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement("土道尊者，最为仁善，治愈幽魂肆虐后的世界。乐土领域，万物安宁。")
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 24, 58));
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownBossSystem.downedLeTuXianZun, -1);
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
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.BrownMoss, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5));
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

            if (domainActive)
            {
                domainDuration++;
                if (domainDuration > 420)
                {
                    domainActive = false;
                    domainDuration = 0;
                    if (Main.netMode != NetmodeID.Server)
                    {
                        CombatText.NewText(NPC.Hitbox, Color.Brown, "乐土领域……消散", false);
                    }
                }
                ApplyLeTuDomain(player);
            }

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
                    CombatText.NewText(NPC.Hitbox, Color.Brown, "乐土领域——万物安宁！", true);
                }
            }
        }

        private void DoFirstStage(Player player)
        {
            Vector2 hoverPos = player.Center + new Vector2(0, -230);
            Vector2 toHover = hoverPos - NPC.Center;
            Vector2 toHoverNorm = toHover.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(5f, toHover.Length() / 28f);
            NPC.velocity = (NPC.velocity * 28f + toHoverNorm * speed) / 29f;

            BarrierTimer++;
            if (BarrierTimer > 180 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                BarrierTimer = 0;
                SpawnEarthBarrier();
            }

            EarthquakeTimer++;
            if (EarthquakeTimer > 150 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                EarthquakeTimer = 0;
                SpawnEarthquake(player);
            }

            NPC.damage = NPC.defDamage;
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void DoSecondStage(Player player)
        {
            float orbitAngle = GeneralTimer * 0.01f;
            Vector2 orbitPos = player.Center + new Vector2((float)Math.Cos(orbitAngle) * 180, -250 + (float)Math.Sin(orbitAngle) * 30);
            Vector2 toOrbit = orbitPos - NPC.Center;
            Vector2 toOrbitNorm = toOrbit.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(7f, toOrbit.Length() / 22f);
            NPC.velocity = (NPC.velocity * 22f + toOrbitNorm * speed) / 23f;

            BarrierTimer++;
            if (BarrierTimer > 120 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                BarrierTimer = 0;
                SpawnEarthBarrier();
            }

            EarthquakeTimer++;
            if (EarthquakeTimer > 100 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                EarthquakeTimer = 0;
                SpawnEarthquake(player);
            }

            DomainTimer++;
            if (DomainTimer > 480 && !domainActive && Main.netMode != NetmodeID.MultiplayerClient)
            {
                DomainTimer = 0;
                domainActive = true;
                domainDuration = 0;
                if (Main.netMode != NetmodeID.Server)
                {
                    CombatText.NewText(NPC.Hitbox, Color.Brown, "乐土领域——展开！", true);
                }
            }

            NPC.damage = (int)(NPC.defDamage * 1.1f);
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void ApplyLeTuDomain(Player player)
        {
            float dist = Vector2.Distance(NPC.Center, player.Center);
            if (dist < 500f)
            {
                NPC.defense = NPC.defDefense + 100;
                int regenAmount = NPC.lifeMax / 40;
                if (domainDuration % 30 == 0)
                {
                    NPC.life = Math.Min(NPC.life + regenAmount, NPC.lifeMax);
                    NPC.HealEffect(regenAmount);
                }
            }
            else
            {
                NPC.defense = NPC.defDefense;
            }
            if (domainDuration % 10 == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float radius = Main.rand.NextFloat() * 250f;
                    Vector2 dustPos = NPC.Center + angle.ToRotationVector2() * radius;
                    Dust.NewDustDirect(dustPos, 10, 10, DustID.BrownMoss, 0, -1f, Scale: 1.5f);
                }
            }
        }

        private void SpawnEarthBarrier()
        {
            var entitySource = NPC.GetSource_FromAI();
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi / 12 * i;
                Vector2 pos = NPC.Center + angle.ToRotationVector2() * 120;
                Vector2 vel = angle.ToRotationVector2() * 2f;
                Projectile.NewProjectile(entitySource, pos, vel, ProjectileID.BoulderStaffOfEarth, 0, 3f, Main.myPlayer);
            }
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDustDirect(NPC.Center, 30, 30, DustID.BrownMoss, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3), Scale: 2f);
                }
            }
        }

        private void SpawnEarthquake(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 4;
            for (int i = 0; i < 8; i++)
            {
                Vector2 spawnPos = player.Center + new Vector2(Main.rand.Next(-250, 250), 400);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2, 2), -10f);
                Projectile.NewProjectile(entitySource, spawnPos, vel, ProjectileID.BoulderStaffOfEarth, damage, 2f, Main.myPlayer);
            }
            if (Main.netMode != NetmodeID.Server)
            {
                PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 8f, 3f, 8, 400f, FullName);
                Main.instance.CameraModifiers.Add(modifier);
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDustDirect(player.Center, 20, 20, DustID.BrownMoss, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-8, -2), Scale: 2f);
                }
            }
        }
    }
}
