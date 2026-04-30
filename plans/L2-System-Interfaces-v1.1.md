# VerminLordMod 蛊师世界 — 系统接口设计（L2 修正版 v1.1）

> 版本：v1.1（基于小 d 评审修正）  
> 维护者：fsx（架构师）  
> 更新日期：2026-04-27  
> 本文档回答「每个系统的职责边界、输入输出、联系方式、数据结构」。  
> 所有接口必须通过事件总线或明确定义的接口层通信，禁止跨域直接调用。  
> **红色标记 🔴 为必须修正项，黄色标记 🟡 为建议修正项。**

---

## 1. L2 设计原则

| 原则 | 说明 |
|------|------|
| **接口先行** | 任何系统在进入 L3（实现）之前，必须先在 L2 中登记接口 |
| **输入输出明确** | 每个系统必须列出「我从哪里接收什么」「我向哪里发送什么」 |
| **数据结构冻结** | L2 中定义的核心结构体在 L3 实现期间不可变更字段名和类型，只能扩展 |
| **事件总线唯一通道** | 跨域通信必须通过 EventBus，域内通信可通过直接调用（同一域内的系统允许紧耦合） |
| **版本兼容** | 接口变更必须向后兼容至少一个版本，或明确标注破坏性变更 |

---

## 2. 事件总线规范（D-03）

### 2.1 事件基类

```csharp
public abstract class GuWorldEvent
{
    public int Tick;           // 发生时的游戏帧
    public FactionID SourceFaction;  // 事件源势力（若有）
}
```

### 2.2 核心事件类型（P0-P1）

```csharp
// 玩家层 → 世界层 / NPC 层
public class PlayerDeathEvent : GuWorldEvent
{
    public int PlayerID;
    public Vector2 Position;
    public List<int> DroppedItemTypes;  // 掉落物品类型ID列表（用于日志）
    public int KillerNPCID;             // 若为 NPC 击杀
    public bool IsBackstab;             // 是否为背刺
}

public class PlayerQiChangedEvent : GuWorldEvent
{
    public int PlayerID;
    public float OldQi;
    public float NewQi;
    public QiChangeReason Reason;       // 消耗/恢复/死亡清空
}

public class GuActivatedEvent : GuWorldEvent
{
    public int PlayerID;
    public int GuTypeID;                // 催动的蛊虫类型
    public bool IsInFamilyCore;         // 是否在核心区催动（影响 NPC 感知）
}

// 🔴 修正 v1.1：新增 —— 玩家死亡时蛊虫损失事件（D-05/D-06/D-20）
public class PlayerGusLostOnDeathEvent : GuWorldEvent
{
    public int PlayerID;
    public List<KongQiaoSlot> EscapedGus;     // 叛逃的蛊虫（忠诚度 < 40%）
    public List<KongQiaoSlot> SelfDestructedGus; // 自毁的蛊虫（忠诚度 < 40% 且触发自毁）
    public List<KongQiaoSlot> RetainedGus;       // 保留的蛊虫（忠诚度 ≥ 40% 或本命蛊）
    public int MainGuTypeID;                     // 本命蛊类型（必定保留）
}

// NPC 层 → 世界层 / 玩家层 / 经济层
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

public class NPCAttitudeChangedEvent : GuWorldEvent
{
    public int NPCType;
    public int TargetPlayerID;
    public GuAttitude OldAttitude;
    public GuAttitude NewAttitude;
    public AttitudeChangeReason Reason;
}

public class NPCLootedPlayerEvent : GuWorldEvent
{
    public int NPCType;
    public int TargetPlayerID;
    public Vector2 LootPosition;
    public List<int> LootedItemTypes;
}

// 🔴 修正 v1.1：新增 —— 深度搜尸事件（问题 10/11）
public class DeepLootingStartedEvent : GuWorldEvent
{
    public int PlayerID;
    public Vector2 CorpsePosition;
    public int CorpseOwnerPlayerID;     // 尸体原主人（用于复仇标记）
    public int DurationTicks;           // 预计持续帧数（3-5 秒 = 180-300 帧）
}

public class DeepLootingCompletedEvent : GuWorldEvent
{
    public int PlayerID;
    public Vector2 CorpsePosition;
    public List<int> LootedItemTypes;   // 搜到的稀有物品
    public bool WasInterrupted;           // 是否被中断（NPC 攻击等）
}

// 世界层 → 所有层
public class WorldEventTriggeredEvent : GuWorldEvent
{
    public WorldEventType EventType;
    public Vector2 EventPosition;
    public int DurationTicks;
}

public class FactionRelationChangedEvent : GuWorldEvent
{
    public FactionID FactionA;
    public FactionID FactionB;
    public int OldRelation;
    public int NewRelation;
    public RelationChangeReason Reason;
}

public class RoleVacancyEvent : GuWorldEvent
{
    public FactionID Faction;
    public FactionRole VacatedRole;
    public int DeceasedNPCType;
    public int SuccessorNPCType;        // 硬编码继承顺位的继任者（MVA）
}

// 经济层 → 所有层
public class ResourceDepletedEvent : GuWorldEvent
{
    public ResourceType Type;
    public Vector2 Position;
    public FactionID ControllingFaction;
}

public class TradeCompletedEvent : GuWorldEvent
{
    public int PlayerID;
    public int NPCType;
    public List<int> SoldItemTypes;
    public List<int> BoughtItemTypes;
    public int YuanStoneDelta;
    public bool IsPriceGouged;         // 是否被坐地起价
}

// 🔴 修正 v1.1：新增 —— 悬赏发布事件（问题 5，解耦 NpcDeathHandler 与 BountySystem）
public class BountyPostedEvent : GuWorldEvent
{
    public FactionID PostingFaction;
    public int TargetPlayerID;
    public int RewardAmount;
    public BountyReason Reason;
    public int BountyID;                // 唯一标识
}
```

---

## 3. 世界层接口

### 3.1 WorldStateMachine

**职责边界**：
- **做**：维护家族关系矩阵、世界事件队列、区域归属规则、职务分配表
- **不做**：不处理玩家个人数据（归玩家层）、不处理 NPC 具体 AI（归 NPC 层）、不处理战斗计算（归战斗层）

**输入**：
| 来源 | 事件/接口 | 数据 |
|------|----------|------|
| NPC 层 | `NPCDeathEvent` | 死亡 NPC 的势力、职务 |
| NPC 层 | `NPCAttitudeChangedEvent` | 势力间态度变化（影响关系矩阵） |
| 玩家层 | `PlayerDeathEvent` | 玩家击杀记录（影响势力声望） |
| 经济层 | `ResourceDepletedEvent` | 资源枯竭（可能触发势力冲突） |
| 叙事层 | `TradeCompletedEvent` | 交易完成（影响势力经济评估） |

