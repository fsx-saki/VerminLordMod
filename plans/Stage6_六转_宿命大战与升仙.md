# 六转·宿命大战与升仙 — 详细实现计划

> **对应Terraria阶段**：Moon Lord阶段（五柱 → Moon Lord）
> **对应小说章节**：第1200~1800章（天庭篇 + 升仙）
> **转数范围**：五转巅峰 → 六转（升仙）
> **前置条件**：五转巅峰 + 击败冰塞川或五行大法师 + 天庭前奏事件
> **预估时长**：15~20小时
> **核心情感**：从凡人巅峰到超越凡人，见证宿命的碎裂

---

## 1. 宿命大战世界事件

### 1.1 三阶段世界事件

**新系统**：`Common/Systems/DestinyWarSystem.cs`

**触发条件**：

| 条件 | 具体要求 |
|------|---------|
| 境界 | 玩家五转巅峰（LevelStage=3） |
| Boss前置 | 击败冰塞川或五行大法师 |
| 剧情前置 | 天庭前奏事件已完成 |
| 时间前置 | 天庭前奏后至少3个游戏日 |

### 1.2 第一阶段：天庭入侵（=四柱事件）

**对应Terraria**：Lunatic Cultist击败后→四柱出现

**事件流程**：

```
触发条件满足 → 玩家进入地表
    ↓
天空变为金色（天庭降临）
    ↓
世界消息："天庭降临！宿命之战开始了！"
    ↓
世界4个位置生成天庭浮岛（=四柱）
    ↓
每个浮岛有守护者（=柱子敌人）
    ↓
玩家需要击败4个浮岛守护者
    ↓
所有守护者击败后 → 龙公出现
```

**四柱对应设计**：

| 柱子 | 天庭浮岛 | 守护者 | 敌人类型 | 颜色 |
|------|---------|--------|---------|------|
| 星辰柱 | 星宿浮岛 | 星宿守卫 | 智道蛊师群 | 紫色 |
| 太阳柱 | 日冕浮岛 | 烈日守卫 | 火道/光道蛊师群 | 金色 |
| 星云柱 | 命运浮岛 | 命运守卫 | 命运道/运道蛊师群 | 粉色 |
| 星旋柱 | 龙威浮岛 | 龙卫 | 气道/变化道蛊师群 | 青色 |

**浮岛生成**：
- 在世界4个象限各生成一个浮岛
- 浮岛高度：地表上方30~50格
- 浮岛大小：40×30格
- 使用 `WorldGen` 在 `DestinyWarSystem.OnInitialize()` 中生成

**浮岛守护者**：
- 每个浮岛中心有一个不可移动的守护者NPC
- 守护者周围持续刷新小怪
- 击杀足够数量小怪后守护者护盾消失
- 护盾消失后可攻击守护者
- 击败守护者后浮岛消失

**护盾机制**：
```csharp
public class PillarShieldNPC : ModNPC
{
    public int KillCount;
    public const int REQUIRED_KILLS = 50; // 需击杀50个小怪

    public override bool CheckActive() => false; // 不可消失

    public override void ModifyHitByProjectile(Projectile proj, ref int damage,
        ref float knockback, ref bool crit, ref int hitDirection)
    {
        if (KillCount < REQUIRED_KILLS)
        {
            damage = 0; // 护盾未破，免疫伤害
            Main.NewText("护盾仍在！继续击杀周围的蛊师！", Color.Gray);
        }
    }
}
```

### 1.3 第二阶段：三方混战

**阵营选择**：

```
四柱全部击败后 → 世界消息："选择你的立场！"
    ↓
弹出阵营选择界面：
  A. 正道——与天庭合作，维护宿命秩序
  B. 魔道——与影宗合作，打破宿命
  C. 中立——为自己而战
    ↓
选择后影响：
  - NPC态度（正道/魔道NPC对玩家态度变化）
  - 可用盟友（阵营NPC会协助战斗）
  - 龙公战难度（正道路线龙公更强，但有盟友；魔道路线龙公稍弱，但孤立）
  - 最终奖励（不同阵营获得不同真传）
```

**阵营选择实现**：
```csharp
// 扩展 FactionAllegianceSystem
public enum DestinyWarFaction
{
    NotChosen,
    Righteous,  // 正道
    Demonic,    // 魔道
    Neutral,    // 中立
}

public static DestinyWarFaction PlayerDestinyChoice;
```

**NPC军团混战**：

| 阵营 | 出现NPC | 行为 |
|------|---------|------|
| 天庭 | 龙公/铜公/眉公/天庭蛊师×20 | 攻击魔道和中立 |
| 影宗 | 影无邪/砚石老人/影宗蛊师×15 | 攻击天庭 |
| 中立 | 散修蛊师×10 | 防守，不主动攻击 |

