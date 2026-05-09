# CombatLayer 重构计划

> **目标**：理清战斗层的组件关系，统一 DaoWeapon 武器体系、GuProjectile 弹道行为、DaoHen 道痕冲突、GuNPCInfo 状态效果、ShaZhao 杀招配方之间的数据流。

---

## 现状分析

### 已实现的战斗系统

| 系统 | 文件 | 行数 | 状态 |
|------|------|------|------|
| [`DaoWeapon`](Content/Items/Weapons/Daos/DaoWeapon.cs) | 道器武器抽象基类（50+ 流派） | 221 行 | ✅ 完整实现 |
| [`DaoType`](Common/GuBehaviors/DaoType.cs) | 流派枚举（50 种） | 13 行 | ✅ 完整 |
| [`DaoEffectTags`](Common/GuBehaviors/DaoEffectTags.cs) | 效果标签枚举（Flags） | 31 行 | ✅ 完整 |
| [`DaoEffectSystem`](Common/GuBehaviors/DaoEffectSystem.cs) | 效果应用系统（DoT、减速、碎甲等） | 85 行 | ✅ 基础实现 |
| [`GuProjectileInfo`](Common/GlobalProjectiles/GuProjectileInfo.cs) | 投射物信息（效果标签、道痕倍率） | 33 行 | ✅ 基础实现 |
| [`GuNPCInfo`](Common/GlobalNPCs/GuNPCInfo.cs) | NPC 状态效果（DoT、减速、碎甲、虚弱、标记） | 80 行 | ✅ 基础实现 |
| [`DaoHenConflictSystem`](Common/Systems/DaoHenConflictSystem.cs) | 道痕冲突系统（16 种 DaoPath） | 311 行 | ✅ 完整实现 |
| [`DaoHenPlayer`](Common/Players/DaoHenPlayer.cs) | 玩家道痕积累（50 种 DaoType） | 37 行 | ✅ 基础实现 |
| [`BulletBehaviors`](Common/BulletBehaviors/) | 15 种弹道行为组件 | 多种 | ✅ 完整实现 |
| [`IKinematicProvider`](Common/GuBehaviors/IKinematicProvider.cs) | 运动学接口 | — | ✅ 存在 |
| [`IOnHitEffectProvider`](Common/GuBehaviors/IOnHitEffectProvider.cs) | 命中效果接口 | — | ✅ 存在 |
| [`ITacticalTriggerProvider`](Common/GuBehaviors/ITacticalTriggerProvider.cs) | 战术触发接口 | — | ✅ 存在 |
| [`TacticalTrigger`](Common/GuBehaviors/TacticalTrigger.cs) | 战术触发枚举 | — | ✅ 存在 |

### 关键发现

1. **DaoWeapon 体系已完整实现** — 50+ 种 DaoType，每种对应一个武器子类（`BanWeapon`、`BloodWeapon` 等），全部继承自 `DaoWeapon : GuWeaponItem`
2. **DaoWeapon 实现了三个接口** — `IKinematicProvider`（弹道运动学）、`IOnHitEffectProvider`（命中效果）、`ITacticalTriggerProvider`（战术触发）
3. **DaoHenConflictSystem 使用 16 种 DaoPath** — 与 `DaoType`（50 种）不同，`DaoPath` 是粗粒度的路径分类，通过 `RegisterDaoHenTag` 映射。**但 `DefaultDaoHenMap` 为空**，导致冲突检测无效
4. **DaoEffectSystem 只实现了基础效果** — 复杂效果（连锁弹射、标记、魅惑等）已预留但未实现（第 38 行注释）
5. **GuProjectileInfo 通过 GlobalProjectile 附加到每个投射物** — 存储效果标签、道痕倍率、来源 DaoType
6. **GuNPCInfo 通过 GlobalNPC 附加到每个 NPC** — 存储 DoT/减速/碎甲/虚弱/标记计时器
7. **BulletBehaviors 已有 15 种行为组件** — 弹道行为体系已完整，无需从零构建