**输出**：
| 目标 | 事件/接口 | 数据 |
|------|----------|------|
| NPC 层 | `FactionRelationChangedEvent` | 势力关系变化 |
| NPC 层 | `RoleVacancyEvent` | 职务空缺通知 |
| 玩家层 | `WorldEventTriggeredEvent` | 世界事件触发 |
| 所有层 | `HeavenTribulationWarningEvent` | 天劫预警（P2） |

**核心数据结构**：
```csharp
public class WorldStateMachine : ModSystem
{
    // 家族关系矩阵 [FactionID, FactionID] -> 关系值 (-100 ~ +100)
    public int[,] FactionRelationMatrix;

    // 活跃世界事件队列
    public List<WorldEventInstance> ActiveEvents;

    // 区域归属表：世界坐标 -> 势力归属
    public Dictionary<Vector2Int, FactionID> TerritoryMap;

    // 职务分配表：势力 -> 职务 -> NPC类型ID
    public Dictionary<FactionID, Dictionary<FactionRole, int>> RoleAssignments;

    // 硬编码继承顺位表（MVA）
    public Dictionary<FactionID, Dictionary<FactionRole, List<int>>> SuccessionChains;

    // 方法
    public void UpdateRelation(FactionID a, FactionID b, int delta, RelationChangeReason reason);
    public FactionID GetTerritoryOwner(Vector2 position);
    public bool IsFamilyCoreZone(Vector2 position, FactionID faction);
    public void OnRoleVacancy(FactionID faction, FactionRole role);
}
```

**[L3 实现提示]**：
- `TerritoryMap` 不要以像素坐标为 key，应以「区域块」（如 100x100 格）为 key，否则内存爆炸
- `SuccessionChains` MVA 阶段硬编码，P2 再接入 PowerStructureSystem 的动态竞选

---

### 3.2 FactionNetwork（子模块）

**职责边界**：
- **做**：存储和查询家族间好感度、家族基本信息（名称/颜色/领地中心）
- **不做**：不处理职务分配（归 PowerStructureSystem）、不处理世界事件调度

**接口**：
```csharp
public interface IFactionNetwork
{
    int GetRelation(FactionID a, FactionID b);
    void SetRelation(FactionID a, FactionID b, int value);
    Color GetFactionColor(FactionID faction);
    Vector2 GetFactionCenter(FactionID faction);
    string GetFactionName(FactionID faction);
}
```

**数据**：
```csharp
public struct FactionInfo
{
    public string Name;
    public Color Color;
    public Vector2 CenterPosition;
    public int DefaultPlayerRelation;  // 玩家初始声望
}
```

---

### 3.3 PowerStructureSystem（P2，占位接口）

**职责边界**：
- **做**：职务分配、竞选调度、玩家参选资格评估、权力真空处理
- **不做**：MVA 阶段不实现，由 WorldStateMachine 的硬编码继承顺位替代

**占位接口**：
```csharp
public interface IPowerStructureSystem
{
    // P2 实现
    void StartElection(FactionID faction, FactionRole role);
    void RegisterCandidate(FactionID faction, FactionRole role, int candidateNPCID);
    void PlayerDeclareCandidacy(Player player, FactionID faction, FactionRole role); // D-24: 需声望+委托/举荐
    void ResolveElection(FactionID faction, FactionRole role);
    int GetCurrentOfficeHolder(FactionID faction, FactionRole role);
}
```

---

## 4. 玩家层接口

### 4.1 QiResourcePlayer

**职责边界**：
- **做**：真元上限、当前真元、恢复/消耗、死亡清空
- **不做**：不处理境界突破（归 QiRealmPlayer）、不处理资质计算（归 QiTalentPlayer）

**输入**：
| 来源 | 事件/接口 | 数据 |
|------|----------|------|
| 战斗层 | `ConsumeQiRequest` | 消耗量、消耗原因 |
| 世界层 | `HeavenTribulationWarningEvent` | 天劫预警（可能需要储备真元） |
| NPC 层 | `NPCLootedPlayerEvent` | 被搜尸（不影响真元，但影响物品） |
| KongQiaoSystem | `UpdateQiOccupied(int)` | 🔴 修正 v1.1：被动接收占据额度更新 |

**输出**：
| 目标 | 事件/接口 | 数据 |
|------|----------|------|
| 战斗层 | `QiInsufficientEvent` | 真元不足，无法催动蛊虫 |
| 世界层 | `PlayerQiChangedEvent` | 真元变动通知 |
| NPC 层 | `PlayerQiExposedEvent` | 真元波动（高消耗时 NPC 可感知） |

**核心数据结构**：
```csharp
public class QiResourcePlayer : ModPlayer
{
    public float QiMax;           // 真元上限（由 QiRealmPlayer 计算后写入）
    public float QiCurrent;       // 当前真元
    public float QiRegenRate;     // 每帧恢复量

    // 占据额度（由 KongQiaoSystem 管理，写入此处）
    public int QiOccupied;        // 当前被启用蛊虫占据的额度
    public int QiAvailable => (int)QiMax - QiOccupied;  // 可用真元上限

    // 🔴 修正 v1.1：增加被动接收方法，明确调用方向
    public void UpdateQiOccupied(int occupied) 
    { 
        QiOccupied = occupied; 
    }
    // 注释：此方法由 KongQiaoSystem 在蛊虫启用/休眠时调用，QiResourcePlayer 不主动查询

    // 方法
    public bool ConsumeQi(float amount, QiConsumeReason reason);
    public void OnDeathClearQi();  // D-20: 死亡清空
    public void RegenQi();         // 每帧恢复
}
```

**[L3 实现提示]**：
- `QiMax` 由 `QiRealmPlayer` 在境界变化时计算并写入，QiResourcePlayer 只读（除初始化）
- `QiOccupied` 由 `KongQiaoSystem` 在蛊虫启用/休眠时调用 `UpdateQiOccupied()` 更新

---

### 4.2 QiRealmPlayer

**职责边界**：
- **做**：境界（1-10转）、阶段（初期/中期/后期/巅峰）、空窍大小、突破判定
- **不做**：不处理真元数值变化（归 QiResourcePlayer）、不处理资质加成（归 QiTalentPlayer）

