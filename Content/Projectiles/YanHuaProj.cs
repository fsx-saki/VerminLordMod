using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class YanHuaProj : BaseBullet
    {
        private bool _hasExploded = false;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 8f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1f, 0.8f, 0.2f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 200, 50),
                GlowLayers = 4,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.6f,
                GlowAlphaDecay = 0.12f,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(1.2f, 0.9f, 0.3f)
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.OnFire, 180)
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.alpha = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            if (Projectile.timeLeft <= 30 && !_hasExploded)
            {
                Explode();
            }

            if (Main.rand.NextBool(2))
            {
                Color[] fireworkColors = new Color[]
                {
                    Color.Red, Color.Orange, Color.Yellow,
                    Color.Lime, Color.Cyan, Color.Magenta, Color.Violet
                };
                Color c = fireworkColors[Main.rand.Next(fireworkColors.Length)];
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowRod, 0f, 0f, 0, c);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!_hasExploded)
            {
                Explode();
            }
        }

        private void Explode()
        {
            if (_hasExploded) return;
            _hasExploded = true;

            int subCount = 5;
            float baseAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
            int subDamage = (int)(Projectile.damage * 0.5f);

            for (int i = 0; i < subCount; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / subCount;
                Vector2 vel = angle.ToRotationVector2() * 5f;
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    vel,
                    ModContent.ProjectileType<YanHuaSubProj>(),
                    subDamage,
                    Projectile.knockBack * 0.5f,
                    Projectile.owner
                );
            }

            Color[] explosionColors = new Color[]
            {
                Color.Red, Color.Orange, Color.Yellow,
                Color.Lime, Color.Cyan
            };
            for (int i = 0; i < 30; i++)
            {
                Color c = explosionColors[i % explosionColors.Length];
                var d = Dust.NewDustDirect(Projectile.Center - new Vector2(8, 8), 16, 16, DustID.RainbowRod, 0f, 0f, 0, c);
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.0f, 1.8f);
            }

            Projectile.Kill();
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }

    public class YanHuaSubProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.scale = 0.8f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 40;
            Projectile.alpha = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Lighting.AddLight(Projectile.Center, 0.8f, 0.6f, 0.2f);

            if (Main.rand.NextBool(2))
            {
                Color[] colors = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Lime, Color.Cyan };
                Color c = colors[(int)Projectile.ai[0] % colors.Length];
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowRod, 0f, 0f, 0, c);
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 180);
        }

        public override void OnKill(int timeLeft)
        {
            Color[] colors = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Lime, Color.Cyan };
            for (int i = 0; i < 8; i++)
            {
                Color c = colors[i % colors.Length];
                var d = Dust.NewDustDirect(Projectile.Center - new Vector2(4, 4), 8, 8, DustID.RainbowRod, 0f, 0f, 0, c);
                d.velocity = Main.rand.NextVector2Circular(2f, 2f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.2f);
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[0] = Main.rand.Next(5);
        }
    }
}
