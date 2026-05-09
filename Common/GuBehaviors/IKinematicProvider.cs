using Terraria;
using Microsoft.Xna.Framework;

namespace VerminLordMod.Common.GuBehaviors
{
    public interface IKinematicProvider
    {
        int ProjectileType { get; }
        float ShootSpeed { get; }
        int ShootCount { get; }
        float SpreadAngle { get; }
        void ModifyProjectile(Projectile projectile, Player player);
        bool OverrideShootLogic { get; }
        void CustomShoot(Player player, Vector2 position, Vector2 velocity, int damage, float knockback);
    }
}
