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
    public class KuangManMoZun : ModNPC
    {
        public bool SecondStage
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value ? 1f : 0f;
        }

        public int CurrentForm
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        public ref float TransformTimer => ref NPC.localAI[0];
        public ref float AttackTimer => ref NPC.localAI[1];
        public ref float FormAttackTimer => ref NPC.localAI[2];
        public ref float GeneralTimer => ref NPC.localAI[3];

        private const int FormWolf = 0;
        private const int FormBear = 1;
        private const int FormDragon = 2;

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
        }

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 100;
            NPC.damage = 520;
            NPC.defense = 210;
            NPC.lifeMax = 110000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 52);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement("变化道尊者，最为狂蛮。自由残缺变，化身万千兽形。")
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 22, 55));
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownBossSystem.downedKuangManMoZun, -1);
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
            int frameSpeed = 4;
            NPC.frameCounter += 0.5f;
            NPC.frameCounter += NPC.velocity.Length() / 8f;
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
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Firework_Red, Main.rand.NextFloat(-6, 6), Main.rand.NextFloat(-6, 6));
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

            TransformTimer++;
            if (SecondStage && TransformTimer > 240 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                TransformTimer = 0;
                TransformForm();
            }

            switch (CurrentForm)
            {
                case FormWolf:
                    DoWolfForm(player);
                    break;
                case FormBear:
                    DoBearForm(player);
                    break;
                case FormDragon:
                    DoDragonForm(player);
                    break;
            }

            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void CheckSecondStage()
        {
            if (SecondStage) return;
            if (NPC.life < NPC.lifeMax * 0.5f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                SecondStage = true;
                CurrentForm = FormWolf;
                NPC.netUpdate = true;
                if (Main.netMode != NetmodeID.Server)
                {
                    CombatText.NewText(NPC.Hitbox, Color.OrangeRed, "自由残缺变——化狼！", true);
                }
            }
        }

        private void TransformForm()
        {
            int newForm = Main.rand.Next(3);
            CurrentForm = newForm;
            NPC.netUpdate = true;
            string formName = newForm switch
            {
                FormWolf => "化狼！",
                FormBear => "化熊！",
                FormDragon => "化龙！",
                _ => "化狼！"
            };
            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, $"自由残缺变——{formName}", true);
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Firework_Red, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5));
                }
            }
        }

        private void DoWolfForm(Player player)
        {
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNorm = toPlayer.SafeNormalize(Vector2.UnitY);
            float speed = SecondStage ? 14f : 8f;
            NPC.velocity = (NPC.velocity * 10f + toPlayerNorm * speed) / 11f;

            NPC.damage = (int)(NPC.defDamage * (SecondStage ? 1.4f : 1f));
            NPC.defense = SecondStage ? 150 : NPC.defDefense;

            FormAttackTimer++;
            if (FormAttackTimer > 60 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                FormAttackTimer = 0;
                var entitySource = NPC.GetSource_FromAI();
                int damage = NPC.damage / 4;
                for (int i = -1; i <= 1; i++)
                {
                    float angle = toPlayer.ToRotation() + i * 0.3f;
                    Vector2 vel = angle.ToRotationVector2() * 12f;
                    Projectile.NewProjectile(entitySource, NPC.Center, vel, ProjectileID.DemonScythe, damage, 1f, Main.myPlayer);
                }
            }
        }

        private void DoBearForm(Player player)
        {
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNorm = toPlayer.SafeNormalize(Vector2.UnitY);
            float speed = SecondStage ? 6f : 4f;
            NPC.velocity = (NPC.velocity * 30f + toPlayerNorm * speed) / 31f;

            NPC.damage = (int)(NPC.defDamage * (SecondStage ? 1.6f : 1f));
            NPC.defense = SecondStage ? 300 : NPC.defDefense;

            FormAttackTimer++;
            if (FormAttackTimer > 90 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                FormAttackTimer = 0;
                var entitySource = NPC.GetSource_FromAI();
                int damage = NPC.damage / 3;
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi / 8 * i;
                    Vector2 vel = angle.ToRotationVector2() * 5f;
                    Projectile.NewProjectile(entitySource, NPC.Center, vel, ProjectileID.BoulderStaffOfEarth, damage, 3f, Main.myPlayer);
                }
                if (Main.netMode != NetmodeID.Server)
                {
                    PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 10f, 4f, 10, 500f, FullName);
                    Main.instance.CameraModifiers.Add(modifier);
                }
            }
        }

        private void DoDragonForm(Player player)
        {
            float orbitAngle = GeneralTimer * 0.03f;
            Vector2 orbitPos = player.Center + new Vector2((float)Math.Cos(orbitAngle) * 350, -200 + (float)Math.Sin(orbitAngle) * 100);
            Vector2 toOrbit = orbitPos - NPC.Center;
            Vector2 toOrbitNorm = toOrbit.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(12f, toOrbit.Length() / 12f);
            NPC.velocity = (NPC.velocity * 15f + toOrbitNorm * speed) / 16f;

            NPC.damage = (int)(NPC.defDamage * (SecondStage ? 1.5f : 1f));
            NPC.defense = SecondStage ? 200 : NPC.defDefense;

            FormAttackTimer++;
            if (FormAttackTimer > 45 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                FormAttackTimer = 0;
                var entitySource = NPC.GetSource_FromAI();
                int damage = NPC.damage / 5;
                Vector2 toPlayer = player.Center - NPC.Center;
                float angle = toPlayer.ToRotation() + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * 14f;
                Projectile.NewProjectile(entitySource, NPC.Center, vel, ProjectileID.DD2BetsyFireball, damage, 1f, Main.myPlayer);
            }
        }
    }
}
