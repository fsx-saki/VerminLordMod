# L3 实现文档：P2 深度系统合集（Dialogue / 资源点 / 小世界 / 悬赏 / 防御 / 天劫 / 道痕 / 权力结构）

> 版本：v1.0（基于 L2-System-Interfaces-v1.2）  
> 优先级：P2 / 深度阶段  
> 预计工作量：每个系统 1-2 天，共 8-10 天  
> 依赖：P0/P1 全部系统已验证通过

---

## 1. 工程定位

P2 系统的共同特点：**MVA 阶段只做占位或最简实现**，在 P0/P1 核心循环验证通过后再填充。它们不阻塞 P0/P1，但决定了 v0.5-v1.0 的体验深度。

**实现顺序**：P0/P1 验收通过后，按以下顺序并行推进：
1. DialogueSystem（对话树）— 与 NpcBrain 紧密耦合
2. ResourceNodeSystem（元泉）— 独立，可早期并行
3. SubworldLibrary 接入 — 需要世界层配合
4. BountySystem（悬赏完整版）— 依赖 NpcDeathHandler
5. DefenseSystem（迷踪阵/守卫）— 独立
6. HeavenTribulationSystem（天劫）— 独立
7. DaoHenConflictSystem（道痕冲突）— 依赖 KongQiaoPlayer 预埋字段
8. PowerStructureSystem（权力结构硬编码）— 依赖 NpcDeathHandler 的 RoleVacancyEvent

---

## 2. DialogueSystem — 对话树与情报战

### 2.1 交付目标

MVA 阶段：劫持原版 `SetChatButtons`，扩展为三层菜单（公开/暗面/杀招）。
P1 阶段：独立对话栈，对话选择影响信念。

### 2.2 核心机制

```csharp
public class DialogueSystem : ModSystem
{
    /// <summary>
    /// 为指定 NPC 生成对话选项。
    /// 由 GuMasterBase.SetChatButtons 调用。
    /// </summary>
    public void GenerateDialogueOptions(Player player, NPC npc, ref string button, ref string button2)
    {
        var belief = (npc.ModNPC as GuMasterBase)?.GetBelief(player);
        if (belief == null) return;

        // 第一层：公开交互（始终可见）
        button = "询问";
        button2 = "交易";

        // 第二层：暗面操作（置信度 > 0.3 时解锁）
        // 第三层：杀招准备（私下相处 + 玩家有杀意标记时解锁）
        // MVA 阶段简化：只实现第一层，P1 再扩展
    }

    /// <summary>
    /// 处理对话选择，影响信念。
    /// </summary>
    public void OnDialogueChoice(Player player, NPC npc, int choiceIndex)
    {
        var guMaster = npc.ModNPC as GuMasterBase;
        if (guMaster == null) return;

        switch (choiceIndex)
        {
            case 0: // 诚实展示修为
                guMaster.UpdateBelief(player, new InteractionResult 
                { 
                    Type = InteractionType.HonestReveal,
                    ConfidenceDelta = 0.2f,
                    RiskThresholdDelta = -0.1f
                });
                break;
            case 1: // 虚报修为
                // MVA 简化：NPC 总是相信（后续按智商判定）
                guMaster.UpdateBelief(player, new InteractionResult
                {
                    Type = InteractionType.Deception,
                    ConfidenceDelta = 0.1f,
                    RiskThresholdDelta = -0.05f
                });
                break;
            case 2: // 拒绝回答
                guMaster.UpdateBelief(player, new InteractionResult
                {
                    Type = InteractionType.Refusal,
                    ConfidenceDelta = -0.05f,
                    RiskThresholdDelta = 0.1f // 更警惕
                });
                break;
        }
    }
}

public struct InteractionResult
{
    public InteractionType Type;
    public float ConfidenceDelta;
    public float RiskThresholdDelta;
}

public enum InteractionType
{
    HonestReveal,   // 诚实展示
    Deception,      // 虚报
    Refusal,        // 拒绝回答
    Threat,         // 威胁
    Bribe,          // 贿赂
    AllianceRequest // 请求结盟
}
```

---

## 3. ResourceNodeSystem — 有限资源点

### 3.1 交付目标

MVA 阶段：实现 1 个资源点「元泉」，每周刷新少量元石，NPC 与玩家竞争。

### 3.2 核心机制

