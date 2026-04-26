using Microsoft.Xna.Framework;
using VerminLordMod.Content.SmoothMovement.Interpolators;
using VerminLordMod.Content.SmoothMovement.Trackers;
using VerminLordMod.Content.SmoothMovement.Orbiters;
using Terraria;

namespace VerminLordMod.Content.SmoothMovement.StateMachine
{
    /// <summary>
    /// 轨道-追击-归位 三态机（星河蛊风格）
    /// 
    /// 状态说明：
    ///   0: Orbit    - 围绕玩家做不规则轨道运动
    ///   1: Chase    - 发现敌人后螺旋追击
    ///   2: Return   - 追击完成或超出距离后返回轨道
    /// 
    /// 使用方式：
    ///   1. 在弹幕的 OnSpawn 中构造并初始化
    ///   2. 在 AI() 中调用 Update() 获取新的 velocity
    ///   3. 将 velocity 赋给 Projectile.velocity
    /// </summary>
    public class OrbitChaseReturnStateMachine
    {
        // 状态常量
        public const int STATE_ORBIT = 0;
        public const int STATE_CHASE = 1;
        public const int STATE_RETURN = 2;

        // 组件
        private readonly ProjectileStateMachine fsm;
        private readonly IOrbiter orbiter;
        private readonly ITracker chaseTracker;
        private readonly IInterpolator interpolator;
        private readonly Player owner;
        private readonly Projectile projectile;

        // 追击参数
        private readonly int chaseRange;
        private readonly float returnDist;
        private readonly float hitDistThreshold;
        private readonly float returnRadiusMargin;

        // 运行时状态
        private NPC chaseTarget;
        private float returnPhase;

        /// <summary>
        /// 构造轨道-追击-归位三态机
        /// </summary>
        /// <param name="projectile">所属弹幕</param>
        /// <param name="owner">所属玩家</param>
        /// <param name="orbiter">轨道器（决定轨道运动方式）</param>
        /// <param name="chaseTracker">追击追踪器（决定追击运动方式）</param>
        /// <param name="interpolator">插值器（决定速度平滑方式）</param>
        /// <param name="chaseRange">追击检测范围（像素）。典型值：260</param>
        /// <param name="returnDist">超出此距离强制返回（像素）。典型值：450</param>
        /// <param name="hitDistThreshold">命中判定距离（像素）。典型值：40</param>
        /// <param name="returnRadiusMargin">返回完成判定余量（像素）。典型值：20~30</param>
        public OrbitChaseReturnStateMachine(
            Projectile projectile,
            Player owner,
            IOrbiter orbiter,
            ITracker chaseTracker,
            IInterpolator interpolator,
            int chaseRange = 260,
            float returnDist = 450f,
            float hitDistThreshold = 40f,
            float returnRadiusMargin = 30f)
        {
            this.projectile = projectile;
            this.owner = owner;
            this.orbiter = orbiter;
            this.chaseTracker = chaseTracker;
            this.interpolator = interpolator;
            this.chaseRange = chaseRange;
            this.returnDist = returnDist;
            this.hitDistThreshold = hitDistThreshold;
            this.returnRadiusMargin = returnRadiusMargin;

            fsm = new ProjectileStateMachine();
            SetupStates();
        }

        private void SetupStates()
        {
            fsm.RegisterState(STATE_ORBIT, onEnter: null, onUpdate: OrbitUpdate);
            fsm.RegisterState(STATE_CHASE, onEnter: ChaseEnter, onUpdate: ChaseUpdate);
            fsm.RegisterState(STATE_RETURN, onEnter: null, onUpdate: ReturnUpdate);
        }

