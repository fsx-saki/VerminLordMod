using Microsoft.Xna.Framework;
using Terraria;

namespace VerminLordMod.Common.SummonBehaviors.Styles
{
    /// <summary>
    /// 飞行运动 — 平滑加速飞向目标。
    ///
    /// 特性：
    /// - 无重力影响
    /// - 使用 Lerp 平滑加速/减速
    /// - 旋转朝向速度方向
    ///
    /// 适用场景：大多数飞行召唤物（火灵、暗影兽、光精灵等）
    ///
    /// 可配置参数：
    /// - Acceleration: 加速度因子（0~1，越大越快达到目标速度）
    /// - RotationOffset: 旋转偏移（默认 PiOver2，贴图朝上）
    /// </summary>
    public class FlyStyle : IMovementStyle
    {
        public string Name => "Fly";

        /// <summary>加速度因子（0~1），越大越快达到目标速度</summary>
        public float Acceleration { get; set; } = 0.1f;

        /// <summary>旋转偏移量（弧度）</summary>
        public float RotationOffset { get; set; } = MathHelper.PiOver2;

        /// <summary>最小距离阈值，小于此距离开始减速</summary>
        public float SlowdownDistance { get; set; } = 60f;

        public FlyStyle() { }

        public FlyStyle(float acceleration)
        {
            Acceleration = acceleration;
        }

        public Vector2 CalculateVelocity(Projectile projectile, Vector2 currentVelocity, Vector2 targetPosition, float desiredSpeed)
        {
            Vector2 toTarget = targetPosition - projectile.Center;
            float distance = toTarget.Length();

            if (distance < 1f)
                return currentVelocity * 0.9f;

            Vector2 direction = toTarget / distance;

            float speed = desiredSpeed;
            if (distance < SlowdownDistance)
            {
                speed = desiredSpeed * (distance / SlowdownDistance);
                speed = System.Math.Max(speed, 1f);
            }

            Vector2 desiredVelocity = direction * speed;
            return Vector2.Lerp(currentVelocity, desiredVelocity, Acceleration);
        }

        public void UpdateRotation(Projectile projectile, Vector2 velocity)
        {
            if (velocity.LengthSquared() > 0.1f)
            {
                projectile.rotation = velocity.ToRotation() + RotationOffset;
            }
        }
    }
}