---

## 重构计划

### Phase 0：立即修复（P0）

#### D-24：填充 DaoType ↔ DaoPath 映射表

**问题**：当前存在两套"道"概念：
- [`DaoType`](Common/GuBehaviors/DaoType.cs) — 50 种细粒度流派，用于武器分类和道痕积累
- [`DaoPath`](Common/Systems/DaoHenConflictSystem.cs:38) — 16 种粗粒度路径，用于道痕冲突检测

两者通过 `RegisterDaoHenTag` 映射，但 **`DefaultDaoHenMap` 为空**，导致 `DaoHenConflictSystem` 的冲突检测完全无效。

**方案**：填充完整映射表：

```csharp
// 在 DaoHenConflictSystem 中建立完整映射
public static class DaoPathMapping
{
    private static readonly Dictionary<DaoType, DaoPath> TypeToPath = new()
    {
        // 火系
        { DaoType.Fire, DaoPath.Fire },
        { DaoType.Lightning, DaoPath.Fire },
        { DaoType.War, DaoPath.Fire },
        // 冰系
        { DaoType.IceSnow, DaoPath.Ice },
        { DaoType.Water, DaoPath.Ice },
        // 力量系
        { DaoType.Bone, DaoPath.Force },
        { DaoType.Knife, DaoPath.Force },
        { DaoType.Sword, DaoPath.Force },
        // 风系
        { DaoType.Wind, DaoPath.Wind },
        { DaoType.Flying, DaoPath.Wind },
        // 血系
        { DaoType.Blood, DaoPath.Blood },
        { DaoType.LifeDeath, DaoPath.Blood },
        // 智慧系
        { DaoType.Wisdom, DaoPath.Wisdom },
        { DaoType.Info, DaoPath.Wisdom },
        // 月系
        { DaoType.Moon, DaoPath.Moon },
        { DaoType.Star, DaoPath.Moon },
        // 毒系
        { DaoType.Poison, DaoPath.Poison },
        { DaoType.Eating, DaoPath.Poison },
        // 木系
        { DaoType.Wood, DaoPath.Wood },
        { DaoType.Mud, DaoPath.Wood },
        // 土系
        { DaoType.Power, DaoPath.Earth },
        { DaoType.Practise, DaoPath.Earth },
        // 光系
        { DaoType.Light, DaoPath.Light },
        { DaoType.Gold, DaoPath.Light },
        // 暗系
        { DaoType.Dark, DaoPath.Dark },
        { DaoType.Shadow, DaoPath.Dark },
        { DaoType.Void, DaoPath.Dark },
        // 灵魂系
        { DaoType.Soul, DaoPath.Soul },
        { DaoType.Dream, DaoPath.Soul },
        { DaoType.Person, DaoPath.Soul },
        // 阵法系
        { DaoType.Rule, DaoPath.Formation },
        { DaoType.Tactical, DaoPath.Formation },
        // 治愈系
        { DaoType.Qi, DaoPath.Healing },
        { DaoType.LifeDeath, DaoPath.Healing },  // 生死兼具血与治愈
    };

    public static DaoPath GetPath(DaoType type)
        => TypeToPath.TryGetValue(type, out var path) ? path : DaoPath.Wisdom; // 默认 Wisdom
}
```

#### D-25：完善 DaoEffectSystem 的复杂效果

**问题**：`DaoEffectSystem.ApplyEffects` 中，复杂效果（Chain、Mark、MoonMark、Fear、Charm、Silence、Disarm、Pull、Push、DrainStat、QiRestore、Heal、Shield）已预留但未实现。

**P0 实现优先级**（按战斗体验影响排序）：
1. **Heal / Shield** — 基础生存效果，影响平衡性
2. **Chain** — 连锁弹射，丰富战斗策略
3. **Mark / MoonMark** — 标记引爆机制，核心玩法
4. **Fear / Charm** — 控制效果
5. **Pull / Push** — 位移效果

