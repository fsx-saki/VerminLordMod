using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// Rule道基础弹幕
    /// </summary>
    public class RuleBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 5f) { AutoRotate = true, RotationOffset = MathHelper.PiOver2 });
            Behaviors.Add(new RuleTrailBehavior
            {
                SuppressDefaultDraw = true,
                GridNodeColor = new Color(150, 200, 255, 200),
                RulerMarkColor = new Color(180, 210, 240, 180),
                OrderRingColor = new Color(120, 180, 255, 160),
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
