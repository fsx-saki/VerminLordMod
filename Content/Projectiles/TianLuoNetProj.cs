using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    public class TianLuoNetProj : ModProjectile
    {
        private const int NetRadius = 400;
        private const float SlowPercent = 0.30f;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.damage = 0;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly)
                        continue;

                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist <= NetRadius)
                    {
                        npc.velocity *= (1f - SlowPercent * 0.05f);
                        npc.GetGlobalNPC<TianLuoNPC>().IsTrapped = true;
                    }
                }

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (!proj.active || proj.friendly)
                        continue;

                    float dist = Vector2.Distance(Projectile.Center, proj.Center);
                    if (dist <= NetRadius)
                    {
                        proj.Kill();
                    }
                }
            }

            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat() * NetRadius * 0.8f;
                Vector2 dustPos = Projectile.Center + new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
                var d = Dust.NewDustDirect(dustPos, 0, 0, 176);
                d.velocity *= 0.1f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.2f);
                d.alpha = 80;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float pulse = 0.8f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.05f);
            float drawRadius = NetRadius * pulse;

            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi / 36f * i;
                Vector2 edge = Projectile.Center + new Vector2((float)Math.Cos(angle) * drawRadius, (float)Math.Sin(angle) * drawRadius);
                var d = Dust.NewDustDirect(edge, 0, 0, 176);
                d.noGravity = true;
                d.scale = 0.8f;
                d.alpha = 100;
                d.velocity = Vector2.Zero;
            }

            for (int ring = 1; ring <= 3; ring++)
            {
                float ringRadius = drawRadius * ring / 3f;
                for (int i = 0; i < 24; i++)
                {
                    float angle = MathHelper.TwoPi / 24f * i + Main.GameUpdateCount * 0.01f * ring;
                    Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle) * ringRadius, (float)Math.Sin(angle) * ringRadius);
                    var d = Dust.NewDustDirect(pos, 0, 0, 176);
                    d.noGravity = true;
                    d.scale = 0.5f;
                    d.alpha = 60;
                    d.velocity = Vector2.Zero;
                }
            }

            return false;
        }
    }

    public class TianLuoNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool IsTrapped { get; set; }

        public override void ResetEffects(NPC npc)
        {
            IsTrapped = false;
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (IsTrapped)
            {
                drawColor = Color.Lerp(drawColor, Color.Cyan, 0.3f);
                npc.alpha = 0;
            }
        }
    }
}
