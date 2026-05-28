# 五转·北原争霸 — 详细实现计划

> **对应Terraria阶段**：后期Hardmode（Duke Fishron / Lunatic Cultist）
> **对应小说章节**：第800~1200章（北原篇）
> **转数范围**：五转初期 → 五转巅峰
> **前置条件**：四转完成 + 义天山事件完成
> **预估时长**：10~15小时

---

## 1. 北原冰原生物群落

### 1.1 生物群落定义

**新文件**：`Content/Biomes/NorthDesertBiome.cs`

| 属性 | 值 |
|------|-----|
| 生物群落名称 | 北原冰原 |
| 类型 | ModBiome（Surface + Underground） |
| 方块判定 | 冰原冻土（新Tile）≥ 150块 |
| 背景样式 | 灰白天空 + 飘雪粒子 + 远处冰山 |
| 水色 | 深蓝色（冰层下） |
| 音乐 | 北原BGM（需新增 `Assets/Music/NorthDesert.wav`） |
| 温度 | 极寒（影响玩家移速和真元消耗） |

### 1.2 冰原冻土方块

**新Tile**：`Content/Tiles/NorthDesert/PermafrostTile.cs`

| 属性 | 值 |
|------|-----|
| 名称 | 冰原冻土 |
| 挖掘难度 | 200%（比石块硬2倍） |
| 爆炸抗性 | 高（50%概率抵抗爆炸） |
| 放置效果 | 周围3格内温度降低 |
| 掉落 | 冰原冻土块（建筑材料） |

**其他新Tile**：
- `冰晶矿`（IceCrystalOre）：北原专属矿石，替代Hardmode后期矿石
- `雪松木`（SnowPineWood）：北原树木
- `冰原灵草`（NorthHerb）：可采集的药草
- `冻土平台`（PermafrostPlatform）：建筑用平台

### 1.3 暴风雪系统

**扩展**：`Common/Systems/WeatherSystem.cs`

新增暴风雪天气状态：

```csharp
// WeatherCondition 枚举新增
Blizzard,        // 暴风雪（北原专属）
AbsoluteZero,    // 绝对零度（极端天气，北原Boss战后触发）
```

**暴风雪效果**：

| 效果 | 数值 | 说明 |
|------|------|------|
| 视野降低 | 50% | 大雪粒子遮挡视线 |
| 移动速度 | -30% | 冰面打滑 |
| 真元消耗 | +50% | 寒冷加速消耗 |
| 弹幕偏转 | 10%概率 | 风雪影响弹道 |
| 刷新敌人 | 冰原荒兽 | 暴风雪期间刷新特殊敌人 |

**暴风雪触发条件**：
- 在北原生物群落中，每游戏日有30%概率触发暴风雪
- 暴风雪持续1~3个游戏日
- 击败冰塞川后，暴风雪频率降低至10%

**绝对零度效果**（极端天气）：

| 效果 | 数值 |
|------|------|
| 视野降低 | 80% |
| 移动速度 | -60% |
| 真元消耗 | +100% |
| 每秒伤害 | 10（冰属性） |
| 水面冻结 | 所有水面变为冰面 |

### 1.4 冰原荒兽

**新NPC**：

| NPC名称 | 类型 | 转数等效 | 生命 | 伤害 | 掉落 |
|---------|------|---------|------|------|------|
| 冰原狼 | 敌对 | 三转 | 800 | 45 | 冰狼皮、冰晶碎片 |
| 冻土巨熊 | 敌对 | 四转 | 2000 | 70 | 熊骨、力量蛊材料 |
| 雪域毒蛇 | 敌对 | 三转 | 600 | 55 | 蛇毒、冰毒蛊材料 |
| 冰晶蝶群 | 敌对 | 二转 | 300/只 | 30 | 冰晶粉、魅惑蛊材料 |
| 风雪巨鹰 | 敌对 | 四转 | 1500 | 60 | 鹰羽、风道蛊材料 |
| 太古冰兽 | 稀有Boss | 五转 | 8000 | 90 | 太古冰核、冰道真传碎片 |

**文件结构**：
```
Content/NPCs/NorthDesert/
├── IceFieldWolf.cs
├── FrostGiantBear.cs
├── SnowViper.cs
├── IceCrystalButterfly.cs
├── BlizzardEagle.cs
└── AncientIceBeast.cs
```

**太古冰兽详细设计**：

| 属性 | 值 |
|------|-----|
| 生命 | 8000 |
| 伤害 | 90 |
| 防御 | 30 |
| 生成条件 | 暴风雪期间，北原深处，5%概率替换普通敌人 |
| 攻击模式 | 冰息（扇形冰弹）+ 冰刺（地面突刺）+ 冰甲（受伤后防御+50%） |

---

## 2. 黑楼兰王庭

### 2.1 王庭城镇

**新文件**：`Content/Biomes/WangTingTown.cs`

| 属性 | 值 |
|------|-----|
| 类型 | 城镇生物群落（Town Biome） |
| 方块判定 | 王庭砖石（新Tile）≥ 80块 |
| NPC数量 | 8~12个城镇NPC |
| 安全区域 | 是（敌对NPC不刷新） |

**王庭NPC列表**：

