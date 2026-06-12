using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// StoryPhase门控系统 — 境界突破需要对应的剧情阶段
    ///
    /// 规则：
    /// - 一转突破: 无限制（默认可达）
    /// - 二转突破: 需要 FamilyRecognition 阶段
    /// - 三转突破: 需要 SouthBorderArrival 阶段
    /// - 四转突破: 需要 YiTianShanAppears 阶段
    /// - 五转突破: 需要 NorthDesertArrival 阶段
    /// - 六转突破: 需要 DestinyWarBegin 阶段
    /// - 七转突破: 需要 Ascension 阶段
    /// - 八转突破: 需要 EightTurnBegin 阶段
    /// - 九转突破: 需要 NineTurnBegin 阶段
    /// </summary>
    public class StoryPhaseGateSystem : ModSystem
    {
        public static StoryPhaseGateSystem Instance => ModContent.GetInstance<StoryPhaseGateSystem>();

        /// <summary> 境界→最低StoryPhase映射 </summary>
        private static readonly Dictionary<int, StoryPhase> LevelGateMap = new()
        {
            { 2, StoryPhase.FamilyRecognition },
            { 3, StoryPhase.SouthBorderArrival },
            { 4, StoryPhase.YiTianShanAppears },
            { 5, StoryPhase.NorthDesertArrival },
            { 6, StoryPhase.DestinyWarBegin },
            { 7, StoryPhase.Ascension },
            { 8, StoryPhase.EightTurnBegin },
            { 9, StoryPhase.NineTurnBegin },
        };

        /// <summary> 境界→突破说明 </summary>
        private static readonly Dictionary<int, string> LevelGateDescriptions = new()
        {
            { 2, "需要获得古月家族认可" },
            { 3, "需要到达南疆" },
            { 4, "需要义天山异变事件" },
            { 5, "需要到达北原" },
            { 6, "需要宿命大战开始" },
            { 7, "需要完成升仙" },
            { 8, "需要突破八转" },
            { 9, "需要突破九转" },
        };

        /// <summary> 检查玩家是否可以突破到指定境界 </summary>
        public static bool CanBreakthrough(Player player, int targetLevel)
        {
            // 一转无限制
            if (targetLevel <= 1) return true;

            // 没有门控的境界默认允许
            if (!LevelGateMap.TryGetValue(targetLevel, out var requiredPhase))
                return true;

            var currentPhase = StoryManager.Instance.GetPhase(player);
            return (int)currentPhase >= (int)requiredPhase;
        }

        /// <summary> 获取突破被拒绝的原因 </summary>
        public static string GetBlockReason(Player player, int targetLevel)
        {
            if (CanBreakthrough(player, targetLevel))
                return "";

            if (LevelGateDescriptions.TryGetValue(targetLevel, out var desc))
                return $"无法突破到{targetLevel}转——{desc}";

            return $"无法突破到{targetLevel}转——剧情进度不足";
        }

        /// <summary> 获取指定境界需要的StoryPhase </summary>
        public static StoryPhase GetRequiredPhase(int targetLevel)
        {
            return LevelGateMap.TryGetValue(targetLevel, out var phase) ? phase : StoryPhase.NotEntered;
        }

        /// <summary> 获取当前玩家可突破的最高境界 </summary>
        public static int GetMaxAllowedLevel(Player player)
        {
            var currentPhase = StoryManager.Instance.GetPhase(player);
            int maxLevel = 1;

            foreach (var kvp in LevelGateMap)
            {
                if ((int)currentPhase >= (int)kvp.Value)
                    maxLevel = Math.Max(maxLevel, kvp.Key);
            }

            return maxLevel;
        }
    }
}
