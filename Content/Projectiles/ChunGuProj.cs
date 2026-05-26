using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class ChunGuProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: 9f, trackingWeight: 1f / 15f)
            {
                Range = 800f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.2f, 0.8f, 0.1f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(80, 220, 40),
                GlowLayers = 3,
                GlowBaseScale = 1.4f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.6f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.4f,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.9f, 0.2f)
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                ColorStart = new Color(80, 220, 40, 255),
                ColorEnd = new Color(20, 100, 10, 0),
                MaxFragments = 40,
                FragmentLife = 20,
                SizeMultiplier = 0.7f,
                SpawnInterval = 2,
                AirResistance = 0.94f,
                Buoyancy = 0.06f,
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
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.alpha = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.knockBack = 2f;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.active && !owner.dead)
            {
                int healAmount = (int)(damageDone * 0.15f);
                if (healAmount > 0)
                {
                    owner.Heal(healAmount);
                }
            }

            if (Main.rand.NextFloat() < 0.3f)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player.active && !player.dead && Vector2.Distance(player.Center, target.Center) < 400f)
                    {
                        player.AddBuff(BuffID.Regeneration, 300);
                    }
                }
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
