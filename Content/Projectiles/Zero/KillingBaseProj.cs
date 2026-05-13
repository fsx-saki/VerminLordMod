using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class KillingBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 10f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.6f, 0.2f, 0.2f),
            });

            Behaviors.Add(new KillingTrailBehavior
            {
                SuppressDefaultDraw = true,
                EnableGhostTrail = true,
                GhostColor = new Color(180, 60, 60, 120),
                BloodStreakColor = new Color(200, 40, 40, 230),
                KillingAuraColor = new Color(180, 30, 30, 200),
                DeathShadowColor = new Color(100, 20, 40, 180),
            });
            Behaviors.Add(new MeleeSwingTrailBehavior
            {
                SuppressDefaultDraw = true,
                SwingArcColor = new Color(200, 50, 50, 200),
                StabImpactColor = new Color(220, 40, 40, 220),
                SmashRingColor = new Color(180, 30, 30, 180),
                SwingArcLength = 35f,
                SwingArcWidth = 0.3f,
                SwingArcCurlAmount = 3.5f,
                StabImpactSpread = 0.3f,
                StabImpactStretch = 4f,
                SmashRingEndRadius = 42f,
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
