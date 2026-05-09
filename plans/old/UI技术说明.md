# VerminLordMod UI 系统技术说明

## 一、概述

VerminLordMod 的 UI 系统采用**两套并行框架**实现：

| 框架 | 核心类 | 适用场景 |
|------|--------|----------|
| **UIManager 框架**（轻量自研） | `UIManager` + `BaseUI` + `BasePanel` + `BaseButton` | 跟随实体的浮动 UI、轻量级覆盖层 |
| **UIState 框架**（tModLoader 标准） | `UIState` + `UserInterface` + `ModSystem` | 固定面板、复杂交互 UI、标准窗口 |

两套框架共享同一套**风格定义层**（`UIStyles` + `UIHelper`），保证视觉一致性。

---

## 二、架构分层

```
┌─────────────────────────────────────────────────────────┐
│                    业务 UI 层                             │
│  CorpseLootUI  DaosUI  KongQiaoUI  GuCraftUI  QiBar    │
│  ReputationUI  WolfWaveBar  DanmakuSelectionUI          │
├─────────────────────────────────────────────────────────┤
│                   框架层                                  │
│  ┌──────────────┐  ┌──────────────────────────────────┐ │
│  │ UIManager    │  │ UIState + UserInterface + ModSystem│ │
│  │ BaseUI       │  │ (tModLoader 标准)                  │ │
│  │ BasePanel    │  │                                   │ │
│  │ BaseButton   │  │                                   │ │
│  └──────────────┘  └──────────────────────────────────┘ │
├─────────────────────────────────────────────────────────┤
│                   风格定义层                               │
│  UIStyles (颜色/字体/布局常量)                            │
│  UIHelper (工厂方法/绘制工具)                              │
│  EasyDraw (SpriteBatch 状态保持)                          │
└─────────────────────────────────────────────────────────┘
```

---

## 三、风格定义层（共享）

### 3.1 UIStyles — 统一风格定义

**文件：** [`Common/UI/UIUtils/UIStyles.cs`](Common/UI/UIUtils/UIStyles.cs)

静态类，定义所有 UI 共享的视觉常量：

| 类别 | 内容 |
|------|------|
| **基础色板** | `PanelBg`(28,30,38), `PanelBgLight`(35,38,48), `TitleBarBg`(38,42,55) 等 |
| **边框** | `Border`(55,60,75), `BorderAccent`(100,150,210), `BorderHighlight`(200,175,90) 等 |
| **文字** | `TitleText`(235,225,200), `TextMain`(210,212,220), `TextSuccess`(120,200,140) 等 |
| **按钮** | `BtnDefault`(50,54,68), `BtnPrimary`(55,100,70), `BtnDanger`(90,50,50) 等 |
| **滚动条** | `ScrollbarBg`, `ScrollbarThumb`, `ScrollbarThumbHover` |
| **特殊 UI** | `ToggleBg`(空窍按钮), `QiBarBg`(真元条), `WolfBarBg`(狼潮条), `LootPanelBg`(战利品) |
| **布局常量** | `PanelPadding=8`, `TitleBarHeight=36`, `ButtonHeight=28`, `SlotSize=48` 等 |
| **字体缩放** | `TitleScale=1.1`, `BodyScale=0.8`, `SmallScale=0.7` 等 |
| **辅助方法** | `GetGuLevelColor()`, `GetRarityColor()`, `HoverOver()`, `PressDown()` |

**设计原则：** 低饱和度、柔和阴影、清晰排版、干净简洁。

### 3.2 UIHelper — UI 辅助工具

**文件：** [`Common/UI/UIUtils/UIHelper.cs`](Common/UI/UIUtils/UIHelper.cs)

静态工厂类，提供统一风格的 UI 元素创建方法：

