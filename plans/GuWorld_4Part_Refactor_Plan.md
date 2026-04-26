# 古月蛊师 4 部分重构计划

## 概述

基于用户最新反馈，需要对现有古月巡逻蛊师系统进行 4 个方面的重构。以下是详细分析和实现方案。

---

## Part 1：弹幕保护系统（Projectile Protection System）

### 问题
玩家飞行的弹幕容易误伤古月巡逻蛊师，导致不必要的敌对。

### 需求
1. **默认状态**：玩家的弹幕对蛊师 NPC 友善（不造成伤害）
2. **交互切换**：玩家与 NPC 对话时，可以点击按钮"去除保护"——此时 NPC 不会觉察
3. **无保护攻击**：在无保护状态下攻击 NPC 后，NPC 弹出震惊感叹号，然后根据逻辑判断战斗/逃跑/呼叫支援
4. **重新交互**：再次交互可重新开启保护

### 实现方案

#### 涉及文件
| 文件 | 修改类型 |
|------|----------|
| [`Content/NPCs/GuMasters/GuMasterBase.cs`](Content/NPCs/GuMasters/GuMasterBase.cs) | 修改 |
| [`Content/NPCs/GuMasters/IGuMasterAI.cs`](Content/NPCs/GuMasters/IGuMasterAI.cs) | 修改 |
| [`Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs`](Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs) | 修改 |

#### 详细设计

**1.1 新增字段（GuMasterBase）**

```csharp
// ===== 弹幕保护系统 =====
/// <summary> 玩家是否对此NPC开启了弹幕保护（默认开启） </summary>
public bool ProjectileProtectionEnabled = true;

/// <summary> 玩家是否已去除保护（NPC不知情） </summary>
public bool ProtectionRemovedByPlayer = false;

/// <summary> 保护状态切换的冷却（防止频繁切换） </summary>
public int ProtectionToggleCooldown = 0;
```

**1.2 修改 ModifyHitByProjectile（GuMasterBase）**

```csharp
public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
{
    if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
    {
        var player = Main.player[projectile.owner];
        if (player.active)
        {
            // 弹幕保护检查
            if (ProjectileProtectionEnabled)
            {
                // 保护开启：弹幕不造成伤害
                modifiers.SetMaxDamage(0);  // 或 modifiers.DisableDamage();
                return;
            }
            
            // 保护已关闭：正常处理攻击
            HandleInteraction(NPC, player, InteractionType.Attack);
        }
    }
}
```

**1.3 震惊反应（GuMasterBase 新增方法）**

当玩家在无保护状态下攻击 NPC 时，在 `HandleInteraction(Attack)` 中触发：

```csharp
case InteractionType.Attack:
    if (!ProjectileProtectionEnabled)
    {
        // 弹出震惊感叹号
        SpawnShockEffect(NPC.Center);
        
        // 根据信念状态决定反应
        var belief = GetBelief(player.name);
        if (belief.RiskThreshold < 0.4f)
        {
            // 自信：反击
            HasBeenHitByPlayer = true;
            AggroTimer = 600;
            result.TriggerCombat = true;
            result.Message = $"{GuMasterDisplayName}震惊地拔出武器！";
        }
        else if (belief.RiskThreshold < 0.7f)
        {
            // 中等：呼叫支援
            HasBeenHitByPlayer = true;
            AggroTimer = 600;
            decision.ShouldCallForHelp = true;
            result.Message = $"{GuMasterDisplayName}震惊地放出信号弹！";
        }
        else
        {
            // 恐惧：逃跑
            CurrentAIState = GuMasterAIState.Flee;
            result.Message = $"{GuMasterDisplayName}惊恐地逃开了！";
        }
    }
    else
    {
        // 有保护时不应该走到这里，但做防御
    }
    break;
```

**1.4 交互按钮切换保护（GuMasterBase.SetChatButtons）**

