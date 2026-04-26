# 消耗类蛊虫永久增益逻辑分析与改进计划

> 基于《蛊真人》原文设定 + 当前代码审计

---

## 一、当前代码全景

### 1.1 消耗类蛊虫清单（`Content/Items/Consumables/`）

| 蛊虫 | 文件 | 增益类型 | 存储字段 | 原文设定 |
|------|------|---------|---------|---------|
| 白豕蛊 | [`ZizhiBing.cs`](Content/Items/Consumables/ZizhiBing.cs) | 永久 | `QiPlayer.whitePigs` (int) | 一转珍稀豕蛊，永久加力，改造蛊师身躯，从根本上增长气力。上限一猪之力 |
| 黑豕蛊 | (代码中无独立文件) | 永久 | `QiPlayer.blackPigs` (int) | 一转珍稀豕蛊，永久加力，可叠加白豕蛊 |
| 斤力蛊 | [`JinLiGu.cs`](Content/Items/Consumables/JinLiGu.cs) | 永久 | `QiPlayer.otherPigs` (int) | 增长蛊师力气 |
| 十斤之力蛊 | [`ShiJinLiGu.cs`](Content/Items/Consumables/ShiJinLiGu.cs) | 永久 | `QiPlayer.otherPigs` (int) | 增长蛊师力气 |
| 钧力蛊 | [`JunLiGu.cs`](Content/Items/Consumables/JunLiGu.cs) | 永久 | `QiPlayer.otherPigs` (int) | 四转力道蛊，增加力气 |
| 十钧之力蛊 | [`ShiJunLiGu.cs`](Content/Items/Consumables/ShiJunLiGu.cs) | 永久 | `QiPlayer.otherPigs` (int) | 四转力道蛊，增加力气 |
| 十命蛊 | [`TenLifeGu.cs`](Content/Items/Consumables/TenLifeGu.cs) | 永久 | `QiPlayer.extraAges` (int) | 增加寿命 |
| 百命蛊 | [`HundredLifeGu.cs`](Content/Items/Consumables/HundredLifeGu.cs) | 永久 | `QiPlayer.extraAges` (int) | 增加寿命 |
| 千命蛊 | [`ThousandLifeGu.cs`](Content/Items/Consumables/ThousandLifeGu.cs) | 永久 | `QiPlayer.extraAges` (int) | 增加寿命 |
| 一胎蛊 | [`OneMinion.cs`](Content/Items/Consumables/OneMinion.cs) | 永久 | `QiPlayer.hasOneMinion` (bool) | 增加召唤栏 |
| 龙珠蟋蟀 | [`DragonBallCricket.cs`](Content/Items/Consumables/DragonBallCricket.cs) | 永久 | `QiPlayer.extraV`, `extraAcc` (float) | 增加移动速度 |
| 花猪蛊 | [`FlowerPig.cs`](Content/Items/Consumables/FlowerPig.cs) | 临时Buff | `FlowerPigbuff` (300帧=5秒) | 一转豕蛊系列，临时加力，暴涨一猪之力 |
| 大力天牛 | [`StrengthLongicorn.cs`](Content/Items/Consumables/StrengthLongicorn.cs) | 临时Buff | `StrengthLongicornbuff` (300帧=5秒) | 一转力量型蛊虫，临时牛力 |
| 黄罗天牛 | [`HuangLuoLongicorn.cs`](Content/Items/Consumables/HuangLuoLongicorn.cs) | 临时Buff | `HuangLuoLongicornbuff` (1800帧=30秒) | 一转耐力型蛊虫，提升持续力 |
| 活叶 | [`LivingLeaf.cs`](Content/Items/Consumables/LivingLeaf.cs) | 即时回血 | 直接 `player.Heal(200)` | 治疗 |
| 资质甲/乙/丙/丁 | [`ZizhiJia.cs`](Content/Items/Consumables/ZizhiJia.cs)等 | 一次性 | `QiPlayer.PlayerZiZhi` (enum) | 开启空窍，设定资质 |
| 元石 | [`YuanS.cs`](Content/Items/Consumables/YuanS.cs) | 即时 | 直接回真元/破境 | 修炼资源 |
| 一转升二转 | [`FirstToSecond.cs`](Content/Items/Consumables/FirstToSecond.cs) | 一次性 | `QiPlayer.LevelUp()` | 突破境界 |
| 二转升三转 | [`SecondToThird.cs`](Content/Items/Consumables/SecondToThird.cs) | 一次性 | `QiPlayer.LevelUp()` | 突破境界 |

