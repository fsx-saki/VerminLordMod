# L3 实现文档：NPC 死亡处理者（NpcDeathHandler）

> 版本：v1.0（基于 L2-System-Interfaces-v1.2）  
> 目标：实现「击杀 → 尸体 → 搜尸 → 悬赏」的零和博弈闭环  
> 优先级：P0-P1 / MVA 简化版  
> 预计工作量：2-3 天  
> 依赖：GuYuePatrolGuMaster（发布死亡事件）/ KongQiaoPlayer（玩家死亡处理）/ LootSystem（搜尸）

---

## 1. 交付目标 

MVA 阶段实现最简版本：
1. **玩家尸体**：死亡后生成可交互实体，散落 30% 基础物品
2. **NPC 搜尸**：掠夺型 NPC 发现尸体后搜刮剩余物品
3. **死亡日志**：聊天栏显示「你的尸体被 [NPC] 搜索过，失去了 [物品]」
4. **基础悬赏**：NPC 死亡后家族发布悬赏（简化版：直接事件发布）

P1 阶段扩展：深度搜尸 3-5 秒进度条、NPC 目击链、完整悬赏面板。

**验收标准**：玩家死亡后能看到自己的尸体，NPC 会搜刮它，玩家可以跑回去捡剩余物品或复仇。

---

## 2. 工程定位

NpcDeathHandler 是**零和博弈的压舱石**。没有它，死亡只是「回家睡觉」，黑暗森林的威慑力归零。

**阻塞关系**：
- 被 GuYuePatrolGuMaster 依赖（击杀后发布 NPCDeathEvent）
- 阻塞 LootSystem（搜尸逻辑依赖尸体实体）
- 阻塞 BountySystem（悬赏由死亡事件触发）

**实现顺序**：紧接 GuYuePatrolGuMaster 之后，与 AwakeningSystem 并行。

---

## 3. 文件清单

| 文件 | 动作 | 说明 |
|------|------|------|
| `Common/Systems/NpcDeathHandler.cs` | **新建** | 死亡事件处理、尸体生成、悬赏发布 |
| `Common/Entities/PlayerCorpse.cs` | **新建** | 玩家尸体实体（可交互、可搜尸、会腐烂） |
| `Content/Items/Weapons/GuMediumWeapon.cs` | **修改** | 增加「媒介武器死亡不掉落」标记 |

---

## 4. 核心机制

### 4.1 玩家尸体实体

```csharp
public class PlayerCorpse : ModProjectile
{
    // 使用 Projectile 而非 NPC，避免被原版 NPC 系统干扰
    // 静态实体，不移动，可交互

    public int OwnerPlayerID;           // 尸体原主人
    public List<Item> RemainingItems;   // 剩余物品（未被搜走的）
    public int DecayTimer;              // 腐烂计时器（默认 18000 帧 = 5 分钟）
    public bool IsLootedByNPC;          // 是否被 NPC 搜过
    public int LootingNPCType;          // 搜尸的 NPC 类型
    public string LootingNPCName;       // 搜尸的 NPC 名称（用于日志）

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 48;
        Projectile.aiStyle = -1;        // 自定义 AI
        Projectile.timeLeft = 18000;  // 5 分钟腐烂
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;      // 无限穿透（不消失）
    }

    public override void AI()
    {
        // 静止不动
        Projectile.velocity = Vector2.Zero;

        // 腐烂倒计时
        if (Projectile.timeLeft <= 0)
        {
            // 腐烂完成：剩余物品散落为原版掉落物
            foreach (var item in RemainingItems)
            {
                if (!item.IsAir)
                    Item.NewItem(Projectile.GetSource_Death(), Projectile.Center, item);
            }
            RemainingItems.Clear();
        }
    }

    public override void OnKill()
    {
        // 确保剩余物品不会凭空消失
        foreach (var item in RemainingItems)
        {
            if (!item.IsAir)
                Item.NewItem(Projectile.GetSource_Death(), Projectile.Center, item);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        // 绘制玩家倒下的剪影（MVA 简化：用灰色矩形 + 玩家名字）
        // P1 可替换为玩家贴图染色
        return true;
    }
}
```

### 4.2 死亡处理流程

