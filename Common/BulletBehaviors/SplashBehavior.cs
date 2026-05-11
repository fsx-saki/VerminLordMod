using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 泼溅模式枚举。
    /// </summary>
    public enum SplashMode
    {
        /// <summary>法线方向 — 沿弹幕→目标连线方向泼溅，模拟流体碰撞。前方多、侧方少、后方极少。</summary>
        Normal,

        /// <summary>径向均匀 — 360° 均匀散射，模拟爆炸/崩解。</summary>
        Radial,

        /// <summary>锥形定向 — 沿速度方向的前方锥形泼溅，模拟霰弹/喷溅。</summary>
        Cone,

        /// <summary>环形 — 垂直于速度方向的环形泼溅，模拟冲击波/涟漪。</summary>
        Ring,

        /// <summary>前方定向 — 仅沿速度正前方泼溅，无侧方/后方，模拟聚焦喷射。</summary>
        Forward,
    }

    /// <summary>
    /// 统一泼溅行为 — 弹幕命中/销毁时，以多种模式泼洒子弹幕和 Dust。
    ///
    /// 设计哲学：
    /// 不同"道"的弹幕命中后应有不同的崩解/泼溅方式：
    /// - 水 → Normal（沿法线泼洒，模拟流体碰撞）
    /// - 火 → Radial（360° 爆炸，模拟燃烧扩散）
    /// - 雷 → Cone（沿速度方向锥形，模拟电弧喷射）
    /// - 暗 → Ring（环形扩散，模拟暗影冲击波）
    /// - 光 → Forward（前方聚焦，模拟光棱折射）
    ///
    /// 一个行为替代 NormalBurstBehavior + ParticleBurstBehavior，
    /// 并扩展更多模式，最大化代码复用。
    ///
    /// 使用方式：
    /// <code>
    /// // 法线泼溅（水）
    /// Behaviors.Add(new SplashBehavior(SplashMode.Normal)
    /// {
    ///     ChildProjectileType = ModContent.ProjectileType&lt;WaterDropProj&gt;(),
    ///     Count = 16, SpeedMin = 4f, SpeedMax = 10f,
    /// });
    ///
    /// // 径向爆炸（火）
    /// Behaviors.Add(new SplashBehavior(SplashMode.Radial)
    /// {
    ///     ChildProjectileType = ModContent.ProjectileType&lt;FireSparkProj&gt;(),
    ///     Count = 12, SpeedMin = 3f, SpeedMax = 8f,
    /// });
    ///
    /// // 锥形喷射（雷）
    /// Behaviors.Add(new SplashBehavior(SplashMode.Cone)
    /// {
    ///     ChildProjectileType = ModContent.ProjectileType&lt;LightningBoltProj&gt;(),
    ///     Count = 8, SpeedMin = 5f, SpeedMax = 12f,
    ///     ConeAngle = 0.5f,
    /// });
    ///
    /// // 环形冲击波（暗）
    /// Behaviors.Add(new SplashBehavior(SplashMode.Ring)
    /// {
    ///     ChildProjectileType = ModContent.ProjectileType&lt;ShadowShardProj&gt;(),
    ///     Count = 10, SpeedMin = 4f, SpeedMax = 8f,
    /// });
    /// </code>
    /// </summary>
    public class SplashBehavior : IBulletBehavior
    {
        public string Name => "Splash";

        // ===== 泼溅模式 =====

        /// <summary>泼溅模式</summary>
        public SplashMode Mode { get; set; } = SplashMode.Normal;

        // ===== 子弹幕参数 =====

        /// <summary>崩解时生成的子弹幕类型（必须设置）</summary>
        public int ChildProjectileType { get; set; } = -1;

        /// <summary>生成数量</summary>
        public int Count { get; set; } = 12;

        /// <summary>最小速度</summary>
        public float SpeedMin { get; set; } = 3f;

        /// <summary>最大速度</summary>
        public float SpeedMax { get; set; } = 8f;

        /// <summary>生成位置随机偏移半径</summary>
        public float SpreadRadius { get; set; } = 6f;

        // ===== Normal 模式专用参数 =====

        /// <summary>前方锥角（弧度），法线方向 ± 此角度内的水滴向前飞溅</summary>
        public float NormalSpreadAngle { get; set; } = 0.6f;

        /// <summary>侧方锥角（弧度），超出 SpreadAngle 但在 SideAngle 内的水滴侧向飞溅</summary>
        public float NormalSideAngle { get; set; } = 1.2f;

        /// <summary>后方飞溅概率（0~1），法线反方向产生水滴的概率</summary>
        public float NormalBackSplashChance { get; set; } = 0.05f;

        // ===== Cone / Forward 模式专用参数 =====

        /// <summary>锥形角度（弧度），速度方向 ± 此角度</summary>
        public float ConeAngle { get; set; } = 0.5f;

        // ===== Ring 模式专用参数 =====

        /// <summary>环形角度偏移（弧度），垂直于速度方向 ± 此角度</summary>
        public float RingAngleOffset { get; set; } = 0.3f;

        // ===== Dust 参数 =====

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
        private Vector2 _lastNormal = Vector2.Zero;
        private bool _hasHitNormal = false;

        public SplashBehavior() { }

        public SplashBehavior(SplashMode mode)
        {
            Mode = mode;
        }

        public SplashBehavior(SplashMode mode, int childProjectileType, int count = 12)
        {
            Mode = mode;
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

            // 记录法线方向（Normal 模式需要）
            Vector2 normal = target.Center - projectile.Center;
            if (normal != Vector2.Zero)
            {
                normal.Normalize();
                _lastNormal = normal;
                _hasHitNormal = true;
            }

            DoBurst(projectile, normal);
            _hasBurst = true;
        }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            if (_hasBurst) return;
            _hasBurst = true;

            Vector2 direction;

            if (_hasHitNormal && _lastNormal != Vector2.Zero)
            {
                direction = _lastNormal;
            }
            else if (projectile.velocity != Vector2.Zero)
            {
                direction = projectile.velocity.SafeNormalize(Vector2.Zero);
            }
            else
            {
                direction = Main.rand.NextVector2Unit();
            }

            DoBurst(projectile, direction);
        }

        /// <summary>
        /// 根据当前模式执行泼溅。
        /// </summary>
        private void DoBurst(Projectile projectile, Vector2 direction)
        {
            Vector2 center = projectile.Center;

            if (ChildProjectileType > 0 && ChildProjectileType < ProjectileLoader.ProjectileCount)
            {
                switch (Mode)
                {
                    case SplashMode.Normal:
                        SpawnNormal(center, direction, projectile);
                        break;
                    case SplashMode.Radial:
                        SpawnRadial(center, projectile);
                        break;
                    case SplashMode.Cone:
                        SpawnCone(center, direction, projectile);
                        break;
                    case SplashMode.Ring:
                        SpawnRing(center, direction, projectile);
                        break;
                    case SplashMode.Forward:
                        SpawnForward(center, direction, projectile);
                        break;
                }
            }

            if (SpawnExtraDust)
            {
                switch (Mode)
                {
                    case SplashMode.Normal:
                        SpawnNormalDust(center, direction);
                        break;
                    case SplashMode.Radial:
                        SpawnRadialDust(center);
                        break;
                    case SplashMode.Cone:
                        SpawnConeDust(center, direction);
                        break;
                    case SplashMode.Ring:
                        SpawnRingDust(center, direction);
                        break;
                    case SplashMode.Forward:
                        SpawnForwardDust(center, direction);
                        break;
                }
            }
        }

        // ============================================================
        //  Normal 模式 — 沿法线方向泼溅
        // ============================================================

        private void SpawnNormal(Vector2 center, Vector2 normal, Projectile parent)
        {
            IEntitySource source = GetSource(parent);
            if (source == null) return;

            for (int i = 0; i < Count; i++)
            {
                float angleOffset;
                float roll = Main.rand.NextFloat();

                if (roll < 0.65f)
                    angleOffset = Main.rand.NextFloat(-NormalSpreadAngle, NormalSpreadAngle);
                else if (roll < 0.90f)
                {
                    float sideAngle = Main.rand.NextFloat(NormalSpreadAngle, NormalSideAngle);
                    angleOffset = Main.rand.NextBool() ? sideAngle : -sideAngle;
                }
                else if (roll < 0.95f)
                {
                    float farAngle = Main.rand.NextFloat(NormalSideAngle, NormalSideAngle * 1.5f);
                    angleOffset = Main.rand.NextBool() ? farAngle : -farAngle;
                }
                else
                {
                    angleOffset = Main.rand.NextFloat(MathHelper.Pi - 0.5f, MathHelper.Pi + 0.5f);
                    if (Main.rand.NextBool()) angleOffset = -angleOffset;
                }

                float angle = normal.ToRotation() + angleOffset;
                float speed = Main.rand.NextFloat(SpeedMin, SpeedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Vector2 spawnPos = center + Main.rand.NextVector2Circular(SpreadRadius, SpreadRadius);

                Projectile.NewProjectile(source, spawnPos, vel, ChildProjectileType, 0, 0f, parent.owner);
            }
        }

        private void SpawnNormalDust(Vector2 center, Vector2 normal)
        {
            for (int i = 0; i < ExtraDustCount; i++)
            {
                float angleOffset;
                float roll = Main.rand.NextFloat();
                if (roll < 0.7f)
                    angleOffset = Main.rand.NextFloat(-NormalSpreadAngle * 1.5f, NormalSpreadAngle * 1.5f);
                else
                    angleOffset = Main.rand.NextFloat(-NormalSideAngle, NormalSideAngle);

                float angle = normal.ToRotation() + angleOffset;
                float speed = Main.rand.NextFloat(DustSpeedMin, DustSpeedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;

                SpawnDust(center, vel);
            }
        }

        // ============================================================
        //  Radial 模式 — 360° 均匀散射
        // ============================================================

        private void SpawnRadial(Vector2 center, Projectile parent)
        {
            IEntitySource source = GetSource(parent);
            if (source == null) return;

            for (int i = 0; i < Count; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(SpeedMin, SpeedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Vector2 spawnPos = center + Main.rand.NextVector2Circular(SpreadRadius, SpreadRadius);

                Projectile.NewProjectile(source, spawnPos, vel, ChildProjectileType, 0, 0f, parent.owner);
            }
        }

        private void SpawnRadialDust(Vector2 center)
        {
            for (int i = 0; i < ExtraDustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(DustSpeedMin, DustSpeedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;
                SpawnDust(center, vel);
            }
        }

        // ============================================================
        //  Cone 模式 — 沿速度方向锥形泼溅
        // ============================================================

        private void SpawnCone(Vector2 center, Vector2 direction, Projectile parent)
        {
            IEntitySource source = GetSource(parent);
            if (source == null) return;

            for (int i = 0; i < Count; i++)
            {
                float angleOffset = Main.rand.NextFloat(-ConeAngle, ConeAngle);
                float angle = direction.ToRotation() + angleOffset;
                float speed = Main.rand.NextFloat(SpeedMin, SpeedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Vector2 spawnPos = center + Main.rand.NextVector2Circular(SpreadRadius, SpreadRadius);

                Projectile.NewProjectile(source, spawnPos, vel, ChildProjectileType, 0, 0f, parent.owner);
            }
        }

        private void SpawnConeDust(Vector2 center, Vector2 direction)
        {
            for (int i = 0; i < ExtraDustCount; i++)
            {
                float angleOffset = Main.rand.NextFloat(-ConeAngle * 1.5f, ConeAngle * 1.5f);
                float angle = direction.ToRotation() + angleOffset;
                float speed = Main.rand.NextFloat(DustSpeedMin, DustSpeedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;
                SpawnDust(center, vel);
            }
        }

        // ============================================================
        //  Ring 模式 — 垂直于速度方向的环形泼溅
        // ============================================================

        private void SpawnRing(Vector2 center, Vector2 direction, Projectile parent)
        {
            IEntitySource source = GetSource(parent);
            if (source == null) return;

            Vector2 perp = new Vector2(-direction.Y, direction.X);

            for (int i = 0; i < Count; i++)
            {
                float angleOffset = Main.rand.NextFloat(-RingAngleOffset, RingAngleOffset);
                float baseAngle = perp.ToRotation();
                float angle = baseAngle + angleOffset;

                // 一半在垂直方向一侧，一半在另一侧
                if (i >= Count / 2)
                    angle += MathHelper.Pi;

                float speed = Main.rand.NextFloat(SpeedMin, SpeedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Vector2 spawnPos = center + Main.rand.NextVector2Circular(SpreadRadius, SpreadRadius);

                Projectile.NewProjectile(source, spawnPos, vel, ChildProjectileType, 0, 0f, parent.owner);
            }
        }

        private void SpawnRingDust(Vector2 center, Vector2 direction)
        {
            Vector2 perp = new Vector2(-direction.Y, direction.X);

            for (int i = 0; i < ExtraDustCount; i++)
            {
                float angleOffset = Main.rand.NextFloat(-RingAngleOffset * 1.5f, RingAngleOffset * 1.5f);
                float baseAngle = perp.ToRotation();
                float angle = baseAngle + angleOffset;
                if (Main.rand.NextBool()) angle += MathHelper.Pi;

                float speed = Main.rand.NextFloat(DustSpeedMin, DustSpeedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;
                SpawnDust(center, vel);
            }
        }

        // ============================================================
        //  Forward 模式 — 仅沿速度正前方泼溅
        // ============================================================

        private void SpawnForward(Vector2 center, Vector2 direction, Projectile parent)
        {
            IEntitySource source = GetSource(parent);
            if (source == null) return;

            for (int i = 0; i < Count; i++)
            {
                float angleOffset = Main.rand.NextFloat(-ConeAngle * 0.5f, ConeAngle * 0.5f);
                float angle = direction.ToRotation() + angleOffset;
                float speed = Main.rand.NextFloat(SpeedMin, SpeedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Vector2 spawnPos = center + direction * Main.rand.NextFloat(0, SpreadRadius);

                Projectile.NewProjectile(source, spawnPos, vel, ChildProjectileType, 0, 0f, parent.owner);
            }
        }

        private void SpawnForwardDust(Vector2 center, Vector2 direction)
        {
            for (int i = 0; i < ExtraDustCount; i++)
            {
                float angleOffset = Main.rand.NextFloat(-ConeAngle, ConeAngle);
                float angle = direction.ToRotation() + angleOffset;
                float speed = Main.rand.NextFloat(DustSpeedMin, DustSpeedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;
                SpawnDust(center, vel);
            }
        }

        // ============================================================
        //  工具方法
        // ============================================================

        private IEntitySource GetSource(Projectile parent)
        {
            IEntitySource source = parent.GetSource_Death();
            if (source == null && parent.owner >= 0 && parent.owner < Main.maxPlayers)
            {
                var player = Main.player[parent.owner];
                if (player != null && player.active)
                    source = player.GetSource_FromThis();
            }
            return source;
        }

        private void SpawnDust(Vector2 center, Vector2 vel)
        {
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

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}