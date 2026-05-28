# 四转·义天山大战 — 详细实现计划

> **对应Terraria阶段**：中期Hardmode（Plantera / Golem）
> **对应小说章节**：第500~800章（义天山篇）
> **转数范围**：四转初期 → 四转巅峰
> **前置条件**：三转完成 + 击败三王传承中至少一个Boss
> **预估时长**：8~12小时

---

## 1. 义天山事件触发

### 1.1 触发条件

| 条件 | 具体要求 | 检测方式 |
|------|---------|---------|
| 境界要求 | 玩家达到四转 | `QiRealmPlayer.GuLevel >= 4` |
| Boss前置 | 击败三王传承中至少一个 | `DownBossSystem` 新增 `downedIronBoneKing` / `downedPoisonSnakeKing` / `downedPhantomKing`，至少一个为true |
| 时间前置 | Hardmode开启后至少经过5个游戏日 | `WorldTimeHelper.CurrentDay > hardmodeStartDay + 5` |
| 剧情前置 | 完成南疆散修营地主线任务 | `QuestSystem` 中 `southBorderMainQuest` 状态为Completed |

### 1.2 触发流程

```
满足所有条件 → 玩家进入地表时触发
    ↓
屏幕震动 + 天空变为暗红色（持续10秒）
    ↓
世界消息："义天山异变——各方势力云集南疆！"
    ↓
世界地图上生成义天山区域（新生物群落）
    ↓
玩家获得"义天山令"物品（传送道具）
    ↓
WorldStateMachine 触发 WorldEventType.YiTianShanEvent
    ↓
义天山入口NPC出现（影宗外围使者）
```

### 1.3 义天山生物群落

**新文件**：`Content/Biomes/YiTianShanBiome.cs`

| 属性 | 值 |
|------|-----|
| 生物群落名称 | 义天山 |
| 类型 | ModBiome（Surface + Underground） |
| 方块判定 | 义天山石砖（新Tile）≥ 100块 |
| 背景样式 | 暗红天空 + 战火硝烟粒子 |
| 水色 | 暗红色 |
| 音乐 | 战斗BGM（需新增Assets/Music/YiTianShan.wav） |

**义天山区域生成规则**：
- 在世界地表随机位置生成，距出生点≥500格
- 区域大小：宽200格 × 高150格
- 三层结构：外围战场（0~50格深）→ 中层战场（50~100格深）→ 内部迷宫（100~150格深）
- 使用 `WorldGen` 在 `ModSystem.PostWorldGen()` 中生成

**新Tile**：
- `义天山石砖`（YiTianShanBrick）：不可破坏，深灰色
- `义天山黑岩`（YiTianShanDarkRock）：不可破坏，黑色
- `义天山入口`（YiTianShanEntrance）：可交互Tile，传送至义天山内部

### 1.4 WorldEventType 扩展

在 `GuWorldSystem.cs` 的 `WorldEventType` 枚举中新增：

```csharp
YiTianShanEvent,         // 义天山事件
DaTongFengEvent,         // 大同风世界事件
ZhenChuanCompetition,    // 真传争夺事件
```

在 `WorldStateMachine.ExecuteEventLogic()` 中新增对应分支。

---

## 2. 义天山三层副本

### 2.1 第一层：外围战场（混战）

**场景描述**：义天山外围，各方势力蛊师混战。玩家需要在混乱中找到通往第二层的入口。

**新NPC**：

| NPC名称 | 类型 | 转数 | 阵营 | 行为 |
|---------|------|------|------|------|
| 影宗守卫 | 敌对 | 四转 | 影宗 | 巡逻+攻击非影宗玩家 |
| 正道蛊师 | 中立→敌对 | 三转~四转 | 正道各家族 | 根据玩家声望决定态度 |
| 魔道蛊师 | 敌对 | 三转~四转 | 魔道 | 主动攻击所有非魔道 |
| 散修掠夺者 | 敌对 | 三转 | 散修 | 偷窃玩家物品 |
| 伤重蛊师 | 中立 | 二转~三转 | 无 | 受伤NPC，可治疗/掠夺 |

**文件结构**：
```
Content/NPCs/YiTianShan/
├── ShadowSectGuard.cs        // 影宗守卫
├── RighteousGuMaster.cs      // 正道蛊师
├── DemonicGuMaster.cs        // 魔道蛊师
├── ScavengerGuMaster.cs      // 散修掠夺者
└── WoundedGuMaster.cs        // 伤重蛊师
```

