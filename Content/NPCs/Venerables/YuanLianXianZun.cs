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
    public class YuanLianXianZun : ModNPC
    {
        public bool SecondStage
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value ? 1f : 0f;
        }

        public ref float LotusTimer => ref NPC.localAI[0];
        public ref float VineAttackTimer => ref NPC.localAI[1];
        public ref float RegenerationTimer => ref NPC.localAI[2];
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
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 80;
            NPC.height = 100;
            NPC.damage = 400;
            NPC.defense = 250;
            NPC.lifeMax = 120000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 55);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement("木道尊者，最善治愈，创天元宝莲。生生不息，莲华万朵。")
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 25, 60));
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownBossSystem.downedYuanLianXianZun, -1);
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
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkFairy, Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-4, 4));
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

            RegenerationTimer++;
            if (RegenerationTimer > 60)
            {
                RegenerationTimer = 0;
                int regenAmount = SecondStage ? NPC.lifeMax / 50 : NPC.lifeMax / 100;
                NPC.life = Math.Min(NPC.life + regenAmount, NPC.lifeMax);
                if (Main.netMode != NetmodeID.Server)
                {
                    NPC.HealEffect(regenAmount);
                }
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
                    CombatText.NewText(NPC.Hitbox, Color.Pink, "天元宝莲……绽放！", true);
                }
            }
        }

        private void DoFirstStage(Player player)
        {
            Vector2 hoverPos = player.Center + new Vector2(0, -220);
            Vector2 toHover = hoverPos - NPC.Center;
            Vector2 toHoverNorm = toHover.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(5f, toHover.Length() / 30f);
            NPC.velocity = (NPC.velocity * 30f + toHoverNorm * speed) / 31f;

            LotusTimer++;
            if (LotusTimer > 150 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                LotusTimer = 0;
                SpawnHealingLotus();
            }

            VineAttackTimer++;
            if (VineAttackTimer > 120 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                VineAttackTimer = 0;
                SpawnVineAttack(player);
            }

            NPC.damage = NPC.defDamage;
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void DoSecondStage(Player player)
        {
            float orbitAngle = GeneralTimer * 0.012f;
            Vector2 orbitPos = player.Center + new Vector2((float)Math.Cos(orbitAngle) * 200, -250 + (float)Math.Sin(orbitAngle) * 40);
            Vector2 toOrbit = orbitPos - NPC.Center;
            Vector2 toOrbitNorm = toOrbit.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(8f, toOrbit.Length() / 20f);
            NPC.velocity = (NPC.velocity * 22f + toOrbitNorm * speed) / 23f;

            LotusTimer++;
            if (LotusTimer > 100 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                LotusTimer = 0;
                SpawnHealingLotus();
                SpawnAttackingLotus(player);
            }

            VineAttackTimer++;
            if (VineAttackTimer > 80 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                VineAttackTimer = 0;
                SpawnVineAttack(player);
            }

            if (GeneralTimer % 480 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                SpawnTianYuanBaoLian(player);
            }

            NPC.damage = (int)(NPC.defDamage * 1.15f);
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void SpawnHealingLotus()
        {
            var entitySource = NPC.GetSource_FromAI();
            Vector2 lotusPos = NPC.Center + new Vector2(Main.rand.Next(-150, 150), Main.rand.Next(-100, 100));
            Projectile.NewProjectile(entitySource, lotusPos, Vector2.Zero, ProjectileID.SpiritHeal, 0, 0f, Main.myPlayer);
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi / 8 * i;
                    Dust.NewDustDirect(lotusPos + angle.ToRotationVector2() * 30, 10, 10, DustID.PinkFairy, 0, -1f, Scale: 1.5f);
                }
            }
        }

        private void SpawnAttackingLotus(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 5;
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi / 5 * i + GeneralTimer * 0.01f;
                Vector2 spawnPos = NPC.Center + angle.ToRotationVector2() * 100;
                Vector2 toPlayer = player.Center - spawnPos;
                Vector2 vel = toPlayer.SafeNormalize(Vector2.UnitY) * 6f;
                Projectile.NewProjectile(entitySource, spawnPos, vel, ProjectileID.FlowerPow, damage, 1f, Main.myPlayer);
            }
        }

        private void SpawnVineAttack(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 4;
            Vector2 toPlayer = player.Center - NPC.Center;
            float baseAngle = toPlayer.ToRotation();
            for (int i = -3; i <= 3; i++)
            {
                float angle = baseAngle + i * 0.15f;
                Vector2 vel = angle.ToRotationVector2() * 7f;
                Projectile.NewProjectile(entitySource, NPC.Center, vel, ProjectileID.PoisonFang, damage, 1f, Main.myPlayer);
            }
        }

        private void SpawnTianYuanBaoLian(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 3;
            int healAmount = NPC.lifeMax / 8;
            NPC.life = Math.Min(NPC.life + healAmount, NPC.lifeMax);
            NPC.HealEffect(healAmount);
            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(NPC.Hitbox, Color.Pink, "天元宝莲——万物生发！", true);
            }
            for (int wave = 0; wave < 3; wave++)
            {
                float radius = 120 + wave * 100;
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi / 12 * i + wave * 0.3f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    Vector2 vel = angle.ToRotationVector2() * 3f;
                    Projectile.NewProjectile(entitySource, pos, vel, ProjectileID.FlowerPow, damage, 1f, Main.myPlayer);
                }
            }
            for (int i = 0; i < 40; i++)
            {
                Dust.NewDustDirect(NPC.Center, 40, 40, DustID.PinkFairy, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5), Scale: 2f);
            }
        }
    }
}
