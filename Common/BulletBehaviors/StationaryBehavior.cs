using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 固定位置行为 — 弹幕生成后不移动，固定在地面位置。
    /// 适用于火焰墙、陷阱、光环等滞留型弹幕。
    /// 
    /// 使用方式：
    ///   Behaviors.Add(new StationaryBehavior());
    /// </summary>
    public class StationaryBehavior : IBulletBehavior
    {
        public string Name => "Stationary";

        /// <summary>是否在生成时锁定位置（true=锁定，false=仅不移动但可被推动）</summary>
        public bool LockPosition { get; set; } = true;

        /// <summary>锁定后的位置偏移</summary>
        public Vector2 PositionOffset { get; set; } = Vector2.Zero;

        /// <summary>是否禁用 tileCollide（true=不碰撞物块）</summary>
        public bool DisableTileCollide { get; set; } = true;

        // 锁定的初始位置
        private Vector2 _lockedPosition;

        public StationaryBehavior() { }

        public StationaryBehavior(bool lockPosition)
        {
            LockPosition = lockPosition;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _lockedPosition = projectile.Center;

            if (DisableTileCollide)
            {
                projectile.tileCollide = false;
            }
        }

        public void Update(Projectile projectile)
        {
            if (LockPosition)
            {
                // 每帧强制回到锁定位置
                projectile.Center = _lockedPosition + PositionOffset;
                projectile.velocity = Vector2.Zero;
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
