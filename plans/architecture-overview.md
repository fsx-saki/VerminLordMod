# VerminLordMod 模组架构分析报告

## 1. 项目概览

- **模组名称**: VerminLordMod（蛊师模组）
- **作者**: 風笙咲
- **版本**: 1.1.0.4
- **依赖**: SubworldLibrary@2.2.2.2
- **语言**: C# (tModLoader, .NET 8.0)
- **主题**: 基于《蛊真人》小说设定的修仙/蛊虫系统

## 2. 目录结构

```
VerminLordMod/
├── build.txt                    # 模组构建配置
├── Common/                      # 通用逻辑层
│   ├── Configs/                 # 模组配置
│   ├── GlobalItems/             # 全局物品修改
│   ├── GlobalNPCs/              # 全局NPC修改
│   ├── Players/                 # 玩家扩展类
│   ├── SubWorlds/               # 子世界（SubworldLibrary）
│   ├── Systems/                 # 系统类
│   └── UI/                      # 用户界面
├── Content/                     # 内容定义层
│   ├── Biomes/                  # 生物群落
│   ├── Buffs/                   # 增益/减益效果
│   ├── Currencies/              # 自定义货币
│   ├── DamageClasses/           # 自定义伤害类型
│   ├── Dusts/                   # 粒子效果
│   ├── Items/                   # 物品
│   │   ├── Accessories/         # 饰品蛊虫
│   │   ├── Consumables/         # 消耗品
│   │   ├── Placeable/           # 可放置物品
│   │   └── Weapons/             # 武器蛊虫
│   ├── NPCs/                    # NPC
│   │   ├── Boss/                # Boss
│   │   ├── Enemy/               # 敌怪
│   │   └── Town/                # 城镇NPC
│   ├── Prefixes/                # 词缀
│   ├── Projectiles/             # 投射物
│   └── Tiles/                   # 物块
├── Assets/                      # 资源文件
├── Localization/                # 本地化
└── Properties/                  # 项目属性
```

## 3. 核心系统架构

### 3.1 玩家系统 (Common/Players/)

#### [`QiPlayer`](Common/Players/QiPlayer.cs) - 核心修仙系统
- **真元系统**: 自定义资源（真元/仙元），包含恢复、消耗、上限管理
- **境界系统**: 转数（1-10转）+ 阶段（初期/中期/后期/巅峰）
- **资质系统**: 甲/乙/丙/丁等资质，影响修炼效率
- **空窍系统**: 空窍大小决定元海容量
- **数据持久化**: 通过 `SaveData`/`LoadData` 保存玩家进度
- **关键字段**: `qiLevel`, `levelStage`, `qiCurrent`, `qiMax`, `kongQiaoMax`, `PlayerZiZhi`

#### [`EffectsPlayer`](Common/Players/EffectsPlayer.cs) - 特效系统
- 处理玩家身上的Buff视觉效果
- 当前实现了旋风特效（`BreezeWheelbuff`）
- 通过 [`PlayerEffectDrawSystem`](Common/Systems/PlayerEffectDrawSystem.cs) 在 `PostDrawInterface` 中调用绘制

#### [`ProjectileTrailPlayer`](Common/Players/ProjectileTrailPlayer.cs) - 投射物拖尾

### 3.2 物品系统 (Content/Items/)

#### 蛊虫基类体系
- [`GuWeaponItem`](Content/Items/Weapons/GuWeaponItem.cs) - 武器蛊虫基类
  - 炼化系统：`controlRate`, `hasBeenControlled`, `uncontrolRate`
  - 真元消耗：`qiCost`
  - 转数等级：`_guLevel`
  - 虚影效果：`moddustType`
  
- [`GuAccessoryItem`](Content/Items/Accessories/GuAccessoryItem.cs) - 饰品蛊虫基类
  - 类似炼化系统
  - 装备限制：防御力与境界挂钩

#### 物品分类
- **饰品蛊虫** (Accessories): 熊力蛊、石皮蛊、铜皮蛊等防御/属性类
- **武器蛊虫** (Weapons): 月光蛊、溪流蛊、骨枪蛊等攻击类
- **消耗品** (Consumables): 舍利蛊（突破境界）、寿蛊（增加生命）、资质道具等
- **可放置物品** (Placeable): 青茅石系列家具、元石矿、音乐盒

### 3.3 词缀系统 (Content/Prefixes/)

- 武器词缀：外向、羞怯、温和、极端、活跃、内向、垂死、自闭
- 饰品词缀：甲壳、鞘翅、蜷缩、伸展、利爪、尖牙
- 通过 [`GlobalPrefix`](Common/GlobalItems/GlobalPrefix.cs) 全局分配

### 3.4 NPC系统 (Content/NPCs/)

