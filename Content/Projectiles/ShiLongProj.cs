using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class ShiLongProj : BaseBullet
    {
        private const float FlySpeed = 7f;
        private const int ShockwaveRange = 200;
        private const int ShockwaveDamage = 40;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.6f, 0.45f, 0.3f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(140, 100, 50),
                GlowLayers = 3,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = true,
                LightColor = new Vector3(0.8f, 0.6f, 0.3f)
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.Stone,
                SpawnChance = 3,
                DustScale = 1.0f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                RandomSpeed = 1.5f
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 12,
                        DustType = DustID.Stone,
                        Color = Color.SaddleBrown,
                        ScaleMin = 0.8f,
                        ScaleMax = 1.5f,
                        SpeedMin = 2f,
                        SpeedMax = 6f,
                        SpreadRadius = 10f
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 100;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.4f)
                target.AddBuff(BuffID.Confused, 120);
        }

        protected override bool OnTileCollided(Vector2 oldVelocity)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist <= ShockwaveRange)
                {
                    float factor = 1f - dist / ShockwaveRange;
                    int shockDmg = (int)(ShockwaveDamage * factor);
                    npc.SimpleStrikeNPC(Projectile.damage / 2, 0);
                }
            }

            for (int i = 0; i < 15; i++)
            {
                var d = Dust.NewDustDirect(Projectile.Center - new Vector2(8, 8), 16, 16, DustID.Stone);
                d.velocity = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f));
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1f, 1.8f);
            }

            return true;
        }
    }
}
