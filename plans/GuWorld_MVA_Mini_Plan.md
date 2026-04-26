# MVA-Mini 实施计划

> 基于 GuWorld_Partial_Refactor_Analysis.md 的可执行步骤
> 目标：4个文件修改，验证「NPC行为不可预测」核心体验

---

## 实施步骤

### Step 1：新增 BeliefState 数据结构

**文件**：[`Content/NPCs/GuMasters/IGuMasterAI.cs`](Content/NPCs/GuMasters/IGuMasterAI.cs)

**修改内容**：
1. 在 `PerceptionContext` 结构后新增 `BeliefState` 结构
2. 在 `AttitudeContext` 中新增 `BeliefState` 字段
3. 在 `IGuMasterAI` 接口中新增 `UpdateBelief()` 方法声明

**关键代码**：
```csharp
public struct BeliefState
{
    public string PlayerName;
    public float RiskThreshold;         // [0,1] 初始0.9
    public float ConfidenceLevel;       // [0,1] 初始0
    public int ObservationCount;
    public float EstimatedPower;        // 对玩家实力估计
    public bool HasTraded;
    public bool HasFought;
    public bool WasDefeated;
}
```

---

### Step 2：修改 GuMasterBase 信念系统

**文件**：[`Content/NPCs/GuMasters/GuMasterBase.cs`](Content/NPCs/GuMasters/GuMasterBase.cs)

**修改内容**：
1. 新增字段：`Dictionary<string, BeliefState> PlayerBeliefs`
2. 新增方法：`UpdateBelief(NPC npc, PerceptionContext context)` — 每帧更新信念
3. 重写 `CalculateAttitude()` — 基于信念分布计算态度
4. 在 `AI()` 主循环中插入信念更新步骤

**信念更新逻辑**：
```
UpdateBelief():
  1. 获取当前目标玩家的信念状态
  2. 观察玩家行为：
     - 玩家生命低 → EstimatedPower 下调
     - 玩家使用高转蛊虫 → EstimatedPower 上调
     - 玩家击杀强敌 → EstimatedPower 上调
     - 玩家逃跑 → EstimatedPower 下调
  3. 更新 RiskThreshold 动态漂移：
     - 观察到玩家弱 → RiskThreshold 下降（更激进）
     - 观察到玩家强 → RiskThreshold 上升（更谨慎）
     - 被玩家击败 → RiskThreshold 大幅上升
     - 击败玩家 → RiskThreshold 小幅下降
  4. 增加 ConfidenceLevel（随观察次数递增）
```

**态度计算逻辑**：
```
CalculateAttitude() 基于信念:
  - RiskThreshold < 0.3 → Hostile（主动攻击）
  - RiskThreshold 0.3-0.5 → Wary（试探）
  - RiskThreshold 0.5-0.7 → Ignore（观望）
  - RiskThreshold 0.7-0.9 → Fearful（回避）
  - RiskThreshold > 0.9 → Flee（逃跑）
  - ConfidenceLevel 低时 → 行为更随机（加入随机偏移）
```

---

### Step 3：改造 GuYuePatrolGuMaster

**文件**：[`Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs`](Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs)

**修改内容**：
1. `SetDefaults()`: 移除 `townNPC = true`，设置 `friendly = false`
2. 保留 `CanChat()` 重写（态度友好时可对话）
3. 重写 `ExecuteCombatAI()` — 实现掠夺型战斗行为
4. 新增 `OnPlayerDeath()` 或利用 `OnKill()` 实现搜尸逻辑
5. 重写 `SpawnChance()` — 调整生成条件

**行为特征**：
- 掠夺型原型：面对弱者动手，面对强者回避，面对未知试探
- 击败玩家后：从玩家背包取走元石和未炼化蛊虫（模拟搜尸）
- 被击败后：下次遇到该玩家时 RiskThreshold 大幅上升

---

### Step 4：GuWorldPlayer 个体级信念接口

**文件**：[`Common/Players/GuWorldPlayer.cs`](Common/Players/GuWorldPlayer.cs)

**修改内容**：
1. 新增 `GetBeliefState(string playerName)` 静态辅助方法
2. 新增 `UpdateGlobalBelief(NPC npc, string playerName, BeliefState belief)` 方法
3. 信念数据持久化通过 NPC 自身的 SaveData/LoadData 完成

---

## 不修改的文件清单

| 文件 | 理由 |
|------|------|
| [`Common/Systems/GuWorldSystem.cs`](Common/Systems/GuWorldSystem.cs) | 家族关系网络保持不变 |
| [`Common/Systems/WorldEventSystem.cs`](Common/Systems/WorldEventSystem.cs) | 世界事件系统保持不变 |
| [`Common/Players/QiPlayer.cs`](Common/Players/QiPlayer.cs) | 真元系统保持不变 |
| [`Common/Players/GuPerkSystem.cs`](Common/Players/GuPerkSystem.cs) | 永久增益系统保持不变 |
| 所有蛊虫物品文件 | 物品系统保持不变 |
| 所有城镇NPC文件 | 城镇NPC保持不变 |

---

## 验证清单

- [ ] 新手首次遇到巡逻蛊师 → 观望/保持距离
- [ ] 玩家持有高价值物品 → 信念更新，NPC试探
- [ ] 玩家生命值低 → RiskThreshold 下降，NPC动手
- [ ] 玩家反杀后再次遇到 → RiskThreshold 上升，NPC回避
- [ ] 多次交互后 → 同一NPC行为不一致
- [ ] 信念数据重启后保留（SaveData/LoadData）
