using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 周期性粉尘行为 — 每帧按概率在弹幕周围生成 Dust 粒子。
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new PeriodicDustBehavior
    /// {
    ///     SpawnChance = 0.5f,
    ///     DustType = DustID.HealingPlus,
    ///     Color = new Color(100, 255, 150, 150),
    /// });
    /// </code>
    /// </summary>
    public class PeriodicDustBehavior : IBulletBehavior
    {
        public string Name => "PeriodicDust";

        /// <summary>每帧生成概率（0~1）</summary>
        public float SpawnChance { get; set; } = 0.5f;

        /// <summary>Dust 类型</summary>
        public int DustType { get; set; } = DustID.HealingPlus;

        /// <summary>粒子颜色</summary>
        public Color Color { get; set; } = new Color(100, 255, 150, 150);

        /// <summary>粒子大小最小值</summary>
        public float ScaleMin { get; set; } = 0.5f;

        /// <summary>粒子大小最大值</summary>
        public float ScaleMax { get; set; } = 1.0f;

        /// <summary>粒子速度</summary>
        public float Speed { get; set; } = 0.5f;

        /// <summary>生成位置散布半径（像素）</summary>
        public float SpreadRadius { get; set; } = 8f;

        /// <summary>是否无重力</summary>
        public bool NoGravity { get; set; } = true;

        public PeriodicDustBehavior() { }

        public PeriodicDustBehavior(int dustType, Color color, float spawnChance = 0.5f)
        {
            DustType = dustType;
            Color = color;
            SpawnChance = spawnChance;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile)
        {
            if (Main.rand.NextFloat() >= SpawnChance) return;

            Vector2 vel = Main.rand.NextVector2Circular(Speed, Speed);
            Dust d = Dust.NewDustPerfect(
                projectile.Center + Main.rand.NextVector2Circular(SpreadRadius, SpreadRadius),
                DustType,
                vel,
                0,
                Color,
                Main.rand.NextFloat(ScaleMin, ScaleMax)
            );
            d.noGravity = NoGravity;
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}