```csharp
public override void SetChatButtons(ref string button, ref string button2)
{
    button = "对话";
    if (CurrentAttitude != GuAttitude.Hostile && CurrentAttitude != GuAttitude.Wary)
    {
        // 根据当前保护状态显示不同按钮
        if (ProjectileProtectionEnabled)
            button2 = "去除保护";
        else
            button2 = "开启保护";
    }
}
```

**1.5 处理按钮点击（GuMasterBase.OnChatButtonClicked）**

```csharp
public override void OnChatButtonClicked(bool firstButton, ref string shop)
{
    if (firstButton)
    {
        Main.npcChatText = GetDialogue(NPC, CurrentAttitude);
    }
    else
    {
        // 切换保护状态
        ProjectileProtectionEnabled = !ProjectileProtectionEnabled;
        if (ProjectileProtectionEnabled)
        {
            Main.npcChatText = "你重新开启了弹幕保护。";
        }
        else
        {
            Main.npcChatText = "你悄悄去除了弹幕保护。";
        }
    }
}
```

**1.6 震惊特效（GuMasterBase 新增方法）**

```csharp
/// <summary> 在NPC位置生成震惊感叹号特效 </summary>
protected void SpawnShockEffect(Vector2 position)
{
    // 使用 Dust 或 Gore 模拟感叹号效果
    for (int i = 0; i < 10; i++)
    {
        Dust.NewDust(position, 10, 10, DustID.Torch, 
            Main.rand.NextFloat(-3f, 3f), 
            Main.rand.NextFloat(-3f, 3f), 
            100, Color.Yellow, 1.5f);
    }
    // 显示感叹号文本
    CombatText.NewText(NPC.getRect(), Color.Yellow, "！", true);
}
```

**1.7 呼叫支援（GuYuePatrolGuMaster.ExecuteAI）**

在 `CallForHelp` 状态中，生成额外的巡逻蛊师：

```csharp
case GuMasterAIState.CallForHelp:
    // 生成2-3个友方NPC
    for (int i = 0; i < Main.rand.Next(2, 4); i++)
    {
        int npcType = ModContent.NPCType<GuYuePatrolGuMaster>();
        Vector2 spawnPos = NPC.Center + new Vector2(Main.rand.Next(-200, 200), -50);
        int newNPC = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, npcType);
        if (newNPC >= 0 && newNPC < Main.maxNPCs)
        {
            var patrolNPC = Main.npc[newNPC].ModNPC as GuYuePatrolGuMaster;
            if (patrolNPC != null)
            {
                patrolNPC.HasBeenHitByPlayer = true;
                patrolNPC.AggroTimer = 1200;
            }
        }
    }
    CurrentAIState = GuMasterAIState.Combat;
    break;
```

---

## Part 2：声望修复 + 目击者系统 + 声望 UI

### 问题
1. 怪物杀死蛊师后错误扣除玩家声望
2. 秘密击杀（无目击者）不应扣除声望
3. 需要 UI 展示各势力声望

### 实现方案

#### 涉及文件
| 文件 | 修改类型 |
|------|----------|
| [`Content/NPCs/GuMasters/GuMasterBase.cs`](Content/NPCs/GuMasters/GuMasterBase.cs) | 修改 |
| [`Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs`](Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs) | 修改 |
| [`Common/Players/GuWorldPlayer.cs`](Common/Players/GuWorldPlayer.cs) | 修改 |
| [`Common/UI/ReputationUI/ReputationUI.cs`](Common/UI/ReputationUI/ReputationUI.cs) | 新建 |
| [`Common/UI/ReputationUI/ReputationUISystem.cs`](Common/UI/ReputationUI/ReputationUISystem.cs) | 新建 |

#### 2.1 修复怪物击杀不扣声望

**问题根因**：`GuMasterBase.OnKill()` 无条件调用 `worldPlayer.RemoveReputation()`，无论击杀者是谁。

**修复方案**：在 `OnKill()` 中检查击杀者身份。

