using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// XueHeMang弹幕 — 道道
    /// </summary>    /// <summary>
    /// 血河蟒弹幕 — 道道
    /// </summary>


    public class XueHeMangProj : BaseBullet
    {
        private const float FlySpeed = 9f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: 1f / 20f)
            {
                Range = 700f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(180, 20, 40),
                GlowLayers = 3,
                GlowBaseScale = 1.4f,
                GlowScaleIncrement = 0.3f,
                GlowBaseAlpha = 0.4f,
                GlowAlphaDecay = 0.12f,
                GlowAlphaMultiplier = 0.25f
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 28,
                FragmentLife = 22,
                SizeMultiplier = 0.65f,
                SpawnInterval = 1,
                AdaptiveTargetLength = 75f,
                SpeedLifeExponent = 0.35f,
                MinFragmentLife = 5,
                ColorStart = new Color(160, 10, 30, 255),
                ColorEnd = new Color(80, 0, 10, 0),
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
                    (BuffID.Bleeding, 300),
                    (BuffID.CursedInferno, 120)
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
                        Color = new Color(180, 20, 30),
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
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 2;
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
