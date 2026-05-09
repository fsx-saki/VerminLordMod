using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;

namespace VerminLordMod.Common.GlobalProjectiles
{
    public class GuProjectileInfo : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public DaoEffectTags EffectsOnHit;
        public float DaoMultiplier = 1f;
        public DaoType SourceDao;
        public float DoTDamage;
        public float DoTDuration;
        public float SlowPercent;
        public int SlowDuration;
        public float ArmorShred;
        public int ArmorShredDuration;
        public float WeakenPercent;
        public float LifeStealPercent;

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[projectile.owner];

            // 1. 应用 DaoEffect（DoT/Slow/ArmorShred 等）
            if (EffectsOnHit != DaoEffectTags.None)
            {
                DaoEffectSystem.ApplyEffects(target, player, projectile,
                    EffectsOnHit, DoTDamage, DoTDuration, SlowPercent, SlowDuration,
                    ArmorShred, ArmorShredDuration, WeakenPercent, LifeStealPercent);
            }

            // 2. 检查战术触发（D-29）
            TacticalTriggerSystem.OnProjectileHit(projectile, target, player);
        }
    }
}
