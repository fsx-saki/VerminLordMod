using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Content.NPCs.Town;

namespace VerminLordMod.Common.Systems
{
    // 简写别名
    using FID = FactionID;

    // ============================================================
    // WorldStateMachine — 统一世界状态管理器（D-01/D-01a/D-02）
    //
    // 合并自：
    //   - GuWorldSystem（势力关系、声望等级、蛊师等级）
    //   - WorldEventSystem（事件调度、周期性事件）
    //   - HeavenTribulationSystem（天劫状态代理）
    //
    // 提供统一的世界状态查询入口和 Save/Load。
    // ============================================================

    /// <summary>
    /// 世界状态机 — 统一管理所有世界级状态。
    /// </summary>
    public class WorldStateMachine : ModSystem
    {
        // ===== 单例访问 =====
        public static WorldStateMachine Instance => ModContent.GetInstance<WorldStateMachine>();

        // ============================================================
        // 势力关系（来自 GuWorldSystem）
        // ============================================================

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

        /// <summary> 获取家族状态 </summary>
        public static FactionState GetFactionState(FactionID id)
        {
            AllFactions.TryGetValue(id, out var state);
            return state;
        }

        // ============================================================
        // 事件调度（来自 WorldEventSystem）
        // ============================================================

        /// <summary> 活跃事件列表 </summary>
        public static List<WorldEventInstance> ActiveEvents = new();

        /// <summary> 事件历史记录 </summary>
        public static List<WorldEventInstance> EventHistory = new();

        /// <summary> 事件计时器 </summary>
        private static int _dayCounter = 0;

        private const int MerchantCaravanInterval = 7;
        private const int BeastTideInterval = 15;
        private const int FactionMeetingInterval = 30;

        private static bool _merchantSpawnedThisCycle = false;
        private static bool _beastTideTriggeredThisCycle = false;

        /// <summary> 检查事件是否活跃 </summary>
        public static bool IsEventActive(WorldEventType type)
        {
            foreach (var evt in ActiveEvents)
            {
                if (evt.EventType == type && evt.IsActive)
                    return true;
            }
            return false;
        }

        /// <summary> 触发一个世界事件 </summary>
        public static WorldEventInstance TriggerEvent(WorldEventType type, int duration, string name)
        {
            var evt = new WorldEventInstance(type, duration, name)
            {
                Position = GetRandomSurfacePosition()
            };

            ActiveEvents.Add(evt);

            string msg = GetEventNotification(evt);
            if (!string.IsNullOrEmpty(msg))
                Main.NewText(msg, Color.Gold);

            ExecuteEventLogic(evt);
            return evt;
        }

        /// <summary> 手动触发事件 </summary>
        public static void ForceEvent(WorldEventType type, int duration = 1, string name = "")
        {
            if (string.IsNullOrEmpty(name))
                name = type.ToString();
            TriggerEvent(type, duration, name);
        }

        // ============================================================
        // 天劫状态代理（来自 HeavenTribulationSystem，D-01a）
        // ============================================================

        /// <summary> 天劫是否活跃 </summary>
        public static bool IsTribulationActive
        {
            get
            {
                var hts = HeavenTribulationSystem.Instance;
                if (hts == null) return false;
                foreach (var trib in hts.ActiveTribulations)
                {
                    if (!trib.IsComplete) return true;
                }
                return false;
            }
        }

        /// <summary> 获取指定玩家的天劫实例 </summary>
        public static TribulationInstance GetPlayerTribulation(Player player)
        {
            var hts = HeavenTribulationSystem.Instance;
            if (hts == null) return null;
            foreach (var trib in hts.ActiveTribulations)
            {
                if (trib.TargetPlayerID == player.whoAmI)
                    return trib;
            }
            return null;
        }

        /// <summary> 获取活跃天劫类型 </summary>
        public static TribulationType? GetActiveTribulationType()
        {
            var hts = HeavenTribulationSystem.Instance;
            if (hts == null) return null;
            foreach (var trib in hts.ActiveTribulations)
            {
                if (!trib.IsComplete) return trib.Type;
            }
            return null;
        }

        // ============================================================
        // 生命周期
        // ============================================================

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

            SetDefaultRelations();

            // 初始化事件系统
            ActiveEvents.Clear();
            EventHistory.Clear();
            _dayCounter = 0;
        }

        public override void OnWorldUnload()
        {
            AllFactions.Clear();
            ActiveEvents.Clear();
            EventHistory.Clear();
        }

        public override void PreUpdateWorld()
        {
            // 事件系统：每天黎明更新一次
            if (Main.dayTime && Main.time == 0.0)
            {
                UpdateEventTimers();
                CheckPeriodicEvents();
            }
        }

        // ============================================================
        // 势力关系初始化
        // ============================================================

        private void SetDefaultRelations()
        {
            SetRelation(FID.GuYue, FID.Bai, 30);
            SetRelation(FID.GuYue, FID.Xiong, -20);
            SetRelation(FID.GuYue, FID.Tie, 0);
            SetRelation(FID.Bai, FID.GuYue, 30);
            SetRelation(FID.Bai, FID.Tie, 10);
            SetRelation(FID.Xiong, FID.GuYue, -20);
            SetRelation(FID.Xiong, FID.Tie, -10);
        }

        private void SetRelation(FactionID a, FactionID b, int value)
        {
            if (AllFactions.ContainsKey(a))
                AllFactions[a].Relations[b] = value;
            if (AllFactions.ContainsKey(b))
                AllFactions[b].Relations[a] = value;
        }

