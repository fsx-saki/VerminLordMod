using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.SmoothMovement.Trackers
{
    /// <summary>
    /// 预判追踪器（带目标速度预测）
    /// 
    /// 假设目标保持匀速直线运动，计算命中点：
    ///   timeToTarget = |targetPos - projectilePos| / projectileSpeed
    ///   predictedPos = targetPos + targetVel × timeToTarget
    /// 
    /// 适用于高速弹幕对移动目标的精确打击
    /// </summary>
    public class PredictiveTracker : ITracker
    {
        private readonly float speed;
        private readonly int maxPredictionFrames;

        /// <param name="speed">弹幕飞行速度。典型值：12f</param>
        /// <param name="maxPredictionFrames">最大预判帧数上限，防止目标太远时预判过度。典型值：60</param>
        public PredictiveTracker(float speed = 12f, int maxPredictionFrames = 60)
        {
            this.speed = speed;
            this.maxPredictionFrames = maxPredictionFrames;
        }

        public Vector2 GetTargetVelocity(Vector2 projectilePos, Vector2 projectileVel, Vector2 targetPos, Vector2 targetVel)
        {
            Vector2 toTarget = targetPos - projectilePos;
            float dist = toTarget.Length();

            if (dist < 1f)
                return Vector2.Zero;

            // 估算飞行时间（限制最大预判帧数）
            float timeToTarget = MathHelper.Min(dist / speed, maxPredictionFrames);

            // 预测目标位置
            Vector2 predictedPos = targetPos + targetVel * timeToTarget;

            Vector2 direction = predictedPos - projectilePos;
            if (direction == Vector2.Zero)
                return Vector2.Zero;
            direction.Normalize();
            return direction * speed;
        }
    }
}
