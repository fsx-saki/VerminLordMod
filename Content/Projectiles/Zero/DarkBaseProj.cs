using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// Dark道基础弹幕
    /// </summary>
    public class DarkBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: 10f, trackingWeight: 1f / 11f)
            {
                Range = 8000f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }
    }
}
