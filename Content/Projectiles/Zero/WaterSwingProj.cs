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
    /// 水道摆动水弹 — 小一号的水弹，以大角度圆弧轨迹通过鼠标指针。
    ///
    /// 设计哲学（与水弹 WaterBaseProj 一致的行为组合模式）：
    /// - 本体由粒子组成（ParticleBodyBehavior），但更小（更少粒子、更小半径）
    /// - 使用 SwingHomingBehavior ArcMode 实现圆弧轨迹：计算过当前位置和鼠标的圆，
    ///   弹幕沿圆弧弯曲飞行，保证大角度通过鼠标指针
    /// - 拖尾使用 WaterTrailBehavior（水滴 + 泡泡）
    /// - 穿透敌人，穿过时洒下 2-3 颗水滴
    /// - 碰到物块时崩解为 WaterDropProj 飞溅
    /// - 所有行为可配置、可复用
    ///
    /// 视觉效果：
    /// - 小水球（约 WaterBaseProj 的 60% 大小）
    /// - 沿圆弧轨迹弯曲飞行，大角度掠过鼠标指针
    /// - 拖尾为水珠和泡泡
    /// - 穿过敌人时洒下水滴，碰到物块时崩解飞溅
    /// </summary>
    public class WaterSwingProj : BaseBullet
    {
        /// <summary>飞行速度</summary>
        private const float FlySpeed = 9f;

        /// <summary>圆弧最小曲率（1/像素），值越大弧线越弯。0.003 保证可见弧度</summary>
        private const float ArcCurvature = 0.004f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new SwingHomingBehavior(
                speed: FlySpeed
            )
            {
                UseArcMode = true,
                ArcMinCurvature = ArcCurvature,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.08f, 0.25f, 0.5f),
            });

            // 2. 粒子体 — 小一号的水球（比 WaterBaseProj 小约 40%）
            var bodyBehavior = new ParticleBodyBehavior(particleCount: 20, bodyRadius: 14f)
            {
                ParticleSize = 1.0f,
                ColorStart = new Color(30, 100, 210, 220),
                ColorEnd = new Color(30, 100, 210, 220),
                SwirlSpeed = 0.12f,
                ReturnForce = 0.9f,
                JitterStrength = 0.6f,
                ShrinkOverLife = true,
                StretchOnMove = true,
                StretchFactor = 0.6f,
                EnableLight = false,
            };
            Behaviors.Add(bodyBehavior);

            // 3. 水系拖尾 — 水珠和泡泡（比 WaterBaseProj 更轻量）
            Behaviors.Add(new WaterTrailBehavior
            {
                MaxFragments = 25,
                ParticleLife = 12,
                SizeMultiplier = 0.5f,
                SpawnChance = 0.6f,
                SplashSpeed = 0.3f,
                SplashAngle = 0.2f,
                InertiaFactor = 0.03f,
                RandomSpread = 0.7f,
                Gravity = 0.18f,
                AirResistance = 0.97f,
                BubbleChance = 0.5f,
                BubbleSizeMultiplier = 1.8f,
                ColorStart = new Color(30, 100, 210, 220),
                ColorEnd = new Color(30, 100, 210, 0),
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            // 4. 法线崩解（命中/销毁时）— 沿法线方向泼洒 WaterDropProj
            Behaviors.Add(new NormalBurstBehavior
            {
                ChildProjectileType = ModContent.ProjectileType<WaterDropProj>(),
                Count = 10,
                SpeedMin = 3f,
                SpeedMax = 7f,
                SpreadRadius = 5f,
                SpreadAngle = 0.5f,
                SideAngle = 1.0f,
                BackSplashChance = 0.03f,
                SpawnExtraDust = true,
                ExtraDustCount = 8,
                DustType = DustID.MagicMirror,
                DustColorStart = new Color(30, 100, 210, 200),
                DustColorEnd = new Color(30, 100, 210, 0),
                DustScaleMin = 0.3f,
                DustScaleMax = 0.7f,
                DustSpeedMin = 1f,
                DustSpeedMax = 3f,
                DustNoGravity = false,
            });

            // 5. 命中洒落 — 穿过敌人时洒下 2-3 颗水滴
            Behaviors.Add(new OnHitDropletBehavior(
                ModContent.ProjectileType<WaterDropProj>(),
                minCount: 2,
                maxCount: 3
            ));

            // 6. 阻止默认贴图绘制 — 本体由 ParticleBodyBehavior 的粒子组成
            Behaviors.Add(new SuppressDrawBehavior());
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 1.0f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 240;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        /// <summary>
        /// 碰到物块时销毁弹幕（水球破掉）。
        /// NormalBurstBehavior.OnKill 自动触发崩解效果。
        /// </summary>
        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
