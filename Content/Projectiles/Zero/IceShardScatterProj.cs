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
    /// 冰道冰锥散射 — 扇形散射冰锥弹幕。
    /// 冰道技术储备库的"冰锥散射"技术：
    /// - 发射后分裂成多个冰锥，呈扇形散射
    /// - 每个冰锥穿透敌人
    /// - 冰晶粒子拖尾
    /// - 命中时冰晶爆散 + 冻结效果
    ///
    /// 行为组合：
    /// - AimBehavior: 直线飞行
    /// - DustTrailBehavior: 冰晶粒子拖尾
    /// - DustKillBehavior: 命中时冰晶爆散
    /// </summary>
    public class IceShardScatterProj : BaseBullet
    {
        /// <summary>飞行速度</summary>
        private const float FlySpeed = 12f;

        /// <summary>散射角度（弧度）</summary>
        private const float ScatterAngle = 0.4f; // 约 23 度

        /// <summary>散射数量</summary>
        private const int ShardCount = 5;

        protected override void RegisterBehaviors()
        {
            // 1. 直线飞行
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 冰晶粒子拖尾
            Behaviors.Add(new DustTrailBehavior(DustID.Ice, spawnChance: 1)
            {
                DustScale = 0.6f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                DustAlpha = 150,
                RandomSpeed = 0.3f
            });

            // 3. 命中时冰晶爆散
            Behaviors.Add(new DustKillBehavior(
                dustType: DustID.Ice,
                dustCount: 12,
                dustSpeed: 3f,
                dustScale: 0.8f
            )
            {
                NoGravity = true
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 2; // 穿透 2 个敌人
            Projectile.timeLeft = 120;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            // 冰晶微光
            Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 0.8f);
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 附加冻结效果
            target.AddBuff(BuffID.Frostburn, 90);
            target.AddBuff(BuffID.Slow, 120);
        }

        /// <summary>
        /// 生成散射冰锥（由 Weapon 调用）
        /// </summary>
        public static void SpawnScatter(Player player, IEntitySource source, Vector2 position, Vector2 baseVelocity, int type, int damage, float knockback)
        {
            float baseAngle = baseVelocity.ToRotation();

            for (int i = 0; i < ShardCount; i++)
            {
                // 计算散射角度（均匀分布）
                float offset = (i - (ShardCount - 1) / 2f) * ScatterAngle / (ShardCount - 1);
                float angle = baseAngle + offset;
                Vector2 vel = angle.ToRotationVector2() * FlySpeed;

                Projectile.NewProjectile(
                    source,
                    position,
                    vel,
                    type,
                    damage,
                    knockback * 0.5f,
                    player.whoAmI
                );
            }
        }
    }
}
