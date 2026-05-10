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
    /// 风道旋风弹 — 旋转前进的旋风弹幕。
    /// 风道技术储备库的"旋风"技术：
    /// - 弹幕沿波浪轨迹旋转前进
    /// - 对路径上的敌人造成多次伤害
    /// - 产生旋转风之粒子效果
    /// - 穿透敌人
    ///
    /// 行为组合：
    /// - AimBehavior: 直线飞行
    /// - WaveBehavior: 波浪轨迹
    /// - DustTrailBehavior: 风之粒子拖尾
    /// - DustKillBehavior: 消散时风之爆散
    /// </summary>
    public class WindCycloneProj : BaseBullet
    {
        /// <summary>飞行速度</summary>
        private const float FlySpeed = 8f;

        /// <summary>计时器</summary>
        private int _timer = 0;

        protected override void RegisterBehaviors()
        {
            // 1. 直线飞行（中等速度）
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 波浪轨迹（旋转感）
            Behaviors.Add(new WaveBehavior(amplitude: 0.08f, frequency: 0.08f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 3. 风之粒子拖尾（大量）
            Behaviors.Add(new DustTrailBehavior(DustID.Cloud, spawnChance: 1)
            {
                DustScale = 0.6f,
                VelocityMultiplier = 0.2f,
                NoGravity = true,
                DustAlpha = 100,
                RandomSpeed = 0.5f
            });

            // 4. 消散时风之爆散
            Behaviors.Add(new DustKillBehavior(
                dustType: DustID.Cloud,
                dustCount: 20,
                dustSpeed: 5f,
                dustScale: 1.2f
            )
            {
                NoGravity = true
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 120;
            Projectile.alpha = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
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
                float radius = Main.rand.NextFloat(5f, 15f);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;

                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.Cloud,
                    angle.ToRotationVector2() * Main.rand.NextFloat(0.5f, 1.5f),
                    50,
                    new Color(180, 220, 255, 150),
                    Main.rand.NextFloat(0.4f, 0.8f)
                );
                d.noGravity = true;
            }

            // 风之微光
            Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 0.6f);
        }
    }
}
