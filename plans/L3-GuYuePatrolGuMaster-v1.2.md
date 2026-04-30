# L3 实现文档：GuYuePatrolGuMaster 超级原型 + NpcBrain 簇

> 版本：v1.2（基于 L2-System-Interfaces-v1.1）  
> 目标：验证核心体验——**玩家无法预测古月巡逻蛊师下一秒行为**  
> 优先级：P0 / MVA  
> 预计工作量：3-4 天  
> 依赖：QiResourcePlayer（基础真元读写）/ PerceptionSystem（简化版）

---

## 1. 交付目标

让 `GuYuePatrolGuMaster.cs` 成为**黑暗森林体验的最小可验证单元**。玩家面对这个 NPC 时：

- 第一次遇到：他远远跟着，不靠近（观望）
- 玩家展示酒虫后：他开始试探（询问/接近）
- 玩家暴露弱点后：他突然动手（攻击）
- 玩家反杀后重生：他再次遇到时保持距离（回避）

**验收标准**：连续 5 次遭遇，玩家无法总结出"他会在第几秒动手"的规律。

---

## 2. 文件清单

| 文件 | 动作 | 说明 |
|------|------|------|
| `Content/NPCs/GuMasters/GuYuePatrolGuMaster.cs` | **重写** | 硬编码 NpcBrain 簇（信念+原型+策略），验证通过后抽象 |
| `Content/NPCs/GuMasters/GuMasterBase.cs` | **修改** | 新增 `Perceive()` / `UpdateBelief()` / `DecideStrategy()` 空方法，供子类重写 |
| `Common/Systems/WorldStateMachine.cs` | **修改** | 新增 `IsFamilyCoreZone()` / `GetTerritoryOwner()` 最简实现 |
| `Common/Players/QiResourcePlayer.cs` | **新建** | 最简真元读写（MVA 占位，后续拆分 QiPlayer） |
| `Content/Items/Currencies/YuanStone.cs` | **新建** | 元石物品（MVA 占位，用于 NPC 掉落和交易） |
| `Content/Items/Materials/WildGuMaterial.cs` | **新建** | 野蛊材料（MVA 占位，用于 NPC 掉落和炼蛊） |

---

## 3. 核心数据结构

### 3.1 BeliefState（信念快照）

```csharp
public struct BeliefState
{
    // 对玩家修为的估计
    public GuRank EstimatedRank;      // 当前最佳估计（一转初阶/中阶/巅峰等）
    public float Confidence;           // 置信度 [0,1]，0=完全不确定，1=确信

    // 风险评估
    public float RiskThreshold;        // 风险阈值 [0,1]，低于此值则攻击
    // 动态漂移规则：
    //   初始值 = 0.9（极度谨慎）
    //   观察到玩家弱（低血量/无蛊虫）→ 每次 -0.15，最低 0.3
    //   观察到玩家强（高真元/反杀记录）→ 每次 +0.2，最高 0.95
    //   被玩家击败后 → 直接设为 0.95（恐惧）

    // 交互历史
    public int InteractionCount;        // 与该玩家的交互次数
    public bool WasDefeatedByPlayer;  // 是否曾被该玩家击败
    public long LastSeenTick;         // 上次见到该玩家的游戏帧

    // 信息来源
    public bool HasObservedCombat;     // 是否观察过玩家战斗
    public bool HasBeenAttackedByPlayer; // 是否被玩家攻击过
}
```

### 3.2 PerceptionContext（感知上下文）

```csharp
public struct PerceptionContext
{
    public Player TargetPlayer;
    public float Distance;             // 与玩家的距离
    public float PlayerHealthPercent;  // 玩家生命百分比
    public float PlayerQiPercent;      // 玩家真元百分比（QiResourcePlayer.QiCurrent / QiMax）

    // 蛊虫感知（MVA 简化：只检测玩家是否手持媒介武器）
    public bool PlayerHasMediumWeapon; // 玩家是否持有蛊道媒介（= 有攻击能力）
    public bool PlayerHasActiveGu;     // 玩家是否催动蛊虫（MVA 简化：检测是否有相关 Buff）

    // 环境
    public bool IsInFamilyCore;        // 是否在古月核心区
    public bool IsInFamilyEdge;      // 是否在古月边缘区
    public bool IsWilderness;          // 是否在野外（非任何家族领地）
    public int NearbyFriendlyNPCs;     // 附近友方 NPC 数量（古月家族）

    // 玩家历史（从 GuWorldPlayer 读取）
    public int PlayerInfamyPoints;     // 恶名值
}
```

