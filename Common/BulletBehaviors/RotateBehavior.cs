using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 恒定旋转行为 — 弹幕每帧以固定角速度旋转。
    /// 适用于 MeteorProj、CycloneProj 等有自转效果的弹幕。
    /// </summary>
    public class RotateBehavior : IBulletBehavior
    {
        public string Name => "Rotate";

        /// <summary>旋转速度（弧度/帧），正=顺时针</summary>
        public float RotationSpeed { get; set; } = 0.5f;

        /// <summary>是否覆盖 AimBehavior/HomingBehavior 的自动旋转</summary>
        public bool OverrideAutoRotate { get; set; } = true;

        public RotateBehavior() { }

        public RotateBehavior(float rotationSpeed)
        {
            RotationSpeed = rotationSpeed;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile)
        {
            projectile.rotation += RotationSpeed;
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
