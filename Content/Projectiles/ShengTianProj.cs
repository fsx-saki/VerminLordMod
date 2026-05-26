using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class ShengTianProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 12f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.9f, 0.3f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 220, 80),
                GlowLayers = 4,
                GlowBaseScale = 1.5f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.6f,
                GlowAlphaDecay = 0.12f,
                GlowAlphaMultiplier = 0.4f,
                EnableLight = true,
                LightColor = new Vector3(1.2f, 1.0f, 0.4f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 10;
            Projectile.timeLeft = 120;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.3f)
            {
                target.AddBuff(BuffID.Ichor, 300);
            }

            for (int i = 0; i < 8; i++)
            {
                var d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.GoldFlame, 0f, 0f, 0, default, 1.2f);
                d.noGravity = true;
                d.velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, 0f));
            }
        }

        protected override void OnAI()
        {
            if (Main.rand.NextBool(3))
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 0, default, 0.8f);
                d.noGravity = true;
                d.velocity *= 0.2f;
            }
        }
    }
}
