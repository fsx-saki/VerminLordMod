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
    public class XingSuXianZun : ModNPC
    {
        public bool SecondStage
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value ? 1f : 0f;
        }

        public ref float StarFormationTimer => ref NPC.localAI[0];
        public ref float ChessPieceTimer => ref NPC.localAI[1];
        public ref float PredictiveTimer => ref NPC.localAI[2];
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
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 70;
            NPC.height = 90;
            NPC.damage = 450;
            NPC.defense = 180;
            NPC.lifeMax = 90000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 45);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement("智道尊者，与天意融合，星宿棋盘掌控命运。")
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 18, 45));
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownBossSystem.downedXingSuXianZun, -1);
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
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5));
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
                    CombatText.NewText(NPC.Hitbox, Color.LightBlue, "星宿棋盘……展开！", true);
                }
            }
        }

        private void DoFirstStage(Player player)
        {
            Vector2 hoverPos = player.Center + new Vector2(0, -250);
            Vector2 toHover = hoverPos - NPC.Center;
            Vector2 toHoverNorm = toHover.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(6f, toHover.Length() / 30f);
            NPC.velocity = (NPC.velocity * 25f + toHoverNorm * speed) / 26f;

            StarFormationTimer++;
            if (StarFormationTimer > 150 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                StarFormationTimer = 0;
                SpawnStarFormation(player);
            }

            ChessPieceTimer++;
            if (ChessPieceTimer > 200 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                ChessPieceTimer = 0;
                SpawnChessPieces(player);
            }

            NPC.damage = NPC.defDamage;
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void DoSecondStage(Player player)
        {
            float orbitAngle = GeneralTimer * 0.015f;
            Vector2 orbitPos = player.Center + new Vector2((float)Math.Cos(orbitAngle) * 300, -200 + (float)Math.Sin(orbitAngle * 2) * 60);
            Vector2 toOrbit = orbitPos - NPC.Center;
            Vector2 toOrbitNorm = toOrbit.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(10f, toOrbit.Length() / 20f);
            NPC.velocity = (NPC.velocity * 20f + toOrbitNorm * speed) / 21f;

            StarFormationTimer++;
            if (StarFormationTimer > 100 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                StarFormationTimer = 0;
                SpawnStarFormation(player);
            }

            ChessPieceTimer++;
            if (ChessPieceTimer > 140 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                ChessPieceTimer = 0;
                SpawnChessPieces(player);
            }

            PredictiveTimer++;
            if (PredictiveTimer > 240 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                PredictiveTimer = 0;
                SpawnPredictiveAttack(player);
            }

            NPC.damage = (int)(NPC.defDamage * 1.2f);
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void SpawnStarFormation(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 5;
            int[] formations = { 5, 7, 9 };
            int count = formations[Main.rand.Next(formations.Length)];
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi / count * i;
                Vector2 offset = angle.ToRotationVector2() * 150;
                Vector2 spawnPos = player.Center + offset;
                Vector2 toPlayer = player.Center - spawnPos;
                Vector2 vel = toPlayer.SafeNormalize(Vector2.UnitY) * 5f;
                Projectile.NewProjectile(entitySource, spawnPos, vel, ProjectileID.StarCloakStar, damage, 1f, Main.myPlayer);
            }
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDustDirect(NPC.Center, 20, 20, DustID.BlueCrystalShard, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2));
            }
        }

        private void SpawnChessPieces(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 4;
            Vector2 predictedPos = player.Center + player.velocity * 30;
            for (int i = 0; i < 8; i++)
            {
                Vector2 spawnPos = predictedPos + new Vector2(Main.rand.Next(-200, 200), -400 - Main.rand.Next(100));
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1, 1), 4f);
                Projectile.NewProjectile(entitySource, spawnPos, vel, ProjectileID.Meteor1, damage, 1f, Main.myPlayer);
            }
        }

        private void SpawnPredictiveAttack(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 3;
            Vector2 futurePos = player.Center + player.velocity * 60;
            Vector2 toFuture = futurePos - NPC.Center;
            float angle = toFuture.ToRotation();
            for (int i = -3; i <= 3; i++)
            {
                float spreadAngle = angle + i * 0.1f;
                Vector2 vel = spreadAngle.ToRotationVector2() * 12f;
                Projectile.NewProjectile(entitySource, NPC.Center, vel, ProjectileID.CultistBossIceMist, damage, 1f, Main.myPlayer);
            }
        }
    }
}
