using Microsoft.Xna.Framework;
using Terraria;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // WorldEventSystem - 已废弃，仅保留 WorldEventInstance 数据结构
    // D-01: 所有功能已合并到 WorldStateMachine
    // ============================================================

    /// <summary> 世界事件实例 </summary>
    public class WorldEventInstance
    {
        public WorldEventType EventType;
        public int StartDay;
        public int Duration;
        public int RemainingDays;
        public Vector2 Position;
        public string EventName;
        public bool IsActive => RemainingDays > 0;
        public bool HasTriggered;

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
}
