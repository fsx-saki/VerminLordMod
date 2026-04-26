using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.SmoothMovement.Orbiters
{
    /// <summary>
    /// 轨道器接口：定义弹幕围绕某中心点运动的通用契约
    /// </summary>
    public interface IOrbiter
    {
        /// <summary>
        /// 计算当前帧的目标轨道位置
        /// </summary>
        /// <param name="center">轨道中心（通常是玩家位置）</param>
        /// <returns>目标轨道位置</returns>
        Vector2 GetOrbitTarget(Vector2 center);
    }
}
