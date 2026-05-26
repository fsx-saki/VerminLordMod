using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Projectiles
{
    public class BianYiPierceProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 12f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.8f, 0.4f, 1.0f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(200, 100, 255),
                GlowLayers = 2,
                GlowBaseScale = 1.0f,
                GlowScaleIncrement = 0.3f,
                GlowBaseAlpha = 0.4f,
                GlowAlphaDecay = 0.12f,
                GlowAlphaMultiplier = 0.25f
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 180;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            if (Main.rand.NextBool(3))
            {
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<VariationDust>(),
                    -Projectile.velocity * 0.15f, 80, default, 0.5f);
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