**输入**：
| 来源 | 事件/接口 | 数据 |
|------|----------|------|
| 叙事层 | `AwakeningCompletedEvent` | 开窍完成，初始化一转初阶 |
| 玩家层 | `BreakthroughRequest` | 突破请求（使用舍利蛊或积累足够） |
| 世界层 | `HeavenTribulationPassedEvent` | 天劫通过（P2） |

**输出**：
| 目标 | 事件/接口 | 数据 |
|------|----------|------|
| QiResourcePlayer | `QiMaxRecalculated` | 重新计算真元上限 |
| QiTalentPlayer | `RealmChangedEvent` | 境界变化（影响资质生效范围） |
| KongQiaoSystem | `KongQiaoCapacityChanged` | 空窍容量变化 |
| 世界层 | `PlayerRealmUpEvent` | 玩家境界提升（NPC 信念更新用） |

**核心数据结构**：
```csharp
public class QiRealmPlayer : ModPlayer
{
    public int GuLevel;           // 转数 1-10
    public int LevelStage;        // 0=初期, 1=中期, 2=后期, 3=巅峰
    public int KongQiaoSize;      // 空窍大小（决定格子数硬上限）
    public float BreakthroughProgress;  // 突破进度

    // 方法
    public bool TryBreakthrough();
    public void CalculateQiMax(out float maxQi);  // 计算后写入 QiResourcePlayer
    public void CalculateKongQiaoSize(out int size); // 计算后写入 KongQiaoSystem
}
```

---

### 4.3 QiTalentPlayer

**职责边界**：
- **做**：资质（甲/乙/丙/丁）、资质对修炼/战斗/感知的加成
- **不做**：不处理境界（归 QiRealmPlayer）、不处理真元（归 QiResourcePlayer）

**输入**：
| 来源 | 事件/接口 | 数据 |
|------|----------|------|
| 叙事层 | `AwakeningCompletedEvent` | 开窍时随机或选择资质 |
| 玩家层 | `TalentItemUsedEvent` | 使用资质提升道具 |

**输出**：
| 目标 | 事件/接口 | 数据 |
|------|----------|------|
| QiResourcePlayer | `QiRegenModifier` | 资质影响恢复速率 |
| QiRealmPlayer | `BreakthroughModifier` | 资质影响突破成功率 |
| NPC 层 | `PlayerTalentExposedEvent` | 资质外露（高资质玩家更容易被 NPC 关注） |

**核心数据结构**：
```csharp
public class QiTalentPlayer : ModPlayer
{
    public enum TalentGrade { Ding, Bing, Yi, Jia }  // 丁/丙/乙/甲
    public TalentGrade Grade;
    public float CultivationSpeedMultiplier;
    public float PerceptionRangeBonus;  // 感知范围加成（影响 NPC 发现玩家的距离）
}
```

---

### 4.4 KongQiaoSystem

**职责边界**：
- **做**：空窍 UI、蛊虫炼化/取出/启用/休眠、占据额度管理、格子数管理、🔴 玩家死亡时蛊虫处理
- **不做**：不处理蛊虫的具体战斗效果（归战斗层）、不处理道痕冲突计算（归 DaoHenConflictSystem）

**输入**：
| 来源 | 事件/接口 | 数据 |
|------|----------|------|
| 玩家层 | `QiRealmPlayer.KongQiaoCapacityChanged` | 格子数硬上限变化 |
| 战斗层 | `MediumWeaponSystem.FireRequest` | 查询当前启用的攻击蛊 |
| NPC 层 | `NPCPerceptionRequest` | NPC 查询玩家暴露的蛊虫气息 |
| 世界层 | `PlayerDeathEvent` | 🔴 修正 v1.1：触发 OnPlayerDeath() |

**输出**：
| 目标 | 事件/接口 | 数据 |
|------|----------|------|
| QiResourcePlayer | `UpdateQiOccupied(int)` | 更新占据额度 |
| 战斗层 | `ActiveAttackGusResponse` | 当前启用的攻击蛊列表 |
| NPC 层 | `ExposedGuInfo` | 玩家当前催动的蛊虫信息 |
| 世界层 | `GuActivatedEvent` | 蛊虫催动事件（核心区催动触发守卫） |
| 世界层 | `PlayerGusLostOnDeathEvent` | 🔴 修正 v1.1：玩家死亡蛊虫损失事件 |

**核心数据结构**：
```csharp
public class KongQiaoSystem : ModPlayer
{
    public List<KongQiaoSlot> KongQiao;  // 空窍格子
    public int MaxSlots;                  // 硬上限（来自 QiRealmPlayer）
    public int UsedSlots => KongQiao.Count;

    // 方法
    public bool TryRefineGu(Item guItem);     // 炼化蛊虫入空窍
    public bool TryExtractGu(int slotIndex);  // 取出蛊虫到背包
    public void SetGuActive(int slotIndex, bool active);  // 启用/休眠
    public List<KongQiaoSlot> GetActiveAttackGus();       // 获取启用的攻击蛊
    public List<KongQiaoSlot> GetActivePassiveGus();      // 获取启用的被动蛊

    // 🔴 修正 v1.1：增加玩家死亡处理方法（D-20/D-05/D-06）
    public void OnPlayerDeath()
    {
        // 1. 全部启用蛊虫强制休眠
        foreach (var slot in KongQiao)
            slot.IsActive = false;

        // 2. 按忠诚度判定叛逃/自毁/保留
        var escaped = new List<KongQiaoSlot>();
        var selfDestructed = new List<KongQiaoSlot>();
        var retained = new List<KongQiaoSlot>();

        foreach (var slot in KongQiao)
        {
            if (slot.IsMainGu) 
            {
                retained.Add(slot);  // 本命蛊必定保留
                continue;
            }

            if (slot.Loyalty < 40f)
            {
                // 随机叛逃或自毁（MVA 简化，P2 可区分）
                if (Main.rand.NextBool())
                    escaped.Add(slot);
                else
                    selfDestructed.Add(slot);
            }
            else
            {
                retained.Add(slot);
            }
        }

        // 3. 从空窍中移除叛逃/自毁的蛊虫
        foreach (var slot in escaped.Concat(selfDestructed))
            KongQiao.Remove(slot);

        // 4. 发布事件
        EventBus.Publish(new PlayerGusLostOnDeathEvent 
        {
            PlayerID = Player.whoAmI,
            EscapedGus = escaped,
            SelfDestructedGus = selfDestructed,
            RetainedGus = retained,
            MainGuTypeID = KongQiao.FirstOrDefault(s => s.IsMainGu)?.GuTypeID ?? -1
        });
    }
}

// 🔴 修正 v1.1：增加 Loyalty 字段和 DaoHenTags 预埋字段
public class KongQiaoSlot
{
    public Item GuItem;           // 蛊虫物品（用于贴图/名称）
    public bool IsActive;         // 启用/休眠
    public int QiOccupation;      // 占据真元额度
    public int GuTypeID;          // 蛊虫类型标识（用于杀招配方匹配）
    public bool IsAttackGu;       // 是否攻击型
    public bool IsPassiveGu;      // 是否被动型
    public bool IsMainGu;         // 是否本命蛊
    public float Refinement;      // 炼化度 [0,100]

    // 🔴 修正 v1.1：新增忠诚度字段（D-05/D-06）
    public float Loyalty;         // 忠诚度 [0,100]
    // 注释：炼化度越高忠诚度越高，但非完全线性。忠诚度还受战斗表现/喂养/时间影响。
    // MVA 阶段简化：Loyalty = Refinement * 0.8f + 20f（基础值），P2 再引入复杂公式。

    // 🟡 修正 v1.1：预埋道痕标签位掩码（D-13，P2 启用）
    public ulong DaoHenTags;      // 道痕标签位掩码（MVA 填充默认值，P2 激活冲突检测）
    // 注释：每种道痕占一位。MVA 阶段填充默认值（如月光蛊 = 月道），P2 通过 DaoHenConflictSystem 激活。

    public int ProjectileType;    // 对应弹幕类型（攻击蛊）
    public int BuffType;          // 对应 Buff（被动蛊）
}
```