```csharp
public override void OnKill()
{
    // 检查击杀者是否是玩家
    int? killerPlayer = WhoKilledMe();
    if (killerPlayer.HasValue)
    {
        var player = Main.player[killerPlayer.Value];
        if (player.active)
        {
            var worldPlayer = player.GetModPlayer<GuWorldPlayer>();
            
            // 检查是否有目击者（同家族NPC在附近）
            if (HasWitnesses(400f))
            {
                worldPlayer.AddInfamy(10);
                worldPlayer.RemoveReputation(GetFaction(), 20, "击杀成员");
                Main.NewText($"你击杀了{GuMasterDisplayName}！{GuWorldSystem.GetFactionDisplayName(GetFaction())}声望下降。", Color.OrangeRed);
            }
            else
            {
                // 无目击者：秘密击杀，不扣声望
                Main.NewText($"你秘密击杀了{GuMasterDisplayName}，无人知晓。", Color.Gray);
            }
        }
    }
    // 非玩家击杀（怪物等）→ 不扣玩家声望
}
```

**辅助方法**：

```csharp
/// <summary> 检查是否有同家族NPC在附近（目击者） </summary>
protected bool HasWitnesses(float range)
{
    for (int i = 0; i < Main.maxNPCs; i++)
    {
        var other = Main.npc[i];
        if (other.active && other.whoAmI != NPC.whoAmI && other.ModNPC is GuMasterBase guMaster)
        {
            if (guMaster.GetFaction() == GetFaction() && 
                Vector2.Distance(NPC.Center, other.Center) < range)
            {
                return true;
            }
        }
    }
    return false;
}

/// <summary> 获取击杀此NPC的玩家ID（如果有） </summary>
private int? WhoKilledMe()
{
    // 方法1：检查 lastInteraction
    // 方法2：检查 NPC.lastInteraction（tML 提供）
    if (NPC.lastInteraction >= 0 && NPC.lastInteraction < Main.maxPlayers)
    {
        var player = Main.player[NPC.lastInteraction];
        if (player.active)
            return NPC.lastInteraction;
    }
    return null;
}
```

#### 2.2 修复 GuYuePatrolGuMaster.OnKill()

同样需要修改子类的 `OnKill()`：

```csharp
public override void OnKill()
{
    // 更新信念
    var player = Main.LocalPlayer;
    var belief = GetBelief(player.name);
    belief.WasDefeated = true;
    belief.RiskThreshold = MathHelper.Min(1f, belief.RiskThreshold + 0.25f);

    // 检查击杀者
    int? killerPlayer = WhoKilledMe();
    if (killerPlayer.HasValue)
    {
        var killer = Main.player[killerPlayer.Value];
        var worldPlayer = killer.GetModPlayer<GuWorldPlayer>();
        
        // 有目击者才扣声望
        if (HasWitnesses(400f))
        {
            worldPlayer.AddInfamy(15);
            worldPlayer.RemoveReputation(FactionID.GuYue, 30, "击杀巡逻蛊师");
            Main.NewText("你击杀了一名古月巡逻蛊师！古月家族声望下降。", Color.OrangeRed);
        }
        else
        {
            Main.NewText("你秘密击杀了一名古月巡逻蛊师，无人知晓。", Color.Gray);
        }
    }
}
```

#### 2.3 声望 UI

**设计**：一个可展开/折叠的面板，显示所有已知势力的声望点数。

**UI 结构**：

```
┌─────────────────────────────────┐
│  势力声望 [展开/折叠] ▲         │  ← 点击标题栏展开/折叠
├─────────────────────────────────┤
│  古月家族:  友善    (+120)      │
│  白家:      中立    (0)         │
│  熊家:      不友好  (-50)       │
│  铁家:      中立    (10)        │
│  ...                            │
│  恶名值:    15                  │
└─────────────────────────────────┘
```

**实现**（参考 [`Common/UI/DaosUI/DaosUI.cs`](Common/UI/DaosUI/DaosUI.cs) 的模式）：

