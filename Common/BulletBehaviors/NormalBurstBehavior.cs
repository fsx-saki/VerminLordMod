using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 法线崩解行为 — 弹幕命中/销毁时，沿法线方向（弹幕→目标连线）泼洒子弹幕。
    ///
    /// 设计哲学：
    /// 水球砸在目标表面时，水滴应该沿表面法线方向飞溅出去，
    /// 而不是均匀360°飞散。这模拟了真实的流体碰撞物理。
    ///
    /// 两种触发方式：
    /// - OnHitNPC：沿弹幕→NPC方向（法线）泼洒，模拟水球砸中敌人
    /// - OnKill：沿最后速度方向泼洒（timeout 时的保底效果）
    ///
    /// 泼洒模式：
    /// - 法线方向前方（0°~±SpreadAngle）：大部分水滴向前飞溅
    /// - 法线方向侧方（±SpreadAngle~±SideAngle）：少量水滴侧向飞溅
    /// - 法线方向后方：几乎不飞溅（水不会向后泼）
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new NormalBurstBehavior
    /// {
    ///     ChildProjectileType = ModContent.ProjectileType&lt;WaterDropProj&gt;(),
    ///     Count = 16,
    ///     SpeedMin = 4f, SpeedMax = 10f,
    ///     SpreadAngle = 0.6f,  // 前方锥角
    ///     SideAngle = 1.2f,    // 侧方锥角
    /// });
    /// </code>
    /// </summary>
    public class NormalBurstBehavior : IBulletBehavior
    {
        public string Name => "NormalBurst";

        // ===== 可配置参数 =====

        /// <summary>崩解时生成的子弹幕类型（必须设置）</summary>
        public int ChildProjectileType { get; set; } = -1;

        /// <summary>生成数量</summary>
        public int Count { get; set; } = 16;

        /// <summary>最小速度</summary>
        public float SpeedMin { get; set; } = 4f;

        /// <summary>最大速度</summary>
        public float SpeedMax { get; set; } = 10f;

        /// <summary>生成位置随机偏移半径</summary>
        public float SpreadRadius { get; set; } = 8f;

        /// <summary>前方锥角（弧度），法线方向 ± 此角度内的水滴向前飞溅</summary>
        public float SpreadAngle { get; set; } = 0.6f;

        /// <summary>侧方锥角（弧度），超出 SpreadAngle 但在 SideAngle 内的水滴侧向飞溅</summary>
        public float SideAngle { get; set; } = 1.2f;

        /// <summary>后方飞溅概率（0~1），法线反方向产生水滴的概率。水不应向后飞，默认很低</summary>
        public float BackSplashChance { get; set; } = 0.05f;

        /// <summary>是否在崩解时生成额外的小碎片（Dust）</summary>
        public bool SpawnExtraDust { get; set; } = true;

        /// <summary>额外 Dust 数量</summary>
        public int ExtraDustCount { get; set; } = 12;

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
        private Vector2 _lastNormal = Vector2.Zero;
        private bool _hasHitNormal = false;

        public NormalBurstBehavior() { }

        public NormalBurstBehavior(int childProjectileType, int count = 16)
        {
            ChildProjectileType = childProjectileType;
            Count = count;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _hasBurst = false;
            _lastNormal = Vector2.Zero;
            _hasHitNormal = false;
        }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (_hasBurst) return;

            // 计算法线方向：弹幕中心 → NPC 中心
            Vector2 normal = target.Center - projectile.Center;
            if (normal != Vector2.Zero)
            {
                normal.Normalize();
                _lastNormal = normal;
                _hasHitNormal = true;
            }

            // 命中时立即泼洒
            DoBurst(projectile, normal);
            _hasBurst = true;
        }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            if (_hasBurst) return;
            _hasBurst = true;

            Vector2 normal;

            if (_hasHitNormal && _lastNormal != Vector2.Zero)
            {
                // 使用命中时记录的法线方向
                normal = _lastNormal;
            }
            else if (projectile.velocity != Vector2.Zero)
            {
                // 使用最后速度方向作为法线（timeout 保底）
                normal = projectile.velocity.SafeNormalize(Vector2.Zero);
            }
            else
            {
                // 完全保底：随机方向
                normal = Main.rand.NextVector2Unit();
            }

            DoBurst(projectile, normal);
        }

        /// <summary>
        /// 沿法线方向泼洒子弹幕和 Dust。
        /// </summary>
        private void DoBurst(Projectile projectile, Vector2 normal)
        {
            Vector2 center = projectile.Center;

            // 1. 生成子弹幕（沿法线方向泼洒）
            if (ChildProjectileType > 0 && ChildProjectileType < ProjectileLoader.ProjectileCount)
            {
                SpawnNormalProjectiles(center, normal, projectile);
            }

            // 2. 额外小碎片（Dust）
            if (SpawnExtraDust)
            {
                SpawnNormalDust(center, normal);
            }
        }

        /// <summary>
        /// 沿法线方向生成子弹幕。
        /// 大部分水滴在前方锥角内（模拟水球砸中后的主要飞溅方向），
        /// 少量在侧方，极少量在后方。
        /// </summary>
        private void SpawnNormalProjectiles(Vector2 center, Vector2 normal, Projectile parent)
        {
            IEntitySource source = parent.GetSource_Death();
            if (source == null && parent.owner >= 0 && parent.owner < Main.maxPlayers)
            {
                var player = Main.player[parent.owner];
                if (player != null && player.active)
                    source = player.GetSource_FromThis();
            }
            if (source == null) return;

            for (int i = 0; i < Count; i++)
            {
                // 决定水滴的角度分布
                float angleOffset;
                float roll = Main.rand.NextFloat();

                if (roll < 0.65f)
                {
                    // 65% 在前方锥角内（主要飞溅方向）
                    angleOffset = Main.rand.NextFloat(-SpreadAngle, SpreadAngle);
                }
                else if (roll < 0.90f)
                {
                    // 25% 在侧方（SpreadAngle ~ SideAngle）
                    float sideAngle = Main.rand.NextFloat(SpreadAngle, SideAngle);
                    angleOffset = Main.rand.NextBool() ? sideAngle : -sideAngle;
                }
                else if (roll < 0.95f)
                {
                    // 5% 在更侧方（SideAngle ~ SideAngle*1.5）
                    float farAngle = Main.rand.NextFloat(SideAngle, SideAngle * 1.5f);
                    angleOffset = Main.rand.NextBool() ? farAngle : -farAngle;
                }
                else
                {
                    // 5% 在后方（极少，模拟反弹的小水滴）
                    angleOffset = Main.rand.NextFloat(MathHelper.Pi - 0.5f, MathHelper.Pi + 0.5f);
                    if (Main.rand.NextBool())
                        angleOffset = -angleOffset;
                }

                float angle = normal.ToRotation() + angleOffset;
                float speed = Main.rand.NextFloat(SpeedMin, SpeedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Vector2 spawnPos = center + Main.rand.NextVector2Circular(SpreadRadius, SpreadRadius);

                Projectile.NewProjectile(
                    source, spawnPos, vel,
                    ChildProjectileType, 0, 0f, parent.owner);
            }
        }

        /// <summary>
        /// 沿法线方向生成 Dust 粒子。
        /// </summary>
        private void SpawnNormalDust(Vector2 center, Vector2 normal)
        {
            for (int i = 0; i < ExtraDustCount; i++)
            {
                // Dust 的角度分布：主要在前方和侧方
                float angleOffset;
                float roll = Main.rand.NextFloat();
                if (roll < 0.7f)
                    angleOffset = Main.rand.NextFloat(-SpreadAngle * 1.5f, SpreadAngle * 1.5f);
                else
                    angleOffset = Main.rand.NextFloat(-SideAngle, SideAngle);

                float angle = normal.ToRotation() + angleOffset;
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
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}