| NPC名称 | 类型 | 功能 | 对话特点 |
|---------|------|------|---------|
| 黑楼兰 | 城镇NPC+可选Boss | 王庭之主，提供任务 | 霸道直率 |
| 毛里求 | 城镇NPC | 黑楼兰副手，商店 | 忠诚沉默 |
| 冰原铁匠 | 城镇NPC | 武器锻造 | 抱怨寒冷 |
| 雪域药师 | 城镇NPC | 丹药/药材 | 温和慈祥 |
| 王庭守卫 | 城镇NPC | 守卫+情报 | 严肃刻板 |
| 北原商人 | 城镇NPC | 特殊商品 | 精明算计 |
| 流浪蛊师 | 城镇NPC | 蛊虫交易 | 神秘莫测 |
| 逃亡者 | 城镇NPC | 情报/任务 | 惊恐不安 |

**文件结构**：
```
Content/NPCs/NorthDesert/Town/
├── NorthBlacksmith.cs
├── SnowPharmacist.cs
├── WangTingGuard.cs
├── NorthMerchant.cs
├── WanderingGuMaster.cs
└── FugitiveNPC.cs
```

### 2.2 黑楼兰NPC

**已有文件**：`Content/NPCs/NorthDesert/HeiLouLan.cs`

**城镇模式**：
- 初始为城镇NPC，提供王庭任务和商店
- 对话选项中有"挑战"选项，触发Boss战
- Boss战为可选内容，不影响主线进度

**对话树设计**：

| 对话选项 | 条件 | 结果 |
|---------|------|------|
| "王庭有什么任务？" | 首次对话 | 开启王庭任务线 |
| "北原的形势如何？" | 完成王庭任务×1 | 黑楼兰透露长生天的威胁 |
| "我想挑战你" | 玩家五转+完成王庭任务×3 | 触发Boss战 |
| "太白云生的情况？" | 完成黑楼兰Boss战 | 触发太白云生相关剧情 |
| "交易" | 任何条件 | 打开王庭商店 |

**王庭商店物品**：

| 物品 | 价格（元石） | 说明 |
|------|-------------|------|
| 冰原武器箱 | 50 | 随机开出北原武器 |
| 冻土丹 | 20 | 10分钟内冰属性抗性+50% |
| 霸道秘方 | 100 | 解锁力道五转蛊虫配方 |
| 王庭令 | 200 | 召唤黑楼兰助战（一次性） |
| 太古冰核 | 80 | 制作冰道高级蛊虫的材料 |

### 2.3 王庭任务系统

**扩展**：`Common/Systems/QuestSystem.cs`

新增王庭专属任务类型：

```csharp
public enum WangTingQuestType
{
    HuntIceBeast,       // 狩猎冰原荒兽
    EscortMerchant,     // 护送商队
    DefendWangTing,     // 防守王庭
    InvestigateChangShengTian,  // 调查长生天
    CollectHerbs,       // 采集冰原灵草
    SubjugateBandits,   // 剿灭北原盗匪
}
```

**任务链设计**（共7个任务）：

| 序号 | 任务名称 | 类型 | 目标 | 奖励 |
|------|---------|------|------|------|
| 1 | 冰原猎手 | 狩猎 | 击杀10只冰原荒兽 | 30元石+冰原武器 |
| 2 | 商队护送 | 护送 | 保护商队从王庭到边境 | 50元石+王庭声望 |
| 3 | 王庭之危 | 防守 | 击退3波长生天进攻 | 80元石+防御蛊虫 |
| 4 | 暗中调查 | 调查 | 潜入长生天营地获取情报 | 100元石+情报道具 |
| 5 | 灵草采集 | 采集 | 采集5株冰原灵草 | 60元石+丹药 |
| 6 | 盗匪清剿 | 剿灭 | 击败北原盗匪首领 | 120元石+力道蛊虫 |
| 7 | 黑楼兰的考验 | Boss | 击败黑楼兰（Boss战） | 200元石+力道真传碎片 |

---

## 3. 长生天势力

### 3.1 长生天NPC互动

**已有NPC文件**：
- `Content/NPCs/NorthDesert/BingSaiChuan.cs`（冰塞川）
- `Content/NPCs/NorthDesert/XueHuLaoZu.cs`（雪胡老祖）
- `Content/NPCs/NorthDesert/WuXingDaFaShi.cs`（五行大法师）

**长生天营地**：

**新文件**：`Content/Biomes/ChangShengTianCamp.cs`

| 属性 | 值 |
|------|-----|
| 类型 | 敌对生物群落 |
| 方块判定 | 长生天祭坛砖≥50块 |
| 刷新敌人 | 长生天蛊师（四转~五转） |
| 特殊机制 | 阵法加成（区域内敌人防御+30%） |

**长生天蛊师NPC**：

| NPC名称 | 转数 | 生命 | 伤害 | 特殊能力 |
|---------|------|------|------|---------|
| 长生天巡逻 | 四转 | 1500 | 60 | 冰道攻击 |
| 长生天精英 | 五转 | 3000 | 80 | 五行轮转攻击 |
| 长生天祭司 | 五转 | 2500 | 70 | 治疗周围NPC |

### 3.2 NPC互动对话

**冰塞川对话**（需先击败后才能对话）：

| 对话选项 | 条件 | 结果 |
|---------|------|------|
| "长生天的目的是什么？" | 击败冰塞川后 | 透露长生天对抗天庭的立场 |
| "宙道的奥秘" | 玩家宙道道痕>50 | 冰塞川传授宙道知识（道痕+10） |
| "时间冻结是如何做到的？" | 玩家五转以上 | 冰塞川解释宙道原理 |
| "合作" | 玩家与长生天关系>30 | 解锁长生天任务线 |

