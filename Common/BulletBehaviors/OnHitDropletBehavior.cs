using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 命中洒落行为 — 弹幕命中 NPC 时，在命中位置洒落若干子弹幕（如小水滴）。
    ///
    /// 适用于穿透型弹幕：穿过敌人时留下少量子体，增强打击反馈。
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new OnHitDropletBehavior
    /// {
    ///     ChildProjectileType = ModContent.ProjectileType&lt;WaterDropProj&gt;(),
    ///     MinCount = 2,
    ///     MaxCount = 3,
    /// });
    /// </code>
    /// </summary>
    public class OnHitDropletBehavior : IBulletBehavior
    {
        public string Name => "OnHitDroplet";

        /// <summary>洒落的子弹幕类型</summary>
        public int ChildProjectileType { get; set; } = -1;

        /// <summary>最少洒落数量</summary>
        public int MinCount { get; set; } = 2;

        /// <summary>最多洒落数量</summary>
        public int MaxCount { get; set; } = 3;

        /// <summary>X 速度最小值</summary>
        public float VelXMin { get; set; } = -1.5f;

        /// <summary>X 速度最大值</summary>
        public float VelXMax { get; set; } = 1.5f;

        /// <summary>Y 速度最小值（负值=向上）</summary>
        public float VelYMin { get; set; } = -2f;

        /// <summary>Y 速度最大值</summary>
        public float VelYMax { get; set; } = -0.5f;

        /// <summary>生成位置散布半径（像素）</summary>
        public float SpreadRadius { get; set; } = 8f;

        public OnHitDropletBehavior() { }

        public OnHitDropletBehavior(int childProjectileType, int minCount = 2, int maxCount = 3)
        {
            ChildProjectileType = childProjectileType;
            MinCount = minCount;
            MaxCount = maxCount;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (ChildProjectileType <= 0) return;

            int count = Main.rand.Next(MinCount, MaxCount + 1);
            IEntitySource source = Main.player[projectile.owner]?.GetSource_FromThis();

            for (int i = 0; i < count; i++)
            {
                Vector2 vel = new Vector2(
                    Main.rand.NextFloat(VelXMin, VelXMax),
                    Main.rand.NextFloat(VelYMin, VelYMax)
                );
                Vector2 pos = target.Center + Main.rand.NextVector2Circular(SpreadRadius, SpreadRadius);

                Projectile.NewProjectile(
                    source,
                    pos,
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