using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class JiuZhuanHuoProj : BaseBullet
    {
        private const float FlySpeed = 9f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.3f, 0.05f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 80, 20),
                GlowLayers = 4,
                GlowBaseScale = 1.4f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.7f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.4f,
                EnableLight = true,
                LightColor = new Vector3(1.2f, 0.4f, 0.05f)
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                ColorStart = new Color(255, 120, 20, 255),
                ColorEnd = new Color(180, 30, 0, 0),
                MaxFragments = 50,
                FragmentLife = 18,
                SizeMultiplier = 0.8f,
                SpawnInterval = 1,
                AirResistance = 0.92f,
                Buoyancy = -0.06f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.25f,
                SplashAngle = 0.7f,
                RandomSpread = 1.2f,
                SuppressDefaultDraw = true
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 90;
            Projectile.alpha = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 360);

            if (Main.rand.NextFloat() < 0.25f)
            {
                target.AddBuff(BuffID.CursedInferno, 180);
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
