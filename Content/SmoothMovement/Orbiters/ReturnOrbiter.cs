using Microsoft.Xna.Framework;
using System;

namespace VerminLordMod.Content.SmoothMovement.Orbiters
{
    /// <summary>
    /// 归位轨道器（从任意位置平滑返回轨道）
    /// 
    /// 对应星河蛊返回态的逻辑：
    ///   在轨道上选择一个偏移角度（通常 +90°）作为返回目标点
    ///   弹幕飞向该点，到达后切换回轨道态
    /// 
    /// 特点：避免弹幕直接飞回玩家位置造成"撞脸"，而是优雅地绕回轨道
    /// </summary>
    public class ReturnOrbiter : IOrbiter
    {
        private readonly float angleOffset;
        private readonly float baseRadius;
        private readonly float ellipseCompression;
        private float currentAngle;

        /// <param name="angleOffset">返回目标在轨道上的角度偏移。典型值：MathHelper.PiOver2（90°）</param>
        /// <param name="baseRadius">轨道基准半径（像素）。典型值：50f</param>
        /// <param name="ellipseCompression">椭圆压缩比。典型值：0.6f</param>
        public ReturnOrbiter(
            float angleOffset = 1.57079637f,
            float baseRadius = 50f,
            float ellipseCompression = 0.6f)
        {
            this.angleOffset = angleOffset;
            this.baseRadius = baseRadius;
            this.ellipseCompression = ellipseCompression;
        }

        public Vector2 GetOrbitTarget(Vector2 center)
        {
            float returnAngle = currentAngle + angleOffset;
            return center + new Vector2(
                (float)Math.Cos(returnAngle) * baseRadius,
                (float)Math.Sin(returnAngle) * baseRadius * ellipseCompression
            );
        }

        /// <summary>设置当前轨道角度（从轨道器同步）</summary>
        public void SyncAngle(float orbitAngle)
        {
            currentAngle = orbitAngle;
        }
    }
}