**混战机制**：
- NPC之间会互相攻击（不同阵营之间）
- 玩家进入区域后，每30秒刷新一波敌人（3~5个）
- 击杀敌人获得"战功"（新货币，用于兑换义天山专属物品）
- 玩家可以选择加入某一阵营（影响NPC态度）

**战功系统**（扩展 `BountySystem`）：
```csharp
public static int YiTianShanWarPoints;  // 义天山战功
public static Dictionary<int, int> FactionWarPoints;  // 各势力战功
```

**入口解锁条件**：
- 击杀至少10个外围敌人
- 或与影宗使者对话获得通行证
- 或使用偷道蛊虫直接传送（消耗真元500）

### 2.2 第二层：百日大战（凤九歌 vs 秦百胜）

**场景描述**：义天山核心战场，凤九歌与秦百胜正在进行史诗级对决。玩家可以旁观或选择助战。

**核心机制**：NPC vs NPC 战斗

**实现方案**：使用两个Boss级NPC同时存在，互相攻击

**凤九歌NPC**（已有 `Content/NPCs/SouthBorder/FengJiuGe.cs`，需扩展为Boss）：

| 属性 | 值 |
|------|-----|
| 生命 | 80000 |
| 伤害 | 120 |
| 防御 | 40 |
| AI类型 | 自定义Boss AI |
| 阵营 | 影宗（亚仙尊级） |

**秦百胜NPC**（已有 `Content/NPCs/ShadowSect/QinBaiSheng.cs`，需扩展为Boss）：

| 属性 | 值 |
|------|-----|
| 生命 | 75000 |
| 伤害 | 110 |
| 防御 | 35 |
| AI类型 | 自定义Boss AI |
| 阵营 | 影宗（七转巅峰） |

**战斗流程**：

```
玩家进入第二层 → 触发过场动画（5秒）
    ↓
凤九歌与秦百胜开始战斗（NPC互殴）
    ↓
每30秒，弹出选择框：
  "凤九歌与秦百胜激战正酣，你选择——"
  A. 协助凤九歌（对秦百胜造成伤害）
  B. 协助秦百胜（对凤九歌造成伤害）
  C. 旁观（不参与，但可能被波及）
    ↓
战斗持续约5分钟（游戏内时间）
    ↓
秦百胜血量降至30% → 触发"大同风"自爆事件
    ↓
秦百胜死亡 → 大同风爆发（见§5）
    ↓
凤九歌重伤 → 玩家可选择：
  A. 救助凤九歌（获得音道真传碎片）
  B. 趁机攻击凤九歌（Boss战，极难）
  C. 离开（进入第三层）
```

**NPC互殴实现**：
- 两个Boss NPC使用 `npc.target` 指向对方
- 通过 `ModNPC.AI()` 控制攻击行为
- 弹幕使用 `hostile = true` 但通过 `npc.owner` 判断友方
- 玩家弹幕对两个Boss都有效（旁观时伤害减半）

### 2.3 第三层：真传争夺（盗天迷宫）

**场景描述**：义天山内部，盗天魔尊留下的迷宫。随机生成，包含多个真传试炼。

**实现方案**：使用 SubworldLibrary 创建子世界

**新文件**：`Common/SubWorlds/DaoTianMaze.cs`

**迷宫生成算法**：
- 使用递归回溯法生成迷宫
- 迷宫大小：80×60格
- 每个房间大小：8×8格
- 房间类型随机：空房间/陷阱房间/宝箱房间/Boss房间/谜题房间

**房间类型详细设计**：

| 房间类型 | 出现概率 | 内容 |
|---------|---------|------|
| 空房间 | 30% | 无特殊内容，可能有装饰 |
| 陷阱房间 | 25% | 地面陷阱（毒雾/落石/蛊虫群），需通过走位避开 |
| 宝箱房间 | 20% | 包含真传碎片/元石/蛊虫材料 |
| Boss房间 | 10% | 守护者Boss（见下文），击败后获得真传 |
| 谜题房间 | 15% | 需要使用特定道系蛊虫解谜（如用火道烧毁荆棘、用冰道冻结水面） |

**守护者Boss**：

