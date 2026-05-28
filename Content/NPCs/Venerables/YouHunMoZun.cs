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
    public class YouHunMoZun : ModNPC
    {
        public bool SecondStage
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value ? 1f : 0f;
        }

        public ref float SoulDrainTimer => ref NPC.localAI[0];
        public ref float ShadowCloneTimer => ref NPC.localAI[1];
        public ref float PhaseTimer => ref NPC.localAI[2];
        public ref float GeneralTimer => ref NPC.localAI[3];

        private int shadowCloneCount = 0;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Ichor] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 70;
            NPC.height = 90;
            NPC.damage = 530;
            NPC.defense = 195;
            NPC.lifeMax = 105000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 53);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement("魂道尊者，最为嗜杀，创立影宗。吞魂之术，万魂噬体。")
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 20, 52));
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownBossSystem.downedYouHunMoZun, -1);
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
                for (int i = 0; i < 40; i++)
                {
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Shadowflame, Main.rand.NextFloat(-6, 6), Main.rand.NextFloat(-6, 6));
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
                    CombatText.NewText(NPC.Hitbox, new Color(48, 0, 96), "十万年潜伏……只为今日！", true);
                }
            }
        }

        private void DoFirstStage(Player player)
        {
            float flickerAlpha = (float)Math.Sin(GeneralTimer * 0.05f) * 0.3f + 0.7f;
            NPC.alpha = (int)((1f - flickerAlpha) * 100);

            Vector2 hoverPos = player.Center + new Vector2(Math.Sign(NPC.Center.X - player.Center.X) * 250, -180);
            Vector2 toHover = hoverPos - NPC.Center;
            Vector2 toHoverNorm = toHover.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(7f, toHover.Length() / 22f);
            NPC.velocity = (NPC.velocity * 22f + toHoverNorm * speed) / 23f;

            SoulDrainTimer++;
            if (SoulDrainTimer > 120 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                SoulDrainTimer = 0;
                SoulDrainAttack(player);
            }

            ShadowCloneTimer++;
            if (ShadowCloneTimer > 200 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                ShadowCloneTimer = 0;
                SpawnShadowClones(player);
            }

            NPC.damage = NPC.defDamage;
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void DoSecondStage(Player player)
        {
            NPC.alpha = 0;

            float teleportChance = (float)Math.Sin(GeneralTimer * 0.03f);
            if (teleportChance > 0.9f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 newPos = player.Center + new Vector2(Main.rand.Next(-300, 300), -150);
                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Shadowflame, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
                    }
                }
                NPC.Center = newPos;
                NPC.netUpdate = true;
            }

            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNorm = toPlayer.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(12f, toPlayer.Length() / 12f);
            NPC.velocity = (NPC.velocity * 12f + toPlayerNorm * speed) / 13f;

            SoulDrainTimer++;
            if (SoulDrainTimer > 80 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                SoulDrainTimer = 0;
                SoulDrainAttack(player);
            }

            ShadowCloneTimer++;
            if (ShadowCloneTimer > 150 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                ShadowCloneTimer = 0;
                SpawnShadowClones(player);
            }

            PhaseTimer++;
            if (PhaseTimer > 360 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                PhaseTimer = 0;
                SpawnTunHunZhiShu(player);
            }

            NPC.damage = (int)(NPC.defDamage * 1.35f);
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void SoulDrainAttack(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 4;
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi / 6 * i + GeneralTimer * 0.02f;
                Vector2 spawnPos = NPC.Center + angle.ToRotationVector2() * 60;
                Vector2 toPlayer = player.Center - spawnPos;
                Vector2 vel = toPlayer.SafeNormalize(Vector2.UnitY) * 8f;
                Projectile.NewProjectile(entitySource, spawnPos, vel, ProjectileID.SpiritHeal, damage, 1f, Main.myPlayer);
            }
            int soulSteal = player.statLifeMax2 / 30;
            player.statLife = Math.Max(player.statLife - soulSteal, 1);
            player.HealEffect(-soulSteal);
            NPC.life = Math.Min(NPC.life + soulSteal, NPC.lifeMax);
            NPC.HealEffect(soulSteal);
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDustDirect(player.position, player.width, player.height, DustID.Shadowflame, 0, -3f, Scale: 1.5f);
                }
            }
        }

        private void SpawnShadowClones(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 5;
            shadowCloneCount = Main.rand.Next(3, 7);
            for (int i = 0; i < shadowCloneCount; i++)
            {
                float angle = MathHelper.TwoPi / shadowCloneCount * i;
                Vector2 spawnPos = player.Center + angle.ToRotationVector2() * 250;
                Vector2 toPlayer = player.Center - spawnPos;
                Vector2 vel = toPlayer.SafeNormalize(Vector2.UnitY) * 6f;
                Projectile.NewProjectile(entitySource, spawnPos, vel, ProjectileID.CultistBossIceMist, damage, 1f, Main.myPlayer);
            }
            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(NPC.Hitbox, new Color(48, 0, 96), "吞魂之术！", true);
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Shadowflame, Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-4, 4), Scale: 1.5f);
                }
            }
        }

        private void SpawnTunHunZhiShu(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 3;
            for (int wave = 0; wave < 4; wave++)
            {
                float radius = 150 + wave * 80;
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi / 20 * i + wave * 0.15f;
                    Vector2 pos = player.Center + angle.ToRotationVector2() * radius;
                    Vector2 toPlayer = player.Center - pos;
                    Vector2 vel = toPlayer.SafeNormalize(Vector2.UnitY) * 5f;
                    Projectile.NewProjectile(entitySource, pos, vel, ProjectileID.SpiritHeal, damage, 1f, Main.myPlayer);
                }
            }
            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(NPC.Hitbox, new Color(48, 0, 96), "十万年潜伏——万魂噬体！", true);
                PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 15f, 5f, 15, 800f, FullName);
                Main.instance.CameraModifiers.Add(modifier);
            }
        }
    }
}