**[L3 实现提示]**：
- UI 通过右键「空窍石」物品触发（MVA 最简），P1 再扩展为 K 键面板
- `TryRefineGu` 需检查背包中是否有该蛊虫 + 消耗真元
- `SetGuActive` 需检查 `QiResourcePlayer.QiAvailable >= QiOccupation`，然后调用 `QiResourcePlayer.UpdateQiOccupied()`
- 🔴 `OnPlayerDeath()` 必须在 `PlayerDeathEvent` 处理链中被调用，确保死亡时蛊虫处理与真元清空同步

---

### 4.5 PlayerBeliefStorage（P1，占位接口）

**职责边界**：
- **做**：持久化每个 NPC 对当前玩家的信念快照
- **不做**：MVA 阶段不实现，信念存于 NPC 实例

**🟡 修正 v1.1：明确持久化时机和范围**

```csharp
public interface IPlayerBeliefStorage
{
    void SaveBelief(int npcType, int playerID, BeliefSnapshot snapshot);
    BeliefSnapshot LoadBelief(int npcType, int playerID);
    void UpdateBelief(int npcType, int playerID, InteractionResult interaction);
}

public struct BeliefSnapshot
{
    public GuRank EstimatedRank;
    public float Confidence;
    public float RiskThreshold;
    public int InteractionCount;
    public long LastInteractionTick;
}
```

**🟡 修正 v1.1 注释**：
- MVA 阶段信念仅在 NPC 实例生命周期内有效。玩家**退出世界**时信念丢失。
- 玩家**离开 NPC 加载范围**（屏幕外）时，NPC 实例释放，信念同样丢失。
- P1 通过 `IPlayerBeliefStorage` 持久化到 `GuWorldPlayer.SaveData`，解决退出/卸载后的失忆问题。
- 这意味着 MVA 阶段 NPC 对玩家的「记忆」只持续在一次游戏会话内，但这是可接受的简化。

---

## 5. NPC 层接口

### 5.1 PerceptionSystem

**职责边界**：
- **做**：NPC 感知玩家修为、蛊虫、位置、环境规则（核心区/边缘/野外）
- **不做**：不处理感知后的决策（归 NpcBrain）、不处理战斗逻辑（归战斗层）

**输入**：
| 来源 | 事件/接口 | 数据 |
|------|----------|------|
| 玩家层 | `PlayerQiExposedEvent` | 玩家真元波动 |
| 玩家层 | `ExposedGuInfo` | 玩家催动的蛊虫信息 |
| 世界层 | `WorldStateMachine.IsFamilyCoreZone` | 当前区域规则 |
| 玩家层 | `PlayerGusLostOnDeathEvent` | 🟡 修正 v1.1：感知玩家尸体旁的蛊虫气息泄漏 |

**输出**：
| 目标 | 事件/接口 | 数据 |
|------|----------|------|
| NpcBrain | `PerceptionContext` | 感知结果（结构化数据） |

**核心数据结构**：
```csharp
// 🟡 修正 v1.1：增加恶名/背刺历史字段（问题 6）
public struct PerceptionContext
{
    public Player TargetPlayer;
    public float Distance;
    public float PlayerHealthPercent;
    public float PlayerQiPercent;       // 玩家真元百分比（高消耗时更低）
    public List<int> VisibleActiveGus;  // 可见的催动蛊虫（玩家主动暴露）
    public List<int> LeakedGus;         // 气息泄漏的蛊虫（高阶蛊虫即使未催动也可能泄漏）
    public GuRank PerceivedRank;        // NPC 感知到的玩家修为（可能错误）
    public bool IsInFamilyCore;         // 是否在核心区
    public bool IsInFamilyEdge;         // 是否在边缘区
    public int NearbyFriendlyNPCs;      // 附近友方 NPC 数量
    public int NearbyHostileNPCs;       // 附近敌方 NPC 数量
    public bool IsNight;

    // 🟡 修正 v1.1：新增玩家历史行为数据
    public int PlayerInfamyPoints;      // 玩家恶名值（影响 NPC 初始风险评估）
    public bool HasBetrayedNPCsBefore;  // 是否有背刺记录（NPC 在第一次见面时即可通过情报网络获知）
    // 注释：InfamyPoints 来自 GuWorldPlayer，是公开信息（恶名远扬）。
    // HasBetrayedNPCsBefore 是布尔简化，P2 可扩展为具体背刺对象列表。
}
```

**[L3 实现提示]**：
- `VisibleActiveGus` 只包含玩家当前催动的蛊虫（IsActive=true）
- `LeakedGus` MVA 阶段不实现（P1 再引入高阶蛊虫气息泄漏）
- 区域判定通过 `WorldStateMachine.GetTerritoryOwner` 查询
- 🟡 `PlayerInfamyPoints` 和 `HasBetrayedNPCsBefore` 从 `GuWorldPlayer` 读取，是 NPC 评估玩家的重要依据

