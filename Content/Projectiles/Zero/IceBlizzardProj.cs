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
    /// - 冰晶粒子拖尾
    /// - 落地时冰晶爆散
    ///
    /// 行为组合：
    /// - GravityBehavior: 重力下落
    /// - DustTrailBehavior: 冰晶粒子拖尾
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

            // 2. 冰晶粒子拖尾
            Behaviors.Add(new DustTrailBehavior(DustID.Ice, spawnChance: 1)
            {
                DustScale = 0.5f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                DustAlpha = 130,
                RandomSpeed = 0.2f
            });

            // 3. 落地时冰晶爆散
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
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.scale = 0.7f;
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
