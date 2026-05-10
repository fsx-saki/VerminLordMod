using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.SmoothMovement.Interpolators;
using VerminLordMod.Content.SmoothMovement.Orbiters;
using VerminLordMod.Content.SmoothMovement.StateMachine;
using VerminLordMod.Content.SmoothMovement.Trackers;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 轨道-追击-归位 三态机行为 — 将 SmoothMovement 三态机封装为 IBulletBehavior。
    /// 
    /// 弹幕会围绕玩家做轨道运动，发现敌人后螺旋追击，超出范围后归位返回。
    /// 完全可配置的轨道器、追踪器、插值器组合。
    /// 
    /// 使用示例：
    ///   Behaviors.Add(new OrbitChaseReturnBehavior(projectile, player)
    ///   {
    ///       Orbiter = new ChaoticOrbiter(initialAngle: 0f, angularSpeed: 0.025f, baseRadius: 50f),
    ///       ChaseTracker = new SpiralApproachTracker(speed: 12f),
    ///       Interpolator = new LerpSmoothInterpolator(transitionSpeed: 0.08f),
    ///       ChaseRange = 260,
    ///       ReturnDistance = 450f,
    ///       HitDistThreshold = 40f,
    ///       ReturnRadiusMargin = 30f
    ///   });
    /// </summary>
    public class OrbitChaseReturnBehavior : IBulletBehavior
    {
        public string Name => "OrbitChaseReturn";

        // ===== 可配置组件 =====

        /// <summary>轨道器（决定轨道运动方式）</summary>
        public IOrbiter Orbiter { get; set; }

        /// <summary>追击追踪器（决定追击运动方式）</summary>
        public ITracker ChaseTracker { get; set; }

        /// <summary>插值器（决定速度平滑方式）</summary>
        public IInterpolator Interpolator { get; set; }

        // ===== 可配置参数 =====

        /// <summary>追击检测范围（像素）。典型值：260</summary>
        public int ChaseRange { get; set; } = 260;

        /// <summary>超出此距离强制返回（像素）。典型值：450</summary>
        public float ReturnDistance { get; set; } = 450f;

        /// <summary>命中判定距离（像素）。典型值：40</summary>
        public float HitDistThreshold { get; set; } = 40f;

        /// <summary>返回完成判定余量（像素）。典型值：20~30</summary>
        public float ReturnRadiusMargin { get; set; } = 30f;

        /// <summary>是否自动旋转到速度方向</summary>
        public bool AutoRotate { get; set; } = true;

        /// <summary>旋转偏移量（弧度）</summary>
        public float RotationOffset { get; set; } = MathHelper.PiOver2;

        // 内部状态
        private OrbitChaseReturnStateMachine _stateMachine;
        private Player _owner;
        private bool _initialized = false;

        public OrbitChaseReturnBehavior() { }

        /// <summary>
        /// 使用默认组件快速构造。
        /// 默认：ChaoticOrbiter + SpiralApproachTracker + LerpSmoothInterpolator
        /// </summary>
        public OrbitChaseReturnBehavior(
            IOrbiter orbiter,
            ITracker chaseTracker,
            IInterpolator interpolator)
        {
            Orbiter = orbiter;
            ChaseTracker = chaseTracker;
            Interpolator = interpolator;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _owner = Main.player[projectile.owner];
            if (_owner == null || !_owner.active)
            {
                _initialized = false;
                return;
            }

            // 使用默认组件（如果未指定）
            var orbiter = Orbiter ?? new ChaoticOrbiter(
                initialAngle: 0f,
                angularSpeed: 0.025f,
                baseRadius: 50f,
                radiusWaveRange: 25f,
                ellipseCompression: 0.6f);

            var tracker = ChaseTracker ?? new SpiralApproachTracker(
                speed: 12f,
                angularFrequency: 0.08f,
                baseAmplitude: 60f,
                spiralRampDuration: 60f);

            var interpolator = Interpolator ?? new LerpSmoothInterpolator(
                transitionSpeed: 0.08f);

            _stateMachine = new OrbitChaseReturnStateMachine(
                projectile, _owner,
                orbiter, tracker, interpolator,
                ChaseRange, ReturnDistance,
                HitDistThreshold, ReturnRadiusMargin);

            _initialized = true;
        }

        public void Update(Projectile projectile)
        {
            if (!_initialized || _stateMachine == null) return;

            Vector2 newVelocity = _stateMachine.Update();
            projectile.velocity = newVelocity;

            // 自动旋转
            if (AutoRotate && projectile.velocity != Vector2.Zero)
            {
                projectile.rotation = projectile.velocity.ToRotation() + RotationOffset;
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;

        /// <summary>获取当前状态ID（0=Orbit, 1=Chase, 2=Return）</summary>
        public int GetCurrentState()
        {
            return _stateMachine?.GetCurrentState() ?? -1;
        }

        /// <summary>获取当前状态持续帧数</summary>
        public float GetStateTimer()
        {
            return _stateMachine?.GetStateTimer() ?? 0f;
        }
    }
}