### 3.3 StrategyDecision（策略决策）

```csharp
public enum Strategy
{
    WaitAndSee,     // 观望：保持距离，观察玩家
    Test,           // 试探：接近到 200px 内，停留 3 秒，观察反应
    DirectRaid,     // 直接抢夺：进入战斗状态，追击玩家
    Avoid,          // 回避：远离玩家，保持 400px 以上距离
    Flee            // 逃跑：生命值 < 30% 时触发，向家族核心区移动
}

public struct StrategyDecision
{
    public Strategy Strategy;
    public int TargetPlayerID;       // 目标玩家
    public int DurationTicks;         // 该策略预计持续帧数（-1 = 直到条件变化）
    public string ReasonLog;          // 决策原因（调试用，MVA 阶段可输出到聊天栏）
}
```

---

## 4. 算法详解

### 4.1 信念更新算法（简化启发式）

MVA 阶段不实现贝叶斯，用**赢者通吃 + 噪声**模型：

```csharp
void UpdateBelief(Player player, PerceptionContext context)
{
    var belief = GetOrCreateBelief(player);

    // 1. 修为估计（基于玩家真元百分比）
    float qiPercent = context.PlayerQiPercent;
    if (qiPercent < 0.3f) belief.EstimatedRank = GuRank.FirstTurnEarly;
    else if (qiPercent < 0.6f) belief.EstimatedRank = GuRank.FirstTurnMid;
    else if (qiPercent < 0.9f) belief.EstimatedRank = GuRank.FirstTurnLate;
    else belief.EstimatedRank = GuRank.FirstTurnPeak;

    // 2. 置信度更新（每次交互 +0.1，最高 0.9）
    belief.Confidence = Math.Min(0.9f, belief.Confidence + 0.1f);

    // 3. 风险阈值漂移（核心算法）
    float playerStrength = CalculatePlayerStrength(context, belief);

    if (belief.WasDefeatedByPlayer)
    {
        // 被击败过 → 极度恐惧
        belief.RiskThreshold = 0.95f;
    }
    else if (playerStrength < 0.4f && belief.Confidence > 0.5f)
    {
        // 玩家弱 + 我确信 → 降低阈值（更激进）
        belief.RiskThreshold = Math.Max(0.3f, belief.RiskThreshold - 0.15f);
    }
    else if (playerStrength > 0.7f)
    {
        // 玩家强 → 提高阈值（更谨慎）
        belief.RiskThreshold = Math.Min(0.95f, belief.RiskThreshold + 0.2f);
    }
    // 否则：保持当前阈值（观望期）

    belief.InteractionCount++;
    belief.LastSeenTick = Main.GameUpdateCount;
}

float CalculatePlayerStrength(PerceptionContext context, BeliefState belief)
{
    float strength = 0.5f; // 基准

    // 真元高 = 强
    strength += (context.PlayerQiPercent - 0.5f) * 0.3f;

    // 生命低 = 弱（可乘之机）
    if (context.PlayerHealthPercent < 0.3f) strength -= 0.3f;

    // 有武器 = 强
    if (context.PlayerHasMediumWeapon) strength += 0.2f;

    // 恶名高 = 强（不好惹）
    strength += context.PlayerInfamyPoints * 0.01f;

    return Math.Clamp(strength, 0f, 1f);
}
```

### 4.2 策略决策算法

