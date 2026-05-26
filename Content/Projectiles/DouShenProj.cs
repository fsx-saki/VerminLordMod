using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    public class DouShenProj : ModProjectile
    {
        private const float ChaseSpeed = 6f;
        private const float ChaseRange = 500f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 20;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 900;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

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
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * ChaseSpeed, 0.1f);
            }
            else
            {
                Vector2 toOwner = owner.Center - Projectile.Center;
                float distToOwner = toOwner.Length();
                if (distToOwner > 200f)
                {
                    toOwner.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOwner * 4f, 0.05f);
                }
                else
                {
                    Projectile.velocity *= 0.95f;
                }
            }

            Projectile.rotation = Projectile.velocity.X > 0 ? 0 : MathHelper.Pi;
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Grass);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X * 0.7f;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y * 0.5f;
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5; i++)
            {
                var d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Grass);
                d.noGravity = true;
                d.velocity *= 0.5f;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(180, 230, 140, 200);
        }
    }
}
