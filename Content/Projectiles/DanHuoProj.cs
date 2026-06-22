using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// 丹火蛊弹幕 — 火道
    /// 大型火球，带重力弹跳，碰撞后小范围爆燃
    /// </summary>
    public class DanHuoProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 1. 重力（星火弹风格）
            Behaviors.Add(new GravityBehavior(acceleration: 0.12f, maxFallSpeed: 12f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 碰撞反弹（最多2次）
            Behaviors.Add(new BounceBehavior(maxBounces: 2, bounceFactor: 0.4f)
            {
                KillOnMaxBounces = true,
                TriggerKillOnMaxBounces = true,
                StopOnLowSpeed = true,
                LowSpeedThreshold = 1f,
                TimeLeftAfterStop = 30
            });

            // 3. 液态火焰拖尾（黄→红渐变）
            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 50,
                FragmentLife = 15,
                SizeMultiplier = 0.8f,
                SpawnInterval = 1,
                ColorStart = new Color(255, 220, 100, 255),
                ColorEnd = new Color(255, 30, 0, 0),
                Buoyancy = 0.05f,
                AirResistance = 0.96f,
                InertiaFactor = 0.4f,
                SplashFactor = 0.2f,
                SplashAngle = 0.5f,
                RandomSpread = 0.8f,
                AutoDraw = true,
                SuppressDefaultDraw = true
            });

            // 4. 碰撞耗尽时小范围爆炸
            Behaviors.Add(new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = 10,
                KillSpeed = 3f,
                KillSizeMultiplier = 0.8f,
                KillFragmentLife = 20,
                ExplodeOnTileCollide = false,
                ColorStart = new Color(255, 220, 100, 255),
                ColorEnd = new Color(255, 30, 0, 0)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 99;
            Projectile.timeLeft = 120;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }
        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}
