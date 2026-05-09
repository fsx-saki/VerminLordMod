# NarrativeLayer 重构计划

> **目标**：理清叙事层的觉醒任务、悬赏系统、情报网络、权力斗争之间的叙事链条，将分散的叙事元素整合为连贯的玩家驱动叙事框架，使玩家的每个选择都产生可感知的世界反馈。

---

## 现状分析

### 已实现的叙事系统

| 系统 | 文件 | 行数 | 状态 |
|------|------|------|------|
| [`BountySystem`](Common/Systems/BountySystem.cs) | 悬赏系统（发布、接取、完成、过期） | 407 行 | ✅ 完整实现（完整生命周期） |
| [`NpcDeathHandler`](Common/Systems/NpcDeathHandler.cs) | 死亡处理（玩家/NPC 死亡、尸体、悬赏触发） | 405 行 | ✅ 完整实现 |
| [`PowerStructureSystem`](Common/Systems/PowerStructureSystem.cs) | 权力结构（继承链、职位空缺） | 390 行 | ✅ 完整实现 |
| [`GuWorldPlayer`](Common/Players/GuWorldPlayer.cs) | 声望/通缉/结盟/背刺 | 395 行 | ✅ 完整实现 |
| [`DialogueSystem`](Common/Systems/DialogueSystem.cs) | 对话树系统（三层菜单、信念影响） | 302 行 | ✅ MVA 占位 |
| [`DialogueTreeManager`](Common/DialogueTree/DialogueTreeManager.cs) | 对话树管理器 | — | ✅ 存在 |
| [`QiRealmPlayer.OnAwakening()`](Common/Players/QiRealmPlayer.cs:35) | 觉醒初始化（给予中品武器） | 15 行 | ✅ 基础实现 |

### 关键发现

1. **`BountySystem` 已完整实现** — 不是"功能基础"，已有完整的悬赏生命周期（发布→接取→完成/过期→奖励），因此 D-41 仅为增强而非必要
2. **`PowerStructureSystem` 已完整实现** — 不是"未实现"，已有硬编码继承链、职位持有者管理、空缺事件，因此 D-42 仅为增强而非必要
3. **`NpcDeathHandler` 已完整实现** — 处理了玩家死亡、NPC 死亡、尸体 loot、悬赏触发、职位空缺
4. **`GuWorldPlayer` 已有完整的声望/通缉/结盟/背刺逻辑** — 包括链式反应（`ApplyChainReaction`）
5. **`AwakeningSystem` 未找到** — 觉醒任务系统尚未实现，当前只有 `QiRealmPlayer.OnAwakening()` 的简单初始化（给武器），**无任务链**
6. **情报网络未实现** — 没有专门的情报系统，玩家无法主动打探 NPC 信念、势力内幕
7. **`QiRealmPlayer.OnAwakening()` 第 35 行** 只做了简单的初始化：设置 GuLevel=1, LevelStage=0, 给予中品武器。没有任务链或剧情

---

## 重构计划

### Phase 0：立即修复（P0）

#### D-40：实现 AwakeningSystem（觉醒任务系统）

**问题**：当前觉醒只是一个简单的初始化调用（`QiRealmPlayer.OnAwakening()`），没有任务链、没有剧情、没有选择。新手引导缺失。

**方案**：创建完整的觉醒任务系统：

```csharp
public class AwakeningSystem : ModSystem
{
    // 觉醒状态
    public enum AwakeningStage
    {
        NotStarted,     // 未开始
        Seeking,        // 寻访（寻找蛊师引路人）
        Trial,          // 试炼（完成蛊师考验）
        Awakened,       // 已觉醒
    }
    
    public Dictionary<int, AwakeningStage> PlayerStages;  // player.whoAmI → 阶段
    
    // 觉醒任务链
    public void StartAwakening(Player player) { ... }
    public void OnSeekComplete(Player player, NPC mentor) { ... }
    public void OnTrialComplete(Player player) { ... }
    public void CompleteAwakening(Player player) { ... }
    
    // 觉醒选择（影响初始势力关系）
    public enum AwakeningPath
    {
        GuYueApprentice,    // 古月学徒 — 初始 GuYue 声望 +50
        WanderingCultivator, // 散修 — 初始所有势力中立
        Reincarnator,       // 转世者 — 初始 DaoHen +10
    }
    
    public AwakeningPath GetChosenPath(Player player) { ... }
}
```

