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
    public class HongLianMoZun : ModNPC
    {
        public bool SecondStage
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value ? 1f : 0f;
        }

        public ref float TimeSlowTimer => ref NPC.localAI[0];
        public ref float TimeReverseTimer => ref NPC.localAI[1];
        public ref float TimeBlastTimer => ref NPC.localAI[2];
        public ref float GeneralTimer => ref NPC.localAI[3];

        private int rewindCooldown = 0;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Slow] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 70;
            NPC.height = 90;
            NPC.damage = 440;
            NPC.defense = 170;
            NPC.lifeMax = 85000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 44);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement("宙道尊者，摧毁宿命蛊，留下七道红莲真传。操控时间流速。")
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 16, 42));
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownBossSystem.downedHongLianMoZun, -1);
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
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.CrimsonTorch, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5));
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

            if (rewindCooldown > 0)
                rewindCooldown--;

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
                    CombatText.NewText(NPC.Hitbox, Color.Crimson, "红莲真传……时间回溯！", true);
                }
            }
        }

        private void DoFirstStage(Player player)
        {
            Vector2 hoverPos = player.Center + new Vector2(Math.Sign(NPC.Center.X - player.Center.X) * 280, -200);
            Vector2 toHover = hoverPos - NPC.Center;
            Vector2 toHoverNorm = toHover.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(7f, toHover.Length() / 25f);
            NPC.velocity = (NPC.velocity * 25f + toHoverNorm * speed) / 26f;

            TimeSlowTimer++;
            if (TimeSlowTimer > 180 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                TimeSlowTimer = 0;
                ApplyTimeSlow(player);
            }

            TimeBlastTimer++;
            if (TimeBlastTimer > 120 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                TimeBlastTimer = 0;
                SpawnTimeBlast(player);
            }

            NPC.damage = NPC.defDamage;
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void DoSecondStage(Player player)
        {
            float orbitAngle = GeneralTimer * 0.02f;
            Vector2 orbitPos = player.Center + new Vector2((float)Math.Cos(orbitAngle) * 250, -180 + (float)Math.Sin(orbitAngle * 2) * 80);
            Vector2 toOrbit = orbitPos - NPC.Center;
            Vector2 toOrbitNorm = toOrbit.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(10f, toOrbit.Length() / 18f);
            NPC.velocity = (NPC.velocity * 20f + toOrbitNorm * speed) / 21f;

            TimeSlowTimer++;
            if (TimeSlowTimer > 120 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                TimeSlowTimer = 0;
                ApplyTimeSlow(player);
            }

            TimeBlastTimer++;
            if (TimeBlastTimer > 80 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                TimeBlastTimer = 0;
                SpawnTimeBlast(player);
            }

            TimeReverseTimer++;
            if (TimeReverseTimer > 360 && rewindCooldown <= 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                TimeReverseTimer = 0;
                RewindTime();
            }

            NPC.damage = (int)(NPC.defDamage * 1.2f);
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void ApplyTimeSlow(Player player)
        {
            float dist = Vector2.Distance(NPC.Center, player.Center);
            if (dist < 800f)
            {
                player.AddBuff(BuffID.Slow, 180);
                player.AddBuff(BuffID.Webbed, 60);
                if (Main.netMode != NetmodeID.Server)
                {
                    CombatText.NewText(player.Hitbox, Color.Crimson, "时间……减速", false);
                    for (int i = 0; i < 10; i++)
                    {
                        Dust.NewDustDirect(player.position, player.width, player.height, DustID.CrimsonTorch, 0, -2f, Scale: 1.5f);
                    }
                }
            }
        }

        private void SpawnTimeBlast(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 4;
            Vector2 toPlayer = player.Center - NPC.Center;
            float baseAngle = toPlayer.ToRotation();
            for (int i = -2; i <= 2; i++)
            {
                float angle = baseAngle + i * 0.2f;
                Vector2 vel = angle.ToRotationVector2() * 9f;
                Projectile.NewProjectile(entitySource, NPC.Center, vel, ProjectileID.CultistBossIceMist, damage, 1f, Main.myPlayer);
            }
        }

        private void RewindTime()
        {
            int healAmount = NPC.lifeMax / 10;
            NPC.life = Math.Min(NPC.life + healAmount, NPC.lifeMax);
            NPC.HealEffect(healAmount);
            rewindCooldown = 600;
            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(NPC.Hitbox, Color.Crimson, "红莲真传——时间回溯！", true);
                for (int i = 0; i < 30; i++)
                {
                    float angle = MathHelper.TwoPi / 30 * i;
                    Dust.NewDustDirect(NPC.Center + angle.ToRotationVector2() * 80, 10, 10, DustID.CrimsonTorch, 0, -3f, Scale: 2f);
                }
            }
        }
    }
}