### 1.2 永久增益存储字段（`QiPlayer.cs`）

```csharp
// 力量类（三个独立字段，语义混乱）
public int whitePigs = 0;   // 白豕蛊 - 但斤力蛊系列也用了 otherPigs
public int blackPigs = 0;   // 黑豕蛊 - 无独立物品使用
public int otherPigs = 0;   // "其他力蛊" - 斤力蛊/钧力蛊系列

// 寿命类
public int extraAges = 0;   // 十命蛊/百命蛊/千命蛊共用

// 速度类
public float extraV = 0f;   // 龙珠蟋蟀
public float extraAcc = 0f; // 龙珠蟋蟀

// 召唤类
public bool hasOneMinion = false;  // 一胎蛊

// 酒虫系列（bool标记，无实际效果实现）
public bool hasWineBug = false;
public bool hasFourWineBug = false;
public bool hasSevenWineBug = false;
public bool hasNineWineBug = false;
```

### 1.3 增益生效路径

```
QiPlayer.ModifyWeaponDamage()  →  whitePigs/blackPigs/otherPigs → 伤害 += 值 * 0.01f
QiPlayer.ModifyWeaponKnockback() → whitePigs/blackPigs/otherPigs → 击退 += 值 * 0.01f
QiPlayer.ModifyMaxStats()      →  extraAges → 生命上限 += 值
QiPlayer.PostUpdateRunSpeeds() →  extraV/extraAcc → 移速/加速度
QiPlayer.PostUpdateEquips()    →  hasOneMinion → 召唤栏 += 1
```

---

## 二、核心问题清单

### 🔴 问题1：力量增益字段语义混乱

**现状**：三个字段 `whitePigs`、`blackPigs`、`otherPigs` 在 [`ModifyWeaponDamage`](Common/Players/QiPlayer.cs:157) 和 [`ModifyWeaponKnockback`](Common/Players/QiPlayer.cs:213) 中被完全相同的逻辑处理——三者都做 `damage += value * 0.01f`。

**问题**：
- `otherPigs` 命名含义模糊，实际存储的是"斤力蛊/钧力蛊"系列的叠加值
- 白豕蛊和黑豕蛊在原文中各有上限（一猪之力），但代码中无上限检查
- 三个字段在伤害计算中完全等价，没有区分意义

**原文依据**：
> "方源已经利用白豕蛊，为自己增添了一猪之力。如果他继续使用第二只白豕蛊，那么不会有任何力量增加的效果。" —— 白豕蛊有上限

> "黑、白豕蛊的能力，就是改造蛊师的身躯，从根本上增长蛊师的气力。" —— 永久改造

> "有斤力蛊、钧力蛊等，增长蛊师力气。" —— 不同体系

### 🔴 问题2：永久增益与临时Buff混用同一消耗品模式

**现状**：消耗品蛊虫使用了两种完全不同的增益机制：

| 机制 | 代表蛊虫 | 特点 |
|------|---------|------|
| **永久字段** | 白豕蛊、斤力蛊、命蛊 | 使用后修改 `QiPlayer` 字段，存档保存，永久有效 |
| **临时Buff** | 花猪蛊、大力天牛、黄罗天牛 | 使用后添加 `ModBuff`，持续几秒到几十秒 |

**问题**：
- 花猪蛊（原文"临时加力"）和大力天牛（原文"临时牛力"）的Buff持续时间仅5秒（300帧），在战斗中几乎无实际作用
- 黄罗天牛（原文"提升持续力"）的Buff持续30秒，但效果只是 `manaRegen += 1`，与原文"耐力"设定不符
- 没有统一的"临时增益"时长标准

**原文依据**：
> "花豕蛊的作用，和蛮力天牛蛊类似，都是临时性增加蛊师的力气。" —— 明确是临时

> "蛮力天牛蛊...一转力量型蛊虫，临时牛力" —— 明确是临时

### 🔴 问题3：酒虫系列有字段无实现

