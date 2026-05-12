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
    /// 冰道暴风雪 — 从天而降的冰晶群弹幕。
    /// 冰道技术储备库的"暴风雪"技术：
    /// - 在鼠标位置上方生成大量冰晶
    /// - 冰晶受重力影响下落
    /// - 穿透敌人
    /// - 较细的冰系拖尾（十字星 + 雪片 + 虚影）
    /// - 落地时冰晶爆散
    ///
    /// 行为组合：
    /// - GravityBehavior: 重力下落
    /// - IceTrailBehavior: 较细的冰系拖尾
    /// - DustKillBehavior: 落地时冰晶爆散
    /// </summary>
    public class IceBlizzardProj : BaseBullet
    {
        /// <summary>重力加速度</summary>
        private const float GravityAccel = 0.15f;

        /// <summary>最大下落速度</summary>
        private const float MaxFallSpeed = 10f;

        protected override void RegisterBehaviors()
        {
            // 1. 重力下落（缓慢）
            Behaviors.Add(new GravityBehavior(acceleration: GravityAccel, maxFallSpeed: MaxFallSpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 较细的冰系拖尾（十字星 + 雪片 + 虚影）
            Behaviors.Add(new IceTrailBehavior
            {
                // GhostTrail: 更细更淡
                GhostMaxPositions = 6,
                GhostRecordInterval = 3,
                GhostWidthScale = 0.15f,
                GhostLengthScale = 1.2f,
                GhostAlpha = 0.4f,
                GhostColor = new Color(140, 210, 255, 160),
                // 十字星星: 更小更少
                MaxStars = 15,
                StarLife = 20,
                StarSpawnInterval = 4,
                StarSize = 0.3f,
                StarColor = new Color(180, 230, 255, 200),
                // 雪片: 更细更少
                MaxSnowflakes = 40,
                SnowflakeLife = 18,
                SnowflakeSize = 0.15f,
                SnowflakeClusterSize = 3,
                SnowflakeSpawnChance = 0.5f,
                SnowflakeGravity = 0.06f,
                AutoDraw = true,
                SuppressDefaultDraw = false,
            });

            // 3. 碰到物块时销毁
            Behaviors.Add(new KillOnContactBehavior
            {
                KillOnHitNPC = false, // 保留穿透敌人的能力
            });

            // 4. 落地时冰晶爆散
            Behaviors.Add(new DustKillBehavior(
                dustType: DustID.Ice,
                dustCount: 10,
                dustSpeed: 3f,
                dustScale: 0.7f
            )
            {
                NoGravity = true
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        protected override void OnAI()
        {
            // 冰晶微光
            Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 0.8f);
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 附加冻结效果
            target.AddBuff(BuffID.Frostburn, 60);
            target.AddBuff(BuffID.Slow, 90);
        }

        /// <summary>
        /// 生成暴风雪冰晶群（由 Weapon 调用）
        /// </summary>
        public static void SpawnBlizzard(Player player, IEntitySource source, Vector2 mousePos, int type, int damage, float knockback)
        {
            int crystalCount = 12;

            for (int i = 0; i < crystalCount; i++)
            {
                // 在鼠标上方 200~400 像素高空，水平随机分布
                float offsetX = Main.rand.NextFloat(-150f, 150f);
                float offsetY = Main.rand.NextFloat(200f, 400f);
                Vector2 spawnPos = new Vector2(
                    mousePos.X + offsetX,
                    mousePos.Y - offsetY
                );

                // 初始速度：轻微水平偏移
                Vector2 initVel = new Vector2(
                    Main.rand.NextFloat(-1f, 1f),
                    0f
                );

                Projectile.NewProjectile(
                    source,
                    spawnPos,
                    initVel,
                    type,
                    damage,
                    knockback,
                    player.whoAmI
                );
            }
        }
    }
}