| Boss名称 | 转数 | 机制 | 掉落 |
|---------|------|------|------|
| 盗天幻影 | 五转 | 随机变化为玩家已击败的Boss的攻击模式 | 盗天真传碎片 |
| 迷宫守卫 | 四转 | 召唤迷宫墙壁困住玩家+穿刺攻击 | 随机三转蛊虫 |

**迷宫时间限制**：
- 进入后限时10分钟（游戏内时间）
- 超时后迷宫崩塌，强制传送出义天山
- 迷宫内无法使用春秋蝉回溯

---

## 3. 凤九歌Boss战

**NPC文件**：扩展 `Content/NPCs/SouthBorder/FengJiuGe.cs`

### 3.1 基础属性

| 属性 | 值 | 说明 |
|------|-----|------|
| 生命 | 120000 | 亚仙尊级 |
| 伤害 | 150 | 基础攻击 |
| 防御 | 50 | 高防御 |
| 击退抗性 | 100% | 不可击退 |
| AI风格 | 自定义 | 三阶段Boss AI |

### 3.2 阶段设计

**第一阶段（100%~60%生命）——音道试探**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 音波弹 | 向玩家发射3发扇形音波弹 | 80 | 每2秒 |
| 回声定位 | 标记玩家位置，1秒后该位置爆炸 | 120 | 每5秒 |
| 共鸣场 | 在地面生成共鸣区域，踩上去受到持续伤害 | 40/秒 | 每8秒 |

**弹幕实现**：
- 音波弹：使用 `AimBehavior` + `WaveBehavior` + `VoiceTrailBehavior`
- 回声定位：使用 `StationaryBehavior`（1秒后 `ExplosionKillBehavior`）
- 共鸣场：使用 `RegionSpawnBehavior` + `PeriodicDustBehavior`

**第二阶段（60%~30%生命）——大风歌**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 大风歌 | 全屏声波攻击，持续3秒 | 60/秒 | 每15秒 |
| 命运歌·初 | 给玩家施加"命运标记"debuff，每秒增加5点伤害 | 5~50/秒 | 每20秒 |
| 音刃风暴 | 8方向同时发射音刃弹幕 | 100 | 每10秒 |

**大风歌详细机制**：
```
触发 → 屏幕震动 + 音波粒子特效（全屏）
    ↓
所有玩家受到持续伤害（60/秒，持续3秒 = 180总伤害）
    ↓
玩家无法跳跃（debuff：音压）
    ↓
地面出现音波纹（视觉效果）
    ↓
3秒后结束，凤九歌进入5秒冷却
```

**实现**：
- 使用 `ModProjectile` 创建全屏声波弹幕
- 弹幕覆盖全屏，`tileCollide = false`，`penetrate = -1`
- 配合 `DebuffOnHitBehavior` 施加"音压"debuff
- 视觉效果使用 `PeriodicDustBehavior` + `VoiceDust`

**命运歌详细机制**：
```
触发 → 玩家获得"命运标记"debuff（持续30秒）
    ↓
每秒伤害递增：5 → 10 → 15 → ... → 150
    ↓
debuff可被"净化类"蛊虫移除
    ↓
若30秒内未移除，最后一击造成300点爆发伤害
```

**实现**：
- 新增Buff：`命运标记`（FateMarkDebuff）
- Buff内部计时器每秒增加伤害值
- 使用 `ModPlayer.UpdateBadLifeRegen()` 实现持续伤害
- 使用 `ModPlayer.PreKill()` 实现爆发伤害

**第三阶段（30%~0%生命）——命运终曲**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 命运歌·极 | 命运标记伤害翻倍 | 10~300/秒 | 立即 |
| 绝响 | 全屏大爆炸（3秒蓄力，需打断） | 500 | 每25秒 |
| 音道领域 | 凤九歌周围30格内所有弹幕被偏转 | - | 持续 |
| 凤鸣九天 | 终极攻击：9发追踪音波弹 | 200 | 每20秒 |

**绝响打断机制**：
```
凤九歌开始蓄力（3秒倒计时显示在屏幕上）
    ↓
蓄力期间凤九歌防御降为0
    ↓
玩家需在3秒内对凤九歌造成≥5000伤害
    ↓
成功打断：凤九歌眩晕5秒，受到双倍伤害
失败：全屏500伤害爆炸
```

### 3.3 掉落物