**雪胡老祖对话**：

| 对话选项 | 条件 | 结果 |
|---------|------|------|
| "北原的历史" | 首次对话 | 雪胡老祖讲述北原往事 |
| "冰道的修炼" | 玩家冰道道痕>30 | 获得冰道修炼指导 |
| "太白云生" | 完成黑楼兰任务线 | 触发太白云生剧情线 |
| "天庭的威胁" | 玩家五转以上 | 雪胡老祖透露天庭即将降临 |

**五行大法师对话**：

| 对话选项 | 条件 | 结果 |
|---------|------|------|
| "五行之道" | 首次对话 | 五行大法师讲解五行相生相克 |
| "五行轮转" | 玩家五行道痕>40 | 获得五行轮转技能书 |
| "挑战" | 玩家五转巅峰 | 触发五行大法师Boss战 |
| "天庭" | 完成北原主线 | 五行大法师透露天庭情报 |

### 3.3 长生天任务线

| 序号 | 任务名称 | 目标 | 奖励 |
|------|---------|------|------|
| 1 | 长生天的邀请 | 前往长生天营地 | 50元石 |
| 2 | 冰原试炼 | 在暴风雪中存活5分钟 | 80元石+冰道蛊虫 |
| 3 | 五行考验 | 分别使用5种元素攻击命中目标 | 100元石+五行知识 |
| 4 | 宙道感悟 | 在冰塞川的指导下领悟宙道 | 120元石+宙道道痕+10 |
| 5 | 天庭前哨 | 调查天庭在北原的活动 | 150元石+天庭情报 |

---

## 4. Boss战设计

### 4.1 黑楼兰（=Duke Fishron）

**已有文件**：`Content/NPCs/NorthDesert/HeiLouLan.cs`

**基础属性**：

| 属性 | 值 | 说明 |
|------|-----|------|
| 生命 | 100000 | 七转力道 |
| 伤害 | 140 | 霸拳伤害 |
| 防御 | 60 | 高防御 |
| 击退抗性 | 100% | 不可击退 |
| AI风格 | 自定义 | 三阶段Boss |

**触发方式**：
- 与黑楼兰对话选择"挑战"
- 或在王庭使用"挑战令"物品

**第一阶段（100%~60%）——霸拳**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 霸拳直击 | 向玩家冲刺+拳击 | 180 | 每3秒 |
| 霸拳冲击波 | 拳击地面产生冲击波 | 120 | 每8秒 |
| 力量蓄积 | 黑楼兰蓄力，防御+100% | - | 每15秒 |

**霸拳直击实现**：
```csharp
// 黑楼兰AI中
if (attackTimer % 180 == 0) // 每3秒
{
    Vector2 dashDir = (target.Center - npc.Center);
    dashDir.Normalize();
    npc.velocity = dashDir * 15f; // 高速冲刺
    // 冲刺期间碰撞伤害翻倍
    dashActive = true;
    dashTimer = 30; // 0.5秒冲刺
}
```

**冲击波实现**：
- 使用 `CircleSpawnHelper` 生成圆形弹幕
- 弹幕从黑楼兰位置向外扩散
- 使用 `PowerTrailBehavior` + `WaveBehavior`

**第二阶段（60%~30%）——力量增幅**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 霸拳·连击 | 连续3次冲刺攻击 | 150×3 | 每6秒 |
| 霸王领域 | 黑楼兰周围15格内玩家受到持续压力 | 50/秒 | 持续 |
| 力量共鸣 | 黑楼兰攻击速度+50%，伤害+30% | - | 被动 |

**霸王领域实现**：
```csharp
// 在黑楼兰AI的Update中
float domainRadius = 15f * 16f; // 15格
foreach (Player player in Main.player)
{
    if (player.active && Vector2.Distance(player.Center, npc.Center) < domainRadius)
    {
        // 持续压力伤害
        player.Hurt(PlayerDeathReason.ByCustomCause("霸王领域的重压"),
            50, 0, false, false, 0, false);
        // 移速降低
        player.moveSpeed *= 0.7f;
    }
}
```

**第三阶段（30%~0%）——霸道极限**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 霸拳·终极 | 超高速冲刺，路径上留下冲击波 | 250 | 每10秒 |
| 霸王陨 | 跳起后砸地，全屏冲击波 | 200 | 每20秒 |
| 力量解放 | 黑楼兰防御归零，攻击+100% | - | 被动 |

**霸王陨实现**：
```
黑楼兰跳至屏幕上方（1秒）
    ↓
锁定玩家位置
    ↓
高速下砸（0.5秒）
    ↓
落地时产生全屏冲击波
    ↓
冲击波从落点向外扩散，速度8格/秒
    ↓
冲击波持续3秒
```

**掉落物**：

| 物品 | 概率 | 数量 | 说明 |
|------|------|------|------|
| 力道真传碎片 | 100% | 2~3 | 合成力道真传 |
| 霸王拳套 | 25% | 1 | 力道五转武器 |
| 黑楼兰的战意 | 15% | 1 | 特殊饰品（力量+20%） |
| 王庭令 | 50% | 1 | 召唤黑楼兰助战 |
| 元石 | 100% | 50~80 | 基础货币 |

### 4.2 冰塞川（=Lunatic Cultist）

