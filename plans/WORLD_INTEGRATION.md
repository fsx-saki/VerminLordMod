# 世界串联设计：主线驱动一切

> 版本：v1.0 | 日期：2026-05-28
> 核心理念：**主线剧情是世界的引擎，所有系统都是齿轮**

---

## 0. 设计哲学

```
传统游戏设计：系统 → 内容 → 剧情包装
本Mod设计：  剧情节点 → 解锁系统 → 生成内容 → 反馈剧情

一切以方源的500年人生为线索
玩家不是方源，但玩家的每一步都走在方源的影子里
世界因剧情而改变，而非因系统而运转
```

**三大原则**：
1. **主线不可跳过** — 核心系统随剧情解锁，不推进剧情就无法获得关键能力
2. **世界有记忆** — 每个选择都会被世界记录，影响后续所有内容
3. **系统服务于叙事** — 不存在"为了系统而系统"的内容，每个机制都有小说依据

---

## 1. 主线剧情阶段统一枚举

**扩展**：`Common/DialogueTree/StoryPhase.cs`

现有7个阶段仅覆盖Stage1，需要扩展为覆盖全部7个Stage的完整枚举：

```csharp
public enum StoryPhase
{
    // === Stage1: 一转·蛊师入门 ===
    NotEntered = 0,          // 未进入古月山寨
    Arrival = 1,             // 初入山寨（守门蛊师盘问）
    AwakeningCeremony = 2,   // 开窍仪式（获得资质和第一只蛊）
    SchoolTraining = 3,      // 学堂历练（学习基础蛊术）
    MedicineRequest = 4,     // 药堂求助（贾金生事件链开始）
    JiaJinShengDeath = 5,    // 贾金生之死（第一次面对生死抉择）
    HuaJiuInheritance = 6,   // 花酒行者传承（第一次获得强力蛊虫）
    FamilyRecognition = 7,   // 家族认可（获得正式身份）

    // === Stage2: 二转·家族争锋 ===
    PreTournament = 10,      // 三寨大比前夕
    TournamentBegin = 11,    // 三寨大比开始
    TournamentFinal = 12,    // 三寨大比决赛（vs白凝冰）
    TianHeAttack = 13,       // 天鹤来袭
    BaiNingBingIceSeal = 14, // 白凝冰冰封
    BloodSacrifice = 15,     // 血祭事件（方源真面目初现）
    LeftQingMao = 16,        // 离开青茅山

    // === Stage3: 三转·南疆流浪 ===
    SouthBorderArrival = 20, // 到达南疆
    ShangXinCiMeet = 21,     // 遇见商心慈
    ThreeKingsInheritance = 22, // 三王传承
    ChunQiuChanFragment = 23,   // 获得春秋蝉残影
    SanXiuCampComplete = 24,    // 散修营地主线完成

    // === Stage4: 四转·义天山大战 ===
    YiTianShanAppears = 30,  // 义天山异变
    YiTianShanDungeon = 31,  // 义天山副本
    DaTongFeng = 32,         // 大同风事件
    FangYuanReveal = 33,     // 方源暴露真面目
    YiTianShanComplete = 34, // 义天山事件完成

    // === Stage5: 五转·北原争霸 ===
    NorthDesertArrival = 40, // 到达北原
    WangTingAlly = 41,       // 王庭结盟
    ChangShengTianContact = 42, // 长生天接触
    TaiBaiYunShengDeath = 43,   // 太白云生之死/存活
    ImmortalZombieChoice = 44,  // 仙僵选择
    HeavenPrelude = 45,         // 天庭前奏

    // === Stage6: 六转·宿命大战与升仙 ===
    DestinyWarBegin = 50,    // 宿命大战开始
    FourPillarsDown = 51,    // 四柱击败
    FactionChoice = 52,      // 阵营选择
    LongGongPhase1 = 53,     // 龙公首次战（必败）
    ChunQiuRebirth = 54,     // 春秋蝉回溯
    LongGongPhase2 = 55,     // 龙公二次战（可胜）
    DestinyShattered = 56,   // 宿命碎裂
    Ascension = 57,          // 升仙

    // === Stage7: 七转以上·蛊仙之路 ===
    SevenTurnBegin = 60,     // 七转开始
    ApertureBuilt = 61,      // 仙窍建设完成
    EightTurnBegin = 62,     // 八转开始
    DaoLordChallenge = 63,   // 道主争夺
    NineTurnBegin = 64,      // 九转开始
    VenerableBattle = 65,    // 尊者之战
    TenTurnFinale = 66,      // 十转终局
    EndingChosen = 67,       // 结局选择完成
}
```

**阶段编号规则**：
- 每个Stage占10个编号（0~9, 10~19, 20~29...）
- 留有间隔便于后续插入新节点
- 编号不连续表示存在分支/可选内容

---

## 2. 主线驱动的系统解锁表

**核心设计**：每个系统/能力/区域都绑定到特定的剧情阶段，不推进剧情就无法解锁

| 解锁项 | 解锁阶段 | 解锁条件 | 影响的系统 |
|--------|---------|---------|-----------|
| **空窍系统** | AwakeningCeremony | 开窍仪式完成 | KongQiaoPlayer |
| **第一只蛊虫** | AwakeningCeremony | 开窍仪式完成 | KongQiaoPlayer |
| **真元系统** | AwakeningCeremony | 开窍仪式完成 | QiResourcePlayer |
| **修炼系统** | SchoolTraining | 学堂入学 | QiRealmPlayer |
| **蛊虫喂养** | SchoolTraining | 学堂学习喂养知识 | GuFeedingSystem |
| **家族声望** | FamilyRecognition | 获得正式身份 | GuWorldPlayer |
| **家族任务** | FamilyRecognition | 获得正式身份 | QuestSystem |
| **交易系统** | FamilyRecognition | 获得正式身份 | TradeSystem |
| **二转突破** | PreTournament | 三寨大比公告 | HeavenTribulationSystem |
| **三寨大比** | PreTournament | 三寨大比公告 | TournamentSystem |
| **白凝冰关系** | TournamentFinal | 决赛相遇 | DialogueSystem |
| **Hardmode** | LeftQingMao | 离开青茅山+击败地脉守护者 | WorldGen |
| **南疆区域** | SouthBorderArrival | 获得推荐信 | BiomeSystem |
| **真传系统** | ThreeKingsInheritance | 完成任意传承 | InheritanceSystem |
| **春秋蝉** | ChunQiuChanFragment | 三王传承隐藏 | ChunQiuChanPlayer |
| **义天山区域** | YiTianShanAppears | 义天山异变事件 | BiomeSystem |
| **阵营选择** | FactionChoice | 四柱击败+天庭降临 | FactionAllegiance |
| **龙公战** | LongGongPhase1 | 四柱全部击败 | DownBossSystem |
| **春秋蝉回溯** | ChunQiuRebirth | 龙公首次击败玩家 | ChunQiuChanPlayer |
| **宿命碎裂** | DestinyShattered | 击败龙公 | DestinySystem |
| **升仙** | Ascension | 宿命碎裂+血劫通过 | QiRealmPlayer |
| **仙窍** | SevenTurnBegin | 升仙完成 | SubworldLibrary |
| **仙元** | SevenTurnBegin | 升仙完成 | QiResourcePlayer |
| **道主争夺** | DaoLordChallenge | 八转+道系真传 | DaoLordSystem |
| **尊者战** | VenerableBattle | 九转+至尊仙胎 | DownBossSystem |

---

## 3. 主线驱动的世界变化链

每个剧情节点触发一系列世界变化，形成连锁反应：

### 3.1 Stage1 链条

```
开窍仪式(AwakeningCeremony)
  → 解锁空窍/真元/修炼系统
  → 学堂NPC开始提供任务
  → 古月山寨内NPC对话变化（"你开窍了？"）
  → 天气系统开始影响修炼速度

贾金生之死(JiaJinShengDeath)
  → 选择A（帮助方源）：方源好感度+20，铁血冷信任-10
  → 选择B（帮助铁血冷）：铁血冷信任+20，方源好感度-5
  → 选择C（旁观）：方源好感度+5（"你很聪明"），铁血冷信任-5
  → 世界消息传播（NPCSocialNetwork）：3个游戏日后所有NPC知道贾金生死讯
  → 药堂家老对话变化
  → 解锁"调查贾金生之死"支线任务

花酒行者传承(HuaJiuInheritance)
  → 获得花酒行者宝箱（含洗髓蛊+二转蛊虫）
  → 资质可能提升（使用洗髓蛊）
  → 古月博注意到玩家实力增长
  → 解锁赤脉/漠脉秘密区域
  → 3个游戏日后：其他家族开始打探花酒传承消息

家族认可(FamilyRecognition)
  → 获得古月家族正式身份
  → 解锁家族任务/交易/声望系统
  → 解锁家族商店（药堂/武堂/器堂）
  → 古月山寨内所有NPC态度+10
  → 其他家族NPC开始出现
  → 触发PreTournament（3个游戏日后）
```