| 物品 | 概率 | 数量 | 说明 |
|------|------|------|------|
| 音道真传碎片 | 100% | 1~3 | 合成音道真传的材料 |
| 凤羽 | 50% | 1 | 制作音道五转武器 |
| 亚仙尊遗物 | 10% | 1 | 制作特殊饰品 |
| 元石 | 100% | 30~50 | 基础货币 |

---

## 4. 秦百胜Boss战

**NPC文件**：扩展 `Content/NPCs/ShadowSect/QinBaiSheng.cs`

### 4.1 基础属性

| 属性 | 值 | 说明 |
|------|-----|------|
| 生命 | 90000 | 七转巅峰 |
| 伤害 | 130 | 基础攻击 |
| 防御 | 45 | 中等防御 |
| 击退抗性 | 95% | 几乎不可击退 |
| AI风格 | 自定义 | 两阶段 + 自爆 |

### 4.2 阶段设计

**第一阶段（100%~30%生命）——偷道攻击**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 偷袭 | 瞬移至玩家背后攻击 | 150 | 每4秒 |
| 偷取真元 | 命中后偷取玩家100点真元 | 80 | 每8秒 |
| 偷取蛊虫 | 命中后有5%概率使玩家空窍中一只蛊虫暂时失效 | 80 | 被动 |
| 偷天换日 | 交换玩家与秦百胜的位置 | 0 | 每15秒 |

**偷取真元实现**：
```csharp
// 在秦百胜的 OnHitPlayer 方法中
var qiPlayer = target.GetModPlayer<QiResourcePlayer>();
if (qiPlayer.QiCurrent >= 100f)
{
    qiPlayer.QiCurrent -= 100f;
    // 秦百胜恢复生命
    npc.life = Math.Min(npc.life + 500, npc.lifeMax);
    Main.NewText("秦百胜偷取了你的真元！", Color.Purple);
}
```

**偷取蛊虫实现**：
```csharp
// 在秦百胜的 OnHitPlayer 方法中
if (Main.rand.NextFloat() < 0.05f)
{
    var kongQiao = target.GetModPlayer<KongQiaoPlayer>();
    var activeSlots = kongQiao.Slots.Where(s => s.IsActive).ToList();
    if (activeSlots.Count > 0)
    {
        var slot = activeSlots[Main.rand.Next(activeSlots.Count)];
        slot.IsActive = false;
        slot.SuppressTimer = 300; // 5秒后恢复
        Main.NewText("秦百胜偷取了你的蛊虫之力！", Color.Purple);
    }
}
```

**第二阶段（30%~0%生命）——大同风蓄力**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 大同风·前奏 | 秦百胜开始发光，防御力逐渐提升 | - | 持续 |
| 偷道·极限 | 偷袭频率翻倍 | 180 | 每2秒 |
| 自爆蓄力 | 秦百胜血量越低，自爆倒计时越快 | - | 被动 |

**自爆触发条件**：
- 秦百胜血量降至0%时触发
- 或战斗超过5分钟后强制触发

### 4.3 掉落物

| 物品 | 概率 | 数量 | 说明 |
|------|------|------|------|
| 偷道真传碎片 | 100% | 1~2 | 合成偷道真传的材料 |
| 秦百胜的遗物 | 30% | 1 | 制作偷道五转武器 |
| 大同风碎片 | 50% | 1 | 大同风相关物品材料 |
| 元石 | 100% | 20~40 | 基础货币 |

---

## 5. 大同风世界事件

### 5.1 触发条件

秦百胜死亡后自动触发，无论玩家是否参与秦百胜战斗。

### 5.2 事件参数

| 参数 | 值 | 说明 |
|------|-----|------|
| 持续时间 | 120秒 | 游戏内时间 |
| 影响范围 | 全世界 | 所有玩家和NPC |
| 伤害频率 | 每秒 | 持续伤害 |
| 基础伤害 | 15/秒 | 可被防御减免 |
| 最大伤害 | 50/秒 | 随时间递增 |

### 5.3 事件效果

**1. 持续伤害**：
```
伤害公式：baseDamage * (1 + elapsedSeconds / 120)
0秒：15/秒
30秒：18.75/秒
60秒：22.5/秒
90秒：26.25/秒
120秒：30/秒
```

