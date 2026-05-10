using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 炎道天降弹幕 — 从天而降的一组大小不一的火焰弹。
    ///
    /// 由武器在鼠标上方随机位置生成，带重力自由落体下落。
    /// 每个弹幕大小不同（scale 0.6~1.2），伤害和速度也随大小变化。
    /// 落地后反弹并爆炸，模拟"陨石雨"效果。
    ///
    /// 行为组合：
    /// - GravityBehavior: 重力（较强，模拟自由落体）
    /// - BounceBehavior: 碰撞反弹（最多1次，落地弹跳）
    /// - LiquidTrailBehavior: 液态火焰拖尾（下落拖尾）
    /// - ExplosionKillBehavior: 销毁时爆炸
    /// </summary>
    public class FireRainProj : BaseBullet
    {
        /// <summary>弹幕大小倍率（0.6~1.2，由武器在生成时随机设置）</summary>
        private float _sizeScale = 1f;

        /// <summary>爆炸行为引用，用于在 SetSizeScale 中更新爆炸参数</summary>
        private ExplosionKillBehavior _explosionBehavior;

        protected override void RegisterBehaviors()
        {
            // 1. 重力（较强，模拟自由落体）
            Behaviors.Add(new GravityBehavior(acceleration: 0.25f, maxFallSpeed: 16f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 碰撞反弹（最多1次，系数0.3f，落地轻轻弹一下）
            Behaviors.Add(new BounceBehavior(maxBounces: 1, bounceFactor: 0.3f)
            {
                KillOnMaxBounces = true,
                TriggerKillOnMaxBounces = true,
                StopOnLowSpeed = true,
                LowSpeedThreshold = 2f,
                TimeLeftAfterStop = 20
            });

            // 3. 液态火焰拖尾（下落拖尾，大小随弹幕尺寸变化）
            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 20,
                FragmentLife = 15,
                SizeMultiplier = 0.5f,
                SpawnInterval = 2,
                AdaptiveTargetLength = 80f,
                SpeedLifeExponent = 0.4f,
                MinFragmentLife = 5,
                ColorStart = new Color(255, 200, 80, 255),
                ColorEnd = new Color(255, 50, 0, 0),
                Buoyancy = 0.01f,
                AirResistance = 0.98f,
                InertiaFactor = 0.2f,
                SplashFactor = 0.1f,
                SplashAngle = 0.3f,
                RandomSpread = 0.5f,
                AutoDraw = true,
                SuppressDefaultDraw = true
            });

            // 4. 销毁时爆炸（大小随弹幕尺寸变化）
            // 先用默认 _sizeScale=1f 注册，SetSizeScale 中会更新参数
            _explosionBehavior = new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = (int)(10 * _sizeScale),
                KillSpeed = 3f * _sizeScale,
                KillSizeMultiplier = 0.6f * _sizeScale,
                KillFragmentLife = 20,
                ExplodeOnTileCollide = false,
                ColorStart = new Color(255, 200, 80, 255),
                ColorEnd = new Color(255, 50, 0, 0)
            };
            Behaviors.Add(_explosionBehavior);
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 99;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        /// <summary>
        /// 设置弹幕的大小和对应属性。
        /// 由武器在生成时调用，传入随机大小倍率。
        /// </summary>
        public void SetSizeScale(float scale)
        {
            _sizeScale = MathHelper.Clamp(scale, 0.5f, 1.5f);
            Projectile.scale = 0.7f + _sizeScale * 0.4f; // 0.7~1.3
            Projectile.width = (int)(12 * Projectile.scale);
            Projectile.height = (int)(12 * Projectile.scale);
            Projectile.damage = (int)(Projectile.damage * (0.7f + _sizeScale * 0.4f)); // 大小影响伤害

            // 同步更新爆炸行为参数（RegisterBehaviors 时 _sizeScale 还是默认值 1f）
            if (_explosionBehavior != null)
            {
                _explosionBehavior.KillCount = (int)(10 * _sizeScale);
                _explosionBehavior.KillSpeed = 3f * _sizeScale;
                _explosionBehavior.KillSizeMultiplier = 0.6f * _sizeScale;
            }
        }
    }
}