### 3.2 Stage2 链条

```
三寨大比决赛(TournamentFinal)
  → 对手=白凝冰
  → 战斗中白凝冰展示冰道能力
  → 胜利：白凝冰注意到玩家，好感度+10
  → 失败：白凝冰轻视玩家，好感度-5
  → 无论结果：方源在观众中观察玩家
  → 解锁白凝冰对话选项

天鹤来袭(TianHeAttack)
  → 山寨部分损坏（建筑修复任务）
  → NPC伤亡（随机1~3个平民NPC死亡）
  → 武堂家老重伤（暂时无法交互）
  → 天鹤上人掉落风道材料
  → 触发白凝冰冰封事件的前置

白凝冰冰封(BaiNingBingIceSeal)
  → 选择A（帮助）：白凝冰好感度+20，冰封留下冰雕
  → 选择B（旁观）：白凝冰好感度+5，冰封留下冰雕
  → 选择C（敌对）：白凝冰自爆，不留下冰雕
  → 冰雕成为地图永久标记（Stage4可能解封）
  → 所有NPC对话增加白凝冰相关内容

血祭事件(BloodSacrifice)
  → 方源真面目初现
  → 选择A（加入方源）：魔道路线倾向+30
  → 选择B（对抗方源）：正道路线倾向+30
  → 选择C（逃离）：中立倾向+20
  → 地脉守护者Boss出现
  → 击败地脉守护者→Hardmode激活

离开青茅山(LeftQingMao)
  → Hardmode矿脉生成
  → 南疆区域解锁
  → 青茅山NPC状态冻结（可返回但不再推进剧情）
  → 方源消失（以"黑土"身份在Stage3出现）
  → 获得推荐信物品
```

### 3.3 Stage3 链条：南疆流浪

```
南疆初到(SouthBorderArrival)
  → 世界生成南疆荒野Biome（地表+地下）
  → 散修营地NPC出现（太白云生、商心慈、黑市商人）
  → 新敌人刷新：南疆毒蛇、荒野狼群、散修劫匪
  → 天气新增：雾、灵潮（影响修炼速度+30%）
  → 方源以"黑土"身份出现在散修营地
  → 解锁南疆声望系统
  → 所有青茅山NPC对话增加"你去了南疆？"内容

遇见商心慈(ShangXinCiMeet)
  → 商心慈NPC头顶出现黄色感叹号
  → 对话选项：
    A. "你需要帮助吗？"→ 商心慈好感度+15，解锁商心慈任务线
    B. "你有什么值钱的东西？"→ 商心慈好感度-5，但获得50元石
    C. "……"→ 无变化
  → 商心慈关系等级影响太白云生事件（Stage5）
  → 商心慈提供南疆情报（三王传承位置线索）

三王传承(ThreeKingsInheritance)
  → 3个传承副本入口在南疆荒野中出现
  → 每个传承有独立5层结构+Boss
  → 完成任意1个：解锁真传系统，获得真传碎片
  → 完成任意2个：解锁组合技能，道痕+15
  → 完成全部3个：获得"三王之力"Buff（全属性+10%）
  → 隐藏房间发现春秋蝉残影
  → 传承期间天气变化：地脉震动（屏幕微震）
  → 传承完成后：散修营地NPC议论玩家的实力

获得春秋蝉残影(ChunQiuChanFragment)
  → 解锁时间回溯能力（5秒回溯，300秒冷却）
  → 春秋蝉残影自动装备到空窍（占据1格）
  → 太白云生注意到玩家拥有春秋蝉气息
  → 方源（黑土）态度变化：EstimatedPower+30
  → 解锁"时间之道"支线对话
  → 春秋蝉在夜间发出微弱光芒（视觉效果）

散修营地主线完成(SanXiuCampComplete)
  → 散修营地所有NPC态度+10
  → 获得南疆散修声望+30
  → 太白云生提供"义天山异变"情报
  → 方源（黑土）消失（前往义天山）
  → 商心慈根据关系等级给出不同告别语
  → 解锁义天山区域的前置条件满足
  → 3个游戏日后：义天山异变事件触发
```

### 3.4 Stage4 链条：义天山大战

```
义天山异变(YiTianShanAppears)
  → 天空变为暗红色（持续10秒）
  → 世界消息："义天山异变——各方势力云集南疆！"
  → 义天山区域在世界地表生成（距出生点≥500格）
  → 义天山令物品自动加入玩家背包
  → 正道/魔道NPC开始在义天山聚集
  → 天气变化：日蚀（天空变暗）
  → 散修营地NPC议论义天山事件
  → 商心慈根据关系等级决定是否跟随玩家

义天山副本(YiTianShanDungeon)
  → 使用义天山令进入5层副本
  → 第1层：正道/魔道战场（选择阵营穿过）
  → 第2层：3条分支路线（炼道/战道/智道真传碎片）
  → 第3层：4个侧殿小Boss（影宗刺客/天庭守卫/散修高手/僵尸蛊师）
  → 第4层：盗天迷宫（随机生成，3个机关）
  → 第5层：Boss战（凤九歌或秦百胜，根据阵营）
  → 副本内天气：封闭空间，不受外界影响
  → 副本内NPC：正道/魔道蛊师可交互

大同风(DaTongFeng)
  → 义天山副本第3层完成后触发
  → 120秒极端天气事件（风起→风盛→风极）
  → 玩家需要寻找掩体生存
  → 风眼中掉落稀有材料（大同风晶/天地精华/风之精髓）
  → 所有NPC停止战斗（被风吹散）
  → 大同风结束后：义天山部分崩塌
  → 建筑损坏（方块被吹走，需修复）

方源暴露真面目(FangYuanReveal)
  → 大同风结束后触发
  → 方源从黑土身份恢复真面目
  → 名场面还原："我乃方源！"
  → 选择A（追击方源）：方源逃脱，正道倾向+20
  → 选择B（放走方源）：方源好感度+10，魔道倾向+10
  → 选择C（加入方源）：方源好感度+30，魔道倾向+30
  → 所有在场NPC态度剧变（根据阵营）
  → 影宗NPC开始出现
  → 春秋蝉碎片升级（10秒回溯）

义天山事件完成(YiTianShanComplete)
  → 获得完整真传→解锁真传技能
  → 义天山区域部分崩塌（不可进入区域增加）
  → 正道/魔道NPC撤离
  → 获得战功值（根据表现）
  → 北原通行证物品出现（太白云生给予）
  → 3个游戏日后：北原区域解锁
  → 方源消失（前往北原）
```

### 3.5 Stage5 链条：北原争霸