---

### 5.2 NpcBrain（P0 系统簇）

**职责边界**：
- **做**：信念更新、原型选择、策略决策
- **不做**：不处理具体移动/攻击代码（归 NPC 的 AI() 方法）、不处理对话 UI（归 DialogueSystem）

**输入**：
| 来源 | 事件/接口 | 数据 |
|------|----------|------|
| PerceptionSystem | `PerceptionContext` | 感知结果 |
| 玩家层 | `PlayerRealmUpEvent` | 玩家境界提升 |
| 玩家层 | `GuActivatedEvent` | 玩家催动蛊虫 |
| 世界层 | `FactionRelationChangedEvent` | 势力关系变化 |
| 经济层 | `ResourceDepletedEvent` | 资源枯竭（影响需求向量） |
| 经济层 | `DeepLootingStartedEvent` | 🟡 修正 v1.1：感知有人正在深度搜尸（可趁机偷袭） |

**输出**：
| 目标 | 事件/接口 | 数据 |
|------|----------|------|
| NPC 实例 | `StrategyDecision` | 策略决策（观望/试探/交易/攻击/逃跑） |
| 世界层 | `NPCAttitudeChangedEvent` | NPC 态度变化 |
| 玩家层 | `BeliefUpdateEvent` | 信念更新（用于 PlayerBeliefStorage P1） |

**核心数据结构**：
```csharp
public class NpcBrain
{
    // 信念子系统
    public class BeliefSubsystem
    {
        public GuRank EstimatedRank;      // 当前最佳估计
        public float Confidence;           // 置信度 [0,1]
        public bool HasObservedCombat;     // 是否观察过玩家战斗
        public float RiskMultiplier => 1f - Confidence * 0.5f;

        public void UpdateFromInteraction(Player player, InteractionResult result);
        public void UpdateFromPerception(PerceptionContext context);
    }

    // 🟡 修正 v1.1：增加对尸体的反应维度（问题 7）
    public class ArchetypeSubsystem
    {
        public GuArchetype Archetype;      // 掠夺型/交易型/隐忍型
        public float RiskThreshold;         // 动态漂移的风险阈值
        public float CooperationBias;
        public float PatienceFactor;

        // 🟡 修正 v1.1：新增对尸体的行为倾向
        public CorpseReaction ReactionToCorpse;  // 对尸体的反应：Ignore / Observe / Loot / ReportToAuthorities
        // 注释：掠夺型倾向 Loot，交易型倾向 Ignore，隐忍型倾向 Observe 或 Report。
        // 这决定了 NpcDeathHandler 中 NPC 发现尸体后的行为。

        public void DriftRiskThreshold(PerceptionContext context, float playerStrength);
    }

    // 策略子系统
    public class StrategySubsystem
    {
        public Strategy Decide(PerceptionContext context, BeliefSubsystem belief, ArchetypeSubsystem archetype);
    }
}

// 🟡 修正 v1.1：新增尸体反应枚举
public enum CorpseReaction
{
    Ignore,             // 无视尸体（交易型默认）
    Observe,            // 观察但不碰（隐忍型默认，可能记录信息）
    Loot,               // 搜刮尸体（掠夺型默认）
    ReportToAuthorities // 报告给家族守卫（所有原型在核心区都可能触发）
}

public enum Strategy
{
    WaitAndSee,         // 观望
    Test,               // 试探（接近/询问）
    ProposeTrade,       // 提出交易
    DirectRaid,         // 直接抢夺（MVA 阶段在边缘区/野外）
    Avoid,              // 回避
    Flee,               // 逃跑
    DeceptiveApproach,  // 伪装接近（P2）
    GatherIntel,        // 收集情报（P2）
    ExecutePlan         // 执行预谋（P2）
}
```

**[L3 实现提示]**：
- MVA 阶段只实现掠夺型原型 + 3 种策略（观望/试探/攻击/回避）
- `RiskThreshold` 动态漂移：初始 0.9 → 观察到玩家弱降至 0.3 → 被反杀升至 0.95
- 信念更新用简单启发式（赢者通吃 + 噪声），不实现贝叶斯
- 🟡 `CorpseReaction` MVA 阶段只实现 Loot（掠夺型）和 Ignore（交易型），P1 再扩展 Observe/Report

---

### 5.3 DialogueSystem（P1，占位接口）

**职责边界**：
- **做**：多层对话树、对话即情报战、对话影响信念
- **不做**：MVA 阶段不实现，使用原版 SetChatButtons

**占位接口**：
```csharp
public interface IDialogueSystem
{
    // 三层菜单
    void OpenPublicMenu(Player player, NPC npc);      // 公开交互：询问/交易/展示
    void OpenDarkMenu(Player player, NPC npc);        // 暗面操作：试探底牌/虚报/下蛊（需条件解锁）
    void OpenKillMenu(Player player, NPC npc);        // 杀招准备：背刺准备/嫁祸诱导（高风险）

    // 对话影响信念
    void OnDialogueChoice(Player player, NPC npc, DialogueChoice choice);
}

public struct DialogueChoice
{
    public string Text;
    public DialogueLayer Layer;        // Public / Dark / Kill
    public BeliefImpact Impact;        // 对 NPC 信念的影响
    public RiskLevel PlayerRisk;      // 玩家承担的风险
}
```

---

### 5.4 NpcDeathHandler（P0 系统簇，简化版）

**职责边界**：
- **做**：MVA 阶段处理 NPC 死亡事件发布、尸体生成、🔴 悬赏事件发布（解耦）
- **不做**：MVA 不实现完整调查链和痕迹系统

**输入**：
| 来源 | 事件/接口 | 数据 |
|------|----------|------|
| NPC 实例 | `OnKill` | NPC 被击杀 |
| 玩家层 | `PlayerDeathEvent` | 玩家死亡（对称处理） |

**输出**：
| 目标 | 事件/接口 | 数据 |
|------|----------|------|
| 世界层 | `NPCDeathEvent` | 发布死亡事件（含 VacatedRole） |
| 叙事层 | `BountyPostedEvent` | 🔴 修正 v1.1：发布悬赏事件（由 BountySystem 订阅处理） |
| 玩家层 | `PlayerLootableCorpseEvent` | 玩家尸体可搜刮通知 |