**实现**：
```csharp
// DaTongFengEvent ModSystem
public override void PreUpdateWorld()
{
    if (!isActive) return;
    elapsedTicks++;

    if (elapsedTicks % 60 == 0) // 每秒
    {
        float damage = 15f * (1f + (float)elapsedTicks / (120f * 60f));
        foreach (Player player in Main.player)
        {
            if (player.active && !player.dead)
            {
                player.Hurt(PlayerDeathReason.ByCustomCause("大同风的毁灭之力"),
                    (int)damage, 0, false, false, 0, false);
            }
        }
    }
}
```

**2. 视野降低**：
- 屏幕边缘添加暗红色滤镜
- 可视距离从正常降至60%
- 使用 `ModSystem.PostDrawInterface()` 绘制全屏半透明覆盖层

**3. 弹幕风暴**：
- 每3秒从随机方向发射一波弹幕（10~20发）
- 弹幕类型：风刃（使用 `WindTrailBehavior`）
- 弹幕伤害：30~60
- 弹幕速度：3~6

**4. NPC行为变化**：
- 所有城镇NPC进入恐慌状态，跑向安全区域
- 敌对NPC停止攻击，也尝试躲避
- 部分NPC可能在大同风中死亡（永久死亡）

### 5.4 大同风结束后

- 义天山区域部分崩塌（方块随机消失）
- 世界消息："大同风消散了……义天山已面目全非"
- 解锁第三层入口（盗天迷宫）
- 部分NPC在大同风中死亡，触发 `NPCDeathEvent` 和 `RoleVacancyEvent`

---

## 6. 真传系统

### 6.1 系统概述

真传是蛊师世界中最珍贵的传承，包含高阶蛊方、功法、阵法等。真传系统基于已有的 `InheritanceSystem` 进行扩展。

### 6.2 真传碎片

**新增物品**：

| 碎片名称 | 来源 | 合成真传 | 数量需求 |
|---------|------|---------|---------|
| 红莲真传碎片·时 | 义天山迷宫Boss | 红莲真传 | 5 |
| 红莲真传碎片·空 | 义天山宝箱 | 红莲真传 | 5 |
| 盗天真传碎片·偷 | 盗天迷宫Boss | 盗天真传 | 5 |
| 盗天真传碎片·虚 | 盗天迷宫谜题 | 盗天真传 | 5 |
| 音道真传碎片 | 凤九歌Boss掉落 | 音道真传 | 3 |
| 偷道真传碎片 | 秦百胜Boss掉落 | 偷道真传 | 3 |

**文件结构**：
```
Content/Items/Recipes/
├── RedLotusShard_Time.cs      // 红莲真传碎片·时
├── RedLotusShard_Void.cs      // 红莲真传碎片·空
├── DaoTianShard_Steal.cs      // 盗天真传碎片·偷
├── DaoTianShard_Illusion.cs   // 盗天真传碎片·虚
├── SoundDaoShard.cs           // 音道真传碎片
├── StealDaoShard.cs           // 偷道真传碎片
├── RedLotusInheritance.cs     // 红莲真传（完整）
├── DaoTianInheritance.cs      // 盗天真传（完整）
├── SoundDaoInheritance.cs     // 音道真传（完整）
└── StealDaoInheritance.cs     // 偷道真传（完整）
```

### 6.3 红莲真传（时间回溯）

**效果**：使用后获得"红莲时回"主动技能

**技能机制**：
```
激活 → 记录当前玩家状态（生命/真元/位置/Buff）
    ↓
5秒内再次激活 → 回溯到记录的状态
    ↓
冷却时间：60秒
消耗真元：300
```

**实现**：
```csharp
// RedLotusInheritance ModItem
public override void UseItem(Player player)
{
    var redLotusPlayer = player.GetModPlayer<RedLotusPlayer>();
    if (!redLotusPlayer.HasSnapshot)
    {
        redLotusPlayer.TakeSnapshot();
        Main.NewText("时间印记已刻下……", Color.OrangeRed);
    }
    else
    {
        redLotusPlayer.RestoreSnapshot();
        Main.NewText("时光倒流！", Color.OrangeRed);
    }
}
```

**新增ModPlayer**：`Common/Players/RedLotusPlayer.cs`

```csharp
public class RedLotusPlayer : ModPlayer
{
    public bool HasSnapshot;
    public int SnapshotLife;
    public float SnapshotQi;
    public Vector2 SnapshotPosition;
    public List<int> SnapshotBuffs;
    public int CooldownTimer;

    public void TakeSnapshot() { /* 记录状态 */ }
    public void RestoreSnapshot() { /* 恢复状态 */ }
}
```

