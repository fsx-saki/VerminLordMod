using Microsoft.Xna.Framework;
using Terraria;

namespace VerminLordMod.Common.SummonBehaviors.Styles
{
    /// <summary>
    /// 运动方式接口 — 定义"怎么动"。
    ///
    /// 这是分层架构的第一层（最底层），只关心运动本身的物理特性：
    /// 飞行、步行、悬浮、瞬移等。
    ///
    /// 与 ITargetSelector（"去哪"）组合后形成 MovementModule（"怎么去哪"）。
    ///
    /// 设计哲学：
    /// 运动方式不关心目标是谁、不关心攻击逻辑，
    /// 只负责根据给定方向和速度参数产生速度向量。
    /// </summary>
    public interface IMovementStyle
    {
        /// <summary>运动方式名称</summary>
        string Name { get; }

        /// <summary>
        /// 根据当前状态计算新的速度向量。
        /// </summary>
        /// <param name="projectile">召唤物</param>
        /// <param name="currentVelocity">当前速度</param>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="desiredSpeed">期望速度</param>
        /// <returns>计算后的速度向量</returns>
        Vector2 CalculateVelocity(Projectile projectile, Vector2 currentVelocity, Vector2 targetPosition, float desiredSpeed);

        /// <summary>
        /// 根据速度更新召唤物的旋转角度。
        /// </summary>
        void UpdateRotation(Projectile projectile, Vector2 velocity);
    }
}