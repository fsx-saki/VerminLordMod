using Terraria;

namespace VerminLordMod.Common.GuBehaviors
{
    /// <summary>
    /// 命中效果提供者接口 — 武器实现此接口以定义命中时附加的道痕效果。
    /// 由 DaoEffectSystem 统一调用，将效果施加到目标NPC。
    /// </summary>
    public interface IOnHitEffectProvider
    {
        /// <summary>命中时附加的道痕效果标签数组</summary>
        DaoEffectTags[] OnHitEffects { get; }

        /// <summary>持续伤害持续时间（帧）</summary>
        float DoTDuration { get; }

        /// <summary>持续伤害每帧伤害量</summary>
        float DoTDamage { get; }

        /// <summary>减速百分比 [0, 1]</summary>
        float SlowPercent { get; }

        /// <summary>减速持续时间（帧）</summary>
        int SlowDuration { get; }

        /// <summary>碎甲量</summary>
        float ArmorShredAmount { get; }

        /// <summary>碎甲持续时间（帧）</summary>
        int ArmorShredDuration { get; }

        /// <summary>虚弱百分比 [0, 1]</summary>
        float WeakenPercent { get; }

        /// <summary>吸血百分比 [0, 1]</summary>
        float LifeStealPercent { get; }

        /// <summary>自定义命中NPC效果（用于特殊逻辑，如引爆标记）</summary>
        void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage);
    }
}
