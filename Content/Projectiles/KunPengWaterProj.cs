using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class KunPengWaterProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: 7f)
            {
                TrackingWeight = 1f / 8f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.2f, 0.4f, 0.8f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(60, 120, 220),
                GlowLayers = 3,
                GlowBaseScale = 1.4f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = true,
                LightColor = new Vector3(0.2f, 0.5f, 1.0f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Chilled, 180);

            for (int i = 0; i < 6; i++)
            {
                var d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Water, 0f, 0f, 0, default, 1.0f);
                d.noGravity = true;
                d.velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
            }
        }

        protected override void OnAI()
        {
            if (Main.rand.NextBool(3))
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Water, 0f, 0f, 0, default, 0.6f);
                d.noGravity = true;
                d.velocity *= 0.2f;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