```csharp
StrategyDecision DecideStrategy(PerceptionContext context, BeliefState belief)
{
    // 1. 紧急状态优先
    if (NPC.life < NPC.lifeMax * 0.3f)
        return new StrategyDecision { Strategy = Strategy.Flee, ReasonLog = "生命值低于30%" };

    // 2. 核心区规则（D-27：名义安全）
    if (context.IsInFamilyCore)
    {
        // 核心区内不能公开战斗，但可以试探
        if (belief.RiskThreshold < 0.4f && belief.Confidence > 0.6f)
            return new StrategyDecision { Strategy = Strategy.Test, ReasonLog = "核心区试探：玩家弱且已知" };
        else
            return new StrategyDecision { Strategy = Strategy.WaitAndSee, ReasonLog = "核心区观望" };
    }

    // 3. 边缘区/野外：根据 RiskThreshold 决策
    if (belief.RiskThreshold < 0.35f && belief.Confidence > 0.5f)
    {
        // 极度激进：直接动手
        return new StrategyDecision { Strategy = Strategy.DirectRaid, ReasonLog = $"Risk={belief.RiskThreshold:F2}，直接抢夺" };
    }
    else if (belief.RiskThreshold < 0.6f)
    {
        // 中等风险：试探
        return new StrategyDecision { Strategy = Strategy.Test, ReasonLog = $"Risk={belief.RiskThreshold:F2}，试探" };
    }
    else if (belief.RiskThreshold > 0.85f || belief.WasDefeatedByPlayer)
    {
        // 高风险或曾被击败：回避
        return new StrategyDecision { Strategy = Strategy.Avoid, ReasonLog = $"Risk={belief.RiskThreshold:F2}，回避" };
    }
    else
    {
        // 默认：观望
        return new StrategyDecision { Strategy = Strategy.WaitAndSee, ReasonLog = $"Risk={belief.RiskThreshold:F2}，观望" };
    }
}
```

### 4.3 策略执行逻辑（AI 状态机）

```csharp
// 在 GuYuePatrolGuMaster.AI() 中
public override void AI()
{
    // 1. 选择目标玩家（最近的玩家）
    Player target = FindNearestPlayer();
    if (target == null) { Patrol(); return; }

    // 2. 感知
    var context = Perceive(target);

    // 3. 更新信念（每 60 帧更新一次，避免过度计算）
    if (Main.GameUpdateCount % 60 == 0)
        UpdateBelief(target, context);

    // 4. 决策（每 30 帧重新评估）
    if (Main.GameUpdateCount % 30 == 0 || currentStrategy == Strategy.WaitAndSee)
    {
        var belief = GetBelief(target);
        var decision = DecideStrategy(context, belief);
        currentStrategy = decision.Strategy;

        // MVA 调试：输出决策原因到聊天栏（P1 删除）
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText($"[DEBUG] 巡逻蛊师对 {target.name}: {decision.ReasonLog}", Color.Gray);
    }

    // 5. 执行策略
    ExecuteStrategy(currentStrategy, target, context);
}

void ExecuteStrategy(Strategy strategy, Player target, PerceptionContext context)
{
    switch (strategy)
    {
        case Strategy.WaitAndSee:
            // 保持 300-400px 距离，面朝玩家，缓慢移动
            MoveToDistance(target.Center, 350f, 50f);
            NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
            break;

        case Strategy.Test:
            // 接近到 200px，停留 3 秒，然后后退
            if (context.Distance > 250f)
                MoveToDistance(target.Center, 200f, 30f);
            else
            {
                // 停留观察：NPC 停止移动，播放"观察"动画
                NPC.velocity *= 0.9f;
                // MVA：若玩家在此期间攻击，RiskThreshold 骤降，下次决策转为 DirectRaid
                if (target.itemAnimation > 0) // 玩家正在挥武器
                {
                    var belief = GetBelief(target);
                    belief.RiskThreshold -= 0.2f; // 玩家攻击 = 暴露弱点/敌意
                }
            }
            break;

        case Strategy.DirectRaid:
            // 进入战斗状态：追击玩家，发射弹幕
            NPC.target = target.whoAmI;
            NPC.aiStyle = NPCAIStyleID.Fighter; // 或使用自定义战斗 AI
            // 发射月光蛊弹幕（若玩家进入射程）
            if (context.Distance < 400f && Main.GameUpdateCount % 60 == 0)
                FireProjectile(target);
            break;

        case Strategy.Avoid:
            // 远离玩家，向巡逻中心点移动
            MoveToDistance(target.Center, 500f, 80f, reverse: true);
            break;

        case Strategy.Flee:
            // 向家族核心区逃跑
            Vector2 corePosition = GetFamilyCorePosition();
            NPC.velocity = (corePosition - NPC.Center).SafeNormalize(Vector2.UnitY) * 4f;
            break;
    }
}
```