```
北原初到(NorthDesertArrival)
  → 北原冰原区域在世界边缘生成
  → 新Biome：冰原+暴风雪+永久冻土
  → 新敌人刷新：冰原狼、冻土尸鬼、北原盗匪
  → 天气新增：暴风雪、绝对零度（持续掉血）
  → 黑楼兰王庭城镇出现
  → 北原声望系统解锁
  → 方源以"齐海"身份出现在王庭

王庭结盟(WangTingAlly)
  → 与黑楼兰对话
  → 选择A（效忠王庭）：黑楼兰好感度+20，获得王庭任务线
  → 选择B（合作平等）：黑楼兰好感度+5，获得有限任务
  → 选择C（拒绝）：黑楼兰好感度-10，王庭NPC敌对
  → 王庭商店解锁（北原特色蛊虫/材料）
  → 冰塞川NPC出现（王庭将军）
  → 盗匪袭击事件开始触发

长生天接触(ChangShengTianContact)
  → 长生天势力NPC出现（雪胡老祖/毛里求）
  → 长生天与王庭对立（选择一方）
  → 选择王庭：冰塞川好感度+15，雪胡老祖好感度-20
  → 选择长生天：雪胡老祖好感度+15，黑楼兰好感度-10
  → 选择中立：双方好感度±0，但任务奖励减少
  → 五行大法师NPC出现（中立势力）

太白云生之死(TaiBaiYunShengDeath)
  → 小说最催泪的场景之一
  → 太白云生为保护众人而战
  → 选择A（救助）：太白云生存活（HP=1），后续提供情报
    - 太白云生："谢谢你……老夫还欠你一条命。"
    - 因果业力-30（善行）
    - 太白云生在Stage6提供关键情报
  → 选择B（旁观）：太白云生死亡
    - 太白云生："这就是……蛊师的世界啊……"
    - 获得太白云生遗物（太白丹方+元石×100）
    - 因果业力+50（未能救人）
    - 所有北原NPC态度-10
  → 选择C（偷袭方源）：方源反击，太白云生仍死亡
    - 方源："你以为你能阻止我？"
    - 玩家受到重创（HP降至10%）
    - 因果业力+80（间接害死太白云生）
    - 方源好感度-30
  → 情感影响系统触发：所有北原NPC态度变化
  → 商心慈根据Stage3关系等级有不同反应
  → 太白云生之死成为全服消息

仙僵选择(ImmortalZombieChoice)
  → 僵盟使者NPC出现
  → 选择A（成为仙僵）：
    - 获得仙僵状态：不死（HP不会低于1）+ 阴气伤害+50%
    - 代价：白天持续掉血 + 无法使用治疗蛊虫 + 阳道蛊虫失效
    - 升仙条件变化：需要先解除仙僵状态
    - 因果业力+30（逆天而行）
  → 选择B（拒绝）：无变化，但失去仙僵增益
  → 仙僵状态影响Stage6升仙流程

天庭前奏(HeavenPrelude)
  → 天空出现金色光芒
  → 天庭先遣队NPC出现（铜公/眉公）
  → 世界消息："天庭的使者降临北原……"
  → 天庭使者发布天庭通告
  → 玩家可选择与天庭合作或对抗
  → 天庭浮岛在世界高空隐约可见
  → 3个游戏日后：宿命大战开始
```

### 3.6 Stage6 链条：宿命大战与升仙

```
宿命大战开始(DestinyWarBegin)
  → 天空变为金色（永久，直到宿命碎裂）
  → 天庭浮岛在世界4个象限各生成1个
  → 四柱事件开始（=Terraria四柱）
  → 星宿浮岛（智道小怪）→ 日冕浮岛（火道小怪）
  → 命运浮岛（运道小怪）→ 龙威浮岛（气道小怪）
  → 每个浮岛有16种专属小怪
  → 击败所有浮岛核心后：四柱击败

四柱击败(FourPillarsDown)
  → 所有浮岛崩塌
  → 世界消息："四柱已破——天庭降临！"
  → 天庭使者NPC出现，要求阵营选择
  → 阵营选择（不可逆）：
    A. 正道：与天庭合作，龙公HP+20%但有盟友NPC
    B. 魔道：与影宗合作，龙公HP正常但有暗杀能力
    C. 中立：独自战斗，无盟友但无额外难度

龙公首次战(LongGongPhase1)
  → 龙公Boss出现（HP=250000/300000）
  → **此战必败**（设计如此）
  → 龙公第一阶段：正常战斗
  → 龙公第二阶段（HP<50%）：龙威全开，全屏伤害
  → 玩家HP归零时触发特殊事件：
    - 不是死亡，而是"时间冻结"
    - 春秋蝉自动激活
    - 画面回溯到战斗开始前
    - 玩家获得"宿命回溯"Buff（全属性+30%，持续300秒）

春秋蝉回溯(ChunQiuRebirth)
  → 春秋蝉完整版激活
  → 时间回溯到龙公战开始前
  → 玩家保留战斗记忆（知道龙公的攻击模式）
  → 获得"宿命回溯"Buff
  → 春秋蝉升级为完整版（15秒回溯，120秒冷却）
  → 世界消息："春秋蝉……逆转了时间！"

龙公二次战(LongGongPhase2)
  → 龙公Boss再次出现
  → 玩家有"宿命回溯"Buff
  → 龙公攻击模式与首次相同，但玩家已知晓
  → 击败龙公后：
    - 龙公："你……竟然逆转了宿命！"
    - 获得龙公掉落物（龙鳞×10+龙公真传碎片）
    - DownBossSystem.downedLongGong = true

宿命碎裂(DestinyShattered)
  → 龙公击败后触发
  → 宿命蛊碎裂动画（10秒）：
    1. 天空出现巨大裂缝
    2. 金色丝线从天空断裂
    3. 所有NPC仰望天空
    4. 世界消息："宿命……碎了！"
  → 天空裂缝永久存在（视觉效果）
  → 所有NPC行为变化（不再受宿命约束）
  → 天劫难度+30%（天意不再约束）
  → 运势波动增大（不再被宿命稳定）
  → 解锁升仙条件

升仙(Ascension)
  → 血劫Boss战（6波天劫+血海幻影Boss）
  → 通过后：升仙动画（10秒）
    1. 玩家身体发光
    2. 天空出现仙光
    3. 所有NPC仰望天空
    4. 世界消息："天地共鸣——你已升仙！"
  → 真元→仙元转换（真元×100=仙元）
  → 仙窍子世界解锁
  → 太古荒兽开始在世界刷新
  → 世界边缘生成荒兽领地
  → 所有NPC对话增加"升仙"内容
  → 天劫难度永久+30%
  → DownBossSystem.hasAscended = true
```

### 3.7 Stage7 链条：蛊仙之路（原创）

```
七转开始(SevenTurnBegin)
  → 仙窍入口物品自动获得
  → 仙窍子世界可用（个人空间）
  → 太古荒兽在世界刷新（7种）
  → 荒兽领地生成（世界边缘）
  → 混沌海区域解锁（世界最远处）
  → 新敌人：混沌蠕虫/混沌水母/混沌精灵
  → 天气新增：混沌侵蚀（随机负面效果）

仙窍建设完成(ApertureBuilt)
  → 仙窍升级到3级
  → 解锁道痕修炼场
  → 解锁仙窍入侵事件（其他玩家/NPC可入侵）
  → 仙窍内可种植/炼丹/锻造
  → 仙窍内NPC可招募（仙窍管家）

八转开始(EightTurnBegin)
  → 道痕共鸣事件触发
  → 魂劫天劫（5波+灵魂战）
  → 通过后突破八转
  → 解锁道主争夺系统
  → 道主NPC开始出现（每个道系1个道主）
  → 挑战道主：胜则成为新道主，败则损失道痕

道主争夺(DaoLordChallenge)
  → 选择一个道系挑战道主
  → 道主战为1v1（不可带盟友）
  → 成为道主后：
    - 获得道主技能（终极道系技能）
    - 该道系所有蛊虫效果+20%
    - 其他道系蛊师对你态度变化
    - 解锁道主专属商店
  → 最多同时持有3个道主地位

九转开始(NineTurnBegin)
  → 大道共鸣事件触发
  → 大道劫（9波+道系试炼）
  → 通过后突破九转
  → 解锁尊者之战系列Boss
  → 尊者NPC开始在世界出现
  → 每击败一位尊者：世界永久改变

尊者之战(VenerableBattle)
  → 10位尊者Boss可挑战
  → 每位尊者有独特机制：
    - 元始仙尊：气道·创世之力
    - 星宿仙尊：智道·全知之眼
    - 无极魔尊：律道·规则改写
    - 狂蛮魔尊：变化道·三形态
    - 红莲魔尊：宙道·时间回溯
    - 元莲仙尊：木道·无限再生
    - 盗天魔尊：偷道·窃取Buff
    - 巨阳仙尊：运道·随机命运
    - 幽魂魔尊：魂道·灵魂攻击
    - 乐土仙尊：土道·绝对防御
  → 击败5位尊者后：天意异变事件触发

十转终局(TenTurnFinale)
  → 天意本体Boss出现（HP=2000000）
  → 击败天意后：永生之门出现
  → 永生之门谜题：5个核心祭坛+45个辅助祭坛
  → 解开永生之门后：蛊界意志Boss（5段血条×1000000）
  → 击败蛊界意志核心后：三结局选择

结局选择(EndingChosen)
  → 结局A：成为新天意
    - 玩家成为世界管理者
    - 可修改世界规则（天气/敌人/掉落）
    - NPC对玩家敬畏
    - 成就："天道轮回"
  → 结局B：超脱蛊界
    - 玩家离开蛊界
    - 世界继续运转（NPC自治）
    - 玩家可随时返回
    - 成就："超脱者"
  → 结局C：摧毁天地大蛊
    - 世界崩塌30秒倒计时
    - 选择重塑或放任
    - 重塑：世界重生，NPC获得自由意志
    - 放任：世界重置（保留角色数据）
    - 成就："终结与新生"
  → 任意结局后：新游戏+可选
```

