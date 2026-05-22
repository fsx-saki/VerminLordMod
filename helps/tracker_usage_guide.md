# 实现状态追踪系统使用指南

## 概述

本系统用于追踪 VerminLordMod 中所有 Content 对象的实现状态，帮助开发者清晰地了解：
- 哪些蛊虫、物品、NPC、弹幕等是**占位实现**（仅有基础框架）
- 哪些是**完整实现**（功能完整）
- 哪些是**部分实现**（有基本功能但需要优化）
- 整体完成进度

系统由三部分组成：
1. **`[ImplStatus]` 属性** — 标记每个类的实现状态
2. **`ImplementationTracker`** — 运行时扫描器，自动收集状态信息
3. **`scan_implementation_status.py`** — 离线扫描脚本，生成报告和待办清单

---

## 1. 使用 `[ImplStatus]` 属性标记类

### 基本用法

在每个 Content 类上添加 `[ImplStatus]` 属性：

```csharp
using VerminLordMod.Common.ImplementationTracker;

namespace VerminLordMod.Content.Items.Weapons.One
{
    /// <summary>
    /// 一转水蛊虫 — WaterArrowGu
    /// </summary>
    [ImplStatus(ImplStatus.Implemented, note = "完整实现，有弹幕和特效")]
    class WaterArrowGu : WaterWeapon
    {
        // ...
    }
}
```

### 状态枚举

| 状态 | 含义 | 使用场景 |
|------|------|----------|
| `ImplStatus.Placeholder` | 占位实现 | 仅有基础框架，逻辑不完整或未测试 |
| `ImplStatus.Partial` | 部分实现 | 有基本功能但缺少细节或需要优化 |
| `ImplStatus.Implemented` | 完整实现 | 功能完整，经过测试 |
| `ImplStatus.Deprecated` | 已废弃 | 不再使用 |
| `ImplStatus.Planned` | 待创建 | 仅有计划，尚未创建文件 |

### 属性参数

```csharp
[ImplStatus(
    status: ImplStatus.Placeholder,   // 实现状态（必填）
    note: "仅有贴图，逻辑未实现",      // 备注说明
    plannedTurn: "一转",               // 计划转数（仅蛊虫）
    daoType: "水"                      // Dao 类型
)]
```

### 快速添加

运行扫描脚本时使用 `--write-attributes` 参数，会自动为所有没有 `[ImplStatus]` 标记的类添加推断的属性：

```bash
python3 tools/scan_implementation_status.py --write-attributes
```

---

## 2. 使用扫描脚本

### 生成报告

```bash
# 生成实现状态报告和待办清单
python3 tools/scan_implementation_status.py

# 输出文件：
#   helps/implementation_status.md  — 完整状态报告
#   helps/implementation_todo.md    — 待办清单
```

### 自动写入属性

```bash
# 扫描并自动为所有 .cs 文件添加 [ImplStatus] 属性
python3 tools/scan_implementation_status.py --write-attributes
```

### 自定义输出目录

```bash
python3 tools/scan_implementation_status.py --output-dir docs
```

---

## 3. 运行时查询（游戏中）

`ImplementationTracker` 在 Mod 加载完成后自动扫描所有类型，提供以下查询接口：

### 在 Mod 代码中使用

```csharp
using VerminLordMod.Common.ImplementationTracker;

// 获取所有占位实现
var placeholders = ImplementationTracker.GetByStatus(ImplStatus.Placeholder);

// 获取指定分类的所有条目
var oneTurnWeapons = ImplementationTracker.GetByCategory("Items.Weapons.一转");

// 获取指定转数的所有条目
var sixTurnGus = ImplementationTracker.GetByTurn("六转");

// 获取指定 Dao 类型的所有条目
var waterGus = ImplementationTracker.GetByDaoType("水");

// 获取统计信息
int total = ImplementationTracker.TotalCount;
int done = ImplementationTracker.ImplementedCount;
int todo = ImplementationTracker.PlaceholderCount;
double progress = ImplementationTracker.ProgressPercent;
```

### 在 Mod 中生成报告

```csharp
// 生成完整报告（字符串）
string report = ImplementationTracker.GenerateReport();

// 生成待办清单
string todoList = ImplementationTracker.GenerateTodoList();
```

---

## 4. 工作流程

### 日常开发流程

1. **开始优化一个占位实现前**：
   - 查看 `helps/implementation_todo.md` 了解当前待办
   - 选择一个占位实现开始工作

2. **实现完成后**：
   - 更新类的 `[ImplStatus]` 属性：
     ```csharp
     // 从
     [ImplStatus(ImplStatus.Placeholder, note = "仅有SetDefaults")]
     // 改为
     [ImplStatus(ImplStatus.Implemented, note = "完整实现，有弹幕和特效")]
     ```

3. **定期更新报告**：
   ```bash
   python3 tools/scan_implementation_status.py
   ```

### 批量标记

如果有一批类需要统一标记状态，可以手动编辑 `[ImplStatus]` 属性，或者修改扫描脚本的启发式规则后重新运行。

---

## 5. 启发式规则说明

扫描脚本使用以下规则判断一个类是否为"占位实现"：

| 规则 | 权重 | 说明 |
|------|------|------|
| 代码行数 < 15 | +2 | 有效代码行数少 |
| 仅有 `SetDefaults()` | +3 | 没有其他 override 方法 |
| `UseItem` 逻辑简单 | +2 | 只有 ConsumeQi + QuickSpawnItem |
| 文件大小 < 800B | +1 | 文件非常小 |

**总分 >= 3 判定为占位实现**。

> 注意：这些是启发式规则，可能会有误判。如果某个类被错误标记，手动添加正确的 `[ImplStatus]` 属性即可覆盖。

---

## 6. 文件结构

```
Common/
  ImplementationTracker/
    ImplStatus.cs                  — [ImplStatus] 属性和 ImplStatus 枚举
    ImplementationTracker.cs       — 运行时追踪器（ModSystem）

tools/
  scan_implementation_status.py   — 离线扫描脚本

helps/
  implementation_status.md         — 实现状态报告（自动生成）
  implementation_todo.md           — 待办清单（自动生成）
  tracker_usage_guide.md           — 本使用指南
```

---

## 7. 最佳实践

1. **每个 Content 类都应该有 `[ImplStatus]` 属性** — 这是追踪系统的基础
2. **创建新类时立即添加 `[ImplStatus(ImplStatus.Placeholder)]`** — 避免遗漏
3. **完成实现后及时更新状态** — 保持报告的准确性
4. **在备注中记录实现细节** — 方便其他开发者了解
5. **定期运行扫描脚本** — 保持报告和待办清单的更新
