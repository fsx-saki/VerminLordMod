using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.SmoothMovement.Interpolators
{
    /// <summary>
    /// 指数平滑插值器（一阶低通滤波器）
    /// 
    /// 对应旋风蛊/月光ProjS的追踪方式：
    ///   v_new = (v_target + v_old × weight) / (weight + 1)
    /// 
    /// 平滑系数 α = 1/(weight+1)，α 越小越平滑（反应越慢）
    /// weight=10 → α≈0.091（旋风蛊风格）
    /// weight=4  → α=0.2（反应更快）
    /// weight=19 → α=0.05（更平滑）
    /// 
    /// 特点：永远追不上目标位置，但有自然的"拖尾感"
    /// </summary>
    public class ExponentialSmoothInterpolator : IInterpolator
    {
        private readonly float weight;

        /// <param name="weight">旧速度的权重（越大越平滑）。典型值：10</param>
        public ExponentialSmoothInterpolator(float weight = 10f)
        {
            this.weight = weight;
        }

        public Vector2 UpdateVelocity(Vector2 currentVelocity, Vector2 targetVelocity)
        {
            return (targetVelocity + currentVelocity * weight) / (weight + 1f);
        }

        /// <summary>获取当前平滑系数 α</summary>
        public float Alpha => 1f / (weight + 1f);
    }
}
