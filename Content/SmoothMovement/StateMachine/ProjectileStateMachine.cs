using System;
using System.Collections.Generic;

namespace VerminLordMod.Content.SmoothMovement.StateMachine
{
    /// <summary>
    /// 通用弹幕状态机
    /// 
    /// 对应星河蛊的三态机模式：
    ///   0: Orbit（轨道环绕）
    ///   1: Chase（追击）
    ///   2: Return（归位返回）
    /// 
    /// 支持任意数量的状态，每个状态有进入(OnEnter)和更新(OnUpdate)回调
    /// 状态切换支持条件判断和自动过渡
    /// </summary>
    public class ProjectileStateMachine
    {
        private readonly Dictionary<int, StateDefinition> states = new Dictionary<int, StateDefinition>();
        private int currentState;
        private int previousState;
        private float stateTimer;

        /// <summary>当前状态ID</summary>
        public int CurrentState => currentState;

        /// <summary>上一个状态ID</summary>
        public int PreviousState => previousState;

        /// <summary>当前状态已持续的帧数</summary>
        public float StateTimer => stateTimer;

        /// <summary>
        /// 注册一个状态
        /// </summary>
        /// <param name="stateId">状态ID（整数标识符）</param>
        /// <param name="onEnter">进入该状态时调用</param>
        /// <param name="onUpdate">每帧更新时调用，返回新的状态ID（-1表示保持当前状态）</param>
        public void RegisterState(int stateId, Action onEnter = null, Func<int> onUpdate = null)
        {
            states[stateId] = new StateDefinition
            {
                OnEnter = onEnter,
                OnUpdate = onUpdate
            };
        }

        /// <summary>
        /// 切换到指定状态
        /// </summary>
        public void TransitionTo(int newState)
        {
            if (!states.ContainsKey(newState))
                return;

            previousState = currentState;
            currentState = newState;
            stateTimer = 0f;

            states[newState].OnEnter?.Invoke();
        }

        /// <summary>
        /// 每帧调用，驱动状态更新
        /// </summary>
        public void Update()
        {
            stateTimer++;

            if (states.TryGetValue(currentState, out var def) && def.OnUpdate != null)
            {
                int nextState = def.OnUpdate();
                if (nextState >= 0 && nextState != currentState)
                {
                    TransitionTo(nextState);
                }
            }
        }

        /// <summary>
        /// 强制设置状态（不触发OnEnter）
        /// </summary>
        public void SetState(int state)
        {
            if (!states.ContainsKey(state))
                return;
            previousState = currentState;
            currentState = state;
            stateTimer = 0f;
        }

        private class StateDefinition
        {
            public Action OnEnter { get; set; }
            public Func<int> OnUpdate { get; set; }
        }
    }
}
