using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using Terraria.ID;

namespace VerminLordMod.Content.Projectiles
{
    public class DaDaoProj : BaseBullet
    {
        private const float FlySpeed = 10f;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1.3f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 99;
            Projectile.timeLeft = 180;
            Projectile.alpha = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.9f, 0.4f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 220, 80),
                GlowLayers = 5,
                GlowBaseScale = 1.4f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.7f,
                GlowAlphaDecay = 0.12f,
                GlowAlphaMultiplier = 0.4f,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.9f, 0.4f)
            });
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.defense > 0)
            {
                target.defense -= 10;
                if (target.defense < 0)
                    target.defense = 0;
            }

            for (int i = 0; i < 8; i++)
            {
                var d = Dust.NewDustDirect(target.position, target.width, target.height, Terraria.ID.DustID.GoldFlame);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.0f, 1.8f);
                d.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
            }
        }
    }
}