**NPC军团AI**：
- 使用 `CombatBehavior` 控制NPC战斗
- NPC之间互相攻击（不同阵营）
- 玩家阵营的NPC会协助玩家
- NPC死亡后不会立即刷新（战争消耗）

### 1.4 第三阶段：龙公之战

**触发条件**：四柱全部击败 + 阵营选择完成

**详见§2**

---

## 2. 龙公Boss战（Moon Lord等效）

**已有文件**：`Content/NPCs/HeavenCourt/LongGong.cs`

### 2.1 基础属性

| 属性 | 值 | 说明 |
|------|-----|------|
| 生命 | 250000 | 准九转，游戏最强Boss |
| 伤害 | 180 | 基础攻击 |
| 防御 | 70 | 极高防御 |
| 击退抗性 | 100% | 完全不可击退 |
| AI风格 | 自定义 | 四阶段Boss |
| 生成方式 | 四柱击败后自动出现 |

### 2.2 阶段设计

**第一阶段（100%~70%）——气道攻击**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 气旋弹 | 向玩家发射3发追踪气旋 | 120 | 每2秒 |
| 气压场 | 龙公周围20格内气压增大，玩家移速-40% | 20/秒 | 持续 |
| 龙息 | 扇形范围吐息攻击 | 150 | 每6秒 |
| 气道·压缩 | 将玩家拉向龙公（强制位移） | 0 | 每10秒 |

**气旋弹实现**：
- 使用 `HomingBehavior` + `QiTrailBehavior`
- 3发弹幕呈扇形发射
- 追踪速度3，持续5秒后自毁
- 使用 `GlowDrawBehavior` 发光效果

**气压场实现**：
```csharp
// 龙公AI中
float pressureRadius = 20f * 16f;
foreach (Player player in Main.player)
{
    if (!player.active || player.dead) continue;
    float dist = Vector2.Distance(player.Center, npc.Center);
    if (dist < pressureRadius)
    {
        player.moveSpeed *= 0.6f;
        if (Main.rand.NextBool(60))
        {
            player.Hurt(PlayerDeathReason.ByCustomCause("气压碾压"),
                20, 0, false, false, 0, false);
        }
    }
}
```

**气道·压缩（强制位移）实现**：
```csharp
// 将玩家拉向龙公
Vector2 pullDir = (npc.Center - player.Center);
pullDir.Normalize();
player.position += pullDir * 5f; // 每帧拉5像素
player.velocity = pullDir * 3f;
```

**第二阶段（70%~40%）——变化道**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 龙人变身 | 龙公变为龙人形态，攻击方式改变 | - | 70%血量时 |
| 龙爪斩 | 近战爪击，前方扇形范围 | 200 | 每3秒 |
| 龙尾扫 | 旋转攻击，360度范围 | 160 | 每8秒 |
| 变化·幻影 | 生成2个幻影分身，同时攻击 | 100 | 每15秒 |
| 变化·吞噬 | 龙公吞噬玩家弹幕，回复生命 | - | 每20秒 |

**龙人变身实现**：
```csharp
// 70%血量时触发
if (npc.life <= npc.lifeMax * 0.7f && !phase2Triggered)
{
    phase2Triggered = true;
    // 变身动画
    npc.dontTakeDamage = true; // 3秒无敌
    // 变身特效
    for (int i = 0; i < 50; i++)
    {
        Dust.NewDustPerfect(npc.Center, DustID.DragonFlame,
            new Vector2(Main.rand.Next(-8, 8), Main.rand.Next(-8, 8)));
    }
    // 改变外观（使用不同的帧图）
    isDragonForm = true;
    // 3秒后取消无敌
}
```

**变化·幻影分身实现**：
```csharp
// 生成2个幻影NPC
for (int i = 0; i < 2; i++)
{
    Vector2 offset = new Vector2(Main.rand.Next(-200, 200), Main.rand.Next(-100, 100));
    int phantom = NPC.NewNPC(npc.GetSource_FromAI(),
        (int)(npc.Center.X + offset.X),
        (int)(npc.Center.Y + offset.Y),
        ModContent.NPCType<LongGongPhantom>());
    // 幻影属性
    Main.npc[phantom].lifeMax = 10000;
    Main.npc[phantom].life = 10000;
    Main.npc[phantom].damage = 100;
    Main.npc[phantom].defense = 20;
}
```

