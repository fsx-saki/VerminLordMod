using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Projectiles.Zero;

namespace VerminLordMod.Content.Projectiles
{
    public class HuoLeiProj : BaseBullet
    {
        private const float FlySpeed = 9f;
        private const int SparkCount = 3;
        private const float SparkSpeed = 5f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.2f, 0.8f, 0.2f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 140, 30),
                GlowLayers = 3,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.6f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(1.2f, 0.6f, 0.1f)
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 40,
                FragmentLife = 20,
                SizeMultiplier = 0.7f,
                SpawnInterval = 1,
                ColorStart = new Color(255, 180, 60, 255),
                ColorEnd = new Color(200, 50, 0, 0),
                Buoyancy = -0.04f,
                AirResistance = 0.93f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.2f,
                SplashAngle = 0.6f,
                RandomSpread = 1.0f,
                AutoDraw = true,
                SuppressDefaultDraw = true
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
            Projectile.timeLeft = 100;
            Projectile.alpha = 5;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.knockBack = 6f;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 300);

            if (Main.rand.NextFloat() < 0.40f)
            {
                target.AddBuff(BuffID.Electrified, 180);
            }

            for (int i = 0; i < SparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / SparkCount + Main.rand.NextFloat(-0.5f, 0.5f);
                float speed = SparkSpeed + Main.rand.NextFloat(-1f, 1f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    vel,
                    ModContent.ProjectileType<FireBaseProj>(),
                    (int)(Projectile.damage * 0.4f),
                    Projectile.knockBack * 0.3f,
                    Projectile.owner
                );
            }

            for (int i = 0; i < 15; i++)
            {
                var d = Dust.NewDustDirect(target.Center, 0, 0, Main.rand.NextBool() ? DustID.Torch : DustID.Electric);
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.5f);
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
