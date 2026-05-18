using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// 火龙蛊弹幕 — 火道
    /// </summary>

    public class HuoLongProj : BaseBullet
    {
        private const float FlySpeed = 8f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1f, 0.4f, 0.05f)
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 25,
                FragmentLife = 20,
                SizeMultiplier = 0.7f,
                SpawnInterval = 1,
                AdaptiveTargetLength = 70f,
                SpeedLifeExponent = 0.3f,
                MinFragmentLife = 5,
                ColorStart = new Color(255, 100, 20, 255),
                ColorEnd = new Color(180, 30, 0, 0),
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
                    (BuffID.OnFire, 300),
                    (BuffID.CursedInferno, 120)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.Torch,
                DustCount = 15,
                SpeedMin = 1f,
                SpeedMax = 5f,
                ScaleMin = 1.2f,
                ScaleMax = 2.5f,
                Color = Color.OrangeRed
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 20,
                        DustType = DustID.Torch,
                        Color = new Color(255, 80, 10),
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
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.3f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