```csharp
public class NpcDeathHandler : ModSystem
{
    /// <summary>
    /// 玩家死亡时调用。由 Player.Kill 事件触发。
    /// </summary>
    public void OnPlayerKilled(Player player, int? killerNPCType = null)
    {
        // 1. 真元清空（已由 QiResourcePlayer.OnDeathClearQi 处理）
        // 2. 蛊虫休眠 + 忠诚度判定（已由 KongQiaoPlayer.OnPlayerDeath 处理）

        // 3. 计算暴露掉落（30% 基础物品散落）
        var exposedDrops = CalculateExposedDrops(player);
        foreach (var item in exposedDrops)
        {
            Item.NewItem(player.GetSource_Death(), player.Center, item);
        }

        // 4. 创建尸体实体
        var remaining = GetRemainingItems(player, exposedDrops);
        int corpseIndex = Projectile.NewProjectile(
            player.GetSource_Death(),
            player.Center,
            Vector2.Zero,
            ModContent.ProjectileType<PlayerCorpse>(),
            0, 0, player.whoAmI);

        if (Main.projectile[corpseIndex].ModProjectile is PlayerCorpse corpse)
        {
            corpse.OwnerPlayerID = player.whoAmI;
            corpse.RemainingItems = remaining;
        }

        // 5. 发布事件
        EventBus.Publish(new PlayerDeathEvent
        {
            PlayerID = player.whoAmI,
            Position = player.Center,
            DroppedItemTypes = exposedDrops.Select(i => i.type).ToList(),
            KillerNPCID = killerNPCType ?? -1,
            IsBackstab = false  // MVA 简化，P1 再检测
        });

        // 6. 死亡日志
        if (killerNPCType.HasValue)
        {
            Main.NewText($"你被 {Lang.GetNPCNameValue(killerNPCType.Value)} 击杀。尸体留在原地。", Color.Red);
        }
        else
        {
            Main.NewText("你死了。尸体留在原地。", Color.Red);
        }
    }

    /// <summary>
    /// NPC 死亡时调用。由 GuMasterBase.OnKill 触发。
    /// </summary>
    public void OnNPCKilled(NPC npc, Player killer)
    {
        // 1. 发布死亡事件
        var deathEvent = new NPCDeathEvent
        {
            NPCType = npc.type,
            NPCWhoAmI = npc.whoAmI,
            KillerPlayerID = killer?.whoAmI ?? -1,
            Position = npc.Center,
            Faction = (npc.ModNPC as GuMasterBase)?.Faction ?? FactionID.Scattered,
            VacatedRole = (npc.ModNPC as GuMasterBase)?.Role ?? FactionRole.None
        };
        EventBus.Publish(deathEvent);

        // 2. 发布悬赏事件（MVA 简化：只有家族 NPC 死亡才发布）
        if (deathEvent.Faction != FactionID.Scattered && killer != null)
        {
            EventBus.Publish(new BountyPostedEvent
            {
                PostingFaction = deathEvent.Faction,
                TargetPlayerID = killer.whoAmI,
                RewardAmount = CalculateBountyReward(npc),
                Reason = BountyReason.Revenge,
                BountyID = GenerateBountyID()
            });

            Main.NewText($"{deathEvent.Faction} 对 {killer.name} 发布了悬赏！", Color.Orange);
        }

        // 3. 职务空缺处理（MVA：硬编码继承顺位）
        if (deathEvent.VacatedRole != FactionRole.None)
        {
            EventBus.Publish(new RoleVacancyEvent
            {
                Faction = deathEvent.Faction,
                VacatedRole = deathEvent.VacatedRole,
                DeceasedNPCType = npc.type,
                SuccessorNPCType = GetHardcodedSuccessor(deathEvent.Faction, deathEvent.VacatedRole)
            });
        }
    }

    // ===== 暴露掉落计算 =====
    private List<Item> CalculateExposedDrops(Player player)
    {
        var drops = new List<Item>();
        float dropRate = 0.3f;  // 30% 物品散落

        foreach (Item item in player.inventory)
        {
            if (item.IsAir) continue;
            if (item.type == ModContent.ItemType<GuMediumWeapon>()) continue; // 媒介不掉落
            if (item.type == ModContent.ItemType<KongQiaoStone>()) continue;   // 空窍石不掉落

            if (Main.rand.NextFloat() < dropRate)
            {
                drops.Add(item.Clone());
                item.TurnToAir();
            }
        }

        // 元石特殊处理：50% 散落
        // 元石在背包中以堆叠形式存在，这里简化处理
        return drops;
    }

    private List<Item> GetRemainingItems(Player player, List<Item> exposed)
    {
        var remaining = new List<Item>();
        foreach (Item item in player.inventory)
        {
            if (!item.IsAir && !exposed.Any(e => e.type == item.type && e.stack == item.stack))
            {
                remaining.Add(item.Clone());
            }
        }
        return remaining;
    }

    private int CalculateBountyReward(NPC npc)
    {
        // MVA 简化：奖励 = NPC 生命值 / 10
        return npc.lifeMax / 10;
    }

    private int GenerateBountyID()
    {
        return Main.GameUpdateCount + Main.rand.Next(1000);
    }

    private int GetHardcodedSuccessor(FactionID faction, FactionRole role)
    {
        // MVA 硬编码继承顺位
        // 例如：古月药堂家老死亡 → 古月药姬（弟子）接替
        return -1; // 占位，P2 再实现具体 NPC
    }
}
```

### 4.3 NPC 搜尸逻辑

