using System;
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
    /// 水道弧形水刃 — 以弧线轨迹从侧面斩击目标。
    ///
    /// 设计哲学：
    /// 水不只有"泼洒"一种攻击方式。高压水刃可以像刀一样斩击。
    /// 弧形轨迹让弹幕从侧面切入，产生强烈的"斩击感"。
    ///
    /// 运动方式（三阶段）：
    ///   阶段0（侧向发射）：沿垂直于鼠标的方向发射，向外飞出
    ///   阶段1（弧线回旋）：逐渐向鼠标方向弯曲，画出一道大弧线
    ///   阶段2（直线冲刺）：到达弧线顶点后直线飞向鼠标
    ///
    /// 视觉效果：
    /// - 使用 LiquidTrailBehavior（与火焰同款贴图拖尾），水蓝→浅蓝渐变
    /// - 拖尾碎片有浮力（向上飘），模拟水雾升腾
    /// - 命中时沿法线泼洒 WaterDropProj
    /// - 本体使用贴图 + 发光层
    ///
    /// 行为组合：
    /// - LiquidTrailBehavior: 贴图碎片拖尾（水蓝渐变 + 浮力）
    /// - NormalBurstBehavior: 命中时沿法线泼洒水滴
    /// - GlowDrawBehavior: 发光绘制
    /// - 自定义 OnAI: 三阶段弧线运动
    /// </summary>
    public class WaterArcProj : BaseBullet
    {
        /// <summary>侧向发射速度</summary>
        private const float LaunchSpeed = 6f;

        /// <summary>弧线回旋阶段的转向速率（弧度/帧）</summary>
        private const float TurnRate = 0.025f;

        /// <summary>弧线回旋阶段持续时间（帧）</summary>
        private const int ArcDuration = 45;

        /// <summary>直线冲刺速度</summary>
        private const float DashSpeed = 14f;

        /// <summary>最大存活时间（帧）</summary>
        private const int MaxLife = 120;

        /// <summary>当前阶段：0=侧向发射，1=弧线回旋，2=直线冲刺</summary>
        private int _phase = 0;

        /// <summary>阶段计时器</summary>
        private int _phaseTimer = 0;

        /// <summary>初始侧向方向</summary>
        private Vector2 _sideDirection;

        /// <summary>当前飞行方向</summary>
        private Vector2 _currentDirection;

        /// <summary>拖尾行为引用（用于动态调整）</summary>
        private LiquidTrailBehavior _trailBehavior;

        protected override void RegisterBehaviors()
        {
            // 1. 贴图碎片拖尾（与火焰同款 LiquidTrailBehavior，水蓝渐变 + 浮力）
            _trailBehavior = new LiquidTrailBehavior
            {
                MaxFragments = 50,
                FragmentLife = 22,
                SizeMultiplier = 0.7f,
                SpawnInterval = 1,
                AdaptiveDensity = true,
                AdaptiveSpeedThreshold = 4f,
                AdaptiveDensityFactor = 5f,
                AdaptiveLife = true,
                AdaptiveTargetLength = 80f,
                SpeedLifeExponent = 0.35f,
                MinFragmentLife = 5,
                ColorStart = new Color(40, 140, 255, 240),
                ColorEnd = new Color(20, 60, 180, 0),
                Buoyancy = 0.04f,
                AirResistance = 0.96f,
                InertiaFactor = 0.35f,
                SplashFactor = 0.15f,
                SplashAngle = 0.4f,
                RandomSpread = 0.6f,
                AutoDraw = true,
                SuppressDefaultDraw = false
            };
            Behaviors.Add(_trailBehavior);

            // 2. 发光绘制
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(80, 180, 255, 120),
                GlowBaseScale = 1.3f,
                GlowLayers = 2,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = true,
                LightColor = new Vector3(0.1f, 0.3f, 0.7f)
            });

            // 3. 法线崩解（命中时沿法线泼洒水滴）
            Behaviors.Add(new NormalBurstBehavior
            {
                ChildProjectileType = ModContent.ProjectileType<WaterDropProj>(),
                Count = 14,
                SpeedMin = 3f,
                SpeedMax = 8f,
                SpreadRadius = 6f,
                SpreadAngle = 0.5f,
                SideAngle = 1.0f,
                BackSplashChance = 0.02f,
                SpawnExtraDust = true,
                ExtraDustCount = 12,
                DustType = DustID.MagicMirror,
                DustColorStart = new Color(40, 140, 255, 220),
                DustColorEnd = new Color(20, 60, 180, 0),
                DustScaleMin = 0.4f,
                DustScaleMax = 0.9f,
                DustSpeedMin = 1f,
                DustSpeedMax = 4f,
                DustNoGravity = false,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            base.OnSpawned(source);

            _phase = 0;
            _phaseTimer = 0;

            // 计算侧向方向：垂直于鼠标方向
            Vector2 toMouse = Main.MouseWorld - Projectile.Center;
            if (toMouse != Vector2.Zero)
            {
                toMouse.Normalize();
                // 随机选择左或右侧向
                float sideSign = Main.rand.NextBool() ? 1f : -1f;
                _sideDirection = new Vector2(-toMouse.Y * sideSign, toMouse.X * sideSign);
                _sideDirection.Normalize();
            }
            else
            {
                _sideDirection = Vector2.UnitY;
            }

            _currentDirection = _sideDirection;
            Projectile.velocity = _currentDirection * LaunchSpeed;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        protected override void OnAI()
        {
            _phaseTimer++;

            switch (_phase)
            {
                case 0:
                    UpdateLaunch();
                    break;
                case 1:
                    UpdateArc();
                    break;
                case 2:
                    UpdateDash();
                    break;
            }

            // 水光
            float lightIntensity = 0.3f + (_phase == 2 ? 0.3f : 0f);
            Lighting.AddLight(Projectile.Center, lightIntensity * 0.1f, lightIntensity * 0.3f, lightIntensity * 0.7f);

            // 动态调整拖尾：冲刺阶段更浓烈
            if (_trailBehavior != null && _trailBehavior.Trail != null)
            {
                if (_phase == 2)
                {
                    _trailBehavior.Trail.SizeMultiplier = 0.9f;
                    _trailBehavior.Trail.FragmentLife = 25;
                }
                else
                {
                    _trailBehavior.Trail.SizeMultiplier = 0.7f;
                    _trailBehavior.Trail.FragmentLife = 22;
                }
            }
        }

        /// <summary>
        /// 阶段0：侧向发射 — 沿垂直于鼠标的方向飞出
        /// </summary>
        private void UpdateLaunch()
        {
            // 保持侧向飞行，持续 15 帧后进入弧线回旋
            Projectile.velocity = _currentDirection * LaunchSpeed;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (_phaseTimer >= 15)
            {
                _phase = 1;
                _phaseTimer = 0;
            }
        }

        /// <summary>
        /// 阶段1：弧线回旋 — 逐渐向鼠标方向弯曲
        /// </summary>
        private void UpdateArc()
        {
            Vector2 toMouse = Main.MouseWorld - Projectile.Center;
            if (toMouse != Vector2.Zero)
            {
                Vector2 desiredDir = toMouse.SafeNormalize(Vector2.Zero);

                // 指数平滑转向，产生大弧线
                _currentDirection = Vector2.Lerp(_currentDirection, desiredDir, TurnRate);
                _currentDirection.Normalize();
            }

            // 速度随弧线进度逐渐增加（弧线末端加速）
            float progress = (float)_phaseTimer / ArcDuration;
            float speed = MathHelper.Lerp(LaunchSpeed, DashSpeed * 0.7f, progress);
            Projectile.velocity = _currentDirection * speed;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 弧线阶段产生侧向水雾粒子
            if (Main.rand.NextBool(2))
            {
                Vector2 sideOffset = new Vector2(-_currentDirection.Y, _currentDirection.X) * Main.rand.NextFloat(-15f, 15f);
                Vector2 pos = Projectile.Center + sideOffset - _currentDirection * 20f;
                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.MagicMirror,
                    sideOffset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f) + _currentDirection * Main.rand.NextFloat(-1f, 0f),
                    0,
                    new Color(60, 160, 255, 150),
                    Main.rand.NextFloat(0.4f, 0.8f)
                );
                d.noGravity = true;
            }

            if (_phaseTimer >= ArcDuration)
            {
                _phase = 2;
                _phaseTimer = 0;

                // 冲刺方向：指向鼠标
                if (toMouse != Vector2.Zero)
                {
                    _currentDirection = toMouse.SafeNormalize(Vector2.Zero);
                }
                Projectile.velocity = _currentDirection * DashSpeed;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
        }

        /// <summary>
        /// 阶段2：直线冲刺 — 高速飞向鼠标
        /// </summary>
        private void UpdateDash()
        {
            // 保持高速直线飞行
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * DashSpeed;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 冲刺时产生尾迹水雾
            if (Main.rand.NextBool(3))
            {
                Vector2 pos = Projectile.Center - _currentDirection * 25f + Main.rand.NextVector2Circular(8f, 8f);
                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.MagicMirror,
                    Main.rand.NextVector2Circular(1f, 1f),
                    0,
                    new Color(80, 200, 255, 180),
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                d.noGravity = true;
            }
        }

        protected override void OnKilled(int timeLeft)
        {
            // 额外水花爆裂（NormalBurstBehavior 已处理主要泼洒）
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 3f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.MagicMirror,
                    vel,
                    0,
                    new Color(60, 180, 255, 150),
                    Main.rand.NextFloat(0.3f, 0.6f)
                );
                d.noGravity = true;
            }
        }

        /// <summary>
        /// 碰到物块时销毁。
        /// </summary>
        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}