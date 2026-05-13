using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// Eating道基础弹幕
    /// </summary>
    public class EatingBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 5f) { AutoRotate = true, RotationOffset = MathHelper.PiOver2 });
            Behaviors.Add(new DevourTrailBehavior
            {
                SuppressDefaultDraw = true,
                VortexMawColor = new Color(180, 80, 200, 220),
                AcidDropColor = new Color(100, 220, 80, 200),
                AbsorbTendrilColor = new Color(200, 120, 220, 180),
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