**变化·吞噬弹幕实现**：
```csharp
// 龙公吞噬范围内的玩家弹幕
float devourRadius = 15f * 16f;
foreach (Projectile proj in Main.projectile)
{
    if (proj.active && proj.friendly && !proj.hostile)
    {
        float dist = Vector2.Distance(proj.Center, npc.Center);
        if (dist < devourRadius)
        {
            // 弹幕被吞噬
            proj.Kill();
            // 龙公回复生命
            npc.life = Math.Min(npc.life + 200, npc.lifeMax);
            // 吞噬特效
            Dust.NewDustPerfect(proj.Center, DustID.PurpleTorch, Vector2.Zero);
        }
    }
}
```

**第三阶段（40%~10%）——龙人寂灭前奏**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 气道+变化道 | 同时使用两种道的攻击 | - | 持续 |
| 龙威 | 周期性释放龙威，玩家被恐惧2秒 | 0 | 每12秒 |
| 龙鳞护甲 | 龙公防御+100%，持续5秒 | - | 每20秒 |
| 龙人·蓄力 | 龙公开始蓄力龙人寂灭 | - | 15%血量时 |

**龙威（恐惧效果）实现**：
```csharp
// 龙威释放
foreach (Player player in Main.player)
{
    if (!player.active || player.dead) continue;
    float dist = Vector2.Distance(player.Center, npc.Center);
    if (dist < 50f * 16f) // 50格范围
    {
        player.AddBuff(BuffID.Frozen, 120); // 2秒冻结（模拟恐惧）
        // 或使用自定义恐惧debuff
    }
}
```

**第四阶段（10%~0%）——龙人寂灭**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 龙人寂灭 | 终极攻击：全屏即死级伤害 | 9999 | 10%血量时 |
| 寂灭蓄力 | 10秒蓄力，屏幕逐渐变白 | - | 触发后 |
| 寂灭前兆 | 蓄力期间释放小范围攻击 | 100 | 每2秒 |

**龙人寂灭详细机制**：
```
龙公血量降至10% → 触发龙人寂灭
    ↓
屏幕震动 + 天空变为纯白
    ↓
世界消息："龙人寂灭——！"
    ↓
10秒倒计时显示在屏幕上
    ↓
蓄力期间：
  - 龙公不可移动，不可攻击
  - 龙公防御归零
  - 每2秒释放一圈弹幕（干扰玩家攻击）
    ↓
10秒后：
  - 全屏9999伤害（即死级）
  - 所有NPC和玩家受到伤害
  - 只有"绝对防御"类Buff可以抵挡
    ↓
若玩家在10秒内击败龙公 → 寂灭被打断
若玩家未能击败 → 寂灭释放 → 玩家必死
```

**即死机制与春秋蝉的交互**：
```
龙人寂灭释放 → 玩家受到9999伤害
    ↓
春秋蝉检测到即死伤害 → 触发回溯
    ↓
【关键设计】：
  首次战斗：春秋蝉回溯后，世界回溯到战前状态（见§3）
  第二次战斗：春秋蝉回溯正常工作，玩家可以继续战斗
```

### 2.3 掉落物

| 物品 | 概率 | 数量 | 说明 |
|------|------|------|------|
| 气道至尊真传碎片 | 100% | 3~5 | 合成气道至尊真传 |
| 龙鳞 | 50% | 5~10 | 制作龙系装备 |
| 龙公之魂 | 20% | 1 | 制作六转武器 |
| 宿命碎片 | 100% | 1 | 宿命蛊碎裂的关键道具 |
| 升仙凭证 | 100% | 1 | 触发升仙事件的必要道具 |
| 元石 | 100% | 100~200 | 基础货币 |

---

## 3. 春秋蝉逆转

### 3.1 首次战斗——必败

**设计理念**：在小说中，方源多次战败后利用春秋蝉回溯重来。玩家首次面对龙公时，龙人寂灭是无法抵挡的，必须经历一次"剧情杀"。

**触发条件**：龙公释放龙人寂灭，玩家死亡

**逆转流程**：

```
龙人寂灭释放 → 玩家受到9999伤害
    ↓
春秋蝉触发回溯（自动，无需玩家操作）
    ↓
特殊回溯动画（不同于普通回溯）：
  - 屏幕逐渐变为纯白（3秒）
  - 所有声音消失
  - 世界消息："春秋蝉……逆转！"
    ↓
屏幕从纯白恢复 → 玩家出现在龙公战前的位置
    ↓
世界状态完全回溯：
  - 龙公恢复满血（但不再出现）
  - 所有NPC恢复到战前状态
  - 玩家消耗品恢复到战前状态
  - 世界事件回溯到宿命大战开始前
    ↓
玩家获得"前世记忆"Buff（见下文）
    ↓
世界消息："你从死亡中归来，带着前世的记忆……"
```