```csharp
// ReputationUI.cs
public class ReputationUI : UIState
{
    private UIPanel _mainPanel;
    private UIText _titleText;
    private UIImageButton _toggleButton;
    private UIList _factionList;
    private bool _isExpanded = false;
    
    public override void OnInitialize()
    {
        // 主面板 - 固定在屏幕右上角
        _mainPanel = new UIPanel();
        _mainPanel.Width.Set(280f, 0f);
        _mainPanel.Height.Set(40f, 0f);  // 折叠时只显示标题
        _mainPanel.Left.Set(Main.screenWidth - 300f, 0f);
        _mainPanel.Top.Set(100f, 0f);
        _mainPanel.BackgroundColor = new Color(30, 30, 50, 200);
        Append(_mainPanel);
        
        // 标题
        _titleText = new UIText("势力声望 [点击展开]", 0.9f);
        _titleText.Left.Set(10f, 0f);
        _titleText.Top.Set(10f, 0f);
        _titleText.OnLeftClick += (evt, listener) => ToggleExpand();
        _mainPanel.Append(_titleText);
        
        // 声望列表（初始隐藏）
        _factionList = new UIList();
        _factionList.Left.Set(10f, 0f);
        _factionList.Top.Set(40f, 0f);
        _factionList.Width.Set(260f, 0f);
        _factionList.Height.Set(200f, 0f);
        _factionList.OverflowHidden = true;
        _mainPanel.Append(_factionList);
    }
    
    private void ToggleExpand()
    {
        _isExpanded = !_isExpanded;
        _mainPanel.Height.Set(_isExpanded ? 260f : 40f, 0f);
        _titleText.SetText(_isExpanded ? "势力声望 [点击折叠]" : "势力声望 [点击展开]");
        Recalculate();
    }
    
    public void RefreshFactionList()
    {
        _factionList.Clear();
        var worldPlayer = Main.LocalPlayer.GetModPlayer<GuWorldPlayer>();
        
        foreach (var (fid, rel) in worldPlayer.FactionRelations)
        {
            string displayName = GuWorldSystem.GetFactionDisplayName(fid);
            string levelName = GetRepLevelName(rel.GetLevel());
            string points = rel.ReputationPoints >= 0 ? $"+{rel.ReputationPoints}" : $"{rel.ReputationPoints}";
            string text = $"{displayName}:  {levelName}  ({points})";
            
            var entry = new UIText(text, 0.8f);
            entry.Left.Set(5f, 0f);
            entry.Top.Set(5f, 0f);
            _factionList.Add(entry);
        }
        
        // 恶名值
        var infamyText = new UIText($"恶名值: {worldPlayer.InfamyPoints}", 0.8f);
        infamyText.TextColor = Color.Gray;
        _factionList.Add(infamyText);
    }
}
```

```csharp
// ReputationUISystem.cs
[Autoload(Side = ModSide.Client)]
public class ReputationUISystem : ModSystem
{
    private UserInterface _reputationUI;
    internal ReputationUI ReputationUIInstance;
    
    public override void Load()
    {
        if (!Main.dedServ)
        {
            _reputationUI = new UserInterface();
            ReputationUIInstance = new ReputationUI();
            ReputationUIInstance.Activate();
        }
    }
    
    public override void UpdateUI(GameTime gameTime)
    {
        _reputationUI?.Update(gameTime);
    }
    
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "VerminLordMod: Reputation UI",
                () => {
                    if (_reputationUI?.CurrentState != null)
                    {
                        _reputationUI.Draw(Main.spriteBatch, new GameTime());
                    }
                    return true;
                },
                InterfaceScaleType.UI
            ));
        }
    }
    
    public void ToggleUI()
    {
        if (_reputationUI.CurrentState == null)
        {
            ReputationUIInstance.RefreshFactionList();
            _reputationUI.SetState(ReputationUIInstance);
        }
        else
        {
            _reputationUI.SetState(null);
        }
    }
}
```

**打开 UI 的方式**：通过快捷键或物品。建议在 [`GuWorldPlayer.PreUpdate()`](Common/Players/GuWorldPlayer.cs:365) 中注册一个快捷键检测，或者绑定到某个物品的右键功能。

---

## Part 3：修复 NPC 被攻击后一直向左跑

### 问题分析

