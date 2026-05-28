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
    public class YuanShiXianZun : ModNPC
    {
        public bool SecondStage
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value ? 1f : 0f;
        }

        public ref float PressureWaveTimer => ref NPC.localAI[0];
        public ref float VacuumBladeTimer => ref NPC.localAI[1];
        public ref float QiBurstTimer => ref NPC.localAI[2];
        public ref float GeneralTimer => ref NPC.localAI[3];

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
        }

        public override void SetDefaults()
        {
            NPC.width = 80;
            NPC.height = 100;
            NPC.damage = 500;
            NPC.defense = 200;
            NPC.lifeMax = 100000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 50);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement("气道尊者，十大尊者之首。开创宗门体系，元始之气笼罩天地。")
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 20, 50));
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownBossSystem.downedYuanShiXianZun, -1);
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
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Cloud, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5));
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
                    CombatText.NewText(NPC.Hitbox, Color.Cyan, "元始之气……觉醒！", true);
                }
            }
        }

        private void DoFirstStage(Player player)
        {
            Vector2 toPlayer = player.Center - NPC.Center;
            float distance = toPlayer.Length();
            Vector2 desiredPos = player.Center + new Vector2(Math.Sign(toPlayer.X) * -300, -200);
            Vector2 toDesired = desiredPos - NPC.Center;
            Vector2 toDesiredNorm = toDesired.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(8f, toDesired.Length() / 30f);
            NPC.velocity = (NPC.velocity * 30f + toDesiredNorm * speed) / 31f;

            PressureWaveTimer++;
            if (PressureWaveTimer > 180 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                PressureWaveTimer = 0;
                SpawnPressureWave();
            }

            VacuumBladeTimer++;
            if (VacuumBladeTimer > 120 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                VacuumBladeTimer = 0;
                SpawnVacuumBlades(player);
            }

            NPC.damage = NPC.defDamage;
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void DoSecondStage(Player player)
        {
            Vector2 toPlayer = player.Center - NPC.Center;
            float orbitAngle = GeneralTimer * 0.02f;
            Vector2 orbitPos = player.Center + new Vector2((float)Math.Cos(orbitAngle) * 250, -150 + (float)Math.Sin(orbitAngle) * 80);
            Vector2 toOrbit = orbitPos - NPC.Center;
            Vector2 toOrbitNorm = toOrbit.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(12f, toOrbit.Length() / 15f);
            NPC.velocity = (NPC.velocity * 20f + toOrbitNorm * speed) / 21f;

            PressureWaveTimer++;
            if (PressureWaveTimer > 120 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                PressureWaveTimer = 0;
                SpawnPressureWave();
                SpawnPressureWave();
            }

            VacuumBladeTimer++;
            if (VacuumBladeTimer > 80 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                VacuumBladeTimer = 0;
                SpawnVacuumBlades(player);
                SpawnVacuumBlades(player);
            }

            QiBurstTimer++;
            if (QiBurstTimer > 300 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                QiBurstTimer = 0;
                SpawnYuanShiQiBurst(player);
            }

            NPC.damage = (int)(NPC.defDamage * 1.3f);
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void SpawnPressureWave()
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 5;
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi / 12 * i;
                Vector2 velocity = angle.ToRotationVector2() * 6f;
                Projectile.NewProjectile(entitySource, NPC.Center, velocity, ProjectileID.VortexLightning, damage, 2f, Main.myPlayer);
            }
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDustDirect(NPC.Center, 20, 20, DustID.Cloud, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
            }
        }

        private void SpawnVacuumBlades(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 4;
            Vector2 toPlayer = player.Center - NPC.Center;
            float baseAngle = toPlayer.ToRotation();
            for (int i = -2; i <= 2; i++)
            {
                float angle = baseAngle + i * 0.15f;
                Vector2 velocity = angle.ToRotationVector2() * 10f;
                Projectile.NewProjectile(entitySource, NPC.Center, velocity, ProjectileID.DemonScythe, damage, 1f, Main.myPlayer);
            }
        }

        private void SpawnYuanShiQiBurst(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 3;
            for (int wave = 0; wave < 5; wave++)
            {
                float radius = 100 + wave * 80;
                for (int i = 0; i < 16; i++)
                {
                    float angle = MathHelper.TwoPi / 16 * i + wave * 0.2f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    Vector2 vel = angle.ToRotationVector2() * 4f;
                    Projectile.NewProjectile(entitySource, pos, vel, ProjectileID.CultistBossIceMist, damage, 1f, Main.myPlayer);
                }
            }
            for (int i = 0; i < 40; i++)
            {
                Dust.NewDustDirect(NPC.Center, 40, 40, DustID.Cloud, Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8), Scale: 2f);
            }
        }
    }
}