---

## 4. 系统间的串联关系图

```
                    ┌─────────────┐
                    │  StoryPhase  │ ← 主线阶段（驱动一切）
                    └──────┬──────┘
                           │
           ┌───────────────┼───────────────┐
           │               │               │
    ┌──────▼──────┐ ┌──────▼──────┐ ┌──────▼──────┐
    │ 系统解锁    │ │ 世界变化    │ │ NPC变化     │
    │ (新能力)    │ │ (新区域)    │ │ (新对话)    │
    └──────┬──────┘ └──────┬──────┘ └──────┬──────┘
           │               │               │
    ┌──────▼──────┐ ┌──────▼──────┐ ┌──────▼──────┐
    │QiRealmPlayer│ │BiomeSystem  │ │DialogueTree │
    │KongQiaoPlayer│ │WorldGen     │ │GuMasterBase │
    │QiResourcePlayer│ │WeatherSystem│ │BeliefState  │
    │ChunQiuChanPlayer│ │DownBossSystem│ │NPCSocialNet │
    └──────┬──────┘ └──────┬──────┘ └──────┬──────┘
           │               │               │
           └───────────────┼───────────────┘
                           │
                    ┌──────▼──────┐
                    │  EventBus   │ ← 事件总线（串联所有系统）
                    └──────┬──────┘
                           │
    ┌──────────┬───────────┼───────────┬──────────┐
    │          │           │           │          │
┌───▼───┐ ┌───▼───┐ ┌────▼────┐ ┌───▼───┐ ┌───▼───┐
│QuestSys│ │KarmaSys│ │FactionSys│ │Tribulation│ │LootSys│
│任务系统│ │因果系统│ │势力系统  │ │天劫系统  │ │掉落系统│
└───┬───┘ └───┬───┘ └────┬────┘ └───┬───┘ └───┬───┘
    │         │          │          │         │
    └─────────┴──────────┴──────────┴─────────┘
                           │
                    ┌──────▼──────┐
                    │ PlayerState │ ← 玩家状态（所有选择的累积）
                    └─────────────┘
```

**关键串联路径**：

1. **StoryPhase → QuestSystem**：主线阶段决定可用任务
2. **StoryPhase → DialogueTree**：主线阶段决定NPC对话内容
3. **StoryPhase → BiomeSystem**：主线阶段决定可进入区域
4. **StoryPhase → DownBossSystem**：主线阶段决定可挑战Boss
5. **QuestSystem → FactionSystem**：完成任务影响声望
6. **FactionSystem → DialogueTree**：声望影响NPC态度
7. **KarmaSystem → TribulationSystem**：因果影响天劫难度
8. **DownBossSystem → WorldGen**：Boss击败触发世界变化
9. **EventBus → All Systems**：事件驱动系统间通信

---

## 5. 现有系统串联改造清单

### 5.1 StoryPhase扩展（🔴最高优先级）

**现状**：StoryPhase只有7个阶段，全部在Stage1
**改造**：扩展为覆盖全部7个Stage的完整枚举（见§1）
**影响文件**：
- `Common/DialogueTree/StoryPhase.cs` — 扩展枚举
- `Common/DialogueTree/StoryManager.cs` — 扩展阶段推进逻辑
- 所有NPC对话文件 — 绑定新阶段

### 5.2 DownBossSystem扩展（🔴最高优先级）

**现状**：只有11个尊者Boss标记
**改造**：添加Stage1-6所有Boss标记

```csharp
// 需要新增的Boss标记
public static bool downedEarthVeinGuardian;    // 地脉守护者（Stage2 Hardmode触发）
public static bool downedHuaJiuXingZhe;        // 花酒行者（Stage1 可选Boss）
public static bool downedIronBoneKing;         // 铁骨王·意志（Stage3 传承Boss）
public static bool downedPoisonSnakeKing;      // 毒蛇王·意志（Stage3 传承Boss）
public static bool downedPhantomKing;          // 幻影王·意志（Stage3 传承Boss）
public static bool downedFengJiuGe;            // 凤九歌（Stage4 义天山Boss）
public static bool downedQinBaiSheng;          // 秦百胜（Stage4 义天山Boss）
public static bool downedHeiLouLan;            // 黑楼兰（Stage5 北原Boss）
public static bool downedBingSaiChuan;         // 冰塞川（Stage5 北原Boss）
public static bool downedWuXingDaFaShi;        // 五行大法师（Stage5 北原Boss）
public static bool downedLongGong;             // 龙公（Stage6 宿命大战Boss）
public static bool downedBloodSeaPhantom;      // 血海幻影（Stage6 血劫Boss）
public static bool destinyShattered;           // 宿命碎裂标记
public static bool hasAscended;                // 升仙标记
```

### 5.3 QuestSystem主线任务注册（🔴最高优先级）

**现状**：只有37个家族日常任务，没有主线任务
**改造**：注册主线任务链

每个主线任务包含：任务ID、名称、描述、目标类型、目标详情、奖励、触发条件、完成后推进的StoryPhase

