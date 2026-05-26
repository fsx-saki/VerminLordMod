using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class HunShouLingProj : BaseBullet
    {
        private const float FlySpeed = 8f;
        private const float TrackWeight = 1f / 12f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
            {
                Range = 700f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.6f, 0.2f, 0.9f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(160, 60, 220),
                GlowLayers = 3,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = true,
                LightColor = new Vector3(0.6f, 0.2f, 0.9f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 150;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.40f)
            {
                target.AddBuff(BuffID.Confused, 240);
            }
            if (Main.rand.NextFloat() < 0.20f)
            {
                target.AddBuff(BuffID.OnFire, 180);
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}
