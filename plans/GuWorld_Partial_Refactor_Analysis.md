# GuWorld 部分重构可行性分析报告

> 基于现有代码基座 vs 重构计划（黑暗森林 MVA）的差距分析
> 日期：2026-04-26

---

## 一、现有代码基座总览

### 1.1 已完成的核心骨架

| 文件 | 功能 | 状态 |
|------|------|------|
| [`Common/Systems/GuWorldSystem.cs`](Common/Systems/GuWorldSystem.cs) | 枚举定义（FactionID/RepLevel/GuRank/GuPersonality/GuAttitude）、家族关系网络、世界级数据持久化 | ✅ 完整 |
| [`Common/Players/GuWorldPlayer.cs`](Common/Players/GuWorldPlayer.cs) | 玩家声望/通缉/结盟/背刺系统、连锁反应、恶名机制 | ✅ 完整 |
| [`Content/NPCs/GuMasters/IGuMasterAI.cs`](Content/NPCs/GuMasters/IGuMasterAI.cs) | 蛊师AI接口（Perceive/Decide/ExecuteAI）、态度计算工具 | ✅ 完整 |
| [`Content/NPCs/GuMasters/GuMasterBase.cs`](Content/NPCs/GuMasters/GuMasterBase.cs) | 蛊师抽象基类（AI主循环/对话/战斗/商店/交互处理） | ✅ 完整 |
| [`Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs`](Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs) | 古月巡逻蛊师（巡逻AI/远程战斗/对话/商店/生成条件） | ✅ 完整 |
| [`Common/Systems/WorldEventSystem.cs`](Common/Systems/WorldEventSystem.cs) | 周期性世界事件（商队/兽潮/集会） | ✅ 完整 |

### 1.2 已有的蛊虫物品系统（独立于GuWorld）

| 文件 | 功能 | 状态 |
|------|------|------|
| [`Common/Players/QiPlayer.cs`](Common/Players/QiPlayer.cs) | 修为系统（一转~九转）、真元系统（恢复/消耗）、破境机制 | ✅ 完整 |
| [`Common/Players/GuPerkSystem.cs`](Common/Players/GuPerkSystem.cs) | 永久增益管理（白豕/黑豕/斤力/钧力/酒虫/寿命） | ✅ 完整 |
| [`Content/Items/Weapons/GuWeaponItem.cs`](Content/Items/Weapons/GuWeaponItem.cs) | 武器蛊虫基类（炼化系统/真元消耗/虚影特效） | ✅ 完整 |
| [`Content/Items/Accessories/GuAccessoryItem.cs`](Content/Items/Accessories/GuAccessoryItem.cs) | 饰品蛊虫基类（炼化系统/真元占据/防御限制） | ✅ 完整 |
| [`Content/Items/Consumables/GuConsumableItem.cs`](Content/Items/Consumables/GuConsumableItem.cs) | 消耗品蛊虫基类 | ✅ 完整 |
| 30+ 具体蛊虫物品 | 各种蛊虫实现（酒虫/豕蛊/力蛊/命蛊/沙砾等） | ✅ 完整 |

---

## 二、重构计划核心目标 vs 现有代码差距

### 2.1 重构计划的核心机制