### 6.4 盗天真传（偷取）

**效果**：使用后获得"盗天偷取"主动技能

**技能机制**：
```
对准NPC使用 → 尝试偷取NPC的物品
    ↓
成功率 = 30% + (偷道道痕 / 100)%
    ↓
成功：随机获得NPC携带的物品
失败：NPC变为敌对
    ↓
冷却时间：30秒
消耗真元：200
```

**实现**：扩展 `SearchSystem`，添加"偷取"搜索类型。

### 6.5 合成系统

在 `RecipeSystem.cs` 中添加真传合成配方：

```csharp
// 红莲真传 = 5×红莲碎片·时 + 5×红莲碎片·空 + 1000真元
CreateRecipe(1)
    .AddIngredient<RedLotusShard_Time>(5)
    .AddIngredient<RedLotusShard_Void>(5)
    .AddCondition(new Condition("需要1000真元", () =>
        Main.LocalPlayer.GetModPlayer<QiResourcePlayer>().QiCurrent >= 1000f))
    .Register();
```

---

## 7. 影宗NPC互动

### 7.1 砚石老人

**已有文件**：`Content/NPCs/ShadowSect/YanShiLaoRen.cs`

**角色定位**：影宗核心成员，控制白凝冰的幕后黑手

**互动内容**：

| 对话选项 | 条件 | 结果 |
|---------|------|------|
| "白凝冰在哪里？" | 完成义天山第一层 | 砚石老人透露白凝冰被控制的信息 |
| "我愿意加入影宗" | 玩家声望<0（恶名） | 加入影宗阵营，获得影宗任务 |
| "你该死！" | 玩家正道声望>50 | 砚石老人变为敌对，触发Boss战 |
| "告诉我真传的位置" | 玩家与影宗关系>30 | 获得盗天迷宫入口线索 |

**砚石老人Boss战**（可选Boss，=Plantera级）：

| 属性 | 值 |
|------|-----|
| 生命 | 60000 |
| 伤害 | 100 |
| 防御 | 55 |
| 机制 | 阵法困敌+控制白凝冰 |

**阵法困敌机制**：
```
砚石老人在地面绘制阵法（3秒蓄力）
    ↓
阵法激活：以砚石老人为中心20格范围内
    ↓
范围内玩家：移动速度-50%，每秒受到30伤害
    ↓
阵法持续10秒，可被破坏（攻击阵眼Tile）
```

**控制白凝冰机制**：
```
砚石老人血量<50%时召唤白凝冰（NPC）
    ↓
白凝冰被控制，攻击玩家
    ↓
玩家可以攻击白凝冰（击败后解除控制）
    ↓
或攻击砚石老人（击败后白凝冰自动解除控制）
    ↓
若白凝冰被击败：白凝冰死亡，砚石老人暴怒（伤害+50%）
若砚石老人被击败：白凝冰恢复自由，给予玩家感谢奖励
```

### 7.2 影无邪

**已有文件**：`Content/NPCs/ShadowSect/YingWuXie.cs`

**角色定位**：影宗少主，心机深沉

**互动内容**：

| 对话选项 | 条件 | 结果 |
|---------|------|------|
| "影宗的目的是什么？" | 完成义天山事件 | 影无邪透露影宗对抗天庭的计划 |
| "我需要力量" | 玩家四转以上 | 影无邪提供"影蛊"（特殊蛊虫，可隐身） |
| "你不可信" | 任何条件 | 影无邪好感-20，但不会立即敌对 |
| "合作" | 玩家与影宗关系>50 | 解锁影宗专属任务线 |

**影无邪的阴谋**（隐藏事件链）：
```
与影无邪合作 → 获得影宗任务
    ↓
完成3个影宗任务后 → 影无邪要求玩家"牺牲"一只蛊虫
    ↓
拒绝：影无邪好感-30，但不会背叛
接受：失去一只蛊虫，但获得"影宗密令"（解锁隐藏商店）
    ↓
继续合作 → 影无邪在关键时刻可能背叛
（根据玩家因果业力值决定：恶业高则背叛，善业高则忠诚）
```

### 7.3 阵营选择系统

**新增系统**：`Common/Systems/FactionAllegianceSystem.cs`

