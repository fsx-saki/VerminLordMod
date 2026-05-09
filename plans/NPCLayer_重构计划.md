# NPCLayer 重构计划

> **目标**：基于已有的 `IGuMasterAI` 接口和 `GuMasterBase` 抽象类，完善信念系统、调查链、社交网络和对话树，使 NPC 行为从硬编码状态机演化为信念驱动的交互架构。

---

## 现状分析

### 已实现的 NPC 系统

| 系统 | 文件 | 行数 | 状态 |
|------|------|------|------|
| [`IGuMasterAI`](Content/NPCs/GuMasters/IGuMasterAI.cs) | NPC AI 接口（感知、信念、决策、态度、对话） | 321 行 | ✅ 完整设计 |
| [`GuMasterBase`](Content/NPCs/GuMasters/GuMasterBase.cs) | 抽象基类实现 IGuMasterAI | 837 行 | ✅ 完整实现 |
| [`GuAttitudeHelper`](Content/NPCs/GuMasters/IGuMasterAI.cs:202) | 态度计算工具类 | 117 行 | ✅ 完整实现 |
| [`GuNPCInfo`](Common/GlobalNPCs/GuNPCInfo.cs) | NPC 状态效果（DoT、减速、碎甲、虚弱、标记） | 80 行 | ✅ 基础实现 |
| [`DialogueSystem`](Common/Systems/DialogueSystem.cs) | 对话树系统（三层菜单、信念影响） | 302 行 | ✅ MVA 占位 |
| [`DialogueTreeManager`](Common/DialogueTree/DialogueTreeManager.cs) | 对话树管理器 | — | ✅ 存在 |

### 关键发现

1. **信念系统已完整实现** — `BeliefState` 包含 `RiskThreshold`、`ConfidenceLevel`、`ObservationCount`、`EstimatedPower`、`HasTraded`、`HasFought`、`WasDefeated`、`HasDefeatedPlayer`、`LastInteractionDay`
2. **信念数据持久化已验证** — `GuMasterBase.SaveData/LoadData`（第 676-721 行）已完整实现所有 `BeliefState` 字段的保存/加载
3. **AI 主循环已实现** — `GuMasterBase.AI()` 第 119 行：`Perceive → UpdateBelief → CalculateAttitude → Decide → ExecuteAI`
4. **态度计算已实现** — `GuAttitudeHelper.CalculateFromBelief()` 基于信念计算态度（Friendly / Neutral / Hostile / Fearful / Cautious）
5. **对话系统已有三层菜单设计** — 公开/暗面/杀招，但 MVA 阶段只实现第一层
6. **`GuNPCInfo` 只处理了状态效果** — 没有存储 NPC 的信念数据（信念数据存储在 `GuMasterBase.PlayerBeliefs` 中）
7. **`GuWorldPlayer` 提供了静态辅助方法** — `GetNPCBelief`、`IsNPCHostileBasedOnBelief`、`GetNPCThreatLevel`

---

## 重构计划

### Phase 0：无需实施（已确认完成）

#### ~~D-16：信念数据持久化验证~~

**状态**：✅ **已确认完成，无需新开发**

`GuMasterBase.SaveData/LoadData`（第 676-721 行）已完整实现所有 `BeliefState` 字段的保存和加载：
- `RiskThreshold`、`ConfidenceLevel`、`ObservationCount`、`EstimatedPower`
- `HasTraded`、`HasFought`、`WasDefeated`、`HasDefeatedPlayer`、`LastInteractionDay`

只需在后续测试中确认无 bug，不需要新开发。

---

### Phase 1：核心增强（P1）

#### D-19：调查链（Investigation Chain）

**问题**：当前 NPC 对玩家的态度变化是即时的（攻击/逃跑/交易），缺乏"怀疑→确认→行动"的渐进过程。

**方案**：实现调查链状态机：

