# Zero 基础蛊虫项目 — 情况说明

## 项目目标

为 50 种道 + 阴阳道（共 51 种）各设计一种**基础蛊虫**，作为该道的代表性示例蛊虫。每种蛊虫应通过**弹道行为 + 命中效果 + 数值参数**的组合，体现该道的核心特点。

## 架构概览（完全脱耦合 ✅）

```
蛊虫物品 (DaoWeapon + IOnHitEffectProvider)
    │
    ├─ 弹道参数 (IKinematicProvider) → 弹射物类型、速度、数量、散布
    ├─ 命中效果 (IOnHitEffectProvider) → 效果标签、持续时间、数值
    ├─ 战术触发 (ITacticalTriggerProvider) → 条件触发增益
    │
    └─ 弹射物 (BaseBullet + IBulletBehavior[])
         ├─ 飞行行为 (AimBehavior / HomingBehavior / WaveBehavior / GravityBehavior / BounceBehavior)
         ├─ 粒子拖尾 (DustTrailBehavior)
         ├─ 发光绘制 (GlowDrawBehavior)
         ├─ 拖尾轨迹 (TrailBehavior)
         ├─ 死亡粒子 (DustKillBehavior)
         └─ 旋转 (RotateBehavior)
```

**数据流**：`DaoWeapon.Shoot()` → 将 `IOnHitEffectProvider` 数据注入 `GuProjectileInfo`（GlobalProjectile）→ 弹射物命中时 `GuProjectileInfo.OnHitNPC()` 自动调用 `DaoEffectSystem.ApplyEffects()`

## 文件结构

```
Content/Items/Weapons/Zero/     ← 蛊虫物品文件（51个）
    FireBaseGu.cs
    WaterBaseGu.cs
    ... (每个道一个)
    YinYangBaseGu.cs
    README.md                   ← 本文件

Content/Projectiles/Zero/       ← 弹射物文件（51个）
    FireBaseProj.cs
    WaterBaseProj.cs
    ...
    YinYangBaseProj.cs
```

## 当前状态 ✅

所有 51 个蛊虫物品 + 51 个弹射物已生成，C# 编译通过。**脱耦合架构已验证成功**。

### 脱耦合核心设计（已验证）

```
┌──────────────────────────────────────────────────────────────────┐
│  三层接口分离（每个接口独立定义、独立实现、独立测试）              │
│                                                                    │
│  IKinematicProvider      → 只管"怎么飞"（弹道参数）               │
│  IOnHitEffectProvider    → 只管"打中后怎样"（命中效果）           │
│  ITacticalTriggerProvider → 只管"什么条件下变强"（战术触发）      │
│                                                                    │
│  三个接口在 DaoWeapon 中聚合，但各自独立运作，互不依赖。           │
│  修改弹道不影响命中效果，修改命中效果不影响战术触发。              │
└──────────────────────────────────────────────────────────────────┘
```

### 弹射物行为系统（组合模式）

```
BaseBullet (sealed 生命周期)
    │
    ├─ RegisterBehaviors() ← 子类唯一需要重写的方法
    │
    └─ List<IBulletBehavior>
         ├─ AimBehavior       (直线飞行)
         ├─ HomingBehavior    (追踪)
         ├─ WaveBehavior      (波浪)
         ├─ GravityBehavior   (重力)
         ├─ BounceBehavior    (反弹)
         ├─ DustTrailBehavior (粒子拖尾)
         ├─ GlowDrawBehavior  (发光)
         ├─ TrailBehavior     (拖尾线)
         ├─ DustKillBehavior  (死亡粒子)
         └─ RotateBehavior    (旋转)
```

### 待完成工作

1. **弹射物行为差异化**：每个道的 `RegisterBehaviors()` 使用不同的行为组合
2. **命中效果参数差异化**：每个道根据其特点设置独特的参数值
3. **占位贴图替换**：替换 2×2 白色 PNG 为正式贴图

## 可用行为组件