| 方法 | 说明 |
|------|------|
| `CreatePanel()` | 创建居中主面板（`UIPanel`） |
| `UpdatePanelCenter()` | 每帧更新面板居中位置 |
| `CreateSubPanel()` | 创建带内边距的子面板 |
| `CreateTitle()` / `CreateText()` | 创建标题/正文文本 |
| `CreateButton()` | 创建扁平风格按钮 |
| `CreateCloseButton()` | 创建右上角关闭按钮（✕） |
| `CreateTitleBar()` | 创建顶部标题栏横条 |
| `CreateScrollbar()` | 创建滚动条 |
| `CreateUIList()` | 创建列表容器 |
| `DrawRoundedRect()` | 绘制圆角矩形边框（9-patch 模拟） |
| `DrawBorder()` | 绘制矩形边框 |
| `DrawItemIcon()` | 绘制物品图标（含堆叠数量） |
| `CheckEscapeReleased()` | ESC 键边沿触发检测 |

### 3.3 EasyDraw — SpriteBatch 状态保持

**文件：** [`Common/UI/UIUtils/EasyDraw.cs`](Common/UI/UIUtils/EasyDraw.cs)

通过反射保持 `SpriteBatch` 的渲染状态（采样器、深度模板、光栅化器、特效、变换矩阵），仅切换 `BlendState` 或 `SpriteSortMode`。用于在 `BasePanel`/`BaseButton` 的 `Draw()` 中切换 `NonPremultiplied` 混合模式。

---

## 四、UIManager 框架（轻量自研）

### 4.1 架构

```
UIManager (ModSystem, 全局单例)
  ├── BaseUI (容器, 按名称标识)
  │     ├── BasePanel (面板, 按名称标识)
  │     │     ├── BaseButton (按钮, 按名称标识)
  │     │     └── BaseButton ...
  │     └── BasePanel ...
  └── BaseUI ...
```

### 4.2 UIManager — UI 管理器

**文件：** [`Common/UI/UIUtils/UIManager.cs`](Common/UI/UIUtils/UIManager.cs)

- 继承 `ModSystem`，`[Autoload(Side = ModSide.Client)]`
- 全局静态持有所有 `BaseUI` 实例列表
- 提供鼠标点击检测（上升沿）：`LeftClicked` / `RightClicked`
- 生命周期：
  - `Load()` → 初始化列表
  - `UpdateUI()` → 遍历所有 UI/面板/按钮，执行更新和点击检测
  - `PostDrawInterface()` → 遍历所有 UI，调用 `DrawSubPanels()`
  - `Unload()` → 清空列表
- 静态方法：`NewUI()`, `FindUI()`

**点击检测机制：** 上升沿检测（`DetectClick`）。当鼠标从按下状态释放时记录 `lastUnPressed=true`，下一帧检测到 `lastUnPressed` 时触发点击。支持左键和右键。

### 4.3 BaseUI — 基础 UI 容器

**文件：** [`Common/UI/UIUtils/BaseUI.cs`](Common/UI/UIUtils/BaseUI.cs)

- 持有 `List<BasePanel>` 子面板列表
- 方法：
  - `DrawSubPanels()` → 遍历绘制所有子面板
  - `NewPanel()` → 添加新面板（同名检测）
  - `FindSubPanels()` → 按名称查找面板

### 4.4 BasePanel — 基础面板

**文件：** [`Common/UI/UIUtils/BasePanel.cs`](Common/UI/UIUtils/BasePanel.cs)

- 属性：`PanelName`, `PanelCenter`, `PanelSize`, `PanelTex`, `Panelcolor`, `PanelRot`
- 状态：`Active`, `Visible`, `CanDraw`, `FullScreen`
- 生命周期委托：
  - `PreUpdate` → `Func<bool>`，返回 false 跳过更新
  - `Update` → `Action`，每帧更新逻辑
  - `PostUpdate` → `Action`，更新后处理
  - `DrawAct` → `Action`，自定义绘制（替代默认纹理绘制）
  - `DrawOther` → `Action<SpriteBatch>`，额外绘制
- 子按钮管理：`SubButtonsList`, `NewButton()`, `FindSubButton()`
- 默认绘制：使用 `PanelTex` 纹理，支持旋转/缩放/颜色/全屏

### 4.5 BaseButton — 基础按钮

**文件：** [`Common/UI/UIUtils/BaseButton.cs`](Common/UI/UIUtils/BaseButton.cs)