        // ============================================================
        // 事件调度逻辑
        // ============================================================

        private void UpdateEventTimers()
        {
            for (int i = ActiveEvents.Count - 1; i >= 0; i--)
            {
                var evt = ActiveEvents[i];
                if (evt.IsActive)
                {
                    evt.RemainingDays--;
                    if (!evt.IsActive)
                    {
                        OnEventEnd(evt);
                        EventHistory.Add(evt);
                        ActiveEvents.RemoveAt(i);
                    }
                }
            }
        }

        private void CheckPeriodicEvents()
        {
            _dayCounter++;

            if (_dayCounter % MerchantCaravanInterval == 0 && !_merchantSpawnedThisCycle)
            {
                TriggerEvent(WorldEventType.MerchantCaravan, 1, "商队到达");
                _merchantSpawnedThisCycle = true;
            }
            if (_dayCounter % MerchantCaravanInterval != 0)
                _merchantSpawnedThisCycle = false;

            if (_dayCounter % BeastTideInterval == 0 && !_beastTideTriggeredThisCycle)
            {
                TriggerEvent(WorldEventType.GuMasterHunt, 3, "小兽潮爆发");
                _beastTideTriggeredThisCycle = true;
            }
            if (_dayCounter % BeastTideInterval != 0)
                _beastTideTriggeredThisCycle = false;

            if (_dayCounter % FactionMeetingInterval == 0)
            {
                TriggerEvent(WorldEventType.FactionMeeting, 5, "家族集会");
            }
        }

        private static void ExecuteEventLogic(WorldEventInstance evt)
        {
            if (evt.HasTriggered) return;
            evt.HasTriggered = true;

            switch (evt.EventType)
            {
                case WorldEventType.MerchantCaravan:
                    JiasTravelingMerchant.UpdateTravelingMerchant();
                    break;

                case WorldEventType.GuMasterHunt:
                    Main.NewText("野兽们变得狂躁不安...", Color.Red);
                    break;

                case WorldEventType.TreasureAppears:
                    break;

                case WorldEventType.FactionMeeting:
                    Main.NewText("各大家族的蛊师开始聚集...", Color.Yellow);
                    break;

                case WorldEventType.FactionWar:
                    Main.NewText("家族之间的冲突升级了！", Color.OrangeRed);
                    break;
            }
        }

        private static void OnEventEnd(WorldEventInstance evt)
        {
            switch (evt.EventType)
            {
                case WorldEventType.MerchantCaravan:
                    Main.NewText("商队离开了。", Color.Gray);
                    break;
                case WorldEventType.GuMasterHunt:
                    Main.NewText("兽潮平息了。", Color.Gray);
                    break;
                case WorldEventType.FactionMeeting:
                    Main.NewText("家族集会结束了。", Color.Gray);
                    break;
            }
        }

        // ============================================================
        // 工具方法
        // ============================================================

        private static Vector2 GetRandomSurfacePosition()
        {
            int x = Main.maxTilesX / 2 + Main.rand.Next(-200, 200);
            int y = (int)Main.worldSurface - 10;
            return new Vector2(x * 16, y * 16);
        }

        private static string GetEventNotification(WorldEventInstance evt)
        {
            return evt.EventType switch
            {
                WorldEventType.MerchantCaravan => "一支商队正在靠近...",
                WorldEventType.GuMasterHunt => "警告：大量野兽正在接近！",
                WorldEventType.TreasureAppears => "你感应到一股奇特的波动...",
                WorldEventType.FactionMeeting => "各大家族的使者正在聚集。",
                WorldEventType.FactionWar => "家族战争爆发了！",
                _ => ""
            };
        }

        // ============================================================
        // 统一 Save/Load（D-02）
        // ============================================================

        public override void SaveWorldData(TagCompound tag)
        {
            // 势力关系
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

            // 事件系统
            var eventData = new List<TagCompound>();
            foreach (var evt in ActiveEvents)
            {
                eventData.Add(new TagCompound
                {
                    ["type"] = (int)evt.EventType,
                    ["startDay"] = evt.StartDay,
                    ["duration"] = evt.Duration,
                    ["remaining"] = evt.RemainingDays,
                    ["posX"] = evt.Position.X,
                    ["posY"] = evt.Position.Y,
                    ["name"] = evt.EventName,
                    ["triggered"] = evt.HasTriggered
                });
            }
            tag["activeEvents"] = eventData;
            tag["dayCounter"] = _dayCounter;

            // 天劫状态（委托给 HeavenTribulationSystem）
            // HeavenTribulationSystem 有自己的 SaveWorldData，由 Terraria 自动调用
        }

        public override void LoadWorldData(TagCompound tag)
        {
            // 势力关系
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

            // 事件系统
            ActiveEvents.Clear();
            if (tag.ContainsKey("activeEvents"))
            {
                var eventData = tag.GetList<TagCompound>("activeEvents");
                foreach (var data in eventData)
                {
                    var evt = new WorldEventInstance(
                        (WorldEventType)data.GetInt("type"),
                        data.GetInt("duration"),
                        data.GetString("name")
                    )
                    {
                        StartDay = data.GetInt("startDay"),
                        RemainingDays = data.GetInt("remaining"),
                        Position = new Vector2(data.GetFloat("posX"), data.GetFloat("posY")),
                        HasTriggered = data.GetBool("triggered")
                    };
                    ActiveEvents.Add(evt);
                }
            }
            _dayCounter = tag.GetInt("dayCounter");
        }
    }
}
