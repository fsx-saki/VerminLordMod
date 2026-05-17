using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class OnKillProjectileBurstBehavior : IBulletBehavior
    {
        public string Name => "OnKillProjectileBurst";

        public int ProjectileType { get; set; } = -1;

        public int Count { get; set; } = 8;

        public float Speed { get; set; } = 5f;

        public float SpeedMin { get; set; } = 2f;

        public float DamageMultiplier { get; set; } = 0.5f;

        public float KnockbackMultiplier { get; set; } = 0.5f;

        public bool UseRandomVelocity { get; set; } = false;

        public float SpreadRadius { get; set; } = 15f;

        public OnKillProjectileBurstBehavior() { }

        public OnKillProjectileBurstBehavior(int projectileType, int count, float speed, float damageMultiplier = 0.5f)
        {
            ProjectileType = projectileType;
            Count = count;
            Speed = speed;
            DamageMultiplier = damageMultiplier;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            if (ProjectileType <= 0) return;

            for (int i = 0; i < Count; i++)
            {
                Vector2 vel;
                if (UseRandomVelocity)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float speed = Main.rand.NextFloat(SpeedMin, Speed);
                    vel = angle.ToRotationVector2() * speed;
                }
                else
                {
                    float angle = MathHelper.TwoPi * i / Count;
                    vel = angle.ToRotationVector2() * Speed;
                }

                Vector2 spawnPos = projectile.Center + Main.rand.NextVector2Circular(SpreadRadius, SpreadRadius);

                Projectile.NewProjectile(
                    projectile.GetSource_FromThis(),
                    spawnPos,
                    vel,
                    ProjectileType,
                    (int)(projectile.damage * DamageMultiplier),
                    projectile.knockBack * KnockbackMultiplier,
                    projectile.owner
                );
            }
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}