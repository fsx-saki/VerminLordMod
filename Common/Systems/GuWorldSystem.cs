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
        Shang,      // 商家
        Heaven,     // 天庭
        ShadowSect, // 影宗
        LingYuanZhai, // 灵缘斋
        ChangShengTian, // 长生天
        HeiLouLan,   // 黑楼兰
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
        Zhuan6_Chu = 60,
        Zhuan6_Zhong = 61,
        Zhuan6_Gao = 62,
        Zhuan6_DianFeng = 63,
        Zhuan7_Chu = 70,
        Zhuan7_Zhong = 71,
        Zhuan7_Gao = 72,
        Zhuan7_DianFeng = 73,
        Zhuan8_Chu = 80,
        Zhuan8_Zhong = 81,
        Zhuan8_Gao = 82,
        Zhuan8_DianFeng = 83,
        Zhuan9_Chu = 90,
        Zhuan9_Zhong = 91,
        Zhuan9_Gao = 92,
        Zhuan9_DianFeng = 93,
        AncientBeast = 100,
        AncientLegendary = 101,
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
        Cunning,        // 阴险狡诈 - 善于算计，以利益为导向
        Ambitious,      // 志向远大 - 野心勃勃
        Fierce,         // 刚烈 - 宁折不弯
        Gentle,         // 温婉 - 温柔善良
        Mysterious,     // 神秘 - 难以捉摸
        Devoted,        // 痴情 - 一往情深
        DualFaced,      // 双面 - 表里不一
        Arrogant,       // 傲慢 - 看不起他人
        Warlike,        // 好战 - 热衷战斗
        Righteous,      // 正义 - 坚持正道
        Compassionate,  // 慈悲 - 大慈大悲
        Ruthless,       // 狠辣 - 心狠手辣
        FreeSpirited,   // 洒脱 - 不受拘束
        Ferocious,      // 凶暴 - 野蛮凶残
        Vengeful,       // 仇恨 - 被仇恨驱动
        Wise,           // 博学 - 智慧深沉
        Resolute,       // 刚毅 - 意志坚定，不屈不挠
        Steadfast,      // 坚定 - 稳如磐石，不可动摇
        Hypocritical,   // 伪善 - 表面仁义，内心阴暗
        Calculating,    // 算计 - 精于谋划，步步为营
        Reckless,       // 孤注一掷 - 不计后果，全力一搏
        Wild,           // 野性 - 天然野性，不受驯服
        Bold,           // 豪迈 - 豪爽大气，不拘小节
        Clever,         // 聪慧 - 聪明机智，善于应变
        Hidden,         // 隐忍 - 隐藏实力，伺机而动
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
