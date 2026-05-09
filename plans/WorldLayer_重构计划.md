# WorldLayer 重构计划

> **目标**：将世界层从分散的 ModSystem 整合为统一的世界状态机，理清 GuWorldSystem / WorldEventSystem / HeavenTribulationSystem / PowerStructureSystem 之间的职责边界，消除重复逻辑，为后续叙事层和 NPC 层提供稳定的世界状态查询接口。

---

## 现状分析

### 已实现的核心系统

| 系统 | 文件 | 行数 | 状态 |
|------|------|------|------|
| [`GuWorldSystem`](Common/Systems/GuWorldSystem.cs) | 世界基础状态（势力、声望等级、蛊师等级、性格、态度） | 258 行 | ✅ 基础实现 |
| [`WorldEventSystem`](Common/Systems/WorldEventSystem.cs) | 世界事件调度（商队、兽潮、家族会议） | 305 行 | ✅ 基础实现 |
| [`HeavenTribulationSystem`](Common/Systems/HeavenTribulationSystem.cs) | 天劫系统（警告、闪避、雷/火/心魔） | 398 行 | ✅ MVA 占位 |
| [`PowerStructureSystem`](Common/Systems/PowerStructureSystem.cs) | 权力结构（继承链、职位空缺） | 390 行 | ✅ 完整实现 |
| [`EventBus`](Common/Events/EventBus.cs) | 轻量级事件总线 | 80 行 | ✅ 可用 |
| [`GuWorldEvent`](Common/Events/GuWorldEvent.cs) | 事件基类 + 所有事件类型 | 270 行 | ✅ 完整 |

### 关键发现

1. **`GuWorldSystem` 第 152 行** 有 `// TODO: D-01 将与 WorldEventSystem 合并为 WorldStateMachine` — 说明原作者已有合并意图
2. **`PowerStructureSystem` 已完整实现** — 不是"未实现"，已有硬编码继承链、职位持有者管理、空缺事件处理
3. **`HeavenTribulationSystem` 已有完整骨架** — 天劫状态机、警告机制、闪避检测、三种天劫类型
4. **`EventBus` 已存在且可用** — 不需要重新造轮子
5. **势力关系存储在 `GuWorldSystem.AllFactions`** — 静态字典，跨世界生命周期
6. **玩家势力关系存储在 `GuWorldPlayer.FactionRelations`** — 每个玩家独立

---

## 重构计划

### Phase 0：立即修复（P0）

#### D-01：合并 GuWorldSystem + WorldEventSystem → WorldStateMachine

**动机**：两个系统都管理世界级状态，`GuWorldSystem` 管理势力关系，`WorldEventSystem` 管理事件调度，但两者共享 `WorldEventType` 枚举和 `FactionID`。合并后提供统一的世界状态查询入口。

**步骤**：

1. 创建新文件 `Common/Systems/WorldStateMachine.cs`
2. 将 `GuWorldSystem` 的全部内容迁移进去（势力关系、声望等级、蛊师等级等）
3. 将 `WorldEventSystem` 的全部内容迁移进去（事件调度、周期性事件）
4. 保留 `GuWorldSystem.cs` 和 `WorldEventSystem.cs` 作为转发器（deprecated），标记 `[Obsolete]`
5. 更新所有引用 `GuWorldSystem` 和 `WorldEventSystem` 的地方指向 `WorldStateMachine`

**关键代码**：

```csharp
// WorldStateMachine.cs 核心结构
public class WorldStateMachine : ModSystem
{
    // === 势力关系（来自 GuWorldSystem）===
    public static Dictionary<FactionID, FactionState> AllFactions = new();
    public static int GetRelation(FactionID a, FactionID b) { ... }

    // === 事件调度（来自 WorldEventSystem）===
    public List<WorldEventInstance> ActiveEvents = new();
    public static WorldEventInstance TriggerEvent(WorldEventType type, int duration, string name) { ... }

    // === 统一查询接口 ===
    public static bool IsEventActive(WorldEventType type) { ... }
    public static FactionState GetFactionState(FactionID id) { ... }
}
```

#### D-01a：将 HeavenTribulationSystem 状态纳入 WorldStateMachine

**问题**：当前 `HeavenTribulationSystem` 通过静态方法被其他系统直接调用（如 `HeavenTribulationSystem.Instance`），缺乏统一的状态管理入口。

**方案**：在合并 `WorldStateMachine` 时，将 `HeavenTribulationSystem` 的状态也纳入统一管理：

```csharp
// WorldStateMachine 中增加天劫状态代理
public class WorldStateMachine : ModSystem
{
    // 天劫状态代理
    public HeavenTribulationState TribulationState => HeavenTribulationSystem.Instance?.CurrentState;
    public bool IsTribulationActive => HeavenTribulationSystem.Instance?.IsActive ?? false;
    
    // 统一的天劫查询接口
    public static HeavenTribulationType GetActiveTribulationType() { ... }
    public static int GetTribulationWarningTime() { ... }
}
```

#### D-02：统一 Save/Load 数据格式

**问题**：`GuWorldSystem`、`WorldEventSystem`、`HeavenTribulationSystem`、`PowerStructureSystem` 各自有独立的 `SaveWorldData` / `LoadWorldData`。数据分散，加载顺序依赖 ModSystem 加载顺序。

