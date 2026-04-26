using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.SmoothMovement.Interpolators
{
    /// <summary>
    /// Lerp插值器（线性插值速度方向）
    /// 
    /// 对应星河蛊的过渡方式：
    ///   v_new = Lerp(v_old, target_dir × speed, transitionSpeed)
    /// 
    /// 与指数平滑的区别：
    /// - Lerp插值的是"速度向量"本身，而非"目标方向"
    /// - transitionSpeed 固定时，收敛速度恒定
    /// - 适合状态切换时的平滑过渡
    /// </summary>
    public class LerpSmoothInterpolator : IInterpolator
    {
        private readonly float transitionSpeed;

        /// <param name="transitionSpeed">Lerp过渡速度 (0~1)，越大越快。典型值：0.08f</param>
        public LerpSmoothInterpolator(float transitionSpeed = 0.08f)
        {
            this.transitionSpeed = transitionSpeed;
        }

        public Vector2 UpdateVelocity(Vector2 currentVelocity, Vector2 targetVelocity)
        {
            return Vector2.Lerp(currentVelocity, targetVelocity, transitionSpeed);
        }
    }
}