- 属性：`ButtonName`, `ButtonCenter`, `ButtonSize`, `ButtonTex`, `ButtonTex_Hover`, `ButtonColor`, `ButtonRot`
- 状态：`ButtonActive`, `ButtonVisible`, `ButtonCanDraw`
- 交互：
  - `Hovered()` → 矩形碰撞检测（基于纹理尺寸）
  - `Clicked()` → 悬停 + 左键点击（同时阻止玩家使用物品）
  - `ButtonClickedEven` → 点击事件委托
  - `ButtonPlaySound` → 点击音效路径
- 生命周期委托：同 `BasePanel`（`PreUpdate`, `Update`, `PostUpdate`, `DrawAct`, `DrawOther`）
- 默认绘制：悬停时切换 `ButtonTex_Hover`，支持 `NonPremultiplied` 混合

### 4.6 使用示例：CorpseLootUI

**文件：** [`Common/UI/DeepLootUI.cs`](Common/UI/DeepLootUI.cs)

```csharp
// 1. 初始化
_baseUI = UIManager.NewUI("CorpseLootUI");
_mainPanel = _baseUI.NewPanel(texture: null, ...);
_mainPanel.DrawAct = DrawPanel;   // 自定义绘制
_mainPanel.Update = UpdatePanel;  // 每帧更新

// 2. 更新（UpdatePanel）
//    - 检查尸体状态、玩家距离
//    - 缓存物品列表
//    - 检测点击（UIManager.LeftClicked）

// 3. 绘制（DrawPanel）
//    - 纯像素绘制（TextureAssets.MagicPixel）
//    - 计算面板位置（尸体上方）
//    - 绘制背景、边框、标题、"全部拾取"按钮、物品槽位
//    - 悬停显示物品名称
```

**特点：** 无纹理依赖、跟随实体位置、纯像素绘制、轻量级。

---

## 五、UIState 框架（tModLoader 标准）

### 5.1 标准模式

每个 UI 由三部分组成：

```
UIState (UI 定义)
  └── UIPanel (主面板)
        ├── UIText (标题)
        ├── UITextPanel (按钮)
        ├── UIList (列表)
        ├── UIScrollbar (滚动条)
        └── ...

ModSystem (生命周期管理)
  ├── Load() → 创建 UserInterface + UIState 实例
  ├── UpdateUI() → 调用 UserInterface.Update()
  ├── ModifyInterfaceLayers() → 注册绘制层
  └── ToggleUI() → 切换显示/隐藏
```

### 5.2 注册到界面层

通过 `ModifyInterfaceLayers()` 插入 `LegacyGameInterfaceLayer`：

```csharp
public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
{
    int index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
    if (index != -1)
    {
        layers.Insert(index, new LegacyGameInterfaceLayer(
            "VerminLordMod: XXX UI",
            () => {
                if (_ui?.CurrentState != null)
                    _ui.Draw(Main.spriteBatch, new GameTime());
                return true;
            },
            InterfaceScaleType.UI
        ));
    }
}
```

### 5.3 各 UI 实现对比