```csharp
public class ResourceNodeSystem : ModSystem
{
    public class ResourceNode
    {
        public Vector2 Position;
        public ResourceType Type;
        public int CurrentAmount;       // 当前储量
        public int MaxAmount;           // 最大储量
        public int RegenTimer;          // 恢复计时器
        public FactionID ControllingFaction; // 控制势力
        public bool IsDepleted => CurrentAmount <= 0;
    }

    public List<ResourceNode> ActiveNodes = new();

    public override void PreUpdateWorld()
    {
        foreach (var node in ActiveNodes)
        {
            if (node.IsDepleted)
            {
                node.RegenTimer++;
                if (node.RegenTimer > 42000) // 7 天 = 42000 帧
                {
                    node.CurrentAmount = node.MaxAmount / 2; // 恢复一半
                    node.RegenTimer = 0;

                    EventBus.Publish(new ResourceDepletedEvent
                    {
                        Type = node.Type,
                        Position = node.Position,
                        ControllingFaction = node.ControllingFaction
                    });
                }
            }
        }
    }

    /// <summary>
    /// 玩家采集资源点。
    /// </summary>
    public bool Harvest(Player player, ResourceNode node, int amount)
    {
        if (node.IsDepleted) return false;

        int actual = Math.Min(amount, node.CurrentAmount);
        node.CurrentAmount -= actual;

        // 给予玩家元石
        for (int i = 0; i < actual; i++)
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<YuanStone>(), 1);

        if (node.IsDepleted)
        {
            Main.NewText("元泉枯竭了。需要等待一段时间才能恢复。", Color.Gray);
        }

        return true;
    }
}

public enum ResourceType
{
    YuanSpring,     // 元泉
    GuVein,         // 蛊虫矿脉（P3）
    HerbGarden,     // 药园（P3）
    SoulStoneMine   // 魂石矿（P3）
}
```

---

## 4. SubworldLibrary 接入 — 驻地与小世界

### 4.1 交付目标

MVA 阶段：实现古月山寨驻地（大世界生物群系）+ 古月族地（1 个小世界）。
P1 阶段：扩展到 3-5 个家族。

### 4.2 核心机制

```csharp
public class GuYueTerritory : Subworld
{
    public override int Width => 400;
    public override int Height => 300;

    public override void Load()
    {
        // 生成古月族地地形：青砖建筑、训练场、议事厅
        // MVA 阶段简化：平坦地形 + 预设建筑
    }

    public override void OnEnter()
    {
        Main.NewText("进入古月族地。此处名义安全，禁止公开战斗。", Color.Cyan);
    }

    public override void OnExit()
    {
        Main.NewText("离开古月族地。", Color.Gray);
    }
}

public class SubworldPortal : ModTile
{
    // 驻地入口：大世界中的传送门/寨门
    public override void RightClick(int i, int j)
    {
        Player player = Main.LocalPlayer;

        // 检查进入条件：声望 >= 中立，或强行闯入（被攻击）
        var rep = player.GetModPlayer<GuWorldPlayer>().GetReputation(FactionID.GuYue);
        if (rep < -20)
        {
            Main.NewText("古月守卫拒绝你进入。", Color.Red);
            return;
        }

        SubworldSystem.Enter<GuYueTerritory>();
    }
}
```

---

## 5. BountySystem — 悬赏完整版

### 5.1 交付目标

MVA 阶段：订阅 `BountyPostedEvent`，维护活跃悬赏列表。
P1 阶段：悬赏面板、领取、结算、NPC 领取悬赏追杀玩家。

### 5.2 核心机制

```csharp
public class BountySystem : ModSystem
{
    public List<Bounty> ActiveBounties = new();

    public void OnBountyPosted(BountyPostedEvent evt)
    {
        ActiveBounties.Add(new Bounty
        {
            BountyID = evt.BountyID,
            PostingFaction = evt.PostingFaction,
            TargetPlayerID = evt.TargetPlayerID,
            RewardAmount = evt.RewardAmount,
            Reason = evt.Reason,
            IsActive = true,
            PostedTick = Main.GameUpdateCount
        });

        // NPC 可领取悬赏（MVA 简化：散修自动领取）
        if (evt.TargetPlayerID >= 0)
        {
            // 生成追杀 NPC（P1 实现）
        }
    }

    public bool ClaimBounty(Player player, int bountyID)
    {
        var bounty = ActiveBounties.FirstOrDefault(b => b.BountyID == bountyID);
        if (bounty == null || !bounty.IsActive) return false;

        // 检查目标是否已被击杀
        Player target = Main.player[bounty.TargetPlayerID];
        if (!target.active) // 目标不在线或已死亡
        {
            // 给予奖励
            player.GetModPlayer<QiResourcePlayer>().RefundQi(bounty.RewardAmount);
            bounty.IsActive = false;
            return true;
        }
        return false;
    }
}

public class Bounty
{
    public int BountyID;
    public FactionID PostingFaction;
    public int TargetPlayerID;
    public int RewardAmount;
    public BountyReason Reason;
    public bool IsActive;
    public int PostedTick;
}
```