---

## 5. 与 L2 接口的对照

| L2 定义 | L3 实现 | 差异说明 |
|---------|---------|---------|
| `BeliefSubsystem` | `BeliefState` struct + `UpdateBelief()` 方法 | MVA 简化为 struct，不独立为类 |
| `ArchetypeSubsystem` | 硬编码为"掠夺型"，只实现 `RiskThreshold` 漂移 | MVA 不实现交易型/隐忍型 |
| `StrategySubsystem` | `DecideStrategy()` 方法 | MVA 只实现 5 种策略中的 4 种（无 DeceptiveApproach/GatherIntel/ExecutePlan） |
| `PerceptionContext` | 简化版：只检测真元/生命/武器/环境 | MVA 不实现 `LeakedGus` / `NearbyHostileNPCs` |
| `CorpseReaction` | 不实现 | MVA 阶段 NPC 不主动搜尸，只攻击在场玩家 |
| `PlayerBeliefStorage` | 信念存于 `Dictionary<int, BeliefState>`（NPC 实例局部变量） | MVA 不持久化，玩家退出后失忆 |

---

## 6. 代码框架

### 6.1 GuYuePatrolGuMaster.cs（核心）

```csharp
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.NPCs.GuMasters
{
    public class GuYuePatrolGuMaster : GuMasterBase
    {
        // ===== MVA 硬编码：NpcBrain 簇 =====
        private Dictionary<int, BeliefState> playerBeliefs = new(); // Key = Player.whoAmI
        private Strategy currentStrategy = Strategy.WaitAndSee;
        private Vector2 patrolCenter; // 巡逻中心点（生成位置）
        private int patrolRadius = 800; // 巡逻半径

        // 生成条件
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // 只在古月驻地生物群系生成
            if (!spawnInfo.Player.InModBiome(ModContent.GetInstance<GuYueCompoundBiome>()))
                return 0f;
            // 白天生成，夜晚更频繁（MVA 简化）
            return Main.dayTime ? 0.1f : 0.3f;
        }

        public override void SetDefaults()
        {
            NPC.width = 24;
            NPC.height = 42;
            NPC.damage = 15;
            NPC.defense = 5;
            NPC.lifeMax = 200;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 50f;
            NPC.aiStyle = -1; // 自定义 AI
            NPC.friendly = false; // 可攻击
            patrolCenter = NPC.Center;
        }

        public override void AI()
        {
            // 1. 选择目标玩家
            Player target = FindNearestPlayer(600f);
            if (target == null)
            {
                Patrol();
                return;
            }

            // 2. 感知与信念更新
            var context = Perceive(target);
            if (Main.GameUpdateCount % 60 == 0)
                UpdateBelief(target, context);

            // 3. 决策
            var belief = GetBelief(target);
            if (Main.GameUpdateCount % 30 == 0 || currentStrategy == Strategy.WaitAndSee)
            {
                var decision = DecideStrategy(context, belief);
                currentStrategy = decision.Strategy;

                // MVA 调试输出
                if (Main.netMode == NetmodeID.SinglePlayer && decision.Strategy != Strategy.WaitAndSee)
                    Main.NewText($"[巡逻蛊师] 对 {target.name}: {decision.ReasonLog}", Color.LightGray);
            }

            // 4. 执行
            ExecuteStrategy(currentStrategy, target, context);
        }

        // ===== 感知系统（MVA 简化） =====
        private PerceptionContext Perceive(Player player)
        {
            // 检测玩家是否刚复活（复活后 180 帧内为脆弱窗口）
            // 🟡 已知局限：此条件也会匹配「玩家手动停用所有蛊虫 + 真元耗光」的正常状态
            // MVA 阶段接受此误判（手动耗光真元也是一种脆弱状态），P1 可通过检测「复活后 N 帧内」精确化
            bool recentlyResurrected = false;
            if (player.TryGetModPlayer(out QiResourcePlayer qiPlayer))
            {
                // 真元清空 + 蛊虫休眠 = 刚复活的状态
                recentlyResurrected = qiPlayer.QiCurrent < qiPlayer.QiMax * 0.1f && qiPlayer.QiOccupied == 0;
            }

            var context = new PerceptionContext
            {
                TargetPlayer = player,
                Distance = Vector2.Distance(NPC.Center, player.Center),
                PlayerHealthPercent = (float)player.statLife / player.statLifeMax2,
                PlayerQiPercent = GetPlayerQiPercent(player),
                PlayerHasMediumWeapon = player.HeldItem.ModItem is MediumWeaponSystem,
                PlayerRecentlyResurrected = recentlyResurrected,  // 复活脆弱窗口标记
                IsInFamilyCore = WorldStateMachine.Instance.IsFamilyCoreZone(NPC.Center, FactionID.GuYue),
                IsInFamilyEdge = WorldStateMachine.Instance.IsFamilyEdgeZone(NPC.Center, FactionID.GuYue),
                IsWilderness = !WorldStateMachine.Instance.IsFamilyCoreZone(NPC.Center, FactionID.GuYue) 
                               && !WorldStateMachine.Instance.IsFamilyEdgeZone(NPC.Center, FactionID.GuYue),
                NearbyFriendlyNPCs = CountNearbyFriendlyNPCs(300f),
                PlayerInfamyPoints = player.GetModPlayer<GuWorldPlayer>().InfamyPoints
            };
            return context;
        }

        private float GetPlayerQiPercent(Player player)
        {
            var qiPlayer = player.GetModPlayer<QiResourcePlayer>();
            if (qiPlayer.QiMax <= 0) return 0f;
            return qiPlayer.QiCurrent / qiPlayer.QiMax;
        }

        // ===== 信念系统（MVA 简化） =====
        private BeliefState GetOrCreateBelief(Player player)
        {
            if (!playerBeliefs.ContainsKey(player.whoAmI))
            {
                playerBeliefs[player.whoAmI] = new BeliefState
                {
                    EstimatedRank = GuRank.FirstTurnMid,
                    Confidence = 0.1f,
                    RiskThreshold = 0.9f, // 初始极度谨慎
                    InteractionCount = 0,
                    LastSeenTick = 0
                };
            }
            return playerBeliefs[player.whoAmI];
        }

        private BeliefState GetBelief(Player player) => GetOrCreateBelief(player);

        private void UpdateBelief(Player player, PerceptionContext context)
        {
            var belief = GetBelief(player);

            // 修为估计
            float qiPct = context.PlayerQiPercent;
            belief.EstimatedRank = qiPct switch
            {
                < 0.3f => GuRank.FirstTurnEarly,
                < 0.6f => GuRank.FirstTurnMid,
                < 0.9f => GuRank.FirstTurnLate,
                _ => GuRank.FirstTurnPeak
            };

            // 置信度
            belief.Confidence = Math.Min(0.9f, belief.Confidence + 0.05f);

            // 风险阈值漂移（核心）
            float strength = CalcPlayerStrength(context, belief);

            if (belief.WasDefeatedByPlayer)
                belief.RiskThreshold = 0.95f;
            else if (strength < 0.4f && belief.Confidence > 0.5f)
                belief.RiskThreshold = Math.Max(0.3f, belief.RiskThreshold - 0.15f);
            else if (strength > 0.7f)
                belief.RiskThreshold = Math.Min(0.95f, belief.RiskThreshold + 0.2f);

            belief.InteractionCount++;
            belief.LastSeenTick = Main.GameUpdateCount;
        }

        private float CalcPlayerStrength(PerceptionContext ctx, BeliefState belief)
        {
            float s = 0.5f;
            s += (ctx.PlayerQiPercent - 0.5f) * 0.3f;
            if (ctx.PlayerHealthPercent < 0.3f) s -= 0.3f;
            if (ctx.PlayerHasMediumWeapon) s += 0.2f;
            s += ctx.PlayerInfamyPoints * 0.01f;
            return Math.Clamp(s, 0f, 1f);
        }

        // ===== 策略系统（MVA 简化） =====
        private StrategyDecision DecideStrategy(PerceptionContext context, BeliefState belief)
        {
            // 紧急：濒死逃跑
            if (NPC.life < NPC.lifeMax * 0.3f)
                return new StrategyDecision { Strategy = Strategy.Flee, ReasonLog = "濒死逃跑" };

            // 核心区规则
            if (context.IsInFamilyCore)
            {
                if (belief.RiskThreshold < 0.4f && belief.Confidence > 0.6f)
                    return new StrategyDecision { Strategy = Strategy.Test, ReasonLog = "核心区试探" };
                return new StrategyDecision { Strategy = Strategy.WaitAndSee, ReasonLog = "核心区观望" };
            }

            // 边缘区/野外
            if (belief.RiskThreshold < 0.35f && belief.Confidence > 0.5f)
                return new StrategyDecision { Strategy = Strategy.DirectRaid, ReasonLog = $"Risk低({belief.RiskThreshold:F2})，抢夺" };
            else if (belief.RiskThreshold < 0.6f)
                return new StrategyDecision { Strategy = Strategy.Test, ReasonLog = $"Risk中({belief.RiskThreshold:F2})，试探" };
            else if (belief.RiskThreshold > 0.85f || belief.WasDefeatedByPlayer)
                return new StrategyDecision { Strategy = Strategy.Avoid, ReasonLog = $"Risk高({belief.RiskThreshold:F2})，回避" };
            else
                return new StrategyDecision { Strategy = Strategy.WaitAndSee, ReasonLog = $"Risk平({belief.RiskThreshold:F2})，观望" };
        }

        // ===== 策略执行 =====
        private void ExecuteStrategy(Strategy strategy, Player target, PerceptionContext context)
        {
            switch (strategy)
            {
                case Strategy.WaitAndSee:
                    MoveToDistance(target.Center, 350f, 2f);
                    NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
                    break;

                case Strategy.Test:
                    if (context.Distance > 250f)
                        MoveToDistance(target.Center, 200f, 3f);
                    else
                    {
                        NPC.velocity *= 0.9f;
                        // 若玩家攻击，下次决策更激进
                        if (target.itemAnimation > 0)
                        {
                            var belief = GetBelief(target);
                            belief.RiskThreshold -= 0.15f;
                        }
                    }
                    break;

                case Strategy.DirectRaid:
                    NPC.target = target.whoAmI;
                    // 追击
                    Vector2 chaseDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                    NPC.velocity = chaseDir * 3f;
                    // 发射弹幕（MVA：简单弹幕）
                    if (context.Distance < 400f && Main.GameUpdateCount % 60 == 0)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, 
                            chaseDir * 8f, ProjectileID.WoodenArrowFriendly, NPC.damage, 2f);
                    }
                    break;

                case Strategy.Avoid:
                    MoveToDistance(target.Center, 500f, 4f, reverse: true);
                    break;

                case Strategy.Flee:
                    Vector2 fleeDir = (patrolCenter - NPC.Center).SafeNormalize(Vector2.UnitY);
                    NPC.velocity = fleeDir * 5f;
                    break;
            }
        }

        // ===== 辅助方法 =====
        private void Patrol()
        {
            // 在巡逻中心周围随机移动
            if (Vector2.Distance(NPC.Center, patrolCenter) > patrolRadius)
            {
                Vector2 returnDir = (patrolCenter - NPC.Center).SafeNormalize(Vector2.UnitY);
                NPC.velocity = returnDir * 2f;
            }
            else
            {
                // 随机闲逛
                if (Main.GameUpdateCount % 120 == 0)
                    NPC.velocity = new Vector2(Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
            }
        }

        private Player FindNearestPlayer(float maxDistance)
        {
            Player nearest = null;
            float nearestDist = maxDistance;
            foreach (Player player in Main.player)
            {
                if (!player.active) continue;
                float dist = Vector2.Distance(NPC.Center, player.Center);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = player;
                }
            }
            return nearest;
        }

        private void MoveToDistance(Vector2 target, float desiredDistance, float speed, bool reverse = false)
        {
            float currentDist = Vector2.Distance(NPC.Center, target);
            Vector2 dir = (target - NPC.Center).SafeNormalize(Vector2.UnitY);
            if (reverse) dir = -dir;

            if (Math.Abs(currentDist - desiredDistance) > 50f)
            {
                NPC.velocity = dir * speed * (currentDist > desiredDistance ? 1f : -1f);
            }
            else
            {
                NPC.velocity *= 0.8f;
            }
        }

        private int CountNearbyFriendlyNPCs(float radius)
        {
            int count = 0;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.ModNPC is GuMasterBase gu && gu.Faction == FactionID.GuYue)
                    if (Vector2.Distance(NPC.Center, npc.Center) < radius)
                        count++;
            }
            return count;
        }

        // ===== 击杀处理 =====
        public override void OnKill()
        {
            // 生成暴露掉落（30% 基础物品散落）
            GenerateExposedDrops();

            // 发布死亡事件
            EventBus.Publish(new NPCDeathEvent
            {
                NPCType = NPC.type,
                NPCWhoAmI = NPC.whoAmI,
                KillerPlayerID = NPC.lastAttackPlayer,
                Position = NPC.Center,
                Faction = FactionID.GuYue,
                VacatedRole = FactionRole.Patrol // 巡逻蛊师无职务，但保留字段
            });

            base.OnKill();
        }

        private void GenerateExposedDrops()
        {
            // MVA 简化掉落：元石 + 随机低阶材料
            // 暴露掉落 = 击杀瞬间散落，玩家可立即拾取
            int yuanStoneCount = Main.rand.Next(3, 8);
            for (int i = 0; i < yuanStoneCount; i++)
            {
                Item.NewItem(NPC.GetSource_Death(), NPC.Center, ModContent.ItemType<YuanStone>(), 1);
            }

            // 20% 概率掉落未炼化蛊虫材料
            if (Main.rand.NextBool(5))
            {
                Item.NewItem(NPC.GetSource_Death(), NPC.Center, ModContent.ItemType<WildGuMaterial>(), 1);
            }
        }

        // 🔴 修正 v1.2：删除 legacyBeliefs 静态字典和 OnSpawn 继承逻辑
        // 理由：与 L1 锁定的「普通 NPC 重生后不继承仇恨」决策直接冲突。
        // 巡逻蛊师的信念生命周期 = 实例生命周期。创建 → 交互 → 死亡 → 信念销毁。
        // 这是 MVA 的正确设计，避免「杀一个等于惹了全部」的全局可预测性。

        public override void HitEffect(NPC.HitInfo hit)
        {
            // 被玩家击败时，标记信念
            if (NPC.life <= 0 && hit.PlayerSlot >= 0 && hit.PlayerSlot < Main.player.Length)
            {
                Player killer = Main.player[hit.PlayerSlot];
                if (killer.active && playerBeliefs.ContainsKey(killer.whoAmI))
                {
                    playerBeliefs[killer.whoAmI].WasDefeatedByPlayer = true;
                    playerBeliefs[killer.whoAmI].RiskThreshold = 0.95f;
                }
            }
            base.HitEffect(hit);
        }
    }
}
```

