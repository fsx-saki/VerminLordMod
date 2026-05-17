using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class BoneThornProj : BaseBullet
    {
        private const int SpikeCount = 16;
        private const float SpikeSpeed = 7f;
        private const float SpikeRadius = 160f;
        private const int SpikeLifetime = 20;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new BoneTrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            Behaviors.Add(new ScaleOverLifeBehavior(0.1f, 1.8f));

            Behaviors.Add(new OnKillAoEBehavior
            {
                Radius = SpikeRadius,
                DamageMultiplier = 1f,
                Knockback = 6f,
                Buffs = new List<(int, int)>
                {
                    (BuffID.BrokenArmor, 200)
                }
            });

            Behaviors.Add(new OnKillProjectileBurstBehavior
            {
                ProjectileType = ModContent.ProjectileType<BoneSpikeProj>(),
                Count = SpikeCount,
                Speed = SpikeSpeed,
                DamageMultiplier = 0.5f,
                KnockbackMultiplier = 0.6f,
                SpreadRadius = 10f
            });

            Behaviors.Add(new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = 20,
                KillSpeed = 5f,
                KillSizeMultiplier = 1.0f,
                KillFragmentLife = 25,
                ColorStart = new Color(230, 220, 190, 255),
                ColorEnd = new Color(160, 140, 100, 0)
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 25,
                        DustType = DustID.Bone,
                        Color = Color.White,
                        ScaleMin = 0.6f,
                        ScaleMax = 1.2f,
                        SpeedMin = 2f,
                        SpeedMax = 6f,
                        SpreadRadius = 20f
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }
    }

    public class BoneSpikeProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.penetrate = 1;
            Projectile.timeLeft = 20;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.aiStyle = -1;
            Projectile.scale = 0.8f;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Bone, Projectile.velocity.X * 0.3f, Projectile.velocity.Y * 0.3f,
                    100, Color.White, Main.rand.NextFloat(0.5f, 1f));
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Bone, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f),
                    100, Color.White, Main.rand.NextFloat(0.4f, 0.7f));
                d.noGravity = true;
            }
        }

        public override bool OnTileCollide(Microsoft.Xna.Framework.Vector2 oldVelocity) => true;
    }
}