#### Boss
- [`ElectricWolfKing`](Content/NPCs/Boss/ElectricWolfKing.cs) - 雷冠头狼
  - 二阶段战斗
  - 召唤小弟（狂电狼）
  - 掉落：元石、寿蛊、雷翼蛊等

#### 城镇NPC
- 学堂家老、药堂家老、御堂家老、白家长老
- 贾家商人（旅行商人）
- 完整的好感度/心情系统

### 3.5 系统类 (Common/Systems/)

- [`DownBossSystem`](Common/Systems/DownBossSystem.cs) - Boss击败记录
- [`WolfSystem`](Common/Systems/WolfSystem.cs) - 狼潮事件系统
- [`PlayerEffectDrawSystem`](Common/Systems/PlayerEffectDrawSystem.cs) - 特效绘制
- [`QingMaoBiomeTileCount`](Common/Systems/QingMaoBiomeTileCount.cs) - 青茅生物群落物块计数
- [`RecipeGroupSystem`](Common/Systems/RecipeGroupSystem.cs) - 配方组
- [`TravelMerchantSystem`](Common/Systems/TravelMerchantSystem.cs) - 旅行商人

### 3.6 UI系统 (Common/UI/)

- [`QiBar`](Common/UI/QiUI/QiBar.cs) - 真元/仙元条（境界颜色渐变）
- [`WolfWaveBar`](Common/UI/WolfWaveUI/WolfWaveBar.cs) - 狼潮进度条
- [`DaosUI`](Common/UI/DaosUI/DaosUI.cs) - 道/UI框架（待完善）
- [`RefineRecipeCallbacks`](Common/UI/RefineRecipeCallbacks.cs) - 炼蛊配方回调

### 3.7 生物群落 (Content/Biomes/)

- [`QingMaoSurfaceBiome`](Content/Biomes/QingMaoSurfaceBiome.cs) - 青茅山地表
- [`QingMaoUndergroundBiome`](Content/Biomes/QingMaoUndergroundBiome.cs) - 青茅山地下
- 自定义水样式、雨、瀑布、背景

### 3.8 伤害类型 (Content/DamageClasses/)

- [`InsectDamageClass`](Content/DamageClasses/InsectDamageClass.cs) - 蛊术伤害
  - 继承通用伤害全部加成
  - 继承魔法伤害部分加成（伤害100%、攻速40%、穿透250%）
  - 触发魔法伤害效果

### 3.9 工具类 (Content/)

- [`Fucs`](Content/Fucs.cs) - 通用工具函数
  - 拖尾绘制、发光效果、Additive混合模式

## 4. 数据流与关键交互

### 4.1 真元消耗流程
```
玩家使用武器蛊虫
  → GuWeaponItem.CanUseItem() 检查 qiCurrent >= qiCost
  → GuWeaponItem.UseItem() 扣除 qiCost
  → QiPlayer.UpdateResource() 每帧恢复真元
```

### 4.2 炼化流程
```
右键使用蛊虫（altFunctionUse == 2）
  → 检查 qiCurrent >= controlQiCost
  → 扣除真元，增加 controlRate
  → UpdateInventory() 中 controlRate 缓慢下降（uncontrolRate）
  → controlRate 达到100时 hasBeenControlled = true
```

### 4.3 境界突破流程
```
使用舍利蛊 或 积累 levelStageUpRate
  → QiPlayer.StageUp() 提升小阶段
  → QiPlayer.LevelUp() 提升大转数
  → SetQis() 重新计算真元上限和恢复速度
```

### 4.4 狼潮事件流程
```
每天清晨概率触发
  → WolfSystem.isWolfWave = true
  → WolfWaveRate 逐渐增加
  → 达到80%时可能召唤雷冠头狼
  → 达到100%时狼潮结束
```

## 5. 当前架构特点与潜在问题

### 优点
1. **主题鲜明**: 完整还原了《蛊真人》的修仙体系
2. **分层清晰**: Common/Content 分离，逻辑与内容解耦
3. **接口设计**: `IAccCanReforge`/`IWeaponCanReforge` 接口用于词缀系统
4. **本地化支持**: 中英文双语

### 潜在问题/改进点
1. **`build.txt` 存在 Git 冲突标记**（`<<<<<<< HEAD` / `>>>>>>> ppe`）
2. **`QiPlayer` 类过于庞大**：承担了真元、境界、Buff、战斗等多重职责
3. **炼化系统代码重复**：`GuWeaponItem` 和 `GuAccessoryItem` 有大量重复的炼化逻辑
4. **硬编码数值**：多处使用魔法数字（如 `UNITKONGQIAO = 100`）
5. **`ModifyWeaponDamage` 中直接修改 `item.damage`**：应使用 `StatModifier` 而非直接赋值
6. **部分文件存在备份残留**（`.png~` 文件）
7. **`GlobalLoots` 类继承 `GlobalTile` 但方法被注释掉**
8. **`DaosUI` 尚为框架，功能未实现**