### 6.2 GuMasterBase.cs（新增空方法）

```csharp
public abstract class GuMasterBase : ModNPC
{
    // 新增：供子类重写的感知/信念/决策钩子
    public virtual PerceptionContext Perceive(Player player) => default;
    public virtual void UpdateBelief(Player player, PerceptionContext context) { }
    public virtual StrategyDecision DecideStrategy(PerceptionContext context, BeliefState belief) => default;

    // 势力标识
    public virtual FactionID Faction => FactionID.Scattered;
    public virtual FactionRole Role => FactionRole.None;
}
```

### 6.3 QiResourcePlayer.cs（MVA 最简占位）

```csharp
public class QiResourcePlayer : ModPlayer
{
    public float QiMax = 100f;
    public float QiCurrent = 100f;
    public float QiRegenRate = 0.5f;
    public int QiOccupied = 0;
    public int QiAvailable => (int)QiMax - QiOccupied;

    public void UpdateQiOccupied(int occupied) => QiOccupied = occupied;

    public bool ConsumeQi(float amount, QiConsumeReason reason)
    {
        if (QiCurrent < amount) return false;
        QiCurrent -= amount;
        return true;
    }

    public override void PostUpdate()
    {
        QiCurrent = Math.Min(QiMax, QiCurrent + QiRegenRate);
    }
}
```