```csharp
public enum FactionAllegiance
{
    None,           // 未选择
    Righteous,      // 正道
    Demonic,        // 魔道
    ShadowSect,     // 影宗
    Neutral,        // 中立
}

public class FactionAllegianceSystem : ModSystem
{
    public static FactionAllegiance PlayerAllegiance;

    // 阵营选择影响：
    // - NPC态度（正道NPC对魔道玩家敌对）
    // - 可接任务（阵营专属任务）
    // - 商店物品（阵营专属商品）
    // - Boss战（某些Boss只对特定阵营出现）
    // - 结局走向（影响后期剧情）
}
```

---

## 8. 具体实现任务清单

### 8.1 基础设施（优先级：🔴最高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S4-01 | 义天山生物群落 | `Content/Biomes/YiTianShanBiome.cs` | 4h |
| S4-02 | 义天山方块（石砖/黑岩/入口） | `Content/Tiles/YiTianShan/` | 3h |
| S4-03 | 义天山区域世界生成 | `Common/Systems/YiTianShanWorldGen.cs` | 6h |
| S4-04 | 义天山事件触发逻辑 | 扩展 `WorldStateMachine.cs` | 3h |
| S4-05 | 大同风世界事件 | `Common/Systems/DaTongFengEventSystem.cs` | 5h |
| S4-06 | 战功系统扩展 | 扩展 `BountySystem.cs` | 2h |
| S4-07 | 阵营选择系统 | `Common/Systems/FactionAllegianceSystem.cs` | 4h |

### 8.2 NPC与Boss（优先级：🔴最高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S4-08 | 凤九歌Boss AI（三阶段） | 扩展 `Content/NPCs/SouthBorder/FengJiuGe.cs` | 8h |
| S4-09 | 凤九歌弹幕（音波弹/大风歌/命运歌/绝响） | `Content/Projectiles/FengJiuGe/` | 6h |
| S4-10 | 秦百胜Boss AI（偷道+自爆） | 扩展 `Content/NPCs/ShadowSect/QinBaiSheng.cs` | 6h |
| S4-11 | 秦百胜弹幕（偷袭/偷取） | `Content/Projectiles/QinBaiSheng/` | 4h |
| S4-12 | 砚石老人Boss AI（阵法+白凝冰） | 扩展 `Content/NPCs/ShadowSect/YanShiLaoRen.cs` | 6h |
| S4-13 | 义天山外围NPC（5种） | `Content/NPCs/YiTianShan/` | 8h |
| S4-14 | 盗天迷宫守护者Boss | `Content/NPCs/YiTianShan/DaoTianPhantom.cs` | 4h |

### 8.3 副本系统（优先级：🟡高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S4-15 | 盗天迷宫子世界 | `Common/SubWorlds/DaoTianMaze.cs` | 10h |
| S4-16 | 迷宫生成算法 | `Common/SubWorlds/DaoTianMazeGenerator.cs` | 6h |
| S4-17 | 迷宫房间系统（5种房间） | `Common/SubWorlds/DaoTianMazeRooms.cs` | 8h |
| S4-18 | 迷宫谜题系统 | `Common/SubWorlds/DaoTianMazePuzzles.cs` | 4h |

### 8.4 真传系统（优先级：🟡高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S4-19 | 真传碎片物品（6种） | `Content/Items/Recipes/` | 3h |
| S4-20 | 完整真传物品（4种） | `Content/Items/Recipes/` | 4h |
| S4-21 | 红莲真传技能（时间回溯） | `Common/Players/RedLotusPlayer.cs` | 5h |
| S4-22 | 盗天真传技能（偷取） | 扩展 `SearchSystem.cs` | 4h |
| S4-23 | 真传合成配方 | 扩展 `RecipeSystem.cs` | 2h |
| S4-24 | 真传UI界面 | `Common/UI/InheritanceUI/` | 4h |

### 8.5 Buff与Debuff（优先级：🟡高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S4-25 | 命运标记Debuff | `Content/Buffs/AddToSelf/Debuff/FateMarkDebuff.cs` | 2h |
| S4-26 | 音压Debuff | `Content/Buffs/AddToSelf/Debuff/SonicPressureDebuff.cs` | 1h |
| S4-27 | 大同风Debuff | `Content/Buffs/AddToSelf/Debuff/DaTongFengDebuff.cs` | 2h |
| S4-28 | 蛊虫失效Debuff | `Content/Buffs/AddToSelf/Debuff/GuSuppressedDebuff.cs` | 2h |
| S4-29 | 影蛊隐身Buff | `Content/Buffs/AddToSelf/Pobuff/ShadowGuStealthBuff.cs` | 2h |