**已有文件**：`Content/NPCs/NorthDesert/BingSaiChuan.cs`

**基础属性**：

| 属性 | 值 | 说明 |
|------|-----|------|
| 生命 | 130000 | 八转宙道 |
| 伤害 | 130 | 基础攻击 |
| 防御 | 50 | 中等防御 |
| 击退抗性 | 100% | 不可击退 |
| AI风格 | 自定义 | 三阶段Boss |

**触发方式**：
- 完成长生天任务线后，在长生天营地与冰塞川对话选择"挑战"
- 或在北原深处使用"宙道共鸣石"召唤

**第一阶段（100%~65%）——宙道攻击**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 时间减速 | 冰塞川周围20格内玩家移速-50% | - | 持续 |
| 冰封弹 | 向玩家发射冰封弹，命中后冻结1秒 | 100 | 每3秒 |
| 时间回溯 | 冰塞川回到3秒前的位置和血量 | - | 每20秒 |
| 宙道冲击 | 时间线攻击——延迟3秒后爆发 | 150 | 每8秒 |

**时间回溯实现**：
```csharp
// 冰塞川AI中
private Vector2 _snapshotPosition;
private int _snapshotLife;
private int _snapshotTimer;

// 每3秒记录一次快照
if (_snapshotTimer % 180 == 0)
{
    _snapshotPosition = npc.Center;
    _snapshotLife = npc.life;
}

// 时间回溯技能
if (attackTimer % 1200 == 0 && npc.life < npc.lifeMax * 0.9f)
{
    npc.Center = _snapshotPosition;
    npc.life = Math.Min(_snapshotLife, npc.lifeMax);
    npc.netUpdate = true;
    // 视觉效果：时间倒流粒子
    for (int i = 0; i < 30; i++)
    {
        Dust.NewDustPerfect(npc.Center, DustID.IceTorch,
            new Vector2(Main.rand.Next(-5, 5), Main.rand.Next(-5, 5)));
    }
}
```

**宙道冲击（延迟爆发）实现**：
```
冰塞川挥手 → 在玩家位置生成时间标记（红色圆圈）
    ↓
3秒后标记位置爆发
    ↓
爆发范围：5格
伤害：150
    ↓
玩家需要在3秒内离开标记区域
```

**第二阶段（65%~30%）——冰封领域**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 冰封领域 | 冰塞川周围30格变为冰封区域 | 30/秒 | 持续 |
| 时间冻结 | 区域内所有弹幕停止3秒 | - | 每15秒 |
| 冰棺 | 将玩家困入冰棺3秒 | 0 | 每12秒 |
| 宙道·加速 | 冰塞川自身速度+100% | - | 被动 |

**冰封领域实现**：
- 冰塞川周围30格地面变为冰面
- 冰面上玩家移速+20%但无法急停（惯性滑动）
- 冰面上每秒受到30点冰属性伤害
- 使用 `ModProjectile` 创建领域弹幕（不可见，范围伤害）

**时间冻结实现**：
```csharp
// 冻结所有玩家弹幕
foreach (Projectile proj in Main.projectile)
{
    if (proj.active && proj.friendly && !proj.hostile)
    {
        // 记录弹幕速度
        frozenProjectiles[proj.whoAmI] = proj.velocity;
        proj.velocity = Vector2.Zero;
        proj.frozen = true; // 需要在GlobalProjectile中处理
    }
}
// 3秒后恢复
```

**冰棺机制**：
```
冰塞川发射冰棺弹 → 命中玩家
    ↓
玩家被冰棺包裹（无法移动/攻击，持续3秒）
    ↓
冰棺可被其他玩家/召唤物攻击破坏（200HP）
    ↓
3秒后冰棺自动碎裂，玩家恢复自由
    ↓
冰棺碎裂时对玩家造成50点伤害
```

**第三阶段（30%~0%）——宙道极限**

| 攻击 | 机制 | 伤害 | 频率 |
|------|------|------|------|
| 时间倒流 | 冰塞川回溯到满血（仅一次） | - | 30%血量时 |
| 绝对零度 | 全屏冰冻2秒 | 100 | 每25秒 |
| 宙道·终焉 | 在地面生成时间裂缝，踩上去被传送回5秒前位置 | 80 | 每10秒 |
| 冰塞川·真身 | 冰塞川变为冰巨人形态，攻击范围+100% | 200 | 持续 |

**时间倒流（仅一次）**：
```
冰塞川血量降至30% → 触发时间倒流
    ↓
屏幕变为灰白色（时间停止效果，2秒）
    ↓
冰塞川恢复至100%血量
    ↓
此技能仅触发一次
    ↓
第二阶段所有技能继续使用
    ↓
冰塞川不再使用时间回溯（已用过）
```

**绝对零度实现**：
```
冰塞川蓄力（2秒，屏幕逐渐变白）
    ↓
全屏冰冻效果
    ↓
所有玩家冻结2秒（无法移动/攻击/使用物品）
    ↓
冻结结束时受到100点伤害
    ↓
可被"抗冰"Buff减免
```

**掉落物**：

| 物品 | 概率 | 数量 | 说明 |
|------|------|------|------|
| 宙道真传碎片 | 100% | 2~4 | 合成宙道真传 |
| 冰塞川的时之沙 | 20% | 1 | 宙道五转武器 |
| 绝对零度核心 | 10% | 1 | 特殊饰品（冰免疫） |
| 宙道道痕卷轴 | 30% | 1 | 使用后宙道道痕+20 |
| 元石 | 100% | 60~100 | 基础货币 |

