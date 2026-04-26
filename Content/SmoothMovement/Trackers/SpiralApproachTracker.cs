using Microsoft.Xna.Framework;
using System;

namespace VerminLordMod.Content.SmoothMovement.Trackers
{
    /// <summary>
    /// 螺旋逼近追踪器
    /// 
    /// 对应星河蛊追击态的追踪方式：
    ///   在指向目标的方向上叠加正弦波横向偏移，形成螺旋逼近路径
    /// 
    /// 公式：
    ///   perpendicular = rotate90(normalize(toEnemy))
    ///   arcOffset = perpendicular × sin(ω × timer) × amplitude × spiralFactor
    ///   target = enemyPos + arcOffset
    /// 
    /// 特点：从侧面绕向敌人，而非直线冲锋，视觉上更灵动
    /// </summary>
    public class SpiralApproachTracker : ITracker
    {
        private readonly float speed;
        private readonly float angularFrequency;
        private readonly float baseAmplitude;
        private readonly float spiralRampDuration;
        private float timer;

        /// <param name="speed">追踪速度。典型值：12f</param>
        /// <param name="angularFrequency">螺旋角频率。典型值：0.08f rad/frame</param>
        /// <param name="baseAmplitude">螺旋基准幅度（像素）。典型值：60f</param>
        /// <param name="spiralRampDuration">螺旋幅度从最小到最大的渐增时间（帧）。典型值：60f</param>
        public SpiralApproachTracker(
            float speed = 12f,
            float angularFrequency = 0.08f,
            float baseAmplitude = 60f,
            float spiralRampDuration = 60f)
        {
            this.speed = speed;
            this.angularFrequency = angularFrequency;
            this.baseAmplitude = baseAmplitude;
            this.spiralRampDuration = spiralRampDuration;
        }

        public Vector2 GetTargetVelocity(Vector2 projectilePos, Vector2 projectileVel, Vector2 targetPos, Vector2 targetVel)
        {
            timer++;

            Vector2 toTarget = targetPos - projectilePos;
            float dist = toTarget.Length();

            if (dist < 1f)
                return Vector2.Zero;

            // 螺旋幅度渐增因子
            float progress = MathHelper.Min(timer / spiralRampDuration, 1f);
            float spiralFactor = MathHelper.Lerp(0.3f, 1f, progress);

            // 计算法向量（垂直方向）
            Vector2 direction = toTarget / dist;
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

            // 正弦波横向偏移
            float arcAngle = timer * angularFrequency;
            Vector2 arcOffset = perpendicular * (float)Math.Sin(arcAngle) * baseAmplitude * spiralFactor;

            // 目标点 = 敌人位置 + 横向偏移
            Vector2 targetPoint = targetPos + arcOffset;

            Vector2 toTargetPoint = targetPoint - projectilePos;
            if (toTargetPoint == Vector2.Zero)
                return Vector2.Zero;
            toTargetPoint.Normalize();
            return toTargetPoint * speed;
        }

        /// <summary>重置计时器（状态切换时调用）</summary>
        public void Reset()
        {
            timer = 0f;
        }
    }
}
