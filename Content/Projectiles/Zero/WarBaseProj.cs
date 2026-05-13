using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class WarBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 8f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.7f, 0.4f, 0.2f),
            });

            Behaviors.Add(new WarTrailBehavior
            {
                SuppressDefaultDraw = true,
                EnableGhostTrail = true,
                GhostColor = new Color(180, 120, 80, 120),
                GunSmokeColor = new Color(160, 140, 120, 200),
                ShrapnelColor = new Color(200, 180, 140, 230),
                WarFlameColor = new Color(255, 160, 60, 255),
            });
            Behaviors.Add(new MeleeSwingTrailBehavior
            {
                SuppressDefaultDraw = true,
                SwingArcColor = new Color(255, 180, 80, 200),
                StabImpactColor = new Color(220, 160, 100, 220),
                SmashRingColor = new Color(255, 140, 60, 180),
                SwingArcLength = 38f,
                SwingArcWidth = 0.4f,
                SwingArcCurlAmount = 4f,
                StabImpactSpread = 0.5f,
                SmashRingEndRadius = 50f,
                SmashRingCrackCount = 8,
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
