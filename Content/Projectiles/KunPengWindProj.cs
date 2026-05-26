using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class KunPengWindProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 12f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.6f, 0.8f, 0.5f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(180, 230, 160),
                GlowLayers = 3,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = true,
                LightColor = new Vector3(0.5f, 0.8f, 0.4f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 knockbackDir = Vector2.Normalize(target.Center - Projectile.Center);
            if (knockbackDir != Vector2.Zero)
            {
                target.velocity += knockbackDir * 5f;
            }

            for (int i = 0; i < 5; i++)
            {
                var d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Cloud, 0f, 0f, 0, default, 0.8f);
                d.noGravity = true;
                d.velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
            }
        }

        protected override void OnAI()
        {
            if (Main.rand.NextBool(2))
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Cloud, 0f, 0f, 0, default, 0.5f);
                d.noGravity = true;
                d.velocity *= 0.2f;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
