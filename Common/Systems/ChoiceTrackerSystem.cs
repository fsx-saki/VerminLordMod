using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// 选择追踪系统 — 记录玩家在关键剧情节点做出的选择
    ///
    /// 每个选择有唯一ID和整数值（0=A, 1=B, 2=C）
    /// 选择结果持久化到世界存档
    /// 选择时通过EventBus发布ChoiceMadeEvent
    /// </summary>
    public class ChoiceTrackerSystem : ModSystem
    {
        public static ChoiceTrackerSystem Instance => ModContent.GetInstance<ChoiceTrackerSystem>();

        /// <summary> 世界级选择记录（所有玩家共享） </summary>
        private readonly Dictionary<string, int> _worldChoices = new();

        /// <summary> 记录一个选择 </summary>
        public static void MakeChoice(string choiceID, int choiceValue)
        {
            Instance._worldChoices[choiceID] = choiceValue;
            EventBus.Publish(new ChoiceMadeEvent
            {
                ChoiceID = choiceID,
                ChoiceValue = choiceValue,
                Tick = (int)Main.GameUpdateCount
            });
        }

        /// <summary> 获取选择值，未选择返回-1 </summary>
        public static int GetChoice(string choiceID)
        {
            return Instance._worldChoices.TryGetValue(choiceID, out var v) ? v : -1;
        }

        /// <summary> 是否已做出该选择 </summary>
        public static bool HasChoice(string choiceID)
        {
            return Instance._worldChoices.ContainsKey(choiceID);
        }

        /// <summary> 获取选择文本描述 </summary>
        public static string GetChoiceDescription(string choiceID)
        {
            return choiceID switch
            {
                "S1_JiaJinSheng" => GetChoice("S1_JiaJinSheng") switch
                {
                    0 => "帮助方源",
                    1 => "帮助铁血冷",
                    2 => "旁观",
                    _ => "未决定"
                },
                "S1_HuaJiuShare" => GetChoice("S1_HuaJiuShare") switch
                {
                    0 => "分享花酒传承",
                    1 => "独占传承",
                    _ => "未决定"
                },
                "S2_TournamentResult" => GetChoice("S2_TournamentResult") switch
                {
                    0 => "冠军",
                    1 => "亚军",
                    2 => "未进决赛",
                    _ => "未参赛"
                },
                "S2_BaiNingBing" => GetChoice("S2_BaiNingBing") switch
                {
                    0 => "帮助白凝冰",
                    1 => "旁观",
                    2 => "敌对",
                    _ => "未决定"
                },
                "S2_BloodSacrifice" => GetChoice("S2_BloodSacrifice") switch
                {
                    0 => "加入方源",
                    1 => "对抗方源",
                    2 => "逃离",
                    _ => "未决定"
                },
                "S3_ShangXinCi" => GetChoice("S3_ShangXinCi") switch
                {
                    0 => "帮助商心慈",
                    1 => "利用商心慈",
                    2 => "无视",
                    _ => "未决定"
                },
                "S4_FangYuanReveal" => GetChoice("S4_FangYuanReveal") switch
                {
                    0 => "追击方源",
                    1 => "放走方源",
                    2 => "加入方源",
                    _ => "未决定"
                },
                "S5_TaiBaiYunSheng" => GetChoice("S5_TaiBaiYunSheng") switch
                {
                    0 => "救助太白云生",
                    1 => "旁观",
                    2 => "偷袭方源",
                    _ => "未决定"
                },
                "S5_ImmortalZombie" => GetChoice("S5_ImmortalZombie") switch
                {
                    0 => "成为仙僵",
                    1 => "拒绝",
                    _ => "未决定"
                },
                "S6_FactionChoice" => GetChoice("S6_FactionChoice") switch
                {
                    0 => "正道",
                    1 => "魔道",
                    2 => "中立",
                    _ => "未决定"
                },
                "S7_Ending" => GetChoice("S7_Ending") switch
                {
                    0 => "成为天意",
                    1 => "超脱蛊界",
                    2 => "摧毁天地大蛊",
                    _ => "未决定"
                },
                _ => "未知选择"
            };
        }

        /// <summary> 获取所有已做出的选择 </summary>
        public static Dictionary<string, int> GetAllChoices()
        {
            return new Dictionary<string, int>(Instance._worldChoices);
        }

        public override void ClearWorld()
        {
            _worldChoices.Clear();
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var keys = new List<string>();
            var values = new List<int>();
            foreach (var kvp in _worldChoices)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
            tag["choiceKeys"] = keys;
            tag["choiceValues"] = values;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            _worldChoices.Clear();
            if (tag.TryGet("choiceKeys", out List<string> keys) &&
                tag.TryGet("choiceValues", out List<int> values))
            {
                for (int i = 0; i < keys.Count && i < values.Count; i++)
                {
                    _worldChoices[keys[i]] = values[i];
                }
            }
        }
    }
}
