using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 命中治疗行为 — 弹幕命中 NPC 时，按伤害比例回复玩家生命，并生成治疗粒子。
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new HealOnHitBehavior
    /// {
    ///     HealMultiplier = 0.5f,
    ///     HealDustCount = 8,
    /// });
    /// </code>
    /// </summary>
    public class HealOnHitBehavior : IBulletBehavior
    {
        public string Name => "HealOnHit";

        /// <summary>治疗比例（伤害 × 此值 = 回复量）</summary>
        public float HealMultiplier { get; set; } = 0.5f;

        /// <summary>治疗粒子数量</summary>
        public int HealDustCount { get; set; } = 8;

        /// <summary>治疗粒子类型</summary>
        public int DustType { get; set; } = DustID.HealingPlus;

        /// <summary>治疗粒子颜色</summary>
        public Color DustColor { get; set; } = new Color(100, 255, 150, 200);

        /// <summary>治疗粒子大小最小值</summary>
        public float DustScaleMin { get; set; } = 0.8f;

        /// <summary>治疗粒子大小最大值</summary>
        public float DustScaleMax { get; set; } = 1.5f;

        /// <summary>治疗粒子速度</summary>
        public float DustSpeed { get; set; } = 3f;

        /// <summary>治疗粒子散布半径（像素）</summary>
        public float DustSpreadRadius { get; set; } = 15f;

        public HealOnHitBehavior() { }

        public HealOnHitBehavior(float healMultiplier)
        {
            HealMultiplier = healMultiplier;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[projectile.owner];
            if (owner == null || !owner.active) return;

            int healAmount = (int)(damageDone * HealMultiplier);
            if (healAmount <= 0) return;

            owner.statLife += healAmount;
            owner.HealEffect(healAmount);

            for (int i = 0; i < HealDustCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(DustSpeed, DustSpeed);
                Dust d = Dust.NewDustPerfect(
                    owner.Center + Main.rand.NextVector2Circular(DustSpreadRadius, DustSpreadRadius),
                    DustType,
                    vel,
                    0,
                    DustColor,
                    Main.rand.NextFloat(DustScaleMin, DustScaleMax)
                );
                d.noGravity = true;
            }
        }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}