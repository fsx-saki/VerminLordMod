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
        /// <summary> 未进入古月山寨 </summary>
        NotEntered,

        /// <summary> 初入山寨 — 守门蛊师盘问，了解山寨概况 </summary>
        Arrival,

        /// <summary> 学堂历练 — 教头考验修为，获得基础训练 </summary>
        SchoolTraining,

        /// <summary> 药堂求助 — 药老需要药材，玩家选择帮助或拒绝 </summary>
        MedicineRequest,

        /// <summary> 家族认可 — 完成学堂和药堂任务后，获得家族正式认可 </summary>
        FamilyRecognition,

        /// <summary> 危机降临 — 山寨遭遇外敌，玩家参与防御 </summary>
        Crisis,

        /// <summary> 终章 — 解决危机，成为家族座上宾 </summary>
        Finale,
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