**世界回溯实现**：
```csharp
// ChunQiuChanPlayer 扩展
public class DestinyRebirthState
{
    public bool HasRebirthedFromDestiny;  // 是否已从宿命战回溯
    public int RebirthCount;              // 回溯次数
    public PlayerStateSnapshot PreBattleSnapshot;  // 战前快照
}

// 在龙人寂灭触发时
public override bool PreKill(double damage, int hitDirection, bool pvp,
    ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
{
    // 检查是否为龙人寂灭伤害
    if (damageSource.SourceNPCType == ModContent.NPCType<LongGong>()
        && damage >= 9000)
    {
        var destinyState = Player.GetModPlayer<DestinyRebirthState>();

        if (!destinyState.HasRebirthedFromDestiny)
        {
            // 首次回溯：触发世界回溯
            TriggerDestinyRebirth();
            return false; // 取消死亡
        }
    }
    return true; // 正常死亡
}
```

**世界回溯详细实现**：
```csharp
private void TriggerDestinyRebirth()
{
    var destinyState = Player.GetModPlayer<DestinyRebirthState>();

    // 1. 播放回溯动画
    StartRebirthCutscene();

    // 2. 回溯玩家状态
    var snapshot = destinyState.PreBattleSnapshot;
    Player.statLife = snapshot.Life;
    Player.statMana = snapshot.Mana;
    Player.position = snapshot.Position;

    // 3. 回溯世界状态
    // - 移除龙公NPC
    foreach (NPC npc in Main.npc)
    {
        if (npc.active && npc.ModNPC is LongGong)
            npc.active = false;
    }

    // - 回溯四柱状态
    DownBossSystem.downedStarPillar = false;
    DownBossSystem.downedSolarPillar = false;
    DownBossSystem.downedNebulaPillar = false;
    DownBossSystem.downedVortexPillar = false;

    // - 回溯世界事件
    DestinyWarSystem.ResetToPreWar();

    // 4. 标记已回溯
    destinyState.HasRebirthedFromDestiny = true;
    destinyState.RebirthCount++;

    // 5. 给予前世记忆Buff
    Player.AddBuff(ModContent.BuffType<PastLifeMemoryBuff>(), 36000); // 10分钟

    // 6. 世界消息
    Main.NewText("春秋蝉逆转！你从死亡中归来，带着前世的记忆……", Color.Gold);
}
```

### 3.2 前世记忆Buff

**新Buff**：`Content/Buffs/AddToSelf/Pobuff/PastLifeMemoryBuff.cs`

| 效果 | 数值 | 说明 |
|------|------|------|
| 伤害加成 | +20% | 前世战斗经验 |
| 防御加成 | +15% | 前世受伤记忆 |
| 移速加成 | +10% | 前世闪避经验 |
| 龙公伤害加成 | +50% | 了解龙公弱点 |
| 弹幕预判 | 龙公弹幕显示预警线 | 前世记忆预判 |
| 持续时间 | 10分钟（游戏内） | 足够再次挑战 |

**弹幕预判实现**：
```csharp
// 在龙公弹幕的PreDraw中
if (Main.LocalPlayer.HasBuff(ModContent.BuffType<PastLifeMemoryBuff>()))
{
    // 绘制弹幕轨迹预测线
    Vector2 predictedPos = projectile.Center + projectile.velocity * 30f;
    Utils.DrawLine(spriteBatch, projectile.Center, predictedPos, Color.Yellow * 0.5f);
}
```

### 3.3 第二次战斗——可以获胜

**与首次战斗的区别**：

| 区别 | 首次 | 第二次 |
|------|------|--------|
| 前世记忆Buff | 无 | 有（+20%伤害/+15%防御/+50%对龙公伤害） |
| 弹幕预判 | 无 | 有（预警线显示） |
| 龙人寂灭 | 必死 | 可被春秋蝉回溯后继续战斗 |
| 盟友 | 无 | 根据阵营选择，可能有NPC助战 |
| 准备 | 无 | 玩家可以提前准备（制作抗性丹药/调整蛊虫配置） |

**第二次战斗策略建议**（通过NPC对话提示）：
- 冰塞川："龙公的龙人寂灭有10秒蓄力，那是他最脆弱的时候。"
- 影无邪："龙公的变化道吞噬弹幕，不要在近距离使用弹幕类蛊虫。"
- 黑楼兰："龙公的气压场会减速，带上加速类蛊虫。"

---

## 4. 宿命蛊碎裂

### 4.1 触发条件

击败龙公后自动触发。

### 4.2 世界事件

```
龙公被击败 → 世界震动
    ↓
天空出现巨大裂缝
    ↓
世界消息："宿命蛊……碎裂了！"
    ↓
红莲魔尊的布局完成（过场动画）
    ↓
宿命系统永久关闭
    ↓
世界永久改变
```

