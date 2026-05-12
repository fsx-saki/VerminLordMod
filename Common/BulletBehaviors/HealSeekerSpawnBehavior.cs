using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 治疗追踪弹生成行为 — 命中 NPC 时按概率生成若干治疗追踪弹。
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new HealSeekerSpawnBehavior
    /// {
    ///     ChildProjectileType = ModContent.ProjectileType&lt;WaterHealSeekerProj&gt;(),
    ///     SpawnChance = 0.6f,
    ///     MinCount = 1,
    ///     MaxCount = 3,
    /// });
    /// </code>
    /// </summary>
    public class HealSeekerSpawnBehavior : IBulletBehavior
    {
        public string Name => "HealSeekerSpawn";

        /// <summary>生成的追踪弹类型</summary>
        public int ChildProjectileType { get; set; } = -1;

        /// <summary>生成概率（0~1）</summary>
        public float SpawnChance { get; set; } = 0.6f;

        /// <summary>最少生成数量</summary>
        public int MinCount { get; set; } = 1;

        /// <summary>最多生成数量</summary>
        public int MaxCount { get; set; } = 3;

        /// <summary>初始速度大小</summary>
        public float Speed { get; set; } = 3f;

        public HealSeekerSpawnBehavior() { }

        public HealSeekerSpawnBehavior(int childProjectileType, float spawnChance = 0.6f, int minCount = 1, int maxCount = 3)
        {
            ChildProjectileType = childProjectileType;
            SpawnChance = spawnChance;
            MinCount = minCount;
            MaxCount = maxCount;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (ChildProjectileType <= 0) return;
            if (Main.rand.NextFloat() >= SpawnChance) return;

            int count = Main.rand.Next(MinCount, MaxCount + 1);
            IEntitySource source = Main.player[projectile.owner]?.GetSource_FromThis();

            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(Speed, Speed);
                Projectile.NewProjectile(
                    source,
                    projectile.Center,
                    vel,
                    ChildProjectileType,
                    0,
                    0f,
                    projectile.owner
                );
            }
        }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}