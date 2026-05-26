using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class ChunQiuChanProj : BaseBullet
    {
        private const float FlySpeed = 7f;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.9f, 0.7f, 0.2f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(200, 160, 40),
                GlowLayers = 3,
                GlowBaseScale = 1.4f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.6f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(0.9f, 0.7f, 0.2f)
            });

            Behaviors.Add(new TimeTrailBehavior
            {
                GhostColor = new Color(220, 180, 60, 140),
                GrainColor = new Color(240, 200, 80, 220),
                ClockHandColor = new Color(200, 170, 50, 200),
                AfterimageColor = new Color(200, 160, 60, 180),
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.Slow, 300)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.GoldFlame,
                DustCount = 12,
                SpeedMin = 1f,
                SpeedMax = 4f,
                ScaleMin = 0.8f,
                ScaleMax = 1.5f,
                Color = Color.Gold
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 15,
                        DustType = DustID.GoldFlame,
                        Color = new Color(220, 180, 40),
                        ScaleMin = 0.6f,
                        ScaleMax = 1.3f,
                        SpeedMin = 2f,
                        SpeedMax = 6f,
                        SpreadRadius = 12f
                    }
                }
            });
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 recordedPos = target.Center;
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<ChunQiuRewindProj>(),
                    0,
                    0f,
                    Projectile.owner,
                    target.whoAmI,
                    recordedPos.X,
                    recordedPos.Y
                );
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }

    public class ChunQiuRewindProj : ModProjectile
    {
        private const int RewindDelay = 120;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.scale = 0f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = RewindDelay;
            Projectile.alpha = 255;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            int npcIndex = (int)Projectile.ai[0];
            if (npcIndex < 0 || npcIndex >= Main.maxNPCs)
            {
                Projectile.Kill();
                return;
            }

            NPC npc = Main.npc[npcIndex];
            if (!npc.active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = npc.Center;

            for (int i = 0; i < 2; i++)
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.GoldFlame);
                d.noGravity = true;
                d.scale = 0.8f;
                d.velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
            }

            if (Projectile.timeLeft <= 1)
            {
                Vector2 rewindPos = new Vector2(Projectile.ai[1], Projectile.ai[2]);

                float distance = Vector2.Distance(npc.Center, rewindPos);
                if (distance > 5f)
                {
                    npc.Center = rewindPos;
                    npc.netUpdate = true;
                    npc.velocity *= 0.1f;

                    for (int i = 0; i < 20; i++)
                    {
                        Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.GoldFlame);
                        d.noGravity = true;
                        d.scale = Main.rand.NextFloat(1f, 2f);
                        d.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}