```csharp
// Chain 效果实现示例
private static void ApplyChain(NPC target, Player player, Projectile proj, float range, int count)
{
    if (count <= 0) return;
    var nearbyNPCs = Main.npc.Where(n => n.active && !n.friendly 
        && n.Distance(target.Center) < range && n.whoAmI != target.whoAmI);
    
    int chainCount = 0;
    foreach (var npc in nearbyNPCs)
    {
        if (chainCount >= count) break;
        // 创建连锁弹射投射物
        Projectile.NewProjectile(proj.GetSource_FromThis(), target.Center,
            (npc.Center - target.Center).SafeNormalize(Vector2.Zero) * 10f,
            proj.type, proj.damage / 2, proj.knockBack, proj.owner);
        chainCount++;
    }
}
```

---

### Phase 1：核心增强（P1）

#### D-26：GuProjectileInfo 数据流标准化

**问题**：当前 `GuProjectileInfo` 的数据由各武器在 `Shoot` 方法中手动设置，缺乏统一的数据流。

**方案**：利用 `DaoWeapon` 的三个接口自动填充 `GuProjectileInfo`：

```csharp
// 在 DaoWeapon 的 Shoot 方法中（或通过 GlobalProjectile.OnSpawn）
public override void OnSpawn(Projectile projectile, IEntitySource source)
{
    if (source is EntitySource_ItemUse itemSource && itemSource.Item.ModItem is DaoWeapon daoWeapon)
    {
        var info = projectile.GetGlobalProjectile<GuProjectileInfo>();
        info.SourceDao = daoWeapon.DaoType;
        info.DaoMultiplier = daoWeapon.DaoMultiplier;
        
        // 从 IOnHitEffectProvider 获取效果
        if (daoWeapon is IOnHitEffectProvider effectProvider)
        {
            info.EffectsOnHit = effectProvider.GetOnHitEffects();
            info.DoTDamage = effectProvider.DoTDamage;
            info.DoTDuration = effectProvider.DoTDuration;
            // ...
        }
    }
}
```

> **说明**：已有基本数据流，此任务为优化，非必要。

#### D-27：ShaZhao（杀招）配方系统（MVA 硬编码）

**问题**：杀招系统尚未实现。杀招是玩家将多个蛊虫组合释放的强力技能。

**MVA 方案**：硬编码 1-2 条杀招配方，验证系统可行性：

```csharp
public class ShaZhaoRecipe
{
    public string Name;
    public string Description;
    
    // 配方要求
    public Dictionary<DaoType, int> RequiredDaoTypes;  // 需要的道痕类型和数量
    public int MinRealmLevel;                           // 最低境界要求
    public int MinKongQiaoSlots;                        // 最低空窍槽位数
    
    // 效果
    public int BaseDamage;
    public float DamageMultiplier;
    public DaoEffectTags Effects;
    public int CooldownTicks;                           // 冷却时间（帧）
    
    // 释放条件
    public Func<Player, bool> AdditionalCondition;
    
    // 执行
    public Action<Player, Vector2, Vector2> Execute;
}

public class ShaZhaoSystem : ModSystem
{
    public static List<ShaZhaoRecipe> AllRecipes = new();
    public Dictionary<int, int> CooldownTimers;  // player.whoAmI → 剩余冷却
    
    public static void RegisterRecipe(ShaZhaoRecipe recipe) { ... }
    public bool TryExecute(Player player, ShaZhaoRecipe recipe) { ... }
    public List<ShaZhaoRecipe> GetAvailableRecipes(Player player) { ... }
}
```

#### D-28：道痕冲突可视化

**问题**：当前 `DaoHenConflictSystem` 的冲突检测是纯逻辑的，玩家无法直观看到哪些蛊虫存在道痕冲突。

