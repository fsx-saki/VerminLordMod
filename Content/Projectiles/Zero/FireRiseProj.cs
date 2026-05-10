using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 炎道升起弹幕 — 从角色中心缓慢飞到头上方，然后飞向鼠标指针。
    ///
    /// 行为组合：
    /// - GravityBehavior: 重力（升起阶段禁用，冲刺阶段启用）
    /// - BounceBehavior: 碰撞反弹（最多2次，系数0.4f）
    /// - LiquidTrailBehavior: 液态火焰拖尾（黄→红渐变，自适应密度防断裂）
    /// - ExplosionKillBehavior: 销毁时爆炸
    ///
    /// 两阶段逻辑通过 OnAI() 扩展点实现：
    ///   阶段0（升起）：沿初始扇形方向缓慢上升，持续 RiseTime 帧
    ///   阶段1（冲刺）：转向鼠标方向加速飞去
    /// </summary>
    public class FireRiseProj : BaseBullet
    {
        /// <summary>升起阶段持续时间（帧）</summary>
        private const int RiseTime = 40;

        /// <summary>冲刺速度</summary>
        private const float DashSpeed = 14f;

        /// <summary>升起速度</summary>
        private const float RiseSpeed = 3f;

        /// <summary>当前阶段：0=升起，1=冲刺</summary>
        private int _phase = 0;

        /// <summary>升起计时器</summary>
        private int _riseTimer = 0;

        /// <summary>初始速度方向（用于保持扇形散开）</summary>
        private Vector2 _initialDirection;

        protected override void RegisterBehaviors()
        {
            // 1. 重力（冲刺阶段才生效）
            Behaviors.Add(new GravityBehavior(acceleration: 0.12f, maxFallSpeed: 12f)
            {
                AutoRotate = false, // 由阶段逻辑控制旋转
            });

            // 2. 碰撞反弹（最多2次，系数0.4f，速度过低时停止）
            Behaviors.Add(new BounceBehavior(maxBounces: 2, bounceFactor: 0.4f)
            {
                KillOnMaxBounces = true,
                TriggerKillOnMaxBounces = true,
                StopOnLowSpeed = true,
                LowSpeedThreshold = 1f,
                TimeLeftAfterStop = 30
            });

            // 3. 液态火焰拖尾（黄→红渐变，自适应密度防断裂）
            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 50,
                FragmentLife = 25,
                SizeMultiplier = 0.5f,
                SpawnInterval = 2,
                AdaptiveDensity = true,
                AdaptiveSpeedThreshold = 4f,
                AdaptiveDensityFactor = 4f, // 14/4≈3.5→每帧3个碎片，确保高速不断裂
                ColorStart = new Color(255, 220, 100, 255),
                ColorEnd = new Color(255, 30, 0, 0),
                Buoyancy = 0.02f,
                AirResistance = 0.98f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.1f,
                SplashAngle = 0.3f,
                RandomSpread = 0.5f,
                AutoDraw = true,
                SuppressDefaultDraw = true
            });

            // 4. 销毁时爆炸
            Behaviors.Add(new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = 8,
                KillSpeed = 3f,
                KillSizeMultiplier = 0.6f,
                KillFragmentLife = 20,
                ExplodeOnTileCollide = false,
                ColorStart = new Color(255, 220, 100, 255),
                ColorEnd = new Color(255, 30, 0, 0)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 升起阶段禁用碰撞，冲刺阶段启用
            Projectile.penetrate = 99;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            base.OnSpawned(source);

            // 保存初始速度方向（由武器 Shoot 设置的扇形方向），
            // 确保弹幕沿各自方向散开上升
            if (Projectile.velocity != Vector2.Zero)
            {
                _initialDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                // 强制向上
                if (_initialDirection.Y > 0)
                    _initialDirection.Y = -_initialDirection.Y;
                _initialDirection.Normalize();

                Projectile.velocity = _initialDirection * RiseSpeed;
            }
            else
            {
                _initialDirection = -Vector2.UnitY;
                Projectile.velocity = _initialDirection * RiseSpeed;
            }

            _phase = 0;
            _riseTimer = 0;
        }

        protected override void OnAI()
        {
            switch (_phase)
            {
                case 0: // 升起阶段
                    UpdateRise();
                    break;

                case 1: // 冲刺阶段
                    UpdateDash();
                    break;
            }
        }

        private void UpdateRise()
        {
            _riseTimer++;

            // 沿初始方向缓慢上升，不衰减水平速度，保持扇形散开
            // 随着时间推移，逐渐加入轻微摆动
            float sway = (float)Math.Sin(_riseTimer * 0.08f) * 0.3f;
            Vector2 dir = _initialDirection;
            dir.X += sway * 0.02f; // 极轻微的摆动，不影响扇形
            dir.Normalize();

            Projectile.velocity = dir * RiseSpeed;

            // 旋转跟随速度方向
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 升起时间到，进入冲刺阶段
            if (_riseTimer >= RiseTime)
            {
                _phase = 1;

                // 基于弹幕自身位置计算到鼠标的方向（不是基于玩家中心）
                Vector2 toMouse = Main.MouseWorld - Projectile.Center;
                if (toMouse != Vector2.Zero)
                {
                    toMouse.Normalize();
                    Projectile.velocity = toMouse * DashSpeed;
                }
                else
                {
                    // 保底：向右飞
                    Projectile.velocity = Vector2.UnitX * DashSpeed;
                }

                // 冲刺阶段启用物块碰撞
                Projectile.tileCollide = true;
            }
        }

        private void UpdateDash()
        {
            // 保持速度恒定
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * DashSpeed;
            }

            // 旋转跟随速度方向
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
    }
}
