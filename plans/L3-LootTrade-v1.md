# L3 实现文档：搜尸/掠夺系统（LootSystem）+ 交易/定价系统（TradeSystem）

> 版本：v1.0（基于 L2-System-Interfaces-v1.2）  
> 优先级：P1 / 沙盒闭环  
> 预计工作量：LootSystem 2 天 + TradeSystem 1 天 = 3 天  
> 依赖：NpcDeathHandler（尸体实体）/ QiResourcePlayer（真元/元石）/ GuPerkSystem（NPC 财富评估）

---

## 第一部分：LootSystem — 搜尸与掠夺

### 1. 交付目标

实现「暴露掉落 + 深度搜尸」的分层设计：
1. **暴露掉落**：击杀瞬间 30% 基础物品散落，可立即拾取
2. **深度搜尸**：玩家/ NPC 对尸体长按交互 3-5 秒，获得稀有物品，期间可被感知
3. **基地搜刮**：NPC 只攻击在场玩家，不翻箱子（D-15）

**验收标准**：玩家能感受到「击杀后立即捡便宜的暴露掉落」和「蹲下来翻尸体的紧张感」。

### 2. 文件清单

| 文件 | 动作 | 说明 |
|------|------|------|
| `Common/Systems/LootSystem.cs` | **新建** | 搜尸核心逻辑、深度搜尸状态机、基地规则 |
| `Common/UI/DeepLootUI.cs` | **新建** | 深度搜尸进度条（MVA 简化：屏幕中央圆形进度） |

### 3. 核心机制

```csharp
public class LootSystem : ModSystem
{
    // ===== 暴露掉落（击杀瞬间） =====
    public List<Item> CalculateExposedDrops(Player player, float rate = 0.3f)
    {
        var drops = new List<Item>();
        foreach (Item item in player.inventory)
        {
            if (item.IsAir) continue;
            if (IsEssentialItem(item)) continue; // 媒介/空窍石不掉落
            if (Main.rand.NextFloat() < rate)
            {
                drops.Add(item.Clone());
                item.TurnToAir();
            }
        }
        return drops;
    }

    private bool IsEssentialItem(Item item)
    {
        return item.type == ModContent.ItemType<GuMediumWeapon>()
            || item.type == ModContent.ItemType<KongQiaoStone>();
    }

    // ===== 深度搜尸（持续交互） =====
    private Dictionary<int, DeepLootState> activeDeepLoots = new(); // Key = Player.whoAmI

    public void StartDeepLoot(Player player, PlayerCorpse corpse)
    {
        if (activeDeepLoots.ContainsKey(player.whoAmI)) return;

        int duration = 180; // 3 秒 = 180 帧（MVA 简化）
        activeDeepLoots[player.whoAmI] = new DeepLootState
        {
            PlayerID = player.whoAmI,
            Corpse = corpse,
            Duration = duration,
            Elapsed = 0,
            IsInterrupted = false
        };

        // 发布事件：NPC 可感知
        EventBus.Publish(new DeepLootingStartedEvent
        {
            PlayerID = player.whoAmI,
            CorpsePosition = corpse.Center,
            CorpseOwnerPlayerID = corpse.OwnerPlayerID,
            DurationTicks = duration
        });

        if (player.whoAmI == Main.myPlayer)
            Main.NewText("开始搜尸...", Color.Yellow);
    }

    public void UpdateDeepLoot(Player player)
    {
        if (!activeDeepLoots.TryGetValue(player.whoAmI, out var state)) return;

        // 检查中断条件
        if (player.velocity.Length() > 0.5f || player.itemAnimation > 0)
        {
            state.IsInterrupted = true;
            EndDeepLoot(player, state);
            return;
        }

        state.Elapsed++;

        // 显示进度（MVA 简化：聊天栏百分比）
        if (player.whoAmI == Main.myPlayer && state.Elapsed % 30 == 0)
        {
            float pct = (float)state.Elapsed / state.Duration * 100f;
            Main.NewText($"搜尸中... {pct:F0}%", Color.Yellow);
        }

        if (state.Elapsed >= state.Duration)
        {
            EndDeepLoot(player, state);
        }
    }

    private void EndDeepLoot(Player player, DeepLootState state)
    {
        activeDeepLoots.Remove(player.whoAmI);

        if (state.IsInterrupted)
        {
            if (player.whoAmI == Main.myPlayer)
                Main.NewText("搜尸被中断！", Color.Red);

            EventBus.Publish(new DeepLootingCompletedEvent
            {
                PlayerID = player.whoAmI,
                CorpsePosition = state.Corpse.Center,
                LootedItemTypes = new List<int>(),
                WasInterrupted = true
            });
            return;
        }

        // 成功完成：获得稀有物品
        var looted = new List<Item>();
        int rareCount = Math.Min(2, state.Corpse.RemainingItems.Count);
        for (int i = 0; i < rareCount && state.Corpse.RemainingItems.Count > 0; i++)
        {
            int idx = Main.rand.Next(state.Corpse.RemainingItems.Count);
            var item = state.Corpse.RemainingItems[idx];
            state.Corpse.RemainingItems.RemoveAt(idx);
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), item);
            looted.Add(item);
        }

        if (player.whoAmI == Main.myPlayer)
        {
            string names = looted.Count > 0 ? string.Join(", ", looted.Select(i => i.Name)) : "无";
            Main.NewText($"搜尸完成！获得：{names}", Color.Green);
        }

        EventBus.Publish(new DeepLootingCompletedEvent
        {
            PlayerID = player.whoAmI,
            CorpsePosition = state.Corpse.Center,
            LootedItemTypes = looted.Select(i => i.type).ToList(),
            WasInterrupted = false
        });
    }

    // ===== 基地搜刮规则（D-15） =====
    public bool CanNPCAttackBase(NPC npc, Player player)
    {
        // NPC 只攻击在场且携带高价值物品的玩家
        if (!player.active) return false; // 玩家不在场

        var qiResource = player.GetModPlayer<QiResourcePlayer>();
        // "高价值" = 携带元石 > 50 或启用高阶蛊虫
        bool hasHighValue = qiResource.QiOccupied > 30; // 简化判定

        return hasHighValue;
    }

    public override void PostUpdateWorld()
    {
        // 每帧更新所有进行中的深度搜尸
        foreach (Player player in Main.player)
        {
            if (player.active && activeDeepLoots.ContainsKey(player.whoAmI))
                UpdateDeepLoot(player);
        }
    }
}

public class DeepLootState
{
    public int PlayerID;
    public PlayerCorpse Corpse;
    public int Duration;
    public int Elapsed;
    public bool IsInterrupted;
}
```