**方案**：
1. 在 `KongQiaoUI` 中添加道痕冲突指示器（红色高亮冲突蛊虫）
2. 在 `DaosUI` 中添加道痕冲突详情面板
3. 添加冲突警告提示（`GetConflictDescription` 已实现，需要 UI 集成）

#### D-29：战术触发系统集成

**问题**：`ITacticalTriggerProvider` 接口已定义，但触发逻辑尚未集成到战斗循环中。

**方案**：

```csharp
// TacticalTrigger 枚举（已定义）
public enum TacticalTrigger
{
    OnHit,           // 命中时
    OnKill,          // 击杀时
    OnLowHealth,     // 低血量时
    OnDodge,         // 闪避时
    OnCombo,         // 连击时
    OnQiAbove,       // 真元高于阈值时
    OnQiBelow,       // 真元低于阈值时
}

// 在 GuProjectileInfo 或 DaoWeapon 中处理触发
public void CheckTacticalTriggers(Projectile projectile, NPC target, Player player)
{
    if (this is ITacticalTriggerProvider triggerProvider)
    {
        foreach (var trigger in triggerProvider.GetTriggers())
        {
            switch (trigger)
            {
                case TacticalTrigger.OnHit:
                    triggerProvider.ExecuteTrigger(trigger, player, target);
                    break;
                case TacticalTrigger.OnKill when !target.active:
                    triggerProvider.ExecuteTrigger(trigger, player, target);
                    break;
                // ...
            }
        }
    }
}
```

---

### Phase 2：远期规划（P2）

#### D-30：连击/Combo 系统

**方案**：添加连击计数器，根据连续命中不同 DaoType 的武器触发额外效果：
- 3 连击：额外伤害
- 5 连击：触发特殊效果
- 7 连击：解锁杀招

#### D-31：战斗日志

**方案**：记录战斗数据用于调试和平衡性分析：
- 每次命中的 DaoType、伤害、效果
- 道痕冲突触发次数
- 杀招使用频率

---

## 数据流架构

```
Player Input
    │
    ▼
DaoWeapon.Shoot()
    │
    ├──► IKinematicProvider ──► 弹道参数（速度、数量、散布）
    ├──► IOnHitEffectProvider ──► DaoEffectTags
    │       │
    │       ▼
    │   GuProjectileInfo (GlobalProjectile)
    │       │
    │       ▼
    │   Projectile.OnHitNPC
    │       │
    │       ├──► DaoEffectSystem.ApplyEffects ──► GuNPCInfo (DoT/减速/碎甲)
    │       │
    │       ├──► DaoHenPlayer.AddDaoHen ──► 道痕积累
    │       │
    │       └──► ITacticalTriggerProvider ──► 战术触发
    │
    └──► DaoHenConflictSystem ──► 冲突检测（空窍中蛊虫）
            │
            └──► KongQiaoPlayer ──► 被动效果
```

## 迁移路线图

| 步骤 | 内容 | 工作量 | 风险 | 优先级 |
|------|------|--------|------|--------|
| D-24 | 填充 DaoType↔DaoPath 映射 | 小 | 低 — 纯数据映射 | **P0**（冲突检测当前无效） |
| D-25 | 完善 DaoEffectSystem 效果 | 中 | 中 — 需要平衡性调整 | **P0**（提升战斗深度） |
| D-27 | ShaZhao 杀招配方（硬编码 1-2 条） | 中 | 中 — 需要设计配方格式 | P1 |
| D-29 | 战术触发系统集成 | 中 | 低 — 接口已定义 | P1 |
| D-28 | 道痕冲突可视化 | 中 | 低 — UI 集成 | P1 |
| D-26 | GuProjectileInfo 数据流标准化 | 中 | 低 — 纯重构 | P1（已有基础） |
| D-30 | 连击系统 | 大 | 中 — 需要设计连击规则 | P2 |
| D-31 | 战斗日志 | 小 | 低 — 纯调试工具 | P2 |
