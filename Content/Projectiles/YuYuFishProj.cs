using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    public class YuYuFishProj : ModProjectile
    {
        private float TargetFoundTimer;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.damage = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 60;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            NPC target = FindTarget(owner);
            if (target != null)
            {
                DashTowardTarget(target);
            }
            else
            {
                FollowPlayer(owner);
            }

            SpawnWaterDust();

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            TargetFoundTimer++;
        }

        private NPC FindTarget(Player owner)
        {
            NPC closest = null;
            float minDist = 300f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy())
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        private void DashTowardTarget(NPC target)
        {
            Vector2 toTarget = target.Center - Projectile.Center;
            float dist = toTarget.Length();
            if (dist > 0f)
            {
                Vector2 dir = toTarget / dist;
                float dashSpeed = 8f;
                Projectile.velocity = (Projectile.velocity * 10f + dir * dashSpeed) / 11f;
            }
        }

        private void FollowPlayer(Player owner)
        {
            Vector2 toOwner = owner.Center - Projectile.Center;
            float dist = toOwner.Length();
            if (dist > 200f)
            {
                Vector2 dir = toOwner / dist;
                Projectile.velocity = (Projectile.velocity * 15f + dir * 6f) / 16f;
            }
            else if (dist > 50f)
            {
                Vector2 dir = toOwner / dist;
                Projectile.velocity = (Projectile.velocity * 20f + dir * 3f) / 21f;
            }
            else
            {
                Projectile.velocity *= 0.95f;
            }
        }

        private void SpawnWaterDust()
        {
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(
                    Projectile.position - Projectile.velocity * 2f,
                    Projectile.width, Projectile.height,
                    DustID.Water, 0f, 0f, 100, default, 1.2f
                );
                d.noGravity = true;
                d.velocity = -Projectile.velocity * 0.3f + new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Wet, 300);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(100, 180, 255, 200);
        }
    }
}
