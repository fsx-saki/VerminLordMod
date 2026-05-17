using Terraria;
using Microsoft.Xna.Framework;

namespace VerminLordMod.Common.GuBehaviors
{
    /// <summary>
    /// 弹道运动学提供者接口 — 武器实现此接口以自定义弹幕的发射参数和逻辑。
    /// 包括弹幕类型、速度、数量、散射角度，以及可选的完全自定义发射逻辑。
    /// </summary>
    public interface IKinematicProvider
    {
        /// <summary>发射的弹幕类型ID</summary>
        int ProjectileType { get; }

        /// <summary>弹幕发射速度</summary>
        float ShootSpeed { get; }

        /// <summary>每次发射的弹幕数量</summary>
        int ShootCount { get; }

        /// <summary>散射角度（弧度）</summary>
        float SpreadAngle { get; }

        /// <summary>弹幕创建后修改其属性（如追踪、穿透等）</summary>
        void ModifyProjectile(Projectile projectile, Player player);

        /// <summary>是否完全覆盖默认发射逻辑</summary>
        bool OverrideShootLogic { get; }

        /// <summary>自定义发射逻辑（当 OverrideShootLogic 为 true 时调用）</summary>
        void CustomShoot(Player player, Vector2 position, Vector2 velocity, int damage, float knockback);
    }
}
