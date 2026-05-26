using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Projectiles
{
    public class BianYiHomingProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: 7f, trackingWeight: 1f / 8f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                Range = 600f,
                EnableLight = true,
                LightColor = new Vector3(0.6f, 0.3f, 1.0f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(150, 80, 255),
                GlowLayers = 2,
                GlowBaseScale = 1.1f,
                GlowScaleIncrement = 0.3f,
                GlowBaseAlpha = 0.4f,
                GlowAlphaDecay = 0.12f,
                GlowAlphaMultiplier = 0.25f
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
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            if (Main.rand.NextBool(4))
            {
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<VariationDust>(),
                    -Projectile.velocity * 0.1f, 80, default, 0.5f);
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
