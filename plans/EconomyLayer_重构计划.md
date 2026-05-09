# EconomyLayer 重构计划

> **目标**：理清经济层的货币体系、交易系统、资源节点、搜尸/掉落、防御工事之间的经济循环，消除 `GlobalNPCLoot` 直接掉落元石与计划中"元石仅通过交易和资源节点获取"的矛盾，建立完整的元石经济闭环。

---

## 现状分析

### 已实现的经济系统

| 系统 | 文件 | 行数 | 状态 |
|------|------|------|------|
| [`TradeSystem`](Common/Systems/TradeSystem.cs) | 动态定价交易系统 | 248 行 | ✅ 完整实现 |
| [`ResourceNodeSystem`](Common/Systems/ResourceNodeSystem.cs) | 资源节点（元泉） | 287 行 | ✅ 基础实现 |
| [`LootSystem`](Common/Systems/LootSystem.cs) | 搜尸系统 | 411 行 | ✅ 完整实现 |
| [`DefenseSystem`](Common/Systems/DefenseSystem.cs) | 迷踪阵防御 | 303 行 | ✅ 完整实现 |
| [`NpcDeathHandler`](Common/Systems/NpcDeathHandler.cs) | 死亡处理（掉落、尸体） | 405 行 | ✅ 完整实现 |
| [`GlobalNPCLoot`](Common/GlobalNPCs/GlobalNPCLoot.cs) | 全局 NPC 掉落 | 315 行 | ✅ 完整实现 |
| [`YuanS`](Content/Items/Consumables/YuanS.cs) | 元石物品 | — | ✅ 存在 |

### 关键发现

1. **`GlobalNPCLoot` 第 295 行直接掉落元石**：
   ```csharp
   npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<YuanS>(), 2, 0, (npc.damage + npc.lifeMax + npc.defense) / 6));
   ```
   这与计划中"元石仅通过交易和资源节点获取"的设计目标矛盾。所有非 Boss 敌人都可能掉落元石。

2. **`TradeSystem` 已有完整的动态定价模型**：
   - `finalPrice = basePrice * urgency * (1/reputationMultiplier) * (1/archetypeMultiplier)`
   - 紧急度基于 NPC 生命值和真元
   - 声望倍率：敌对 2.0x → 中立 1.0x → 友好 0.5x
   - 原型倍率：基于 `GuPersonality`
   - 价格欺诈检测（`IsPriceGouged`，阈值 2.0x）

3. **`ResourceNodeSystem` 已有基础实现**：
   - 元泉（YuanSpring）资源节点
   - 再生机制（7 天恢复 50%）
   - 采集接口（`Harvest` 方法）
   - 默认在世界中心附近生成一个元泉，由 GuYue 控制

4. **`LootSystem` 已有完整搜尸流程**：
   - 玩家死亡：30% 物品暴露，创建尸体
   - NPC 可 loot 玩家尸体（50% 剩余物品）
   - 玩家可 loot NPC 尸体
   - 关键物品保护（GuMediumWeapon、KongQiaoStone）

5. **`DefenseSystem` 已有完整实现**：
   - 迷踪阵（MiZongZhenTile）— 减少 NPC 感知范围 50%
   - 守卫风险加成
   - 玩家状态追踪（DefensePlayer ModPlayer）

6. **`NpcDeathHandler` 处理了死亡经济**：
   - 玩家被杀：掉落物暴露、尸体创建、事件发布
   - NPC 被杀：死亡事件、悬赏发布、职位空缺
   - 悬赏金额计算：`lifeMax / 10`

---

## 重构计划

### Phase 0：立即修复（P0）

#### D-32：移除 GlobalNPCLoot 的元石直接掉落

**问题**：`GlobalNPCLoot` 第 295 行让所有非 Boss 敌人都可能掉落元石，破坏了"元石仅通过交易和资源节点获取"的经济设计。

**方案**：

