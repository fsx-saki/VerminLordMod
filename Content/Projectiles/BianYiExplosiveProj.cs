using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Projectiles
{
    public class BianYiExplosiveProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 8f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.5f, 0.2f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 120, 50),
                GlowLayers = 2,
                GlowBaseScale = 1.2f,
                GlowScaleIncrement = 0.3f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f
            });

            Behaviors.Add(new OnKillProjectileBurstBehavior
            {
                ProjectileType = ModContent.ProjectileType<BianYiSubProj>(),
                Count = 3,
                Speed = 5f,
                DamageMultiplier = 0.4f,
                KnockbackMultiplier = 0.5f,
                UseRandomVelocity = true,
                SpeedMin = 3f,
                SpreadRadius = 8f
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
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
                    -Projectile.velocity * 0.1f, 80, new Color(255, 120, 50), 0.6f);
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }

    public class BianYiSubProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 5f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.6f, 0.3f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.scale = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.alpha = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustPerfect(target.Center, ModContent.DustType<VariationDust>(),
                    Main.rand.NextVector2Circular(3f, 3f), 80, new Color(255, 120, 50), 0.7f);
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
