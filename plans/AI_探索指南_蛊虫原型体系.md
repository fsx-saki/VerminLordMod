# 蛊虫原型体系 — AI 探索指南

> 本文档供 AI Agent 快速理解蛊虫原型体系的设计原则与现状。
> 按顺序阅读"必读文件"即可建立完整心智模型。

---

## 一、四层脱耦合架构总览

```
┌──────────────────────────────────────────────────────────────────┐
│  L4 原型蛊层（Zero/）                                            │
│  "一道的技术储备库" — 攻击模式切换、弹幕选择、Shoot 分发、持久化     │
│  实现 IOnHitEffectProvider / IKinematicProvider / ITactical...   │
├──────────────────────────────────────────────────────────────────┤
│  L3 流派视觉层（Daos/）                                          │
│  "手持时冒什么颜色的烟" — 仅重写 moddustType（虚影粒子类型）        │
├──────────────────────────────────────────────────────────────────┤
│  L2 蛊虫基类 + 效果接口层                                        │
│  GuWeaponItem：炼化、真元消耗、等级惩罚、空窍炼入                   │
│  GuBehaviors/：IOnHitEffectProvider → DaoEffectSystem            │
│                IKinematicProvider / ITacticalTriggerProvider      │
│                DaoEffectTags（21 种效果位掩码）                    │
│                DaoType（51 种道枚举）                              │
├──────────────────────────────────────────────────────────────────┤
│  L1 弹幕行为组合层（BulletBehaviors/）                            │
│  "怎么动、怎么画" — BaseBullet + IBulletBehavior（50+ 行为组件）   │
│  sealed 生命周期 + 声明式 RegisterBehaviors() + 行为间零耦合       │
└──────────────────────────────────────────────────────────────────┘
```

**核心脱耦合原则**：

1. **行为与效果分离** — 弹幕层只管运动/视觉，效果层只管命中后状态。通过 `GuProjectileInfo`（GlobalProjectile）桥接。
2. **标签驱动** — `DaoEffectTags` 位掩码让效果组合变成集合运算（`DoT | LifeSteal`），无需为每种组合写子类。
3. **组合优于继承** — 弹幕用 `RegisterBehaviors()` 声明式组合行为，而非继承树。
4. **接口隔离** — `IOnHitEffectProvider`、`IKinematicProvider`、`ITacticalTriggerProvider` 各管一摊，按需实现。
5. **模式内聚** — 攻击模式切换逻辑封装在原型蛊内部，数组索引切换弹幕/速度/伤害倍率。

---

## 二、必读文件（按顺序）

### 第 1 步：理解弹幕行为组合系统（L1）

| 顺序 | 文件 | 关注点 |
|------|------|--------|
| 1 | `Common/BulletBehaviors/IBulletBehavior.cs` | 行为接口定义：OnSpawn/Update/OnHitNPC/OnKill/PreDraw/OnTileCollide |
| 2 | `Common/BulletBehaviors/BaseBullet.cs` | sealed 生命周期 + `RegisterBehaviors()` 抽象方法 + 行为链执行 |
| 3 | `Content/Projectiles/Zero/FireBaseProj.cs` | 火道基础弹幕：Gravity+Bounce+ExplosionKill+LiquidTrail 组合范例 |
| 4 | `Content/Projectiles/Zero/WaterBaseProj.cs` | 水道基础弹幕：Gravity+ParticleBody+WaterTrail+NormalBurst+SuppressDraw |
| 5 | `Content/Projectiles/Zero/IceSnowBaseProj.cs` | 冰道基础弹幕：Gravity+IceTrail+IceCrystalPlace+KillOnContact+SuppressDraw |

**关键认知**：弹幕不知道自己是"什么道"，它只是行为的组合。火弹幕和水弹幕的区别在于组合了不同的 Behavior，而非继承不同的基类。

### 第 2 步：理解效果接口层（L2）

