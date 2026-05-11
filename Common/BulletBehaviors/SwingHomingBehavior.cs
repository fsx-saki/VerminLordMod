using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 摆动追踪行为 — 弹幕以小角度左右摆动，同时以弧线追踪鼠标指针方向。
    ///
    /// 设计思路：
    /// - 弹幕沿初始方向飞行，同时叠加一个正弦波横向摆动（小角度）
    /// - 摆动的同时，弹幕的基准方向会逐渐转向鼠标指针（弧线追踪）
    /// - 追踪使用指数平滑插值，产生平滑的弧线效果
    /// - 适用于"水蛇"、"游鱼"等有生命感的弹幕
    ///
    /// ArcMode（圆弧模式）：
    /// - 启用后不再使用正弦摆动 + 渐进追踪
    /// - 改为计算过当前位置和鼠标位置的圆弧轨迹
    /// - 弹幕沿圆弧以恒定速率弯曲，保证大角度通过鼠标
    ///
    /// 与 HomingBehavior 的区别：
    /// - HomingBehavior 直接追踪最近的敌人，追踪路径较直
    /// - SwingHomingBehavior 追踪鼠标位置，叠加摆动，路径呈 S 形弧线
    /// </summary>
    public class SwingHomingBehavior : IBulletBehavior
    {
        public string Name => "SwingHoming";

        /// <summary>飞行速度（像素/帧）</summary>
        public float Speed { get; set; } = 8f;

        /// <summary>摆动幅度（弧度），控制左右摆动的最大角度</summary>
        public float SwingAmplitude { get; set; } = 0.15f;

        /// <summary>摆动频率，控制摆动快慢</summary>
        public float SwingFrequency { get; set; } = 0.04f;

        /// <summary>追踪权重（0~1），越高追踪鼠标越灵敏。推荐 0.02~0.05 产生平滑弧线</summary>
        public float TrackingWeight { get; set; } = 0.03f;

        /// <summary>追踪范围（像素），0 表示无限</summary>
        public float Range { get; set; } = 0f;

        /// <summary>是否自动旋转到速度方向</summary>
        public bool AutoRotate { get; set; } = true;

        /// <summary>旋转偏移量（弧度）</summary>
        public float RotationOffset { get; set; } = MathHelper.PiOver2;

        /// <summary>是否启用光照</summary>
        public bool EnableLight { get; set; } = false;

        /// <summary>光照颜色</summary>
        public Vector3 LightColor { get; set; } = Vector3.Zero;

        /// <summary>启用圆弧模式：计算过当前位置和鼠标的圆弧轨迹，大角度弯曲通过鼠标</summary>
        public bool UseArcMode { get; set; } = false;

        /// <summary>圆弧最小曲率（1/像素），确保弧线始终可见。0 表示不强制</summary>
        public float ArcMinCurvature { get; set; } = 0.003f;

        /// <summary>摆动相位偏移，使同一帧发射的多枚弹幕摆动不同步</summary>
        private float _phaseOffset;

        /// <summary>初始方向（发射时的鼠标方向），用于摆动基准</summary>
        private Vector2 _initialDirection;

        /// <summary>当前基准方向（逐渐转向鼠标）</summary>
        private Vector2 _baseDirection;

        /// <summary>是否已初始化</summary>
        private bool _initialized;

        public SwingHomingBehavior() { }

        public SwingHomingBehavior(float speed, float swingAmplitude = 0.15f, float swingFrequency = 0.04f, float trackingWeight = 0.03f)
        {
            Speed = speed;
            SwingAmplitude = swingAmplitude;
            SwingFrequency = swingFrequency;
            TrackingWeight = trackingWeight;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            // 记录初始方向
            if (projectile.velocity != Vector2.Zero)
            {
                _initialDirection = projectile.velocity.SafeNormalize(Vector2.Zero);
                _baseDirection = _initialDirection;
            }
            else
            {
                _initialDirection = Vector2.UnitX;
                _baseDirection = Vector2.UnitX;
            }

            // 随机相位偏移，使多枚弹幕摆动不同步
            _phaseOffset = Main.rand.NextFloat(0f, MathHelper.TwoPi);
            _initialized = true;
        }

        public void Update(Projectile projectile)
        {
            if (!_initialized)
                return;

            Vector2 mousePos = Main.MouseWorld;

            if (UseArcMode)
            {
                UpdateArcMode(projectile, mousePos);
            }
            else
            {
                UpdateSwingMode(projectile, mousePos);
            }

            if (AutoRotate && projectile.velocity != Vector2.Zero)
            {
                projectile.rotation = projectile.velocity.ToRotation() + RotationOffset;
            }

            if (EnableLight && LightColor != Vector3.Zero)
            {
                Lighting.AddLight(projectile.Center, LightColor.X, LightColor.Y, LightColor.Z);
            }
        }

        private void UpdateSwingMode(Projectile projectile, Vector2 mousePos)
        {
            Vector2 toMouse = mousePos - projectile.Center;

            if (toMouse != Vector2.Zero)
            {
                Vector2 desiredDir = toMouse.SafeNormalize(Vector2.Zero);

                float dist = toMouse.Length();
                if (Range <= 0f || dist <= Range)
                {
                    _baseDirection = Vector2.Lerp(_baseDirection, desiredDir, TrackingWeight);
                    _baseDirection = _baseDirection.SafeNormalize(Vector2.Zero);
                }
            }

            float swingAngle = (float)System.Math.Sin(Main.GameUpdateCount * SwingFrequency + _phaseOffset) * SwingAmplitude;
            Vector2 swingDirection = _baseDirection.RotatedBy(swingAngle);

            projectile.velocity = swingDirection * Speed;
        }

        private void UpdateArcMode(Projectile projectile, Vector2 mousePos)
        {
            Vector2 toMouse = mousePos - projectile.Center;
            float dist = toMouse.Length();

            if (dist < 4f)
            {
                if (projectile.velocity.Length() > 0.01f)
                    projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * Speed;
                return;
            }

            Vector2 currentDir = projectile.velocity.SafeNormalize(Vector2.Zero);
            if (currentDir == Vector2.Zero)
                currentDir = toMouse.SafeNormalize(Vector2.Zero);

            Vector2 perp = new Vector2(-currentDir.Y, currentDir.X);
            float perpDotV = Vector2.Dot(perp, toMouse);

            float radius;
            if (System.Math.Abs(perpDotV) > 0.01f)
            {
                radius = (dist * dist) / (2f * perpDotV);

                float absRadius = System.Math.Abs(radius);
                float maxRadius = dist * 5f;
                float minRadius = ArcMinCurvature > 0f ? 1f / ArcMinCurvature : dist * 0.15f;
                if (minRadius > dist * 0.15f)
                    minRadius = dist * 0.15f;
                if (minRadius > maxRadius)
                    minRadius = maxRadius * 0.5f;

                absRadius = MathHelper.Clamp(absRadius, minRadius, maxRadius);
                radius = absRadius * System.Math.Sign(perpDotV);
            }
            else
            {
                radius = dist * 5f;
                if (System.Math.Abs(radius) < 1f)
                    radius = 1000f;
            }

            float angularStep = Speed / System.Math.Abs(radius);
            float curveSign = System.Math.Sign(radius);

            projectile.velocity = projectile.velocity.RotatedBy(angularStep * curveSign);
            if (projectile.velocity.Length() > 0.01f)
                projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * Speed;
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