**核心数据结构**：
```csharp
public class NpcDeathHandler
{
    // MVA 阶段只实现基础事件发布
    public void OnNPCKilled(NPC npc, Player killer);
    public void OnPlayerKilled(Player player, NPC killer);

    // 生成尸体（玩家尸体为可交互实体）
    public void SpawnPlayerCorpse(Player player, List<Item> droppedItems);
    public void SpawnNPCCorpse(NPC npc, List<Item> droppedItems);

    // 🔴 修正 v1.1：悬赏发布改为事件驱动，NpcDeathHandler 只发布事件，不直接处理悬赏逻辑
    // 原：public void PostBounty(...) → 删除
    // 新：在 OnNPCKilled 内部发布 BountyPostedEvent，由 BountySystem（或 MVA 占位）订阅

    public void OnNPCKilled(NPC npc, Player killer)
    {
        // ... 生成尸体等逻辑 ...

        // 发布死亡事件
        EventBus.Publish(new NPCDeathEvent { ... });

        // 🔴 修正 v1.1：发布悬赏事件（解耦职责）
        if (npc.ModNPC is GuMasterBase guMaster && guMaster.Faction != FactionID.Scattered)
        {
            EventBus.Publish(new BountyPostedEvent 
            {
                PostingFaction = guMaster.Faction,
                TargetPlayerID = killer.whoAmI,
                RewardAmount = CalculateBountyReward(guMaster),
                Reason = BountyReason.Revenge,
                BountyID = GenerateBountyID()
            });
        }
    }
}

public class PlayerCorpse : ModProjectile  // 或 ModNPC，待定
{
    public int OwnerPlayerID;
    public List<Item> RemainingItems;
    public int DecayTimer;             // 腐烂计时器
    public bool IsLootedByNPC;         // 是否被 NPC 搜过
    public int LootingNPCType;         // 搜尸的 NPC 类型（用于死亡日志）
}
```

**[L3 实现提示]**：
- 玩家尸体建议用 `ModProjectile`（静态实体，不移动）或自定义 `Tile`，不用 `ModNPC`（避免被原版 NPC 系统干扰）
- 尸体腐烂后剩余物品散落为原版掉落物，由原版系统接管
- 死亡日志通过聊天消息发送：「你的尸体被 [NPC 名称] 搜索过，失去了 [物品列表]」
- 🔴 悬赏逻辑从 NpcDeathHandler 中移除，改为发布 `BountyPostedEvent`，由 BountySystem 订阅处理

---

## 6. 战斗层接口

### 6.1 MediumWeaponSystem

**职责边界**：
- **做**：唯一媒介武器的查询、空窍数据驱动弹幕、杀招判定
- **不做**：不处理蛊虫的具体属性（归 KongQiaoSystem）、不处理道痕冲突（归 DaoHenConflictSystem）

**输入**：
| 来源 | 事件/接口 | 数据 |
|------|----------|------|
| 玩家层 | `KongQiaoSystem.GetActiveAttackGus()` | 启用的攻击蛊列表 |
| 玩家层 | `QiResourcePlayer.ConsumeQi()` | 真元消耗 |

**输出**：
| 目标 | 事件/接口 | 数据 |
|------|----------|------|
| 玩家层 | `QiConsumeRequest` | 请求扣除真元 |
| NPC 层 | `CombatInitiatedEvent` | 战斗发起（NPC 可感知） |
| 世界层 | `LargeScaleCombatEvent` | 大规模战斗（家族战争等） |

**核心数据结构**：
```csharp
public class MediumWeaponSystem : ModItem  // 媒介武器基类
{
    // 🟢 修正 v1.1：标注 MVA 硬编码，P1 时抽取到 ShaZhaoRecipeSystem
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, 
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // MVA 硬编码：P1 时抽取到 ShaZhaoRecipeSystem.FindMatchingRecipe() 和 FireShaZhao()
        // 当前版本直接内联杀招匹配逻辑，P1 必须重构解耦。

        var guPlayer = player.GetModPlayer<KongQiaoSystem>();
        var activeAttackGus = guPlayer.GetActiveAttackGus();

        if (activeAttackGus.Count == 0) return false;

        // 检查杀招配方（MVA 硬编码 1 条：月光蛊+酒虫=酒月斩）
        var shaZhao = FindMvaHardcodedRecipe(activeAttackGus.Select(g => g.GuTypeID));

        if (shaZhao != null && player.GetModPlayer<QiResourcePlayer>().ConsumeQi(shaZhao.QiCost, QiConsumeReason.ShaZhao))
        {
            // 释放杀招
            FireShaZhao(source, position, velocity, shaZhao, player);
            // 参与者休眠（D-10: 仅参与者休眠）
            foreach (var gu in activeAttackGus.Where(g => shaZhao.RequiredGuTypes.Contains(g.GuTypeID)))
                gu.IsActive = false;
            return false;
        }
        else
        {
            // 平 A 齐射（D-12: 同时散射）
            foreach (var gu in activeAttackGus)
            {
                float spread = Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 vel = velocity.RotatedBy(spread) * (0.8f + Main.rand.NextFloat(0.4f));
                FireProjectile(source, position, vel, gu, player);
                player.GetModPlayer<QiResourcePlayer>().ConsumeQi(2, QiConsumeReason.NormalAttack);
            }
            return false;
        }
    }

    // MVA 硬编码配方查询，P1 替换为 ShaZhaoRecipeSystem.FindMatchingRecipe()
    private ShaZhaoRecipe FindMvaHardcodedRecipe(IEnumerable<int> activeGuTypeIDs)
    {
        var ids = activeGuTypeIDs.ToList();
        if (ids.Contains(MOONLIGHT_GU_TYPE) && ids.Contains(WINEBUG_GU_TYPE))
            return new ShaZhaoRecipe 
            { 
                Name = "酒月斩", 
                RequiredGuTypes = new List<int> { MOONLIGHT_GU_TYPE, WINEBUG_GU_TYPE },
                MainProjectile = ModContent.ProjectileType<ShaZhaoJiuYueZhan>(),
                QiCost = 30,
                CooldownTicks = 300,
                DamageMultiplier = 3
            };
        return null;
    }
}
```

**[L3 实现提示]**：
- 媒介武器自身 `Item.damage = 0`，所有伤害来自蛊虫数据
- 杀招配方 MVA 硬编码 1 条，P1 再扩展为字典查询
- 齐射时弹幕角度微调（`velocity.RotatedBy(spread)`）实现散射效果
- 🟢 注释明确标注 MVA 硬编码，P1 时必须重构抽取到 ShaZhaoRecipeSystem

---

### 6.2 ShaZhaoRecipeSystem（P1，占位接口）