### 4. LootSystem 验收标准

| 场景 | 操作 | 预期行为 |
|------|------|---------|
| 暴露掉落 | 击杀玩家/NPC | 30% 物品散落，可立即拾取 |
| 深度搜尸 | 长按尸体 | 3 秒进度，完成后获得稀有物品 |
| 搜尸中断 | 移动或攻击 | 进度重置，提示中断 |
| NPC 感知搜尸 | NPC 在附近 | 收到 DeepLootingStartedEvent，可能偷袭 |
| 基地安全 | 玩家不在家 | NPC 不翻箱子，只攻击在场玩家 |

---

## 第二部分：TradeSystem — 动态定价

### 5. 交付目标

实现「完全自由定价」的交易系统：
1. NPC 根据玩家急需程度、声望、自身原型坐地起价
2. 无价格上限，玩家可选择接受/拒绝/动手
3. MVA 阶段只实现城镇 NPC（学堂家老/药堂家老/贾家商人）

**验收标准**：玩家能感受到「急需疗伤时药堂家老漫天要价」的压迫感。

### 6. 文件清单

| 文件 | 动作 | 说明 |
|------|------|------|
| `Common/Systems/TradeSystem.cs` | **新建** | 动态定价算法、交易完成事件 |
| `Content/NPCs/Town/YaoTangJiaLao.cs` | **修改** | 接入动态定价（药堂家老） |
| `Content/NPCs/Town/JiasTravelingMerchant.cs` | **修改** | 接入动态定价（贾家商人） |

### 7. 核心机制