### 4.3 宿命系统关闭效果

**新增系统**：`Common/Systems/DestinySystem.cs`

```csharp
public class DestinySystem : ModSystem
{
    public static bool IsDestinyShattered;  // 宿命是否已碎裂

    // 宿命碎裂前：
    // - NPC有"宿命约束"：某些行为被限制
    // - 世界事件按"宿命"发展
    // - 玩家的选择受到"命运"影响

    // 宿命碎裂后：
    // - NPC行为更自由（更随机、更不可预测）
    // - 世界事件不再按宿命发展
    // - 玩家的选择真正自由
}
```

**NPC行为变化**：

| 变化 | 宿命碎裂前 | 宿命碎裂后 |
|------|-----------|-----------|
| NPC决策 | 受宿命约束，行为可预测 | 完全自由，行为不可预测 |
| NPC背叛 | 低概率（宿命约束忠诚） | 高概率（自由意志） |
| NPC成长 | 固定路径 | 随机成长 |
| NPC关系 | 相对稳定 | 动态变化 |
| 事件触发 | 按宿命时间线 | 随机触发 |

**实现**：
```csharp
// 在 GuMasterBase 的 CalculateAttitude 中
public override GuAttitude CalculateAttitude(AttitudeContext ctx)
{
    if (DestinySystem.IsDestinyShattered)
    {
        // 宿命碎裂后：态度计算更随机
        float randomness = Main.rand.NextFloat(-0.3f, 0.3f);
        ctx.BaseAttitude += randomness;
    }
    else
    {
        // 宿命约束：态度计算更稳定
        // 原有逻辑不变
    }
}
```

**世界永久改变**：

| 改变 | 说明 |
|------|------|
| 天空颜色 | 偶尔出现裂缝特效（永久） |
| NPC对话 | 所有NPC对话增加"宿命碎裂"相关内容 |
| 天劫变化 | 天劫难度+30%（天意不再约束天劫） |
| 运势系统 | 运势波动更大（不再被宿命稳定） |
| 事件频率 | 世界事件更频繁（宿命不再限制） |

---

## 5. 升仙系统

### 5.1 六转突破要求

| 要求 | 具体内容 |
|------|---------|
| 境界 | 五转巅峰（LevelStage=3, BreakthroughProgress=100%） |
| 击败龙公 | `DownBossSystem.downedLongGong == true` |
| 宿命碎裂 | `DestinySystem.IsDestinyShattered == true` |
| 升仙凭证 | 拥有"升仙凭证"物品 |
| 真元 | 真元满值 |
| 空窍 | 至少6个空窍槽位有蛊虫 |

### 5.2 血劫Boss战

**对应**：`HeavenTribulationSystem.BloodTribulation`（已有框架）

**血劫详细设计**：

| 属性 | 值 |
|------|-----|
| 总波数 | 6波 |
| 每波持续时间 | 15秒 |
| 波间间隔 | 5秒 |
| 基础伤害 | 80/波（逐波递增） |
| 特殊机制 | 血海幻影Boss |

**血海幻影Boss**：

| 属性 | 值 |
|------|-----|
| 生命 | 50000 |
| 伤害 | 100 |
| 防御 | 30 |
| 机制 | 分裂+再生+血道攻击 |

**血海幻影攻击**：

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 血刃 | 发射血色弹幕 | 80 | 每3秒 |
| 血海分裂 | 血量降至50%时分裂为2个 | - | 50%血量 |
| 血液吸取 | 命中玩家后恢复自身生命 | 60 | 每5秒 |
| 血爆 | 死亡时爆炸，范围伤害 | 150 | 死亡时 |

**血劫通过条件**：
- 存活6波天劫弹幕 + 击败血海幻影
- 天劫弹幕从天空落下（使用 `HeavenTribulationSystem` 已有逻辑）
- 血海幻影在第3波时出现

### 5.3 仙窍开启

**升仙成功后**：

```
血劫通过 → 世界消息："天地共鸣——你已升仙！"
    ↓
播放升仙动画（10秒）
  - 玩家身体发光
  - 天空出现仙光
  - 所有NPC仰望天空
    ↓
玩家境界变为六转
    ↓
仙窍开启（新系统解锁）
    ↓
真元系统转换为仙元系统
    ↓
世界永久改变（见§6）
```

### 5.4 仙元替代真元

**扩展**：`Common/Players/QiResourcePlayer.cs`