---

## 7. 验收标准

### 7.1 功能验收（必须全部通过）

| 场景 | 操作 | 预期行为 | 通过标准 |
|------|------|---------|---------|
| 新手首次遇到 | 玩家接近巡逻蛊师 | NPC 保持 300-400px 距离，面朝玩家，不攻击 | 连续 10 秒不进入战斗状态 |
| 玩家展示酒虫 | 玩家催动蛊虫（获得 Buff） | NPC 置信度上升，若玩家真元低则 RiskThreshold 下降 | 调试输出显示 RiskThreshold < 0.6 |
| 玩家暴露弱点 | 玩家生命值 < 30% | NPC 接近到 200px，若玩家不反击则转为 DirectRaid | 3 秒内进入战斗状态 |
| 玩家反杀 | 玩家击杀巡逻蛊师 | 同一实例被反杀后，RiskThreshold 升至 0.95 | 调试输出显示 WasDefeatedByPlayer = true |
| 恐惧衰减 | 同一实例被反杀后，等待 50 秒（NPC 恢复生命） | RiskThreshold 从 0.95 缓慢下降至 ~0.80 | 调试输出显示 FramesSinceDefeated 增加，RiskThreshold 下降 |
| 复活脆弱窗口 | 玩家被击杀后复活，立即遇到 NPC | NPC 观望而非立即攻击（即使玩家真元为 0） | 调试输出显示「玩家刚复活，犹豫观望」 |
| 核心区规则 | 玩家进入古月核心区 | NPC 不攻击，即使 RiskThreshold 很低也只试探 | 核心区内不触发 DirectRaid |
| 濒死逃跑 | NPC 生命值 < 30% | 向巡逻中心逃跑，不再追击 | 移动方向指向巡逻中心 |
| NPC 死亡掉落 | 击杀巡逻蛊师 | 散落元石和材料，可立即拾取 | 地面上出现掉落物 |
| 新实例不继承 | 击杀后等待重生，新实例生成 | 新实例对玩家无记忆，RiskThreshold 回到初始 0.9 | 调试输出显示新实例无 WasDefeatedByPlayer 标记 |