| 组件 | 类名 | 用途 | 关键参数 |
|------|------|------|---------|
| 自机狙 | `AimBehavior` | 直线飞行 | Speed, AutoRotate, EnableLight, LightColor |
| 追踪 | `HomingBehavior` | 追踪敌人 | Speed, TrackingWeight, Range, AutoRotate |
| 波浪 | `WaveBehavior` | 正弦波轨迹 | Amplitude, Frequency |
| 重力 | `GravityBehavior` | 抛物线/重力下落 | Acceleration, MaxFallSpeed, GravityDirection |
| 反弹 | `BounceBehavior` | 碰撞反弹 | MaxBounces, BounceFactor |
| 粒子拖尾 | `DustTrailBehavior` | 飞行粒子 | DustType, SpawnChance, DustScale, NoGravity |
| 发光绘制 | `GlowDrawBehavior` | 发光效果 | GlowColor, GlowLayers, EnableLight |
| 拖尾轨迹 | `TrailBehavior` | 拖尾线 | (通过 TrailManager 配置) |
| 死亡粒子 | `DustKillBehavior` | 销毁时粒子 | DustType, DustCount, DustSpeed |
| 旋转 | `RotateBehavior` | 自转 | RotationSpeed |

## 可用命中效果（DaoEffectTags）

| 标签 | 效果 | 适用道 |
|------|------|--------|
| `DoT` | 持续灼烧伤害 | 火、毒、暗、死、血 |
| `Slow` | 减速 | 水、冰、时、土 |
| `Freeze` | 冻结 | 冰 |
| `ArmorShred` | 破甲 | 金、剑、战 |
| `Weaken` | 虚弱（减攻） | 暗、月、幻 |
| `LifeSteal` | 吸血 | 血、生 |
| `Chain` | 连锁传导 | 雷、空 |
| `Mark` | 标记 | 星、月、暗 |
| `Heal` | 治疗友方 | 木、命、生、阴阳 |
| `Shield` | 护盾 | 土、金 |
| `Fear` | 恐惧 | 梦、暗 |
| `Charm` | 魅惑 | 梦、爱 |
| `Pull` | 牵引 | 空、风 |
| `Push` | 击退 | 风、战 |
| `Blind` | 致盲 | 光、幻 |
| `Silence` | 沉默 | 音、律 |
| `Disarm` | 缴械 | 盗、巧 |
| `QiRestore` | 回蓝 | 气、智 |

## 各道基础信息

