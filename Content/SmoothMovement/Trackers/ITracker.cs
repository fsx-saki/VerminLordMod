using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.SmoothMovement.Trackers
{
    /// <summary>
    /// 追踪器接口：定义弹幕追踪目标的通用契约
    /// </summary>
    public interface ITracker
    {
        /// <summary>
        /// 计算当前帧的目标速度向量
        /// </summary>
        /// <param name="projectilePos">弹幕当前位置</param>
        /// <param name="projectileVel">弹幕当前速度</param>
        /// <param name="targetPos">目标位置</param>
        /// <param name="targetVel">目标速度（用于预判）</param>
        /// <returns>目标速度向量</returns>
        Vector2 GetTargetVelocity(Vector2 projectilePos, Vector2 projectileVel, Vector2 targetPos, Vector2 targetVel);
    }
}
