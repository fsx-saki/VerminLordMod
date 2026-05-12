using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 水系拖尾行为 — 将 TrailManager + WaterTrail 封装为 IBulletBehavior。
    ///
    /// 自动管理 WaterTrail 的创建、更新、绘制和清理生命周期。
    /// 使用圆形渐变粒子模拟水滴和泡泡，配合 AlphaBlend 混合呈现水的实在质感。
    ///
    /// 使用示例：
    ///   Behaviors.Add(new WaterTrailBehavior
    ///   {
    ///       MaxFragments = 80,
    ///       ParticleLife = 25,
    ///       SizeMultiplier = 1.0f,
    ///       SplashSpeed = 4f,
    ///       ColorStart = new Color(30, 100, 200, 200),
    ///       ColorEnd = new Color(60, 150, 255, 0),
    ///       AutoDraw = true,
    ///       SuppressDefaultDraw = false
    ///   });
    /// </summary>
    public class WaterTrailBehavior : IBulletBehavior
    {
        public string Name => "WaterTrail";

        /// <summary>拖尾管理器实例</summary>
        public TrailManager TrailManager { get; } = new TrailManager();

        /// <summary>WaterTrail 实例（运行时只读）</summary>
        public WaterTrail Trail { get; private set; }

        // ===== 可配置参数（直接映射到 WaterTrail） =====

        /// <summary>最大粒子总数</summary>
        public int MaxFragments { get; set; } = 80;

        /// <summary>粒子存活时间（帧）</summary>
        public int ParticleLife { get; set; } = 25;

        /// <summary>基础生成间隔（帧）</summary>
        public int SpawnInterval { get; set; } = 1;

        /// <summary>粒子大小倍率</summary>
        public float SizeMultiplier { get; set; } = 1.0f;

        /// <summary>粒子生成概率（0~1）</summary>
        public float SpawnChance { get; set; } = 0.8f;

        /// <summary>尾部飞溅速度</summary>
        public float SplashSpeed { get; set; } = 4f;

        /// <summary>尾部飞溅角度范围（弧度）</summary>
        public float SplashAngle { get; set; } = 1.5f;

        /// <summary>反向惯性系数</summary>
        public float InertiaFactor { get; set; } = 0.4f;

        /// <summary>随机扩散范围</summary>
        public float RandomSpread { get; set; } = 2f;

        /// <summary>起始颜色（深蓝，水滴感）</summary>
        public Color ColorStart { get; set; } = new Color(30, 100, 200, 200);

        /// <summary>结束颜色（浅蓝透明）</summary>
        public Color ColorEnd { get; set; } = new Color(60, 150, 255, 0);

        /// <summary>是否启用速度自适应密度</summary>
        public bool AdaptiveDensity { get; set; } = true;

        /// <summary>速度自适应密度阈值</summary>
        public float AdaptiveSpeedThreshold { get; set; } = 3f;

        /// <summary>速度自适应密度系数</summary>
        public float AdaptiveDensityFactor { get; set; } = 4f;

        /// <summary>生成位置偏移</summary>
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        /// <summary>重力系数（正值向下）</summary>
        public float Gravity { get; set; } = 0.12f;

        /// <summary>空气阻力系数</summary>
        public float AirResistance { get; set; } = 0.96f;

        /// <summary>泡泡大小倍率（相对于水滴）</summary>
        public float BubbleChance { get; set; } = 0.15f;

        /// <summary>泡泡大小倍率（相对于水滴）</summary>
        public float BubbleSizeMultiplier { get; set; } = 1.8f;

        /// <summary>是否启用速度自适应存活时间</summary>
        public bool AdaptiveLife { get; set; } = true;

        /// <summary>拖尾目标长度（像素）</summary>
        public float AdaptiveTargetLength { get; set; } = 60f;

        /// <summary>速度对存活时间的影响指数（0~1）</summary>
        public float SpeedLifeExponent { get; set; } = 0.35f;

        /// <summary>粒子存活时间下限（帧）</summary>
        public int MinParticleLife { get; set; } = 8;

        /// <summary>
        /// 是否在 PreDraw 中由本行为绘制拖尾。
        /// 如果为 false，需要外部自行调用 TrailManager.Draw()。
        /// </summary>
        public bool AutoDraw { get; set; } = true;

        /// <summary>
        /// 是否在 PreDraw 中返回 false（阻止引擎默认绘制）。
        /// 当弹幕有自定义绘制（如发光层）时需要设为 true。
        /// </summary>
        public bool SuppressDefaultDraw { get; set; } = false;

        public WaterTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            // 创建并配置 WaterTrail
            Trail = new WaterTrail
            {
                MaxFragments = MaxFragments,
                ParticleLife = ParticleLife,
                SpawnInterval = SpawnInterval,
                SizeMultiplier = SizeMultiplier,
                SpawnChance = SpawnChance,
                SplashSpeed = SplashSpeed,
                SplashAngle = SplashAngle,
                InertiaFactor = InertiaFactor,
                RandomSpread = RandomSpread,
                ColorStart = ColorStart,
                ColorEnd = ColorEnd,
                AdaptiveDensity = AdaptiveDensity,
                AdaptiveSpeedThreshold = AdaptiveSpeedThreshold,
                AdaptiveDensityFactor = AdaptiveDensityFactor,
                SpawnOffset = SpawnOffset,
                Gravity = Gravity,
                AirResistance = AirResistance,
                BubbleChance = BubbleChance,
                BubbleSizeMultiplier = BubbleSizeMultiplier,
                AdaptiveLife = AdaptiveLife,
                AdaptiveTargetLength = AdaptiveTargetLength,
                SpeedLifeExponent = SpeedLifeExponent,
                MinParticleLife = MinParticleLife,
            };

            TrailManager.Add(Trail);
        }

        public void Update(Projectile projectile)
        {
            TrailManager.Update(projectile.Center, projectile.velocity);
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            TrailManager.Clear();
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            if (AutoDraw)
            {
                TrailManager.Draw(spriteBatch);
            }
            return !SuppressDefaultDraw;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