```csharp
public class QiResourcePlayer : ModPlayer
{
    // 现有
    public float QiCurrent;
    public float QiMax;

    // 新增
    public bool IsImmortal;          // 是否已升仙
    public float ImmortalQiCurrent;  // 仙元当前值
    public float ImmortalQiMax;      // 仙元最大值

    // 仙元 = 真元 × 100
    // 升仙时：ImmortalQiMax = QiMax * 100
    // 仙元恢复速度 = 真元恢复速度 × 10
}
```

**仙元与真元的区别**：

| 属性 | 真元 | 仙元 |
|------|------|------|
| 数值 | ~1000 | ~100000 |
| 恢复速度 | 5/秒 | 50/秒 |
| 消耗速度 | 正常 | 蛊虫消耗×0.1（仙元效率更高） |
| 获取方式 | 修炼/元石 | 修炼/仙元石/仙窍产出 |
| 上限增长 | 境界提升 | 境界提升+仙窍扩建 |

---

## 6. Post-升仙世界变化

### 6.1 新维度：仙窍内部

**新文件**：`Common/SubWorlds/ImmortalAperture.cs`

**使用SubworldLibrary创建子世界**

| 属性 | 值 |
|------|-----|
| 世界大小 | 400×300格 |
| 初始状态 | 空旷虚空（深色背景） |
| 进入方式 | 使用"仙窍入口"物品 |
| 建设方式 | 放置功能Tile |

**仙窍内部区域**：

| 区域 | 解锁条件 | 功能 | Tile |
|------|---------|------|------|
| 灵田 | 默认解锁 | 种植仙草 | `灵田Tile` |
| 蛊室 | 默认解锁 | 培养仙蛊 | `蛊室Tile` |
| 炼丹房 | 消耗仙元石×50 | 炼制仙丹 | `炼丹炉Tile` |
| 阵法殿 | 消耗仙元石×100 | 布置防御阵法 | `阵法台Tile` |
| 藏蛊阁 | 消耗仙元石×80 | 存储蛊虫 | `藏蛊架Tile` |
| 修炼室 | 消耗仙元石×30 | 加速修炼 | `修炼台Tile` |

**仙窍产出**：

| 区域 | 产出 | 频率 |
|------|------|------|
| 灵田 | 仙草材料 | 每3游戏日 |
| 蛊室 | 仙蛊培养进度 | 每5游戏日 |
| 炼丹房 | 仙丹（需手动操作） | 手动 |
| 修炼室 | 仙元恢复+100% | 持续 |

**仙窍升级**：

| 等级 | 仙窍大小 | 解锁区域 | 消耗 |
|------|---------|---------|------|
| 1 | 400×300 | 灵田+蛊室 | 默认 |
| 2 | 500×400 | +炼丹房 | 仙元石×50 |
| 3 | 600×500 | +阵法殿 | 仙元石×100 |
| 4 | 700×600 | +藏蛊阁 | 仙元石×200 |
| 5 | 800×700 | +修炼室 | 仙元石×500 |

### 6.2 仙蛊炼制

**扩展**：`Common/Systems/WeaponCraftingSystem.cs`

**仙蛊与凡蛊的区别**：

| 属性 | 凡蛊 | 仙蛊 |
|------|------|------|
| 消耗 | 真元 | 仙元 |
| 伤害 | 基础 | 基础×10 |
| 效果 | 单一 | 复合（多种道痕效果） |
| 进化 | 有上限 | 可无限进化 |
| 损坏 | 死亡可能丢失 | 死亡不丢失（仙级绑定） |

**仙蛊炼制配方示例**：

| 仙蛊 | 材料 | 仙元消耗 | 效果 |
|------|------|---------|------|
| 仙焰蛊 | 火道六转蛊×1 + 太古之光×3 + 仙元石×20 | 10000仙元 | 火道攻击，范围+300% |
| 仙冰蛊 | 冰道六转蛊×1 + 太古冰核×3 + 仙元石×20 | 10000仙元 | 冰道攻击+冻结 |
| 仙风蛊 | 风道六转蛊×1 + 天柱风×3 + 仙元石×20 | 10000仙元 | 风道攻击+击退 |

### 6.3 太古荒兽

**新敌人类型**：太古荒兽（仙级敌人）

| 荒兽 | 生命 | 伤害 | 防御 | 掉落 |
|------|------|------|------|------|
| 太古火龙 | 30000 | 150 | 40 | 太古龙鳞、火道仙材 |
| 太古冰凤 | 25000 | 130 | 35 | 太古凤羽、冰道仙材 |
| 太古雷兽 | 20000 | 180 | 25 | 太古雷角、雷道仙材 |
| 太古土灵 | 40000 | 100 | 80 | 太古土核、土道仙材 |
| 太古虚空洞 | 15000 | 200 | 10 | 太古虚空碎片、虚空道仙材 |

