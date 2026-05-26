using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    public class YinLangProj : ModProjectile
    {
        private int _chaseTimer;
        private int _lifeTimer;
        private const int MaxLifeTime = 900;
        private const float ChaseSpeed = 8f;
        private const float ChaseRange = 600f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLifeTime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            _lifeTimer++;
            Player owner = Main.player[Projectile.owner];

            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            NPC target = null;
            float minDist = ChaseRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < minDist)
                {
                    minDist = dist;
                    target = npc;
                }
            }

            if (target != null)
            {
                Vector2 dir = target.Center - Projectile.Center;
                dir.Normalize();
                dir *= ChaseSpeed;
                Projectile.velocity = (Projectile.velocity * 10f + dir) / 11f;

                if (Math.Abs(dir.X) > 0.1f)
                    Projectile.spriteDirection = dir.X > 0 ? 1 : -1;
            }
            else
            {
                Vector2 toOwner = owner.Center - Projectile.Center;
                float ownerDist = toOwner.Length();

                if (ownerDist > 200f)
                {
                    toOwner.Normalize();
                    Projectile.velocity = (Projectile.velocity * 10f + toOwner * 4f) / 11f;
                }
                else
                {
                    Projectile.velocity *= 0.95f;
                }
            }

            Projectile.rotation = Projectile.velocity.X * 0.05f;

            _chaseTimer++;
            if (_chaseTimer % 8 == 0)
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Bone);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 6; i++)
            {
                var d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood);
                d.velocity *= 2f;
                d.noGravity = true;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool MinionContactDamage()
        {
            return true;
        }
    }
}
