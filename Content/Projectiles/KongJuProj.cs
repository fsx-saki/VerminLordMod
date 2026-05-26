using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class KongJuProj : BaseBullet
    {
        private const float FlySpeed = 8f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.05f, 0.4f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(60, 10, 80),
                GlowLayers = 3,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.6f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(0.35f, 0.05f, 0.45f)
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.PurpleTorch,
                DustCount = 10,
                SpeedMin = 1f,
                SpeedMax = 4f,
                ScaleMin = 0.8f,
                ScaleMax = 1.5f,
                Color = Color.DarkOrchid
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 12,
                        DustType = DustID.PurpleTorch,
                        Color = new Color(80, 10, 120),
                        ScaleMin = 0.8f,
                        ScaleMax = 1.4f,
                        SpeedMin = 2f,
                        SpeedMax = 5f,
                        SpreadRadius = 10f
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1.1f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 80;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.knockBack = 3f;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.25f)
            {
                target.AddBuff(BuffID.Confused, 180);
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