### 4.3 五行大法师

**已有文件**：`Content/NPCs/NorthDesert/WuXingDaFaShi.cs`

**基础属性**：

| 属性 | 值 | 说明 |
|------|-----|------|
| 生命 | 110000 | 八转五行道 |
| 伤害 | 120 | 基础攻击 |
| 防御 | 45 | 中等防御 |
| 击退抗性 | 90% | 几乎不可击退 |
| AI风格 | 自定义 | 五行轮转Boss |

**核心机制——五行轮转**：

```
五行大法师在金/木/水/火/土五种形态间轮转
    ↓
每种形态持续15秒
    ↓
当前形态决定攻击方式和弱点
    ↓
克制属性造成双倍伤害：
  金克木、木克土、土克水、水克火、火克金
    ↓
被克制属性造成半倍伤害
```

**五行形态详细设计**：

| 形态 | 颜色 | 攻击方式 | 弱点 | 克制 |
|------|------|---------|------|------|
| 金 | 白色 | 金属弹幕（高伤害，直线） | 火道 | 木道 |
| 木 | 绿色 | 藤蔓束缚+毒雾 | 金道 | 土道 |
| 水 | 蓝色 | 水波攻击+冰冻 | 土道 | 火道 |
| 火 | 红色 | 火焰弹幕+灼烧 | 水道 | 金道 |
| 土 | 黄色 | 岩石投掷+地面震击 | 木道 | 水道 |

**五行轮转实现**：
```csharp
public enum WuXingPhase
{
    Metal, Wood, Water, Fire, Earth
}

private WuXingPhase currentPhase;
private int phaseTimer;
private const int PHASE_DURATION = 900; // 15秒

// 在AI中
phaseTimer++;
if (phaseTimer >= PHASE_DURATION)
{
    phaseTimer = 0;
    currentPhase = (WuXingPhase)(((int)currentPhase + 1) % 5);
    // 切换形态特效
    OnPhaseChange();
}

// 伤害计算
public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref float knockback,
    ref bool crit, ref int hitDirection)
{
    var projDaoType = GuProjectileInfo.GetDaoType(proj);
    if (IsCounteredBy(projDaoType))
        damage = (int)(damage * 2f);  // 克制双倍
    else if (IsResistantTo(projDaoType))
        damage = (int)(damage * 0.5f); // 被克制半倍
}
```

**特殊攻击——五行合一**（血量<20%时）：

```
五行大法师同时使用五种元素攻击
    ↓
5种弹幕同时发射（金/木/水/火/土各一种）
    ↓
此时五行大法师没有弱点（所有属性正常伤害）
    ↓
持续15秒后回到单一形态
```

**掉落物**：

| 物品 | 概率 | 数量 | 说明 |
|------|------|------|------|
| 五行真传碎片 | 100% | 2~3 | 合成五行真传 |
| 五行轮转蛊 | 15% | 1 | 五行道五转武器 |
| 五行调和丹 | 30% | 3 | 使用后五行道痕各+5 |
| 元石 | 100% | 50~80 | 基础货币 |

---

## 5. 僵盟系统

### 5.1 仙僵转化

**新系统**：`Common/Systems/ImmortalZombieSystem.cs`

**仙僵概述**：仙僵是蛊师通过特殊方法将自身转化为不死之身的途径。转化后获得强大的肉体能力，但失去部分人性。

**转化条件**：

| 条件 | 要求 |
|------|------|
| 境界 | 五转以上 |
| 材料 | 僵尸蛊×1 + 太古魂核×1 + 仙元石×10 |
| 任务 | 完成"僵盟接触"任务 |
| 代价 | 永久失去10%真元上限 |

**转化流程**：
```
与僵盟使者对话 → 选择"我愿意成为仙僵"
    ↓
消耗所有材料
    ↓
播放转化动画（5秒，玩家变为灰色）
    ↓
玩家获得"仙僵"永久Buff
    ↓
世界消息："一位新的仙僵诞生了……"
```

### 5.2 仙僵效果

**增益**：

| 效果 | 数值 | 说明 |
|------|------|------|
| 生命上限 | +50% | 仙僵肉体强大 |
| 防御 | +30% | 仙僵肉体坚韧 |
| 生命恢复 | +5/秒 | 仙僵持续自愈 |
| 免疫 | 冰冻/中毒/流血 | 仙僵不受这些debuff影响 |
| 近战伤害 | +40% | 仙僵近战强力 |

**减益**：

| 效果 | 数值 | 说明 |
|------|------|------|
| 真元上限 | -10% | 永久减少 |
| 真元恢复 | -30% | 仙僵真元恢复缓慢 |
| 移动速度 | -15% | 仙僵行动迟缓 |
| NPC好感 | -20（所有） | 仙僵令人恐惧 |
| 阳光伤害 | 5/秒（白天地表） | 仙僵畏惧阳光 |
| 无法使用 | 治疗类蛊虫 | 仙僵无法被常规治疗 |