用户反馈"被我攻击后 NPC 依旧一心往左跑"。尽管之前添加了 RiskThreshold 上限 0.5f 的修复，但问题仍然存在。

### 根因分析

查看 [`GuMasterBase.AI()`](Content/NPCs/GuMasters/GuMasterBase.cs:109) 的流程：

1. `Perceive()` → 获取感知上下文
2. `UpdateBelief()` → 更新信念
3. `GetBelief()` → 获取信念
4. `CalculateAttitude()` → 调用 `CalculateFromBelief()`
5. `Decide()` → 决策
6. `ExecuteAI()` → 执行

**关键问题**：`CalculateFromBelief()` 中，当 `hasBeenHit = true` 时返回 `Hostile`。`Decide()` 中 `Hostile` 在距离 < 500f 时进入 `Combat` 状态。但 `ExecuteCombatAI()` 在 [`GuYuePatrolGuMaster`](Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs:232) 中被重写了。

**真正的问题可能在于**：

1. **`AggroTimer` 重置逻辑**：在 [`GuMasterBase.AI()`](Content/NPCs/GuMasters/GuMasterBase.cs:141-142) 中：
   ```csharp
   if (AggroTimer > 0) AggroTimer--;
   if (AggroTimer == 0) HasBeenHitByPlayer = false;
   ```
   当 `AggroTimer` 减到 0 时，`HasBeenHitByPlayer` 被重置为 `false`，然后 `CalculateFromBelief()` 不再返回 `Hostile`，而是根据 `RiskThreshold` 计算。此时如果 `RiskThreshold` 被 `UpdateBeliefState()` 推高到 > 0.7，就会进入 `Fearful` 状态 → `Flee` → 向左跑。

2. **`UpdateBeliefState()` 的平滑更新**：即使 `GuYuePatrolGuMaster.UpdateBelief()` 设置了上限 0.5f，但 `UpdateBeliefState()` 的平滑公式 `belief.RiskThreshold = belief.RiskThreshold * 0.8f + targetThreshold * 0.2f` 每帧都在计算。如果 `targetThreshold` 很高（因为 `EstimatedPower` 低），阈值仍然可能被推高。

3. **`AggroTimer` 初始值**：`HandleInteraction(Attack)` 设置 `AggroTimer = 600`（10秒）。10秒后 `HasBeenHitByPlayer` 被重置，NPC 回到信念驱动状态。

### 修复方案

**3.1 延长 AggroTimer（GuMasterBase）**

```csharp
case InteractionType.Attack:
    HasBeenHitByPlayer = true;
    AggroTimer = 1800; // 从 600 改为 1800（30秒），给玩家足够时间
    ...
```

**3.2 攻击后强制锁定 Combat 状态（GuMasterBase.Decide）**

在 `Decide()` 中，如果 `HasBeenHitByPlayer == true`，强制进入 Combat：

```csharp
public virtual Decision Decide(NPC npc, PerceptionContext context)
{
    var decision = new Decision { NewState = CurrentAIState };

    // 如果被玩家攻击过且仇恨未消，强制战斗
    if (HasBeenHitByPlayer && AggroTimer > 0)
    {
        decision.NewState = GuMasterAIState.Combat;
        decision.ShouldAttack = true;
        return decision;
    }

    // 原有态度驱动决策...
    switch (CurrentAttitude)
    {
        ...
    }
    return decision;
}
```

**3.3 修复 GuYuePatrolGuMaster.ExecuteCombatAI 中的后退逻辑**

查看 [`GuYuePatrolGuMaster.ExecuteCombatAI()`](Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs:260-268) 的谨慎模式：

```csharp
if (dist < 120f)
{
    // 太近则后退
    float fleeDir = npc.Center.X > target.Center.X ? 1 : -1;
    npc.velocity.X = fleeDir * 2.5f;
}
```

这里的 `fleeDir` 计算：如果 NPC 在玩家右边（`Center.X > target.Center.X`），`fleeDir = 1`（向右跑）。如果 NPC 在玩家左边，`fleeDir = -1`（向左跑）。**这本身是正确的**——NPC 应该远离玩家。