```
InvestigationState
├── Unaware ────── 未察觉（默认）
├── Suspicious ─── 可疑（观察到异常行为）
├── Confirmed ──── 确认（收集到足够证据）
└── Action ─────── 采取行动（攻击/报警/逃跑）

状态转换条件：
Unaware → Suspicious: 观察到玩家攻击同族 / 玩家声望异常
Suspicious → Confirmed: 多次观察 / 其他 NPC 传递信息
Confirmed → Action: 确认敌意 / 玩家进入攻击范围
```

```csharp
public class InvestigationState
{
    public float SuspicionLevel;       // 0.0 - 1.0
    public int ObservationCount;       // 观察次数
    public List<string> ObservedActions; // 观察到的行为
    public int LastObservationDay;     // 最后观察日
    
    public bool IsSuspicious => SuspicionLevel > 0.3f;
    public bool IsConfirmed => SuspicionLevel > 0.7f;
    
    public void Observe(string action, float weight) { ... }
    public void DecayOverTime(int days) { ... }
}
```

#### D-20：NPC 社交网络

**问题**：NPC 之间缺乏信息共享。一个 NPC 被玩家攻击后，其他 NPC 不会得知。当前只有 `AlertNearbyAllies`，无跨 NPC 信念传播。

**方案**：利用已有的 `AlertNearbyAllies` 和 `HasWitnesses` 方法，扩展为社交网络：

```csharp
public class NPCSocialNetwork : ModSystem
{
    // NPC 之间的关系图
    public Dictionary<int, HashSet<int>> AllyGraph;  // NPC type → 盟友 NPC types
    public Dictionary<int, HashSet<int>> RivalGraph; // NPC type → 敌对 NPC types
    
    // 信息传播
    public void SpreadBelief(int sourceNPC, string playerName, BeliefState belief) { ... }
    public void SpreadAlert(int sourceNPC, string playerName, float range) { ... }
    
    // 信念聚合
    public BeliefState AggregateBelief(int npcType, string playerName) { ... }
}
```

#### D-21：对话树系统增强（第二、三层）

**问题**：`DialogueSystem` 目前只实现了第一层（公开交互），第二层（暗面操作）和第三层（杀招）留空。

**P1 实现**：
1. 第二层：暗面交易（情报买卖、雇佣刺杀、背叛交易）
2. 第三层：杀招准备（下毒、设陷阱、策反）
3. 对话选择影响信念（RiskThreshold / ConfidenceLevel）

---

### Phase 2：远期规划（P2）

#### D-17：NPC 原型系统（Archetype System）

**问题**：当前 `GuMasterBase` 是抽象类，所有 NPC 共享同一套 AI 逻辑。不同"原型"（如商人型、战士型、情报型）的行为差异通过虚方法重写实现。

**方案**：引入 NPC 原型配置化：

```csharp
public enum GuMasterArchetype
{
    Merchant,    // 商人型 — 优先交易，低风险承受
    Warrior,     // 战士型 — 好战，高风险承受
    Informant,   // 情报型 — 观察为主，收集信息
    Scholar,     // 学者型 — 研究蛊虫，中立
    Assassin,    // 刺客型 — 潜伏，一击脱离
    Leader,      // 领袖型 — 指挥其他 NPC
}

public class ArchetypeConfig
{
    public GuMasterArchetype Archetype;
    public float BaseRiskThreshold;       // 基础风险承受
    public float BaseConfidenceLevel;     // 基础自信
    public float PerceptionRange;         // 感知范围
    public float AggressionMultiplier;    // 攻击性倍率
    public float TradePriceMultiplier;    // 交易价格倍率
    public Func<NPC, PerceptionContext, Decision> DefaultDecision;
}
```

> **降级说明**：当前通过虚方法（`GetPersonality`, `GetDialogue` 等）已实现不同原型，配置化虽好但非必要。

#### D-18：感知系统增强

**问题**：当前 `PerceptionContext` 包含基本信息，但缺少部分字段。

**方案**：扩展 `PerceptionContext`：