```csharp
// 修改 GlobalNPCLoot.cs 第 295 行
// 原来：
npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<YuanS>(), 2, 0, 
    (npc.damage + npc.lifeMax + npc.defense) / 6));

// 改为：移除元石掉落，改为掉落可交易的蛊虫材料
// 替代掉落：虫壳、毒腺、丝线等材料
// 玩家可通过交易将这些材料卖给 NPC 换取元石，保持经济循环
```

**替代掉落设计**：
- 普通蛊虫：掉落虫壳（`InsectCarapace`）、毒液（`VenomSack`）
- 精英蛊虫：额外掉落丝线（`SilkThread`）、精魄（`EssenceShard`）
- Boss：保留特殊掉落，不涉及元石

---

### Phase 1：核心增强（P1）

#### D-33：元石经济闭环

**问题**：当前元石的获取和消耗没有形成闭环：
- 获取：~~GlobalNPCLoot 掉落~~ + 资源节点采集 + 交易
- 消耗：交易购买物品
- 缺少：元石消耗出口（如修炼消耗、阵法维护、装备强化）

**方案**：建立元石经济闭环：

```
元石获取                         元石消耗
├── 资源节点（元泉）采集          ├── NPC 交易购买
├── 完成任务/悬赏                 ├── 修炼消耗（突破时需要元石）
├── 出售物品/材料给 NPC           ├── 阵法维护（迷踪阵消耗元石）
├── 搜尸（极小概率）              ├── 装备强化/炼蛊
└── 势力工资（加入势力后定期）    └── 传送费用（跨区域传送）
```

**具体实现**：

```csharp
// 修炼消耗：突破时消耗元石
// 在 QiRealmPlayer.StageUp / LevelUp 中增加元石检查
public class QiRealmPlayer : ModPlayer
{
    public bool StageUp()
    {
        int yuanStoneCost = GetBreakthroughCost();
        if (!ConsumeYuanStones(player, yuanStoneCost))
            return false;  // 元石不足，无法突破
        // ... 突破逻辑
    }
    
    private int GetBreakthroughCost()
    {
        return LevelStage switch
        {
            0 => 10,   // 小境界突破
            1 => 20,
            2 => 30,
            3 => 50 + Level * 10,  // 大境界突破
        };
    }
}
```

> **注意**：当前 `QiRealmPlayer.StageUp` 和 `LevelUp` 中未实现元石检查，需要在此处添加。

#### D-34：资源节点扩展

**问题**：当前只有一个元泉资源节点，且只支持 GuYue 势力控制。

**方案**：

```csharp
public enum ResourceNodeType
{
    YuanSpring,      // 元泉 → 元石
    HerbGarden,      // 药园 → 药材
    OreVein,         // 矿脉 → 矿石
    SpiritPool,      // 灵池 → 修炼加速
}

public class ResourceNode
{
    public Vector2 Position;
    public ResourceNodeType Type;
    public int CurrentAmount;
    public int MaxAmount;
    public int RegenTimer;
    public FactionID ControllingFaction;
    public float RegenRatio = 0.5f;
    public int RegenInterval = 42000;  // 7 天
    
    // 不同资源类型的产出
    public int GetHarvestItemType() => Type switch
    {
        ResourceNodeType.YuanSpring => ModContent.ItemType<YuanS>(),
        ResourceNodeType.HerbGarden => ModContent.ItemType<Herb>(),
        ResourceNodeType.OreVein => ModContent.ItemType<Ore>(),
        ResourceNodeType.SpiritPool => ModContent.ItemType<SpiritEssence>(),
    };
}
```

#### D-37：防御工事经济

**问题**：`DefenseSystem` 的迷踪阵没有维护成本。

**方案**：
```csharp
public class DefenseSystem : ModSystem
{
    public const int MIZONG_MAINTENANCE_COST = 5;  // 每个迷踪阵每天消耗 5 元石
    
    public override void PreUpdateWorld()
    {
        if (Main.dayTime && Main.time == 0)  // 每天开始时
        {
            foreach (var pos in ActiveMiZongPositions)
            {
                // 检查附近玩家是否有足够元石
                // 如果没有，迷踪阵失效
            }
        }
    }
}
```

---

### Phase 2：远期规划（P2）

#### D-35：动态物价系统增强