**实现**：
```csharp
// 新增ModPlayer
public class ImmortalZombiePlayer : ModPlayer
{
    public bool IsImmortalZombie;
    public int SunDamageTimer;

    public override void ResetEffects()
    {
        if (IsImmortalZombie)
        {
            Player.statLifeMax2 = (int)(Player.statLifeMax2 * 1.5f);
            Player.statDefense += (int)(Player.statDefense * 0.3f);
            Player.lifeRegen += 5;
            Player.GetDamage(DamageClass.Melee) += 0.4f;
            Player.moveSpeed *= 0.85f;

            // 阳光伤害
            if (Main.dayTime && Player.ZoneOverworldHeight)
            {
                SunDamageTimer++;
                if (SunDamageTimer >= 60)
                {
                    Player.Hurt(PlayerDeathReason.ByCustomCause("阳光灼烧仙僵之躯"),
                        5, 0, false, false, 0, false);
                    SunDamageTimer = 0;
                }
            }
        }
    }

    public override bool CanUseItem(Item item)
    {
        if (IsImmortalZombie && IsHealingGu(item))
            return false;
        return true;
    }
}
```

### 5.3 僵盟任务线

| 序号 | 任务名称 | 目标 | 奖励 |
|------|---------|------|------|
| 1 | 僵盟接触 | 在北原深处找到僵盟使者 | 50元石 |
| 2 | 仙僵的秘密 | 收集3个太古魂核 | 80元石+僵尸蛊 |
| 3 | 僵盟试炼 | 在不使用治疗物品的情况下击败5个强敌 | 100元石 |
| 4 | 仙僵之路 | 完成仙僵转化（可选） | 仙僵Buff |
| 5 | 僵盟的请求 | 协助僵盟对抗天庭先遣队 | 150元石+僵盟声望 |

---

## 6. 太白云生之死

### 6.1 故事事件

**已有NPC**：`Content/NPCs/Allies/TaiBaiYunSheng.cs`

**触发条件**：
- 完成黑楼兰Boss战
- 完成长生天任务线至少3个
- 玩家五转以上

**事件流程**：

```
完成前置条件 → 太白云生出现在王庭
    ↓
与太白云生对话 → "方源……我一直在追踪他的踪迹"
    ↓
太白云生请求玩家协助追踪方源
    ↓
【任务：追踪方源】
前往北原深处，找到方源的踪迹
    ↓
到达指定位置 → 触发过场动画
    ↓
方源出现："太白，你终究还是来了。"
    ↓
太白云生与方源对峙
    ↓
方源使用春秋蝉+炼道攻击太白云生
    ↓
【关键选择点】
    ↓
A. 救助太白云生（冲向方源攻击）
B. 旁观（不做任何行动）
C. 趁机偷袭方源（背后攻击）
    ↓
选择A：太白云生存活，但重伤
选择B：太白云生死亡
选择C：方源反击，太白云生仍死亡
    ↓
无论结果如何，方源都会离开
```

### 6.2 太白云生存活路线

```
太白云生存活 → 被送往王庭疗伤
    ↓
3个游戏日后可再次对话
    ↓
太白云生："多亏你救了我……但方源的实力远超我的想象。"
    ↓
太白云生提供：
  - 炼道知识（炼道道痕+15）
  - 太白丹方（特殊丹药配方）
  - 方源情报（解锁天庭相关剧情）
    ↓
太白云生成为王庭常驻NPC
    ↓
后续可协助玩家对抗天庭
```

### 6.3 太白云生死亡路线

```
太白云生死亡 → 触发世界事件
    ↓
世界消息："太白云生……陨落了。"
    ↓
所有北原NPC好感-5（悲伤）
    ↓
太白云生掉落：
  - 太白云生的遗物（特殊饰品）
  - 炼道真传碎片×2
  - 太白丹方（仍可获得）
    ↓
黑楼兰对话变化：
  "太白云生的死……是北原的巨大损失。"
    ↓
后续无法获得太白云生的协助
    ↓
因果业力+50（未能救人）
```

### 6.4 情感影响系统

**新增系统**：`Common/Systems/EmotionalImpactSystem.cs`

```csharp
public class EmotionalImpactRecord
{
    public string EventID;
    public string Description;
    public EmotionalImpactType Type;  // 悲伤/愤怒/恐惧/希望/绝望
    public int Intensity;              // 1~100
    public List<string> AffectedNPCs;  // 受影响的NPC
    public int DurationDays;           // 持续天数
}

public enum EmotionalImpactType
{
    Grief,      // 悲伤
    Anger,      // 愤怒
    Fear,       // 恐惧
    Hope,       // 希望
    Despair,    // 绝望
}
```

太白云生之死的影响：

| NPC | 情感 | 强度 | 持续 | 效果 |
|-----|------|------|------|------|
| 黑楼兰 | 悲伤 | 60 | 10日 | 对话变化，商店折扣 |
| 毛里求 | 愤怒 | 40 | 5日 | 攻击力+10% |
| 雪胡老祖 | 悲伤 | 80 | 15日 | 提供额外任务 |
| 商心慈 | 绝望 | 70 | 20日 | 暂时离开（不再出现） |

---

## 7. 天庭前奏

### 7.1 天庭NPC出现

**已有NPC文件**：
- `Content/NPCs/HeavenCourt/LongGong.cs`（龙公）
- `Content/NPCs/HeavenCourt/TongGong.cs`（铜公）
- `Content/NPCs/HeavenCourt/MeiGong.cs`（眉公）
- `Content/NPCs/HeavenCourt/QinDingLing.cs`（秦鼎灵）
- `Content/NPCs/HeavenCourt/GuYueFangZheng.cs`（古月方正）