**方案**：在 `WorldStateMachine` 中提供统一的 `SaveWorldData` / `LoadWorldData`，内部委托给各子系统：

```csharp
public override void SaveWorldData(TagCompound tag)
{
    tag["factions"] = SerializeFactions(AllFactions);
    tag["events"] = SerializeEvents(ActiveEvents);
    tag["tribulations"] = HeavenTribulationSystem.Instance.Save();
    tag["powerStructure"] = PowerStructureSystem.Instance.Save();
}
```

---

### Phase 1：核心增强（P1）

#### D-04：WorldEvent 数据驱动化

**问题**：当前事件调度逻辑硬编码在 `CheckPeriodicEvents()` 中（第 108-138 行），添加新事件需要修改代码。

**方案**：引入事件模板配置：

```csharp
public class WorldEventTemplate
{
    public WorldEventType Type;
    public string Name;
    public int BaseInterval;       // 基础间隔（天）
    public int Duration;           // 持续时间（天）
    public Func<Player, bool> TriggerCondition;  // 触发条件
    public Action<WorldEventInstance> OnStart;   // 事件开始回调
    public Action<WorldEventInstance> OnTick;    // 每 tick 回调
    public Action<WorldEventInstance> OnEnd;     // 事件结束回调
}
```

#### D-05：天劫系统增强（伤害实现）

**问题**：`HeavenTribulationSystem` 已有完整骨架，但：
- MVA 阶段只做特效和警告，没有实际伤害
- 闪避检测只检查玩家是否在出生点附近（400px）
- 天劫类型只有 Lightning / Fire / HeartDemon 三种

**P1 增强**（提升为 P1，因为天劫伤害直接影响玩家生存体验）：
1. 实现实际的天劫伤害逻辑（落雷、火焰、心魔幻象）
2. 添加天劫准备状态（玩家可主动选择渡劫时机）
3. 添加天劫成功率计算（基于玩家属性、装备、丹药）

#### D-06：权力结构动态化

**问题**：`PowerStructureSystem` 的继承链是硬编码的（第 97-113 行），只支持 GuYue 势力。

**方案**：
1. 将继承链配置化，支持所有 9 个势力
2. 添加职位任期和选举机制
3. 添加玩家可竞选职位的接口

---

### Phase 2：远期规划（P2）

#### D-03：EventBus 类型安全增强

**问题**：当前 `EventBus` 使用 `Dictionary<Type, Delegate>`，发布/订阅时没有编译时类型检查。虽然可用，但容易在重构时遗漏事件处理器的更新。

**方案**：保持轻量级设计，但添加以下增强：

```csharp
// 添加事件源追踪
public class EventSource
{
    public string SystemName { get; init; }
    public int GameTick { get; init; }
}

// 在 GuWorldEvent 基类中添加
public abstract class GuWorldEvent
{
    public int Tick { get; set; }
    public FactionID SourceFaction { get; set; }
    public EventSource Source { get; set; }  // 新增
}
```

> **降级说明**：当前 `EventBus` 运行稳定，类型安全增强为可选优化，非紧急。

#### D-07：子世界管理

**问题**：项目使用了 `SubworldLibrary`，但子世界的进入/退出逻辑分散在各处。

**方案**：在 `WorldStateMachine` 中添加子世界管理：
- 统一的进入/退出接口
- 子世界状态保存/恢复
- 子世界与主世界的时间同步

#### D-08：世界演化日志

**方案**：添加世界事件日志系统，记录所有重要世界事件（势力关系变化、天劫、职位变更等），供叙事层和 UI 层查询。

---

## 依赖关系

```
WorldStateMachine (D-01)
├── EventBus (D-03) ─── 事件发布/订阅
├── FactionState ─────── 势力关系
├── WorldEventInstance ─ 事件调度
├── HeavenTribulation ── 天劫系统 (D-01a)
└── PowerStructure ───── 权力结构
     └── SubworldManager (D-07) ── 子世界
```

## 迁移路线图

| 步骤 | 内容 | 工作量 | 风险 | 优先级 |
|------|------|--------|------|--------|
| D-01 | 合并 GuWorldSystem + WorldEventSystem | 中 | 低 — 已有 TODO 注释 | **P0** |
| D-01a | 天劫状态纳入 WorldStateMachine | 小 | 低 — 与 D-01 同步进行 | **P0** |
| D-02 | 统一 Save/Load | 小 | 低 — 纯重构 | **P0** |
| D-05 | 天劫伤害实现 | 大 | 中 — 需要平衡性调整 | **P1**（提升，影响生存体验） |
| D-04 | 事件模板化 | 中 | 中 — 需要设计模板接口 | P1 |
| D-06 | 权力结构动态化 | 中 | 低 — 已有基础 | P2 |
| D-03 | EventBus 增强 | 小 | 低 — 向后兼容 | P2（降级，运行稳定） |
| D-07 | 子世界管理 | 大 | 高 — 依赖 SubworldLibrary | P2 |
| D-08 | 世界演化日志 | 小 | 低 — 纯新增功能 | P3 |
