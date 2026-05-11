using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class LightningChainProj : BaseBullet
    {
        private const float FlySpeed = 14f;
        private const float TrackWeight = 1f / 8f;
        private const int MaxLife = 120;
        private const int ChainRange = 350;
        private const int MaxChains = 4;

        private int _chainCount = 0;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
            {
                Range = 600f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });

            Behaviors.Add(new DustTrailBehavior(DustID.Electric, spawnChance: 1)
            {
                DustScale = 0.7f,
                VelocityMultiplier = 0.12f,
                NoGravity = true,
                DustAlpha = 200,
                RandomSpeed = 0.4f
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(200, 220, 255, 200),
                GlowBaseScale = 1.3f,
                GlowLayers = 2,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(0.6f, 0.7f, 1.0f)
            });

            Behaviors.Add(new SplashBehavior(SplashMode.Cone)
            {
                Count = 6,
                SpeedMin = 3f,
                SpeedMax = 8f,
                SpreadRadius = 3f,
                ConeAngle = 0.35f,
                SpawnExtraDust = true,
                ExtraDustCount = 10,
                DustType = DustID.Electric,
                DustColorStart = new Color(180, 200, 255, 220),
                DustColorEnd = new Color(100, 150, 255, 0),
                DustScaleMin = 0.4f,
                DustScaleMax = 0.8f,
                DustSpeedMin = 2f,
                DustSpeedMax = 5f,
                DustNoGravity = true,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 99;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (_chainCount >= MaxChains)
                return;

            NPC nearest = null;
            float nearestDist = ChainRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly && npc.whoAmI != target.whoAmI)
                {
                    float dist = Vector2.Distance(target.Center, npc.Center);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = npc;
                    }
                }
            }

            if (nearest != null)
            {
                Vector2 chainVel = (nearest.Center - target.Center).SafeNormalize(Vector2.Zero) * FlySpeed;

                int chainProj = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    chainVel,
                    Projectile.type,
                    (int)(Projectile.damage * 0.7f),
                    Projectile.knockBack * 0.7f,
                    Projectile.owner
                );

                if (Main.projectile[chainProj].ModProjectile is LightningChainProj chain)
                {
                    chain._chainCount = _chainCount + 1;
                }

                for (int j = 0; j < 8; j++)
                {
                    Vector2 arcPos = Vector2.Lerp(target.Center, nearest.Center, j / 8f);
                    Dust d = Dust.NewDustPerfect(arcPos, DustID.Electric, Vector2.Zero, 0, default, 0.8f);
                    d.noGravity = true;
                }
            }
        }
    }
}