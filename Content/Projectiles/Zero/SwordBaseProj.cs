using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class SwordBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 9f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.5f, 0.6f, 0.8f),
            });

            Behaviors.Add(new SwordTrailBehavior
            {
                SuppressDefaultDraw = true,
                EnableGhostTrail = true,
                GhostColor = new Color(180, 200, 255, 140),
                SwordGlowColor = new Color(200, 220, 255, 230),
                SwordQiColor = new Color(160, 200, 255, 180),
                SwordScarColor = new Color(140, 180, 255, 200),
            });
            Behaviors.Add(new MeleeSwingTrailBehavior
            {
                SuppressDefaultDraw = true,
                SwingArcColor = new Color(180, 200, 255, 200),
                StabImpactColor = new Color(200, 220, 255, 220),
                SmashRingColor = new Color(160, 190, 255, 180),
                SwingArcLength = 40f,
                SwingArcWidth = 0.35f,
                SwingArcCurlAmount = 2.5f,
                StabImpactStretch = 3f,
                SmashRingEndRadius = 45f,
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
