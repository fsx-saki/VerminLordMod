using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// 血狂蛊弹幕 — 血道
    /// </summary>

    public class XueKuangProj : BaseBullet
    {
        private const float FlySpeed = 10f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: 1f / 25f)
            {
                Range = 800f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(200, 30, 30),
                GlowLayers = 3,
                GlowBaseScale = 1.4f,
                GlowScaleIncrement = 0.3f,
                GlowBaseAlpha = 0.4f,
                GlowAlphaDecay = 0.12f,
                GlowAlphaMultiplier = 0.25f
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 25,
                FragmentLife = 20,
                SizeMultiplier = 0.6f,
                SpawnInterval = 1,
                AdaptiveTargetLength = 70f,
                SpeedLifeExponent = 0.3f,
                MinFragmentLife = 5,
                ColorStart = new Color(180, 20, 20, 255),
                ColorEnd = new Color(100, 0, 0, 0),
                Buoyancy = 0.01f,
                AirResistance = 0.95f,
                InertiaFactor = 0.25f,
                SplashFactor = 0.1f,
                SplashAngle = 0.3f,
                RandomSpread = 0.5f,
                AutoDraw = true,
                SuppressDefaultDraw = true
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.CursedInferno, 150),
                    (BuffID.Bleeding, 240)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.Blood,
                DustCount = 12,
                SpeedMin = 1f,
                SpeedMax = 4f,
                ScaleMin = 1f,
                ScaleMax = 2f,
                Color = Color.DarkRed
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 18,
                        DustType = DustID.Blood,
                        Color = new Color(200, 30, 30),
                        ScaleMin = 0.8f,
                        ScaleMax = 1.5f,
                        SpeedMin = 2f,
                        SpeedMax = 6f,
                        SpreadRadius = 12f
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
