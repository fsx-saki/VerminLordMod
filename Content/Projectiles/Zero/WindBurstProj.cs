using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class WindBurstProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 8f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.7f, 0.5f),
            });

            Behaviors.Add(new WindTrailBehavior
            {
                EnableGhostTrail = true,
                GhostAlpha = 0.25f,
                GhostMaxPositions = 8,
                MaxStreaks = 30,
                StreakLife = 12,
                StreakSize = 0.4f,
                StreakStretch = 2.0f,
                MaxVortex = 15,
                VortexLife = 20,
                VortexSize = 0.3f,
                MaxMist = 5,
                MistLife = 25,
                MistSpawnChance = 0.06f,
                AutoDraw = true,
                SuppressDefaultDraw = true,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        protected override void OnKilled(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_Death(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<WindBurstExplosionProj>(),
                    (int)(Projectile.damage * 1.5f),
                    8f,
                    Projectile.owner
                );
            }
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 60);
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }

    public class WindBurstExplosionProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new SpiralExpandBehavior
            {
                ChargeFrames = 20,
                ExpandFrames = 18,
                FadeFrames = 12,
                ExpandSpeed = 7f,
                SpinSpeed = 0.18f,
                ArmCount = 5,
                HitRadius = 130f,
                HitInterval = 6,
                Knockback = 10f,
                DustType = DustID.Cloud,
                DustColor = new Color(180, 245, 225, 200),
                DustCountPerArm = 4,
                EnableLight = true,
                LightColor = new Vector3(0.4f, 0.8f, 0.6f),
                SuppressDefaultDraw = true,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 120);
        }

        protected override void OnKilled(int timeLeft) { }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}
