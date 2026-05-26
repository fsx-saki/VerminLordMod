using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class MiHunProj : BaseBullet
    {
        private const float FlySpeed = 6f;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1.3f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 120;
            Projectile.alpha = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.5f, 0.1f, 0.8f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(140, 40, 200),
                GlowLayers = 3,
                GlowBaseScale = 1.5f,
                GlowScaleIncrement = 0.6f,
                GlowBaseAlpha = 0.7f,
                GlowAlphaDecay = 0.2f,
                GlowAlphaMultiplier = 0.4f,
                EnableLight = true,
                LightColor = new Vector3(0.5f, 0.1f, 0.8f)
            });

            Behaviors.Add(new SoulTrailBehavior
            {
                GhostColor = new Color(160, 60, 220, 160),
                FlameColor = new Color(180, 80, 255, 230),
                ChainColor = new Color(130, 50, 200, 200),
                WispColor = new Color(200, 140, 255, 240),
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.PurpleTorch,
                DustCount = 10,
                SpeedMin = 1f,
                SpeedMax = 4f,
                ScaleMin = 0.8f,
                ScaleMax = 1.5f,
                Color = new Color(160, 60, 220)
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 15,
                        DustType = DustID.PurpleTorch,
                        Color = new Color(140, 40, 200),
                        ScaleMin = 0.6f,
                        ScaleMax = 1.4f,
                        SpeedMin = 2f,
                        SpeedMax = 5f,
                        SpreadRadius = 12f
                    }
                }
            });
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.5f)
            {
                target.AddBuff(BuffID.Confused, 300);
            }

            if (Main.rand.NextFloat() < 0.3f)
            {
                target.AddBuff(BuffID.OnFire, 180);
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}