| UI | 文件 | 面板尺寸 | 插入位置 | 特点 |
|----|------|----------|----------|------|
| **空窍面板** | [`KongQiaoUI/KongQiaoUI.cs`](Common/UI/KongQiaoUI/KongQiaoUI.cs) | 500×460 | Mouse Text 前 | 可滚动列表、合炼按钮、ESC 关闭 |
| **空窍入口按钮** | [`KongQiaoUI/KongQiaoToggle.cs`](Common/UI/KongQiaoUI/KongQiaoToggle.cs) | 44×44 | Resource Bars | 可拖动、位置持久化（ModPlayer 存档） |
| **合炼面板** | [`KongQiaoUI/GuCraftUI.cs`](Common/UI/KongQiaoUI/GuCraftUI.cs) | 740×540 | Mouse Text 前 | 搜索框、分类筛选、材料槽位、批量合成 |
| **道痕面板** | [`DaosUI/DaosUI.cs`](Common/UI/DaosUI/DaosUI.cs) | 380×280 | Mouse Text 前 | 简单列表、占位内容 |
| **真元条** | [`QiUI/QiBar.cs`](Common/UI/QiUI/QiBar.cs) | 200×72 | Resource Bars | 境界渐变色、实时更新 |
| **声望面板** | [`ReputationUI/ReputationUI.cs`](Common/UI/ReputationUI/ReputationUI.cs) | 260×36~340 | Mouse Text 前 | 可折叠、右上角固定 |
| **狼潮进度条** | [`WolfWaveUI/WolfWaveBar.cs`](Common/UI/WolfWaveUI/WolfWaveBar.cs) | 200×48 | Resource Bars | 仅在狼潮事件时显示 |
| **弹幕选择 UI** | [`DanmakuUI/DanmakuSelectionUI.cs`](Common/UI/DanmakuUI/DanmakuSelectionUI.cs) | 680×520 | Mouse Text 前 | 动态分类、全模组弹幕扫描 |

### 5.4 自定义 UI 组件

**文件：** [`KongQiaoUI/UIItemSlot.cs`](Common/UI/KongQiaoUI/UIItemSlot.cs)

- `UIItemSlot` — 可交互物品槽位，支持高亮、堆叠显示、点击回调
- `UICategoryButton` — 分类标签按钮，选中态带底部高亮线

**文件：** [`KongQiaoUI/KongQiaoUI.cs`](Common/UI/KongQiaoUI/KongQiaoUI.cs)

- `KongQiaoSlotUI` — 空窍格子 UI 元素（继承 `UIPanel`），显示蛊虫图标/名称/状态/操作按钮

**文件：** [`DanmakuUI/DanmakuSelectionUI.cs`](Common/UI/DanmakuUI/DanmakuSelectionUI.cs)

- `DanmakuListItem` — 弹幕列表项（继承 `UIPanel`），悬停变色

---

## 六、两套框架对比

| 维度 | UIManager 框架 | UIState 框架 |
|------|---------------|-------------|
| **依赖** | 自研，无外部依赖 | tModLoader 标准 `UIState`/`UserInterface` |
| **复杂度** | 低，纯像素绘制 | 高，完整的 UI 元素树 |
| **灵活性** | 高，任意位置/跟随实体 | 受限于 UI 元素布局系统 |
| **交互** | 手动点击检测（矩形碰撞） | 事件驱动（`OnLeftClick` 等） |
| **滚动/列表** | 需自行实现 | 内置 `UIList` + `UIScrollbar` |
| **输入框** | 需自行实现 | 内置 `UITextBox` |
| **性能** | 轻量，无额外开销 | 标准开销 |
| **适用场景** | 浮动 UI、HUD 元素 | 固定面板、复杂交互窗口 |

---

## 七、UI 与业务系统的交互模式

### 7.1 通过 ModSystem 桥接

```
业务 System (ModSystem)
  ├── 持有 UI 实例引用
  ├── 调用 UI 的 Open/Close/Toggle 方法
  └── UI 回调业务 System 的方法
```

**示例：** [`LootSystem`](Common/Systems/LootSystem.cs) ↔ [`CorpseLootUI`](Common/UI/DeepLootUI.cs)

```
LootSystem.UpdateCorpseDetection()
  → 检测玩家靠近尸体
  → CorpseLootUI.Instance.Open(corpse)
  → CorpseLootUI 绘制面板
  → 玩家点击物品槽位
  → CorpseLootUI.UpdatePanel() 检测点击
  → LootSystem.TakeItemFromCorpse()
```

### 7.2 通过 ModPlayer 持久化 UI 状态

**示例：** 空窍按钮位置保存

```
KongQiaoToggle (拖动按钮)
  → KongQiaoToggleSavePlayer (ModPlayer)
    → SaveData() / LoadData()
    → OnEnterWorld() → RestoreTogglePosition()
```

### 7.3 通过物品触发 UI

```csharp
// KongQiaoStone.cs
ModContent.GetInstance<KongQiaoUISystem>().ToggleUI();
```

