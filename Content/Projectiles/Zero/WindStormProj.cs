using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 风道风卷残云 — 追踪旋风弹幕。
    /// 风道技术储备库的"风卷残云"技术：
    /// - 追踪最近的敌人
    /// - 波浪轨迹飞行
    /// - 穿透敌人，造成多次伤害
    /// - 产生大量风之粒子
    /// - 命中时产生风爆
    ///
    /// 行为组合：
    /// - HomingBehavior: 追踪敌人
    /// - WaveBehavior: 波浪轨迹
    /// - DustTrailBehavior: 风之粒子拖尾
    /// - DustKillBehavior: 消散时风之爆散
    /// </summary>
    public class WindStormProj : BaseBullet
    {
        /// <summary>飞行速度</summary>
        private const float FlySpeed = 10f;

        /// <summary>追踪范围</summary>
        private const float HomingRange = 500f;

        /// <summary>计时器</summary>
        private int _timer = 0;

        protected override void RegisterBehaviors()
        {
            // 1. 追踪最近的敌人
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: 1f / 15f)
            {
                Range = HomingRange,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 波浪轨迹（追踪时产生飘忽不定的轨迹）
            Behaviors.Add(new WaveBehavior(amplitude: 0.05f, frequency: 0.06f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 3. 大量风之粒子拖尾
            Behaviors.Add(new DustTrailBehavior(DustID.Cloud, spawnChance: 1)
            {
                DustScale = 0.8f,
                VelocityMultiplier = 0.15f,
                NoGravity = true,
                DustAlpha = 130,
                RandomSpeed = 0.6f
            });

            // 4. 消散时风之爆散
            Behaviors.Add(new DustKillBehavior(
                dustType: DustID.Cloud,
                dustCount: 25,
                dustSpeed: 6f,
                dustScale: 1.3f
            )
            {
                NoGravity = true
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1.1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 180;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            base.OnSpawned(source);
            _timer = 0;
        }

        protected override void OnAI()
        {
            _timer++;

            // 旋转风之粒子
            if (_timer % 2 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Main.rand.NextFloat(8f, 18f);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;

                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.Cloud,
                    angle.ToRotationVector2() * Main.rand.NextFloat(0.3f, 1.0f),
                    50,
                    new Color(200, 230, 255, 160),
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                d.noGravity = true;
            }

            // 风之微光
            Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 0.7f);
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 命中时产生风爆
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 4f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.Cloud,
                    vel,
                    0,
                    new Color(200, 230, 255, 200),
                    Main.rand.NextFloat(0.6f, 1.2f)
                );
                d.noGravity = true;
            }
        }
    }
}
