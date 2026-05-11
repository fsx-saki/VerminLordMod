using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 液滴泼洒行为 — 弹幕存活期间，定时从中心向上方泼洒子弹幕（液滴）。
    ///
    /// 设计哲学：
    /// 漩涡、水球等弹幕在持续期间应不断向外泼洒小液滴，
    /// 液滴受重力下落，碰到物块反弹，形成"水花四溅"的视觉效果。
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new DropletSplashBehavior
    /// {
    ///     ChildProjectileType = ModContent.ProjectileType&lt;WaterDropProj&gt;(),
    ///     Interval = 15,
    ///     Count = 3,
    ///     SpreadX = 30f,
    ///     SpeedYMin = -3f,
    ///     SpeedYMax = -1f,
    /// });
    /// </code>
    /// </summary>
    public class DropletSplashBehavior : IBulletBehavior
    {
        public string Name => "DropletSplash";

        /// <summary>泼洒的子弹幕类型（必须设置）</summary>
        public int ChildProjectileType { get; set; } = -1;

        /// <summary>泼洒间隔（帧）</summary>
        public int Interval { get; set; } = 15;

        /// <summary>每次泼洒的液滴数量</summary>
        public int Count { get; set; } = 3;

        /// <summary>液滴生成位置水平散布范围（像素）</summary>
        public float SpreadX { get; set; } = 30f;

        /// <summary>液滴初始垂直速度最小值（负值=向上）</summary>
        public float SpeedYMin { get; set; } = -3f;

        /// <summary>液滴初始垂直速度最大值（负值=向上）</summary>
        public float SpeedYMax { get; set; } = -1f;

        /// <summary>液滴生成位置垂直偏移（负值=上方）</summary>
        public float SpawnOffsetY { get; set; } = -10f;

        /// <summary>液滴水平速度倍率（SpreadX 乘以此值得到水平速度范围）</summary>
        public float HorizontalSpeedMultiplier { get; set; } = 0.15f;

        private int _timer;

        public DropletSplashBehavior() { }

        public DropletSplashBehavior(int childProjectileType, int interval = 15, int count = 3)
        {
            ChildProjectileType = childProjectileType;
            Interval = interval;
            Count = count;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _timer = 0;
        }

        public void Update(Projectile projectile)
        {
            if (ChildProjectileType <= 0 || ChildProjectileType >= ProjectileLoader.ProjectileCount)
                return;

            _timer++;
            if (_timer < Interval)
                return;

            _timer = 0;

            for (int i = 0; i < Count; i++)
            {
                Vector2 velocity = new Vector2(
                    Main.rand.NextFloat(-SpreadX, SpreadX) * HorizontalSpeedMultiplier,
                    Main.rand.NextFloat(SpeedYMin, SpeedYMax)
                );
                Vector2 spawnPos = projectile.Center + new Vector2(
                    Main.rand.NextFloat(-SpreadX, SpreadX),
                    SpawnOffsetY
                );

                Projectile.NewProjectile(
                    projectile.GetSource_FromThis(),
                    spawnPos,
                    velocity,
                    ChildProjectileType,
                    0,
                    0f,
                    projectile.owner
                );
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}