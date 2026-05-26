using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class QiuGuProj : BaseBullet
    {
        private const float FlySpeed = 8f;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 100;
            Projectile.alpha = 20;
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
                LightColor = new Vector3(0.9f, 0.5f, 0.1f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(220, 140, 30),
                GlowLayers = 3,
                GlowBaseScale = 1.4f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.6f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(0.9f, 0.5f, 0.1f)
            });

            Behaviors.Add(new TimeTrailBehavior
            {
                GhostColor = new Color(220, 150, 40, 140),
                GrainColor = new Color(240, 170, 50, 220),
                ClockHandColor = new Color(200, 140, 40, 200),
                AfterimageColor = new Color(200, 130, 40, 180),
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.CursedInferno, 180),
                    (BuffID.OnFire, 300)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.Torch,
                DustCount = 12,
                SpeedMin = 1f,
                SpeedMax = 4f,
                ScaleMin = 0.8f,
                ScaleMax = 1.5f,
                Color = new Color(220, 140, 30)
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 15,
                        DustType = DustID.Torch,
                        Color = new Color(220, 140, 30),
                        ScaleMin = 0.6f,
                        ScaleMax = 1.3f,
                        SpeedMin = 2f,
                        SpeedMax = 6f,
                        SpreadRadius = 12f
                    }
                }
            });
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
