using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    public class LiuShaHeProj : ModProjectile
    {
        private const float ZoneRadius = 200f;
        private const float SlowMultiplier = 0.6f;
        private const int DamagePerSecond = 5;

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 40;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(ZoneRadius * 0.3f, ZoneRadius);
                var pos = Projectile.Center + new Vector2((float)Math.Cos(angle) * dist, (float)Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.Sandstorm);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.velocity += new Vector2(0f, -0.5f);
                d.scale = Main.rand.NextFloat(0.8f, 1.5f);
            }

            if (Projectile.timeLeft % 60 == 0)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly)
                        continue;

                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist <= ZoneRadius)
                    {
                        npc.SimpleStrikeNPC(DamagePerSecond, 0, false, 0f, DamageClass.Default);

                        for (int j = 0; j < 3; j++)
                        {
                            var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Sandstorm);
                            d.noGravity = true;
                            d.velocity *= 0.3f;
                            d.scale = Main.rand.NextFloat(0.8f, 1.2f);
                        }
                    }
                }
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist <= ZoneRadius)
                {
                    npc.velocity *= SlowMultiplier;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(ZoneRadius * 0.5f, ZoneRadius);
                var pos = Projectile.Center + new Vector2((float)Math.Cos(angle) * dist, (float)Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.Sandstorm);
                d.noGravity = true;
                d.velocity *= 0.5f;
                d.scale = Main.rand.NextFloat(1.0f, 1.8f);
            }
        }
    }
}