| 机制 | 重构计划要求 | 现有代码状态 | 差距 |
|------|-------------|-------------|------|
| **空窍双轨制** | 已炼化蛊虫进入空窍（独立UI），未炼化留在背包 | ❌ 不存在 | 所有蛊虫都是背包Item，无空窍概念 |
| **真元预算制** | 启用蛊虫占据真元额度，软约束 | ⚠️ 部分 | QiPlayer有真元系统，但无「占据额度」概念 |
| **蛊虫状态** | 休眠/启用/催动三态 | ❌ 不存在 | 只有炼化/未炼化二态 |
| **媒介武器** | 空壳触发器，查询空窍发射弹幕 | ❌ 不存在 | 蛊虫直接作为武器使用 |
| **杀招配方** | 多蛊虫组合释放 | ❌ 不存在 | 每只蛊虫独立使用 |
| **信念黑箱** | NPC对玩家持概率分布，非点估计 | ❌ 不存在 | 当前是确定性态度计算 |
| **动态RiskThreshold** | 根据观察动态漂移 | ❌ 不存在 | 态度由声望+性格静态决定 |
| **无安全区** | 家族核心区不能公开战斗但可下毒 | ❌ 不存在 | 当前NPC是townNPC，默认安全 |
| **个体级信念** | 每个NPC对每个玩家独立信念 | ❌ 不存在 | 当前是全局声望系统 |
| **死亡惩罚** | 真元清空/背包搜刮/虚弱期 | ❌ 不存在 | 死亡只有真元减半 |
| **天外道痕开局** | 凡人→开窍事件→空窍石 | ❌ 不存在 | 当前开局直接有Hopeness+Info |

### 2.2 现有代码可直接复用的部分

| 现有组件 | 重构中的用途 | 需修改程度 |
|---------|-------------|-----------|
| [`QiPlayer`](Common/Players/QiPlayer.cs) 真元系统 | 真元预算制的底层机制 | 小改（增加占据额度字段） |
| [`GuWorldPlayer`](Common/Players/GuWorldPlayer.cs) 声望系统 | 信念系统的输入之一 | 中改（改为个体级信念存储） |
| [`GuMasterBase`](Content/NPCs/GuMasters/GuMasterBase.cs) AI框架 | 信念黑箱的载体 | 大改（替换态度计算为信念分布） |
| [`GuYuePatrolGuMaster`](Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs) | MVA验证NPC | 大改（改为敌对/非townNPC） |
| [`GuWeaponItem`](Content/Items/Weapons/GuWeaponItem.cs) 炼化系统 | 空窍中蛊虫的炼化状态 | 复用炼化逻辑 |
| [`GuPerkSystem`](Common/Players/GuPerkSystem.cs) | 永久增益 | 基本不变 |

---

## 三、部分重构可行性评估

### 3.1 可行且低风险的部分

#### ✅ **P0：NPC信念黑箱 + 动态RiskThreshold**

**理由**：
- 现有 [`GuMasterBase`](Content/NPCs/GuMasters/GuMasterBase.cs) 的 AI 主循环（Perceive→CalculateAttitude→Decide→ExecuteAI）结构完整
- 只需将 `CalculateAttitude()` 从确定性公式改为信念分布 + 动态漂移
- 不需要修改文件结构，只需修改 [`GuMasterBase.cs`](Content/NPCs/GuMasters/GuMasterBase.cs) 和 [`IGuMasterAI.cs`](Content/NPCs/GuMasters/IGuMasterAI.cs) 中的态度计算逻辑
- 新增 `BeliefState` 数据结构（对每个玩家的信念分布）

**影响范围**：2个文件修改 + 1个新数据结构

#### ✅ **P0：GuYuePatrolGuMaster 改为非townNPC + 敌对原型**

**理由**：
- 当前 [`GuYuePatrolGuMaster`](Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs) 是 `townNPC = true`，这限制了敌对行为
- 重构计划要求它是「掠夺型」原型，可攻击玩家
- 只需修改 `SetDefaults()` 中的 `NPC.friendly` 和 `NPC.townNPC`，重写AI行为
- 保留对话/交易能力（通过态度判断）

**影响范围**：1个文件修改

#### ✅ **P1：个体级信念存储**

**理由**：
- 当前 [`GuWorldPlayer`](Common/Players/GuWorldPlayer.cs) 已有 `FactionRelations`（家族级声望）
- 新增 `Dictionary<string, BeliefState>`（以玩家名为key的个体级信念）
- 信念数据存储在 NPC 实例中（通过 `SaveData/LoadData`）
- 与现有声望系统共存：声望作为信念的初始值

**影响范围**：1个文件修改（GuWorldPlayer）+ NPC数据持久化