```csharp
// 主线任务注册示例
public static void RegisterMainQuests()
{
    // === Stage1: 一转·蛊师入门 ===
    RegisterQuest(new QuestData
    {
        ID = "MQ-01",
        Name = "开窍仪式",
        Description = "参加古月山寨的开窍仪式，测试你的资质",
        Type = QuestType.MainStory,
        Objectives = new() { new(QuestObjectiveType.TalkToNPC, "学堂家老", 1) },
        Rewards = new() { new(QuestRewardType.Item, "月蛊", 1), new(QuestRewardType.QiExp, 100) },
        RequiredPhase = StoryPhase.Arrival,
        AdvanceToPhase = StoryPhase.AwakeningCeremony
    });

    RegisterQuest(new QuestData
    {
        ID = "MQ-02",
        Name = "学堂入门",
        Description = "进入学堂，学习蛊师基础知识",
        Type = QuestType.MainStory,
        Objectives = new()
        {
            new(QuestObjectiveType.TalkToNPC, "学堂教头", 1),
            new(QuestObjectiveType.CollectItem, "元石", 5),
            new(QuestObjectiveType.KillNPCType, "荒兽", 3)
        },
        Rewards = new() { new(QuestRewardType.Item, "蛊虫饲料×10", 1), new(QuestRewardType.QiExp, 200) },
        RequiredPhase = StoryPhase.AwakeningCeremony,
        AdvanceToPhase = StoryPhase.SchoolTraining
    });

    RegisterQuest(new QuestData
    {
        ID = "MQ-03",
        Name = "药堂求助",
        Description = "药堂家老需要帮助采集药材，这是获得家族认可的机会",
        Type = QuestType.MainStory,
        Objectives = new()
        {
            new(QuestObjectiveType.TalkToNPC, "药堂家老", 1),
            new(QuestObjectiveType.CollectItem, "灵草", 3),
            new(QuestObjectiveType.ReachRepLevel, "古月家族", 1)
        },
        Rewards = new() { new(QuestRewardType.Item, "治疗蛊虫", 1), new(QuestRewardType.Reputation, "古月+5") },
        RequiredPhase = StoryPhase.SchoolTraining,
        AdvanceToPhase = StoryPhase.MedicineRequest
    });

    RegisterQuest(new QuestData
    {
        ID = "MQ-04",
        Name = "贾金生之死",
        Description = "贾金生被发现死在荒野中，你需要做出选择",
        Type = QuestType.MainStory,
        Objectives = new()
        {
            new(QuestObjectiveType.TalkToNPC, "铁血冷", 1),
            new(QuestObjectiveType.SearchNode, "贾金生尸体", 1),
            new(QuestObjectiveType.TalkToNPC, "方源", 1)
        },
        Rewards = new() { new(QuestRewardType.Item, "贾金生的遗物", 1), new(QuestRewardType.QiExp, 500) },
        RequiredPhase = StoryPhase.MedicineRequest,
        AdvanceToPhase = StoryPhase.JiaJinShengDeath
    });

    RegisterQuest(new QuestData
    {
        ID = "MQ-05",
        Name = "花酒行者传承",
        Description = "在青茅山深处发现了花酒行者的传承",
        Type = QuestType.MainStory,
        Objectives = new()
        {
            new(QuestObjectiveType.EnterTerritory, "花酒洞府", 1),
            new(QuestObjectiveType.KillNPC, "花酒行者·意志", 1),
            new(QuestObjectiveType.CollectItem, "花酒宝箱", 1)
        },
        Rewards = new() { new(QuestRewardType.Item, "洗髓蛊", 1), new(QuestRewardType.Item, "二转蛊虫选择箱", 1) },
        RequiredPhase = StoryPhase.JiaJinShengDeath,
        AdvanceToPhase = StoryPhase.HuaJiuInheritance
    });

    RegisterQuest(new QuestData
    {
        ID = "MQ-06",
        Name = "家族认可",
        Description = "获得古月家族的正式认可，成为正式蛊师",
        Type = QuestType.MainStory,
        Objectives = new()
        {
            new(QuestObjectiveType.ReachRepLevel, "古月家族", 3),
            new(QuestObjectiveType.TalkToNPC, "古月博", 1),
            new(QuestObjectiveType.CraftItem, "家族信物", 1)
        },
        Rewards = new() { new(QuestRewardType.Reputation, "古月+20"), new(QuestRewardType.Item, "古月家族令", 1) },
        RequiredPhase = StoryPhase.HuaJiuInheritance,
        AdvanceToPhase = StoryPhase.FamilyRecognition
    });

    // === Stage2: 二转·家族争锋 ===
    RegisterQuest(new QuestData
    {
        ID = "MQ-07", Name = "三寨大比",
        Description = "参加青茅山三寨大比，展示你的实力",
        Type = QuestType.MainStory,
        Objectives = new()
        {
            new(QuestObjectiveType.TalkToNPC, "古月博", 1),
            new(QuestObjectiveType.KillNPC, "淘汰赛对手", 1),
            new(QuestObjectiveType.KillNPC, "决赛对手", 1)
        },
        Rewards = new() { new(QuestRewardType.Item, "元石×50", 1), new(QuestRewardType.QiExp, 1000) },
        RequiredPhase = StoryPhase.PreTournament,
        AdvanceToPhase = StoryPhase.TournamentFinal
    });

    RegisterQuest(new QuestData
    {
        ID = "MQ-08", Name = "天鹤来袭",
        Description = "天鹤上人袭击青茅山，保卫家园！",
        Type = QuestType.MainStory,
        Objectives = new()
        {
            new(QuestObjectiveType.SurviveWave, "天鹤攻击", 3),
            new(QuestObjectiveType.KillNPC, "鹤群", 10)
        },
        Rewards = new() { new(QuestRewardType.Item, "风道材料×5", 1), new(QuestRewardType.Reputation, "古月+10") },
        RequiredPhase = StoryPhase.TournamentFinal,
        AdvanceToPhase = StoryPhase.TianHeAttack
    });

    RegisterQuest(new QuestData
    {
        ID = "MQ-09", Name = "白凝冰的抉择",
        Description = "白凝冰的空窍无法支撑，她做出了惊人的决定",
        Type = QuestType.MainStory,
        Objectives = new() { new(QuestObjectiveType.TalkToNPC, "白凝冰", 1) },
        Rewards = new() { new(QuestRewardType.Item, "冰魄碎片×5", 1) },
        RequiredPhase = StoryPhase.TianHeAttack,
        AdvanceToPhase = StoryPhase.BaiNingBingIceSeal
    });

    RegisterQuest(new QuestData
    {
        ID = "MQ-10", Name = "血祭之夜",
        Description = "青茅山发生了不可挽回的事件……",
        Type = QuestType.MainStory,
        Objectives = new()
        {
            new(QuestObjectiveType.TalkToNPC, "方源", 1),
            new(QuestObjectiveType.SurviveWave, "血祭", 5)
        },
        Rewards = new() { new(QuestRewardType.QiExp, 2000) },
        RequiredPhase = StoryPhase.BaiNingBingIceSeal,
        AdvanceToPhase = StoryPhase.BloodSacrifice
    });

    RegisterQuest(new QuestData
    {
        ID = "MQ-11", Name = "地脉守护者",
        Description = "击败地脉守护者，打破青茅山的封印",
        Type = QuestType.MainStory,
        Objectives = new() { new(QuestObjectiveType.KillNPC, "地脉守护者", 1) },
        Rewards = new() { new(QuestRewardType.Item, "地脉精华", 1), new(QuestRewardType.QiExp, 3000) },
        RequiredPhase = StoryPhase.BloodSacrifice,
        AdvanceToPhase = StoryPhase.LeftQingMao
    });

    RegisterQuest(new QuestData
    {
        ID = "MQ-12", Name = "离开青茅山",
        Description = "青茅山已不再安全，是时候踏入更广阔的世界了",
        Type = QuestType.MainStory,
        Objectives = new()
        {
            new(QuestObjectiveType.TalkToNPC, "古月博", 1),
            new(QuestObjectiveType.CollectItem, "推荐信", 1)
        },
        Rewards = new() { new(QuestRewardType.Item, "推荐信", 1), new(QuestRewardType.StoryProgress, "Hardmode", 0) },
        RequiredPhase = StoryPhase.LeftQingMao,
        AdvanceToPhase = StoryPhase.SouthBorderArrival
    });

    // === Stage3: 三转·南疆流浪 ===
    RegisterQuest(new QuestData
    {
        ID = "MQ-13", Name = "南疆初到",
        Description = "到达南疆散修营地，开始新的生活",
        Type = QuestType.MainStory,
        Objectives = new()
        {
            new(QuestObjectiveType.EnterTerritory, "南疆荒野", 1),
            new(QuestObjectiveType.TalkToNPC, "太白云生", 1)
        },
        Rewards = new() { new(QuestRewardType.Reputation, "散修+10"), new(QuestRewardType.QiExp, 1500) },
        RequiredPhase = StoryPhase.SouthBorderArrival,
        AdvanceToPhase = StoryPhase.ShangXinCiMeet
    });

    RegisterQuest(new QuestData
    {
        ID = "MQ-14", Name = "商心慈",
        Description = "与商心慈建立关系，了解南疆的局势",
        Type = QuestType.MainStory,
        Objectives = new()
        {
            new(QuestObjectiveType.TalkToNPC, "商心慈", 3),
            new(QuestObjectiveType.CollectItem, "南疆药材", 5),
            new(QuestObjectiveType.KillNPCType, "南疆毒蛇", 5)
        },
        Rewards = new() { new(QuestRewardType.Item, "商心慈的信物", 1), new(QuestRewardType.Reputation, "散修+15") },
        RequiredPhase = StoryPhase.ShangXinCiMeet,
        AdvanceToPhase = StoryPhase.ThreeKingsInheritance
    });

    RegisterQuest(new QuestData
    {
        ID = "MQ-15", Name = "三王传承",
        Description = "完成三王传承副本，获得真传力量",
        Type = QuestType.MainStory,
        Objectives = new()
        {
            new(QuestObjectiveType.KillNPC, "传承守护者", 1),
            new(QuestObjectiveType.CollectItem, "真传碎片", 1)
        },
        Rewards = new() { new(QuestRewardType.Item, "真传碎片×3", 1), new(QuestRewardType.QiExp, 5000) },
        RequiredPhase = StoryPhase.ThreeKingsInheritance,
        AdvanceToPhase = StoryPhase.ChunQiuChanFragment
    });

    RegisterQuest(new QuestData
    {
        ID = "MQ-16", Name = "春秋蝉",
        Description = "在三王传承深处发现了春秋蝉的残影",
        Type = QuestType.MainStory,
        Objectives = new() { new(QuestObjectiveType.CollectItem, "春秋蝉·残影", 1) },
        Rewards = new() { new(QuestRewardType.Item, "春秋蝉·残影", 1), new(QuestRewardType.QiExp, 3000) },
        RequiredPhase = StoryPhase.ChunQiuChanFragment,
        AdvanceToPhase = StoryPhase.SanXiuCampComplete
    });

    // === Stage4~6 简略注册（结构相同） ===
    RegisterQuest(new QuestData { ID = "MQ-17", Name = "义天山异变", RequiredPhase = StoryPhase.YiTianShanAppears, AdvanceToPhase = StoryPhase.YiTianShanDungeon });
    RegisterQuest(new QuestData { ID = "MQ-18", Name = "大同风", RequiredPhase = StoryPhase.DaTongFeng, AdvanceToPhase = StoryPhase.FangYuanReveal });
    RegisterQuest(new QuestData { ID = "MQ-19", Name = "方源真面目", RequiredPhase = StoryPhase.FangYuanReveal, AdvanceToPhase = StoryPhase.YiTianShanComplete });
    RegisterQuest(new QuestData { ID = "MQ-20", Name = "义天山终战", RequiredPhase = StoryPhase.YiTianShanComplete, AdvanceToPhase = StoryPhase.NorthDesertArrival });
    RegisterQuest(new QuestData { ID = "MQ-21", Name = "北原初到", RequiredPhase = StoryPhase.NorthDesertArrival, AdvanceToPhase = StoryPhase.WangTingAlly });
    RegisterQuest(new QuestData { ID = "MQ-22", Name = "王庭结盟", RequiredPhase = StoryPhase.WangTingAlly, AdvanceToPhase = StoryPhase.ChangShengTianContact });
    RegisterQuest(new QuestData { ID = "MQ-23", Name = "长生天", RequiredPhase = StoryPhase.ChangShengTianContact, AdvanceToPhase = StoryPhase.TaiBaiYunShengDeath });
    RegisterQuest(new QuestData { ID = "MQ-24", Name = "太白云生", RequiredPhase = StoryPhase.TaiBaiYunShengDeath, AdvanceToPhase = StoryPhase.HeavenPrelude });
    RegisterQuest(new QuestData { ID = "MQ-25", Name = "天庭前奏", RequiredPhase = StoryPhase.HeavenPrelude, AdvanceToPhase = StoryPhase.DestinyWarBegin });
    RegisterQuest(new QuestData { ID = "MQ-26", Name = "宿命大战", RequiredPhase = StoryPhase.DestinyWarBegin, AdvanceToPhase = StoryPhase.FourPillarsDown });
    RegisterQuest(new QuestData { ID = "MQ-27", Name = "龙公之战", RequiredPhase = StoryPhase.LongGongPhase1, AdvanceToPhase = StoryPhase.ChunQiuRebirth });
    RegisterQuest(new QuestData { ID = "MQ-28", Name = "再战龙公", RequiredPhase = StoryPhase.LongGongPhase2, AdvanceToPhase = StoryPhase.DestinyShattered });
    RegisterQuest(new QuestData { ID = "MQ-29", Name = "宿命碎裂", RequiredPhase = StoryPhase.DestinyShattered, AdvanceToPhase = StoryPhase.Ascension });
    RegisterQuest(new QuestData { ID = "MQ-30", Name = "升仙", RequiredPhase = StoryPhase.Ascension, AdvanceToPhase = StoryPhase.SevenTurnBegin });
}
```