但问题在于：当 `RiskThreshold < 0.4f` 时进入自信模式（近战追击），当 `RiskThreshold >= 0.4f` 时进入谨慎模式（远程游走）。由于 `GuYuePatrolGuMaster.UpdateBelief()` 设置了上限 0.5f，所以 NPC 始终处于谨慎模式，**始终在后退**。

**修复**：降低自信模式的阈值，让 NPC 在被攻击后更容易进入近战追击模式。

```csharp
// 在 GuYuePatrolGuMaster.UpdateBelief() 中，被攻击后强制降低 RiskThreshold
if (HasBeenHitByPlayer)
{
    belief.RiskThreshold = MathHelper.Min(belief.RiskThreshold, 0.3f);
}
```

**3.4 综合修复方案**

在 [`GuMasterBase.AI()`](Content/NPCs/GuMasters/GuMasterBase.cs:109) 中增加攻击后的行为锁定：

```csharp
public override void AI()
{
    // 1. 感知
    var context = Perceive(NPC);

    // 2. 更新信念
    UpdateBelief(NPC, context);

    // 3. 获取信念
    string playerName = Main.LocalPlayer.name;
    var belief = GetBelief(playerName);

    // 4. 计算态度
    var attCtx = new AttitudeContext { ... };
    CurrentAttitude = CalculateAttitude(NPC, attCtx);

    // 5. 决策
    var decision = Decide(NPC, context);

    // 6. 执行
    ExecuteAI(NPC, decision);

    // 7. 更新计时器
    if (AggroTimer > 0) AggroTimer--;
    // 注意：不再自动重置 HasBeenHitByPlayer！
    // 改为由子类或特定条件重置
}
```

**移除自动重置**，改为在 `AggroTimer == 0` 时只触发一次状态检查：

```csharp
if (AggroTimer > 0)
{
    AggroTimer--;
    if (AggroTimer == 0)
    {
        // 仇恨消退，但保留标记用于信念系统
        // HasBeenHitByPlayer 不再自动重置
        // 而是由信念系统自然过渡
    }
}
```

---

## Part 4：势力声望与个体信念分离

### 问题
用户明确指出：势力声望和个体 NPC 的行为是两个独立系统。势力声望只在势力层面（悬赏、商店价格等）起作用，个体 NPC 只用信念来判断玩家。

### 当前问题
查看 [`GuAttitudeHelper.CalculateFromBelief()`](Content/NPCs/GuMasters/IGuMasterAI.cs:186)：

```csharp
public static GuAttitude CalculateFromBelief(BeliefState belief, GuPersonality personality, bool hasBeenHit)
{
    if (hasBeenHit) return GuAttitude.Hostile;
    // ... 只使用 belief + personality，没有使用 faction reputation ✓
}
```

**好消息**：当前 `CalculateFromBelief()` 已经**没有使用** faction reputation。这是正确的。

**但需要检查的地方**：

1. [`GuMasterBase.OnKill()`](Content/NPCs/GuMasters/GuMasterBase.cs:511) — 无条件调用 `RemoveReputation()`，需要改为目击者检查（Part 2 已处理）
2. [`GuMasterBase.HandleInteraction(Attack)`](Content/NPCs/GuMasters/GuMasterBase.cs:392) — 调用 `AddInfamy()`，这是合理的（恶名影响所有家族）
3. [`GuYuePatrolGuMaster.OnKill()`](Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs:367) — 同样需要目击者检查

### 架构澄清

