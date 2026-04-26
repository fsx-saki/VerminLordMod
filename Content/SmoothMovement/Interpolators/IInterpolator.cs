using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.SmoothMovement.Interpolators
{
    /// <summary>
    /// 插值器接口：定义速度/位置平滑更新的通用契约
    /// </summary>
    public interface IInterpolator
    {
        /// <summary>
        /// 对速度进行平滑插值，返回新的速度向量
        /// </summary>
        /// <param name="currentVelocity">当前速度</param>
        /// <param name="targetVelocity">目标速度（由外部计算得出）</param>
        /// <returns>平滑后的新速度</returns>
        Vector2 UpdateVelocity(Vector2 currentVelocity, Vector2 targetVelocity);
    }
}