**职责边界**：
- **做**：杀招配方匹配、释放后休眠管理、冷却计时
- **不做**：MVA 阶段不独立为系统，硬编码在 MediumWeaponSystem 中

**占位接口**：
```csharp
public interface IShaZhaoRecipeSystem
{
    ShaZhaoRecipe FindMatchingRecipe(IEnumerable<int> activeGuTypeIDs);
    bool IsOnCooldown(int playerID, int recipeID);
    void StartCooldown(int playerID, int recipeID, int cooldownTicks);
}

public class ShaZhaoRecipe
{
    public string Name;
    public List<int> RequiredGuTypes;
    public int MainProjectile;
    public float QiCost;
    public int CooldownTicks;
    public int DamageMultiplier;       // 相对于平 A 的伤害倍率
}
```

---

### 6.3 DaoHenConflictSystem（P2，占位接口）

**职责边界**：
- **做**：道痕冲突检测（装备时缓存）、效果衰减/自伤
- **不做**：MVA 阶段不实现，P2 再接入。但 KongQiaoSlot 已预埋 DaoHenTags 字段。

**占位接口**：
```csharp
public interface IDaoHenConflictSystem
{
    ConflictMask CalculateConflictMask(List<KongQiaoSlot> activeGus);  // 装备时调用
    bool HasConflict(ConflictMask mask, DaoPath pathA, DaoPath pathB);
    float ApplyConflictPenalty(ConflictMask mask, ref int damage, ref float knockback);
}

public struct ConflictMask
{
    public ulong Mask;  // 位掩码，每种道痕占一位
}
```

---

## 7. 经济层接口

### 7.1 YuanStoneEconomy

**职责边界**：
- **做**：元石作为核心货币的流通规则、获取方式、NPC 经济模拟
- **不做**：不处理具体交易 UI（归 TradeSystem）、不处理物品属性（归物品系统）

**输入**：
| 来源 | 事件/接口 | 数据 |
|------|----------|------|
| NPC 层 | `NPCLootedPlayerEvent` | NPC 搜尸获得元石 |
| 玩家层 | `PlayerDeathEvent` | 玩家死亡掉落元石 |
| 世界层 | `ResourceDepletedEvent` | 资源点枯竭 |

**输出**：
| 目标 | 事件/接口 | 数据 |
|------|----------|------|
| NPC 层 | `NPCWealthChangedEvent` | NPC 财富变化（影响行为） |
| 玩家层 | `PlayerYuanStoneChangedEvent` | 玩家元石变动 |

**核心规则（D-14）**：
- **完全移除怪物掉落元石**
- 获取方式：搜尸（NPC/玩家尸体）、矿脉开采、交易、任务悬赏
- NPC 之间会交易、家族收税、散修抢劫商队（P1 实现）

---

### 7.2 LootSystem

**职责边界**：
- **做**：搜尸分层设计（暴露掉落 + 深度搜尸）、基地搜刮规则
- **不做**：不处理元石流通（归 YuanStoneEconomy）、不处理尸体生成（归 NpcDeathHandler）

**输入**：
| 来源 | 事件/接口 | 数据 |
|------|----------|------|
| NpcDeathHandler | `PlayerCorpse` / `NPCCorpse` | 尸体实体 |
| NPC 层 | `NPCPerceptionRequest` | NPC 查询玩家财富 |

**输出**：
| 目标 | 事件/接口 | 数据 |
|------|----------|------|
| 玩家层 | `PlayerItemsLostEvent` | 玩家失去物品 |
| 经济层 | `NPCLootedPlayerEvent` | NPC 搜尸结果 |
| NPC 层 | `DeepLootingStartedEvent` | 🔴 修正 v1.1：深度搜尸进行中（可被 NPC 感知） |

**核心数据结构**：
```csharp
public class LootSystem
{
    // 🔴 修正 v1.1：补充深度搜尸分层逻辑（问题 10）

    // 第一层：暴露掉落（击杀瞬间自动散落）
    public List<Item> CalculateExposedDrops(Player player, float dropRate = 0.3f);
    // 注释：死亡时 30% 基础物品（元石/常见材料）自动掉落在尸体周围，可立即拾取。

    // 第二层：深度搜尸（需要持续交互）
    public void StartDeepLoot(Player player, PlayerCorpse corpse);
    // 注释：开始深度搜尸，持续 3-5 秒（180-300 帧）。
    // 期间发布 DeepLootingStartedEvent，NPC 的 PerceptionSystem 可感知。
    // 深度搜尸可获得稀有物品（蛊虫/材料），但玩家处于脆弱状态。

    public void UpdateDeepLoot(Player player, PlayerCorpse corpse);
    // 注释：每帧更新深度搜尸进度。若玩家移动/受击/主动取消，则中断。

    public void CompleteDeepLoot(Player player, PlayerCorpse corpse);
    // 注释：深度搜尸完成，发布 DeepLootingCompletedEvent。

    // 搜尸范围规则（D-09 / D-15）
    public bool CanLootContainer(Player player, Vector2 corpsePosition, Vector2 containerPosition);
    // 注释：检查箱子是否在尸体附近 5 格内。超出范围的箱子不被搜刮。

    // NPC 搜尸逻辑
    public void NPCLootCorpse(NPC npc, PlayerCorpse corpse);
    // 注释：NPC 发现尸体后，根据 NpcBrain.ArchetypeSubsystem.ReactionToCorpse 决定是否搜刮。
}
```

**[L3 实现提示]**：
- 暴露掉落：击杀瞬间执行，物品散落在尸体周围（原版掉落机制）
- 深度搜尸：玩家右键尸体触发，显示进度条（3-5 秒），期间不能移动/攻击
- 🔴 深度搜尸期间发布 `DeepLootingStartedEvent`，NPC 可感知并趁机偷袭
- 尸体腐烂后剩余物品全部散落，由原版系统接管

---

### 7.3 TradeSystem（P1 重构接口）

**职责边界**：
- **做**：动态定价、交易完成、价格受急需程度/声望/情报影响
- **不做**：不处理物品转移（原版系统接管）、不处理对话（归 DialogueSystem）

**核心规则（D-16）**：
- **完全自由定价（无上限）**
- NPC 坐地起价基于：玩家急需程度（是否有替代来源）、玩家声望、NPC 原型（交易型更温和，掠夺型更狠）
- 玩家可选择：接受高价 / 拒绝交易 / 动手抢夺（触发战斗）