**太古荒兽刷新条件**：
- 仅在升仙后出现
- 在世界特定区域（荒兽领地）刷新
- 每个游戏日刷新1~2只
- 击杀后3个游戏日才会再次刷新

### 6.4 尊者级NPC

**升仙后出现的特殊NPC**：

| NPC | 类型 | 功能 | 出现条件 |
|-----|------|------|---------|
| 红莲意志 | 特殊NPC | 提供宙道知识 | 宿命碎裂后 |
| 盗天残影 | 特殊NPC | 提供偷道知识 | 完成盗天真传 |
| 巨阳分身 | 特殊NPC | 提供运道知识 | 升仙后30日 |
| 幽魂残魂 | 特殊NPC | 提供魂道知识 | 仙窍等级≥3 |

---

## 7. 具体实现任务清单

### 7.1 宿命大战系统（优先级：🔴最高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S6-01 | 宿命大战世界事件系统 | `Common/Systems/DestinyWarSystem.cs` | 8h |
| S6-02 | 天庭浮岛生成（四柱） | `Common/Systems/HeavenIslandWorldGen.cs` | 6h |
| S6-03 | 浮岛守护者NPC（4种） | `Content/NPCs/HeavenCourt/Pillars/` | 8h |
| S6-04 | 浮岛小怪NPC（8种） | `Content/NPCs/HeavenCourt/Pillars/` | 8h |
| S6-05 | 阵营选择UI | `Common/UI/FactionChoiceUI/` | 4h |
| S6-06 | NPC军团混战AI | 扩展 `CombatBehavior.cs` | 6h |
| S6-07 | 天庭浮岛生物群落 | `Content/Biomes/HeavenIslandBiome.cs` | 3h |

### 7.2 龙公Boss战（优先级：🔴最高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S6-08 | 龙公Boss AI（四阶段） | 扩展 `LongGong.cs` | 12h |
| S6-09 | 龙公弹幕——气道（气旋弹/气压场/龙息/压缩） | `Content/Projectiles/LongGong/` | 6h |
| S6-10 | 龙公弹幕——变化道（龙爪/龙尾/幻影/吞噬） | `Content/Projectiles/LongGong/` | 6h |
| S6-11 | 龙人寂灭终极攻击 | `Content/Projectiles/LongGong/DragonExtinction.cs` | 4h |
| S6-12 | 龙公幻影分身NPC | `Content/NPCs/HeavenCourt/LongGongPhantom.cs` | 3h |

### 7.3 春秋蝉逆转（优先级：🔴最高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S6-13 | 宿命回溯机制 | 扩展 `ChunQiuChanPlayer.cs` | 6h |
| S6-14 | 世界回溯逻辑 | `Common/Systems/WorldRollbackSystem.cs` | 8h |
| S6-15 | 前世记忆Buff | `Content/Buffs/AddToSelf/Pobuff/PastLifeMemoryBuff.cs` | 3h |
| S6-16 | 弹幕预判系统 | 扩展 `GuProjectileInfo.cs` | 4h |
| S6-17 | 回溯过场动画 | `Common/Systems/RebirthCutscene.cs` | 4h |

### 7.4 宿命蛊碎裂（优先级：🟡高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S6-18 | 宿命系统 | `Common/Systems/DestinySystem.cs` | 5h |
| S6-19 | 宿命碎裂世界事件 | 扩展 `DestinyWarSystem.cs` | 4h |
| S6-20 | NPC自由意志AI调整 | 扩展 `GuMasterBase.cs` | 6h |
| S6-21 | 世界永久改变效果 | `Common/Systems/PostDestinyWorldChanges.cs` | 4h |

### 7.5 升仙系统（优先级：🟡高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S6-22 | 血劫Boss战（血海幻影） | `Content/NPCs/Boss/BloodSeaPhantom.cs` | 6h |
| S6-23 | 血劫弹幕 | `Content/Projectiles/BloodSeaPhantom/` | 4h |
| S6-24 | 升仙事件链 | `Common/Systems/AscensionSystem.cs` | 5h |
| S6-25 | 仙元系统 | 扩展 `QiResourcePlayer.cs` | 4h |
| S6-26 | 升仙动画 | `Common/Systems/AscensionCutscene.cs` | 3h |

### 7.6 Post-升仙内容（优先级：🟡高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S6-27 | 仙窍子世界 | `Common/SubWorlds/ImmortalAperture.cs` | 10h |
| S6-28 | 仙窍功能Tile（6种） | `Content/Tiles/ImmortalAperture/` | 6h |
| S6-29 | 仙窍升级系统 | 扩展 `ImmortalAperture.cs` | 4h |
| S6-30 | 仙蛊炼制系统 | 扩展 `WeaponCraftingSystem.cs` | 5h |
| S6-31 | 仙元石物品 | `Content/Items/Materials/ImmortalYuanStone.cs` | 1h |
| S6-32 | 太古荒兽NPC（5种） | `Content/NPCs/AncientBeasts/` | 10h |
| S6-33 | 尊者级NPC（4种） | `Content/NPCs/VenerableWills/` | 6h |