**现状**：`QiPlayer` 中定义了四个 bool 字段：
```csharp
public bool hasWineBug = false;
public bool hasFourWineBug = false;
public bool hasSevenWineBug = false;
public bool hasNineWineBug = false;
```

但这些字段：
- 在 `SaveData`/`LoadData` 中保存/加载
- 在 `resetAll` 中重置
- **没有任何地方读取并使用它们产生实际效果**

**原文依据**：
> "酒虫在元海中载沉载浮...精炼真元的一转蛊虫" —— 酒虫的核心作用是精炼真元

> "四味酒虫...二转增幅蛊虫，由酒虫合炼而来" —— 有升级路线

### 🔴 问题4：`IGu` 接口为空接口

**现状**：[`IGu.cs`](Content/Items/IGu.cs) 定义了一个空接口：
```csharp
public interface IGu { }
```

大量消耗品类实现了它，但没有任何契约约束。这导致：
- 每个消耗品自己实现 `CanUseItem`/`UseItem` 逻辑，大量重复代码
- 没有统一的"蛊虫消耗品"基类来规范真元消耗、蛊虫等级、炼化等行为
- 部分蛊虫（如 `WanShi`、`YuanS`、`WolfWaveCard`）甚至没有实现 `IGu`

### 🔴 问题5：真元消耗与蛊虫等级脱节

**现状**：每个蛊虫在 `SetDefaults` 中硬编码 `qiCost`，但：
- [`HundredLifeGu.cs`](Content/Items/Consumables/HundredLifeGu.cs:44) 中 `qiCost=20` 覆盖了构造函数中的 `qiCost=100`
- [`ThousandLifeGu.cs`](Content/Items/Consumables/ThousandLifeGu.cs:44) 同样 `qiCost=20` 覆盖了 `qiCost=1000`
- 蛊虫等级 `_guLevel` 只用于"强行调动"的伤害计算，没有与真元消耗挂钩

### 🔴 问题6：`ResetVariables` 会清除所有临时增益

**现状**：[`QiPlayer.ResetVariables()`](Common/Players/QiPlayer.cs:98) 在每帧 `ResetEffects` 中被调用：
```csharp
private void ResetVariables() {
    qiRegenRate = baseQiRegenRate;
    qiMax2 = qiMax;
}
```

这本身没问题，但永久增益字段（`whitePigs`、`extraAges` 等）**不会**被重置——这是正确的。然而没有清晰的注释说明哪些字段是"永久"、哪些是"每帧重置"，容易导致新开发者误用。

### 🔴 问题7：力量增益没有上限控制

**现状**：在 [`ModifyWeaponDamage`](Common/Players/QiPlayer.cs:157) 中虽然有 `LimitSth` 配置项做上限：
```csharp
damage += Utils.Clamp(whitePigs, 0, qiLevel * 10) * 0.01f;
```

但：
- 这只在配置开启时生效
- 上限基于 `qiLevel * 10`，与原文设定（白豕蛊上限一猪之力）无关
- 蛊虫使用本身没有上限检查，玩家可以无限叠加

---

## 三、改进计划

### 方案A：短期修复（低风险，快速见效）

#### A1. 修复 `qiCost` 被覆盖的 Bug

**文件**：[`HundredLifeGu.cs`](Content/Items/Consumables/HundredLifeGu.cs:44)、[`ThousandLifeGu.cs`](Content/Items/Consumables/ThousandLifeGu.cs:44)

**操作**：删除 `SetDefaults` 中的 `qiCost = 20;` 行，或修正为正确的值。

#### A2. 统一力量增益字段命名

**文件**：[`QiPlayer.cs`](Common/Players/QiPlayer.cs)

**操作**：
- `whitePigs` → `whitePigPower`（白豕蛊力量）
- `blackPigs` → `blackPigPower`（黑豕蛊力量）
- `otherPigs` → `jinLiPower`（斤力蛊系列力量）
- 添加 `const int MAX_WHITE_PIG_POWER = 1` 等上限常量

#### A3. 延长临时Buff持续时间

**文件**：[`FlowerPig.cs`](Content/Items/Consumables/FlowerPig.cs:55)、[`StrengthLongicorn.cs`](Content/Items/Consumables/StrengthLongicorn.cs:59)

**操作**：
- 花猪蛊：300帧 → 3600帧（1分钟）
- 大力天牛：300帧 → 3600帧（1分钟）
- 黄罗天牛：1800帧 → 7200帧（2分钟）

