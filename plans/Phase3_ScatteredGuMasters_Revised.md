# Phase 3：散修蛊师（修订方案）

## 设计理念

根据用户反馈，散修蛊师**不是敌怪**，而是**NPC体系中的一部分**：

- ✅ 使用蛊攻击（复用现有Projectile）
- ✅ 可对话、可交易（但默认不信任玩家）
- ✅ 各有不同（名字、性格、蛊虫、修为多样化）
- ✅ 模拟真实蛊师世界

## 架构变化

### 删除
- `HostileGuMaster.cs` — 不再需要"敌对散修抽象类"
- `StrongScatteredGuMaster.cs` — 不再需要"精英敌怪"

### 新增
- `ScatteredGuMasterBase.cs` — 散修蛊师抽象基类（继承GuMasterBase）
- `ScatteredGuMaster_Wanderer.cs` — 一转游方散修（浪迹天涯型）
- `ScatteredGuMaster_Hermit.cs` — 二转隐居散修（隐居修炼型）
- `ScatteredGuMaster_Merchant.cs` — 一转行商散修（游商交易型）

### 修改
- `GuWorld_Overall_Architecture.md` — 更新Phase 3描述
- `Localization/zh-Hans_Mods.VerminLordMod.hjson` — 添加散修NPC本地化
- `Localization/en-US_Mods.VerminLordMod.hjson` — 添加散修NPC本地化

## 详细设计

### 1. ScatteredGuMasterBase 抽象基类

**文件**: `Content/NPCs/GuMasters/ScatteredGuMasterBase.cs`

继承自 `GuMasterBase`，添加散修专属行为：

```csharp
public abstract class ScatteredGuMasterBase : GuMasterBase
{
    // 散修固定势力
    public override FactionID GetFaction() => FactionID.Scattered;
    
    // 散修默认性格（子类可重写）
    public override GuPersonality GetPersonality() => GuPersonality.Neutral;
    
    // 散修名字列表（子类提供）
    public abstract string[] ScatteredNameList { get; }
    
    // 散修使用的蛊投射物类型
    public abstract int GuProjectileType { get; }
    
    // 散修出售物品列表（子类提供）
    public abstract List<Item> GetScatteredShopItems();
    
    // 散修对话（默认不信任）
    public override string GetDialogue(NPC npc, GuAttitude attitude)
    {
        switch (attitude)
        {
            case GuAttitude.Hostile:
                return $"{GuMasterDisplayName}拔出蛊虫：\"再上前一步，休怪我无情！\"";
            case GuAttitude.Wary:
                return $"{GuMasterDisplayName}警惕地看着你：\"你是哪家的？报上名来。\"";
            case GuAttitude.Ignore:
                return $"{GuMasterDisplayName}扫了你一眼：\"哼，又是个多管闲事的。\"";
            case GuAttitude.Friendly:
                return $"{GuMasterDisplayName}微微点头：\"道友请了，这世道能遇到个明白人不容易。\"";
            default:
                return base.GetDialogue(npc, attitude);
        }
    }
    
    // 散修战斗AI（使用蛊攻击）
    public override void ExecuteCombatAI(NPC npc)
    {
        var target = Main.player[npc.target];
        float dist = Vector2.Distance(npc.Center, target.Center);
        
        // 使用蛊虫攻击（复用现有Projectile）
        if (dist < 400f && Main.rand.NextBool(45))
        {
            Vector2 direction = target.Center - npc.Center;
            direction.Normalize();
            direction *= 7f;
            
            int projType = GuProjectileType;
            int damage = npc.damage / 2;
            Projectile.NewProjectile(
                npc.GetSource_FromAI(),
                npc.Center,
                direction,
                projType,
                damage,
                3f,
                Main.myPlayer
            );
        }
        
        // 基础移动逻辑
        // ...
    }
    
    // 散修被击杀后：增加恶名，不影响任何家族声望
    public override void OnKill()
    {
        var player = Main.LocalPlayer;
        var worldPlayer = player.GetModPlayer<GuWorldPlayer>();
        worldPlayer.AddInfamy(10);
        Main.NewText($"你击杀了一名{GuMasterDisplayName}！恶名上升。", Color.OrangeRed);
    }
}
```

