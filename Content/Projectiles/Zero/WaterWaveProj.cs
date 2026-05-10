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
    /// 水道水波弹 — 仿真水波扩散弹幕。
    ///
    /// 设计哲学（与水弹 WaterBaseProj 一致）：
    /// - 本体由粒子组成（WaveBodyBehavior），不依赖贴图
    /// - 粒子在二维平面上铺开，模拟水波扩散效果
    /// - 拖尾使用 WaterTrailBehavior（水滴 + 泡泡）
    /// - 崩解使用 ParticleBurstBehavior → WaterDropProj
    /// - 所有行为可配置、可复用
    ///
    /// 视觉效果：
    /// - 波浪形粒子体在二维平面上铺开（沿飞行方向 × 垂直方向）
    /// - 纵波（疏密波）：粒子沿传播方向前后振动
    /// - 横向涟漪：垂直方向粒子左右摆动，模拟水波扩散
    /// - 粒子从中心向边缘逐渐缩小淡出
    /// - 拖尾为水波经过后留下的水滴和泡沫
    /// - 碰到物块时崩解为 WaterDropProj 飞溅
    /// </summary>
    public class WaterWaveProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 1. 直线飞行（较慢，水波扩散感）
            Behaviors.Add(new AimBehavior(speed: 6f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 波浪形粒子体 — 水波本体（二维网格排列）
            var waveBody = new WaveBodyBehavior
            {
                WaveLength = 60f,           // 波长（一个完整疏密周期）
                Amplitude = 10f,            // 纵向振动幅度（前后偏移）
                Width = 80f,                // 水波宽度（垂直方向铺开范围）
                Rows = 8,                   // 垂直方向粒子行数
                ParticlesPerRow = 24,       // 每行粒子数（沿飞行方向）
                ParticleSize = 1.0f,        // 粒子大小
                WaveSpeed = 0.1f,           // 波动速度
                DecayRate = 0.35f,          // 边缘衰减
                CompressionScale = 1.6f,    // 密部粒子缩放
                RarefactionScale = 0.4f,    // 疏部粒子缩放
                VerticalDecay = 0.6f,       // 垂直方向边缘衰减
                RippleAmplitude = 4f,       // 横向涟漪幅度
                ColorStart = new Color(30, 100, 200, 200),
                ColorEnd = new Color(20, 60, 150, 0),
                Alpha = 1.0f,
                EnableLight = true,
                LightColor = new Vector3(0.1f, 0.3f, 0.6f),
                AutoGenerateTexture = true,
                TextureSize = 12,
            };
            Behaviors.Add(waveBody);

            // 3. 水系拖尾 — 水波经过后留下的水滴和泡沫
            Behaviors.Add(new WaterTrailBehavior
            {
                MaxFragments = 80,
                ParticleLife = 25,
                SizeMultiplier = 0.5f,
                SpawnChance = 0.8f,
                SplashSpeed = 0.5f,
                SplashAngle = 0.3f,
                InertiaFactor = 0.05f,
                RandomSpread = 1.0f,
                Gravity = 0.15f,
                AirResistance = 0.97f,
                BubbleChance = 0.4f,
                BubbleSizeMultiplier = 1.8f,
                ColorStart = new Color(30, 100, 200, 200),
                ColorEnd = new Color(30, 100, 200, 0),
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            // 4. 崩解（命中/销毁时）— 生成 WaterDropProj 向四面八方飞散
            Behaviors.Add(new ParticleBurstBehavior
            {
                ChildProjectileType = ModContent.ProjectileType<WaterDropProj>(),
                Count = 10,
                SpeedMin = 2f,
                SpeedMax = 5f,
                SpreadRadius = 8f,
                AngleSpread = 0.5f,
                SpawnExtraDust = true,
                ExtraDustCount = 12,
                DustType = DustID.MagicMirror,
                DustColorStart = new Color(30, 100, 200, 200),
                DustColorEnd = new Color(30, 100, 200, 0),
                DustScaleMin = 0.3f,
                DustScaleMax = 0.7f,
                DustSpeedMin = 1f,
                DustSpeedMax = 4f,
                DustNoGravity = false,
            });

            // 5. 液体反应（融入水中）
            Behaviors.Add(new LiquidReactionBehavior
            {
                EnableMerge = true,
                EnableVaporize = false,
                EnableShimmerBounce = true,
                MergeDustCount = 12,
                MergeDustType = DustID.Water
            });

            // 6. 阻止默认贴图绘制 — 本体由 WaveBodyBehavior 的粒子组成
            Behaviors.Add(new SuppressDrawBehavior());
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1; // 穿透所有敌人
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        /// <summary>
        /// 碰到物块时销毁弹幕（水波撞到障碍物崩解）。
        /// ParticleBurstBehavior.OnKill 自动触发崩解效果。
        /// </summary>
        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