| 顺序 | 文件 | 关注点 |
|------|------|--------|
| 6 | `Common/GuBehaviors/DaoEffectTags.cs` | 21 种效果标签的位掩码枚举 |
| 7 | `Common/GuBehaviors/IOnHitEffectProvider.cs` | 命中效果接口：OnHitEffects 标签 + 数值参数 + CustomOnHitNPC |
| 8 | `Common/GuBehaviors/DaoEffectSystem.cs` | 静态分发器：根据 tags 位掩码自动 Apply 对应效果 |
| 9 | `Common/GlobalProjectiles/GuProjectileInfo.cs` | 桥接枢纽：弹幕命中时读取效果标签 → 调用 DaoEffectSystem + TacticalTriggerSystem |
| 10 | `Common/GuBehaviors/DaoType.cs` | 51 种道枚举 |
| 11 | `Common/GuBehaviors/IKinematicProvider.cs` | 弹道参数接口 |
| 12 | `Common/GuBehaviors/ITacticalTriggerProvider.cs` | 战术触发接口 |
| 13 | `Common/GuBehaviors/TacticalTrigger.cs` | 11 种战术事件枚举 |
| 14 | `Common/GuBehaviors/TacticalTriggerSystem.cs` | 战术触发系统：连击/背击/击杀等条件触发 Buff |
| 15 | `Common/GuBehaviors/ShaZhaoSystem.cs` | 杀招配方系统：道痕需求 + 境界检查 + 冷却 + 执行 |

**关键认知**：效果从武器端声明（`IOnHitEffectProvider`），通过 `GuProjectileInfo` 写入弹幕，命中时由 `DaoEffectSystem` 统一分发。弹幕层和效果层互不引用。

### 第 3 步：理解蛊虫基类和流派层（L2-L3）

| 顺序 | 文件 | 关注点 |
|------|------|--------|
| 16 | `Content/Items/Weapons/GuWeaponItem.cs` | 蛊虫武器基类：炼化系统、真元消耗、等级惩罚、空窍炼入、虚影粒子 |
| 17 | `Content/Items/Weapons/Daos/FireWeapon.cs` | 流派层范例：仅重写 `moddustType` |
| 18 | `Content/Items/Weapons/Daos/WaterWeapon.cs` | 同上 |
| 19 | `Content/Items/Weapons/Daos/IceSnowWeapon.cs` | 同上 |

**关键认知**：Daos/ 下 51 个流派类每个仅重写一个字段（`moddustType`），是纯视觉标记层，不承载战斗逻辑。

### 第 4 步：理解原型蛊实现层（L4）

| 顺序 | 文件 | 关注点 |
|------|------|--------|
| 20 | `Content/Items/Weapons/Zero/FireBaseGu.cs` | **最完整的范例**：5 模式 + R 键切换 + Shoot 分发 + 持久化 |
| 21 | `Content/Items/Weapons/Zero/WaterBaseGu.cs` | 6 模式 + IsMouseSpawnMode 判断 |
| 22 | `Content/Items/Weapons/Zero/IceSnowBaseGu.cs` | 5 模式 + 散射/领域/暴风雪特殊生成 |
| 23 | `Content/Items/Weapons/Zero/LightningBaseGu.cs` | 5 模式（较简） |
| 24 | `Content/Items/Weapons/Zero/WindBaseGu.cs` | 5 模式（较简） |
| 25 | `Content/Items/Weapons/Zero/BloodBaseGu.cs` | **未完成范例**：仅 1 种弹幕，无模式切换 |

---

## 三、原型蛊的通用模式

每个原型蛊遵循相同的结构模板：

```csharp
public class XxxBaseGu : XxxWeapon, IOnHitEffectProvider
{
    // 1. 基础属性重写
    protected override int qiCost => 8;
    protected override int _useTime => 20;
    protected override int _guLevel => 1;
    protected override int controlQiCost => 5;
    protected override float unitConntrolRate => 25;

    // 2. 攻击模式系统
    public int attackMode = 0;
    private static readonly string[] AttackModeNames = { ... };
    private int[] _modeProjectileTypes;
    private readonly float[] _modeShootSpeeds = { ... };
    private readonly float[] _modeDamageMultipliers = { ... };
    private readonly int[] _modeUseTimes = { ... };

    // 3. IOnHitEffectProvider 实现
    public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Xxx };
    public float DoTDuration => 3f;
    // ... 其他效果参数
    public void CustomOnHitNPC(...) { }

    // 4. SetDefaults：初始化弹幕类型数组 + 默认模式
    // 5. UpdateInventory：R 键切换检测
    // 6. SwitchAttackMode：模式切换 + 属性更新 + 提示
    // 7. ModifyTooltips：显示当前模式
    // 8. CanUseItem：同步弹幕类型
    // 9. Shoot：根据模式分发弹幕生成逻辑
    // 10. SaveData/LoadData：持久化 attackMode
}
```