**天庭先遣队**：

| NPC名称 | 类型 | 转数 | 行为 |
|---------|------|------|------|
| 天庭使者 | 城镇NPC | 六转 | 在世界各处出现，发布天庭通告 |
| 天庭巡逻 | 敌对 | 五转 | 在特定区域巡逻，攻击魔道玩家 |
| 天庭侦察兵 | 敌对 | 四转 | 隐身状态，被发现后逃跑 |

**天庭使者对话**：

| 对话选项 | 条件 | 结果 |
|---------|------|------|
| "天庭为何而来？" | 首次对话 | 使者宣布天庭即将降临 |
| "我愿归顺天庭" | 玩家正道声望>30 | 开启天庭任务线（与影宗对立） |
| "天庭无权管辖我" | 玩家魔道声望>30 | 使者警告，天庭将视为敌对 |
| "龙公是谁？" | 任何条件 | 使者描述龙公的恐怖实力 |

### 7.2 龙公的阴影

**龙公NPC**（五转阶段仅为剧情出现，不触发Boss战）：

**出现方式**：
- 在北原完成特定事件后，天空偶尔出现龙形云彩
- 玩家靠近时听到龙吟声
- 屏幕短暂震动
- 世界消息："一股令人窒息的威压从天际传来……"

**龙公的阴影效果**：
- 龙公存在时，所有NPC的"恐惧"信念值+10
- 敌对NPC有20%概率逃跑
- 天气强制变为晴朗（龙公压制天气）

### 7.3 宿命大战铺垫

**隐藏事件链**：

```
完成北原主线 + 击败冰塞川/五行大法师
    ↓
天庭使者再次出现
    ↓
"宿命之战即将来临，所有蛊师都将被卷入……"
    ↓
世界事件"天庭降临"触发
    ↓
天庭浮岛出现在世界高空（新生物群落）
    ↓
天庭NPC军团开始在世界各处出现
    ↓
玩家可以选择：
  A. 前往天庭（正道路线）
  B. 联合影宗（魔道路线）
  C. 保持中立（中立路线）
    ↓
选择影响Stage6的阵营起始状态
```

---

## 8. 具体实现任务清单

### 8.1 基础设施（优先级：🔴最高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S5-01 | 北原冰原生物群落 | `Content/Biomes/NorthDesertBiome.cs` | 4h |
| S5-02 | 冰原方块（冻土/冰晶矿/雪松/灵草/平台） | `Content/Tiles/NorthDesert/` | 5h |
| S5-03 | 北原区域世界生成 | `Common/Systems/NorthDesertWorldGen.cs` | 6h |
| S5-04 | 暴风雪天气扩展 | 扩展 `WeatherSystem.cs` | 4h |
| S5-05 | 绝对零度极端天气 | 扩展 `WeatherSystem.cs` | 3h |
| S5-06 | 王庭城镇生物群落 | `Content/Biomes/WangTingTown.cs` | 3h |
| S5-07 | 王庭方块与建筑 | `Content/Tiles/NorthDesert/WangTing/` | 5h |
| S5-08 | 长生天营地生物群落 | `Content/Biomes/ChangShengTianCamp.cs` | 3h |
| S5-09 | 僵盟系统 | `Common/Systems/ImmortalZombieSystem.cs` | 5h |
| S5-10 | 仙僵ModPlayer | `Common/Players/ImmortalZombiePlayer.cs` | 4h |
| S5-11 | 情感影响系统 | `Common/Systems/EmotionalImpactSystem.cs` | 4h |
| S5-12 | 天庭前奏事件系统 | `Common/Systems/HeavenPreludeSystem.cs` | 4h |

### 8.2 Boss战（优先级：🔴最高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S5-13 | 黑楼兰Boss AI（三阶段+霸拳） | 扩展 `HeiLouLan.cs` | 8h |
| S5-14 | 黑楼兰弹幕（霸拳/冲击波/霸王陨） | `Content/Projectiles/HeiLouLan/` | 5h |
| S5-15 | 冰塞川Boss AI（三阶段+宙道） | 扩展 `BingSaiChuan.cs` | 10h |
| S5-16 | 冰塞川弹幕（冰封弹/时间冻结/绝对零度） | `Content/Projectiles/BingSaiChuan/` | 7h |
| S5-17 | 五行大法师Boss AI（五行轮转） | 扩展 `WuXingDaFaShi.cs` | 8h |
| S5-18 | 五行大法师弹幕（5种元素+五行合一） | `Content/Projectiles/WuXingDaFaShi/` | 6h |
| S5-19 | 太古冰兽稀有Boss | `Content/NPCs/NorthDesert/AncientIceBeast.cs` | 4h |

### 8.3 NPC与对话（优先级：🟡高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S5-20 | 冰原荒兽NPC（5种） | `Content/NPCs/NorthDesert/` | 8h |
| S5-21 | 长生天蛊师NPC（3种） | `Content/NPCs/NorthDesert/` | 4h |
| S5-22 | 王庭城镇NPC（6种） | `Content/NPCs/NorthDesert/Town/` | 8h |
| S5-23 | 天庭先遣NPC（3种） | `Content/NPCs/HeavenCourt/` | 5h |
| S5-24 | 黑楼兰对话树 | `Content/NPCs/NorthDesert/HeiLouLanDialogue.cs` | 3h |
| S5-25 | 冰塞川对话树 | `Content/NPCs/NorthDesert/BingSaiChuanDialogue.cs` | 3h |
| S5-26 | 雪胡老祖对话树 | `Content/NPCs/NorthDesert/XueHuLaoZuDialogue.cs` | 2h |
| S5-27 | 五行大法师对话树 | `Content/NPCs/NorthDesert/WuXingDaFaShiDialogue.cs` | 2h |
| S5-28 | 太白云生剧情事件 | `Content/NPCs/Allies/TaiBaiYunShengEvent.cs` | 5h |

