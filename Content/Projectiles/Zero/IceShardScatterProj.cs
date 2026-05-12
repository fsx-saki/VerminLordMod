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
    ///
    /// 行为组合：
    /// - AimBehavior: 直线飞行
    /// - BounceBehavior: 物块反弹 2 次后销毁
    /// - IceTrailBehavior: 冰系拖尾（十字星 + 雪片）
    /// - SuppressDrawBehavior: 隐藏默认贴图
    /// - OnKilled: 碎裂成 IceFragmentProj 冰碎片弹幕
    /// </summary>
    public class IceShardScatterProj : BaseBullet
    {
        private const float FlySpeed = 12f;
        private const float ScatterAngle = 0.4f;
        private const int ShardCount = 5;

        private const int KillFragmentCount = 6;
        private const float KillFragmentSpeed = 4f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 反弹 2 次后销毁，触发 OnKill → OnKilled 爆出冰碎片
            Behaviors.Add(new BounceBehavior(maxBounces: 2, bounceFactor: 0.6f)
            {
                KillOnMaxBounces = true,
                TriggerKillOnMaxBounces = true,
            });

            Behaviors.Add(new IceTrailBehavior
            {
                MaxStars = 25,
                StarLife = 30,
                StarSpawnInterval = 2,
                StarSize = 0.7f,
                MaxSnowflakes = 80,
                SnowflakeLife = 28,
                SnowflakeSize = 0.35f,
                SnowflakeClusterSize = 5,
                SnowflakeSpawnChance = 0.7f,
                SnowflakeGravity = 0.1f,
                AutoDraw = true,
                SuppressDefaultDraw = false,
            });

            Behaviors.Add(new SuppressDrawBehavior());
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 0.8f);
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 90);
            target.AddBuff(BuffID.Slow, 120);
        }

        protected override void OnKilled(int timeLeft)
        {
            int fragType = ModContent.ProjectileType<IceFragmentProj>();
            IEntitySource source = Main.player[Projectile.owner]?.GetSource_FromThis();

            for (int i = 0; i < KillFragmentCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(KillFragmentSpeed, KillFragmentSpeed);
                vel.Y -= Main.rand.NextFloat(1f, KillFragmentSpeed * 0.5f);

                Projectile.NewProjectile(
                    source,
                    Projectile.Center,
                    vel,
                    fragType,
                    0,
                    0f,
                    Projectile.owner
                );
            }
        }

        public static void SpawnScatter(Player player, IEntitySource source, Vector2 position, Vector2 baseVelocity, int type, int damage, float knockback)
        {
            float baseAngle = baseVelocity.ToRotation();

            for (int i = 0; i < ShardCount; i++)
            {
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