using Microsoft.Xna.Framework;
using Terraria;

namespace VerminLordMod.Common.SummonBehaviors.Selectors
{
    /// <summary>
    /// 最近敌人选择器 — 选择范围内最近的敌人。
    ///
    /// 优先使用玩家的右键锁定目标（OwnerMinionAttackTargetNPC），
    /// 其次自动搜索范围内最近的敌人。
    ///
    /// 适用场景：几乎所有攻击型召唤物
    ///
    /// 可配置参数：
    /// - Range: 搜索范围，0 表示无限
    /// </summary>
    public class NearestEnemySelector : ITargetSelector
    {
        public string Name => "NearestEnemy";

        /// <summary>搜索范围（像素），0 表示无限</summary>
        public float Range { get; set; } = 600f;

        public NearestEnemySelector() { }

        public NearestEnemySelector(float range)
        {
            Range = range;
        }

        public Vector2? SelectTarget(Projectile projectile, Player owner)
        {
            NPC target = SummonHelper.FindNearestEnemy(projectile.Center, Range, projectile);
            return target?.Center;
        }
    }
}