# PlayerLayer 重构计划

> **目标**：理清玩家层的职责边界，统一玩家死亡处理链，为 NPC 感知系统和战斗系统提供稳定的玩家状态查询接口。

---

## 现状分析

### 已实现的玩家系统

| 系统 | 文件 | 行数 | 状态 |
|------|------|------|------|
| [`QiResourcePlayer`](Common/Players/QiResourcePlayer.cs) | 真元资源管理（消耗、回复、死亡清空） | 184 行 | ✅ 完整实现 |
| [`QiRealmPlayer`](Common/Players/QiRealmPlayer.cs) | 境界系统（等级、阶段、突破进度、觉醒） | 199 行 | ✅ 完整实现 |
| [`QiTalentPlayer`](Common/Players/QiTalentPlayer.cs) | 资质系统（影响修炼速度、突破成功率） | — | ✅ 已存在并实现 |
| [`KongQiaoPlayer`](Common/Players/KongQiaoPlayer.cs) | 空窍系统（蛊槽、炼蛊、提取、激活） | 340 行 | ✅ 完整实现 |
| [`ChunQiuChanPlayer`](Common/Players/ChunQiuChanPlayer.cs) | 春秋蝉重生保护 | 271 行 | ✅ 完整实现 |
| [`GuWorldPlayer`](Common/Players/GuWorldPlayer.cs) | 声望/通缉/结盟/背刺 | 395 行 | ✅ 完整实现 |
| [`DaoHenPlayer`](Common/Players/DaoHenPlayer.cs) | 道痕积累与倍率计算 | 37 行 | ✅ 基础实现 |
| [`GuPerkSystem`](Common/Players/GuPerkSystem.cs) | 永久增益系统 | — | ✅ 已存在并实现 |

### 关键发现

1. **`QiPlayer.cs` 已不存在** — 已被删除/重构，代码已拆分到 `QiResourcePlayer`、`QiRealmPlayer`、`QiTalentPlayer`
2. **`QiTalentPlayer` 已存在** — 资质系统已实现，无需新建
3. **`GuPerkSystem` 已存在** — 永久增益系统已实现，无需新建
4. **`DaoHenPlayer` 与 `DaoHenConflictSystem` 职责清晰** — `DaoHenPlayer` 管理玩家道痕积累（`DaoType`），`DaoHenConflictSystem` 管理蛊虫道痕冲突（`DaoPath`），通过 `DaoType` ↔ `DaoPath` 映射关联。映射表尚未填满，但那是战斗层的工作
5. **`KongQiaoPlayer` 的 `OnPlayerDeath` 处理了蛊虫丢失逻辑** — 但 `NpcDeathHandler.OnPlayerKilled` 也处理了掉落逻辑，存在重复

---

## 重构计划

### Phase 0：立即修复（P0）

#### D-09：统一玩家死亡处理流程

**问题**：玩家死亡时，以下逻辑分散在多处：
- [`KongQiaoPlayer.OnPlayerDeath()`](Common/Players/KongQiaoPlayer.cs:189) — 蛊虫逃跑/自毁/保留
- [`NpcDeathHandler.OnPlayerKilled()`](Common/Systems/NpcDeathHandler.cs:69) — 掉落物暴露、尸体创建
- [`QiResourcePlayer.OnDeathClearQi()`](Common/Players/QiResourcePlayer.cs:84) — 真元清空
- [`ChunQiuChanPlayer`](Common/Players/ChunQiuChanPlayer.cs) — 重生保护拦截

**方案**：创建统一的死亡事件处理链：

```csharp
// 在 GuWorldEvent 中新增
public class PlayerDyingEvent : GuWorldEvent
{
    public Player Player;
    public int? KillerNPCType;
    public bool IsRebirthProtected;  // 春秋蝉设置此标志
    public bool CancelDeath;         // 如果为 true，取消死亡
}

// 处理顺序：
// 1. ChunQiuChanPlayer 拦截 → 设置 IsRebirthProtected
// 2. KongQiaoPlayer 处理蛊虫
// 3. QiResourcePlayer 清空真元
// 4. NpcDeathHandler 处理掉落
```

---

### Phase 1：核心增强（P1）

#### D-13：KongQiaoPlayer 重构

**问题**：`KongQiaoPlayer` 目前 340 行，承担了过多职责：
- 蛊槽管理（TryRefineGu、TryExtractGu、SetGuActive）
- 真元占用计算（GetTotalQiOccupation、RecalculateQiOccupied）
- 死亡处理（OnPlayerDeath）
- 被动效果更新（PostUpdate）
- 投射物类型映射（GetGuProjectileType）

**方案**：拆分为关注点分离的结构：

```
KongQiaoPlayer (空窍管理器)
├── KongQiaoSlotManager ── 蛊槽增删改查
├── KongQiaoQiCalculator ── 真元占用计算
├── KongQiaoDeathHandler ── 死亡蛊虫处理
├── KongQiaoEffectApplier ── 被动效果应用
└── KongQiaoProjectileMapper ── 蛊虫→投射物映射
```

#### D-14：玩家状态快照系统

**方案**：为 NPC 感知系统提供统一的玩家状态快照：

```csharp
public struct PlayerStateSnapshot
{
    public int LifePercent;
    public int QiLevel;
    public int RealmLevel;
    public int ActiveGuCount;
    public float DaoHenMultiplier;
    public bool HasChunQiuChan;
    public int InfamyLevel;
    public FactionID[] AlliedFactions;
}
```

由 `PerceptionContext` 引用，避免每次感知时重复计算。

---

### Phase 2：远期规划（P2）

#### D-15：多周目继承

**方案**：添加玩家数据跨周目继承机制：
- 永久增益（GuPerkSystem）继承
- 部分道痕继承
- 声望重置但保留历史记录

---

## 依赖关系

```
PlayerLayer
├── QiResourcePlayer ─── 真元资源
├── QiRealmPlayer ────── 境界系统
├── QiTalentPlayer ───── 资质系统（已实现）
├── KongQiaoPlayer ───── 空窍系统
│   └── DaoHenConflictSystem ── 道痕冲突检测
├── ChunQiuChanPlayer ── 重生保护
├── GuWorldPlayer ────── 声望/通缉
├── DaoHenPlayer ─────── 道痕积累
├── GuPerkSystem ─────── 永久增益（已实现）
└── PlayerStateSnapshot(D-14) ── 状态快照
     └── PerceptionContext (NPCLayer)
```

## 迁移路线图

| 步骤 | 内容 | 工作量 | 风险 | 优先级 |
|------|------|--------|------|--------|
| D-09 | 统一死亡处理链 | 中 | 中 — 涉及多个 Player 和 System | **P0** |
| D-13 | KongQiaoPlayer 拆分 | 大 | 中 — 需要确保向后兼容 | P1 |
| D-14 | 玩家状态快照 | 小 | 低 — 纯新增 | P1 |
| D-15 | 多周目继承 | 大 | 高 — 需要设计继承策略 | P2 |

### 已取消/降级的原任务

| 原任务 | 原因 | 状态 |
|--------|------|------|
| D-10 明确 DaoHen 职责边界 | 全景报告确认 `DaoHenPlayer` 与 `DaoHenConflictSystem` 已职责清晰，映射表未填满是战斗层的事 | 降级为文档注解，无需代码修改 |
| D-11 实现 QiTalentPlayer | `QiTalentPlayer.cs` 已存在并实现 | 已删除 |
| D-12 实现 GuPerkSystem | `GuPerkSystem.cs` 已存在并实现 | 已删除 |
