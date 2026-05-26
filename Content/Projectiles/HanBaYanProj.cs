using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class HanBaYanProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 8f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.3f, 0.05f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 80, 20),
                GlowLayers = 3,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.6f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(1.5f, 0.5f, 0.1f)
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                ColorStart = new Color(255, 150, 30, 255),
                ColorEnd = new Color(200, 30, 0, 0),
                MaxFragments = 50,
                FragmentLife = 15,
                SizeMultiplier = 0.6f,
                SpawnInterval = 1,
                AirResistance = 0.94f,
                Buoyancy = -0.02f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.2f,
                SplashAngle = 0.5f,
                RandomSpread = 0.8f,
                AutoDraw = true,
                SuppressDefaultDraw = true
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
            Projectile.timeLeft = 100;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 480);
            target.AddBuff(BuffID.OnFire3, 180);

            if (Main.rand.NextFloat() < 0.2f)
            {
                target.AddBuff(BuffID.CursedInferno, 120);
            }

            for (int i = 0; i < 12; i++)
            {
                var d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Torch);
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.5f);
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
