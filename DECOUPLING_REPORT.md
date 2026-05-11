# VerminLordMod 脱耦合设计报告

> 生成日期：2026-05-11
> 范围：全项目架构审查 + NPC 脱耦合方案

---

## 目录

1. [总体架构概览](#1-总体架构概览)
2. [已脱耦合部分：弹幕系统（参考模板）](#2-已脱耦合部分弹幕系统参考模板)
3. [已脱耦合部分：召唤物系统](#3-已脱耦合部分召唤物系统)
4. [NPC 系统现状与耦合分析](#4-npc-系统现状与耦合分析)
5. [NPC 脱耦合方案设计](#5-npc-脱耦合方案设计)
6. [其他子系统耦合分析](#6-其他子系统耦合分析)
7. [全局数据流与事件总线](#7-全局数据流与事件总线)
8. [脱耦合优先级路线图](#8-脱耦合优先级路线图)

---

## 1. 总体架构概览

```
VerminLordMod/
├── Content/                          # 具体内容（物品、NPC、弹射物、Buff等）
│   ├── Items/Weapons/
│   │   ├── Zero/          ← 51个基础蛊虫（已脱耦合 ✅）
│   │   ├── Daos/          ← DaoWeapon 基类 + 51个道武器
│   │   ├── One/~Six/      ← 各转数蛊虫武器
│   │   └── GuWeaponItem.cs ← 蛊虫武器基类
│   ├── NPCs/
│   │   ├── GuMasters/     ← 蛊师基类 + IGuMasterAI 接口
│   │   ├── GuYue/         ← 古月家族NPC（继承 GuMasterBase）
│   │   ├── Boss/          ← Boss（直接继承 ModNPC）
│   │   ├── Enemy/         ← 普通敌人（直接继承 ModNPC）
│   │   └── Town/          ← 城镇NPC
│   └── Projectiles/Zero/  ← 51个基础弹射物
│
└── Common/                          # 通用系统（行为、系统、全局钩子）
    ├── BulletBehaviors/   ← 弹幕行为组件（组合模式 ✅）
    ├── SummonBehaviors/   ← 召唤物行为组件（组合模式 ✅）
    ├── GuBehaviors/       ← 蛊虫行为（DaoEffect、TacticalTrigger、ShaZhao）
    ├── GlobalNPCs/        ← 全局NPC钩子（GuNPCInfo、GlobalNPCLoot等）
    ├── GlobalProjectiles/ ← 全局弹射物钩子（GuProjectileInfo）
    ├── Systems/           ← 世界级系统（WorldStateMachine、DialogueSystem等）
    ├── Players/           ← ModPlayer（QiResource、DaoHen、KongQiao等）
    ├── Events/            ← 事件总线（EventBus）
    ├── DialogueTree/      ← 对话树系统
    └── Search/            ← 搜索系统
```

### 核心设计模式

| 模式 | 应用位置 | 状态 |
|------|---------|------|
| **组合模式** | BulletBehaviors, SummonBehaviors | ✅ 已脱耦合 |
| **接口分离** | IKinematicProvider, IOnHitEffectProvider, ITacticalTriggerProvider | ✅ 已脱耦合 |
| **模板方法** | GuMasterBase (AI: Perceive→Belief→Decide→Execute) | ⚠️ 部分耦合 |
| **事件总线** | EventBus (Publish/Subscribe) | ✅ 已脱耦合 |
| **数据驱动** | GuYueNPCConfig (NPC属性配置) | ✅ 已脱耦合 |
| **策略模式** | GuAttitudeHelper.CalculateFromBelief | ✅ 已脱耦合 |

---

## 2. 已脱耦合部分：弹幕系统（参考模板）

### 2.1 架构

```
┌─────────────────────────────────────────────────────────────────┐
│  DaoWeapon (物品层)                                              │
│  ├─ IKinematicProvider      → 弹道参数（速度/数量/散布）         │
│  ├─ IOnHitEffectProvider    → 命中效果（DoT/Slow/ArmorShred等）  │
│  └─ ITacticalTriggerProvider → 战术触发（低血/夜晚/空中等）      │
│                                                                   │
│  Shoot() → GuProjectileInfo (GlobalProjectile)                   │
│       ↓                                                           │
│  OnHitNPC() → DaoEffectSystem.ApplyEffects()                     │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  BaseBullet (弹射物层)                                           │
│  ├─ sealed OnSpawn/AI/OnHitNPC/OnKill/PreDraw/OnTileCollide     │
│  └─ List<IBulletBehavior>  ← 组合模式                           │
│       ├─ AimBehavior       (直线飞行)                            │
│       ├─ HomingBehavior    (追踪)                                │
│       ├─ WaveBehavior      (波浪)                                │
│       ├─ GravityBehavior   (重力)                                │
│       ├─ BounceBehavior    (反弹)                                │
│       ├─ DustTrailBehavior (粒子拖尾)                            │
│       ├─ GlowDrawBehavior  (发光绘制)                            │
│       ├─ TrailBehavior     (拖尾轨迹)                            │
│       ├─ DustKillBehavior  (死亡粒子)                            │
│       └─ RotateBehavior    (旋转)                                │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 脱耦合关键设计

1. **接口分离**：三个接口（IKinematicProvider / IOnHitEffectProvider / ITacticalTriggerProvider）各自独立，修改弹道不影响命中效果
2. **组合模式**：BaseBullet 通过 `List<IBulletBehavior>` 组合行为，子类只需 `RegisterBehaviors()`
3. **sealed 生命周期**：BaseBullet 锁定 OnSpawn/AI/OnHitNPC 等方法，防止子类破坏行为系统
4. **数据注入**：物品层通过 GlobalProjectile 注入命中数据，弹射物层不感知物品

### 2.3 可复用模式

这个模式可以直接复用到 NPC 系统：

```
弹幕系统                     →    NPC 系统
─────────────────────────────────────────────
IBulletBehavior (接口)       →    INPCBehavior (接口)
BaseBullet (sealed 基类)     →    NPCBehaviorHost (sealed 基类)
RegisterBehaviors()          →    RegisterBehaviors()
AimBehavior / HomingBehavior →    IdleBehavior / PatrolBehavior / CombatBehavior
GuProjectileInfo (数据注入)  →    NPCBehaviorData (数据注入)
```

---

## 3. 已脱耦合部分：召唤物系统

### 3.1 架构

```
SummonMinion (sealed 生命周期)
    │
    ├─ RegisterBehaviors() ← 子类唯一需要重写
    │
    └─ List<ISummonBehavior>
         ├─ FollowMovement   (跟随移动)
         ├─ OrbitMovement    (环绕移动)
         ├─ ChargeMovement   (冲锋移动)
         └─ Styles/FlyStyle  (飞行风格)
```

### 3.2 与弹幕系统的对比

| 特性 | BaseBullet | SummonMinion |
|------|-----------|-------------|
| 生命周期 | 一次性（发射→命中→销毁） | 持久（生成→跟随→攻击→...） |
| Update 参数 | `Update(Projectile)` | `Update(Projectile, Player owner)` |
| 内置功能 | 无 | CheckActive (Buff管理) |
| 行为数量 | 10+ | 4 |

---

## 4. NPC 系统现状与耦合分析

### 4.1 继承层次

```
ModNPC
  └─ GuMasterBase (abstract)          ← 蛊师基类，实现 IGuMasterAI
       ├─ GuYueNPCBase (abstract)     ← 古月家族基类
       │    ├─ GuYueChief             ← 族长
       │    ├─ GuYueSchoolElder       ← 学堂家老
       │    ├─ GuYueMedicineElder     ← 药堂家老
       │    ├─ GuYueDefenseElder      ← 御堂家老
       │    ├─ GuYueChiElder          ← 赤脉家老
       │    ├─ GuYueMoElder           ← 漠脉家老
       │    ├─ GuYueMedicinePulseElder← 药脉家老
       │    ├─ GuYueFirstTurnGuMaster ← 一转蛊师
       │    ├─ GuYueSecondTurnGuMaster← 二转蛊师
       │    ├─ GuYueFistInstructor    ← 拳脚教头
       │    ├─ GuYueServant           ← 杂役
       │    └─ GuYueCommoner          ← 凡人
       └─ GuYuePatrolGuMaster        ← 巡逻蛊师（野外敌对）
```

### 4.2 当前耦合问题

#### 问题 1：GuMasterBase 过于庞大（~600行）

[GuMasterBase.cs](file:///home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/Content/NPCs/GuMasters/GuMasterBase.cs) 包含了：
- AI 主循环（Perceive → Belief → Decide → Execute）
- 信念系统（PlayerBeliefs 字典）
- 态度计算
- 决策逻辑
- 行为执行（Idle/Approach/Combat/Talk）
- 交互处理（Talk/Trade/Attack/Provoke/Ally/Betray/Bribe）
- 对话系统
- 弹幕保护系统
- 战斗AI
- 震惊特效
- 盟友警报

**耦合表现**：修改战斗AI会影响对话系统，修改交互处理会影响AI决策。

#### 问题 2：IGuMasterAI 接口过于庞大

[IGuMasterAI.cs](file:///home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/Content/NPCs/GuMasters/IGuMasterAI.cs) 定义了 10 个方法：
- Perceive / UpdateBelief / GetBelief
- Decide / CalculateAttitude
- ExecuteAI / HandleInteraction
- GetDialogue / GetChatButtons / OnChatButtonClicked

**耦合表现**：所有蛊师NPC必须实现全部10个方法，即使某些NPC不需要战斗AI。

#### 问题 3：GuYueNPCBase 与 GuMasterBase 紧耦合

[GuYueNPCBase.cs](file:///home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/Content/NPCs/GuYue/GuYueNPCBase.cs) 继承 GuMasterBase，但：
- 重写了 SetDefaults 将 NPC 设为 townNPC
- 重写了对话系统
- 添加了 GuYueNPCConfig 数据驱动
- 添加了同家族保护

**耦合表现**：GuYueNPCBase 依赖 GuMasterBase 的所有实现，无法独立测试。

#### 问题 4：Boss/Enemy 直接继承 ModNPC，无法复用

[ElectricWolfKing.cs](file:///home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/Content/NPCs/Boss/ElectricWolfKing.cs) 和 [ElectricWolf.cs](file:///home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/Content/NPCs/Enemy/ElectricWolf.cs) 直接继承 ModNPC：
- Boss 有复杂的多阶段AI（~400行硬编码）
- Enemy 使用原版 AI（NPCAIStyleID.Unicorn）
- 无法复用 GuMasterBase 的信念/态度/决策系统

#### 问题 5：GlobalNPC 分散在多个文件中

```
Common/GlobalNPCs/
├── GuNPCInfo.cs        ← DoT/Slow/ArmorShred/Weaken/Mark/Shield/Fear/Charm/Silence/Disarm
├── GlobalNPCLoot.cs    ← 掉落表（~200行硬编码）
├── GlobalDeadMsg.cs    ← 死亡消息
└── GlobalWolf.cs       ← 狼潮刷怪率
```

**耦合表现**：GuNPCInfo 直接操作 NPC 的 velocity/damage，与 NPC 自身 AI 可能冲突。

---

## 5. NPC 脱耦合方案设计

### 5.1 核心思路：参考弹幕系统的组合模式

```
当前架构（紧耦合）:
  GuMasterBase (600行)
    ├── AI 主循环
    ├── 信念系统
    ├── 态度计算
    ├── 决策逻辑
    ├── 行为执行
    ├── 交互处理
    ├── 对话系统
    └── 弹幕保护

目标架构（脱耦合）:
  NPCBehaviorHost (sealed 生命周期, ~100行)
    │
    ├── RegisterBehaviors() ← 子类唯一需要重写
    │
    └── List<INPCBehavior>
         ├── BeliefBehavior      (信念系统)
         ├── AttitudeBehavior    (态度计算)
         ├── DecisionBehavior    (决策逻辑)
         ├── MovementBehavior    (移动：Idle/Patrol/Approach/Flee)
         ├── CombatBehavior      (战斗AI)
         ├── InteractionBehavior (交互处理)
         ├── DialogueBehavior    (对话系统)
         ├── ProtectionBehavior  (弹幕保护)
         └── SocialBehavior      (盟友警报/社交网络)
```

### 5.2 INPCBehavior 接口设计

```csharp
/// <summary>
/// NPC 行为接口 — 所有 NPC 行为必须实现此接口。
/// 通过组合模式，一个 NPC 可以同时拥有多个行为。
/// </summary>
public interface INPCBehavior
{
    /// <summary>行为名称（调试用）</summary>
    string Name { get; }

    /// <summary>NPC 生成时调用</summary>
    void OnSpawn(NPC npc);

    /// <summary>每帧更新（在基类AI之前调用）</summary>
    void PreAI(NPC npc);

    /// <summary>每帧更新（在基类AI之后调用）</summary>
    void PostAI(NPC npc);

    /// <summary>NPC 被物品击中时调用</summary>
    void OnHitByItem(NPC npc, Player player, Item item, NPC.HitModifiers modifiers);

    /// <summary>NPC 被弹射物击中时调用</summary>
    bool? CanBeHitByProjectile(NPC npc, Projectile projectile);

    /// <summary>NPC 死亡时调用</summary>
    void OnKill(NPC npc);

    /// <summary>是否可以对话</summary>
    bool? CanChat(NPC npc);

    /// <summary>获取对话文本</summary>
    string GetChat(NPC npc);

    /// <summary>设置对话按钮</summary>
    void SetChatButtons(NPC npc, ref string button, ref string button2);

    /// <summary>对话按钮点击</summary>
    void OnChatButtonClicked(NPC npc, bool firstButton, ref string shop);
}
```

### 5.3 NPCBehaviorHost 设计

```csharp
/// <summary>
/// NPC 行为宿主 — 所有使用行为系统的 NPC 应继承此类。
/// 通过 sealed 关键字锁定生命周期方法，确保行为系统正确执行。
/// </summary>
public abstract class NPCBehaviorHost : ModNPC
{
    protected List<INPCBehavior> Behaviors { get; } = new();

    protected abstract void RegisterBehaviors();

    public sealed override void OnSpawn(NPC npc)
    {
        RegisterBehaviors();
        foreach (var b in Behaviors) b.OnSpawn(npc);
        OnSpawned(npc);
    }

    public sealed override void AI()
    {
        foreach (var b in Behaviors) b.PreAI(NPC);
        OnAI();
        foreach (var b in Behaviors) b.PostAI(NPC);
    }

    public sealed override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        foreach (var b in Behaviors) b.OnHitByItem(NPC, player, item, modifiers);
    }

    public sealed override bool? CanBeHitByProjectile(Projectile projectile)
    {
        bool? result = null;
        foreach (var b in Behaviors)
        {
            var r = b.CanBeHitByProjectile(NPC, projectile);
            if (r.HasValue) result = r.Value;
        }
        return result;
    }

    public sealed override void OnKill()
    {
        foreach (var b in Behaviors) b.OnKill(NPC);
        OnKilled();
    }

    // ... 其他 sealed 方法

    // 扩展点
    protected virtual void OnSpawned(NPC npc) { }
    protected virtual void OnAI() { }
    protected virtual void OnKilled() { }
}
```

### 5.4 行为组件拆分

| 当前 GuMasterBase 中的功能 | 拆分为独立 Behavior | 说明 |
|---|---|---|
| AI 主循环 (Perceive→Belief→Decide→Execute) | `AILoopBehavior` | 核心AI循环 |
| 信念系统 (PlayerBeliefs, UpdateBelief) | `BeliefBehavior` | 黑暗森林信念 |
| 态度计算 (CalculateAttitude) | `AttitudeBehavior` | 基于信念的态度 |
| 决策逻辑 (Decide) | `DecisionBehavior` | 态度→决策 |
| 行为执行 (ExecuteAI: Idle/Approach/Combat/Talk) | `MovementBehavior` | 移动逻辑 |
| 战斗AI (ExecuteCombatAI) | `CombatBehavior` | 战斗逻辑 |
| 交互处理 (HandleInteraction) | `InteractionBehavior` | Talk/Trade/Attack等 |
| 对话系统 (GetDialogue/SetChatButtons) | `DialogueBehavior` | 对话 |
| 弹幕保护 (ProjectileProtectionEnabled) | `ProtectionBehavior` | 弹幕保护 |
| 盟友警报 (AlertNearbyAllies) | `SocialBehavior` | 社交网络 |
| 震惊特效 (SpawnShockEffect) | `VisualBehavior` | 视觉效果 |
| 同家族保护 (CanBeHitByNPC/CanHitNPC) | `FactionBehavior` | 阵营保护 |

### 5.5 迁移路径

```
阶段 1：创建 INPCBehavior 接口 + NPCBehaviorHost 基类
阶段 2：将 GuMasterBase 的功能逐个拆分为 Behavior 组件
阶段 3：GuMasterBase 改为继承 NPCBehaviorHost，在 RegisterBehaviors 中注册组件
阶段 4：GuYueNPCBase 继承 NPCBehaviorHost，注册古月家族特有 Behavior
阶段 5：Boss/Enemy 也可以继承 NPCBehaviorHost，复用 Behavior 组件
```

### 5.6 具体示例

```csharp
// 迁移后的 GuMasterBase
public abstract class GuMasterBase : NPCBehaviorHost
{
    public abstract FactionID GetFaction();
    public abstract GuRank GetRank();
    public abstract GuPersonality GetPersonality();

    protected override void RegisterBehaviors()
    {
        // 核心AI
        Behaviors.Add(new BeliefBehavior(GetFaction(), GetRank(), GetPersonality()));
        Behaviors.Add(new AttitudeBehavior());
        Behaviors.Add(new DecisionBehavior());
        Behaviors.Add(new MovementBehavior());

        // 交互
        Behaviors.Add(new InteractionBehavior());
        Behaviors.Add(new DialogueBehavior());

        // 保护
        Behaviors.Add(new ProtectionBehavior());
        Behaviors.Add(new FactionBehavior(GetFaction()));

        // 社交
        Behaviors.Add(new SocialBehavior());

        // 子类可添加额外行为
        RegisterExtraBehaviors();
    }

    protected virtual void RegisterExtraBehaviors() { }
}

// 迁移后的 GuYuePatrolGuMaster
public class GuYuePatrolGuMaster : GuMasterBase
{
    public override FactionID GetFaction() => FactionID.GuYue;
    public override GuRank GetRank() => GuRank.Zhuan1_Gao;
    public override GuPersonality GetPersonality() => GuPersonality.Cautious;

    protected override void RegisterExtraBehaviors()
    {
        // 巡逻蛊师特有：掠夺行为
        Behaviors.Add(new LootBehavior());
        // 巡逻行为
        Behaviors.Add(new PatrolBehavior(patrolRadius: 200f));
    }
}

// Boss 也可以复用
public class ElectricWolfKing : NPCBehaviorHost
{
    protected override void RegisterBehaviors()
    {
        Behaviors.Add(new BossAIBehavior());
        Behaviors.Add(new MultiStageBehavior());
        Behaviors.Add(new MinionSpawnBehavior());
        Behaviors.Add(new CombatBehavior { Damage = 50, Defense = 15 });
    }
}
```

---

## 6. 其他子系统耦合分析

### 6.1 GlobalNPC 系统

| 文件 | 耦合问题 | 脱耦合方案 |
|------|---------|-----------|
| [GuNPCInfo.cs](file:///home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/Common/GlobalNPCs/GuNPCInfo.cs) | 直接操作 NPC.velocity/damage，与 NPC AI 可能冲突 | 改为通过事件通知 NPC，由 NPC 自己的 Behavior 处理 |
| [GlobalNPCLoot.cs](file:///home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/Common/GlobalNPCs/GlobalNPCLoot.cs) | ~200行硬编码掉落表 | 抽取为 LootTable 数据配置，支持 JSON 加载 |
| [GlobalDeadMsg.cs](file:///home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/Common/GlobalNPCs/GlobalDeadMsg.cs) | 硬编码特定 NPC 死亡消息 | 改为事件订阅：`EventBus.Subscribe<NPCDeathEvent>(OnNPCDeath)` |
| [GlobalWolf.cs](file:///home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/Common/GlobalNPCs/GlobalWolf.cs) | 直接修改刷怪率 | 已通过 WolfSystem 管理，耦合度低 ✅ |

### 6.2 Systems 系统

| 系统 | 耦合度 | 说明 |
|------|--------|------|
| WorldStateMachine | 低 ✅ | 统一状态管理，通过静态方法访问 |
| DialogueSystem | 中 ⚠️ | 直接操作 GuMasterBase 的 Belief，应通过接口 |
| NpcDeathHandler | 中 ⚠️ | 直接访问 KongQiaoPlayer/QiResourcePlayer，应通过事件 |
| NPCSocialNetwork | 低 ✅ | 独立的关系图管理 |
| InvestigationChainSystem | 低 ✅ | 独立的调查状态管理 |
| ShaZhaoSystem | 中 ⚠️ | 硬编码杀招配方，应抽取为数据配置 |
| WolfSystem | 低 ✅ | 独立的狼潮管理 |

### 6.3 Players 系统

| ModPlayer | 耦合度 | 说明 |
|-----------|--------|------|
| QiResourcePlayer | 低 ✅ | 独立的真元管理 |
| QiRealmPlayer | 低 ✅ | 独立的境界管理 |
| DaoHenPlayer | 低 ✅ | 独立的道痕管理 |
| KongQiaoPlayer | 低 ✅ | 独立的空窍管理 |
| GuWorldPlayer | 低 ✅ | 独立的声望管理 |
| ShieldPlayer | 低 ✅ | 独立的护盾管理 |
| TacticalTriggerPlayer | 低 ✅ | 独立的战术触发状态追踪 |

### 6.4 物品系统

| 组件 | 耦合度 | 说明 |
|------|--------|------|
| GuWeaponItem | 低 ✅ | 蛊虫武器基类，独立管理炼化/真元 |
| DaoWeapon | 低 ✅ | 三层接口分离，已脱耦合 |
| GuMediumWeapon | 未检查 | 需要审查 |

---

## 7. 全局数据流与事件总线

### 7.1 当前数据流

```
物品 (DaoWeapon)
  │  Shoot()
  ▼
GuProjectileInfo (GlobalProjectile)
  │  OnHitNPC()
  ▼
DaoEffectSystem.ApplyEffects()
  │
  ▼
GuNPCInfo (GlobalNPC)
  │  PostAI() — 每帧应用效果
  ▼
NPC (velocity/damage 修改)
```

### 7.2 事件总线使用情况

[EventBus.cs](file:///home/fsx/.local/share/Terraria/tModLoader/ModSources/VerminLordMod/Common/Events/EventBus.cs) 当前使用场景：

| 事件 | 发布者 | 订阅者 |
|------|--------|--------|
| PlayerDeathEvent | NpcDeathHandler | (待订阅) |
| NPCDeathEvent | NpcDeathHandler | BountySystem, PowerStructureSystem |
| BountyPostedEvent | NpcDeathHandler | BountySystem |

**评价**：事件总线设计良好 ✅，但使用场景还不够广泛。建议扩展：
- NPC 被攻击事件 → SocialBehavior 订阅（盟友警报）
- NPC 态度变化事件 → DialogueSystem 订阅（对话更新）
- 物品炼化完成事件 → 成就系统订阅

---

## 8. 脱耦合优先级路线图

### P0（立即执行 — 架构基础）

| 任务 | 工作量 | 影响范围 |
|------|--------|---------|
| 创建 `INPCBehavior` 接口 | 小 | 所有NPC |
| 创建 `NPCBehaviorHost` 基类 | 中 | 所有NPC |
| 将 `GuNPCInfo` 的效果应用改为事件通知 | 小 | GlobalNPC |

### P1（短期 — 核心重构）

| 任务 | 工作量 | 影响范围 |
|------|--------|---------|
| 拆分 `GuMasterBase` 为 Behavior 组件 | 大 | GuMasterBase + 所有子类 |
| `GuMasterBase` 改为继承 `NPCBehaviorHost` | 大 | 继承链 |
| `GuYueNPCBase` 改为继承 `NPCBehaviorHost` | 中 | 古月家族NPC |
| 抽取 `GlobalNPCLoot` 为数据配置 | 中 | 掉落系统 |

### P2（中期 — 扩展复用）

| 任务 | 工作量 | 影响范围 |
|------|--------|---------|
| Boss/Enemy 继承 `NPCBehaviorHost` | 中 | Boss + Enemy |
| 扩展 EventBus 使用场景 | 小 | 全局 |
| ShaZhaoSystem 配方数据化 | 中 | 杀招系统 |

### P3（长期 — 完全脱耦合）

| 任务 | 工作量 | 影响范围 |
|------|--------|---------|
| DialogueSystem 通过接口而非具体类型交互 | 中 | 对话系统 |
| NpcDeathHandler 通过事件而非直接调用 | 小 | 死亡处理 |
| 所有 GlobalNPC 通过事件总线通信 | 中 | GlobalNPC |

---

## 附录 A：现有脱耦合模式总结

```
已脱耦合 ✅：
  1. 弹幕行为系统 (IBulletBehavior + BaseBullet) — 组合模式
  2. 召唤物行为系统 (ISummonBehavior + SummonMinion) — 组合模式
  3. 物品命中效果 (IOnHitEffectProvider + DaoEffectSystem) — 接口分离
  4. 物品弹道参数 (IKinematicProvider) — 接口分离
  5. 物品战术触发 (ITacticalTriggerProvider) — 接口分离
  6. NPC 属性配置 (GuYueNPCConfig) — 数据驱动
  7. NPC 态度计算 (GuAttitudeHelper.CalculateFromBelief) — 策略模式
  8. 事件总线 (EventBus) — 发布/订阅
  9. NPC 社交网络 (NPCSocialNetwork) — 独立系统
  10. 调查链 (InvestigationChainSystem) — 独立系统

待脱耦合 ⚠️：
  1. GuMasterBase AI 系统 — 模板方法 → 组合模式
  2. GlobalNPCLoot 掉落表 — 硬编码 → 数据配置
  3. GuNPCInfo 效果应用 — 直接操作 → 事件通知
  4. ShaZhaoSystem 杀招配方 — 硬编码 → 数据配置
  5. DialogueSystem 对话 — 直接操作 Belief → 接口交互
  6. Boss/Enemy AI — 直接继承 ModNPC → NPCBehaviorHost
```

## 附录 B：关键文件索引

| 文件 | 路径 | 行数 | 职责 |
|------|------|------|------|
| BaseBullet.cs | Common/BulletBehaviors/ | 122 | 弹幕行为宿主 |
| IBulletBehavior.cs | Common/BulletBehaviors/ | 53 | 弹幕行为接口 |
| SummonMinion.cs | Common/SummonBehaviors/ | 154 | 召唤物行为宿主 |
| ISummonBehavior.cs | Common/SummonBehaviors/ | 61 | 召唤物行为接口 |
| GuMasterBase.cs | Content/NPCs/GuMasters/ | ~600 | 蛊师NPC基类 |
| IGuMasterAI.cs | Content/NPCs/GuMasters/ | 320 | 蛊师AI接口 |
| GuYueNPCBase.cs | Content/NPCs/GuYue/ | 261 | 古月家族NPC基类 |
| GuYueNPCEnums.cs | Content/NPCs/GuYue/ | 256 | 古月NPC配置数据 |
| DaoWeapon.cs | Content/Items/Weapons/Daos/ | ~200 | 道武器基类 |
| GuWeaponItem.cs | Content/Items/Weapons/ | 195 | 蛊虫武器基类 |
| GuNPCInfo.cs | Common/GlobalNPCs/ | 176 | 全局NPC效果 |
| GuProjectileInfo.cs | Common/GlobalProjectiles/ | 39 | 全局弹射物数据注入 |
| DaoEffectSystem.cs | Common/GuBehaviors/ | ~200 | 命中效果应用 |
| EventBus.cs | Common/Events/ | 80 | 事件总线 |
| WorldStateMachine.cs | Common/Systems/ | ~200 | 世界状态管理 |
| NpcDeathHandler.cs | Common/Systems/ | ~200 | NPC死亡处理 |
| NPCSocialNetwork.cs | Common/Systems/ | ~200 | NPC社交网络 |
| DialogueSystem.cs | Common/Systems/ | ~200 | 对话系统 |