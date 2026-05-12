using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 接触销毁行为 — 弹幕碰到物块或敌人时立即销毁，触发 OnKill 效果。
    ///
    /// 适用于"碰到任何东西就爆炸"类型的弹幕（如冰爆术）。
    /// 爆炸逻辑应放在弹幕的 OnKilled 中，此行为只负责触发销毁。
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new KillOnContactBehavior());
    /// </code>
    /// </summary>
    public class KillOnContactBehavior : IBulletBehavior
    {
        public string Name => "KillOnContact";

        /// <summary>是否在碰撞物块时销毁弹幕，默认 true</summary>
        public bool KillOnTileCollide { get; set; } = true;

        /// <summary>是否在命中 NPC 时销毁弹幕，默认 true</summary>
        public bool KillOnHitNPC { get; set; } = true;

        public KillOnContactBehavior() { }

        /// <summary>
        /// 快速构造，可指定是否启用物块/敌人销毁。
        /// </summary>
        public KillOnContactBehavior(bool killOnTileCollide = true, bool killOnHitNPC = true)
        {
            KillOnTileCollide = killOnTileCollide;
            KillOnHitNPC = killOnHitNPC;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (KillOnHitNPC)
            {
                projectile.Kill();
            }
        }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        /// <summary>
        /// 碰撞物块时返回 true 销毁弹幕，触发 OnKill → OnKilled。
        /// </summary>
        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            return KillOnTileCollide ? true : null;
        }
    }
}
