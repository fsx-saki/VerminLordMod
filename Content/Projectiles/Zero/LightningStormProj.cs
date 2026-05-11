using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class LightningStormProj : BaseBullet
    {
        private const int StormLife = 180;
        private const int BoltInterval = 15;
        private const float BoltRadius = 200f;
        private const int BoltsPerWave = 3;

        private int _timer = 0;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new StationaryBehavior());

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(150, 180, 255, 100),
                GlowBaseScale = 2.5f,
                GlowLayers = 2,
                GlowAlphaMultiplier = 0.2f,
                EnableLight = true,
                LightColor = new Vector3(0.4f, 0.5f, 0.9f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = StormLife;
            Projectile.alpha = 128;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            _timer++;

            Lighting.AddLight(Projectile.Center, 0.3f, 0.4f, 0.8f);

            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + new Vector2(
                    Main.rand.NextFloat(-BoltRadius, BoltRadius),
                    Main.rand.NextFloat(-60f, 20f)
                );
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Electric, new Vector2(0, -Main.rand.NextFloat(1f, 3f)), 0, default, 0.6f);
                d.noGravity = true;
            }

            if (_timer % BoltInterval == 0)
            {
                for (int b = 0; b < BoltsPerWave; b++)
                {
                    float boltX = Projectile.Center.X + Main.rand.NextFloat(-BoltRadius, BoltRadius);
                    Vector2 boltStart = new Vector2(boltX, Projectile.Center.Y - 40f);
                    Vector2 boltEnd = new Vector2(boltX, Projectile.Center.Y + Main.rand.NextFloat(40f, 120f));

                    for (int j = 0; j < Main.maxNPCs; j++)
                    {
                        NPC npc = Main.npc[j];
                        if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                        {
                            float distX = MathHelper.Distance(npc.Center.X, boltX);
                            if (distX < 30f && npc.Center.Y > boltStart.Y && npc.Center.Y < boltEnd.Y + 100f)
                            {
                                Player owner = Main.player[Projectile.owner];
                                if (owner != null && owner.active)
                                {
                                    bool crit = Main.rand.Next(100) < Projectile.CritChance;
                                    npc.StrikeNPC(new NPC.HitInfo
                                    {
                                        Damage = Projectile.damage,
                                        Knockback = 2f,
                                        HitDirection = npc.Center.X > boltX ? 1 : -1,
                                        Crit = crit
                                    });
                                }
                                npc.AddBuff(BuffID.Electrified, 60);
                            }
                        }
                    }

                    for (int s = 0; s < 15; s++)
                    {
                        Vector2 sparkPos = Vector2.Lerp(boltStart, boltEnd, s / 15f)
                            + new Vector2(Main.rand.NextFloat(-8f, 8f), 0);
                        Dust d = Dust.NewDustPerfect(sparkPos, DustID.Electric, Vector2.Zero, 0, default, 0.7f);
                        d.noGravity = true;
                    }
                }
            }
        }
    }
}