---

## 6. DefenseSystem — 迷踪阵与守卫

### 6.1 交付目标

MVA 阶段：实现「迷踪阵」Tile，降低 NPC 感知精度；「守卫」NPC，提高风险阈值。

### 6.2 核心机制

```csharp
public class MiZongZhenTile : ModTile
{
    // 迷踪阵：消耗元石运转，降低 NPC 对玩家位置的感知精度
    public override void PlaceInWorld(int i, int j, Item item)
    {
        // 放置时绑定玩家
        Main.NewText("迷踪阵已布置。附近的 NPC 将难以感知你的精确位置。", Color.Cyan);
    }

    public override void NearbyEffects(int i, int j, Player player)
    {
        // 范围内玩家获得 Buff：NPC 感知距离 -50%
        player.AddBuff(ModContent.BuffType<MiZongBuff>(), 2);
    }
}

public class MiZongBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeLoss[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        // 标记玩家处于迷踪阵中
        player.GetModPlayer<DefensePlayer>().IsInMiZong = true;
    }
}

public class DefensePlayer : ModPlayer
{
    public bool IsInMiZong;
    public int GuardCount;  // 雇佣的守卫数量

    public override void PostUpdate()
    {
        // 守卫提高 NPC 风险阈值
        if (GuardCount > 0)
        {
            // 通过事件广播给附近 NPC
            // P1 实现
        }
    }
}
```

---

## 7. HeavenTribulationSystem — 天劫

### 7.1 交付目标

MVA 阶段：实现天劫预警和规避框架，不触发实际伤害（P2 再填充）。

### 7.2 核心机制

```csharp
public class HeavenTribulationSystem : ModSystem
{
    public Dictionary<int, TribulationState> PlayerTribulations = new(); // Key = Player.whoAmI

    public class TribulationState
    {
        public int PlayerID;
        public int NextTribulationTick;     // 下次天劫帧数
        public TribulationType Type;        // 雷/火/心魔
        public int WarningTicks;            // 预警期（提前 3 天 = 18000 帧）
        public bool IsActive;               // 是否正在进行
    }

    public override void PostUpdateWorld()
    {
        foreach (var state in PlayerTribulations.Values)
        {
            if (!state.IsActive && Main.GameUpdateCount >= state.NextTribulationTick - state.WarningTicks)
            {
                // 预警期开始
                Player player = Main.player[state.PlayerID];
                if (player.active && player.whoAmI == Main.myPlayer)
                {
                    int daysRemaining = (state.NextTribulationTick - Main.GameUpdateCount) / 6000;
                    Main.NewText($"天道预警：{daysRemaining} 天后将遭遇 {state.Type} 劫！", Color.Purple);
                }
            }

            if (!state.IsActive && Main.GameUpdateCount >= state.NextTribulationTick)
            {
                // 天劫降临
                TriggerTribulation(state);
            }
        }
    }

    private void TriggerTribulation(TribulationState state)
    {
        state.IsActive = true;
        Player player = Main.player[state.PlayerID];

        // MVA 阶段：只播放特效和提示，不造成伤害
        // P2 再实现实际伤害和规避判定
        Main.NewText($"天劫降临！{state.Type} 劫正在形成...", Color.Purple);

        // 规避检查：玩家是否在安全区/福地/使用了避雷蛊
        bool isEvading = CheckEvading(player);
        if (isEvading)
        {
            Main.NewText("你成功规避了天劫。", Color.Green);
            state.IsActive = false;
            ScheduleNextTribulation(state);
        }
        else
        {
            // P2：造成伤害
            // player.Hurt(...)
        }
    }

    private bool CheckEvading(Player player)
    {
        // MVA 简化：只要玩家在出生点附近就视为规避
        return Vector2.Distance(player.Center, player.Spawn()) < 400f;
    }

    private void ScheduleNextTribulation(TribulationState state)
    {
        // 二转以上每 15 天一次
        state.NextTribulationTick = Main.GameUpdateCount + 90000;
        state.Type = (TribulationType)Main.rand.Next(3);
    }
}

public enum TribulationType { Lightning, Fire, HeartDemon }
```

