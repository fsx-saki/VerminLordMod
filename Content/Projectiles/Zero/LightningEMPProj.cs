using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class LightningEMPProj : BaseBullet
    {
        private const float FlySpeed = 10f;
        private const int MaxLife = 120;
        private const float BlastRadius = 160f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.5f, 0.6f, 1.0f)
            });

            Behaviors.Add(new DustTrailBehavior(DustID.Electric, spawnChance: 1)
            {
                DustScale = 0.9f,
                VelocityMultiplier = 0.15f,
                NoGravity = true,
                DustAlpha = 200,
                RandomSpeed = 0.5f
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(150, 180, 255, 200),
                GlowBaseScale = 1.5f,
                GlowLayers = 3,
                GlowAlphaMultiplier = 0.4f,
                EnableLight = true,
                LightColor = new Vector3(0.5f, 0.6f, 1.0f)
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
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist <= BlastRadius)
                    {
                        Vector2 pushDir = (npc.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        float pushForce = 6f * (1f - dist / BlastRadius);
                        npc.velocity += pushDir * pushForce;

                        npc.AddBuff(BuffID.Electrified, 120);
                        npc.AddBuff(BuffID.Confused, 60);

                        Player owner = Main.player[Projectile.owner];
                        if (owner != null && owner.active)
                        {
                            bool crit = Main.rand.Next(100) < Projectile.CritChance;
                            npc.StrikeNPC(new NPC.HitInfo
                            {
                                Damage = Projectile.damage,
                                Knockback = pushForce,
                                HitDirection = pushDir.X > 0 ? 1 : -1,
                                Crit = crit
                            });
                        }
                    }
                }
            }

            for (int i = 0; i < 40; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(3f, 10f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.Electric,
                    vel,
                    0,
                    default,
                    Main.rand.NextFloat(0.5f, 1.2f)
                );
                d.noGravity = true;
            }

            for (int i = 0; i < 3; i++)
            {
                float ringRadius = BlastRadius * 0.3f;
                for (int j = 0; j < 20; j++)
                {
                    float angle = MathHelper.TwoPi * j / 20f;
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * ringRadius * (i + 1) / 3f;
                    Dust d = Dust.NewDustPerfect(pos, DustID.Electric, Vector2.Zero, 0, default, 0.5f);
                    d.noGravity = true;
                }
            }
        }
    }
}