| # | DaoType | 中文道名 | 基类 | 当前效果 | 当前弹道 |
|---|---------|---------|------|---------|---------|
| 0 | Ban | 盘 | BanWeapon | DoT | Aim |
| 1 | Blood | 血 | BloodWeapon | LifeSteal | Aim |
| 2 | Bone | 骨 | BoneWeapon | ArmorShred | Aim |
| 3 | Charm | 魅 | CharmWeapon | Charm | Aim |
| 4 | Cloud | 云 | CloudWeapon | Slow | Aim |
| 5 | Dark | 暗 | DarkWeapon | DoT+Weaken | Aim |
| 6 | Draw | 画 | DrawWeapon | 无 | Aim |
| 7 | Dream | 梦 | DreamWeapon | Fear+Charm | Aim |
| 8 | Eating | 食 | EatingWeapon | LifeSteal | Aim |
| 9 | Fire | 火 | FireWeapon | DoT | Aim |
| 10 | Flying | 飞 | FlyingWeapon | 无 | Aim |
| 11 | Gold | 金 | GoldWeapon | ArmorShred+Shield | Aim |
| 12 | IceSnow | 冰雪 | IceSnowWeapon | Freeze | Aim |
| 13 | Info | 讯 | InfoWeapon | 无 | Aim |
| 14 | Killing | 杀 | KillingWeapon | DoT | Aim |
| 15 | Knife | 匕 | KnifeWeapon | 无 | Aim |
| 16 | LifeDeath | 生死 | LifeDeathWeapon | DoT+Heal | Aim |
| 17 | Light | 光 | LightWeapon | Blind | Aim |
| 18 | Lightning | 雷 | LightningWeapon | DoT+Slow | Aim |
| 19 | Love | 爱 | LoveWeapon | Heal | Aim |
| 20 | Luck | 运 | LuckWeapon | 无 | Aim |
| 21 | Moon | 月 | MoonWeapon | Weaken | Aim |
| 22 | Mud | 泥 | MudWeapon | Slow+Shield | Aim |
| 23 | Pellet | 弹 | PelletWeapon | 无 | Aim |
| 24 | Person | 人 | PersonWeapon | Heal | Aim |
| 25 | Poison | 毒 | PoisonWeapon | DoT+Slow | Aim |
| 26 | Power | 力 | PowerWeapon | ArmorShred | Aim |
| 27 | Practise | 修 | PractiseWeapon | Heal | Aim |
| 28 | Qi | 气 | QiWeapon | QiRestore | Aim |
| 29 | Rule | 律 | RuleWeapon | Silence | Aim |
| 30 | Shadow | 影 | ShadowWeapon | DoT+Weaken | Aim |
| 31 | Sky | 天 | SkyWeapon | 无 | Aim |
| 32 | Slave | 奴 | SlaveWeapon | Weaken | Aim |
| 33 | Soul | 魂 | SoulWeapon | LifeSteal | Aim |
| 34 | Space | 空 | SpaceWeapon | Pull | Aim |
| 35 | Star | 星 | StarWeapon | Mark | Aim |
| 36 | Stealing | 盗 | StealingWeapon | Disarm | Aim |
| 37 | SuccessFailure | 成败 | SuccessFailureWeapon | 无 | Aim |
| 38 | Sword | 剑 | SwordWeapon | ArmorShred | Aim |
| 39 | Tactical | 策 | TacticalWeapon | 无 | Aim |
| 40 | Time | 时 | TimeWeapon | Slow | Aim |
| 41 | Unreal | 幻 | UnrealWeapon | Weaken | Aim |
| 42 | Variation | 变 | VariationWeapon | 无 | Aim |
| 43 | Voice | 音 | VoiceWeapon | Silence | Aim |
| 44 | Void | 虚 | VoidWeapon | DoT | Aim |
| 45 | War | 战 | WarWeapon | ArmorShred+Push | Aim |
| 46 | Water | 水 | WaterWeapon | Slow | Aim |
| 47 | Wind | 风 | WindWeapon | Push | Aim |
| 48 | Wisdom | 智 | WisdomWeapon | QiRestore | Aim |
| 49 | Wood | 木 | WoodWeapon | Heal | Aim |
| 50 | YinYang | 阴阳 | YinYangWeapon | DoT+Heal | Aim |

## 讨论/修改流程

1. 选择要讨论的道（如 "火道"）
2. 描述你对这个蛊虫的视觉/玩法想象
3. 我会根据现有架构提出实现方案
4. 确认后生成/修改对应文件
5. 编译验证

## 关键接口参考

### IOnHitEffectProvider
```csharp
public interface IOnHitEffectProvider
{
    DaoEffectTags[] OnHitEffects { get; }     // 效果标签组合
    float DoTDuration { get; }                 // 灼烧持续时间（秒）
    float DoTDamage { get; }                   // 灼烧每跳伤害
    float SlowPercent { get; }                 // 减速比例（0~1）
    int SlowDuration { get; }                  // 减速持续时间（帧）
    float ArmorShredAmount { get; }            // 破甲值
    int ArmorShredDuration { get; }            // 破甲持续时间（帧）
    float WeakenPercent { get; }               // 虚弱比例（0~1）
    float LifeStealPercent { get; }            // 吸血比例（0~1）
    void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage);
}
```

### BaseBullet.RegisterBehaviors()
```csharp
protected override void RegisterBehaviors()
{
    // 可以组合多个行为
    Behaviors.Add(new HomingBehavior(speed: 12f, trackingWeight: 1f/15f));
    Behaviors.Add(new DustTrailBehavior(DustID.Torch, 2) { DustScale = 1.5f });
    Behaviors.Add(new GlowDrawBehavior() { GlowColor = Color.OrangeRed });
}
```

### DaoWeapon 子类需要重写的属性
```csharp
protected override int qiCost => 8;           // 真气消耗
protected override int _useTime => 20;        // 使用间隔（帧）
protected override int _guLevel => 1;         // 蛊虫等级
protected override int controlQiCost => 5;    // 控制真气消耗
protected override float unitConntrolRate => 25; // 控制率
```