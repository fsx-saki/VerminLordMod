using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class DarkTentacleProj : BaseBullet
    {
        private const int TentacleLife = 120;
        private const int TentacleCount = 5;
        private const float TentacleRange = 180f;
        private const int DamageInterval = 15;

        private int _timer = 0;
        private Vector2[] _tentacleTargets;
        private bool _initialized = false;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new StationaryBehavior());

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(40, 10, 80, 60),
                GlowBaseScale = 2.0f,
                GlowLayers = 1,
                GlowAlphaMultiplier = 0.1f,
                EnableLight = true,
                LightColor = new Vector3(0.1f, 0.02f, 0.3f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TentacleLife;
            Projectile.alpha = 200;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            _timer++;

            if (!_initialized)
            {
                _initialized = true;
                _tentacleTargets = new Vector2[TentacleCount];
                for (int t = 0; t < TentacleCount; t++)
                {
                    float angle = MathHelper.TwoPi * t / TentacleCount + Main.rand.NextFloat(-0.3f, 0.3f);
                    float dist = Main.rand.NextFloat(TentacleRange * 0.5f, TentacleRange);
                    _tentacleTargets[t] = Projectile.Center + angle.ToRotationVector2() * dist;
                }
            }

            Lighting.AddLight(Projectile.Center, 0.1f, 0.02f, 0.2f);

            for (int t = 0; t < TentacleCount; t++)
            {
                float progress = (float)_timer / TentacleLife;
                float waveOffset = (float)System.Math.Sin(_timer * 0.15f + t * 1.5f) * 30f;

                Vector2 tipPos = Vector2.Lerp(Projectile.Center, _tentacleTargets[t], System.Math.Min(progress * 2f, 1f));
                tipPos += new Vector2(waveOffset, 0);

                for (int s = 0; s < 8; s++)
                {
                    float segProgress = s / 8f;
                    Vector2 segPos = Vector2.Lerp(Projectile.Center, tipPos, segProgress);
                    segPos += new Vector2((float)System.Math.Sin(_timer * 0.15f + t * 1.5f + segProgress * 3f) * 15f * (1f - segProgress), 0);

                    if (s % 2 == 0)
                    {
                        Dust d = Dust.NewDustPerfect(segPos, DustID.Shadowflame, Vector2.Zero, 0, default, 0.4f);
                        d.noGravity = true;
                    }
                }

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                    {
                        if (Vector2.Distance(tipPos, npc.Center) < 30f && _timer % DamageInterval == 0)
                        {
                            Player owner = Main.player[Projectile.owner];
                            if (owner != null && owner.active)
                            {
                                bool crit = Main.rand.Next(100) < Projectile.CritChance;
                                npc.StrikeNPC(new NPC.HitInfo
                                {
                                    Damage = Projectile.damage,
                                    Knockback = 2f,
                                    HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1,
                                    Crit = crit
                                });
                            }
                            npc.AddBuff(BuffID.ShadowFlame, 90);
                        }
                    }
                }
            }
        }
    }
}