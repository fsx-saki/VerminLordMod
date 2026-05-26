using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class CanZhuProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: 8f, trackingWeight: 1f / 10f)
            {
                Range = 1000f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.6f, 0.1f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 160, 50),
                GlowLayers = 3,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.6f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.5f, 0.05f)
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                ColorStart = new Color(255, 180, 60, 255),
                ColorEnd = new Color(200, 50, 0, 0),
                MaxFragments = 40,
                FragmentLife = 20,
                SizeMultiplier = 0.7f,
                SpawnInterval = 1,
                AirResistance = 0.93f,
                Buoyancy = -0.04f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.2f,
                SplashAngle = 0.6f,
                RandomSpread = 1.0f,
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
            Projectile.timeLeft = 300;
            Projectile.alpha = 5;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.knockBack = 2f;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 600);
            target.AddBuff(BuffID.CursedInferno, 300);
        }
    }
}
