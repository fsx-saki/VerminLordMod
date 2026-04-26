using Microsoft.Xna.Framework;
using System;

namespace VerminLordMod.Content.SmoothMovement.Orbiters
{
    /// <summary>
    /// 混沌轨道器（多频正弦波半径扰动）
    /// 
    /// 对应星河蛊轨道态的轨道方式：
    ///   在三轴（x/y/半径）上叠加不同频率的正弦波，产生拟混沌效果
    /// 
    /// 公式：
    ///   irregularRadius = baseRadius 
    ///     + sin(ω₁t) × A₁ 
    ///     + sin(ω₂t × 1.7) × A₂ 
    ///     + sin(ω₃t × 2.3) × A₃
    /// 
    /// 频率比为无理数比，轨道永不重复，视觉上丰富多变
    /// </summary>
    public class ChaoticOrbiter : IOrbiter
    {
        private float angle;
        private float phase1;
        private float phase2;
        private float phase3;
        private readonly float angularSpeed;
        private readonly float baseRadius;
        private readonly float radiusWaveRange;
        private readonly float ellipseCompression;

        /// <param name="initialAngle">初始角度（弧度）。典型值：0f</param>
        /// <param name="angularSpeed">角速度（弧度/帧）。典型值：0.025f</param>
        /// <param name="baseRadius">基准轨道半径（像素）。典型值：50f</param>
        /// <param name="radiusWaveRange">半径波动幅度（像素）。典型值：25f</param>
        /// <param name="ellipseCompression">椭圆压缩比（Y轴 = radius × compression）。典型值：0.6f</param>
        public ChaoticOrbiter(
            float initialAngle = 0f,
            float angularSpeed = 0.025f,
            float baseRadius = 50f,
            float radiusWaveRange = 25f,
            float ellipseCompression = 0.6f)
        {
            this.angle = initialAngle;
            this.angularSpeed = angularSpeed;
            this.baseRadius = baseRadius;
            this.radiusWaveRange = radiusWaveRange;
            this.ellipseCompression = ellipseCompression;
        }

        public Vector2 GetOrbitTarget(Vector2 center)
        {
            angle += angularSpeed;
            phase1 += 0.017f;
            phase2 += 0.013f;
            phase3 += 0.009f;

            // 三频正弦波叠加 → 拟混沌半径
            float irregularRadius = baseRadius
                + (float)Math.Sin(phase1) * radiusWaveRange * 0.5f
                + (float)Math.Sin(phase2 * 1.7f) * radiusWaveRange * 0.3f
                + (float)Math.Sin(phase3 * 2.3f) * radiusWaveRange * 0.2f;

            return center + new Vector2(
                (float)Math.Cos(angle) * irregularRadius,
                (float)Math.Sin(angle) * irregularRadius * ellipseCompression
            );
        }

        /// <summary>获取当前轨道角度</summary>
        public float CurrentAngle => angle;

        /// <summary>设置轨道角度（用于状态切换时同步角度）</summary>
        public void SetAngle(float newAngle)
        {
            angle = newAngle;
        }

        /// <summary>获取当前不规则半径</summary>
        public float GetCurrentRadius()
        {
            return baseRadius
                + (float)Math.Sin(phase1) * radiusWaveRange * 0.5f
                + (float)Math.Sin(phase2 * 1.7f) * radiusWaveRange * 0.3f
                + (float)Math.Sin(phase3 * 2.3f) * radiusWaveRange * 0.2f;
        }
    }
}
