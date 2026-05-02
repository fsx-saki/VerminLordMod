using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 液态拖尾初始化辅助类 — 抽象出 AcidWaterProj/BloodWaterProj/BloodDropProj 中重复的 LiquidTrail 配置。
    ///
    /// 所有方法都接受 TrailBehavior 实例（而非 BaseBullet），
    /// 因为 Behaviors 列表是 protected 的，外部类无法直接访问。
    ///
    /// 使用方式：
    /// <code>
    /// protected override void OnSpawned(IEntitySource source)
    /// {
    ///     var trail = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
    ///     LiquidTrailHelper.SetupBloodTrail(trail, tex);
    /// }
    /// </code>
    /// </summary>
    public static class LiquidTrailHelper
    {
        /// <summary>
        /// 设置液态拖尾（完整参数版）
        /// </summary>
        /// <param name="trail">TrailBehavior 实例</param>
        /// <param name="texture">弹幕贴图</param>
        /// <param name="colorStart">起始颜色</param>
        /// <param name="colorEnd">结束颜色</param>
        /// <param name="maxFragments">最大碎片数</param>
        /// <param name="fragmentLife">碎片寿命（帧）</param>
        /// <param name="sizeMultiplier">大小倍率</param>
        /// <param name="spawnInterval">生成间隔（帧）</param>
        /// <param name="buoyancy">浮力（正=上浮，负=下沉）</param>
        /// <param name="airResistance">空气阻力</param>
        /// <param name="inertiaFactor">惯性因子</param>
        /// <param name="splashFactor">飞溅因子</param>
        /// <param name="splashAngle">飞溅角度</param>
        /// <param name="randomSpread">随机扩散</param>
        /// <returns>创建的 LiquidTrail 实例</returns>
        public static LiquidTrail SetupLiquidTrail(
            TrailBehavior trail,
            Texture2D texture,
            Color colorStart,
            Color colorEnd,
            int maxFragments = 30,
            int fragmentLife = 20,
            float sizeMultiplier = 0.8f,
            int spawnInterval = 1,
            float buoyancy = 0f,
            float airResistance = 0.97f,
            float inertiaFactor = 0.5f,
            float splashFactor = 0.3f,
            float splashAngle = 0.6f,
            float randomSpread = 0.7f)
        {
            var liquid = trail.TrailManager.AddLiquidTrail(
                texture,
                colorStart: colorStart,
                colorEnd: colorEnd,
                maxFragments: maxFragments,
                fragmentLife: fragmentLife,
                sizeMultiplier: sizeMultiplier,
                spawnInterval: spawnInterval);

            liquid.Buoyancy = buoyancy;
            liquid.AirResistance = airResistance;
            liquid.InertiaFactor = inertiaFactor;
            liquid.SplashFactor = splashFactor;
            liquid.SplashAngle = splashAngle;
            liquid.RandomSpread = randomSpread;

            return liquid;
        }

        /// <summary>
        /// 设置酸性液态拖尾（绿色调，用于 AcidWaterProj）
        /// </summary>
        public static LiquidTrail SetupAcidTrail(TrailBehavior trail, Texture2D texture)
        {
            return SetupLiquidTrail(trail, texture,
                colorStart: new Color(80, 255, 50, 200),
                colorEnd: new Color(20, 120, 10, 0),
                maxFragments: 30, fragmentLife: 20, sizeMultiplier: 0.8f,
                buoyancy: 0.03f, airResistance: 0.97f, inertiaFactor: 0.5f,
                splashFactor: 0.3f, splashAngle: 0.6f, randomSpread: 0.7f);
        }

        /// <summary>
        /// 设置血液液态拖尾（红色调，用于 BloodWaterProj / BloodDropProj）
        /// </summary>
        public static LiquidTrail SetupBloodTrail(TrailBehavior trail, Texture2D texture,
            int maxFragments = 15, int fragmentLife = 15,
            float sizeMultiplier = 0.3f)
        {
            return SetupLiquidTrail(trail, texture,
                colorStart: new Color(180, 20, 20, 200),
                colorEnd: new Color(80, 0, 0, 0),
                maxFragments: maxFragments,
                fragmentLife: fragmentLife,
                sizeMultiplier: sizeMultiplier,
                buoyancy: -0.02f, airResistance: 0.95f, inertiaFactor: 0.3f,
                splashFactor: 0.4f, splashAngle: 0.5f, randomSpread: 0.6f);
        }

        /// <summary>
        /// 设置血液液态拖尾（大尺寸版，用于 BloodHandprintsProj 的拖尾）
        /// </summary>
        public static LiquidTrail SetupBloodTrailLarge(TrailBehavior trail, Texture2D texture)
        {
            return SetupLiquidTrail(trail, texture,
                colorStart: new Color(180, 20, 20, 200),
                colorEnd: new Color(80, 0, 0, 0),
                maxFragments: 20, fragmentLife: 20, sizeMultiplier: 1.2f,
                buoyancy: -0.02f, airResistance: 0.95f, inertiaFactor: 0.3f,
                splashFactor: 0.4f, splashAngle: 0.5f, randomSpread: 0.6f);
        }
    }
}
