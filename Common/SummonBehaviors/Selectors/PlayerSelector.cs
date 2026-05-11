using Microsoft.Xna.Framework;
using Terraria;

namespace VerminLordMod.Common.SummonBehaviors.Selectors
{
    /// <summary>
    /// 玩家选择器 — 返回玩家身边的位置。
    ///
    /// 用于召唤物无敌人时的待机/跟随状态。
    ///
    /// 可配置参数：
    /// - FollowDistance: 跟随距离
    /// - FollowBehind: 是否在玩家身后（根据朝向）
    /// - HeightOffset: 高度偏移
    /// </summary>
    public class PlayerSelector : ITargetSelector
    {
        public string Name => "Player";

        /// <summary>跟随距离（像素）</summary>
        public float FollowDistance { get; set; } = 80f;

        /// <summary>是否在玩家身后（根据朝向）</summary>
        public bool FollowBehind { get; set; } = true;

        /// <summary>高度偏移（Y轴，负值=上方）</summary>
        public float HeightOffset { get; set; } = -20f;

        public PlayerSelector() { }

        public PlayerSelector(float followDistance, bool followBehind = true)
        {
            FollowDistance = followDistance;
            FollowBehind = followBehind;
        }

        public Vector2? SelectTarget(Projectile projectile, Player owner)
        {
            Vector2 pos = owner.Center;

            if (FollowBehind)
            {
                pos.X -= owner.direction * FollowDistance;
            }

            pos.Y += HeightOffset;

            return pos;
        }
    }
}