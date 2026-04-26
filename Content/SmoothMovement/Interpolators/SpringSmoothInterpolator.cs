using Microsoft.Xna.Framework;
using System;

namespace VerminLordMod.Content.SmoothMovement.Interpolators
{
    /// <summary>
    /// 弹簧阻尼平滑插值器（二阶动力学系统）
    ///
    /// 比指数平滑多一个"加速度"维度，能产生更丰富的运动质感：
    /// - 欠阻尼：有过冲，弹性感
    /// - 临界阻尼：最快收敛无过冲
    /// - 过阻尼：缓慢收敛无过冲
    ///
    /// 公式：
    ///   acceleration = (targetPos - position) × stiffness - velocity × damping
    ///   velocity += acceleration × dt
    ///   position += velocity × dt
    ///
    /// 临界阻尼条件：damping = 2 × sqrt(stiffness)
    /// </summary>
    public class SpringSmoothInterpolator
    {
        private Vector2 position;
        private Vector2 velocity;
        private readonly float stiffness;
        private readonly float damping;

        /// <param name="initialPosition">初始位置</param>
        /// <param name="initialVelocity">初始速度</param>
        /// <param name="stiffness">弹簧刚度，控制追踪强度。典型值：0.1f</param>
        /// <param name="damping">阻尼系数，控制过冲抑制。典型值：0.5f</param>
        public SpringSmoothInterpolator(
            Vector2 initialPosition,
            Vector2 initialVelocity,
            float stiffness = 0.1f,
            float damping = 0.5f)
        {
            this.position = initialPosition;
            this.velocity = initialVelocity;
            this.stiffness = stiffness;
            this.damping = damping;
        }

        /// <summary>
        /// 每帧调用，更新弹簧系统
        /// </summary>
        /// <param name="targetPos">目标位置</param>
        /// <param name="dt">时间步长（通常为1f）</param>
        /// <returns>平滑后的新位置</returns>
        public Vector2 Update(Vector2 targetPos, float dt = 1f)
        {
            Vector2 acceleration = (targetPos - position) * stiffness - velocity * damping;
            velocity += acceleration * dt;
            position += velocity * dt;
            return position;
        }

        /// <summary>获取当前速度（可用于设置弹幕的 Projectile.velocity）</summary>
        public Vector2 CurrentVelocity => velocity;

        /// <summary>获取当前位置</summary>
        public Vector2 CurrentPosition => position;

        /// <summary>重置状态</summary>
        public void Reset(Vector2 newPosition, Vector2 newVelocity)
        {
            position = newPosition;
            velocity = newVelocity;
        }

        /// <summary>判断是否为临界阻尼</summary>
        public bool IsCriticallyDamped => Math.Abs(damping - 2f * (float)Math.Sqrt(stiffness)) < 0.001f;

        /// <summary>判断是否为欠阻尼（有过冲）</summary>
        public bool IsUnderDamped => damping < 2f * (float)Math.Sqrt(stiffness);

        /// <summary>判断是否为过阻尼</summary>
        public bool IsOverDamped => damping > 2f * (float)Math.Sqrt(stiffness);
    }
}