### 5.4 NPC对话树与主线绑定（🟡高优先级）

**现状**：对话树框架完善但内容几乎为空
**改造**：为每个关键NPC创建阶段感知的对话树

**对话树阶段感知机制**：
```csharp
// 在DialogueTreeBuilder中添加阶段条件
public DialogueTreeBuilder AddStageCondition(StoryPhase minPhase, StoryPhase maxPhase)
{
    // 只有在指定阶段范围内才显示此对话选项
    _currentNode.Condition = new StoryPhaseCondition(minPhase, maxPhase);
    return this;
}

// 示例：方源的对话树
var fangYuanDialogue = new DialogueTreeBuilder()
    .Root("方源看着你，眼神深邃。")
    .Option("你好")
        .AddStageCondition(StoryPhase.SchoolTraining, StoryPhase.FamilyRecognition)
        .Response("嗯。")
    .Option("贾金生的事……")
        .AddStageCondition(StoryPhase.JiaJinShengDeath, StoryPhase.BloodSacrifice)
        .Response("你对他很感兴趣？为什么？")
        .Option("我只是好奇")
            .Response("好奇心是好事。但要小心，好奇心也会害死猫。")
        .Option("我觉得你和他有关")
            .Response("……你比其他人聪明。但聪明人往往死得更快。")
            .Effect(belief => belief.RiskThreshold += 10)
    .Option("你到底是谁？")
        .AddStageCondition(StoryPhase.BloodSacrifice, StoryPhase.BloodSacrifice)
        .Response("我是方源。一个想要活下去的人。仅此而已。")
    .Option("黑土……是你吗？")
        .AddStageCondition(StoryPhase.SouthBorderArrival, StoryPhase.YiTianShanAppears)
        .Response("……你认出我了。有意思。")
        .Effect(belief => belief.EstimatedPower += 20)
    .Option("方源，我来了。")
        .AddStageCondition(StoryPhase.YiTianShanAppears, StoryPhase.YiTianShanComplete)
        .Response("你来了。我等你很久了。")
    .Build();
```

### 5.5 世界生成与主线绑定（🟡高优先级）

**现状**：只有青茅山生物群落
**改造**：每个区域绑定到主线阶段

```
世界生成策略：

初始世界（StoryPhase.NotEntered）：
  - 仅生成青茅山区域
  - 其他区域被"迷雾"遮挡（不可见/不可进入）

Stage2解锁（StoryPhase.LeftQingMao）：
  - Hardmode激活
  - 南疆区域从迷雾中显现
  - 新矿脉生成

Stage3解锁（StoryPhase.SouthBorderArrival）：
  - 南疆散修营地激活
  - 三王传承入口出现

Stage4解锁（StoryPhase.YiTianShanAppears）：
  - 义天山区域生成
  - 天空出现异象

Stage5解锁（StoryPhase.NorthDesertArrival）：
  - 北原冰原区域生成
  - 王庭城镇激活

Stage6解锁（StoryPhase.DestinyWarBegin）：
  - 天庭浮岛生成
  - 四柱出现

Stage7解锁（StoryPhase.SevenTurnBegin）：
  - 荒兽领地生成
  - 仙窍子世界可用
```

### 5.6 天气/季节与主线绑定（🟢中优先级）

```
天气系统与剧情联动：

Stage1（青茅山）：
  - 默认天气：晴/多云/雨
  - 花酒传承时：雷暴（天意感应）
  - 血祭事件时：血月+灵潮

Stage2（三寨大比）：
  - 大比期间：晴朗（天意眷顾）
  - 天鹤来袭：大风+雷暴
  - 白凝冰冰封：暴风雪

Stage3（南疆）：
  - 新天气：雾/灵潮
  - 三王传承时：地脉震动

Stage4（义天山）：
  - 大同风：极端天气事件
  - 义天山异变：日蚀

Stage5（北原）：
  - 新天气：暴风雪/绝对零度
  - 天庭前奏：金色天空

Stage6（宿命大战）：
  - 四柱事件：对应颜色天气
  - 宿命碎裂：天空裂缝（永久）
  - 升仙：仙光普照

Stage7（原创）：
  - 混沌海：混沌侵蚀天气
  - 天意战：规则改写天气
```

---

## 6. 关键选择与后果追踪系统

**新系统**：`Common/Systems/ChoiceTrackerSystem.cs`

每个关键选择都有唯一ID，记录选择结果，影响后续所有内容：

```csharp
public class ChoiceTrackerSystem : ModSystem
{
    public static Dictionary<string, int> Choices = new();

    public static void MakeChoice(string choiceID, int choiceValue)
    {
        Choices[choiceID] = choiceValue;
        EventBus.Publish(new ChoiceMadeEvent(choiceID, choiceValue));
    }

    public static int GetChoice(string choiceID)
    {
        return Choices.TryGetValue(choiceID, out var v) ? v : -1;
    }

    public static bool HasChoice(string choiceID)
    {
        return Choices.ContainsKey(choiceID);
    }
}
```

**关键选择ID列表**：

| 选择ID | 阶段 | 选择内容 | 影响范围 |
|--------|------|---------|---------|
| `S1_JiaJinSheng` | Stage1 | 帮助方源/帮助铁血冷/旁观 | 方源好感度、铁血冷信任 |
| `S1_HuaJiuShare` | Stage1 | 分享花酒传承/独占 | 家族声望、其他家族态度 |
| `S2_TournamentResult` | Stage2 | 冠军/亚军/未进决赛 | 白凝冰态度、古月博信任 |
| `S2_BaiNingBing` | Stage2 | 帮助/旁观/敌对 | 白凝冰存活、Stage4解封 |
| `S2_BloodSacrifice` | Stage2 | 加入方源/对抗方源/逃离 | 正道/魔道倾向 |
| `S3_ShangXinCi` | Stage3 | 帮助/利用/无视 | 商心慈关系、太白云生事件 |
| `S4_FangYuanReveal` | Stage4 | 追击方源/放走方源/加入方源 | 阵营倾向 |
| `S5_TaiBaiYunSheng` | Stage5 | 救助/旁观/偷袭方源 | 太白云生存活、因果业力 |
| `S5_ImmortalZombie` | Stage5 | 成为仙僵/拒绝 | 仙僵状态、升仙条件 |
| `S6_FactionChoice` | Stage6 | 正道/魔道/中立 | Stage6-7全部内容 |
| `S7_Ending` | Stage7 | 成为天意/超脱/摧毁 | 终局 |

