using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuMasters
{
    // ============================================================
    // IGuMasterAI - 蛊师NPC智能接口
    // 定义所有蛊师NPC必须实现的决策/感知/行为方法
    // 这是"每个蛊师有自己的逻辑和判断方式"的基础
    // ============================================================

    /// <summary> NPC感知到的环境信息 </summary>
    public struct PerceptionContext
    {
        public Player TargetPlayer;                     // 目标玩家
        public float DistanceToPlayer;                  // 距离
        public int PlayerLifePercent;                   // 玩家生命百分比
        public bool PlayerHasQiEnabled;                 // 玩家是否开启空窍
        public int NearbyAlliesCount;                   // 附近友方NPC数量
        public int NearbyEnemiesCount;                  // 附近敌方NPC数量
        public bool IsInOwnTerritory;                   // 是否在自己地盘
        public bool IsInPlayerTerritory;                // 是否在玩家地盘
        public float TimeOfDay;                         // 当前时间
        public bool IsRaining;                          // 是否下雨
        public bool PlayerIsHoldingValuable;            // 玩家是否持有贵重物品
        public int PlayerInfamy;                        // 玩家恶名值
        public int PlayerQiLevel;                       // 玩家修为等级（用于信念估计）
        public int PlayerDamage;                        // 玩家主手武器伤害（用于信念估计）
    }

    /// <summary> NPC对单个玩家的信念状态（黑暗森林核心） </summary>
    public class BeliefState
    {
        /// <summary> 玩家名称（作为key） </summary>
        public string PlayerName;

        /// <summary> 风险阈值 [0,1]，初始0.9（极度谨慎），越低越激进 </summary>
        public float RiskThreshold = 0.9f;

        /// <summary> 置信度 [0,1]，随观察次数递增 </summary>
        public float ConfidenceLevel = 0f;

        /// <summary> 观察次数 </summary>
        public int ObservationCount = 0;

        /// <summary> 对玩家实力的估计 [0,1] </summary>
        public float EstimatedPower = 0.5f;

        /// <summary> 是否交易过 </summary>
        public bool HasTraded = false;

        /// <summary> 是否战斗过 </summary>
        public bool HasFought = false;

        /// <summary> 是否被该玩家击败过 </summary>
        public bool WasDefeated = false;

        /// <summary> 是否击败过该玩家 </summary>
        public bool HasDefeatedPlayer = false;

        /// <summary> 上次交互的游戏天数 </summary>
        public int LastInteractionDay = 0;

        /// <summary> 生成默认信念（极度谨慎） </summary>
        public static BeliefState Default(string playerName) => new BeliefState
        {
            PlayerName = playerName,
            RiskThreshold = 0.9f,
            ConfidenceLevel = 0f,
            ObservationCount = 0,
            EstimatedPower = 0.5f,
            HasTraded = false,
            HasFought = false,
            WasDefeated = false,
            HasDefeatedPlayer = false,
            LastInteractionDay = 0
        };
    }

    /// <summary> NPC的决策结果 </summary>
    public struct Decision
    {
        public GuMasterAIState NewState;                // 新状态
        public InteractionType Interaction;             // 交互类型
        public string DialogueLine;                     // 对话文本
        public bool ShouldAttack;                       // 是否攻击
        public bool ShouldFlee;                         // 是否逃跑
        public bool ShouldCallForHelp;                  // 是否呼叫支援
        public Vector2 MoveTarget;                      // 移动目标
    }

    /// <summary> 蛊师AI状态 </summary>
    public enum GuMasterAIState
    {
        Idle,           // 闲逛
        Patrol,         // 巡逻
        Investigate,    // 调查（感知到异常）
        Approach,       // 接近玩家
        Talk,           // 对话
        Trade,          // 交易
        Combat,         // 战斗
        Flee,           // 逃跑
        CallForHelp,    // 呼叫支援
        Return,         // 归位
    }

    /// <summary> 蛊师对玩家的态度计算上下文 </summary>
    public struct AttitudeContext
    {
        public GuWorldPlayer WorldPlayer;
        public FactionID NpcFaction;
        public GuPersonality Personality;
        public GuRank Rank;
        public int NpcLevel;            // NPC等级（影响态度判断）
        public bool HasBeenHitByPlayer; // 是否被玩家攻击过
        public int AggroTimer;          // 仇恨计时器
        public BeliefState Belief;      // 对当前玩家的信念状态
    }

    // ============================================================
    // IGuMasterAI 接口
    // 所有蛊师NPC必须实现此接口
    // ============================================================
    public interface IGuMasterAI
    {
        // ===== 感知 =====

        /// <summary> 感知环境，返回当前上下文 </summary>
        PerceptionContext Perceive(NPC npc);

        // ===== 信念 =====

        /// <summary> 更新对当前目标玩家的信念（每帧调用） </summary>
        void UpdateBelief(NPC npc, PerceptionContext context);

        /// <summary> 获取对指定玩家的信念状态 </summary>
        BeliefState GetBelief(string playerName);

        // ===== 决策 =====

        /// <summary> 基于感知做出决策 </summary>
        Decision Decide(NPC npc, PerceptionContext context);

        /// <summary> 计算对玩家的态度（基于信念分布） </summary>
        GuAttitude CalculateAttitude(NPC npc, AttitudeContext context);

        // ===== 行为 =====

        /// <summary> 执行当前状态的AI逻辑 </summary>
        void ExecuteAI(NPC npc, Decision decision);

        /// <summary> 处理与玩家的交互 </summary>
        InteractionResult HandleInteraction(NPC npc, Player player, InteractionType interaction);

        // ===== 对话 =====

        /// <summary> 获取对话文本（基于当前态度和上下文） </summary>
        string GetDialogue(NPC npc, GuAttitude attitude);

        /// <summary> 获取对话按钮 </summary>
        void GetChatButtons(NPC npc, ref string button, ref string button2);

        /// <summary> 处理对话按钮点击 </summary>
        void OnChatButtonClicked(NPC npc, bool firstButton, ref string shop);
    }

    // ============================================================
    // 工具方法 - 态度计算（基于信念）
    //
    // 架构说明（重要）：
    //
    // 势力声望（FactionReputation）和个体信念（BeliefState）是两个独立系统：
    //
    // ┌─────────────────────────────────────────────────────────────┐
    // │  势力声望系统 (GuWorldPlayer.FactionRelations)              │
    // │  ├─ 商店价格影响                                           │
    // │  ├─ 悬赏发布/撤销                                          │
    // │  ├─ 结盟/背刺                                             │
    // │  └─ 势力间连锁反应                                         │
    // │  作用范围: 势力层面                                         │
    // └─────────────────────────────────────────────────────────────┘
    //                            ↑ 独立运行，互不干扰
    // ┌─────────────────────────────────────────────────────────────┐
    // │  个体信念系统 (GuMasterBase.PlayerBeliefs[playerName])      │
    // │  ├─ RiskThreshold: 风险阈值（越低越激进）                    │
    // │  ├─ ConfidenceLevel: 置信度                                 │
    // │  ├─ EstimatedPower: 对玩家实力的估计                         │
    // │  ├─ WasDefeated / HasDefeatedPlayer                         │
    // │  └─ HasTraded / HasFought                                   │
    // │  作用范围: 单个NPC对单个玩家的判断                            │
    // │  决定: 攻击/逃跑/观望/对话                                   │
    // └─────────────────────────────────────────────────────────────┘
    //
    // CalculateFromBelief() 只使用 BeliefState + Personality，
    // 不使用 FactionReputation。这是设计上故意的。
    // 即使玩家被某家族悬赏，个体NPC仍然用自己的信念来判断。
    // 悬赏只在势力层面起作用（商店关闭、家族事件等）。
    // ============================================================
    public static class GuAttitudeHelper
    {
        /// <summary>
        /// 基于信念分布计算态度（替代原来的确定性公式）
        ///
        /// 核心逻辑：
        /// - RiskThreshold < 0.3 → Hostile（主动攻击）
        /// - RiskThreshold 0.3~0.5 → Wary（试探）
        /// - RiskThreshold 0.5~0.7 → Ignore（观望）
        /// - RiskThreshold 0.7~0.9 → Fearful（回避）
        /// - RiskThreshold > 0.9 → Flee（逃跑）
        /// - ConfidenceLevel 低时 → 加入随机偏移，行为不可预测
        /// </summary>
        public static GuAttitude CalculateFromBelief(BeliefState belief, GuPersonality personality, bool hasBeenHit)
        {
            // 被攻击过 → 强制敌对（短期）
            if (hasBeenHit) return GuAttitude.Hostile;

            float threshold = belief.RiskThreshold;

            // 性格修正 RiskThreshold
            threshold = personality switch
            {
                GuPersonality.Aggressive => threshold - 0.15f,  // 好斗：更激进
                GuPersonality.Cautious => threshold + 0.15f,     // 谨慎：更保守
                GuPersonality.Greedy when belief.HasTraded => threshold - 0.1f, // 贪婪且知道你有货
                GuPersonality.Proud => threshold - 0.05f,        // 高傲：略激进
                GuPersonality.Benevolent => threshold + 0.2f,    // 仁慈：更保守
                _ => threshold
            };

            // 置信度低时加入随机偏移（行为不可预测）
            if (belief.ConfidenceLevel < 0.3f)
            {
                float randomOffset = (float)(Main.rand.NextDouble() * 0.3 - 0.15);
                threshold += randomOffset;
            }

            // 被击败过 → 大幅提高阈值
            if (belief.WasDefeated)
                threshold += 0.25f;

            // 击败过玩家 → 小幅降低阈值
            if (belief.HasDefeatedPlayer)
                threshold -= 0.1f;

            // 根据阈值决定态度
            threshold = MathHelper.Clamp(threshold, 0f, 1f);

            if (threshold < 0.3f) return GuAttitude.Hostile;
            if (threshold < 0.5f) return GuAttitude.Wary;
            if (threshold < 0.7f) return GuAttitude.Ignore;
            if (threshold < 0.9f) return GuAttitude.Fearful;
            return GuAttitude.Fearful; // >0.9 也是恐惧（逃跑）
        }

        /// <summary>
        /// 更新信念状态（核心方法）
        /// </summary>
        public static void UpdateBeliefState(BeliefState belief, PerceptionContext context, bool wasDefeated, bool defeatedPlayer)
        {
            // 增加观察次数
            belief.ObservationCount++;
            belief.LastInteractionDay = (int)(Main.time / 54000); // 游戏天数

            // 根据玩家生命值估计实力
            float powerFromLife = 1f - (context.PlayerLifePercent / 100f);
            // 根据玩家修为估计实力
            float powerFromQi = context.PlayerQiLevel / 10f;
            // 根据玩家伤害估计实力
            float powerFromDamage = MathHelper.Clamp(context.PlayerDamage / 50f, 0f, 1f);

            float newEstimate = (powerFromLife * 0.3f + powerFromQi * 0.4f + powerFromDamage * 0.3f);
            // 平滑更新估计值
            belief.EstimatedPower = belief.EstimatedPower * 0.7f + newEstimate * 0.3f;

            // 更新 RiskThreshold 动态漂移
            // 玩家看起来弱 → 阈值下降（更激进）
            // 玩家看起来强 → 阈值上升（更谨慎）
            float targetThreshold = 0.9f - (belief.EstimatedPower * 0.6f);
            belief.RiskThreshold = belief.RiskThreshold * 0.8f + targetThreshold * 0.2f;

            // 战斗结果影响
            if (wasDefeated)
            {
                belief.WasDefeated = true;
                belief.RiskThreshold = MathHelper.Min(1f, belief.RiskThreshold + 0.25f);
            }
            if (defeatedPlayer)
            {
                belief.HasDefeatedPlayer = true;
                belief.RiskThreshold = MathHelper.Max(0f, belief.RiskThreshold - 0.1f);
            }

            // 置信度随观察次数递增（但不超过0.9）
            belief.ConfidenceLevel = MathHelper.Min(0.9f, belief.ObservationCount * 0.1f);

            // 边界限制
            belief.RiskThreshold = MathHelper.Clamp(belief.RiskThreshold, 0f, 1f);
            belief.EstimatedPower = MathHelper.Clamp(belief.EstimatedPower, 0f, 1f);
        }

        /// <summary> 根据态度获取默认对话 </summary>
        public static string GetDefaultDialogue(GuAttitude attitude, string npcName)
        {
            return attitude switch
            {
                GuAttitude.Ignore => npcName + "看了你一眼，没有理会。",
                GuAttitude.Friendly => npcName + "微笑着向你点头致意。",
                GuAttitude.Wary => npcName + "警惕地看着你，手按在蛊虫上。",
                GuAttitude.Hostile => npcName + "冷冷地盯着你：\"滚开，否则别怪我不客气！\"",
                GuAttitude.Fearful => npcName + "颤抖着后退了几步。",
                GuAttitude.Respectful => npcName + "恭敬地向你行礼：\"见过大人。\"",
                GuAttitude.Contemptuous => npcName + "轻蔑地瞥了你一眼：\"就凭你？\"",
                _ => npcName + "看了你一眼。"
            };
        }
    }
}