**问题**：`TradeSystem` 已有动态定价，但缺少：
- 跨 NPC 的价格联动（一个 NPC 的价格影响其他 NPC）
- 供需关系影响（某种物品被大量购买后价格上涨）
- 黑市价格（高风险高回报）

**方案**：

```csharp
public class MarketSystem : ModSystem
{
    // 全局供需追踪
    public Dictionary<int, float> SupplyDemandIndex;  // item.type → 供需指数
    
    // 价格波动
    public float GetMarketPrice(Item item)
    {
        float basePrice = item.value;
        float supplyDemand = GetSupplyDemandFactor(item.type);
        float volatility = GetRandomVolatility(item.type);
        return basePrice * supplyDemand * volatility;
    }
    
    // 记录交易影响供需
    public void RecordTrade(Item item, int quantity)
    {
        if (!SupplyDemandIndex.ContainsKey(item.type))
            SupplyDemandIndex[item.type] = 1.0f;
        // 购买增加需求，出售增加供给
        SupplyDemandIndex[item.type] += quantity * 0.01f;
    }
}
```

> **降级说明**：当前 `TradeSystem` 已有足够的动态定价能力，供需系统为远期增强。

#### D-36：搜尸系统经济化

**问题**：当前搜尸系统只处理物品暴露和拾取，没有经济层面的设计。

**方案**：
1. 搜尸获得物品可自动估价
2. 添加"搜刮"perk 提升搜尸收益
3. 尸体可被 NPC 搜刮（已实现 `NPCLootCorpse`），但 NPC 搜刮后物品进入 NPC 库存，玩家可通过交易买回

#### D-38：势力经济系统

**方案**：每个势力拥有独立的经济系统：
- 势力金库（元石储备）
- 势力商店（成员可购买特殊物品）
- 势力任务（完成后获得元石和声望）
- 势力税收（成员交易的部分收入上缴）

#### D-39：玩家商铺

**方案**：玩家可开设自己的商铺：
- 租赁店铺（消耗元石）
- 上架物品（自定义价格）
- 自动交易（离线时也可交易）

---

## 经济循环图

```
                    ┌─────────────────────────────┐
                    │         元石 (YuanS)          │
                    └──────────┬──────────────────┘
                               │
            ┌──────────────────┼──────────────────┐
            ▼                  ▼                  ▼
    ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
    │  资源节点采集  │  │  NPC 交易     │  │  任务/悬赏    │
    │  (D-34)       │  │  (D-35)       │  │  (Narrative) │
    └──────┬───────┘  └──────┬───────┘  └──────┬───────┘
           │                 │                 │
           └─────────────────┼─────────────────┘
                             ▼
                    ┌────────────────┐
                    │   元石收入      │
                    └───────┬────────┘
                            │
            ┌───────────────┼───────────────┐
            ▼               ▼               ▼
    ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
    │  修炼消耗     │ │  阵法维护    │ │  装备强化    │
    │  (D-33)       │ │  (D-37)      │ │  (P2)        │
    └──────────────┘ └──────────────┘ └──────────────┘
```

## 迁移路线图

| 步骤 | 内容 | 工作量 | 风险 | 优先级 |
|------|------|--------|------|--------|
| D-32 | 移除 GlobalNPCLoot 元石掉落 | 小 | 中 — 影响游戏平衡，需要替代掉落 | **P0** |
| D-33 | 元石经济闭环（修炼消耗、阵法维护） | 中 | 中 — 需要设计消耗出口 | P1 |
| D-34 | 资源节点扩展 | 中 | 低 — 已有基础 | P1 |
| D-37 | 防御工事经济（迷踪阵维护成本） | 小 | 低 — 纯新增 | P1 |
| D-35 | 动态物价系统增强 | 大 | 中 — 需要设计供需模型 | P2（降级，已有基础） |
| D-36 | 搜尸系统经济化 | 小 | 低 — 纯增强 | P2 |
| D-38 | 势力经济系统 | 大 | 高 — 复杂的经济模拟 | P2 |
| D-39 | 玩家商铺 | 大 | 高 — 需要 UI 和存储 | P2 |
