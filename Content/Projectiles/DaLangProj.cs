using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class DaLangProj : BaseBullet
    {
        private const float FlySpeed = 8f;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 70;
            Projectile.alpha = 20;
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
                LightColor = new Vector3(0.1f, 0.3f, 0.8f)
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 20,
                FragmentLife = 18,
                SizeMultiplier = 0.5f,
                SpawnInterval = 2,
                AdaptiveTargetLength = 60f,
                SpeedLifeExponent = 0.3f,
                MinFragmentLife = 5,
                ColorStart = new Color(30, 100, 220, 255),
                ColorEnd = new Color(10, 50, 150, 0),
                Buoyancy = 0.01f,
                AirResistance = 0.95f,
                InertiaFactor = 0.25f,
                SplashFactor = 0.1f,
                SplashAngle = 0.3f,
                RandomSpread = 0.4f,
                AutoDraw = true,
                SuppressDefaultDraw = false
            });
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.velocity += (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 5f;

            if (Main.rand.NextFloat() < 0.20f)
            {
                target.AddBuff(BuffID.Chilled, 120);
            }

            for (int i = 0; i < 10; i++)
            {
                var d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Water);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.0f, 1.5f);
                d.velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