### 7.7 六转武器补充

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S6-34 | 六转武器（新增12个，达到80个目标） | `Content/Items/Weapons/Six/` | 12h |

**六转武器列表**：

| 编号 | 武器名称 | 道系 | 机制 |
|------|---------|------|------|
| S6-W01 | 仙焰蛊 | 火道 | 范围火道攻击+灼烧 |
| S6-W02 | 仙冰蛊 | 冰道 | 冰道攻击+冻结+碎裂 |
| S6-W03 | 仙风蛊 | 风道 | 风道攻击+击退+龙卷 |
| S6-W04 | 仙雷蛊 | 雷道 | 雷道攻击+连锁闪电 |
| S6-W05 | 龙鳞剑 | 气道 | 龙公掉落制作，气道终极 |
| S6-W06 | 宿命之丝 | 命运道 | 宿命碎裂后获得 |
| S6-W07 | 红莲之焰 | 宙道 | 红莲意志赠予 |
| S6-W08 | 盗天之影 | 偷道 | 盗天残影赠予 |
| S6-W09 | 血海之核 | 血道 | 血劫Boss掉落 |
| S6-W10 | 龙人遗甲 | 变化道 | 龙公掉落制作 |
| S6-W11 | 仙窍之钥 | 特殊 | 进入仙窍的钥匙 |
| S6-W12 | 升仙丹 | 炼丹 | 辅助升仙的丹药 |

### 7.8 DownBossSystem扩展

| 编号 | 任务 | 说明 |
|------|------|------|
| S6-35 | 新增 `downedLongGong` | 龙公击败标记 |
| S6-36 | 新增 `downedBloodSeaPhantom` | 血海幻影击败标记 |
| S6-37 | 新增 `destinyShattered` | 宿命碎裂标记 |
| S6-38 | 新增 `hasAscended` | 升仙标记 |
| S6-39 | 新增 `downedStarPillar` 等4个 | 四柱击败标记 |
| S6-40 | 新增 `destinyRebirthCount` | 宿命回溯次数 |

### 7.9 总工时估算

| 类别 | 工时 |
|------|------|
| 宿命大战系统 | 43h |
| 龙公Boss战 | 31h |
| 春秋蝉逆转 | 25h |
| 宿命蛊碎裂 | 19h |
| 升仙系统 | 22h |
| Post-升仙内容 | 42h |
| 六转武器 | 12h |
| **总计** | **~194h** |

---

## 附录A：关键剧情对话

### 龙公战前对话

```
龙公："你来了。我等这一天，已经等了很久。"
龙公："宿命不可违逆。你的一切挣扎，不过是宿命的安排。"
龙公："但……如果你能打破宿命，那就来试试吧。"
```

### 宿命碎裂对话

```
红莲意志（远方传来）："终于……宿命蛊碎了。"
红莲意志："我等待这一天，已经等了无数个轮回。"
红莲意志："方源……不，大爱仙尊，你的布局终于完成了。"
```

### 升仙对话

```
世界意志（天地共鸣）："凡人……你竟敢超越凡人的界限。"
世界意志："从今以后，你不再是凡人。"
世界意志："但仙人的道路，比凡人更加残酷。"
```

## 附录B：与现有系统的集成点

| 现有系统 | 集成方式 |
|---------|---------|
| `WorldStateMachine` | 新增宿命大战/升仙事件类型 |
| `DownBossSystem` | 新增6个标记 |
| `HeavenTribulationSystem` | 血劫Boss战（已有框架，需扩展） |
| `ChunQiuChanPlayer` | 宿命回溯机制（重大扩展） |
| `QiResourcePlayer` | 仙元系统（重大扩展） |
| `QiRealmPlayer` | 六转境界 |
| `KongQiaoPlayer` | 仙窍系统关联 |
| `GuMasterBase` | 宿命碎裂后AI调整 |
| `InheritanceSystem` | 注册升仙相关真传 |
| `WeaponCraftingSystem` | 仙蛊炼制 |
| `SubworldLibrary` | 仙窍子世界 |
| `EventBus` | 新增宿命碎裂/升仙事件 |
| `DaoHenConflictSystem` | 新增命运道/变化道道痕 |
| `FactionAllegianceSystem` | 宿命大战阵营选择 |
| `PlayerStateSnapshot` | 世界回溯快照 |
