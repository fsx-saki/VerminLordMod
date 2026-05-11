using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class LightningBoltProj : BaseBullet
    {
        private const float FallSpeed = 0.3f;
        private const float MaxFallSpeed = 20f;
        private const float BlastRadius = 120f;
        private const int MaxLife = 120;

        private float _currentSpeed = 0f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 255, 200, 220),
                GlowBaseScale = 2.0f,
                GlowLayers = 4,
                GlowAlphaMultiplier = 0.5f,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 1.0f, 0.6f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 40;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            _currentSpeed = MathHelper.Min(_currentSpeed + FallSpeed, MaxFallSpeed);
            Projectile.velocity.Y = _currentSpeed;

            Projectile.rotation = MathHelper.PiOver2;

            Lighting.AddLight(Projectile.Center, 0.8f, 0.8f, 0.4f);

            for (int i = 0; i < 3; i++)
            {
                Vector2 dustPos = Projectile.Center + new Vector2(
                    Main.rand.NextFloat(-8f, 8f),
                    Main.rand.NextFloat(-20f, 0f)
                );
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Electric, Vector2.Zero, 0, default, 0.8f);
                d.noGravity = true;
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    if (Projectile.getRect().Intersects(npc.getRect()))
                    {
                        Player owner = Main.player[Projectile.owner];
                        if (owner != null && owner.active)
                        {
                            bool crit = Main.rand.Next(100) < Projectile.CritChance;
                            npc.StrikeNPC(new NPC.HitInfo
                            {
                                Damage = Projectile.damage,
                                Knockback = 8f,
                                HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1,
                                Crit = crit
                            });
                        }
                        npc.AddBuff(BuffID.Electrified, 180);
                        npc.AddBuff(BuffID.Slow, 120);
                    }
                }
            }
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
                        npc.AddBuff(BuffID.Electrified, 120);

                        Player owner = Main.player[Projectile.owner];
                        if (owner != null && owner.active)
                        {
                            bool crit = Main.rand.Next(100) < Projectile.CritChance;
                            npc.StrikeNPC(new NPC.HitInfo
                            {
                                Damage = (int)(Projectile.damage * 0.5f),
                                Knockback = 4f,
                                HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1,
                                Crit = crit
                            });
                        }
                    }
                }
            }

            for (int i = 0; i < 50; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 8f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.Electric,
                    vel,
                    0,
                    default,
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                d.noGravity = true;
            }
        }
    }
}