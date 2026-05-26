using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class XiaGuProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 10f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.6f, 0.1f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 120, 20),
                GlowLayers = 4,
                GlowBaseScale = 1.5f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.7f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.4f,
                EnableLight = true,
                LightColor = new Vector3(1.2f, 0.7f, 0.1f)
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                ColorStart = new Color(255, 140, 30, 255),
                ColorEnd = new Color(200, 40, 0, 0),
                MaxFragments = 45,
                FragmentLife = 18,
                SizeMultiplier = 0.8f,
                SpawnInterval = 1,
                AirResistance = 0.93f,
                Buoyancy = -0.05f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.25f,
                SplashAngle = 0.7f,
                RandomSpread = 1.2f,
                SuppressDefaultDraw = true
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.scale = 1.3f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.alpha = 5;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.knockBack = 5f;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 300);
            target.AddBuff(BuffID.OnFire, 600);
        }

        protected override bool OnTileCollided(Vector2 oldVelocity)
        {
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.active)
            {
                EntitySource_ItemUse_WithAmmo source = new EntitySource_ItemUse_WithAmmo(owner, owner.HeldItem, 0);
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi / 4f * i + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;
                    Projectile.NewProjectile(source, Projectile.Center, vel, Projectile.type, Projectile.damage / 2, Projectile.knockBack / 2f, Projectile.owner);
                }
            }
            return true;
        }
    }
}