---

## 8. DaoHenConflictSystem — 道痕冲突

### 8.1 交付目标

MVA 阶段：不实现冲突检测，只确认 `KongQiaoSlot.DaoHenTags` 预埋字段已填充默认值。
P2 阶段：装备时生成 `ConflictMask`，使用时查询。

### 8.2 核心机制（P2 占位）

```csharp
public interface IDaoHenConflictSystem
{
    ConflictMask CalculateConflictMask(List<KongQiaoSlot> activeGus);
    bool HasConflict(ConflictMask mask, DaoPath pathA, DaoPath pathB);
}

public struct ConflictMask
{
    public ulong Mask;  // 位掩码
}

public enum DaoPath
{
    None = 0,
    Fire = 1,     // 炎道
    Ice = 2,      // 冰道
    Force = 3,    // 力道
    Wind = 4,     // 风道
    Blood = 5,    // 血道
    Wisdom = 6,   // 智道
    Moon = 7      // 月道
}

// MVA 阶段：在 KongQiaoPlayer.TryRefineGu 中填充默认值
// 月光蛊 = Moon，酒虫 = None，骨枪蛊 = Force， etc.
```

---

## 9. PowerStructureSystem — 权力结构（硬编码继承）

### 9.1 交付目标

MVA 阶段：职务 NPC 死亡后，按硬编码继承顺位自动补位，不开放玩家竞选。
P2 阶段：完整竞选系统。

### 9.2 核心机制

```csharp
public class PowerStructureSystem : ModSystem
{
    // 硬编码继承顺位表
    public static readonly Dictionary<FactionID, Dictionary<FactionRole, List<int>>> SuccessionChains = new()
    {
        [FactionID.GuYue] = new Dictionary<FactionRole, List<int>>
        {
            [FactionRole.MedicineElder] = new List<int> { NPCID.Count + 1, NPCID.Count + 2 }, // 弟子列表
            [FactionRole.LearningElder] = new List<int> { NPCID.Count + 3 },
            [FactionRole.PatrolCaptain] = new List<int> { NPCID.Count + 4 }
        }
        // 其他家族 P1 再填充
    };

    public void OnRoleVacancy(RoleVacancyEvent evt)
    {
        if (evt.SuccessorNPCType > 0)
        {
            // 自动生成继任者 NPC
            // MVA 简化：聊天栏提示「X 接替了 Y 的职务」
            Main.NewText($"{evt.Faction} 的 {evt.VacatedRole} 由继任者接任。", Color.Yellow);
        }
        else
        {
            // 无继任者：家族功能停摆
            Main.NewText($"{evt.Faction} 的 {evt.VacatedRole} 空缺，相关功能暂时不可用。", Color.Gray);
        }
    }
}
```

---

## 10. 验收标准（P2 合集）

| 系统 | 验收场景 | 通过标准 |
|------|---------|---------|
| DialogueSystem | 与巡逻蛊师对话选择「诚实展示」 | RiskThreshold 下降，Confidence 上升 |
| ResourceNodeSystem | 采集元泉至枯竭 | 元石获得，元泉显示枯竭，7 天后恢复一半 |
| SubworldLibrary | 进入古月族地 | 加载小世界，显示提示，地形不同 |
| BountySystem | 查看悬赏列表 | 显示至少 1 条活跃悬赏 |
| DefenseSystem | 布置迷踪阵 | NPC 感知距离缩短，调试输出验证 |
| HeavenTribulation | 等待天劫预警 | 3 天前收到紫色预警文字 |
| DaoHenConflict | 炼化两只冲突道痕蛊虫 | DaoHenTags 字段非零，P2 后检测冲突 |
| PowerStructure | 击杀学堂家老 | 聊天栏显示继任者接任或空缺提示 |

---

> 本文档由第三层实现工程师维护。  
> 所有 P2 系统在 P0/P1 验收通过前**禁止开始实现**，避免范围膨胀。