---

---

## 7. 关键NPC跨阶段状态变化表

以下核心NPC的状态随主线推进而变化，所有变化通过ChoiceTrackerSystem和StoryPhase联动：

### 7.1 方源（贯穿全剧的核心NPC）

| 阶段 | 身份 | 对玩家态度 | 关键行为 | 对话风格 |
|------|------|-----------|---------|---------|
| SchoolTraining | 学堂同学 | 冷淡/观察 | 独来独往，不与人交往 | 简短、冷淡 |
| JiaJinShengDeath | 被怀疑者 | 根据选择变化 | 若帮助方源→微微认可；若对抗→警惕 | 试探性 |
| HuaJiuInheritance | 竞争者 | 评估实力 | 观察玩家是否获得传承 | 评估性 |
| BloodSacrifice | 真面目初现 | 震撼/威胁 | 血祭事件中暴露实力 | 霸道、直接 |
| SouthBorderArrival | 黑土 | 隐藏/试探 | 以黑土身份出现，暗中观察 | 伪装、温和 |
| YiTianShanAppears | 方源 | 正面交锋 | 暴露真面目，名场面 | 傲慢、自信 |
| NorthDesertArrival | 齐海 | 再次伪装 | 以齐海身份出现在北原 | 伪装、谨慎 |
| DestinyWarBegin | 宿命操控者 | 重视/利用 | 在宿命大战中扮演关键角色 | 深沉、算计 |
| Ascension | 五阶段Boss | 最终对决 | 作为终极Boss与玩家决战 | 巅峰、超然 |

**方源好感度影响链**：

| 好感度范围 | Stage1-2影响 | Stage3-4影响 | Stage5-6影响 |
|-----------|-------------|-------------|-------------|
| 0~20（陌生） | 不主动对话 | 黑土身份不透露 | 不提供情报 |
| 21~50（认识） | 偶尔搭话 | 黑土暗示身份 | 提供有限情报 |
| 51~80（认可） | 主动分享信息 | 黑土承认身份 | 提供关键情报 |
| 81~100（信任） | 提供独家信息 | 主动合作 | 成为临时盟友 |

### 7.2 白凝冰

| 阶段 | 状态 | 对玩家态度 | 关键行为 |
|------|------|-----------|---------|
| TournamentFinal | 对手 | 竞争/好奇 | 三寨大比决赛对手 |
| BaiNingBingIceSeal | 冰封/自爆 | 根据选择变化 | 冰封自身或自爆 |
| YiTianShanAppears | 解封（若冰封） | 根据好感度 | 义天山再出现 |
| DestinyWarBegin | 天庭阵营 | 根据好感度 | 可能助战或敌对 |

**白凝冰存活条件**：ChoiceTracker.GetChoice("S2_BaiNingBing") != 2（未选择敌对）

### 7.3 太白云生

| 阶段 | 状态 | 对玩家态度 | 关键行为 |
|------|------|-----------|---------|
| SouthBorderArrival | 散修营地长老 | 友好 | 提供南疆情报 |
| ThreeKingsInheritance | 导师 | 信任 | 指引三王传承 |
| NorthDesertArrival | 旅伴 | 亲密 | 跟随玩家到北原 |
| TaiBaiYunShengDeath | 生死关头 | 感激/绝望 | 根据选择存活或死亡 |
| DestinyWarBegin | 情报提供者（若存活） | 忠诚 | 提供天庭情报 |

**太白云生存活条件**：ChoiceTracker.GetChoice("S5_TaiBaiYunSheng") == 0（选择救助）

### 7.4 商心慈

| 阶段 | 状态 | 对玩家态度 | 关键行为 |
|------|------|-----------|---------|
| ShangXinCiMeet | 陌生 | 根据选择 | 初次相遇 |
| ThreeKingsInheritance | 朋友/利用对象 | 根据好感度 | 提供南疆情报 |
| TaiBaiYunShengDeath | 悲伤 | 情感波动 | 对太白云生之死的反应 |
| YiTianShanComplete | 离别 | 根据好感度 | 告别方式不同 |

### 7.5 古月博

| 阶段 | 状态 | 对玩家态度 | 关键行为 |
|------|------|-----------|---------|
| Arrival | 族长 | 审视 | 决定是否允许玩家留下 |
| FamilyRecognition | 认可 | 信任 | 授予正式身份 |
| PreTournament | 导师 | 指导 | 安排三寨大比参赛 |
| BloodSacrifice | 震惊 | 根据选择 | 面对方源真面目 |
| LeftQingMao | 留守 | 感慨 | 留在青茅山 |

---

## 8. 选择后果跨阶段影响链

每个关键选择不仅影响当前阶段，还会在后续阶段产生连锁反应：

### 8.1 S1_JiaJinSheng（贾金生之死）

```
选择A（帮助方源）→ 方源好感度+20
  → Stage2: 方源在血祭事件中不攻击玩家
  → Stage3: 黑土主动向玩家透露身份
  → Stage4: 方源在义天山给玩家留一条生路
  → Stage6: 方源在宿命大战中提供关键情报
  → 最终影响: 方源五阶段Boss战难度-10%

选择B（帮助铁血冷）→ 铁血冷信任+20
  → Stage2: 铁血冷提供额外家族任务
  → Stage3: 铁血冷写信告知南疆情报
  → Stage4: 铁血冷在义天山协助防守
  → Stage6: 铁血冷在宿命大战中助战
  → 最终影响: 正道路线盟友+1

选择C（旁观）→ 方源好感度+5
  → Stage2: 方源对玩家保持距离
  → Stage3: 黑土不主动接触玩家
  → Stage4: 方源对玩家态度中立
  → Stage6: 方源不提供情报也不阻挠
  → 最终影响: 中立路线，无额外盟友也无额外敌人
```

### 8.2 S2_BloodSacrifice（血祭事件）

```
选择A（加入方源）→ 魔道倾向+30
  → Stage3: 散修营地NPC态度-10（风声传出）
  → Stage4: 影宗NPC主动接触玩家
  → Stage5: 僵盟使者优先出现
  → Stage6: 魔道阵营自动选择
  → Stage7: 影宗道主友好

选择B（对抗方源）→ 正道倾向+30
  → Stage3: 散修营地NPC态度+10
  → Stage4: 天庭NPC主动接触玩家
  → Stage5: 王庭NPC更信任玩家
  → Stage6: 正道阵营自动选择
  → Stage7: 天庭道主友好

选择C（逃离）→ 中立倾向+20
  → Stage3: 散修营地NPC态度±0
  → Stage4: 双方NPC都不主动接触
  → Stage5: 需要自己建立关系
  → Stage6: 可自由选择阵营
  → Stage7: 全部道主中立
```

### 8.3 S5_TaiBaiYunSheng（太白云生之死）

```
选择A（救助）→ 太白云生存活
  → Stage5: 太白云生感谢玩家，提供北原情报
  → Stage6: 太白云生提供天庭弱点情报（龙公战难度-15%）
  → Stage6: 商心慈好感度+20（感激你救了太白云生）
  → Stage7: 太白云生在道主争夺中支持玩家
  → 因果业力: -30（善行）

选择B（旁观）→ 太白云生死亡
  → Stage5: 获得太白云生遗物（太白丹方+元石×100）
  → Stage6: 无太白云生情报（龙公战难度+10%）
  → Stage6: 商心慈好感度-20（怨恨你未救太白云生）
  → Stage7: 太白云生位置空缺，需自己争取
  → 因果业力: +50（未能救人）

选择C（偷袭方源）→ 方源反击，太白云生仍死
  → Stage5: 玩家受重创（HP降至10%），方源好感度-30
  → Stage6: 方源在宿命大战中针对玩家（龙公战难度+20%）
  → Stage6: 商心慈好感度-40（怨恨你间接害死太白云生）
  → Stage7: 方源成为额外敌人
  → 因果业力: +80（间接害死太白云生）
```

