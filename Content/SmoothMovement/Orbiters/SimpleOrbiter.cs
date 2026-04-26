using Microsoft.Xna.Framework;
using System;

namespace VerminLordMod.Content.SmoothMovement.Orbiters
{
    /// <summary>
    /// 简单椭圆轨道器
    /// 
    /// 对应星河蛊轨道态的基础轨道：
    ///   x = center.x + cos(angle) × radiusX
    ///   y = center.y + sin(angle) × radiusY
    /// 
    /// 特点：匀速圆周/椭圆运动，无扰动
    /// </summary>
    public class SimpleOrbiter : IOrbiter
    {
        private float angle;
        private readonly float angularSpeed;
        private readonly float radiusX;
        private readonly float radiusY;

        /// <param name="initialAngle">初始角度（弧度）。典型值：0f</param>
        /// <param name="angularSpeed">角速度（弧度/帧）。典型值：0.025f</param>
        /// <param name="radiusX">水平轨道半径（像素）。典型值：50f</param>
        /// <param name="radiusY">垂直轨道半径（像素）。典型值：30f（椭圆压扁）</param>
        public SimpleOrbiter(float initialAngle = 0f, float angularSpeed = 0.025f, float radiusX = 50f, float radiusY = 30f)
        {
            this.angle = initialAngle;
            this.angularSpeed = angularSpeed;
            this.radiusX = radiusX;
            this.radiusY = radiusY;
        }

        public Vector2 GetOrbitTarget(Vector2 center)
        {
            angle += angularSpeed;
            return center + new Vector2(
                (float)Math.Cos(angle) * radiusX,
                (float)Math.Sin(angle) * radiusY
            );
        }

        /// <summary>获取当前轨道角度</summary>
        public float CurrentAngle => angle;

        /// <summary>设置轨道角度（用于状态切换时同步角度）</summary>
        public void SetAngle(float newAngle)
        {
            angle = newAngle;
        }
    }
}