### 3.2 可行但风险中等的部分

#### ⚠️ **P1：空窍数据结构（不含独立UI）**

**理由**：
- 重构计划要求「空窍双轨制」，但MVA阶段可以简化
- 方案：在 [`GuWorldPlayer`](Common/Players/GuWorldPlayer.cs) 中新增 `KongQiao` 数据结构
- 存储已炼化蛊虫的 ItemID + 状态（休眠/启用）
- 暂不实现独立UI，通过「空窍石」物品右键打开简单面板
- 真元占据额度：在 [`QiPlayer`](Common/Players/QiPlayer.cs) 中新增 `occupiedQi` 字段

**影响范围**：2-3个文件修改

#### ⚠️ **P1：真元预算制（软约束）**

**理由**：
- 现有 [`QiPlayer`](Common/Players/QiPlayer.cs) 已有 `qiMax`/`qiCurrent`/`qiRegenRate`
- 新增：`occupiedQi`（已占据真元）、`availableQi`（可用真元 = qiMax - occupiedQi）
- 启用蛊虫时：`occupiedQi += guQiCost`
- 停用时：`occupiedQi -= guQiCost`
- 攻击时额外消耗：从 `qiCurrent` 中扣除

**影响范围**：QiPlayer + GuWeaponItem/GuAccessoryItem

### 3.3 高风险/需大量工作的部分

#### ❌ **P2：媒介武器 + 杀招配方**

**理由**：
- 需要新建武器类型（媒介武器），与现有 `GuWeaponItem` 体系不同
- 杀招配方需要配方匹配系统
- 涉及弹幕发射逻辑重构
- **建议推迟到MVA验证通过后**

#### ❌ **P2：无安全区 + 明面/暗面规则**

**理由**：
- 需要修改NPC生成逻辑，判断区域类型
- 需要「下毒/嫁祸/诱导」等新交互类型
- 涉及复杂的位置判断和NPC行为逻辑
- **建议推迟**

#### ❌ **P3：天外道痕开局**

**理由**：
- 需要修改玩家初始物品（替换Hopeness+Info）
- 需要「开窍事件」触发机制
- 需要「野蛊」替换原版怪物
- **建议推迟**

#### ❌ **P3：死亡惩罚增强**

**理由**：
- 需要修改死亡逻辑（真元清空/虚弱期）
- 需要NPC搜尸系统
- 涉及多人模式同步
- **建议推迟**

---

## 四、推荐的部分重构范围

### MVA-Mini：最小可验证版本

只验证一个核心问题：**NPC的行为是否不可预测？**

#### 涉及文件（4个）

| 文件 | 修改类型 | 修改内容 |
|------|---------|---------|
| [`Content/NPCs/GuMasters/IGuMasterAI.cs`](Content/NPCs/GuMasters/IGuMasterAI.cs) | 修改 | 新增 `BeliefState` 结构、`RiskThreshold` 动态漂移接口 |
| [`Content/NPCs/GuMasters/GuMasterBase.cs`](Content/NPCs/GuMasters/GuMasterBase.cs) | 修改 | 替换 `CalculateAttitude()` 为信念分布计算；新增 `UpdateBelief()` 方法 |
| [`Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs`](Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs) | 修改 | 改为非townNPC；实现掠夺型原型；添加搜尸逻辑 |
| [`Common/Players/GuWorldPlayer.cs`](Common/Players/GuWorldPlayer.cs) | 修改 | 新增个体级信念存储接口 |

#### 不修改的文件

- [`Common/Systems/GuWorldSystem.cs`](Common/Systems/GuWorldSystem.cs) — 家族关系网络保持不变
- [`Common/Systems/WorldEventSystem.cs`](Common/Systems/WorldEventSystem.cs) — 世界事件系统保持不变
- [`Common/Players/QiPlayer.cs`](Common/Players/QiPlayer.cs) — 真元系统保持不变
- 所有蛊虫物品文件 — 保持不变

---

## 五、实施步骤