        /// <summary>
        /// 每帧调用，返回平滑后的速度向量
        /// </summary>
        public Vector2 Update()
        {
            // 检测最近敌人
            NPC nearestEnemy = FindNearestEnemy(chaseRange);

            // 超出返回距离强制返回
            float distToOwner = projectile.Distance(owner.Center);
            if (distToOwner > returnDist && fsm.CurrentState != STATE_RETURN)
            {
                fsm.TransitionTo(STATE_RETURN);
            }

            // 轨道态下发现敌人 → 追击
            if (fsm.CurrentState == STATE_ORBIT && nearestEnemy != null)
            {
                chaseTarget = nearestEnemy;
                fsm.TransitionTo(STATE_CHASE);
            }

            fsm.Update();

            // 根据当前状态计算目标速度
            Vector2 targetVelocity = ComputeTargetVelocity();
            return interpolator.UpdateVelocity(projectile.velocity, targetVelocity);
        }

        private Vector2 ComputeTargetVelocity()
        {
            switch (fsm.CurrentState)
            {
                case STATE_ORBIT:
                {
                    Vector2 orbitTarget = orbiter.GetOrbitTarget(owner.Center);
                    return (orbitTarget - projectile.Center) * 0.15f;
                }
                case STATE_CHASE:
                {
                    if (chaseTarget == null || !chaseTarget.active)
                    {
                        fsm.TransitionTo(STATE_RETURN);
                        return Vector2.Zero;
                    }
                    return chaseTracker.GetTargetVelocity(
                        projectile.Center, projectile.velocity,
                        chaseTarget.Center, chaseTarget.velocity);
                }
                case STATE_RETURN:
                {
                    Vector2 returnTarget = orbiter.GetOrbitTarget(owner.Center);
                    Vector2 dir = returnTarget - projectile.Center;
                    if (dir != Vector2.Zero)
                        dir.Normalize();
                    return dir * 12f; // 返回速度
                }
                default:
                    return Vector2.Zero;
            }
        }

        private int OrbitUpdate()
        {
            return -1; // 保持轨道态
        }

        private void ChaseEnter()
        {
            returnPhase = 0f;
            // 如果追击追踪器有 Reset 方法（如 SpiralApproachTracker）
            if (chaseTracker is SpiralApproachTracker spiral)
                spiral.Reset();
        }

        private int ChaseUpdate()
        {
            if (chaseTarget == null || !chaseTarget.active)
                return STATE_RETURN;

            float distToEnemy = projectile.Distance(chaseTarget.Center);

            if (distToEnemy < hitDistThreshold)
            {
                returnPhase = 1f;
            }

            if (returnPhase >= 1f)
            {
                float distToOwner = projectile.Distance(owner.Center);
                // 获取轨道半径用于判定是否回到轨道
                float orbitRadius = 50f; // 默认值
                if (orbiter is ChaoticOrbiter chaotic)
                    orbitRadius = chaotic.GetCurrentRadius();

                if (distToOwner < orbitRadius + returnRadiusMargin)
                {
                    return STATE_ORBIT;
                }
                return STATE_RETURN;
            }

            return -1; // 保持追击态
        }

        private int ReturnUpdate()
        {
            float distToOwner = projectile.Distance(owner.Center);
            float orbitRadius = 50f;
            if (orbiter is ChaoticOrbiter chaotic)
                orbitRadius = chaotic.GetCurrentRadius();

            if (distToOwner < orbitRadius + returnRadiusMargin + 10f)
            {
                // 同步轨道角度
                if (orbiter is SimpleOrbiter simple)
                {
                    Vector2 offset = projectile.Center - owner.Center;
                    simple.SetAngle((float)System.Math.Atan2(offset.Y, offset.X));
                }
                else if (orbiter is ChaoticOrbiter chaotic2)
                {
                    Vector2 offset = projectile.Center - owner.Center;
                    chaotic2.SetAngle((float)System.Math.Atan2(offset.Y, offset.X));
                }
                return STATE_ORBIT;
            }
            return -1;
        }

        private NPC FindNearestEnemy(float range)
        {
            NPC nearest = null;
            float nearestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = projectile.Distance(npc.Center);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = npc;
                    }
                }
            }
            return nearest;
        }

        /// <summary>获取当前状态ID</summary>
        public int GetCurrentState() => fsm.CurrentState;

        /// <summary>获取当前状态持续帧数</summary>
        public float GetStateTimer() => fsm.StateTimer;
    }
}
