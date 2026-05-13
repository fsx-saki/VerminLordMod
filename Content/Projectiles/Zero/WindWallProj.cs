using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class WindWallProj : BaseBullet
    {
        private const int Duration = 240;
        private const float PushRange = 120f;
        private const float PushStrength = 4f;
        private int _timer;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new StationaryBehavior());

            Behaviors.Add(new AreaDamageBehavior
            {
                HitRadius = 70f,
                HitInterval = 12,
                Knockback = 5f,
                DirectionalKnockback = true,
            });

            Behaviors.Add(new FadeInOutBehavior
            {
                FadeInDuration = 0.1f,
                FadeOutStart = 0.85f,
                MinAlpha = 255,
                MaxAlpha = 30,
            });

            Behaviors.Add(new PeriodicDustBehavior
            {
                SpawnChance = 0.8f,
                DustType = DustID.Cloud,
                SpreadRadius = 35f,
                Speed = 1.5f,
                ScaleMin = 0.3f,
                ScaleMax = 0.7f,
                Color = new Color(180, 240, 220, 130),
                NoGravity = true,
                SuppressDefaultDraw = true,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            base.OnSpawned(source);
            _timer = 0;
        }

        protected override void OnAI()
        {
            _timer++;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.CanBeChasedBy() || npc.friendly)
                    continue;

                Vector2 toNpc = npc.Center - Projectile.Center;
                float dist = toNpc.Length();

                if (dist <= PushRange && dist > 10f)
                {
                    float pushForce = PushStrength * (1f - dist / PushRange);
                    Vector2 pushDir = toNpc.SafeNormalize(Vector2.Zero);
                    npc.velocity += pushDir * pushForce;
                }
            }

            Lighting.AddLight(Projectile.Center, 0.2f, 0.5f, 0.4f);
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 60);
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 12; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 5f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.Cloud, vel, 0,
                    new Color(160, 230, 210, 160),
                    Main.rand.NextFloat(0.5f, 1.0f));
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}
