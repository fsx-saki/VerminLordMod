using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    /// <summary>
    /// TunJiangChan弹幕 — 水道
    /// </summary>
    public class TunJiangChanProj : BaseBullet
    {
        private const float FlySpeed = 6f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.1f, 0.3f, 0.7f)
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 35,
                FragmentLife = 25,
                SizeMultiplier = 0.8f,
                SpawnInterval = 1,
                AdaptiveTargetLength = 90f,
                SpeedLifeExponent = 0.35f,
                MinFragmentLife = 6,
                ColorStart = new Color(30, 80, 200, 255),
                ColorEnd = new Color(10, 40, 120, 0),
                Buoyancy = 0.02f,
                AirResistance = 0.93f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.15f,
                SplashAngle = 0.4f,
                RandomSpread = 0.5f,
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.Wet, 360),
                    (BuffID.Slow, 180)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.Water,
                DustCount = 15,
                SpeedMin = 2f,
                SpeedMax = 5f,
                ScaleMin = 1.2f,
                ScaleMax = 2.5f,
                Color = Color.Blue
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 20,
                        DustType = DustID.Water,
                        Color = new Color(30, 80, 200),
                        ScaleMin = 1f,
                        ScaleMax = 2f,
                        SpeedMin = 2f,
                        SpeedMax = 6f,
                        SpreadRadius = 15f
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1.4f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 240;
            Projectile.alpha = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