### 方案B：中期重构（推荐）

#### B1. 创建 `GuConsumableItem` 基类

**新文件**：`Content/Items/Consumables/GuConsumableItem.cs`

```csharp
public abstract class GuConsumableItem : ModItem
{
    public virtual int QiCost => 20;
    public virtual int GuLevel => 1;
    
    public override bool CanUseItem(Player player) {
        var qiPlayer = player.GetModPlayer<QiPlayer>();
        return qiPlayer.qiCurrent >= QiCost;
    }
    
    public override bool? UseItem(Player player) {
        var qiPlayer = player.GetModPlayer<QiPlayer>();
        if (GuLevel > qiPlayer.qiLevel) {
            // 强行调动惩罚
        }
        qiPlayer.qiCurrent -= QiCost;
        return true;
    }
}
```

**收益**：
- 消除所有消耗品中的重复 `CanUseItem`/`UseItem` 模板代码
- 统一真元消耗、蛊虫等级检查逻辑
- 废弃空接口 `IGu`

#### B2. 实现酒虫效果

**文件**：[`QiPlayer.cs`](Common/Players/QiPlayer.cs)

在 `UpdateResource` 中添加：
```csharp
if (hasWineBug) {
    // 酒虫：精炼真元，真元恢复速度 +1
    qiRegenRate += 1;
}
if (hasFourWineBug) {
    qiRegenRate += 2;
}
// ... 以此类推
```

#### B3. 添加力量增益上限

**文件**：[`QiPlayer.cs`](Common/Players/QiPlayer.cs)

在 `UseItem` 中（或基类中）添加：
```csharp
// 白豕蛊上限：一猪之力
if (item is WhitePigGu && whitePigPower >= MAX_WHITE_PIG_POWER) {
    return false; // 提示已达上限
}
```

### 方案C：长期架构（推荐完整方案）

#### C1. 引入"永久增益"管理系统

**新文件**：`Common/Players/GuPerkSystem.cs`

```csharp
public class GuPerkSystem : ModPlayer
{
    // 力量类 - 按蛊虫种类区分
    public int whitePigPower { get; private set; }  // 白豕蛊之力
    public int blackPigPower { get; private set; }  // 黑豕蛊之力
    public int jinLiPower { get; private set; }     // 斤力蛊系列
    public int junLiPower { get; private set; }     // 钧力蛊系列
    
    // 上限常量（基于原文）
    public const int MAX_WHITE_PIG_POWER = 1;   // 一猪之力
    public const int MAX_BLACK_PIG_POWER = 1;   // 一猪之力
    // 斤力蛊/钧力蛊理论上限由蛊师修为决定
    
    // 寿命类
    public int extraAges { get; private set; }
    
    // 速度类
    public float extraSpeed { get; private set; }
    public float extraAccel { get; private set; }
    
    // 召唤类
    public bool hasOneMinion { get; private set; }
    
    // 酒虫系列
    public WineBugLevel wineBugLevel { get; private set; }
    
    // 安全添加方法（带上限检查）
    public bool TryAddWhitePigPower(int amount) { ... }
    public bool TryAddJinLiPower(int amount) { ... }
}
```

**收益**：
- 永久增益逻辑从 `QiPlayer` 中分离，职责单一
- 每个增益类型有明确的上限控制
- 提供 `TryAdd` 模式，使用失败时返回 false，便于物品给出提示

#### C2. 统一消耗品蛊虫分类

```
消耗品蛊虫分类：
├── 永久增益类（一次性使用，存档保存）
│   ├── 力量系：白豕蛊、黑豕蛊、斤力蛊、钧力蛊
│   ├── 寿命系：十命蛊、百命蛊、千命蛊
│   ├── 速度系：龙珠蟋蟀
│   └── 召唤系：一胎蛊
├── 临时增益类（使用后获得Buff，持续一段时间）
│   ├── 花猪蛊（临时加力）
│   ├── 大力天牛（临时牛力）
│   └── 黄罗天牛（提升耐力）
├── 功能类（即时效果）
│   ├── 活叶（回血）
│   ├── 元石（回真元/破境）
│   └── 升转丹（突破境界）
└── 开启类（一次性，不可逆）
    └── 资质甲/乙/丙/丁（开启空窍）
```

