using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 炎道爆弹 — 大型弹幕，直线飞行。
    /// 碰到物块或销毁时爆出一圈 FireBaseProj（星火弹）。
    /// 
    /// 行为组合：
    /// - AimBehavior: 直线飞行
    /// - GravityBehavior: 重力
    /// - LiquidTrailBehavior: 液态火焰拖尾（大号，浓烈）
    /// - ExplosionKillBehavior: 销毁时爆炸（视觉）
    /// - OnKill 生成一圈 FireBaseProj（通过 OnKilled 扩展点）
    /// </summary>
    public class FireBombProj : BaseBullet
    {
        /// <summary>飞行速度（像素/帧）</summary>
        private const float FlySpeed = 10f;

        /// <summary>爆炸时生成的星火弹数量</summary>
        private const int SpawnCount = 8;

        /// <summary>星火弹飞散速度</summary>
        private const float SpawnSpeed = 6f;

        protected override void RegisterBehaviors()
        {
            // 1. 直线飞行
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1f, 0.5f, 0.05f)
            });

            // 2. 重力（轻微，让弹幕有抛物线感）
            Behaviors.Add(new GravityBehavior(acceleration: 0.08f, maxFallSpeed: 8f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 3. 液态火焰拖尾（大号，浓烈）
            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 40,
                FragmentLife = 25,
                SizeMultiplier = 0.8f,
                SpawnInterval = 1,
                AdaptiveTargetLength = 120f,
                SpeedLifeExponent = 0.35f,
                MinFragmentLife = 8,
                ColorStart = new Color(255, 180, 60, 255),
                ColorEnd = new Color(200, 30, 0, 0),
                Buoyancy = 0.02f,
                AirResistance = 0.96f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.15f,
                SplashAngle = 0.4f,
                RandomSpread = 0.6f,
                AutoDraw = true,
                SuppressDefaultDraw = true
            });

            // 4. 销毁时爆炸（视觉碎片效果）
            Behaviors.Add(new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = 20,
                KillSpeed = 5f,
                KillSizeMultiplier = 1.2f,
                KillFragmentLife = 30,
                ExplodeOnTileCollide = true,
                TileCollideCount = 15,
                TileCollideSpeed = 4f,
                TileCollideSizeMultiplier = 1.0f,
                TileCollideFragmentLife = 25,
                DestroyOnTileCollideExplosion = true,
                ColorStart = new Color(255, 200, 80, 255),
                ColorEnd = new Color(255, 30, 0, 0)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 99; // 不因命中敌人而销毁
            Projectile.timeLeft = 120;  // 存活 120 帧
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnKilled(int timeLeft)
        {
            // 销毁时爆出一圈 FireBaseProj（星火弹）
            int fireBaseType = ModContent.ProjectileType<FireBaseProj>();

            for (int i = 0; i < SpawnCount; i++)
            {
                float angle = MathHelper.TwoPi * i / SpawnCount;
                Vector2 vel = angle.ToRotationVector2() * SpawnSpeed;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    vel,
                    fireBaseType,
                    (int)(Projectile.damage * 0.6f), // 子弹伤害为主弹的 60%
                    Projectile.knockBack * 0.5f,
                    Projectile.owner
                );
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity)
        {
            // 碰到物块时也爆出一圈 FireBaseProj
            // ExplosionKillBehavior 的 DestroyOnTileCollideExplosion=true 会销毁弹幕，
            // 然后 OnKilled 会被调用，所以这里不需要重复生成
            return false;
        }
    }
}