```csharp
public class NpcDeathHandler : ModSystem
{
    /// <summary>
    /// NPC 发现玩家尸体后搜尸。
    /// 由 GuYuePatrolGuMaster 的 CorpseReaction.Loot 触发。
    /// </summary>
    public void NPCLootCorpse(NPC npc, PlayerCorpse corpse)
    {
        if (corpse.IsLootedByNPC) return;  // 已被搜过
        if (corpse.RemainingItems.Count == 0) return;

        // 掠夺型 NPC 搜走 50% 剩余物品
        int lootCount = Math.Max(1, corpse.RemainingItems.Count / 2);
        var looted = new List<Item>();

        for (int i = 0; i < lootCount && corpse.RemainingItems.Count > 0; i++)
        {
            int index = Main.rand.Next(corpse.RemainingItems.Count);
            var item = corpse.RemainingItems[index];
            corpse.RemainingItems.RemoveAt(index);
            looted.Add(item);
        }

        corpse.IsLootedByNPC = true;
        corpse.LootingNPCType = npc.type;
        corpse.LootingNPCName = npc.GivenOrTypeName;

        // 发布事件
        EventBus.Publish(new NPCLootedPlayerEvent
        {
            NPCType = npc.type,
            TargetPlayerID = corpse.OwnerPlayerID,
            LootPosition = corpse.Center,
            LootedItemTypes = looted.Select(i => i.type).ToList()
        });

        // 死亡日志（发送给尸体原主人）
        Player owner = Main.player[corpse.OwnerPlayerID];
        if (owner.active && owner.whoAmI == Main.myPlayer)
        {
            string itemNames = string.Join(", ", looted.Select(i => i.Name));
            Main.NewText($"[死亡日志] 你的尸体被 {corpse.LootingNPCName} 搜索过，失去了：{itemNames}", Color.Orange);
        }
    }
}
```

---

## 5. 与 GuYuePatrolGuMaster 的衔接

在 `GuYuePatrolGuMaster` 中增加尸体感知：

```csharp
// 在 AI() 中，选择目标玩家后增加：
private PlayerCorpse FindNearbyCorpse(float radius)
{
    foreach (Projectile proj in Main.projectile)
    {
        if (proj.active && proj.ModProjectile is PlayerCorpse corpse)
        {
            if (Vector2.Distance(NPC.Center, corpse.Center) < radius)
                return corpse;
        }
    }
    return null;
}

// 在 ExecuteStrategy 中，增加对尸体的反应：
if (currentStrategy == Strategy.WaitAndSee || currentStrategy == Strategy.Test)
{
    var corpse = FindNearbyCorpse(300f);
    if (corpse != null && !corpse.IsLootedByNPC && Archetype.ReactionToCorpse == CorpseReaction.Loot)
    {
        // 走向尸体并搜尸
        MoveToDistance(corpse.Center, 50f, 3f);
        if (Vector2.Distance(NPC.Center, corpse.Center) < 60f)
        {
            NpcDeathHandler.Instance.NPCLootCorpse(NPC, corpse);
        }
    }
}
```

---

## 6. 验收标准

| 场景 | 操作 | 预期行为 | 通过标准 |
|------|------|---------|---------|
| 玩家死亡 | 被巡逻蛊师击杀 | 真元清空，蛊虫休眠，生成尸体，散落物品 | 地面上有尸体实体和掉落物 |
| NPC 搜尸 | 掠夺型 NPC 经过尸体 | 搜走 50% 剩余物品，标记已搜 | 尸体 `IsLootedByNPC == true`，聊天栏日志 |
| 死亡日志 | 被搜尸后 | 收到「你的尸体被 X 搜索过，失去了 Y」 | 聊天栏出现橙色文字 |
| 尸体腐烂 | 等待 5 分钟 | 剩余物品散落为原版掉落 | 尸体消失，地上有掉落物 |
| 悬赏发布 | 击杀家族 NPC | 家族发布悬赏，聊天栏提示 | `BountyPostedEvent` 发布，显示悬赏信息 |
| 媒介保留 | 死亡检查背包 | 蛊道媒介和空窍石仍在背包 | 媒介和空窍石未掉落 |
| 复仇夺回 | 击杀搜尸的 NPC | 可夺回被搜走的元石 | NPC 掉落其随身携带的元石 |

---

## 7. 风险与回退

| 风险 | 回退方案 |
|------|---------|
| 尸体实体太多导致性能问题 | 限制同屏尸体数量（最多 5 个），超出的直接散落物品 |
| NPC 不搜尸 | 在 `FindNearbyCorpse` 中降低距离阈值（300 → 500），或强制巡逻路径经过尸体 |
| 玩家找不到自己的尸体 | 死亡时在地图上标记尸体位置（仅自己可见的小地图图标） |
| 悬赏奖励过高导致经济膨胀 | 降低奖励公式（`lifeMax / 20`），或悬赏只能用元石领取 |

---

> 本文档由第三层实现工程师维护。  
> 实现过程中发现接口不匹配，立即回传 L2 登记修正。