#### C3. 废弃 `IGu` 接口

移除空接口 `IGu`，所有蛊虫物品改为继承具体基类：
- `GuConsumableItem`（消耗品基类）
- `GuWeaponItem`（武器基类，已有）
- `GuAccessoryItem`（饰品基类）

---

## 四、实施路线图

| 优先级 | 任务 | 影响范围 | 预估工时 |
|--------|------|---------|---------|
| 🔴 P0 | 修复 `qiCost` 被覆盖的 Bug | 2个文件 | 5分钟 |
| 🔴 P0 | 延长临时Buff持续时间 | 3个文件 | 5分钟 |
| 🟡 P1 | 创建 `GuConsumableItem` 基类 | 新建1个 + 修改15个文件 | 1小时 |
| 🟡 P1 | 废弃 `IGu` 接口 | 1个文件删除 + 15个文件改继承 | 30分钟 |
| 🟡 P1 | 实现酒虫效果 | 2个文件 | 30分钟 |
| 🟢 P2 | 统一力量增益字段命名 | 3个文件 | 20分钟 |
| 🟢 P2 | 添加力量增益上限 | 3个文件 | 20分钟 |
| 🔵 P3 | 创建 `GuPerkSystem` 管理系统 | 新建1个 + 重构 `QiPlayer` | 2小时 |

---

## 五、与原文对照表

| 蛊虫 | 原文设定 | 当前实现 | 是否符合 | 改进方向 |
|------|---------|---------|---------|---------|
| 白豕蛊 | 永久加力，上限一猪之力 | 永久加力，无上限 | ❌ | 添加上限 |
| 黑豕蛊 | 永久加力，可叠加白豕蛊 | 字段存在但无物品使用 | ❌ | 创建物品 |
| 斤力蛊 | 增长力气 | 永久加力 | ✅ | 命名优化 |
| 钧力蛊 | 四转力道蛊，增加力气 | 永久加力 | ✅ | 命名优化 |
| 花猪蛊 | 临时加力，暴涨一猪之力 | 临时Buff 5秒 | ⚠️ | 延长持续时间 |
| 大力天牛 | 临时牛力 | 临时Buff 5秒 | ⚠️ | 延长持续时间 |
| 黄罗天牛 | 提升持续力（耐力） | 临时Buff 30秒，+manaRegen | ⚠️ | 改为耐力相关效果 |
| 酒虫 | 精炼真元 | 字段存在无效果 | ❌ | 实现真元精炼 |
| 四味酒虫 | 二转增幅蛊虫 | 字段存在无效果 | ❌ | 实现 |
| 七香酒虫 | 精炼真元 | 字段存在无效果 | ❌ | 实现 |
| 九眼酒虫 | 四转酒虫，提纯黄金真元 | 字段存在无效果 | ❌ | 实现 |
| 十命蛊 | 增加寿命 | 增加 extraAges | ✅ | 无 |
| 百命蛊 | 增加寿命 | 增加 extraAges | ✅ | 无 |
| 千命蛊 | 增加寿命 | 增加 extraAges | ✅ | 无 |
| 一胎蛊 | 增加召唤栏 | 增加 hasOneMinion | ✅ | 无 |
| 龙珠蟋蟀 | 增加速度 | 增加 extraV/extraAcc | ✅ | 无 |
| 资质甲/乙/丙/丁 | 开启空窍 | 开启空窍 | ✅ | 无 |

---

## 六、总结

当前消耗类蛊虫的永久增益逻辑存在 **7个核心问题**，其中最严重的是：

1. **力量增益字段语义混乱**——三个字段做同样的事
2. **酒虫系列有字段无实现**——占用了存档空间但无实际效果
3. **`IGu` 空接口**——没有发挥接口的契约作用
4. **临时Buff持续时间过短**——5秒的Buff在战斗中形同虚设

**推荐立即执行 P0 修复**（修复 qiCost Bug + 延长Buff时间），然后按 P1→P2→P3 的顺序逐步重构。

---

## 七、重构完成总结（2026-04-26）

### 7.1 新架构概览

