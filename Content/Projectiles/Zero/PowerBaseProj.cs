using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// Power道基础弹幕
    /// </summary>
    public class PowerBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 7f) { AutoRotate = true, RotationOffset = MathHelper.PiOver2 });
            Behaviors.Add(new PowerTrailBehavior
            {
                SuppressDefaultDraw = true,
                ShockWaveColor = new Color(255, 200, 80, 200),
                AuraColor = new Color(255, 180, 60, 220),
                BurstLineColor = new Color(255, 160, 50, 200),
            });
            Behaviors.Add(new MeleeSwingTrailBehavior
            {
                SuppressDefaultDraw = true,
                SwingArcColor = new Color(255, 220, 100, 200),
                StabImpactColor = new Color(255, 200, 80, 220),
                SmashRingColor = new Color(255, 180, 60, 180),
                SwingArcLength = 42f,
                SwingArcWidth = 0.45f,
                SwingArcCurlAmount = 2f,
                StabImpactSize = 0.7f,
                SmashRingEndRadius = 55f,
                SmashRingWidth = 0.5f,
                SmashRingExpandSpeed = 1.5f,
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
