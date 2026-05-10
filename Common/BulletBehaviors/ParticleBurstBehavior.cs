using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 粒子体崩解行为 — 弹幕销毁时，生成大量子弹幕向四面八方飞散。
    ///
    /// 效果描述：
    /// - 弹幕死亡时，生成指定类型的子弹幕（如 WaterDropProj）向四周飞散
    /// - 子弹幕受重力、反弹、拖尾等行为影响（由子弹幕自身定义）
    /// - 模拟水球、泥球、能量球等"崩解"而非"爆炸"的视觉效果
    ///
    /// 实现方式：
    /// - 使用 ExplosionSpawnHelper.SpawnProjectiles() 生成子弹幕
    /// - 与 BloodHandprintsProj.OnKill → BloodDropProj 模式一致，经过验证
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new ParticleBurstBehavior
    /// {
    ///     ChildProjectileType = ModContent.ProjectileType<WaterDropProj>(),
    ///     Count = 12,
    ///     SpeedMin = 3f, SpeedMax = 6f,
    ///     SpreadRadius = 5f,
    ///     AngleSpread = 0.4f,
    /// });
    /// </code>
    /// </summary>
    public class ParticleBurstBehavior : IBulletBehavior
    {
        public string Name => "ParticleBurst";

        // ===== 可配置参数 =====

        /// <summary>崩解时生成的子弹幕类型（必须设置）</summary>
        public int ChildProjectileType { get; set; } = -1;

        /// <summary>生成数量</summary>
        public int Count { get; set; } = 12;

        /// <summary>最小速度</summary>
        public float SpeedMin { get; set; } = 3f;

        /// <summary>最大速度</summary>
        public float SpeedMax { get; set; } = 6f;

        /// <summary>生成位置随机偏移半径</summary>
        public float SpreadRadius { get; set; } = 5f;

        /// <summary>角度随机偏移（弧度）</summary>
        public float AngleSpread { get; set; } = 0.4f;

        /// <summary>是否在崩解时生成额外的小碎片（Dust）</summary>
        public bool SpawnExtraDust { get; set; } = true;

        /// <summary>额外 Dust 数量</summary>
        public int ExtraDustCount { get; set; } = 10;

        /// <summary>额外 Dust 类型</summary>
        public int DustType { get; set; } = DustID.MagicMirror;

        /// <summary>Dust 颜色起始</summary>
        public Color DustColorStart { get; set; } = new Color(30, 100, 200, 200);

        /// <summary>Dust 颜色结束</summary>
        public Color DustColorEnd { get; set; } = new Color(30, 100, 200, 0);

        /// <summary>Dust 大小范围</summary>
        public float DustScaleMin { get; set; } = 0.4f;

        /// <summary>Dust 大小范围</summary>
        public float DustScaleMax { get; set; } = 0.8f;

        /// <summary>Dust 速度范围</summary>
        public float DustSpeedMin { get; set; } = 1f;

        /// <summary>Dust 速度范围</summary>
        public float DustSpeedMax { get; set; } = 3f;

        /// <summary>Dust 是否无重力</summary>
        public bool DustNoGravity { get; set; } = false;

        // ===== 内部状态 =====

        private bool _hasBurst = false;

        public ParticleBurstBehavior() { }

        public ParticleBurstBehavior(int childProjectileType, int count = 12)
        {
            ChildProjectileType = childProjectileType;
            Count = count;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _hasBurst = false;
        }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            if (_hasBurst) return;
            _hasBurst = true;

            Vector2 center = projectile.Center;

            // 1. 生成子弹幕（使用 ExplosionSpawnHelper，与 BloodHandprintsProj 模式一致）
            if (ChildProjectileType > 0 && ChildProjectileType < ProjectileLoader.ProjectileCount)
            {
                ExplosionSpawnHelper.SpawnProjectiles(
                    center,
                    ChildProjectileType,
                    count: Count,
                    speedMin: SpeedMin,
                    speedMax: SpeedMax,
                    owner: projectile.owner,
                    damage: 0,
                    spreadRadius: SpreadRadius,
                    angleSpread: AngleSpread,
                    source: projectile.GetSource_Death());
            }

            // 2. 额外小碎片（Dust）
            if (SpawnExtraDust)
            {
                SpawnExtraDustParticles(center);
            }
        }

        /// <summary>
        /// 生成额外的小碎片（Dust），增加崩解细节。
        /// </summary>
        private void SpawnExtraDustParticles(Vector2 center)
        {
            for (int i = 0; i < ExtraDustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(DustSpeedMin, DustSpeedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;

                float t = Main.rand.NextFloat(0f, 0.6f);
                Color dustColor = Color.Lerp(DustColorStart, DustColorEnd, t);
                float scale = Main.rand.NextFloat(DustScaleMin, DustScaleMax);

                int dustId = Dust.NewDust(
                    center - new Vector2(4f), 8, 8,
                    DustType,
                    vel.X, vel.Y,
                    80, dustColor, scale
                );

                if (dustId >= 0 && dustId < Main.dust.Length)
                {
                    Dust d = Main.dust[dustId];
                    d.noGravity = DustNoGravity;
                    d.velocity = vel;
                    d.fadeIn = 0.3f;
                }
            }
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true; // 崩解效果由子弹幕自身绘制
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