```
VerminLordMod/
├── Content/Items/Consumables/
│   ├── GuConsumableItem.cs          ← 蛊虫消耗品基类（抽象）
│   ├── WhitePigGu.cs                ← 白豕蛊（永久+1%，上限一猪之力）
│   ├── BlackPigGu.cs                ← 黑豕蛊（永久+1%，上限一猪之力）
│   ├── JinLiGu.cs                   ← 斤力蛊（永久+1%）
│   ├── ShiJinLiGu.cs                ← 十斤之力蛊（永久+10%）
│   ├── JunLiGu.cs                   ← 钧力蛊（永久+30%）
│   ├── ShiJunLiGu.cs                ← 十钧之力蛊（永久+300%）
│   ├── TenLifeGu.cs                 ← 十命蛊（永久+10寿命）
│   ├── HundredLifeGu.cs             ← 百命蛊（永久+100寿命）
│   ├── ThousandLifeGu.cs            ← 千命蛊（永久+1000寿命）
│   ├── DragonBallCricket.cs         ← 龙珠蟋蟀（永久移速+20%）
│   ├── OneMinion.cs                 ← 一胎蛊（永久+1召唤栏）
│   ├── FlowerPig.cs                 ← 花猪蛊（临时Buff 60秒）
│   ├── StrengthLongicorn.cs         ← 蛮力天牛蛊（临时Buff 60秒）
│   ├── HuangLuoLongicorn.cs         ← 皇酪天牛蛊（临时Buff 120秒）
│   ├── LivingLeaf.cs                ← 活叶（即时回血200，5分钟CD）
│   ├── WineBug.cs                   ← 酒虫（一转，真元恢复+1）
│   ├── FourFlavorWineBug.cs         ← 四味酒虫（二转，真元恢复+2）
│   ├── SevenWineBug.cs              ← 七香酒虫（三转，真元恢复+4）
│   └── NineWineBug.cs               ← 九眼酒虫（四转，真元恢复+8）
├── Common/Players/
│   ├── GuPerkSystem.cs              ← 永久增益管理系统（NEW）
│   └── QiPlayer.cs                  ← 真元系统（委托增益给 GuPerkSystem）
└── Content/Buffs/AddToSelf/Pobuff/
    ├── FlowerPigbuff.cs             ← 花猪Buff（近战伤害+30%）
    ├── StrengthLongicornbuff.cs     ← 蛮力天牛Buff（近战伤害+50%）
    └── HuangLuoLongicornbuff.cs     ← 皇酪天牛Buff（真元恢复，效果在QiPlayer中实现）
```

### 7.2 核心类设计

#### `GuConsumableItem`（蛊虫消耗品基类）

| 成员 | 类型 | 说明 |
|------|------|------|
| `QiCost` | `abstract int` | 真元消耗量 |
| `GuLevel` | `abstract int` | 蛊虫等级（一转=1） |
| `IsStackable` | `virtual bool` | 是否可堆叠（默认true） |
| `IsConsumed` | `virtual bool` | 使用后是否消耗（默认true） |
| `CanApplyEffect()` | `virtual bool` | 额外使用条件检查 |
| `ApplyEffect()` | `abstract void` | 具体效果实现 |

**统一流程**：
```
CanUseItem → qiCurrent >= QiCost && CanApplyEffect()
UseItem → 强行调动惩罚 → 扣除真元 → ApplyEffect()
```

#### `GuPerkSystem`（永久增益管理系统）

| 字段 | 类型 | 上限 | 说明 |
|------|------|------|------|
| `whitePigPower` | `int` | `MAX_WHITE_PIG_POWER = 1` | 白豕蛊之力 |
| `blackPigPower` | `int` | `MAX_BLACK_PIG_POWER = 1` | 黑豕蛊之力 |
| `jinLiPower` | `int` | 无（受修为限制） | 斤力蛊系列 |
| `junLiPower` | `int` | 无（受修为限制） | 钧力蛊系列 |
| `extraAges` | `int` | 无 | 额外寿命 |
| `extraSpeed` | `float` | 无 | 额外移速 |
| `extraAccel` | `float` | 无 | 额外加速度 |
| `hasOneMinion` | `bool` | 一次 | 一胎蛊 |
| `wineBugLevel` | `WineBugLevel` | `NineEye` | 酒虫等级 |

