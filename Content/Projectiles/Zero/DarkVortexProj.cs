using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class DarkVortexProj : BaseBullet
    {
        private const int VortexLife = 150;
        private const float PullRange = 250f;
        private const float PullStrength = 0.15f;
        private const int DamageInterval = 20;

        private int _timer = 0;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new StationaryBehavior());

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(60, 10, 100, 80),
                GlowBaseScale = 3.0f,
                GlowLayers = 2,
                GlowAlphaMultiplier = 0.15f,
                EnableLight = true,
                LightColor = new Vector3(0.2f, 0.02f, 0.4f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = VortexLife;
            Projectile.alpha = 150;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            _timer++;

            Lighting.AddLight(Projectile.Center, 0.15f, 0.02f, 0.3f);

            float pulse = 1f + (float)System.Math.Sin(_timer * 0.1f) * 0.2f;
            Projectile.scale = pulse;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist <= PullRange && dist > 20f)
                    {
                        Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                        float strength = PullStrength * (1f - dist / PullRange);
                        npc.velocity += pullDir * strength;

                        if (_timer % DamageInterval == 0)
                        {
                            Player owner = Main.player[Projectile.owner];
                            if (owner != null && owner.active)
                            {
                                bool crit = Main.rand.Next(100) < Projectile.CritChance;
                                npc.StrikeNPC(new NPC.HitInfo
                                {
                                    Damage = Projectile.damage,
                                    Knockback = 0.5f,
                                    HitDirection = pullDir.X > 0 ? 1 : -1,
                                    Crit = crit
                                });
                            }
                            npc.AddBuff(BuffID.ShadowFlame, 60);
                        }
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Main.rand.NextFloat(20f, PullRange * 0.5f);
                Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * radius;
                Vector2 dustVel = (Projectile.Center - dustPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f);

                Dust d = Dust.NewDustPerfect(dustPos, DustID.Shadowflame, dustVel, 0, default, 0.5f);
                d.noGravity = true;
            }
        }
    }
}