using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

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
    // GuWorldSystem - 世界级数据持有者（已弃用）
    // 保存所有家族状态
    // D-01: 已合并到 WorldStateMachine，此类作为转发器保留
    // ============================================================
    [System.Obsolete("请使用 WorldStateMachine 替代 GuWorldSystem")]
    public class GuWorldSystem : ModSystem
    {
        /// <summary> 所有已知家族 </summary>
        public static Dictionary<FactionID, FactionState> AllFactions = new();

        /// <summary> 家族显示名称映射 </summary>
        public static string GetFactionDisplayName(FactionID id) => id switch
        {
            FactionID.GuYue => "古月家族",
            FactionID.Bai => "白家",
            FactionID.Xiong => "熊家",
            FactionID.Tie => "铁家",
            FactionID.Bai2 => "百家",
            FactionID.Wang => "汪家",
            FactionID.Zhao => "赵家",
            FactionID.Jia => "贾家",
            FactionID.Scattered => "散修",
            _ => "未知"
        };

        /// <summary> 获取家族之间关系值 </summary>
        public static int GetRelation(FactionID a, FactionID b)
        {
            if (a == b) return 100;
            if (!AllFactions.ContainsKey(a) || !AllFactions[a].Relations.ContainsKey(b))
                return 0;
            return AllFactions[a].Relations[b];
        }

        public override void OnWorldLoad()
        {
            // 初始化家族数据
            AllFactions.Clear();
            AllFactions[FID.GuYue] = new FactionState(FID.GuYue, "古月家族", "GuYueTerritory");
            AllFactions[FID.Bai] = new FactionState(FID.Bai, "白家", "BaiTerritory");
            AllFactions[FID.Xiong] = new FactionState(FID.Xiong, "熊家", "XiongTerritory");
            AllFactions[FID.Tie] = new FactionState(FID.Tie, "铁家", "TieTerritory");
            AllFactions[FID.Bai2] = new FactionState(FID.Bai2, "百家", "Bai2Territory");
            AllFactions[FID.Wang] = new FactionState(FID.Wang, "汪家", "WangTerritory");
            AllFactions[FID.Zhao] = new FactionState(FID.Zhao, "赵家", "ZhaoTerritory");
            AllFactions[FID.Jia] = new FactionState(FID.Jia, "贾家", null);

            // 初始化家族间关系（基于小说设定）
            SetDefaultRelations();
        }

        private void SetDefaultRelations()
        {
            // 古月与周边家族的关系
            SetRelation(FID.GuYue, FID.Bai, 30);
            SetRelation(FID.GuYue, FID.Xiong, -20);
            SetRelation(FID.GuYue, FID.Tie, 0);

            // 白家
            SetRelation(FID.Bai, FID.GuYue, 30);
            SetRelation(FID.Bai, FID.Tie, 10);

            // 熊家
            SetRelation(FID.Xiong, FID.GuYue, -20);
            SetRelation(FID.Xiong, FID.Tie, -10);
        }

        private void SetRelation(FactionID a, FactionID b, int value)
        {
            if (AllFactions.ContainsKey(a))
                AllFactions[a].Relations[b] = value;
            if (AllFactions.ContainsKey(b))
                AllFactions[b].Relations[a] = value; // 默认对称
        }

        public override void SaveWorldData(TagCompound tag)
        {
            // 保存家族间关系变化（使用 List<TagCompound> 替代嵌套 Dictionary）
            var relationsData = new List<TagCompound>();
            foreach (var (fid, state) in AllFactions)
            {
                foreach (var (otherId, val) in state.Relations)
                {
                    relationsData.Add(new TagCompound
                    {
                        ["factionA"] = fid.ToString(),
                        ["factionB"] = otherId.ToString(),
                        ["value"] = val
                    });
                }
            }
            tag["factionRelations"] = relationsData;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.TryGet("factionRelations", out List<TagCompound> relationsData))
            {
                foreach (var entry in relationsData)
                {
                    var aStr = entry.GetString("factionA");
                    var bStr = entry.GetString("factionB");
                    var val = entry.GetInt("value");
                    if (System.Enum.TryParse<FactionID>(aStr, out var fidA) && AllFactions.ContainsKey(fidA)
                        && System.Enum.TryParse<FactionID>(bStr, out var fidB))
                    {
                        AllFactions[fidA].Relations[fidB] = val;
                    }
                }
            }
        }
    }

    }