### 2. ScatteredGuMaster_Wanderer — 游方散修

**文件**: `Content/NPCs/GuMasters/ScatteredGuMaster_Wanderer.cs`

- **修为**: 一转中阶~高阶
- **性格**: 随机（Neutral/Cautious/Greedy）
- **蛊攻击**: 使用基础蛊投射物（如 `MoonlightProj`、`GoldNeedleProj`、`PineNeedleProj` 等随机选择）
- **行为**: 在世界中游荡，不固定在一个地方
- **对话**: 警惕、不信任，但可以交易基础物品
- **商店**: 出售基础蛊材、食物、低级材料
- **生成**: 白天/夜晚均可，地表，非腐化/血腥区域

```csharp
[AutoloadHead]
public class ScatteredGuMaster_Wanderer : ScatteredGuMasterBase
{
    // 随机选择蛊虫类型
    private static readonly int[] PossibleGuProjectiles = {
        ModContent.ProjectileType<MoonlightProj>(),
        ModContent.ProjectileType<GoldNeedleProj>(),
        ModContent.ProjectileType<PineNeedleProj>(),
        ModContent.ProjectileType<WaterArrowProj>(),
    };
    
    private int _selectedGuType;
    
    public override int GuProjectileType => _selectedGuType;
    
    public override string[] ScatteredNameList => new[] {
        "云游散修", "流浪蛊师", "独行蛊师", "游方道人", "江湖散人"
    };
    
    // 游走AI：不断移动，不在一个地方久留
    // ...
}
```

### 3. ScatteredGuMaster_Hermit — 隐居散修

**文件**: `Content/NPCs/GuMasters/ScatteredGuMaster_Hermit.cs`

- **修为**: 二转初阶~中阶（比游方散修强）
- **性格**: 随机（Cautious/Neutral/Proud）
- **蛊攻击**: 使用更强力的蛊投射物（如 `IceKnife`、`FrostArrowProj`、`SwordQiProj`、`BloodSkullProj`）
- **行为**: 固定在某个区域"隐居"，不主动离开
- **对话**: 更加孤僻，但若玩家声望高或修为高，可能获得尊重
- **商店**: 出售更高级的蛊材、修炼资源
- **生成**: 夜晚概率更高，森林/洞穴区域

### 4. ScatteredGuMaster_Merchant — 行商散修

**文件**: `Content/NPCs/GuMasters/ScatteredGuMaster_Merchant.cs`

- **修为**: 一转中阶
- **性格**: Greedy（贪婪）
- **蛊攻击**: 使用非致命性蛊（如 `DingShenProj` 定身、`QingTengGuProj` 缠绕）
- **行为**: 类似旅行商人，在各地之间移动
- **对话**: 相对友好（为了做生意），但涉及利益时翻脸
- **商店**: 丰富的商品列表，价格可能略高
- **生成**: 白天，地表

## 蛊攻击复用策略

不创建新的投射物，而是**复用现有Projectile**，每个散修子类从以下池中随机选择：

| 散修类型 | 可用蛊投射物 | 风格 |
|---------|------------|------|
| 游方散修 | MoonlightProj, GoldNeedleProj, PineNeedleProj, WaterArrowProj | 基础蛊虫攻击 |
| 隐居散修 | IceKnife, FrostArrowProj, SwordQiProj, BloodSkullProj, StarArrowProj | 进阶蛊虫攻击 |
| 行商散修 | DingShenProj, QingTengGuProj, GoldNeedleProj | 非致命/控制型 |

## 对话与交易系统

### 态度决定对话

| 态度 | 对话风格 | 可交易? |
|------|---------|--------|
| Hostile | 威胁、拔蛊相向 | ❌ |
| Wary | 警惕、盘问来历 | ❌ |
| Ignore | 冷漠、爱理不理 | ✅（基础物品） |
| Friendly | 客气、称道友 | ✅（全部物品） |
| Contemptuous | 嘲讽、看不起 | ❌ |
| Fearful | 害怕、退缩 | ❌ |

### 商店系统

使用 `NPCShop` + `YuanS` 货币（与现有Town NPC一致）：

