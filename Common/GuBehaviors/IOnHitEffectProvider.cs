using Terraria;

namespace VerminLordMod.Common.GuBehaviors
{
    public interface IOnHitEffectProvider
    {
        DaoEffectTags[] OnHitEffects { get; }
        float DoTDuration { get; }
        float DoTDamage { get; }
        float SlowPercent { get; }
        int SlowDuration { get; }
        float ArmorShredAmount { get; }
        int ArmorShredDuration { get; }
        float WeakenPercent { get; }
        float LifeStealPercent { get; }
        void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage);
    }
}
