using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class BingXiaoProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 9f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.6f, 1.0f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(100, 180, 255),
                GlowLayers = 3,
                GlowBaseScale = 1.2f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.6f, 1.0f)
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.Ice,
                SpawnChance = 3,
                DustScale = 0.8f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                RandomSpeed = 1.0f
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 80;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.30f)
            {
                target.AddBuff(BuffID.Frostburn, 240);
            }
            if (Main.rand.NextFloat() < 0.15f)
            {
                target.AddBuff(BuffID.Frozen, 60);
            }

            for (int i = 0; i < 6; i++)
            {
                var d = Dust.NewDustDirect(target.Center, 0, 0, DustID.Ice,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f),
                    80, new Color(100, 180, 255), 1.0f);
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
