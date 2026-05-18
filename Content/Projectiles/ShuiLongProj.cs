using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// 水龙蛊弹幕 — 水道
    /// </summary>
    public class ShuiLongProj : BaseBullet
    {
        private const float FlySpeed = 8f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.1f, 0.3f, 0.8f)
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 20,
                FragmentLife = 18,
                SizeMultiplier = 0.5f,
                SpawnInterval = 2,
                AdaptiveTargetLength = 60f,
                SpeedLifeExponent = 0.3f,
                MinFragmentLife = 5,
                ColorStart = new Color(30, 100, 220, 255),
                ColorEnd = new Color(10, 50, 150, 0),
                Buoyancy = 0.01f,
                AirResistance = 0.95f,
                InertiaFactor = 0.25f,
                SplashFactor = 0.1f,
                SplashAngle = 0.3f,
                RandomSpread = 0.4f,
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.Wet, 300)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.Water,
                DustCount = 12,
                SpeedMin = 1f,
                SpeedMax = 4f,
                ScaleMin = 1f,
                ScaleMax = 2f,
                Color = Color.Blue
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 15,
                        DustType = DustID.Water,
                        Color = new Color(30, 100, 220),
                        ScaleMin = 0.8f,
                        ScaleMax = 1.5f,
                        SpeedMin = 2f,
                        SpeedMax = 5f,
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