**觉醒任务链流程**：
```
1. 玩家与任意 GuMasterBase NPC 对话
   → NPC 检测到玩家未觉醒
   → 触发觉醒对话选项："我想踏上修炼之路"

2. NPC 给出试炼任务
   → 收集特定物品 / 击败特定怪物 / 前往特定地点
   → 任务进度追踪

3. 完成任务后返回 NPC
   → NPC 为玩家觉醒（调用 QiRealmPlayer.OnAwakening()）
   → 发布 AwakeningCompletedEvent
   → 根据玩家选择影响初始势力关系
```

---

### Phase 1：核心增强（P1）

#### D-43：情报网络系统

**问题**：当前没有专门的情报系统。玩家无法主动获取关于 NPC、势力、悬赏的信息。

**方案**：

```csharp
public class IntelligenceNetwork : ModSystem
{
    // 情报类型
    public enum IntelType
    {
        NPCLocation,        // NPC 位置
        NPCAttitude,        // NPC 态度
        FactionRelation,    // 势力关系
        BountyInfo,         // 悬赏信息
        ResourceLocation,   // 资源位置
        PowerVacancy,       // 职位空缺
    }
    
    // 情报片段
    public class IntelFragment
    {
        public IntelType Type;
        public string Title;
        public string Content;
        public int Reliability;        // 可信度 0-100
        public int ObtainedDay;        // 获取游戏日
        public bool IsVerified;        // 是否已验证
    }
    
    // 获取情报的途径
    public IntelFragment GatherIntel(Player player, IntelType type, NPC source) { ... }
    public bool VerifyIntel(Player player, IntelFragment intel) { ... }
    public void ShareIntel(Player player, NPC target, IntelFragment intel) { ... }
}
```

**情报获取途径**：
1. **对话试探** — 通过 `DialogueSystem` 的暗面操作获取情报
2. **观察** — 通过 `PerceptionContext` 收集信息
3. **交易** — 从情报贩子 NPC 购买情报
4. **搜尸** — 从尸体上发现情报

---

### Phase 2：远期规划（P2）

#### D-41：悬赏系统叙事化

**问题**：当前 `BountySystem` 功能完整但缺乏叙事深度。悬赏只是"发布→击杀→领奖"的机械流程。

**方案**：为悬赏添加叙事层：

```csharp
public class Bounty
{
    // 现有字段
    public int BountyID;
    public FactionID Issuer;
    public int TargetPlayerID;
    public int Reward;
    public int PostedDay;
    public int ExpiryDay;
    public BountyReason Reason;
    
    // 新增叙事字段
    public string StoryContext;        // 悬赏背后的故事
    public string IssuerQuote;         // 发布者的"狠话"
    public List<string> Rumors;        // 关于目标的传闻
    public bool IsPublicKnowledge;     // 是否公开（私密悬赏只有特定 NPC 知道）
    public int? SecretBenefactor;      // 幕后主使（NPC type）
}
```

> **降级说明**：`BountySystem` 已有完整生命周期，叙事化为增强而非必要。

#### D-42：权力斗争叙事化

**问题**：`PowerStructureSystem` 的职位空缺和继承逻辑是纯机械的，没有叙事上下文。

**方案**：为权力结构添加叙事事件：

```csharp
public class PowerNarrativeEvent
{
    public FactionID Faction;
    public FactionRole Role;
    public string EventTitle;          // 事件标题
    public string EventDescription;    // 事件描述
    public int TriggerDay;             // 触发游戏日
    public List<PowerNarrativeChoice> Choices;  // 玩家可选行动
    
    public struct PowerNarrativeChoice
    {
        public string Description;     // 选项描述
        public int ReputationChange;   // 声望变化
        public int RiskLevel;          // 风险等级
        public Action<Player> Execute; // 执行逻辑
    }
}
```

> **降级说明**：`PowerStructureSystem` 已完整实现，叙事化为增强而非必要。

