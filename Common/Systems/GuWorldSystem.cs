using System.Collections.Generic;

namespace VerminLordMod.Common.Systems
{
    // 简写别名
    using FID = FactionID;

    // ============================================================
    // 蛊世界模拟核心 - 枚举与数据结构定义
    // 这是整个家族/势力/蛊师体系的骨架层
    // ============================================================

    /// <summary> 家族/势力ID </summary>
    public enum FactionID
    {
        None = 0,
        GuYue,      // 古月家族
        Bai,        // 白家
        Xiong,      // 熊家
        Tie,        // 铁家
        Bai2,       // 百家
        Wang,       // 汪家
        Zhao,       // 赵家
        Jia,        // 贾家（商队）
        Scattered,  // 散修（无势力）
    }

    /// <summary> 声望等级 </summary>
    public enum RepLevel
    {
        Hostile = -2,       // 敌对 - NPC主动攻击，发布悬赏
        Unfriendly = -1,    // 不友好 - NPC不交易，可能嘲讽
        Neutral = 0,        // 中立 - 默认，正常交易
        Friendly = 1,       // 友好 - 折扣交易，提供情报
        Allied = 2,         // 盟友 - 特殊服务，共同作战
    }

    /// <summary> 玩家与NPC的交互类型 </summary>
    public enum InteractionType
    {
        None,
        Talk,           // 对话
        Trade,          // 交易
        Attack,         // 攻击
        Provoke,        // 挑衅
        Ally,           // 结盟
        Betray,         // 背刺
        Bounty,         // 悬赏
        Intimidate,     // 威吓
        Bribe,          // 贿赂
        RequestQuest,   // 请求任务
    }

    /// <summary> 世界事件类型 </summary>
    public enum WorldEventType
    {
        None,
        FactionWar,         // 家族战争
        GuMasterHunt,       // 蛊师猎杀
        TreasureAppears,    // 宝藏现世
        FactionMeeting,     // 家族集会
        MerchantCaravan,    // 商队到来
    }

    /// <summary> 蛊师修为等级（一转~九转） </summary>
    public enum GuRank
    {
        Zhuan1_Chu = 10,      // 一转初阶
        Zhuan1_Zhong = 11,    // 一转中阶
        Zhuan1_Gao = 12,      // 一转高阶
        Zhuan1_DianFeng = 13, // 一转巅峰
        Zhuan2_Chu = 20,
        Zhuan2_Zhong = 21,
        Zhuan2_Gao = 22,
        Zhuan2_DianFeng = 23,
        Zhuan3_Chu = 30,
        Zhuan3_Zhong = 31,
        Zhuan3_Gao = 32,
        Zhuan3_DianFeng = 33,
        Zhuan4_Chu = 40,
        Zhuan4_Zhong = 41,
        Zhuan4_Gao = 42,
        Zhuan4_DianFeng = 43,
        Zhuan5_Chu = 50,
        Zhuan5_Zhong = 51,
        Zhuan5_Gao = 52,
        Zhuan5_DianFeng = 53,
    }

    /// <summary> 蛊师性格类型（影响AI决策） </summary>
    public enum GuPersonality
    {
        Neutral,        // 中立
        Aggressive,     // 好斗 - 容易主动攻击
        Cautious,       // 谨慎 - 打不过就跑
        Greedy,         // 贪婪 - 对宝物敏感
        Loyal,          // 忠诚 - 对家族忠诚
        Treacherous,    // 反复无常 - 可能背刺
        Benevolent,     // 仁慈 - 不会下杀手
        Proud,          // 高傲 - 被挑衅必反击
        Cruel,          // 残忍 - 以折磨敌人为乐
        Cold,           // 冷酷 - 无情无义，只看利益
    }

    /// <summary> 蛊师对玩家的态度（动态计算） </summary>
    public enum GuAttitude
    {
        Ignore,         // 无视
        Friendly,       // 友好
        Wary,           // 警惕
        Hostile,        // 敌对
        Fearful,        // 恐惧
        Respectful,     // 尊敬
        Contemptuous,   // 轻蔑
    }

    /// <summary> 一次交互的结果 </summary>
    public struct InteractionResult
    {
        public bool Success;
        public string Message;
        public int RepChange;           // 声望变化
        public int FactionRepChange;    // 对其他家族的影响
        public bool TriggerCombat;      // 是否触发战斗
        public bool TriggerBounty;      // 是否触发悬赏
    }

    /// <summary> 家族状态数据 </summary>
    public class FactionState
    {
        public FactionID ID;
        public string DisplayName;
        public string TerritorySubworld;    // 对应的小世界名称
        public Dictionary<FactionID, int> Relations = new(); // 对其他家族的好感度 [-100, 100]

        public FactionState(FactionID id, string name, string subworld)
        {
            ID = id;
            DisplayName = name;
            TerritorySubworld = subworld;
        }
    }

    // ============================================================
    // GuWorldSystem - 已废弃，仅保留枚举定义
    // D-01: 所有功能已合并到 WorldStateMachine
    // ============================================================
}
