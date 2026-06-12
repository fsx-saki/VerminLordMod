using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Events;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// 区域解锁系统 — 根据StoryPhase控制区域访问权限
    /// 
    /// 区域解锁条件：
    /// - 青茅山（Stage1）: 默认解锁
    /// - 南疆（Stage3）: LeftQingMao阶段后解锁
    /// - 义天山（Stage4）: SanXiuCampComplete阶段后解锁
    /// - 北原（Stage5）: YiTianShanComplete阶段后解锁
    /// - 宿命战场（Stage6）: HeavenPrelude阶段后解锁
    /// - 蛊仙境界（Stage7）: Ascension阶段后解锁
    /// </summary>
    public class AreaUnlockSystem : ModSystem
    {
        public static AreaUnlockSystem Instance => ModContent.GetInstance<AreaUnlockSystem>();

        /// <summary> 已解锁的区域ID列表 </summary>
        private readonly HashSet<string> _unlockedAreas = new();

        /// <summary> 区域定义 </summary>
        private static readonly Dictionary<string, AreaDef> AreaDefinitions = new()
        {
            { "QingMaoShan", new AreaDef("青茅山", StoryPhase.Arrival, "古月山寨所在地") },
            { "SouthBorder", new AreaDef("南疆", StoryPhase.LeftQingMao, "散修营地，三王传承") },
            { "YiTianShan", new AreaDef("义天山", StoryPhase.SanXiuCampComplete, "远古蛊仙传承") },
            { "NorthDesert", new AreaDef("北原", StoryPhase.YiTianShanComplete, "冰原王庭，长生天") },
            { "DestinyBattlefield", new AreaDef("宿命战场", StoryPhase.HeavenPrelude, "天庭与影宗的决战之地") },
            { "ImmortalRealm", new AreaDef("蛊仙境界", StoryPhase.Ascension, "升仙后的世界") },
        };

        public override void OnWorldLoad()
        {
            _unlockedAreas.Clear();
            // 青茅山默认解锁
            _unlockedAreas.Add("QingMaoShan");

            // 订阅阶段推进事件
            EventBus.Subscribe<StoryPhaseAdvancedEvent>(OnPhaseAdvanced);
        }

        public override void OnWorldUnload()
        {
            _unlockedAreas.Clear();
        }

        /// <summary> 检查区域是否已解锁 </summary>
        public static bool IsAreaUnlocked(string areaID)
        {
            return Instance._unlockedAreas.Contains(areaID);
        }

        /// <summary> 解锁区域 </summary>
        public static void UnlockArea(string areaID)
        {
            if (Instance._unlockedAreas.Add(areaID))
            {
                if (AreaDefinitions.TryGetValue(areaID, out var def))
                {
                    Main.NewText($"[蛊世界] 新区域解锁——{def.DisplayName}！", Microsoft.Xna.Framework.Color.Gold);
                    EventBus.Publish(new AreaUnlockedEvent
                    {
                        AreaName = areaID,
                        Tick = (int)Main.GameUpdateCount
                    });

                    var uiSystem = ModContent.GetInstance<global::VerminLordMod.Common.UI.StoryUI.StoryUISystem>();
                    uiSystem?.ShowToast($"新区域：{def.DisplayName}", Microsoft.Xna.Framework.Color.Gold, 6f);
                }
            }
        }

        /// <summary> 获取区域解锁进度（0~1） </summary>
        public static float GetUnlockProgress()
        {
            return (float)Instance._unlockedAreas.Count / AreaDefinitions.Count;
        }

        /// <summary> 获取所有区域状态 </summary>
        public static List<AreaStatus> GetAllAreaStatuses()
        {
            var result = new List<AreaStatus>();
            foreach (var kvp in AreaDefinitions)
            {
                result.Add(new AreaStatus
                {
                    AreaID = kvp.Key,
                    DisplayName = kvp.Value.DisplayName,
                    Description = kvp.Value.Description,
                    IsUnlocked = Instance._unlockedAreas.Contains(kvp.Key),
                    RequiredPhase = kvp.Value.RequiredPhase
                });
            }
            return result;
        }

        private void OnPhaseAdvanced(StoryPhaseAdvancedEvent e)
        {
            // 检查是否有新区域需要解锁
            var phase = (StoryPhase)e.NewPhase;
            foreach (var kvp in AreaDefinitions)
            {
                if (!_unlockedAreas.Contains(kvp.Key) &&
                    (int)phase >= (int)kvp.Value.RequiredPhase)
                {
                    UnlockArea(kvp.Key);
                }
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["unlockedAreas"] = new List<string>(_unlockedAreas);
        }

        public override void LoadWorldData(TagCompound tag)
        {
            _unlockedAreas.Clear();
            if (tag.TryGet("unlockedAreas", out List<string> areas))
            {
                foreach (var area in areas)
                    _unlockedAreas.Add(area);
            }
            // 确保青茅山始终解锁
            _unlockedAreas.Add("QingMaoShan");
        }

        // ==================== 内部类 ====================

        private class AreaDef
        {
            public string DisplayName;
            public StoryPhase RequiredPhase;
            public string Description;

            public AreaDef(string name, StoryPhase phase, string desc)
            {
                DisplayName = name;
                RequiredPhase = phase;
                Description = desc;
            }
        }

        public class AreaStatus
        {
            public string AreaID;
            public string DisplayName;
            public string Description;
            public bool IsUnlocked;
            public StoryPhase RequiredPhase;
        }
    }
}