### 8.4 S6_FactionChoice（阵营选择）

```
选择A（正道）→ 与天庭合作
  → Stage6: 龙公HP+20%但有铜公+眉公助战
  → Stage6: 星宿/日冕浮岛小怪伤害-20%
  → Stage7: 天庭道主让位（友好竞争）
  → Stage7: 仙窍入侵概率-50%（天庭保护）
  → Stage7终局: 偏向结局A（成为天意）

选择B（魔道）→ 与影宗合作
  → Stage6: 龙公HP正常+影无邪+砚石老人助战
  → Stage6: 命运/龙威浮岛小怪伤害-20%
  → Stage7: 影宗道主协助（暗中帮助）
  → Stage7: 影宗提供提前预警（仙窍入侵）
  → Stage7终局: 偏向结局B（超脱）

选择C（中立）→ 独自战斗
  → Stage6: 龙公HP正常但无盟友
  → Stage6: 所有浮岛小怪伤害正常
  → Stage7: 需要自己挑战所有道主
  → Stage7: 无保护无预警
  → Stage7终局: 偏向结局C（摧毁）
```

---

## 9. EventBus事件定义

所有系统间通信通过EventBus进行，以下是主线驱动所需的核心事件：

### 9.1 剧情事件

```csharp
// 剧情阶段推进事件
public class StoryPhaseAdvancedEvent : GuWorldEvent
{
    public StoryPhase OldPhase;
    public StoryPhase NewPhase;
    public int PlayerID;
}

// 关键选择事件
public class ChoiceMadeEvent : GuWorldEvent
{
    public string ChoiceID;
    public int ChoiceValue;  // 0=A, 1=B, 2=C
}

// 系统解锁事件
public class SystemUnlockedEvent : GuWorldEvent
{
    public string SystemName;  // "KongQiao", "QiResource", "Inheritance" etc.
    public int PlayerID;
}
```

### 9.2 世界事件

```csharp
// 区域解锁事件
public class AreaUnlockedEvent : GuWorldEvent
{
    public string AreaName;  // "SouthBorder", "YiTianShan", "NorthDesert" etc.
    public int WorldX;
    public int WorldY;
}

// 天气联动事件
public class StoryWeatherEvent : GuWorldEvent
{
    public string WeatherType;  // "BloodMoon", "SpiritTide", "GoldenSky" etc.
    public int Duration;  // 持续秒数
    public StoryPhase TriggerPhase;
}

// Boss击败事件
public class BossDefeatedEvent : GuWorldEvent
{
    public string BossName;
    public bool FirstTime;
    public StoryPhase RequiredPhase;
}
```

### 9.3 NPC事件

```csharp
// NPC态度变化事件
public class NPCAttitudeChangedEvent : GuWorldEvent
{
    public int NPCID;
    public string AttitudeOld;
    public string AttitudeNew;
    public string Reason;  // "StoryPhase", "Choice", "Karma" etc.
}

// NPC死亡事件（已有NpcDeathHandler，扩展）
public class StoryNPCDeathEvent : GuWorldEvent
{
    public string NPCName;
    public StoryPhase CurrentPhase;
    public bool IsStoryCritical;  // 剧情关键NPC死亡
}

// NPC出现事件
public class NPCSpawnedByStoryEvent : GuWorldEvent
{
    public string NPCName;
    public StoryPhase TriggerPhase;
    public int SpawnX;
    public int SpawnY;
}
```

### 9.4 玩家事件

```csharp
// 境界突破事件
public class BreakthroughEvent : GuWorldEvent
{
    public int OldLevel;
    public int NewLevel;
    public bool IsMajorBreakthrough;  // 大境界突破
}

// 升仙事件
public class AscensionEvent : GuWorldEvent
{
    public int PlayerID;
    public bool IsFirstAscension;  // 服务器首次升仙
}

// 天劫事件（已有HeavenTribulationSystem，扩展）
public class TribulationResultEvent : GuWorldEvent
{
    public int TribulationType;
    public bool Success;
    public StoryPhase RequiredPhase;
}
```

### 9.5 事件订阅关系

| 事件 | 订阅者 | 行为 |
|------|--------|------|
| StoryPhaseAdvancedEvent | QuestSystem | 解锁/锁定任务 |
| StoryPhaseAdvancedEvent | DialogueSystem | 更新NPC对话 |
| StoryPhaseAdvancedEvent | WorldStateMachine | 触发世界事件 |
| StoryPhaseAdvancedEvent | WeatherSystem | 切换天气 |
| ChoiceMadeEvent | ChoiceTrackerSystem | 记录选择 |
| ChoiceMadeEvent | GuWorldPlayer | 更新声望/好感度 |
| ChoiceMadeEvent | KarmaSystem | 更新因果业力 |
| ChoiceMadeEvent | NPCSocialNetwork | 传播选择消息 |
| SystemUnlockedEvent | QiRealmPlayer | 解锁新能力 |
| SystemUnlockedEvent | KongQiaoPlayer | 解锁空窍槽位 |
| AreaUnlockedEvent | WorldGen | 生成新区域 |
| AreaUnlockedEvent | BiomeSystem | 激活新Biome |
| BossDefeatedEvent | DownBossSystem | 记录击败 |
| BossDefeatedEvent | StoryManager | 推进剧情 |
| BossDefeatedEvent | WorldGen | 触发世界变化 |
| NPCAttitudeChangedEvent | DialogueSystem | 更新对话 |
| NPCAttitudeChangedEvent | NPCSocialNetwork | 传播态度变化 |
| BreakthroughEvent | HeavenTribulationSystem | 触发天劫 |
| BreakthroughEvent | StoryManager | 检查阶段推进 |
| AscensionEvent | QiResourcePlayer | 真元→仙元 |
| AscensionEvent | WorldGen | 生成荒兽领地 |

---

## 10. 实现优先级路线图

### Phase 1：核心串联（🔴最高，1~2周）

1. 扩展StoryPhase枚举（§1）
2. 扩展DownBossSystem（§5.2）
3. 创建ChoiceTrackerSystem（§6）
4. 注册主线任务链MQ-01~MQ-12（§5.3）
5. 修改StoryManager支持新阶段推进逻辑

### Phase 2：世界串联（🟡高，2~4周）

6. 实现阶段驱动的区域解锁（§5.5）
7. 为关键NPC创建阶段感知对话树（§5.4）
8. 实现天气与剧情联动（§5.6）
9. 注册主线任务链MQ-13~MQ-30

### Phase 3：深度串联（🟢中，4~8周）

10. 实现选择后果的跨阶段影响
11. 实现NPC态度的阶段感知变化
12. 实现世界事件的阶段触发
13. 实现Boss战的阶段门控

### Phase 4：打磨（🔵低，持续）

14. 对话内容填充（每个NPC至少10句阶段感知对话）
15. 商店内容填充（每个NPC至少5个阶段感知商品）
16. 平衡性调整
17. 多人模式测试

---

## 11. 现有代码改造检查清单

| 文件 | 改造内容 | 优先级 |
|------|---------|--------|
| `StoryPhase.cs` | 扩展枚举到67个阶段 | 🔴 |
| `StoryManager.cs` | 支持新阶段推进+分支 | 🔴 |
| `DownBossSystem.cs` | 新增12个Boss标记 | 🔴 |
| `QuestSystem.cs` | 注册30个主线任务 | 🔴 |
| `ChoiceTrackerSystem.cs` | 新建选择追踪系统 | 🔴 |
| `GuMasterBase.cs` | 对话树阶段感知 | 🟡 |
| `WorldStateMachine.cs` | 阶段驱动的世界事件 | 🟡 |
| `WeatherSystem.cs` | 剧情联动天气 | 🟡 |
| `QiRealmPlayer.cs` | 阶段门控突破 | 🟡 |
| `KongQiaoPlayer.cs` | 阶段门控空窍 | 🟡 |
| `ChunQiuChanPlayer.cs` | 阶段升级春秋蝉 | 🟡 |
| `GuWorldPlayer.cs` | 选择影响声望 | 🟡 |
| `KarmaSystem.cs` | 选择影响因果 | 🟡 |
| `HeavenTribulationSystem.cs` | 阶段门控天劫 | 🟢 |
| `FactionPowerSystem.cs` | 阶段影响权力 | 🟢 |