#### D-44：声望事件链

**问题**：当前声望变化是线性的（加/减点数），没有事件链。

**方案**：声望变化触发连锁叙事事件：

```csharp
// 声望阈值事件
public class ReputationEvent
{
    public FactionID Faction;
    public int Threshold;              // 触发阈值
    public RepLevel TargetLevel;       // 目标等级
    public string EventDescription;    // 事件描述
    public Action<Player> OnTrigger;   // 触发回调
}

// 示例：达到 Allied 等级时
new ReputationEvent
{
    Faction = FactionID.GuYue,
    Threshold = 200,
    TargetLevel = RepLevel.Allied,
    EventDescription = "古月家族长老会一致同意接纳你为荣誉族人",
    OnTrigger = (player) => {
        // 给予特殊物品
        // 解锁势力商店
        // 通知所有古月 NPC 态度提升
    },
};
```

#### D-45：背刺与背叛叙事

**问题**：`GuWorldPlayer.BetrayAlly` 已实现，但缺乏叙事深度。

**方案**：
1. 背刺暴露后触发势力战争事件
2. 背刺记录影响其他势力的初始信任度
3. 背刺者可能被原势力派遣刺客追杀

#### D-47：玩家名声系统

**方案**：超越单个势力的声望，建立跨势力的玩家名声：
- 仁义值（正面行为）
- 凶名值（负面行为）
- 名声影响 NPC 的初始态度

#### D-48：剧情分支记录

**方案**：记录玩家的关键选择，用于后续剧情分支：
```csharp
public class StoryFlag
{
    public string FlagID;
    public string Description;
    public int GameDay;
    public Dictionary<string, object> Context;
}

public class StoryFlagSystem : ModSystem
{
    public Dictionary<int, List<StoryFlag>> PlayerFlags;  // player.whoAmI → flags
    public bool HasFlag(int playerID, string flagID) { ... }
    public void SetFlag(int playerID, string flagID, object context = null) { ... }
}
```

#### D-46：势力战争系统

**方案**：当两个势力关系降至 Hostile 时触发势力战争：
- 势力 NPC 主动攻击对方势力 NPC
- 玩家可选择阵营
- 战争结果影响世界格局

---

## 叙事链条图

```
觉醒 (D-40)
  │
  ├──► 选择势力 ──► 声望系统 ──► 声望事件链 (D-44)
  │       │                        │
  │       │                        ├──► 结盟 ──► 势力商店
  │       │                        │       │
  │       │                        │       └──► 背刺 (D-45) ──► 势力战争 (D-46)
  │       │                        │
  │       │                        └──► 敌对 ──► 悬赏 (D-41)
  │       │                                    │
  │       │                                    └──► 刺杀/被刺杀
  │       │
  │       └──► 情报网络 (D-43) ──► 权力斗争 (D-42)
  │                                       │
  │                                       ├──► 职位继承
  │                                       └──► 势力内乱
  │
  └──► 散修 ──► 中立声望 ──► 自由交易
          │
          └──► 情报贩子 ──► 情报网络 (D-43)
```

## 迁移路线图

| 步骤 | 内容 | 工作量 | 风险 | 优先级 |
|------|------|--------|------|--------|
| D-40 | 实现 AwakeningSystem（觉醒任务链） | 大 | 中 — 需要设计任务链 | **P0** |
| D-43 | 情报网络系统 | 大 | 中 — 需要设计情报模型 | P1 |
| D-41 | 悬赏系统叙事化 | 中 | 低 — 已有完整基础 | P2（降级） |
| D-42 | 权力斗争叙事化 | 中 | 低 — 已有完整基础 | P2（降级） |
| D-44 | 声望事件链 | 中 | 低 — 纯新增 | P2 |
| D-45 | 背刺与背叛叙事 | 小 | 低 — 已有基础 | P2 |
| D-47 | 玩家名声系统 | 中 | 中 — 需要与现有声望系统整合 | P2 |
| D-48 | 剧情分支记录 | 小 | 低 — 纯数据记录 | P2 |
| D-46 | 势力战争系统 | 大 | 高 — 复杂的 AI 和世界状态管理 | P3 |