### Step 1：新增 BeliefState 数据结构

在 [`IGuMasterAI.cs`](Content/NPCs/GuMasters/IGuMasterAI.cs) 中新增：

```csharp
/// <summary> NPC对单个玩家的信念状态 </summary>
public struct BeliefState
{
    public string PlayerName;           // 玩家标识
    public float RiskThreshold;         // 风险阈值 [0,1]，初始0.9
    public float ConfidenceLevel;       // 置信度 [0,1]
    public int ObservationCount;        // 观察次数
    public float EstimatedPower;        // 对玩家实力的估计
    public bool HasTraded;              // 是否交易过
    public bool HasFought;              // 是否战斗过
    public bool WasDefeated;            // 是否被击败过
}
```

### Step 2：修改 GuMasterBase 态度计算

将 [`GuMasterBase.CalculateAttitude()`](Content/NPCs/GuMasters/GuMasterBase.cs:184) 从确定性公式改为信念驱动：

- 新增 `Dictionary<string, BeliefState>` 存储对每个玩家的信念
- `Perceive()` 中调用 `UpdateBelief()` 更新信念
- `CalculateAttitude()` 基于 `BeliefState.RiskThreshold` 和 `ConfidenceLevel` 计算态度
- 新增 `UpdateBelief()` 方法：根据观察更新 RiskThreshold 漂移

### Step 3：改造 GuYuePatrolGuMaster

- 移除 `townNPC = true`，改为 `friendly = false`（但保留对话能力）
- 实现掠夺型原型行为：
  - RiskThreshold 高时 → 观望/保持距离
  - RiskThreshold 低时 → 突然动手
  - 击败玩家后 → 搜刮背包（取走元石/未炼化蛊虫）
- 保留对话功能（通过 `CanChat()` 判断）

### Step 4：GuWorldPlayer 个体级信念接口

- 新增 `GetBelief(NPC npc)` / `SetBelief(NPC npc, BeliefState belief)` 方法
- NPC 的信念数据存储在 NPC 实例中（通过 ModNPC 的 SaveData/LoadData）

---

## 六、验证场景

| 场景 | 预期行为 | 验收标准 |
|------|---------|---------|
| 新手首次遇到 | RiskThreshold 0.9 → 观望/保持距离 | 玩家感受到「他在观察我，但没动手」 |
| 玩家展示酒虫 | 信念更新 → 试探（询问/接近） | 玩家感受到「他注意到酒虫了」 |
| 玩家暴露弱点 | RiskThreshold 降至 0.3 → 突然动手 | 玩家感受到「他是因为我弱才动手的」 |
| 玩家反杀后 | RiskThreshold 升至 0.95 → 回避 | 玩家感受到「他记得我，他怕了」 |
| 多次交互后 | 同一NPC不同阶段行为完全不同 | 玩家无法预测下一秒行为 |

---

## 七、不纳入本次重构的内容

以下内容虽然重要，但建议在 MVA-Mini 验证通过后再实施：

1. **空窍双轨制** — 需要独立UI，工作量大
2. **媒介武器 + 杀招配方** — 需要新武器体系
3. **无安全区 + 明面/暗面规则** — 需要区域判断系统
4. **天外道痕开局** — 需要修改初始流程
5. **死亡惩罚增强** — 需要搜尸系统
6. **其他家族NPC扩展** — 待信念系统验证后抽象基类再扩展

---

## 八、风险与缓解

| 风险 | 影响 | 缓解措施 |
|------|------|---------|
| 信念参数难以调优 | NPC行为不符合预期 | 增加调试用参数显示（Debugger物品） |
| 非townNPC导致对话系统失效 | 无法与NPC交互 | 保留 `CanChat()` 重写，态度友好时可对话 |
| 信念数据持久化 | 重启后信念丢失 | 使用 NPC SaveData/LoadData 持久化 |
| 多人模式信念同步 | 不同玩家看到不同行为 | 信念以NPC实例存储，天然隔离 |
