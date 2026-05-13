using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// Flying道基础弹幕
    /// </summary>
    public class FlyingBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 10f) { AutoRotate = true, RotationOffset = MathHelper.PiOver2 });
            Behaviors.Add(new FlyingTrailBehavior
            {
                SuppressDefaultDraw = true,
                FeatherColor = new Color(220, 235, 255, 200),
                WindTailColor = new Color(180, 210, 255, 150),
                SpeedAfterColor = new Color(160, 200, 255, 120),
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