### 8.6 剧情与对话（优先级：🟡高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S4-30 | 砚石老人对话树 | `Content/NPCs/ShadowSect/YanShiLaoRenDialogue.cs` | 3h |
| S4-31 | 影无邪对话树+阴谋事件链 | `Content/NPCs/ShadowSect/YingWuXieDialogue.cs` | 4h |
| S4-32 | 凤九歌战前/战后对话 | `Content/NPCs/SouthBorder/FengJiuGeDialogue.cs` | 2h |
| S4-33 | 义天山过场动画系统 | `Common/Systems/CutsceneSystem.cs` | 6h |

### 8.7 音效与特效（优先级：🟢中）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S4-34 | 义天山BGM | `Assets/Music/YiTianShan.wav` | 外部 |
| S4-35 | 大风歌音效 | `Assets/Music/DaFengGe.wav` | 外部 |
| S4-36 | 义天山粒子效果 | 扩展 `Dusts/` | 3h |
| S4-37 | 大同风全屏特效 | `Common/Systems/DaTongFengVisualSystem.cs` | 3h |

### 8.8 DownBossSystem扩展

| 编号 | 任务 | 说明 |
|------|------|------|
| S4-38 | 新增 `downedFengJiuGe` | 凤九歌击败标记 |
| S4-39 | 新增 `downedQinBaiSheng` | 秦百胜击败标记 |
| S4-40 | 新增 `downedYanShiLaoRen` | 砚石老人击败标记 |
| S4-41 | 新增 `yiTianShanCompleted` | 义天山事件完成标记 |
| S4-42 | 新增 `daTongFengSurvived` | 大同风存活标记 |

### 8.9 总工时估算

| 类别 | 工时 |
|------|------|
| 基础设施 | 27h |
| NPC与Boss | 36h |
| 副本系统 | 28h |
| 真传系统 | 22h |
| Buff与Debuff | 9h |
| 剧情与对话 | 15h |
| 音效与特效 | 9h |
| **总计** | **~146h** |

---

## 附录A：四转武器补充

当前四转武器40个，目标50个，需新增10个：

| 编号 | 武器名称 | 道系 | 类型 | 机制 |
|------|---------|------|------|------|
| S4-W01 | 大风歌·音刃 | 音道 | 近战 | 攻击发射音波 |
| S4-W02 | 命运丝线 | 命运道 | 远程 | 命中后标记敌人，下次攻击双倍伤害 |
| S4-W03 | 偷天手 | 偷道 | 近战 | 命中偷取敌人Buff |
| S4-W04 | 影遁蛊 | 影道 | 饰品 | 使用后隐身3秒 |
| S4-W05 | 大同风·残片 | 风道 | 远程 | 发射风刃弹幕 |
| S4-W06 | 红莲之火 | 火道 | 远程 | 命中后时间延迟爆炸 |
| S4-W07 | 盗天迷步 | 偷道 | 饰品 | 闪避后瞬移至敌人背后 |
| S4-W08 | 阵法·困 | 阵道 | 召唤 | 放置困敌阵法 |
| S4-W09 | 凤羽扇 | 音道 | 远程 | 扇形音波攻击 |
| S4-W10 | 义天山令 | 特殊 | 消耗品 | 传送至义天山 |

## 附录B：与现有系统的集成点

| 现有系统 | 集成方式 |
|---------|---------|
| `WorldStateMachine` | 新增义天山/大同风事件类型 |
| `DownBossSystem` | 新增4个Boss击败标记 |
| `InheritanceSystem` | 注册义天山相关真传 |
| `HeavenTribulationSystem` | 四转→五转心魔劫（已有） |
| `EventBus` | 新增义天山相关事件 |
| `KongQiaoPlayer` | 偷取蛊虫机制 |
| `QiResourcePlayer` | 偷取真元机制 |
| `ChunQiuChanPlayer` | 迷宫内禁止回溯 |
| `GuWorldPlayer` | 阵营选择影响声望 |
| `DaoHenConflictSystem` | 新增音道/偷道/命运道道痕 |
| `GuDropRegistry` | 新增义天山NPC掉落 |
| `DialogueTreeManager` | 新增影宗NPC对话树 |
| `SubworldLibrary` | 盗天迷宫子世界 |