### 7.2 体验验收（主观评估）

- [ ] 玩家无法通过"他上次打我了所以这次一定打我"来预测行为
- [ ] 玩家感受到"他在观察我"的紧张感（NPC 面朝玩家、保持距离）
- [ ] 玩家感受到"他是因为我弱才动手"的因果逻辑（而非随机攻击）
- [ ] 调试输出（聊天栏）显示的 RiskThreshold 变化符合直觉

### 7.3 代码验收

- [ ] `GuYuePatrolGuMaster.cs` 行数控制在 400 行以内（当前基线 649 行，重构后应更精简）
- [ ] 无硬编码魔法数字（所有常量提取为字段或配置）
- [ ] 事件总线发布至少 1 个事件（NPCDeathEvent）

---

## 8. 风险与回退

| 风险 | 影响 | 回退方案 |
|------|------|---------|
| AI 计算频率过高导致卡顿 | 性能问题 | 降低信念更新频率（60帧→120帧），降低感知距离 |
| RiskThreshold 漂移过快导致行为乱跳 | 体验问题 | 增加漂移冷却期（每次变化后 300 帧内不再变化） |
| 玩家在核心区被无限试探骚扰 | 体验问题 | 核心区试探次数上限（最多 3 次后转为永久观望） |
| 调试输出过多污染聊天栏 | 开发问题 | 增加开关 `/gudebug` 指令控制输出 |

---

## 9. 下一步（验证通过后）

1. 将 `BeliefState` / `PerceptionContext` / `StrategyDecision` 提取到 `Common/NPC/NpcBrain/` 目录
2. 将 `UpdateBelief` / `DecideStrategy` / `ExecuteStrategy` 抽象为 `NpcBrain` 基类
3. 为 `GuMasterBase` 接入 `NpcBrain` 实例，让所有蛊师 NPC 共享同一套决策逻辑
4. 实现交易型/隐忍型原型，通过配置切换

---

> 本文档由第三层实现工程师维护。  
> 实现过程中发现接口不匹配，立即回传 L2 登记修正，禁止私自修改 L2 冻结的数据结构。
