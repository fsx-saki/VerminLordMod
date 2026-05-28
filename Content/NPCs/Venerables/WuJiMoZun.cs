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
    public class WuJiMoZun : ModNPC
    {
        public bool SecondStage
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value ? 1f : 0f;
        }

        public bool LawInversionActive
        {
            get => NPC.ai[1] == 1f;
            set => NPC.ai[1] = value ? 1f : 0f;
        }

        public ref float LawZoneTimer => ref NPC.localAI[0];
        public ref float LawBladeTimer => ref NPC.localAI[1];
        public ref float InversionTimer => ref NPC.localAI[2];
        public ref float GeneralTimer => ref NPC.localAI[3];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 80;
            NPC.height = 100;
            NPC.damage = 480;
            NPC.defense = 190;
            NPC.lifeMax = 95000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 48);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement("律道尊者，创立疯魔窟，研究生死之道。律法领域内规则逆转。")
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 18, 48));
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownBossSystem.downedWuJiMoZun, -1);
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
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PurpleTorch, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5));
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

            if (LawInversionActive)
            {
                InversionTimer++;
                if (InversionTimer > 300)
                {
                    LawInversionActive = false;
                    InversionTimer = 0;
                    if (Main.netMode != NetmodeID.Server)
                    {
                        CombatText.NewText(NPC.Hitbox, Color.Purple, "律法领域……消散", false);
                    }
                }
                ApplyLawInversion(player);
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
                    CombatText.NewText(NPC.Hitbox, Color.Purple, "生死之间……方见大道！", true);
                }
            }
        }

        private void DoFirstStage(Player player)
        {
            Vector2 hoverPos = player.Center + new Vector2(Math.Sign(NPC.Center.X - player.Center.X) * 250, -180);
            Vector2 toHover = hoverPos - NPC.Center;
            Vector2 toHoverNorm = toHover.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(7f, toHover.Length() / 25f);
            NPC.velocity = (NPC.velocity * 25f + toHoverNorm * speed) / 26f;

            LawZoneTimer++;
            if (LawZoneTimer > 200 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                LawZoneTimer = 0;
                SpawnLawZone(player);
            }

            LawBladeTimer++;
            if (LawBladeTimer > 130 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                LawBladeTimer = 0;
                SpawnLawBlades(player);
            }

            NPC.damage = NPC.defDamage;
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void DoSecondStage(Player player)
        {
            float orbitAngle = GeneralTimer * 0.018f;
            Vector2 orbitPos = player.Center + new Vector2((float)Math.Cos(orbitAngle) * 200, -150 + (float)Math.Sin(orbitAngle * 3) * 50);
            Vector2 toOrbit = orbitPos - NPC.Center;
            Vector2 toOrbitNorm = toOrbit.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(11f, toOrbit.Length() / 15f);
            NPC.velocity = (NPC.velocity * 18f + toOrbitNorm * speed) / 19f;

            LawZoneTimer++;
            if (LawZoneTimer > 150 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                LawZoneTimer = 0;
                SpawnLawZone(player);
            }

            LawBladeTimer++;
            if (LawBladeTimer > 90 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                LawBladeTimer = 0;
                SpawnLawBlades(player);
            }

            if (!LawInversionActive && GeneralTimer % 360 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                LawInversionActive = true;
                InversionTimer = 0;
                if (Main.netMode != NetmodeID.Server)
                {
                    CombatText.NewText(NPC.Hitbox, Color.Magenta, "律法领域——规则逆转！", true);
                }
            }

            NPC.damage = (int)(NPC.defDamage * 1.25f);
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void ApplyLawInversion(Player player)
        {
            float dist = Vector2.Distance(NPC.Center, player.Center);
            if (dist < 600f)
            {
                player.AddBuff(BuffID.Cursed, 30);
                if (GeneralTimer % 30 == 0)
                {
                    int healAmount = player.statLifeMax2 / 20;
                    NPC.HealEffect(healAmount);
                    NPC.life = Math.Min(NPC.life + healAmount, NPC.lifeMax);
                }
            }
            if (GeneralTimer % 5 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float radius = Main.rand.NextFloat() * 300f;
                    Vector2 dustPos = NPC.Center + angle.ToRotationVector2() * radius;
                    Dust.NewDustDirect(dustPos, 10, 10, DustID.PurpleTorch, 0, -2f, Scale: 1.5f);
                }
            }
        }

        private void SpawnLawZone(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 5;
            Vector2 zoneCenter = player.Center + new Vector2(Main.rand.Next(-200, 200), Main.rand.Next(-100, 100));
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8 * i;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                Projectile.NewProjectile(entitySource, zoneCenter, vel, ProjectileID.SpiritHeal, 0, 0f, Main.myPlayer);
            }
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi / 16 * i;
                Vector2 vel = angle.ToRotationVector2() * 5f;
                Projectile.NewProjectile(entitySource, zoneCenter, vel, ProjectileID.CultistBossIceMist, damage, 1f, Main.myPlayer);
            }
        }

        private void SpawnLawBlades(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 4;
            Vector2 toPlayer = player.Center - NPC.Center;
            float baseAngle = toPlayer.ToRotation();
            for (int i = 0; i < 6; i++)
            {
                float angle = baseAngle + MathHelper.Pi / 3 * i;
                Vector2 vel = angle.ToRotationVector2() * 8f;
                Projectile.NewProjectile(entitySource, NPC.Center, vel, ProjectileID.DemonScythe, damage, 1f, Main.myPlayer);
            }
        }
    }
}