```csharp
public override void AddShops()
{
    var shop = new NPCShop(Type, ShopName);
    
    // 基础物品（所有散修都有）
    shop.Add(ModContent.ItemType<WanShi>());
    shop.Add(ModContent.ItemType<LivingLeaf>());
    
    // 散修特色物品（子类提供）
    foreach (var item in GetScatteredShopItems())
        shop.Add(item);
    
    shop.Register();
}
```

## 生成机制

散修蛊师使用 `SpawnChance` 控制生成，与家族蛊师（GuYuePatrolGuMaster）共享生成规则：

```csharp
public override float SpawnChance(NPCSpawnInfo spawnInfo)
{
    var qiPlayer = spawnInfo.Player.GetModPlayer<QiPlayer>();
    if (!qiPlayer.qiEnabled) return 0f;
    
    // 散修在各处都可能出现
    if (spawnInfo.Player.ZoneForest || spawnInfo.Player.ZoneDesert || spawnInfo.Player.ZoneJungle)
        return 0.03f;
    
    return 0f;
}
```

## 文件清单

### 新建文件
| 文件 | 说明 |
|------|------|
| `Content/NPCs/GuMasters/ScatteredGuMasterBase.cs` | 散修抽象基类 |
| `Content/NPCs/GuMasters/ScatteredGuMaster_Wanderer.cs` | 游方散修 |
| `Content/NPCs/GuMasters/ScatteredGuMaster_Hermit.cs` | 隐居散修 |
| `Content/NPCs/GuMasters/ScatteredGuMaster_Merchant.cs` | 行商散修 |
| `Content/NPCs/GuMasters/ScatteredGuMaster_Wanderer.png` | 贴图（从GuYuePatrolGuMaster复制） |
| `Content/NPCs/GuMasters/ScatteredGuMaster_Wanderer_Head.png` | 头像 |
| `Content/NPCs/GuMasters/ScatteredGuMaster_Hermit.png` | 贴图 |
| `Content/NPCs/GuMasters/ScatteredGuMaster_Hermit_Head.png` | 头像 |
| `Content/NPCs/GuMasters/ScatteredGuMaster_Merchant.png` | 贴图 |
| `Content/NPCs/GuMasters/ScatteredGuMaster_Merchant_Head.png` | 头像 |

### 修改文件
| 文件 | 说明 |
|------|------|
| `Localization/zh-Hans_Mods.VerminLordMod.hjson` | 添加散修NPC本地化 |
| `Localization/en-US_Mods.VerminLordMod.hjson` | 添加散修NPC本地化 |
| `plans/GuWorld_Overall_Architecture.md` | 更新Phase 3描述 |

## 实现步骤

### 步骤1：创建 ScatteredGuMasterBase.cs
散修抽象基类，包含：
- 固定 FactionID.Scattered
- 蛊攻击抽象方法
- 默认散修对话
- 散修击杀处理
- 商店系统骨架

### 步骤2：创建 ScatteredGuMaster_Wanderer.cs
游方散修——最常见，到处游荡，使用基础蛊攻击

### 步骤3：创建 ScatteredGuMaster_Hermit.cs
隐居散修——更强，固定区域，使用进阶蛊攻击

### 步骤4：创建 ScatteredGuMaster_Merchant.cs
行商散修——友好倾向，丰富商品，使用控制型蛊

### 步骤5：复制占位贴图
从 GuYuePatrolGuMaster 复制贴图作为占位

### 步骤6：更新本地化文件
添加散修NPC的 DisplayName 和 TownNPCMood

### 步骤7：更新架构文档
标记 Phase 3 完成状态

## 与旧方案对比

| 方面 | 旧方案（已否决） | 新方案 |
|------|----------------|--------|
| NPC类型 | 敌怪（Enemy） | NPC体系（继承GuMasterBase） |
| 攻击方式 | 普通投射物（WoodenArrow） | 蛊攻击（复用现有Projectile） |
| 对话 | ❌ 无 | ✅ 有（态度决定内容） |
| 交易 | ❌ 无 | ✅ 有（态度决定是否可交易） |
| 多样性 | 2种（普通+精英） | 3种（游方+隐居+行商） |
| 名字 | 固定 | 随机名字列表 |
| 性格 | 固定好斗 | 随机性格 |
| 击杀影响 | 无 | 增加恶名 |
| 真实感 | 低（纯敌怪） | 高（模拟真实散修） |