### 8.4 任务系统（优先级：🟡高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S5-29 | 王庭任务线（7个任务） | 扩展 `QuestSystem.cs` | 6h |
| S5-30 | 长生天任务线（5个任务） | 扩展 `QuestSystem.cs` | 4h |
| S5-31 | 僵盟任务线（5个任务） | 扩展 `QuestSystem.cs` | 4h |
| S5-32 | 天庭前奏事件链 | `Common/Systems/HeavenPreludeEventChain.cs` | 5h |

### 8.5 物品与装备（优先级：🟡高）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S5-33 | 力道真传碎片+完整真传 | `Content/Items/Recipes/` | 3h |
| S5-34 | 宙道真传碎片+完整真传 | `Content/Items/Recipes/` | 3h |
| S5-35 | 五行真传碎片+完整真传 | `Content/Items/Recipes/` | 3h |
| S5-36 | 冰原武器箱+北原专属物品 | `Content/Items/` | 4h |
| S5-37 | 仙僵转化材料 | `Content/Items/` | 2h |
| S5-38 | 太白云生遗物 | `Content/Items/` | 2h |

### 8.6 Buff与Debuff（优先级：🟢中）

| 编号 | 任务 | 文件 | 预估工时 |
|------|------|------|---------|
| S5-39 | 仙僵Buff | `Content/Buffs/AddToSelf/Pobuff/ImmortalZombieBuff.cs` | 2h |
| S5-40 | 冰冻Debuff | `Content/Buffs/AddToSelf/Debuff/FrozenDebuff.cs` | 2h |
| S5-41 | 冰棺Debuff | `Content/Buffs/AddToSelf/Debuff/IceCoffinDebuff.cs` | 2h |
| S5-42 | 阳光灼烧Debuff | `Content/Buffs/AddToSelf/Debuff/SunburnDebuff.cs` | 1h |
| S5-43 | 五行克制标记 | `Content/Buffs/AddToSelf/Debuff/WuXingCounterDebuff.cs` | 2h |
| S5-44 | 霸王领域Debuff | `Content/Buffs/AddToSelf/Debuff/HegemonDomainDebuff.cs` | 2h |

### 8.7 DownBossSystem扩展

| 编号 | 任务 | 说明 |
|------|------|------|
| S5-45 | 新增 `downedHeiLouLan` | 黑楼兰击败标记 |
| S5-46 | 新增 `downedBingSaiChuan` | 冰塞川击败标记 |
| S5-47 | 新增 `downedWuXingDaFaShi` | 五行大法师击败标记 |
| S5-48 | 新增 `downedAncientIceBeast` | 太古冰兽击败标记 |
| S5-49 | 新增 `taiBaiYunShengSaved` | 太白云生存活标记 |
| S5-50 | 新增 `isImmortalZombie` | 仙僵转化标记 |
| S5-51 | 新增 `heavenPreludeTriggered` | 天庭前奏触发标记 |

### 8.8 总工时估算

| 类别 | 工时 |
|------|------|
| 基础设施 | 50h |
| Boss战 | 48h |
| NPC与对话 | 40h |
| 任务系统 | 19h |
| 物品与装备 | 17h |
| Buff与Debuff | 11h |
| **总计** | **~185h** |

---

## 附录A：五转武器确认

当前五转武器78个，目标78个，无需新增。但需确保以下Boss关联武器可制作：

| 武器 | 制作材料 | 来源 |
|------|---------|------|
| 霸王拳套 | 力道真传碎片×3 + 霸铜×20 + 金精×10 | 黑楼兰掉落 |
| 时之沙 | 宙道真传碎片×3 + 太古魂核×5 + 星辰石×10 | 冰塞川掉落 |
| 五行轮转蛊 | 五行真传碎片×3 + 五光山×5 + 天晶×10 | 五行大法师掉落 |

## 附录B：与现有系统的集成点

| 现有系统 | 集成方式 |
|---------|---------|
| `WorldStateMachine` | 新增北原/天庭前奏事件类型 |
| `DownBossSystem` | 新增4个Boss击败标记+3个事件标记 |
| `WeatherSystem` | 新增暴风雪/绝对零度天气 |
| `HeavenTribulationSystem` | 五转→六转血劫（已有框架） |
| `InheritanceSystem` | 注册北原相关真传 |
| `QuestSystem` | 新增3条任务线（17个任务） |
| `FactionAllegianceSystem` | 天庭/影宗/中立阵营选择 |
| `KarmaSystem` | 太白云生事件影响因果业力 |
| `CorpsePlayer` | 仙僵与尸体交互变化 |
| `DaoHenConflictSystem` | 新增冰道/宙道/五行道道痕 |
| `GuDropRegistry` | 新增北原NPC掉落 |
| `EventBus` | 新增太白云生事件/天庭前奏事件 |