**安全添加方法**：
- `TryAddWhitePigPower(amount)` — 带上限检查
- `TryAddBlackPigPower(amount)` — 带上限检查
- `AddJinLiPower(amount)` — 无上限
- `AddJunLiPower(amount)` — 无上限
- `AddExtraAges(amount)` — 无上限
- `AddExtraSpeed(speed, accel)` — 无上限
- `SetOneMinion()` — 一次性
- `UpgradeWineBug(targetLevel)` — 只能升不能降

**效果计算方法**：
- `GetPowerDamageBonus(qiLevel, limited)` — 力量类伤害加成
- `GetAgeHealthBonus(qiLevel, limited)` — 寿命类生命加成
- `GetWineBugRegenBonus()` — 酒虫真元恢复加成（1/2/4/8）

### 7.3 解决的问题对照

| 问题 | 状态 | 解决方案 |
|------|------|---------|
| P0: qiCost被覆盖Bug | ✅ 已修复 | 删除旧模式，GuConsumableItem使用abstract QiCost |
| P0: 临时Buff太短 | ✅ 已修复 | 花猪/蛮力天牛 5s→60s，皇酪天牛 30s→120s |
| P1: 力量增益字段混乱 | ✅ 已修复 | 分离为whitePigPower/blackPigPower/jinLiPower/junLiPower |
| P1: 酒虫无实现 | ✅ 已修复 | WineBugLevel枚举 + GetWineBugRegenBonus() + 4个消耗品 |
| P1: IGu空接口 | ✅ 已废弃 | 蛊虫消耗品改为继承GuConsumableItem |
| P2: 无上限控制 | ✅ 已修复 | 白豕/黑豕蛊上限一猪之力，TryAdd模式 |
| P3: 无统一基类 | ✅ 已修复 | GuConsumableItem抽象基类 |

### 7.4 原文对照（更新后）

| 蛊虫 | 原文设定 | 实现效果 | 符合度 |
|------|---------|---------|:------:|
| 白豕蛊 | 永久加力，上限一猪之力 | 永久+1%，上限1次 | ✅ |
| 黑豕蛊 | 永久加力，可叠加白豕蛊 | 永久+1%，上限1次 | ✅ |
| 斤力蛊 | 增长力气 | 永久+1% | ✅ |
| 十斤之力蛊 | 增长力气 | 永久+10% | ✅ |
| 钧力蛊 | 四转力道蛊，增加力气 | 永久+30% | ✅ |
| 十钧之力蛊 | 四转力道蛊，增加力气 | 永久+300% | ✅ |
| 花猪蛊 | 临时加力，暴涨一猪之力 | 临时Buff 60秒，+30%近战伤害 | ✅ |
| 蛮力天牛蛊 | 临时牛力 | 临时Buff 60秒，+50%近战伤害 | ✅ |
| 皇酪天牛蛊 | 提升持续力（耐力） | 临时Buff 120秒，真元恢复 | ✅ |
| 酒虫 | 精炼真元（一转） | 真元恢复+1 | ✅ |
| 四味酒虫 | 精炼真元（二转） | 真元恢复+2 | ✅ |
| 七香酒虫 | 精炼真元（三转） | 真元恢复+4 | ✅ |
| 九眼酒虫 | 提纯黄金真元（四转） | 真元恢复+8 | ✅ |
| 十命蛊 | 增加寿命 | 额外+10寿命 | ✅ |
| 百命蛊 | 增加寿命 | 额外+100寿命 | ✅ |
| 千命蛊 | 增加寿命 | 额外+1000寿命 | ✅ |
| 一胎蛊 | 增加召唤栏 | 额外+1召唤栏 | ✅ |
| 龙珠蟋蟀 | 增加速度 | 移速+20%，加速度+20% | ✅ |
| 活叶 | 治疗 | 回血200，5分钟冷却 | ✅ |

### 7.5 待办事项

- [ ] 移除 `IGu` 接口在非蛊虫消耗品上的实现（Hopeness, KsitigarbhaFlowerGu, FifthToSixth, Shari系列）
- [ ] 考虑将 `IGu` 接口改为有实际成员的接口，或完全移除
- [ ] 为酒虫系列添加更丰富的原文效果（如真元品质提升的视觉表现）
- [ ] 考虑添加"炼化"系统到 GuConsumableItem 基类（目前酒虫系列在 Weapons 目录下有炼化版本）
