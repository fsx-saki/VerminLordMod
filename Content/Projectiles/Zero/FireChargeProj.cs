using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 炎道蓄力爆弹 — 蓄力型大型火弹。
    ///
    /// 蓄力阶段：
    /// - 弹幕固定在玩家前方，跟随鼠标方向
    /// - 弹幕逐渐变大（scale 0.3→1.5），透明度降低
    /// - 伤害随蓄力时间增加（最多 3 倍）
    /// - 火焰粒子从玩家身上汇聚到弹幕
    /// - 拖尾宽度随蓄力同步变宽（0.5→2.0）
    ///
    /// 推出阶段：
    /// - 弹幕沿鼠标方向高速飞行
    /// - 带浓烈的液态火焰拖尾（宽度继承蓄力进度）
    /// - 碰到物块或销毁时产生巨大爆炸（爆炸特效大小随蓄力时间增长）
    ///
    /// 行为组合：
    /// - ChargeProjectileBehavior: 蓄力逻辑（解耦自 BloodHandprintsProj）
    /// - AimBehavior: 推出后直线飞行
    /// - LiquidTrailBehavior: 液态火焰拖尾（宽度随蓄力动态变化）
    /// - ExplosionKillBehavior: 巨大爆炸（大小随蓄力动态变化）
    /// </summary>
    public class FireChargeProj : BaseBullet
    {
        /// <summary>推出速度（像素/帧）</summary>
        private const float FireSpeed = 14f;

        /// <summary>最大蓄力时间（帧），5秒</summary>
        private const int MaxChargeTime = 300;

        /// <summary>爆炸时生成的星火弹数量</summary>
        private const int SpawnCount = 12;

        /// <summary>星火弹飞散速度</summary>
        private const float SpawnSpeed = 7f;

        /// <summary>蓄力行为引用</summary>
        private ChargeProjectileBehavior _chargeBehavior;

        /// <summary>拖尾行为引用（用于动态更新宽度）</summary>
        private LiquidTrailBehavior _trailBehavior;

        /// <summary>爆炸行为引用（用于动态更新爆炸参数）</summary>
        private ExplosionKillBehavior _explosionBehavior;

        /// <summary>拖尾宽度随蓄力的变化范围（最小值，满蓄力时）</summary>
        private const float TrailSizeMin = 0.5f;

        /// <summary>拖尾宽度随蓄力的变化范围（最大值，满蓄力时）</summary>
        private const float TrailSizeMax = 2.0f;

        /// <summary>爆炸碎片数量随蓄力的变化范围（最小值）</summary>
        private const int ExplosionCountMin = 15;

        /// <summary>爆炸碎片数量随蓄力的变化范围（最大值，满蓄力时）</summary>
        private const int ExplosionCountMax = 40;

        /// <summary>爆炸碎片大小随蓄力的变化范围（最小值）</summary>
        private const float ExplosionSizeMin = 0.8f;

        /// <summary>爆炸碎片大小随蓄力的变化范围（最大值，满蓄力时）</summary>
        private const float ExplosionSizeMax = 2.5f;

        /// <summary>爆炸碎片速度随蓄力的变化范围（最小值）</summary>
        private const float ExplosionSpeedMin = 5f;

        /// <summary>爆炸碎片速度随蓄力的变化范围（最大值，满蓄力时）</summary>
        private const float ExplosionSpeedMax = 12f;

        protected override void RegisterBehaviors()
        {
            // 1. 蓄力行为（解耦自 BloodHandprintsProj 的蓄力逻辑）
            _chargeBehavior = new ChargeProjectileBehavior
            {
                MaxChargeTime = MaxChargeTime,
                ChargeDistance = 130f,
                StartScale = 0.3f,
                EndScale = 1.5f,
                StartAlpha = 100,
                EndAlpha = 0,
                DamageMultiplier = 2f,       // 满蓄力 3 倍伤害
                ChargeRotationSpeed = 0.025f,
                FireSpeed = FireSpeed,
                RoundCompleteFlag = 1f,
                OnChargeParticle = SpawnChargeParticles,
                // 每帧更新拖尾宽度，使其随蓄力进度同步变宽
                OnChargeUpdate = UpdateTrailSize,
                // 推出时根据蓄力进度设置爆炸参数
                OnFire = OnFireProjectile,
            };
            Behaviors.Add(_chargeBehavior);

            // 2. 推出后直线飞行（AimBehavior 接管运动）
            // speed=0 因为推出速度由 ChargeProjectileBehavior 设置
            Behaviors.Add(new AimBehavior(speed: 0f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.5f, 0.6f, 0.05f)
            });

            // 3. 液态火焰拖尾（浓烈，大号，宽度随蓄力动态变化）
            _trailBehavior = new LiquidTrailBehavior
            {
                MaxFragments = 50,
                FragmentLife = 30,
                SizeMultiplier = TrailSizeMin,  // 初始宽度（蓄力后会动态增大）
                SpawnInterval = 1,
                AdaptiveTargetLength = 150f,
                SpeedLifeExponent = 0.3f,
                MinFragmentLife = 10,
                ColorStart = new Color(255, 200, 80, 255),
                ColorEnd = new Color(255, 50, 0, 0),
                Buoyancy = 0.02f,
                AirResistance = 0.96f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.15f,
                SplashAngle = 0.4f,
                RandomSpread = 0.6f,
                AutoDraw = true,
                SuppressDefaultDraw = true
            };
            Behaviors.Add(_trailBehavior);

            // 4. 销毁时巨大爆炸（视觉碎片效果，大小随蓄力动态变化）
            _explosionBehavior = new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = ExplosionCountMin,       // 初始数量（蓄力后动态增大）
                KillSpeed = ExplosionSpeedMin,       // 初始速度（蓄力后动态增大）
                KillSizeMultiplier = ExplosionSizeMin, // 初始大小（蓄力后动态增大）
                KillFragmentLife = 35,
                ExplodeOnTileCollide = true,
                TileCollideCount = 20,
                TileCollideSpeed = 6f,
                TileCollideSizeMultiplier = 1.3f,
                TileCollideFragmentLife = 30,
                DestroyOnTileCollideExplosion = true,
                ColorStart = new Color(255, 220, 100, 255),
                ColorEnd = new Color(255, 30, 0, 0)
            };
            Behaviors.Add(_explosionBehavior);
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.scale = 0.3f;       // 初始缩小，蓄力后逐渐变大
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 99;     // 不因命中敌人而销毁
            Projectile.timeLeft = 600;     // 较长存活时间
            Projectile.alpha = 100;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.netImportant = true;
        }

        /// <summary>
        /// 蓄力阶段每帧更新拖尾宽度，使其随蓄力进度同步变宽。
        /// 蓄力 0% → SizeMultiplier = TrailSizeMin (0.5)
        /// 蓄力 100% → SizeMultiplier = TrailSizeMax (2.0)
        /// 推出后宽度固定为蓄力结束时的值，不再变化。
        /// </summary>
        private void UpdateTrailSize(Projectile projectile, float progress)
        {
            if (_trailBehavior?.Trail == null) return;
            _trailBehavior.Trail.SizeMultiplier = MathHelper.Lerp(TrailSizeMin, TrailSizeMax, progress);
        }

        /// <summary>
        /// 推出弹幕时的回调 — 根据蓄力进度设置爆炸参数。
        /// 蓄力越久，爆炸碎片越多、越大、飞散越快。
        /// </summary>
        private void OnFireProjectile(Projectile projectile, float progress)
        {
            if (_explosionBehavior == null) return;

            // 根据蓄力进度插值爆炸参数
            _explosionBehavior.KillCount = (int)MathHelper.Lerp(ExplosionCountMin, ExplosionCountMax, progress);
            _explosionBehavior.KillSpeed = MathHelper.Lerp(ExplosionSpeedMin, ExplosionSpeedMax, progress);
            _explosionBehavior.KillSizeMultiplier = MathHelper.Lerp(ExplosionSizeMin, ExplosionSizeMax, progress);

            // 碰撞爆炸也同步增大
            _explosionBehavior.TileCollideCount = (int)(_explosionBehavior.KillCount * 0.6f);
            _explosionBehavior.TileCollideSpeed = _explosionBehavior.KillSpeed * 0.7f;
            _explosionBehavior.TileCollideSizeMultiplier = _explosionBehavior.KillSizeMultiplier * 0.8f;
        }

        protected override void OnKilled(int timeLeft)
        {
            // 销毁时爆出一圈 FireBaseProj（星火弹）
            // 蓄力越久，星火弹越多
            float progress = _chargeBehavior?.ChargeProgress ?? 0.5f;
            int count = (int)(SpawnCount * (0.5f + progress * 0.5f));

            int fireBaseType = ModContent.ProjectileType<FireBaseProj>();

            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * SpawnSpeed;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    vel,
                    fireBaseType,
                    (int)(Projectile.damage * 0.4f),
                    Projectile.knockBack * 0.3f,
                    Projectile.owner
                );
            }
        }

        /// <summary>
        /// 蓄力阶段粒子效果 — 火焰粒子从玩家身上汇聚到弹幕
        /// </summary>
        private void SpawnChargeParticles(Projectile projectile, float progress)
        {
            if (_chargeBehavior == null) return;

            Player owner = Main.player[projectile.owner];
            if (owner == null) return;

            // 每帧生成 1~3 个火焰粒子，从玩家位置飞向弹幕
            int particleCount = 1 + (int)(progress * 2);

            for (int i = 0; i < particleCount; i++)
            {
                // 从玩家周围随机位置出发
                Vector2 startPos = owner.Center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 dirToProj = projectile.Center - startPos;
                float dist = dirToProj.Length();
                if (dist < 1f) continue;

                dirToProj.Normalize();

                // 火焰粒子
                Dust d = Dust.NewDustPerfect(
                    startPos,
                    DustID.Torch,
                    dirToProj * Main.rand.NextFloat(3f, 8f) * (0.5f + progress),
                    0,
                    new Color(255, 200, 80, 200),
                    Main.rand.NextFloat(0.8f, 1.5f)
                );
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            // 蓄力进度较高时，额外生成火星粒子
            if (progress > 0.5f && Main.rand.NextBool(3))
            {
                Vector2 sparkPos = projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                Dust d = Dust.NewDustPerfect(
                    sparkPos,
                    DustID.YellowTorch,
                    Main.rand.NextVector2Circular(2f, 2f),
                    0,
                    new Color(255, 255, 200, 255),
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                d.noGravity = true;
            }
        }
    }
}