**数据结构**：
```csharp
public class TradeSystem
{
    public float CalculatePrice(Player player, NPC npc, Item item)
    {
        float basePrice = item.value;
        float urgencyMultiplier = CalculateUrgency(player, item);  // 急需程度
        float reputationMultiplier = CalculateReputationDiscount(player, npc);  // 声望影响
        float archetypeMultiplier = GetNPCArchetype(npc).CooperationBias;  // NPC 原型

        return basePrice * urgencyMultiplier * (1f / reputationMultiplier) * (1f / archetypeMultiplier);
    }
}
```

---

## 8. 叙事层接口

### 8.1 AwakeningSystem

**职责边界**：
- **做**：凡人开局、击杀第一只野蛊、开窍仪式、空窍初始化
- **不做**：不处理后续境界提升（归 QiRealmPlayer）、不处理蛊虫获取（归经济层）

**输入**：
| 来源 | 事件/接口 | 数据 |
|------|----------|------|
| 世界层 | `WildGuSpawnedEvent` | 野蛊生成 |
| 战斗层 | `WildGuKilledEvent` | 野蛊被击杀 |

**输出**：
| 目标 | 事件/接口 | 数据 |
|------|----------|------|
| 玩家层 | `AwakeningCompletedEvent` | 开窍完成，初始化 QiResourcePlayer / QiRealmPlayer / QiTalentPlayer / KongQiaoSystem |
| 世界层 | `PlayerAwakenedEvent` | 玩家成为蛊师（NPC 信念初始化） |

**核心流程（D-17 / D-18）**：
1. 玩家开局为凡人（无空窍，无真元，只能用原版装备）
2. 10 分钟内遇到第一只野蛊（替换原版特定怪物）
3. 击杀后掉落「空窍石」（或自动触发开窍）
4. 开启空窍，一转初阶，格子数 3，真元上限 100
5. 第一只炼化的蛊虫自动成为本命蛊

**[L3 实现提示]**：
- 凡人阶段完全依赖原版装备，让玩家熟悉 TR 基础
- 野蛊替换原版怪物（如丛林黄蜂 → 蜂刺蛊），掉落率 2-5%
- 若 10 分钟内未击杀野蛊，空窍石作为保底手动触发

---

### 8.2 BountySystem（P2，占位接口）

**职责边界**：
- **做**：悬赏发布、领取、结算
- **不做**：MVA 阶段不独立为系统，由 NpcDeathHandler 发布 `BountyPostedEvent`，BountySystem 订阅处理

**占位接口**：
```csharp
public interface IBountySystem
{
    void OnBountyPosted(BountyPostedEvent evt);  // 订阅 BountyPostedEvent
    void PostBountyByPlayer(Player player, int targetNPCID, int reward);
    bool ClaimBounty(Player player, int bountyID);
    List<Bounty> GetActiveBounties();
}

public class Bounty
{
    public int BountyID;
    public FactionID PostingFaction;
    public int TargetPlayerID;
    public int RewardAmount;
    public BountyReason Reason;
    public bool IsActive;
}
```

---

## 9. 跨域通信矩阵

| 事件 | 发布者 | 订阅者 | 优先级 |
|------|--------|--------|--------|
| `PlayerDeathEvent` | 玩家层 | 世界层 / NPC 层 / 经济层 | P0 |
| `PlayerQiChangedEvent` | 玩家层 | NPC 层（感知用） | P0 |
| `GuActivatedEvent` | 玩家层 | NPC 层 / 世界层 | P0 |
| `PlayerGusLostOnDeathEvent` | 玩家层 | 世界层 / NPC 层 | P0 |
| `NPCDeathEvent` | NPC 层 | 世界层 / 经济层 / 叙事层 | P0 |
| `NPCAttitudeChangedEvent` | NPC 层 | 世界层 | P1 |
| `NPCLootedPlayerEvent` | NPC 层 | 经济层 / 玩家层 | P1 |
| `DeepLootingStartedEvent` | 经济层 | NPC 层 | P1 |
| `DeepLootingCompletedEvent` | 经济层 | 玩家层 / 世界层 | P1 |
| `BountyPostedEvent` | NPC 层 / 叙事层 | 叙事层 | P1 |
| `WorldEventTriggeredEvent` | 世界层 | 所有层 | P1 |
| `FactionRelationChangedEvent` | 世界层 | NPC 层 / 玩家层 | P1 |
| `RoleVacancyEvent` | 世界层 | NPC 层 / 叙事层 | P2 |
| `ResourceDepletedEvent` | 经济层 | 世界层 / NPC 层 | P1 |
| `TradeCompletedEvent` | 经济层 | 世界层 | P1 |
| `AwakeningCompletedEvent` | 叙事层 | 玩家层 / 世界层 | P0 |

---

## 10. L2 到 L3 的准入条件

系统满足以下条件后，可从 L2 进入 L3（实现）：

1. **接口冻结**：核心数据结构（struct/class 的字段名和类型）在 L2 中定义后不可变更
2. **事件注册**：该系统发布和订阅的所有事件已在 EventBus 中注册
3. **域内评审**：同一域内的其他系统负责人确认接口不冲突
4. **架构师签字**：fsx 确认该系统的 L2 设计符合 L1 哲学

---

## 11. 修订历史

| 日期 | 版本 | 修订内容 |
|------|------|---------|
| 2026-04-27 | v1.0 | 初始版本，基于 L1-FINAL 生成 |
| 2026-04-27 | v1.1 | 小 d 评审修正：① KongQiaoSlot 增加 Loyalty 字段；② QiResourcePlayer 增加 UpdateQiOccupied 方法；③ KongQiaoSystem 增加 OnPlayerDeath 方法和 PlayerGusLostOnDeathEvent；④ LootSystem 补充深度搜尸分层逻辑；⑤ NpcDeathHandler 解耦悬赏职责（改为发布 BountyPostedEvent）；⑥ PerceptionContext 增加恶名/背刺字段；⑦ ArchetypeSubsystem 增加 CorpseReaction；⑧ MediumWeaponSystem 标注 MVA 硬编码；⑨ KongQiaoSlot 预埋 DaoHenTags；⑩ PlayerBeliefStorage 明确持久化时机；⑪ 事件总线增加 DeepLootingStarted/Completed 事件 |

---

> 本文档由第二层接口设计师维护。  
> 任何接口的新增、变更、删除必须在此文档中先登记，再进入 L3 实现。  
> 下一批 L2 补充：SubworldLibrary 接口（驻地/小世界进出）、DefenseSystem 接口（迷踪阵/守卫）。