```
┌─────────────────────────────────────────────────────────────┐
│                    势力声望系统 (FactionReputation)           │
│  GuWorldPlayer.FactionRelations                             │
│  ├─ 商店价格影响                                            │
│  ├─ 悬赏发布/撤销                                           │
│  ├─ 结盟/背刺                                              │
│  └─ 势力间连锁反应                                          │
│  作用范围: 势力层面                                          │
└─────────────────────────────────────────────────────────────┘
                           ↑ 独立
┌─────────────────────────────────────────────────────────────┐
│                  个体信念系统 (BeliefState)                   │
│  GuMasterBase.PlayerBeliefs[playerName]                     │
│  ├─ RiskThreshold: 风险阈值（越低越激进）                     │
│  ├─ ConfidenceLevel: 置信度                                  │
│  ├─ EstimatedPower: 对玩家实力的估计                          │
│  ├─ WasDefeated / HasDefeatedPlayer                         │
│  └─ HasTraded / HasFought                                   │
│  作用范围: 单个NPC对单个玩家的判断                             │
│  决定: 攻击/逃跑/观望/对话                                    │
└─────────────────────────────────────────────────────────────┘
```

### 需要修改的地方

**4.1 确保 CalculateFromBelief 不使用 faction reputation**

当前代码已经正确，不需要修改。但需要添加注释明确说明。

**4.2 确保悬赏不影响个体 NPC 行为**

即使玩家被古月家族悬赏，个体古月巡逻蛊师仍然使用自己的 `BeliefState` 来判断。悬赏只在势力层面起作用（例如：发布悬赏的家族商店关闭、家族成员在特定事件中集体行动等）。

**4.3 添加文档注释**

在 [`IGuMasterAI.cs`](Content/NPCs/GuMasters/IGuMasterAI.cs) 和 [`GuWorldPlayer.cs`](Common/Players/GuWorldPlayer.cs) 中添加架构说明注释。

---

## 文件修改汇总

| # | 文件 | 操作 | 涉及 Part |
|---|------|------|-----------|
| 1 | [`Content/NPCs/GuMasters/IGuMasterAI.cs`](Content/NPCs/GuMasters/IGuMasterAI.cs) | 修改 | P1, P4 |
| 2 | [`Content/NPCs/GuMasters/GuMasterBase.cs`](Content/NPCs/GuMasters/GuMasterBase.cs) | 修改 | P1, P2, P3, P4 |
| 3 | [`Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs`](Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs) | 修改 | P1, P2, P3 |
| 4 | [`Common/Players/GuWorldPlayer.cs`](Common/Players/GuWorldPlayer.cs) | 修改 | P2, P4 |
| 5 | [`Common/UI/ReputationUI/ReputationUI.cs`](Common/UI/ReputationUI/ReputationUI.cs) | 新建 | P2 |
| 6 | [`Common/UI/ReputationUI/ReputationUISystem.cs`](Common/UI/ReputationUI/ReputationUISystem.cs) | 新建 | P2 |

---

## 实施顺序

1. **Part 3（NPC左跑修复）** — 最紧急，影响游戏体验
2. **Part 1（弹幕保护）** — 核心功能需求
3. **Part 2（声望修复 + UI）** — 修复 + 新功能
4. **Part 4（架构分离）** — 文档 + 少量代码修改

---

## 验证场景

### 场景 1：弹幕保护
1. 玩家发射弹幕 → 古月巡逻蛊师不受伤害
2. 与 NPC 对话 → 点击"去除保护" → NPC 无反应
3. 再次发射弹幕 → NPC 受伤，弹出感叹号
4. NPC 根据信念状态：反击/逃跑/呼叫支援
5. 再次对话 → 点击"开启保护" → 恢复正常

### 场景 2：声望修复
1. 怪物杀死古月巡逻蛊师 → 不扣玩家声望
2. 玩家秘密击杀（无目击者）→ 不扣声望
3. 玩家在目击者面前击杀 → 扣声望
4. 打开声望 UI → 显示各势力声望

### 场景 3：NPC 不左跑
1. 玩家攻击古月巡逻蛊师 → NPC 进入战斗模式（追击或远程攻击）
2. 仇恨持续 30 秒 → 不会自动切换为逃跑
3. 30 秒后根据信念状态自然过渡

### 场景 4：信念与声望分离
1. 玩家被古月家族悬赏 → 个体巡逻蛊师仍用自己的信念判断
2. 玩家与某巡逻蛊师交易过 → 该蛊师对玩家友好（信念变化）
3. 但古月家族整体声望可能仍为负（势力层面）
