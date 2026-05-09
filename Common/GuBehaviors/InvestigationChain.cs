using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Common.GuBehaviors
{
    /// <summary>
    /// 调查链状态（D-19）。
    ///
    /// NPC 对玩家的态度变化从"未察觉→可疑→确认→行动"渐进发展，
    /// 而非即时切换。这为玩家提供了反应时间和策略空间。
    /// </summary>
    public class InvestigationState
    {
        /// <summary> 怀疑度 [0.0, 1.0] </summary>
        public float SuspicionLevel;

        /// <summary> 观察次数 </summary>
        public int ObservationCount;

        /// <summary> 观察到的行为列表 </summary>
        public List<string> ObservedActions = new();

        /// <summary> 最后观察的游戏日 </summary>
        public int LastObservationDay;

        /// <summary> 是否处于可疑状态 </summary>
        public bool IsSuspicious => SuspicionLevel > 0.3f;

        /// <summary> 是否已确认 </summary>
        public bool IsConfirmed => SuspicionLevel > 0.7f;

        /// <summary> 是否已采取行动 </summary>
        public bool IsActing => SuspicionLevel >= 1.0f;

        /// <summary>
        /// 观察到某个行为，增加怀疑度。
        /// </summary>
        /// <param name="action">行为描述</param>
        /// <param name="weight">权重 [0, 1]</param>
        public void Observe(string action, float weight)
        {
            ObservedActions.Add(action);
            ObservationCount++;
            SuspicionLevel = System.Math.Min(1f, SuspicionLevel + weight);
            LastObservationDay = (int)(Main.time / 36000); // 游戏日
        }

        /// <summary>
        /// 随时间衰减怀疑度。
        /// </summary>
        /// <param name="days">经过的游戏天数</param>
        public void DecayOverTime(int days)
        {
            if (days <= 0) return;

            // 每天衰减 0.05，但不会低于 0
            float decay = days * 0.05f;
            SuspicionLevel = System.Math.Max(0f, SuspicionLevel - decay);

            // 如果怀疑度降到 0.3 以下，清除观察记录
            if (SuspicionLevel <= 0.3f)
            {
                ObservedActions.Clear();
                ObservationCount = 0;
            }
        }

        /// <summary>
        /// 重置状态。
        /// </summary>
        public void Reset()
        {
            SuspicionLevel = 0f;
            ObservationCount = 0;
            ObservedActions.Clear();
            LastObservationDay = 0;
        }
    }

    /// <summary>
    /// 调查链系统（D-19）。
    ///
    /// 管理所有 NPC 对玩家的调查状态，提供统一的查询和更新接口。
    /// </summary>
    public class InvestigationChainSystem : ModSystem
    {
        /// <summary>
        /// 调查状态缓存：NPC.whoAmI → InvestigationState
        /// </summary>
        public Dictionary<int, InvestigationState> NPCInvestigations = new();

        public override void OnWorldLoad()
        {
            NPCInvestigations.Clear();
        }

        public override void OnWorldUnload()
        {
            NPCInvestigations.Clear();
        }

        /// <summary>
        /// 获取或创建指定 NPC 对玩家的调查状态。
        /// </summary>
        public InvestigationState GetOrCreateState(int npcWhoAmI)
        {
            if (!NPCInvestigations.TryGetValue(npcWhoAmI, out var state))
            {
                state = new InvestigationState();
                NPCInvestigations[npcWhoAmI] = state;
            }
            return state;
        }

        /// <summary>
        /// NPC 观察到玩家的某个行为。
        /// </summary>
        public void ReportObservation(int npcWhoAmI, string action, float weight)
        {
            var state = GetOrCreateState(npcWhoAmI);
            state.Observe(action, weight);
        }

        /// <summary>
        /// 每日衰减所有 NPC 的调查度。
        /// </summary>
        public void DailyDecay()
        {
            foreach (var kvp in NPCInvestigations)
            {
                int currentDay = (int)(Main.time / 36000);
                int daysSinceLastObs = currentDay - kvp.Value.LastObservationDay;
                if (daysSinceLastObs > 0)
                {
                    kvp.Value.DecayOverTime(daysSinceLastObs);
                }
            }
        }

        /// <summary>
        /// 获取 NPC 对玩家的态度修正（基于调查状态）。
        /// </summary>
        public float GetAttitudeModifier(int npcWhoAmI)
        {
            if (NPCInvestigations.TryGetValue(npcWhoAmI, out var state))
            {
                if (state.IsActing) return -1.0f;    // 已行动：极端敌对
                if (state.IsConfirmed) return -0.5f; // 已确认：显著敌对
                if (state.IsSuspicious) return -0.2f; // 可疑：轻微敌对
            }
            return 0f;
        }

        /// <summary>
        /// 清除指定 NPC 的调查状态（如 NPC 死亡或玩家使用特殊手段）。
        /// </summary>
        public void ClearState(int npcWhoAmI)
        {
            NPCInvestigations.Remove(npcWhoAmI);
        }
    }
}