```csharp
public struct PerceptionContext
{
    // 现有字段
    public Player TargetPlayer;
    public float DistanceToPlayer;
    public float PlayerLifePercent;
    public bool PlayerHasQiEnabled;
    public int NearbyAlliesCount;
    public int NearbyEnemiesCount;
    public bool IsInOwnTerritory;
    public int TimeOfDay;
    public bool IsRaining;
    public int PlayerInfamy;
    public int PlayerQiLevel;
    public int PlayerDamage;

    // 新增字段（可随需添加）
    public int PlayerActiveGuCount;           // 玩家激活的蛊虫数量
    public float PlayerDaoHenMultiplier;      // 玩家最高道痕倍率
    public bool PlayerHasChunQiuChan;         // 玩家是否有春秋蝉
    public FactionID[] PlayerAlliedFactions;  // 玩家结盟势力
    public float NearbyAllyBeliefAggregate;   // 附近盟友对玩家的信念聚合
    public bool IsPlayerInMiZong;             // 玩家是否在迷踪阵中
}
```

> **降级说明**：当前 `PerceptionContext` 字段足够，新增字段可随需添加，非紧急。

#### D-22：NPC 记忆系统

**方案**：扩展信念系统，添加长期记忆：
- 玩家对 NPC 做过的好事/坏事
- NPC 对玩家的情感倾向（仇恨/感激/恐惧）
- 记忆衰减机制（随时间淡忘）

#### D-23：NPC 日常行为

**方案**：为 NPC 添加非交互时的日常行为：
- 巡逻路线
- 作息时间（白天活动/夜晚休息）
- 社交活动（与其他 NPC 交谈）

---

## 依赖关系

```
NPCLayer
├── IGuMasterAI ────────── AI 接口
│   ├── PerceptionContext(D-18) ── 感知上下文
│   ├── BeliefState ────────────── 信念状态
│   ├── Decision ──────────────── 决策结构
│   └── GuAttitudeHelper ──────── 态度计算
├── GuMasterBase ────────── 抽象基类
│   ├── ArchetypeConfig(D-17) ──── 原型配置
│   ├── InvestigationState(D-19) ── 调查链
│   └── NPCSocialNetwork(D-20) ─── 社交网络
├── DialogueSystem ──────── 对话系统
│   └── DialogueTreeManager ────── 对话树管理
├── GuNPCInfo ───────────── NPC 状态效果
└── PlayerStateSnapshot(D-14) ──── 玩家状态快照 (PlayerLayer)
```

## 迁移路线图

| 步骤 | 内容 | 工作量 | 风险 | 优先级 |
|------|------|--------|------|--------|
| D-19 | 调查链实现（状态机） | 大 | 中 — 需要设计状态转换 | **P1** |
| D-20 | NPC 社交网络（信念传播） | 大 | 中 — 需要设计信息传播模型 | **P1** |
| D-21 | 对话树增强（第二、三层） | 大 | 中 — 需要设计暗面/杀招逻辑 | **P1** |
| D-17 | NPC 原型配置化 | 中 | 低 — 已有虚方法基础 | P2（降级） |
| D-18 | 感知系统扩展 | 小 | 低 — 纯扩展 | P2（降级） |
| D-22 | NPC 记忆系统 | 大 | 高 — 复杂的状态管理 | P2 |
| D-23 | NPC 日常行为 | 大 | 高 — 需要路径规划 | P2 |

### 已取消/降级的原任务

| 原任务 | 原因 | 状态 |
|--------|------|------|
| D-16 信念数据持久化验证 | `GuMasterBase.SaveData/LoadData` 已完整实现所有 `BeliefState` 字段 | 确认无 bug，无需新开发 |
| D-17 NPC 原型系统 | 当前通过虚方法已实现不同原型 | 降级为 P2 |
| D-18 感知系统增强 | 当前 `PerceptionContext` 字段足够 | 降级为 P2 |
