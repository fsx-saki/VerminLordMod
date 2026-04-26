using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Content.NPCs.Town;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // WorldEventSystem - 世界事件触发系统
    //
    // 负责：
    // - 周期性触发世界事件（商队、兽潮、传承等）
    // - 事件状态管理
    // - 事件条件检查
    // - 与 GuWorldSystem 联动
    //
    // 事件类型（对应 WorldEventType）：
    // - MerchantCaravan: 商队到来（复用 JiasTravelingMerchant）
    // - GuMasterHunt: 蛊师猎杀（随机生成敌对蛊师）
    // - TreasureAppears: 宝藏出现（后续由小世界系统实现）
    // - FactionMeeting: 家族集会（NPC聚集）
    // - FactionWar: 家族战争（NPC互相对战）
    // ============================================================

    /// <summary> 世界事件实例 </summary>
    public class WorldEventInstance
    {
        public WorldEventType EventType;
        public int StartDay;                // 触发时的游戏天数
        public int Duration;                // 持续天数（0=瞬时）
        public int RemainingDays;           // 剩余天数
        public Vector2 Position;            // 事件发生位置
        public string EventName;            // 事件名称
        public bool IsActive => RemainingDays > 0;
        public bool HasTriggered;           // 是否已触发（防止重复触发）

        public WorldEventInstance(WorldEventType type, int duration, string name)
        {
            EventType = type;
            StartDay = (int)Main.time;
            Duration = duration;
            RemainingDays = duration;
            EventName = name;
            HasTriggered = false;
        }
    }

    public class WorldEventSystem : ModSystem
    {
        // ===== 事件队列 =====
        public static List<WorldEventInstance> ActiveEvents = new();
        public static List<WorldEventInstance> EventHistory = new();

        // ===== 事件计时器 =====
        private static int _dayCounter = 0;
        private const int MerchantCaravanInterval = 7;  // 每7天一次商队
        private const int BeastTideInterval = 15;        // 每15天一次兽潮
        private const int FactionMeetingInterval = 30;   // 每30天一次家族集会

        // ===== 事件触发标志 =====
        private static bool _merchantSpawnedThisCycle = false;
        private static bool _beastTideTriggeredThisCycle = false;

        public override void OnWorldLoad()
        {
            ActiveEvents.Clear();
            EventHistory.Clear();
            _dayCounter = 0;
        }

        public override void PreUpdateWorld()
        {
            // 只在每天黎明（新的一天开始时）更新一次
            // Main.time == 0.0 且 Main.dayTime == true 表示新的一天刚刚开始
            if (Main.dayTime && Main.time == 0.0)
            {
                // 更新事件计时器
                UpdateEventTimers();

                // 检查周期性事件
                CheckPeriodicEvents();
            }
        }

        /// <summary> 更新事件计时器 </summary>
        private void UpdateEventTimers()
        {
            // 减少活跃事件的剩余天数
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

        /// <summary> 检查周期性事件 </summary>
        private void CheckPeriodicEvents()
        {
            _dayCounter++;

            // 商队事件（每7天）
            if (_dayCounter % MerchantCaravanInterval == 0 && !_merchantSpawnedThisCycle)
            {
                TriggerEvent(WorldEventType.MerchantCaravan, 1, "商队到达");
                _merchantSpawnedThisCycle = true;
            }

            // 重置商队标志（每天重置，但只在间隔日触发）
            if (_dayCounter % MerchantCaravanInterval != 0)
                _merchantSpawnedThisCycle = false;

            // 兽潮事件（每15天）
            if (_dayCounter % BeastTideInterval == 0 && !_beastTideTriggeredThisCycle)
            {
                TriggerEvent(WorldEventType.GuMasterHunt, 3, "小兽潮爆发");
                _beastTideTriggeredThisCycle = true;
            }

            if (_dayCounter % BeastTideInterval != 0)
                _beastTideTriggeredThisCycle = false;

            // 家族集会（每30天）
            if (_dayCounter % FactionMeetingInterval == 0)
            {
                TriggerEvent(WorldEventType.FactionMeeting, 5, "家族集会");
            }
        }

        // ============================================================
        // 事件触发
        // ============================================================

        /// <summary> 触发一个世界事件 </summary>
        public static WorldEventInstance TriggerEvent(WorldEventType type, int duration, string name)
        {
            var evt = new WorldEventInstance(type, duration, name)
            {
                Position = GetRandomSurfacePosition()
            };

            ActiveEvents.Add(evt);

            // 广播事件通知
            string msg = GetEventNotification(evt);
            if (!string.IsNullOrEmpty(msg))
                Main.NewText(msg, Color.Gold);

            // 执行事件具体逻辑
            ExecuteEventLogic(evt);

            return evt;
        }

        /// <summary> 执行事件具体逻辑 </summary>
        private static void ExecuteEventLogic(WorldEventInstance evt)
        {
            if (evt.HasTriggered) return;
            evt.HasTriggered = true;

            switch (evt.EventType)
            {
                case WorldEventType.MerchantCaravan:
                    // 触发商队NPC刷新
                    JiasTravelingMerchant.UpdateTravelingMerchant();
                    break;

                case WorldEventType.GuMasterHunt:
                    // 触发兽潮/猎杀事件
                    // 后续由 WolfSystem 或专门的 BeastTideSystem 处理
                    Main.NewText("野兽们变得狂躁不安...", Color.Red);
                    break;

                case WorldEventType.TreasureAppears:
                    // 后续由小世界系统实现
                    break;

                case WorldEventType.FactionMeeting:
                    // 家族成员聚集
                    Main.NewText("各大家族的蛊师开始聚集...", Color.Yellow);
                    break;

                case WorldEventType.FactionWar:
                    // 家族战争
                    Main.NewText("家族之间的冲突升级了！", Color.OrangeRed);
                    break;
            }
        }

        /// <summary> 事件结束处理 </summary>
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

        /// <summary> 获取随机地表位置 </summary>
        private static Vector2 GetRandomSurfacePosition()
        {
            int x = Main.maxTilesX / 2 + Main.rand.Next(-200, 200);
            int y = (int)Main.worldSurface - 10;
            return new Vector2(x * 16, y * 16);
        }

        /// <summary> 获取事件通知文本 </summary>
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

        /// <summary> 手动触发事件（供外部调用） </summary>
        public static void ForceEvent(WorldEventType type, int duration = 1, string name = "")
        {
            if (string.IsNullOrEmpty(name))
                name = type.ToString();
            TriggerEvent(type, duration, name);
        }

        // ============================================================
        // 持久化
        // ============================================================

        public override void SaveWorldData(TagCompound tag)
        {
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
        }

        public override void LoadWorldData(TagCompound tag)
        {
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
