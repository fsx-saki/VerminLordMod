using Microsoft.Xna.Framework;
using Terraria;

namespace VerminLordMod.Common.SummonBehaviors
{
    /// <summary>
    /// 召唤物辅助工具 — 提供目标搜索、距离计算等通用方法。
    /// </summary>
    public static class SummonHelper
    {
        /// <summary>
        /// 寻找最近的敌人。
        /// 优先使用玩家的右键锁定目标（OwnerMinionAttackTargetNPC）。
        /// </summary>
        /// <param name="center">搜索中心</param>
        /// <param name="range">搜索范围，0 表示无限</param>
        /// <param name="projectile">召唤物 Projectile（用于获取 OwnerMinionAttackTargetNPC）</param>
        /// <returns>最近的敌人，无则返回 null</returns>
        public static NPC FindNearestEnemy(Vector2 center, float range, Projectile projectile)
        {
            NPC target = null;
            float minDist = range > 0 ? range : float.MaxValue;

            NPC lockedTarget = projectile.OwnerMinionAttackTargetNPC;
            if (lockedTarget != null && lockedTarget.CanBeChasedBy(projectile))
            {
                float dist = (lockedTarget.Center - center).Length();
                if (range <= 0 || dist < range)
                {
                    return lockedTarget;
                }
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(projectile))
                {
                    float dist = (npc.Center - center).Length();
                    if (dist < minDist)
                    {
                        minDist = dist;
                        target = npc;
                    }
                }
            }

            return target;
        }

        /// <summary>
        /// 寻找最近的敌人（无 Projectile 引用版本，不检查 OwnerMinionAttackTargetNPC）。
        /// </summary>
        public static NPC FindNearestEnemy(Vector2 center, float range)
        {
            NPC target = null;
            float minDist = range > 0 ? range : float.MaxValue;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.life > 0)
                {
                    float dist = (npc.Center - center).Length();
                    if (dist < minDist)
                    {
                        minDist = dist;
                        target = npc;
                    }
                }
            }

            return target;
        }
    }
}