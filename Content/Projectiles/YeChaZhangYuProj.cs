using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class YeChaZhangYuProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 7f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.05f, 0.2f, 0.35f)
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                ColorStart = new Color(20, 80, 120, 255),
                ColorEnd = new Color(5, 30, 60, 0),
                MaxFragments = 40,
                FragmentLife = 25,
                SizeMultiplier = 0.7f,
                SpawnInterval = 2,
                AirResistance = 0.94f,
                Buoyancy = 0.03f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.2f,
                SplashAngle = 0.6f,
                RandomSpread = 0.8f,
                SuppressDefaultDraw = true
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(20, 100, 160),
                GlowLayers = 3,
                GlowBaseScale = 1.2f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = true,
                LightColor = new Vector3(0.1f, 0.3f, 0.5f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 90;
            Projectile.alpha = 15;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.3f)
            {
                target.AddBuff(BuffID.Chilled, 180);
            }

            target.position += target.DirectionFrom(Projectile.Center) * 3f;

            for (int i = 0; i < 12; i++)
            {
                var d = Dust.NewDustDirect(target.Center - new Vector2(8, 8), 16, 16, DustID.Water, 0f, 0f, 0, new Color(20, 80, 140));
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.4f);
            }
        }

        protected override void OnAI()
        {
            if (Main.rand.NextBool(3))
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Water, 0f, 0f, 0, new Color(20, 80, 140));
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
