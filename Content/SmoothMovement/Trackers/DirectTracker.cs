using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.SmoothMovement.Trackers
{
    /// <summary>
    /// 直接追踪器（无预判）
    /// 
    /// 对应旋风蛊/月光ProjS的追踪方式：
    ///   直接指向目标当前位置，以固定速度飞行
    /// 
    /// 公式：
    ///   targetVel = normalize(targetPos - projectilePos) × speed
    /// </summary>
    public class DirectTracker : ITracker
    {
        private readonly float speed;

        /// <param name="speed">追踪速度。典型值：10f</param>
        public DirectTracker(float speed = 10f)
        {
            this.speed = speed;
        }

        public Vector2 GetTargetVelocity(Vector2 projectilePos, Vector2 projectileVel, Vector2 targetPos, Vector2 targetVel)
        {
            Vector2 direction = targetPos - projectilePos;
            if (direction == Vector2.Zero)
                return Vector2.Zero;
            direction.Normalize();
            return direction * speed;
        }
    }
}
