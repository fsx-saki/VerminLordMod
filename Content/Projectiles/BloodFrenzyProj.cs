using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// BloodFrenzyGu弹幕 — 道道
    /// </summary>    /// <summary>
    /// BloodFrenzyGu弹幕 — 道道
    /// </summary>    /// <summary>
    /// BloodFrenzyGu弹幕 — 道道
    /// </summary>



    /// <summary>




    /// BloodFrenzyGu弹幕 — 血道




    /// </summary>




    public class BloodFrenzyProj : BaseBullet
    {
        private const float FlySpeed = 10f;
        private const float TrackWeight = 1f / 25f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
            {
                Range = 800f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            Behaviors.Add(new GlowLightBehavior(new Vector3(0.8f, 0.1f, 0.1f)));

            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 30,
                FragmentLife = 22,
                SizeMultiplier = 0.6f,
                SpawnInterval = 1,
                AdaptiveTargetLength = 80f,
                SpeedLifeExponent = 0.35f,
                MinFragmentLife = 6,
                ColorStart = new Color(200, 40, 40, 255),
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

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 60, 60),
                GlowLayers = 3,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.3f,
                GlowBaseAlpha = 0.4f,
                GlowAlphaDecay = 0.12f,
                GlowAlphaMultiplier = 0.25f
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.Confused, 180),
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
                        Count = 15,
                        DustType = DustID.Blood,
                        Color = new Color(200, 30, 30),
                        ScaleMin = 0.8f,
                        ScaleMax = 1.5f,
                        SpeedMin = 1f,
                        SpeedMax = 4f,
                        SpreadRadius = 10f
                    }
                }
            });

            Behaviors.Add(new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = 18,
                KillSpeed = 4f,
                KillSizeMultiplier = 1.0f,
                KillFragmentLife = 25,
                ExplodeOnTileCollide = true,
                TileCollideCount = 12,
                TileCollideSpeed = 3f,
                TileCollideSizeMultiplier = 0.8f,
                TileCollideFragmentLife = 20,
                ColorStart = new Color(220, 40, 40, 255),
                ColorEnd = new Color(160, 0, 0, 0)
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