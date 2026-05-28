using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Items.Materials;

namespace VerminLordMod.Content.NPCs.Venerables
{
    [AutoloadBossHead]
    public class DaoTianMoZun : ModNPC
    {
        public bool SecondStage
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value ? 1f : 0f;
        }

        public ref float StealTimer => ref NPC.localAI[0];
        public ref float TeleportTimer => ref NPC.localAI[1];
        public ref float BackstabTimer => ref NPC.localAI[2];
        public ref float GeneralTimer => ref NPC.localAI[3];

        private int stolenBuffCount = 0;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 70;
            NPC.height = 90;
            NPC.damage = 460;
            NPC.defense = 160;
            NPC.lifeMax = 88000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 46);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new MoonLordPortraitBackgroundProviderBestiaryInfoElement(),
                new FlavorTextBestiaryInfoElement("偷道尊者，来自异界，盗取一切。盗亦有道，万事留一线。")
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 17, 44));
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownBossSystem.downedDaoTianMoZun, -1);
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
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Shadowflame, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5));
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
                    CombatText.NewText(NPC.Hitbox, Color.DarkViolet, "盗亦有道……你的力量归我了！", true);
                }
            }
        }

        private void DoFirstStage(Player player)
        {
            Vector2 hoverPos = player.Center + new Vector2(Math.Sign(NPC.Center.X - player.Center.X) * 300, -200);
            Vector2 toHover = hoverPos - NPC.Center;
            Vector2 toHoverNorm = toHover.SafeNormalize(Vector2.UnitY);
            float speed = Math.Min(8f, toHover.Length() / 20f);
            NPC.velocity = (NPC.velocity * 20f + toHoverNorm * speed) / 21f;

            StealTimer++;
            if (StealTimer > 180 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                StealTimer = 0;
                StealPlayerBuffs(player);
            }

            TeleportTimer++;
            if (TeleportTimer > 240 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                TeleportTimer = 0;
                TeleportBehindPlayer(player);
            }

            BackstabTimer++;
            if (BackstabTimer > 100 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                BackstabTimer = 0;
                SpawnStolenProjectile(player);
            }

            NPC.damage = NPC.defDamage;
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void DoSecondStage(Player player)
        {
            float moveSpeed = 10f;
            if (GeneralTimer % 120 < 60)
            {
                Vector2 behindPlayer = player.Center + new Vector2(-player.direction * 200, -150);
                Vector2 toBehind = behindPlayer - NPC.Center;
                Vector2 toBehindNorm = toBehind.SafeNormalize(Vector2.UnitY);
                NPC.velocity = (NPC.velocity * 15f + toBehindNorm * moveSpeed) / 16f;
            }
            else
            {
                Vector2 toPlayer = player.Center - NPC.Center;
                Vector2 toPlayerNorm = toPlayer.SafeNormalize(Vector2.UnitY);
                NPC.velocity = (NPC.velocity * 10f + toPlayerNorm * moveSpeed) / 11f;
            }

            StealTimer++;
            if (StealTimer > 120 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                StealTimer = 0;
                StealPlayerBuffs(player);
            }

            TeleportTimer++;
            if (TeleportTimer > 150 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                TeleportTimer = 0;
                TeleportBehindPlayer(player);
            }

            BackstabTimer++;
            if (BackstabTimer > 60 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                BackstabTimer = 0;
                SpawnStolenProjectile(player);
            }

            if (GeneralTimer % 300 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                StealLife(player);
            }

            NPC.damage = (int)(NPC.defDamage * 1.3f);
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void StealPlayerBuffs(Player player)
        {
            stolenBuffCount = 0;
            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                int buffType = player.buffType[i];
                if (buffType > 0 && !Main.debuff[buffType] && buffType != BuffID.ManaRegeneration)
                {
                    player.DelBuff(i);
                    stolenBuffCount++;
                    i--;
                    if (stolenBuffCount >= 3) break;
                }
            }
            if (stolenBuffCount > 0)
            {
                int healAmount = stolenBuffCount * 500;
                NPC.life = Math.Min(NPC.life + healAmount, NPC.lifeMax);
                NPC.HealEffect(healAmount);
                if (Main.netMode != NetmodeID.Server)
                {
                    CombatText.NewText(player.Hitbox, Color.DarkViolet, $"盗取了{stolenBuffCount}个增益！", false);
                    for (int i = 0; i < 15; i++)
                    {
                        Dust.NewDustDirect(player.position, player.width, player.height, DustID.Shadowflame, 0, -3f, Scale: 1.5f);
                    }
                }
            }
        }

        private void TeleportBehindPlayer(Player player)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Shadowflame, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
                }
            }
            Vector2 behindPos = player.Center + new Vector2(-player.direction * 200, -100);
            NPC.Center = behindPos;
            NPC.velocity = Vector2.Zero;
            NPC.netUpdate = true;
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Shadowflame, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
                }
            }
        }

        private void SpawnStolenProjectile(Player player)
        {
            var entitySource = NPC.GetSource_FromAI();
            int damage = NPC.damage / 4;
            Vector2 toPlayer = player.Center - NPC.Center;
            float angle = toPlayer.ToRotation() + Main.rand.NextFloat(-0.2f, 0.2f);
            Vector2 vel = angle.ToRotationVector2() * 10f;
            int[] projTypes = { ProjectileID.DemonScythe, ProjectileID.CultistBossIceMist, ProjectileID.DD2BetsyFireball, ProjectileID.FlowerPow };
            int projType = projTypes[Main.rand.Next(projTypes.Length)];
            Projectile.NewProjectile(entitySource, NPC.Center, vel, projType, damage, 1f, Main.myPlayer);
        }

        private void StealLife(Player player)
        {
            int stealAmount = player.statLifeMax2 / 10;
            player.statLife = Math.Max(player.statLife - stealAmount, 1);
            player.HealEffect(-stealAmount);
            NPC.life = Math.Min(NPC.life + stealAmount, NPC.lifeMax);
            NPC.HealEffect(stealAmount);
            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(NPC.Hitbox, Color.DarkViolet, "盗亦有道——窃命！", true);
            }
        }
    }
}
