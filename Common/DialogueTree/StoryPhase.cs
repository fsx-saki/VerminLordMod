using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.DialogueTree
{
    /// <summary>
    /// 剧情阶段枚举 — 定义古月山寨的主线推进阶段。
    ///
    /// 每个阶段对应一组可触发的剧情和可生成的 NPC。
    /// 阶段线性推进，不可跳过，但玩家可以在阶段内自由探索。
    /// </summary>
    public enum StoryPhase
    {
        // === Stage1: 一转·蛊师入门 ===
        NotEntered = 0,
        Arrival = 1,
        AwakeningCeremony = 2,
        SchoolTraining = 3,
        MedicineRequest = 4,
        JiaJinShengDeath = 5,
        HuaJiuInheritance = 6,
        FamilyRecognition = 7,

        // === Stage2: 二转·家族争锋 ===
        PreTournament = 10,
        TournamentBegin = 11,
        TournamentFinal = 12,
        TianHeAttack = 13,
        BaiNingBingIceSeal = 14,
        BloodSacrifice = 15,
        LeftQingMao = 16,

        // === Stage3: 三转·南疆流浪 ===
        SouthBorderArrival = 20,
        ShangXinCiMeet = 21,
        ThreeKingsInheritance = 22,
        ChunQiuChanFragment = 23,
        SanXiuCampComplete = 24,

        // === Stage4: 四转·义天山大战 ===
        YiTianShanAppears = 30,
        YiTianShanDungeon = 31,
        DaTongFeng = 32,
        FangYuanReveal = 33,
        YiTianShanComplete = 34,

        // === Stage5: 五转·北原争霸 ===
        NorthDesertArrival = 40,
        WangTingAlly = 41,
        ChangShengTianContact = 42,
        TaiBaiYunShengDeath = 43,
        ImmortalZombieChoice = 44,
        HeavenPrelude = 45,

        // === Stage6: 六转·宿命大战与升仙 ===
        DestinyWarBegin = 50,
        FourPillarsDown = 51,
        FactionChoice = 52,
        LongGongPhase1 = 53,
        ChunQiuRebirth = 54,
        LongGongPhase2 = 55,
        DestinyShattered = 56,
        Ascension = 57,

        // === Stage7: 七转以上·蛊仙之路 ===
        SevenTurnBegin = 60,
        ApertureBuilt = 61,
        EightTurnBegin = 62,
        DaoLordChallenge = 63,
        NineTurnBegin = 64,
        VenerableBattle = 65,
        TenTurnFinale = 66,
        EndingChosen = 67,

        // === 旧版兼容（保留旧值映射） ===
        Crisis = BloodSacrifice,
        Finale = LeftQingMao,
    }

    /// <summary>
    /// 玩家剧情进度 — 记录单个玩家的剧情推进状态。
    /// 每个玩家独立追踪，支持多玩家同时在线。
    /// </summary>
    public class PlayerStoryProgress
    {
        /// <summary> 当前所处的剧情阶段 </summary>
        public StoryPhase CurrentPhase { get; set; } = StoryPhase.NotEntered;

        /// <summary> 已完成的剧情 ID 列表 </summary>
        public HashSet<string> CompletedStoryIDs { get; set; } = new();

        /// <summary> 当前激活的剧情 ID（同一时间只能激活一个） </summary>
        public string ActiveStoryID { get; set; }

        /// <summary> 剧情相关的自定义标记（用于分支记录） </summary>
        public Dictionary<string, string> StoryFlags { get; set; } = new();

        /// <summary> 是否已完成指定剧情 </summary>
        public bool HasCompleted(string storyID) => CompletedStoryIDs.Contains(storyID);

        /// <summary> 标记剧情为已完成 </summary>
        public void CompleteStory(string storyID)
        {
            CompletedStoryIDs.Add(storyID);
            if (ActiveStoryID == storyID)
                ActiveStoryID = null;
        }

        /// <summary> 设置剧情标记 </summary>
        public void SetFlag(string key, string value) => StoryFlags[key] = value;

        /// <summary> 获取剧情标记 </summary>
        public string GetFlag(string key) =>
            StoryFlags.TryGetValue(key, out var val) ? val : null;

        /// <summary> 获取当前阶段所属的Stage编号（1~7），0表示未进入 </summary>
        public int GetCurrentStage()
        {
            int v = (int)CurrentPhase;
            if (v <= 7) return 1;
            if (v >= 10 && v <= 16) return 2;
            if (v >= 20 && v <= 24) return 3;
            if (v >= 30 && v <= 34) return 4;
            if (v >= 40 && v <= 45) return 5;
            if (v >= 50 && v <= 57) return 6;
            if (v >= 60 && v <= 67) return 7;
            return 0;
        }

        /// <summary> 获取当前阶段的显示名称 </summary>
        public string GetStageDisplayName()
        {
            return GetCurrentStage() switch
            {
                1 => "一转·蛊师入门",
                2 => "二转·家族争锋",
                3 => "三转·南疆流浪",
                4 => "四转·义天山大战",
                5 => "五转·北原争霸",
                6 => "六转·宿命大战",
                7 => "七转·蛊仙之路",
                _ => "未踏入蛊道"
            };
        }

        /// <summary> 序列化 </summary>
        public TagCompound Save()
        {
            // StoryFlags 是 Dictionary<string, string>，NBT 不支持直接序列化，
            // 转为 List<string> 键值对列表存储（"key:value" 格式）
            var flagList = new List<string>();
            foreach (var kvp in StoryFlags)
                flagList.Add($"{kvp.Key}:{kvp.Value}");

            return new TagCompound
            {
                ["phase"] = CurrentPhase.ToString(),
                ["completed"] = new List<string>(CompletedStoryIDs),
                ["active"] = ActiveStoryID ?? "",
                ["flags"] = flagList,
            };
        }

        /// <summary> 反序列化 </summary>
        public static PlayerStoryProgress Load(TagCompound tag)
        {
            var progress = new PlayerStoryProgress();
            if (tag.TryGet("phase", out string phaseStr)
                && System.Enum.TryParse<StoryPhase>(phaseStr, out var phase))
            {
                progress.CurrentPhase = phase;
            }
            if (tag.TryGet("completed", out List<string> completed))
            {
                progress.CompletedStoryIDs = new HashSet<string>(completed);
            }
            progress.ActiveStoryID = tag.GetString("active");
            if (string.IsNullOrEmpty(progress.ActiveStoryID))
                progress.ActiveStoryID = null;
            // 从 List<string> 键值对列表还原 Dictionary<string, string>
            if (tag.TryGet("flags", out List<string> flagList))
            {
                progress.StoryFlags = new Dictionary<string, string>();
                foreach (var entry in flagList)
                {
                    int colonIdx = entry.IndexOf(':');
                    if (colonIdx > 0)
                        progress.StoryFlags[entry[..colonIdx]] = entry[(colonIdx + 1)..];
                }
            }
            return progress;
        }
    }
}