```csharp
public class TradeSystem : ModSystem
{
    /// <summary>
    /// 计算动态价格。
    /// 公式：基础价格 × 急需度 × (1/声望折扣) × (1/原型亲和)
    /// </summary>
    public float CalculatePrice(Player player, NPC npc, Item item)
    {
        float basePrice = item.value;

        // 1. 急需度：玩家是否急需此物品（MVA 简化： always 1.5，P1 再细化）
        float urgency = CalculateUrgency(player, item);

        // 2. 声望折扣：友好 = 0.8，敌对 = 2.0，中立 = 1.0
        float reputationMultiplier = CalculateReputationDiscount(player, npc);

        // 3. 原型亲和：交易型 NPC 更温和，掠夺型更狠
        float archetypeMultiplier = 1f;
        if (npc.ModNPC is GuMasterBase guMaster)
        {
            archetypeMultiplier = guMaster.Archetype.CooperationBias; // 0.5 ~ 1.5
        }

        // D-16: 完全自由，无上限
        return basePrice * urgency * (1f / reputationMultiplier) * (1f / archetypeMultiplier);
    }

    private float CalculateUrgency(Player player, Item item)
    {
        // MVA 简化：根据玩家生命值判断急需度
        float healthPct = (float)player.statLife / player.statLifeMax2;
        if (healthPct < 0.3f && item.healLife > 0) return 3.0f; // 濒死急需治疗 = 3 倍
        if (healthPct < 0.5f && item.healLife > 0) return 2.0f;
        if (player.GetModPlayer<QiResourcePlayer>().QiCurrent < 20f) return 1.5f; // 真元低 = 急需元石
        return 1.0f;
    }

    private float CalculateReputationDiscount(Player player, NPC npc)
    {
        var guWorldPlayer = player.GetModPlayer<GuWorldPlayer>();
        var faction = (npc.ModNPC as GuMasterBase)?.Faction ?? FactionID.Scattered;
        int rep = guWorldPlayer.GetReputation(faction); // -100 ~ 100

        // 声望 → 折扣：敌对( -100 ) = 2.0 倍，中立(0) = 1.0，友好(100) = 0.5 倍
        return MathHelper.Lerp(2.0f, 0.5f, (rep + 100f) / 200f);
    }

    /// <summary>
    /// 交易完成后发布事件。
    /// </summary>
    public void OnTradeCompleted(Player player, NPC npc, List<Item> sold, List<Item> bought, int yuanStoneDelta)
    {
        bool isGouged = CalculatePrice(player, npc, bought.FirstOrDefault()) > bought.FirstOrDefault()?.value * 2;

        EventBus.Publish(new TradeCompletedEvent
        {
            PlayerID = player.whoAmI,
            NPCType = npc.type,
            SoldItemTypes = sold.Select(i => i.type).ToList(),
            BoughtItemTypes = bought.Select(i => i.type).ToList(),
            YuanStoneDelta = yuanStoneDelta,
            IsPriceGouged = isGouged
        });
    }
}
```

### 8. 与城镇 NPC 的衔接

修改 `YaoTangJiaLao.cs`：

```csharp
public class YaoTangJiaLao : ModNPC
{
    public override void SetChatButtons(ref string button, ref string button2)
    {
        button = "购买丹药";
        button2 = "治疗";
    }

    public override void OnChatButtonClicked(bool firstButton, ref bool shop)
    {
        if (firstButton)
        {
            // 打开动态定价商店
            shop = true;
            // 原版商店逻辑，但价格由 TradeSystem 计算
        }
        else
        {
            // 治疗服务：价格动态计算
            Player player = Main.LocalPlayer;
            float healPrice = TradeSystem.Instance.CalculatePrice(player, NPC, 
                new Item(ItemID.HealingPotion)); // 用治疗药水作为价格基准

            if (player.GetModPlayer<QiResourcePlayer>().ConsumeQi(healPrice, QiConsumeReason.Skill))
            {
                player.statLife = player.statLifeMax2;
                Main.NewText($"药堂家老为你疗伤，收取 {healPrice:F0} 真元等价的元石。", Color.Green);
            }
            else
            {
                Main.NewText("元石不足，无法治疗。", Color.Red);
            }
        }
    }

    public override void ModifyActiveShop(string shopName, Item[] items)
    {
        // 动态调整商店物品价格
        Player player = Main.LocalPlayer;
        foreach (Item item in items)
        {
            if (item.IsAir) continue;
            float newPrice = TradeSystem.Instance.CalculatePrice(player, NPC, item);
            item.value = (int)newPrice;
        }
    }
}
```

### 9. TradeSystem 验收标准

| 场景 | 操作 | 预期行为 |
|------|------|---------|
| 正常交易 | 与药堂家老购买 | 价格基于基础价值 |
| 濒死治疗 | 生命值 < 30% 时治疗 | 价格 3 倍，提示昂贵 |
| 敌对交易 | 声望敌对时购买 | 价格 2 倍，NPC 冷言冷语 |
| 交易拒绝 | 元石不足 | NPC 拒绝服务，可能嘲讽 |
| 动手抢夺 | 价格太高时攻击 NPC | 进入战斗状态，可击杀 |

---

> 本文档由第三层实现工程师维护。  
> 实现过程中发现接口不匹配，立即回传 L2 登记修正。