---

## 四、数据流全景

```
玩家按左键
  → GuWeaponItem.CanUseItem()（检查炼化/等级/真元）
  → GuWeaponItem.Shoot()
    → XxxBaseGu.Shoot()（根据 attackMode 选择弹幕类型和生成方式）
      → Projectile.NewProjectile()
        → BaseBullet.OnSpawn() → RegisterBehaviors() → 各 Behavior.OnSpawn()
        → BaseBullet.AI() → 各 Behavior.Update()（运动/视觉/碰撞）
        → 命中 NPC
          → BaseBullet.OnHitNPC() → 各 Behavior.OnHitNPC()
          → GuProjectileInfo.OnHitNPC()
            → DaoEffectSystem.ApplyEffects()（DoT/Slow/Freeze/...）
            → TacticalTriggerSystem.OnProjectileHit()（连击/背击/...）
        → 销毁
          → BaseBullet.OnKill() → 各 Behavior.OnKill()（爆炸/碎片/...）

玩家按 R 键
  → XxxBaseGu.UpdateInventory() → SwitchAttackMode()
    → attackMode 索引 +1
    → 更新 Item.shoot / shootSpeed / damage
    → CombatText 提示
```

---

## 五、当前实现状态

| 原型蛊 | 模式数 | 弹幕文件数 | 状态 |
|--------|--------|-----------|------|
| FirebaseGu | 5 | 10+ | ✅ 完整 |
| WaterBaseGu | 6 | 10+ | ✅ 完整 |
| IceSnowBaseGu | 5 | 6 | ✅ 完整 |
| LightningBaseGu | 5 | 5 | ⚠️ 框架在，弹幕较简 |
| WindBaseGu | 5 | 5 | ⚠️ 框架在 |
| DarkBaseGu | 4 | 4 | ⚠️ 框架在（1 个被注释） |
| BloodBaseGu | 1 | 1 | ❌ 仅基础弹幕，无模式切换 |
| 其余 44 个 | 0 | 1 | ❌ 骨架（仅 XxxBaseProj 占位） |

---

## 六、扩展现有原型蛊的步骤

给一个未完成的 Dao 添加新攻击模式：

1. **创建弹幕**：在 `Content/Projectiles/Zero/` 中新建类，继承 `BaseBullet`，在 `RegisterBehaviors()` 中组合行为
2. **添加模式条目**：在对应 `XxxBaseGu` 的数组中添加新条目（AttackModeNames / _modeProjectileTypes / _modeShootSpeeds / _modeDamageMultipliers / _modeUseTimes）
3. **处理 Shoot 分支**：如果新模式需要在鼠标位置生成或特殊弹道，在 `Shoot()` 中添加对应分支
4. **设置效果标签**：在 `IOnHitEffectProvider.OnHitEffects` 中声明命中效果

---

## 七、关键目录速查

| 目录 | 内容 |
|------|------|
| `Common/BulletBehaviors/` | 弹幕行为组件（50+ 个） |
| `Common/GuBehaviors/` | 效果接口与系统 |
| `Common/GlobalProjectiles/` | GuProjectileInfo（弹幕效果桥接） |
| `Common/GlobalNPCs/GuNPCInfo.cs` | NPC 端效果状态存储 |
| `Content/Items/Weapons/Daos/` | 51 个流派视觉层（仅 moddustType） |
| `Content/Items/Weapons/Zero/` | 51 个原型蛊实现 |
| `Content/Items/Weapons/GuWeaponItem.cs` | 蛊虫武器基类 |
| `Content/Projectiles/Zero/` | 原型蛊弹幕（90+ 个） |
| `Content/Dusts/` | 51 种道域粒子 |