---

## 八、文件清单

### 风格定义层
| 文件 | 说明 |
|------|------|
| [`Common/UI/UIUtils/UIStyles.cs`](Common/UI/UIUtils/UIStyles.cs) | 统一风格定义（颜色/字体/布局常量） |
| [`Common/UI/UIUtils/UIHelper.cs`](Common/UI/UIUtils/UIHelper.cs) | UI 辅助工具（工厂方法/绘制工具） |
| [`Common/UI/UIUtils/EasyDraw.cs`](Common/UI/UIUtils/EasyDraw.cs) | SpriteBatch 状态保持 |

### UIManager 框架
| 文件 | 说明 |
|------|------|
| [`Common/UI/UIUtils/UIManager.cs`](Common/UI/UIUtils/UIManager.cs) | UI 管理器（ModSystem） |
| [`Common/UI/UIUtils/BaseUI.cs`](Common/UI/UIUtils/BaseUI.cs) | 基础 UI 容器 |
| [`Common/UI/UIUtils/BasePanel.cs`](Common/UI/UIUtils/BasePanel.cs) | 基础面板 |
| [`Common/UI/UIUtils/BaseButton.cs`](Common/UI/UIUtils/BaseButton.cs) | 基础按钮 |

### UIState 框架 — 业务 UI
| 文件 | 说明 |
|------|------|
| [`Common/UI/KongQiaoUI/KongQiaoUI.cs`](Common/UI/KongQiaoUI/KongQiaoUI.cs) | 空窍面板 + 格子 UI 元素 |
| [`Common/UI/KongQiaoUI/KongQiaoUISystem.cs`](Common/UI/KongQiaoUI/KongQiaoUISystem.cs) | 空窍 UI 系统（管理面板/按钮/合炼） |
| [`Common/UI/KongQiaoUI/KongQiaoToggle.cs`](Common/UI/KongQiaoUI/KongQiaoToggle.cs) | 空窍入口按钮（可拖动） |
| [`Common/UI/KongQiaoUI/GuCraftUI.cs`](Common/UI/KongQiaoUI/GuCraftUI.cs) | 合炼面板 |
| [`Common/UI/KongQiaoUI/UIItemSlot.cs`](Common/UI/KongQiaoUI/UIItemSlot.cs) | 物品槽位 + 分类按钮组件 |
| [`Common/UI/DaosUI/DaosUI.cs`](Common/UI/DaosUI/DaosUI.cs) | 道痕面板 |
| [`Common/UI/QiUI/QiBar.cs`](Common/UI/QiUI/QiBar.cs) | 真元/仙元条 |
| [`Common/UI/ReputationUI/ReputationUI.cs`](Common/UI/ReputationUI/ReputationUI.cs) | 声望面板 |
| [`Common/UI/ReputationUI/ReputationUISystem.cs`](Common/UI/ReputationUI/ReputationUISystem.cs) | 声望 UI 系统 |
| [`Common/UI/WolfWaveUI/WolfWaveBar.cs`](Common/UI/WolfWaveUI/WolfWaveBar.cs) | 狼潮进度条 |
| [`Common/UI/DanmakuUI/DanmakuSelectionUI.cs`](Common/UI/DanmakuUI/DanmakuSelectionUI.cs) | 弹幕选择 UI |

### UIManager 框架 — 业务 UI
| 文件 | 说明 |
|------|------|
| [`Common/UI/DeepLootUI.cs`](Common/UI/DeepLootUI.cs) | 尸体战利品 UI（轻量版） |
| [`Common/UI/RefineRecipeCallbacks.cs`](Common/UI/RefineRecipeCallbacks.cs) | 精炼配方回调 |

### 与 UI 交互的 System
| 文件 | 说明 |
|------|------|
| [`Common/Systems/LootSystem.cs`](Common/Systems/LootSystem.cs) | 战利品系统（控制尸体 UI 生命周期） |
| [`Common/Systems/TradeSystem.cs`](Common/Systems/TradeSystem.cs) | 交易系统（含 UI 显示相关逻辑） |
