using Microsoft.Xna.Framework;
using System.Collections.Generic;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.Events
{
    /// <summary>
    /// 蛊世界事件基类（D-03 自建轻量级事件队列）
    /// 所有通过 EventBus 发布的事件必须继承此类。
    /// MVA 阶段：最简字典实现。
    /// P1 扩展：订阅优先级、错误隔离。
    /// </summary>
    public abstract class GuWorldEvent
    {
        /// <summary> 发生时的游戏帧 </summary>
        public int Tick;

        /// <summary> 事件源势力（若无则为 FactionID.None） </summary>
        public FactionID SourceFaction;
    }

    // ============================================================
    // 玩家层 → 世界层 / NPC 层
    // ============================================================

    /// <summary> 玩家死亡事件 </summary>
    public class PlayerDeathEvent : GuWorldEvent
    {
        public int PlayerID;
        public Vector2 Position;
        public List<int> DroppedItemTypes;  // 掉落物品类型ID列表（用于日志）
        public int KillerNPCID;             // 若为 NPC 击杀
        public bool IsBackstab;             // 是否为背刺
    }

    /// <summary> 玩家真元变化事件 </summary>
    public class PlayerQiChangedEvent : GuWorldEvent
    {
        public int PlayerID;
        public float OldQi;
        public float NewQi;
        public QiChangeReason Reason;       // 消耗/恢复/死亡清空
    }

    /// <summary> 蛊虫催动事件 </summary>
    public class GuActivatedEvent : GuWorldEvent
    {
        public int PlayerID;
        public int GuTypeID;                // 催动的蛊虫类型
        public bool IsInFamilyCore;         // 是否在核心区催动（影响 NPC 感知）
    }

    /// <summary> 玩家死亡时蛊虫损失事件（D-05/D-06/D-20） </summary>
    public class PlayerGusLostOnDeathEvent : GuWorldEvent
    {
        public int PlayerID;
        public List<int> EscapedGuTypeIDs;         // 叛逃的蛊虫类型ID（忠诚度 < 40%）
        public List<int> SelfDestructedGuTypeIDs;  // 自毁的蛊虫类型ID（忠诚度 < 40% 且触发自毁）
        public List<int> RetainedGuTypeIDs;        // 保留的蛊虫类型ID（忠诚度 ≥ 40% 或本命蛊）
        public int MainGuTypeID;                   // 本命蛊类型（必定保留）
    }

    // ============================================================
    // NPC 层 → 世界层 / 玩家层 / 经济层
    // ============================================================

    /// <summary> NPC 死亡事件 </summary>
    public class NPCDeathEvent : GuWorldEvent
    {
        public int NPCType;
        public int NPCWhoAmI;
        public int KillerPlayerID;
        public KillTrace Trace;
        public Vector2 Position;
        public FactionID Faction;
        public FactionRole VacatedRole;     // 若死亡者是职务 NPC
        public List<int> WitnessNPCIDs;     // 目击 NPC
    }

    /// <summary> NPC 态度变化事件 </summary>
    public class NPCAttitudeChangedEvent : GuWorldEvent
    {
        public int NPCType;
        public int TargetPlayerID;
        public GuAttitude OldAttitude;
        public GuAttitude NewAttitude;
        public AttitudeChangeReason Reason;
    }

    /// <summary> NPC 搜刮玩家尸体事件 </summary>
    public class NPCLootedPlayerEvent : GuWorldEvent
    {
        public int NPCType;
        public int TargetPlayerID;
        public Vector2 LootPosition;
        public List<int> LootedItemTypes;
    }

    /// <summary> 深度搜尸开始事件 </summary>
    public class DeepLootingStartedEvent : GuWorldEvent
    {
        public int PlayerID;
        public Vector2 CorpsePosition;
        public int CorpseOwnerPlayerID;     // 尸体原主人（用于复仇标记）
        public int DurationTicks;           // 预计持续帧数（3-5 秒 = 180-300 帧）
    }

    /// <summary> 深度搜尸完成事件 </summary>
    public class DeepLootingCompletedEvent : GuWorldEvent
    {
        public int PlayerID;
        public Vector2 CorpsePosition;
        public List<int> LootedItemTypes;   // 搜到的稀有物品
        public bool WasInterrupted;         // 是否被中断（NPC 攻击等）
    }

    // ============================================================
    // 世界层 → 所有层
    // ============================================================

    /// <summary> 世界事件触发 </summary>
    public class WorldEventTriggeredEvent : GuWorldEvent
    {
        public WorldEventType EventType;
        public Vector2 EventPosition;
        public int DurationTicks;
    }

    /// <summary> 势力关系变化事件 </summary>
    public class FactionRelationChangedEvent : GuWorldEvent
    {
        public FactionID FactionA;
        public FactionID FactionB;
        public int OldRelation;
        public int NewRelation;
        public RelationChangeReason Reason;
    }

    /// <summary> 职务空缺事件 </summary>
    public class RoleVacancyEvent : GuWorldEvent
    {
        public FactionID Faction;
        public FactionRole VacatedRole;
        public int DeceasedNPCType;
        public int SuccessorNPCType;        // 硬编码继承顺位的继任者（MVA）
    }

    // ============================================================
    // 经济层 → 所有层
    // ============================================================

    /// <summary> 资源枯竭事件 </summary>
    public class ResourceDepletedEvent : GuWorldEvent
    {
        public ResourceType Type;
        public Vector2 Position;
        public FactionID ControllingFaction;
    }

    /// <summary> 交易完成事件 </summary>
    public class TradeCompletedEvent : GuWorldEvent
    {
        public int PlayerID;
        public int NPCType;
        public List<int> SoldItemTypes;
        public List<int> BoughtItemTypes;
        public int YuanStoneDelta;
        public bool IsPriceGouged;          // 是否被坐地起价
    }

    // ============================================================
    // 叙事层 → 所有层
    // ============================================================

    /// <summary> 悬赏发布事件 </summary>
    public class BountyPostedEvent : GuWorldEvent
    {
        public FactionID PostingFaction;
        public int TargetPlayerID;
        public int RewardAmount;
        public BountyReason Reason;
        public int BountyID;                // 唯一标识
    }

    /// <summary> 开窍完成事件 </summary>
    public class AwakeningCompletedEvent : GuWorldEvent
    {
        public int PlayerID;
    }

    /// <summary> 玩家境界提升事件 </summary>
    public class PlayerRealmUpEvent : GuWorldEvent
    {
        public int PlayerID;
        public int NewLevel;
        public int NewStage;
    }

    // ============================================================
    // 辅助枚举（L2 跨域通信矩阵用）
    // ============================================================

    public enum QiChangeReason
    {
        Consume,
        Regen,
        DeathClear,
        ShaZhao,
        Other
    }

    public enum AttitudeChangeReason
    {
        PlayerAction,
        FactionRelationChange,
        WitnessedEvent,
        ReputationChange,
        Other
    }

    public enum RelationChangeReason
    {
        PlayerAction,
        NPCDeath,
        War,
        Trade,
        Diplomacy,
        Other
    }

    public enum BountyReason
    {
        Revenge,
        PlayerKill,
        Theft,
        Betrayal,
        Other
    }

    public enum FactionRole
    {
        None,
        ClanLeader,
        Elder,
        GuardCaptain,
        Merchant,
        Scout,
        Healer,
        Trainer
    }

    public enum ResourceType
    {
        YuanStoneVein,
        HerbGarden,
        GuBreedingGround,
        SpiritWell,
        Other
    }

    /// <summary> 击杀追踪信息（用于 NPC 死亡事件） </summary>
    public class KillTrace
    {
        public int KillerPlayerID;
        public string WeaponName;
        public float Distance;
        public bool IsBackstab;
        public bool IsAmbush;
    }
}
