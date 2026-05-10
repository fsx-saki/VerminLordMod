using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 炎道基础弹幕 — 星火弹
    /// 完全通过行为组件组合实现，零硬编码逻辑。
    /// 
    /// 行为组合：
    /// - GravityBehavior: 重力（0.12f/帧）
    /// - BounceBehavior: 碰撞反弹（最多2次，系数0.4f）
    /// - LiquidTrailBehavior: 液态火焰拖尾（黄→红渐变）
    /// - ExplosionKillBehavior: 碰撞耗尽时爆炸 + 销毁时爆炸
    /// - GlowDrawBehavior: 发光绘制
    /// </summary>
    public class FireBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 1. 重力（星火弹 0.12f/帧）
            Behaviors.Add(new GravityBehavior(acceleration: 0.12f, maxFallSpeed: 12f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 碰撞反弹（最多2次，系数0.4f，速度过低时停止）
            // TriggerKillOnMaxBounces = true 时，反弹耗尽返回 true（销毁），
            // 引擎调用 OnKill，由 ExplosionKillBehavior 处理爆炸。
            Behaviors.Add(new BounceBehavior(maxBounces: 2, bounceFactor: 0.4f)
            {
                KillOnMaxBounces = true,
                TriggerKillOnMaxBounces = true,
                StopOnLowSpeed = true,
                LowSpeedThreshold = 1f,
                TimeLeftAfterStop = 30
            });

            // 3. 爆炸效果（OnKill 时触发，覆盖 timeout 和 bounce 耗尽两种情况）
            Behaviors.Add(new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = 15,
                KillSpeed = 4f,
                KillSizeMultiplier = 1f,
                KillFragmentLife = 25,
                ExplodeOnTileCollide = false, // 由 BounceBehavior.TriggerKillOnMaxBounces 触发 OnKill
                ColorStart = new Color(255, 220, 100, 255),
                ColorEnd = new Color(255, 30, 0, 0)
            });

            // 4. 液态火焰拖尾（黄→红渐变，星火风格）
            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 50,
                FragmentLife = 15,
                SizeMultiplier = 0.6f,
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

            // 5. 发光绘制（微弱，星火弹风格）
            // Behaviors.Add(new GlowDrawBehavior
            // {
            //     GlowColor = new Color(255, 180, 60),
            //     GlowLayers = 0,
            //     GlowBaseScale = 0f,
            //     GlowScaleIncrement = 0f,
            //     GlowBaseAlpha = 0.3f,
            //     GlowAlphaDecay = 0f,
            //     GlowAlphaMultiplier = 0f,
            //     EnableLight = false
            // });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 99; // 不因命中敌人而销毁
            Projectile.timeLeft = 120; // 星火弹 120 帧
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }
    }
}
