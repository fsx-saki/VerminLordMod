using Microsoft.Xna.Framework;
using Terraria;

namespace VerminLordMod.Common.SummonBehaviors.Selectors
{
    /// <summary>
    /// 目标选择器接口 — 定义"去哪"。
    ///
    /// 这是分层架构的第二层，只关心选择目标位置：
    /// 最近敌人、玩家位置、锁定敌人、环绕玩家等。
    ///
    /// 与 IMovementStyle（"怎么动"）组合后形成 MovementModule（"怎么去哪"）。
    ///
    /// 设计哲学：
    /// 目标选择器不关心怎么移动、不关心攻击逻辑，
    /// 只负责根据当前状态返回一个目标坐标。
    /// 返回 null 表示当前无有效目标。
    /// </summary>
    public interface ITargetSelector
    {
        /// <summary>选择器名称</summary>
        string Name { get; }

        /// <summary>
        /// 选择目标位置。
        /// </summary>
        /// <param name="projectile">召唤物</param>
        /// <param name="owner">召唤者玩家</param>
        /// <returns>目标位置，null 表示无目标</returns>
        Vector2? SelectTarget(Projectile projectile, Player owner);
    }
}