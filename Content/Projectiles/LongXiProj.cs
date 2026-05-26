using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class LongXiProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 7f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.1f, 0.3f, 0.8f)
            });

            Behaviors.Add(new LiquidTrailBehavior
            {
                ColorStart = new Color(50, 120, 255, 255),
                ColorEnd = new Color(20, 60, 180, 0),
                MaxFragments = 40,
                FragmentLife = 20,
                SizeMultiplier = 0.8f,
                SpawnInterval = 2,
                AirResistance = 0.94f,
                Buoyancy = 0.02f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.2f,
                SplashAngle = 0.6f,
                RandomSpread = 1.0f,
                SuppressDefaultDraw = true
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(50, 120, 255),
                GlowLayers = 3,
                GlowBaseScale = 1.2f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = true,
                LightColor = new Vector3(0.2f, 0.5f, 1.0f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1.1f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 120;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Chilled, 180);

            Vector2 hitPos = target.Center;
            float pullRange = 200f;
            float pullStrength = 0.3f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy() || npc.whoAmI == target.whoAmI)
                    continue;

                float dist = Vector2.Distance(hitPos, npc.Center);
                if (dist <= pullRange && dist > 5f)
                {
                    Vector2 toHit = hitPos - npc.Center;
                    float force = pullStrength * (1f - dist / pullRange);
                    npc.velocity += toHit.SafeNormalize(Vector2.Zero) * force;
                }
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
