# VerminLordMod 蛊师世界 — 系统全景架构（L1 终版 v1.0）

> 版本：v1.0 FINAL（全部 27 项决策已锁定）  
> 维护者：fsx、小d、小k  
> 更新日期：2026-04-27  
> 本文档只回答「系统有哪些」「属于哪个域」「当前状态」「优先级」，不回答「怎么做」。  
> 所有系统按「域」隔离，域之间通过「接口/事件」联系，禁止跨域直接调用。  
> **本文档锁定后，任何系统的新增、删除、优先级调整必须经过架构师评审，以补丁形式追加。**

---

## 1. 项目愿景

在 tModLoader 中构建一个基于《蛊真人》世界观的蛊师世界模拟系统。  
核心体验不是「完成任务刷声望」，而是**「你永远不知道眼前这个 NPC 下一秒是递来酒虫还是递来刀」**。

---

## 2. 核心设计哲学（不可违背）

| 原则 | 含义 | 反例 |
|------|------|------|
| **黑暗森林** | 任何角色都可能因利益计算而背叛或击杀玩家 | 固定声望 = 友好/敌对 |
| **信息不透明** | NPC 对玩家的评估是内部黑箱，玩家只能通过行为反推 | 直接显示 NPC 好感度数值 |
| **不可逆** | 击杀、背叛、家族覆灭产生永久性世界线变化 | NPC 死亡后秒复活 |
| **零和博弈** | 资源稀缺，玩家获得 = 他人失去 | 击杀怪物凭空掉落元石 |
| **维度隔离** | 已炼化蛊虫（空窍）与物理物品（背包）完全隔离 | 本命蛊可以被 NPC 从箱子里偷走 |

---

## 3. 现有代码基线状态

基于 `architecture-overview.md` 对现有代码的扫描，以下系统**已存在于代码库中**：

- **QiPlayer.cs**：真元/境界/资质/空窍大小（一体式，819行级别）
- **GuWorldPlayer.cs**：家族声望/通缉/背刺记录/恶名/声望（持久化）
- **ChunQiuChanPlayer.cs**：春秋蝉回溯/即死保护/时光能量
- **GuPerkSystem.cs**：永久增益（力量/寿命/速度/召唤/酒虫系列）
- **EffectsPlayer.cs / ProjectileTrailPlayer.cs**：特效与拖尾
- **GuWeaponItem.cs / GuAccessoryItem.cs / IGu.cs**：蛊虫物品基类与接口
- **GuLists.cs**：顽石开出蛊虫列表
- **大量具体蛊虫**：一转~六转武器蛊虫 + 饰品蛊虫 + 消耗品 + 道域武器（36种）
- **词缀系统**：武器词缀（外向/羞怯/温和/极端/活跃/内向/垂死/自闭）+ 饰品词缀（甲壳/鞘翅/蜷缩/伸展/利爪/尖牙）
- **NPC 层**：Boss（雷冠头狼）、Enemy（狂电狼/强电狼/军团蚁/刃血蝠蛊）、GuMasters（IGuMasterAI / GuMasterBase / GuYuePatrolGuMaster）、Town（学堂家老/药堂家老/御堂家老/白家长老/贾家商人）
- **Systems 层**：DownBossSystem / WolfSystem / PlayerEffectDrawSystem / QingMaoBiomeTileCount / RecipeGroupSystem / TravelMerchantSystem / GuWorldSystem / WorldEventSystem
- **UI 层**：QiBar / WolfWaveBar / DaosUI（框架） / RefineRecipeCallbacks / ReputationUI / DanmakuSelectionUI
- **Biomes**：青茅山地表/地下/古月驻地/水样式/雨/瀑布/背景
- **DamageClasses**：InsectDamageClass（蛊术伤害）
- **Buffs**：大量对敌/对己/连击 Buff（已完整实现）
- **Projectiles**：大量蛊虫对应弹幕（已完整实现）
- **Debuggers**：NpcDebugger / ReputationReset / Bugger / ChunQiuChanna / DanmakuTestWeapon / Enter / Gua / Info / PeppaPig / Posioner / QuickQi / SetMax / TYBL / UITester / ZZSetting
- **Currencies**：YuanSCurrency（元石货币）
- **SubWorlds**：Example.cs（小世界示例，SubworldLibrary）

**已知代码债务**：
- QiPlayer 过于庞大（真元+境界+Buff+战斗一体）
- GuMasterBase 过于庞大（819行，AI+信念+对话+战斗+弹幕保护）
- GuYuePatrolGuMaster 较大（649行，巡逻+战斗+搜尸耦合）
- GuWorldSystem 与 WorldEventSystem 职责边界模糊
- 炼化逻辑在 GuWeaponItem 与 GuAccessoryItem 中重复
- DaosUI 为框架，功能未实现
- 部分武器蛊虫目录缺少对应 `.cs` 文件（有贴图无代码）

---

## 4. 系统总览图

```
┌─────────────────────────────────────────────────────────────┐
│                      世界层 (World Layer)                     │
│  ├─ 世界状态机 (WorldStateMachine)          【已存在，需重构】 │
│  ├─ 家族关系网络 (FactionNetwork)             【已存在，待扩展】 │
│  ├─ 权力结构系统 (PowerStructureSystem)      【新增，P2】      │
│  ├─ 天道/天劫系统 (HeavenTribulationSystem)   【规划中】        │
│  └─ 小世界/福地系统 (SubworldLibrary)        【已存在示例】    │
├─────────────────────────────────────────────────────────────┤
│                      玩家层 (Player Layer)                  │
│  ├─ 真元资源系统 (QiResourcePlayer)          【已存在，需拆分】 │
│  ├─ 境界系统 (QiRealmPlayer)                  【已存在，需拆分】 │
│  ├─ 资质系统 (QiTalentPlayer)                【已存在，需拆分】 │
│  ├─ 空窍系统 (KongQiaoSystem)                 【开发中】        │
│  ├─ 道痕体质 (DaoHenProfile)                  【规划中】        │
│  ├─ 天道注视值 (HeavenGaze)                   【规划中】        │
│  ├─ 个体级信念存储 (PlayerBeliefStorage)       【规划中】        │
│  ├─ 春秋蝉回溯 (ChunQiuChanPlayer)            【已存在】        │
│  └─ 永久增益 (GuPerkSystem)                   【已存在】        │
├─────────────────────────────────────────────────────────────┤
│                      NPC层 (NPC Layer)                      │
│  ├─ 感知系统 (PerceptionSystem)              【已存在，需重构】 │
│  ├─ NPC 大脑 (NpcBrain)                       【开发中】        │
│  │   ├─ 信念子系统 (BeliefSubsystem)                          │
│  │   ├─ 原型子系统 (ArchetypeSubsystem)                       │
│  │   └─ 策略子系统 (StrategySubsystem)                        │
│  ├─ 对话系统 (DialogueSystem)                 【规划中】        │
│  ├─ NPC 死亡处理者 (NpcDeathHandler)            【规划中】        │
│  │   ├─ 背刺子系统 (AssassinationSubsystem)                   │
│  │   ├─ 调查链子系统 (InvestigationChainSubsystem)             │
│  │   └─ 痕迹子系统 (KillTraceSubsystem)                       │
├─────────────────────────────────────────────────────────────┤
│                      战斗层 (Combat Layer)                  │
│  ├─ 媒介武器系统 (MediumWeaponSystem)         【规划中】        │
│  ├─ 杀招配方系统 (ShaZhaoRecipeSystem)         【规划中】        │
│  ├─ 蛊虫弹幕系统 (GuProjectileSystem)          【已存在】        │
│  ├─ 道痕冲突系统 (DaoHenConflictSystem)        【规划中】        │
│  └─ NPC vs NPC 战斗 (NPCCombatSystem)         【规划中】        │
├─────────────────────────────────────────────────────────────┤
│                      经济层 (Economy Layer)                 │
│  ├─ 元石流通系统 (YuanStoneEconomy)            【已存在货币】   │
│  ├─ 搜尸/掠夺系统 (LootSystem)                 【开发中】        │
│  ├─ 交易/定价系统 (TradeSystem)               【已存在，需重构】 │
│  ├─ 资源点系统 (ResourceNodeSystem)            【规划中】        │
│  └─ 防御工事系统 (DefenseSystem)              【规划中】        │
├─────────────────────────────────────────────────────────────┤
│                      叙事层 (Narrative Layer)               │
│  ├─ 开局/开窍系统 (AwakeningSystem)            【规划中】        │
│  ├─ 悬赏系统 (BountySystem)                   【已存在基础】   │
│  ├─ 情报网络 (IntelligenceNetwork)             【规划中】        │
│  └─ 事件模板库 (EventTemplateLibrary)          【砍掉】         │
└─────────────────────────────────────────────────────────────┘
```

---

## 5. 系统清单（按域隔离）

### 5.1 世界层 (World Layer)

负责世界级的状态、规则与宏观事件。

| 系统 | 代码状态 | 优先级 | 实现方式 | 一句话描述 |
|------|---------|--------|----------|-----------|
| **WorldStateMachine** | 已存在（GuWorldSystem + WorldEventSystem） | P0 | 系统级 | D-01: GuWorldSystem 与 WorldEventSystem **合并**为统一世界状态机。对外暴露统一接口，内部按「状态存储 / 事件调度」分模块。 |
| **FactionNetwork** | 已存在（内嵌） | P1 | 子模块 | 降级为 WorldStateMachine 的子模块。8 大家族间好感度矩阵，影响 NPC 初始信念。家族关系是世界级状态，写入权归 WorldStateMachine，玩家侧只读引用。 |
| **PowerStructureSystem** | **新增** | P2 | 系统级 | D-23: 家族内职务分配（家老/族长/候选/无职务）、竞选、继承顺位、权力真空的连锁反应。嵌套于 WorldStateMachine 下，与 FactionNetwork 同级。 |
| **HeavenTribulationSystem** | 规划中 | P2 | 系统级 | 天劫调度、预警、规避机制。二转以上 NPC/玩家周期性面临灾劫。 |
| **SubworldLibrary** | 已存在（Example.cs） | **P1** | 系统级 | D-26: 从 P3 **提升为 P1**。MVA 只做 1 个示例（古月族地），P1 确立模板。家族领地小世界、福地、洞天。 |

**域接口**：
- 向世界层其他系统广播：家族关系变化、天劫预警、世界事件触发、职务真空事件
- 向 NPC 层提供：当前家族势力分布、区域归属（核心区/边缘/野外）、职务空缺信息
- 向玩家层提供：当前天道注视值、天劫倒计时、家族声望（只读）

**安全分层（D-27）**：
| 区域 | 安全程度 | 说明 |
|------|---------|------|
| **野外** | 无安全 | 掠夺型 NPC 随时可能攻击 |
| **家族驻地外围（大世界生物群系）** | 低安全 | 门卫观察但不保护玩家，只保护家族 NPC |
| **家族领地内部（小世界）** | **名义安全** | 公开战斗触发守卫围攻，但暗杀/下蛊/偷窃可行，被发现后全族通缉 |
| **玩家私人福地（P3）** | 最高安全 | 只有受邀者能进入，但可能被攻破 |

---

### 5.2 玩家层 (Player Layer)

负责玩家体内的蛊师能力体系。

| 系统 | 代码状态 | 优先级 | 实现方式 | 一句话描述 |
|------|---------|--------|----------|-----------|
| **QiResourcePlayer** | 已存在（QiPlayer.cs 部分） | P0 | 系统级 | D-02: 从 QiPlayer **拆分**出的 ModPlayer。真元上限、当前真元、恢复/消耗。死亡时真元清空（D-20）。 |
| **QiRealmPlayer** | 已存在（QiPlayer.cs 部分） | P0 | 系统级 | D-02: 从 QiPlayer **拆分**出的 ModPlayer。境界（1-10转）、阶段（初期/中期/后期/巅峰）、空窍大小。 |
| **QiTalentPlayer** | 已存在（QiPlayer.cs 部分） | P0 | 系统级 | D-02: 从 QiPlayer **拆分**出的 ModPlayer。资质（甲/乙/丙/丁）、修炼效率、Buff 触发。 |
| **KongQiaoSystem** | 开发中 | P0 | 系统级 | 空窍 UI、蛊虫炼化/取出/启用/休眠、容量管理。D-04: **软约束（占据额度）+ 硬上限（格子数）并存**。D-05: 死亡时本命蛊保留，其余按忠诚度处理。 |
| **DaoHenProfile** | 规划中 | P2 | 系统级 | 玩家道痕体质、蛊虫道痕冲突、炼化度。先实现 5-6 种核心道痕（炎/冰/力/风/血/智），后续扩展。 |
| **HeavenGaze** | 规划中 | P2 | 系统级 | 重生/回溯次数积累，影响 NPC 初始信念。死亡+10，回溯（春秋蝉）+5。与 ChunQiuChanPlayer 联动。 |
| **PlayerBeliefStorage** | 规划中 | P1 | 系统级 | 持久化每个 NPC 对当前玩家的信念快照。当前信念存于 NPC 实例，玩家退出后 NPC 释放导致失忆，需迁移到玩家侧持久化。 |
| **ChunQiuChanPlayer** | 已存在 | P1 | 系统级 | 春秋蝉回溯/即死保护/时光能量。与 ChunQiuChan 物品联动。 |
| **GuPerkSystem** | 已存在 | P1 | 系统级 | 永久增益（力量/寿命/速度/召唤/酒虫系列）。通过 TryAdd 方法安全添加。 |

**域接口**：
- 向战斗层提供：当前启用的攻击蛊列表、真元是否充足、道痕冲突掩码
- 向 NPC 层提供：玩家当前暴露的气息（催动的蛊虫）、修为波动
- 向世界层提供：玩家击杀记录、家族声望变化、死亡事件

**死亡处理（D-20 / D-05）**：
| 层级 | 内容 | 说明 |
|------|------|------|
| **第一层** | 真元完全清空 + 全部启用蛊虫强制休眠 | 创造脆弱窗口，复活后无法立即反击 |
| **第二层** | HeavenGaze +10（回溯+5）+ MVA 只更新击杀者信念 | 天道注视，P1 再扩展目击者 |
| **第三层** | 生成可交互尸体 + 背包物品随机掉落 20-40% + NPC 可搜尸 + 死亡日志 | 零和博弈核心：失去的物品被 NPC 获得，可复仇夺回 |

**空窍双轨制**：
| 存储位置 | 内容 | NPC 可见性 | 死亡处理 |
|---------|------|-----------|---------|
| **空窍** | 已炼化蛊虫 | 未催动时完全不可见 | 本命蛊保留，其余按忠诚度（D-06: <40% 可能损失，≥40% 保留） |
| **背包/箱子** | 未炼化蛊虫、元石、材料 | 可被感知（元石气息） | 尸体中可被 NPC 搜刮（D-09: 背包 + 附近 5 格箱子；D-15: 不翻箱子，只攻击在场玩家） |

---

### 5.3 NPC 层 (NPC Layer)

负责所有蛊师 NPC 的 AI、交互与生命周期。

| 系统 | 代码状态 | 优先级 | 实现方式 | 一句话描述 |
|------|---------|--------|----------|-----------|
| **PerceptionSystem** | 已存在（IGuMasterAI.cs 中） | P0 | 系统级 | NPC 感知玩家修为、蛊虫、位置、环境规则（核心区/边缘/野外）。需扩展「可见蛊虫」检测。 |
| **NpcBrain** | 开发中 | P0 | **系统簇** | **P0 合并实现**：BeliefSystem + ArchetypeSystem + StrategySystem。  
- **信念子系统**：NPC 对玩家的概率分布评估（修为/底牌），内部黑箱。  
- **原型子系统**：掠夺型/交易型/隐忍型 + 动态 RiskThreshold 漂移。  
- **策略子系统**：基于原型+信念的策略选择（观望/试探/交易/背刺/逃跑）。 |
| **DialogueSystem** | 规划中 | P1 | 系统级 | 多层对话树：公开交互/暗面操作/杀招准备。对话即情报战。劫持原版 SetChatButtons，替换为独立对话栈。 |
| **NpcDeathHandler** | 规划中 | P1 | **系统簇** | **P0 合并实现**：AssassinationSystem + InvestigationChain + KillTraceSystem。  
- **背刺子系统**：MVA 判定 = 非战斗状态 + 近距离（D-08）；P2 增加背后锥形 + 置信度>0.7。  
- **调查链子系统**：发现尸体→现场勘查→嫌疑人排查→悬赏/复仇→结案。MVA 简化为「直接悬赏」。  
- **痕迹子系统**：伤口类型/时间/目击/毁尸状态。MVA 只记录伤口类型。 |

**域接口**：
- 向玩家层请求：玩家的暴露信息（可见蛊虫、修为）
- 向战斗层广播：进入战斗状态、选择的弹幕类型、杀招触发
- 向经济层广播：死亡掉落、搜尸结果
- 向世界层广播：NPC 死亡事件、家族声望变化、职务真空事件

**城镇 NPC 死亡（D-07）**：
- 可击杀，永不复活
- 族长出手复活作为「昂贵后悔药」：消耗巨量元石 + 稀有材料 + 修为跌落一阶
- 触发 PowerStructureSystem 硬编码继承顺位（MVA）/ 完整竞选（P2）

---

### 5.4 战斗层 (Combat Layer)

负责战斗规则、弹幕与杀招。

| 系统 | 代码状态 | 优先级 | 实现方式 | 一句话描述 |
|------|---------|--------|----------|-----------|
| **MediumWeaponSystem** | 规划中 | P0 | 系统级 | D-11: **唯一一把蛊道媒介**。空壳武器，查询空窍中的蛊虫数据驱动弹幕。当前武器蛊虫直接使用 GuWeaponItem 的 Shoot，需重构为「媒介查询空窍」模式。 |
| **ShaZhaoRecipeSystem** | 规划中 | P1 | 系统级 | 杀招配方库，匹配启用的蛊虫组合，释放后参与者休眠。MVA 硬编码 1 条配方（月光蛊+酒虫=酒月斩），验证通过后扩展通用系统。D-10: **仅参与者休眠**。 |
| **GuProjectileSystem** | 已存在 | P0 | 系统级 | 蛊虫弹幕（大量已实现）。涵盖元素/弹幕/特效/召唤/连击类。 |
| **DaoHenConflictSystem** | 规划中 | P2 | 系统级 | D-13: **装备时缓存（O(1)）**。道痕冲突检测、效果衰减/自伤。先实现 5-6 种核心道痕，后续扩展。 |
| **NPCCombatSystem** | 规划中 | P3 | 系统级 | NPC vs NPC 战斗、家族战争、领地冲突。 |

**域接口**：
- 向玩家层请求：当前启用的攻击蛊、真元扣除
- 向 NPC 层广播：战斗结果、伤害来源
- 向世界层广播：大规模战斗事件

**平 A 与杀招**：
| 场景 | 行为 | 决策 |
|------|------|------|
| 无杀招匹配 | 所有启用的攻击蛊 **同时散射**（D-12） | 每只发射自己的弹幕，各自扣少量真元 |
| 杀招匹配 | 释放杀招弹幕（高伤）→ **仅参与者休眠**（D-10）→ 进入冷却 | 消耗大量真元 |

---

### 5.5 经济层 (Economy Layer)

负责资源的生产、流通与掠夺。

| 系统 | 代码状态 | 优先级 | 实现方式 | 一句话描述 |
|------|---------|--------|----------|-----------|
| **YuanStoneEconomy** | 已存在（YuanSCurrency） | P1 | 系统级 | D-14: **完全移除怪物掉落元石**。元石作为核心货币，仅通过搜尸/矿脉/交易获取。当前已实现自定义货币，但流通逻辑（NPC 之间交易/收税/抢劫）未实现。 |
| **LootSystem** | 开发中 | P1 | 系统级 | 搜尸（自动拾取基础物品 + 深度搜尸稀有物品）、基地搜刮。D-09: **背包 + 附近 5 格箱子**；D-15: **只攻击在场玩家，不翻箱子**。当前 GuYuePatrolGuMaster 有基础搜尸逻辑，需抽象为通用系统。 |
| **TradeSystem** | 已存在，需重构 | P0 | 系统级 | D-16: **动态定价完全自由（无上限）**。NPC 坐地起价，玩家可选择动手或忍受。当前价格固定，需改为效用计算驱动。 |
| **ResourceNodeSystem** | 规划中 | P1 | 系统级 | 元泉等有限资源点，NPC 与玩家竞争，储量耗尽后枯竭。当前无资源点概念。 |
| **DefenseSystem** | 规划中 | P2 | 系统级 | 迷踪阵（降低 NPC 感知精度）、守卫雇佣（提高 NPC 风险阈值）。当前无防御工事。 |

**域接口**：
- 向 NPC 层提供：当前区域资源分布、玩家财富评估
- 向玩家层广播：元石变动、物品被掠夺
- 向世界层广播：资源点枯竭、商队被劫

---

### 5.6 叙事层 (Narrative Layer)

负责玩家的成长路径与世界的叙事推进。

| 系统 | 代码状态 | 优先级 | 实现方式 | 一句话描述 |
|------|---------|--------|----------|-----------|
| **AwakeningSystem** | 规划中 | P0 | 系统级 | D-17: **击杀第一只野蛊**触发开窍；D-18: **凡人阶段 10 分钟短暂过渡**。当前玩家开局即拥有真元系统，需改为凡人起步。 |
| **BountySystem** | 已存在（基础） | P2 | 系统级 | 悬赏发布（家族对玩家/玩家对 NPC）、悬赏领取与结算。当前有 BountyStatus 数据结构，但完整流程未实现。 |
| **IntelligenceNetwork** | 规划中 | P3 | 硬编码级 | 情报收集、传递、污染（假情报）、可信度评估。P3 之前以硬编码行为嵌入具体 NPC，不独立为系统。 |
| **EventTemplateLibrary** | 规划中 | P3 | **砍掉** | ROI 过低，不实现。 |

**域接口**：
- 向 NPC 层提供：任务目标、悬赏目标
- 向玩家层提供：情报线索、悬赏告示
- 向世界层广播：重大叙事事件

**任务系统（D-19）**：
- **完全放弃任务面板**（MissionSystem 砍掉）
- 悬赏告示可以存在（NPC 发布悬赏告示，玩家自己发现、记住、决定是否行动）
- 这本身就是信息不透明的体现

---

## 6. 优先级矩阵（修订版）

| 优先级 | 代号 | 目标 | 包含系统 | 合并簇 | 当前代码债务 |
|--------|------|------|---------|--------|-------------|
| **P0** | MVA | 验证核心体验：玩家无法预测 NPC 行为 | QiResourcePlayer / QiRealmPlayer / QiTalentPlayer / KongQiaoSystem / PerceptionSystem / NpcBrain / MediumWeaponSystem / AwakeningSystem / WorldStateMachine / GuProjectileSystem | NpcBrain / NpcDeathHandler（简化） | QiPlayer 拆分；GuMasterBase 拆分；GuYuePatrolGuMaster 抽象 |
| **P1** | 沙盒 | 古月山寨完整运转：对话、背刺、经济闭环、驻地/领地 | DialogueSystem / NpcDeathHandler（完整） / LootSystem / TradeSystem（动态定价） / ResourceNodeSystem / PlayerBeliefStorage / FactionNetwork / SubworldLibrary / DefenseSystem | — | GuWorldPlayer 与 WorldStateMachine 职责厘清 |
| **P2** | 深度 | 世界开始呼吸：调查链、天劫、道痕、悬赏、权力结构 | InvestigationChain（完整） / KillTraceSystem（完整） / HeavenTribulationSystem / DaoHenProfile / DaoHenConflictSystem / BountySystem（完整） / PowerStructureSystem / HeavenGaze | — | 事件系统边界厘清 |
| **P3** | 扩展 | 多家族、情报战、大规模战争、私人福地 | SubworldLibrary（非家族小世界） / NPCCombatSystem / IntelligenceNetwork（系统级） / 福地/洞天 | — | DaosUI 实现 |

---

## 7. 版本路线图（修订版）

| 版本 | 代号 | 核心交付 | 验收标准 | 关键依赖 |
|------|------|---------|---------|---------|
| **v0.1** | 开窍 | 凡人开局 + 空窍 + 真元 + 1 媒介 + 1 野蛊 | 击杀野蛊→开窍→炼化→用媒介发射弹幕 | D-17, D-18 |
| **v0.2** | 巡逻 | 古月巡逻蛊师超级原型（NpcBrain + 无安全区） | 玩家无法预测其行为 | NpcBrain 簇验证通过 |
| **v0.3** | 山寨 | 古月山寨 5 NPC + 对话 + 背刺 + 搜尸 + NpcDeathHandler | 击杀引发连锁反应 | D-07, D-08, D-09 |
| **v0.4** | 经济 | 元石流通 + 资源点 + 防御工事 + 驻地/领地（1 个） | 争夺元泉、管理真元预算、进入古月族地小世界 | D-25, D-26, D-27 |
| **v0.5** | 天道 | 天劫 + 调查链完整版 + 悬赏完整版 + 权力结构（硬编码继承） | 击杀 NPC 后世界真的变了 | D-20~D-24 |
| **v1.0** | 蛊界 | 多家族 + 小世界模板 + 完整经济闭环 + 竞选系统 | 选择阵营、参与家族战争、拥有福地 | PowerStructureSystem 完整版 |

---

## 8. 跨域通信规范

所有系统通过**自建轻量级事件队列**通信（D-03），禁止直接引用。

```csharp
// 示例：NPC 死亡事件
public class NPCDeathEvent : IGuWorldEvent 
{
    public int NPCType;
    public int KillerPlayerID;
    public KillTrace Trace;
    public Vector2 Position;
    public FactionID Faction;
    public FactionRole VacatedRole;  // 若死亡者是职务 NPC，记录空缺职务
}

// 发布
EventBus.Publish(new NPCDeathEvent { ... });

// 订阅（PowerStructureSystem / NpcDeathHandler）
EventBus.Subscribe<NPCDeathEvent>(OnNPCDeath);
```

MVA 阶段用最简字典实现，P1 再扩展订阅优先级和错误隔离。

---

## 9. 术语表

| 术语 | 定义 |
|------|------|
| **空窍** | 玩家体内的蛊虫存储空间，与背包物理隔离。已炼化蛊虫存放于此，NPC 不可直接搜刮 |
| **真元** | 催动蛊虫的能量，上限随修为提升。启用蛊虫时一次性扣除「占据额度」，停用后归还 |
| **占据额度** | 启用蛊虫时占用的真元上限额度。软约束：玩家可以塞满空窍，但启用过多会导致可用真元枯竭 |
| **格子数** | 空窍的硬上限，随修为提升。与占据额度并存，服务不同目的 |
| **信念** | NPC 对玩家的概率分布评估（修为/底牌），内部黑箱，不暴露数值 |
| **原型** | NPC 的底层行为模板：掠夺型（RiskThreshold 动态漂移）/ 交易型 / 隐忍型 |
| **杀招** | 多只蛊虫组合释放的强力攻击，释放后参与者进入冷却休眠 |
| **媒介** | 空壳武器，查询空窍中的蛊虫数据驱动弹幕。玩家只持有一把 |
| **道痕** | 蛊虫的属性标签，相斥道痕同时催动会冲突。装备时生成 ConflictMask 缓存 |
| **痕迹** | 击杀现场遗留的线索（伤口类型/时间/目击/毁尸状态） |
| **尸体** | 玩家/NPC 死亡后生成的可交互实体，右键打开 UI 查看/搜刮剩余物品，腐烂后散落 |
| **开窍** | 凡人转变为蛊师的仪式，击杀第一只野蛊后触发，开启空窍 |
| **野蛊** | 替换原版特定怪物的蛊虫生物，击杀掉落未炼化蛊虫 |
| **名义安全** | 小世界/家族领地内的规则：公开战斗触发守卫围攻，但暗杀/下蛊/偷窃可行，被发现后全族通缉 |
| **职务真空** | 有职务的 NPC 死亡后产生的权力空缺，触发继承顺位或竞选 |

---

## 10. 已锁定决策汇总（27 项）

以下决策已由 fsx（架构师）与小 d（实现负责人）共同确认，**锁定后不可单方面变更**。变更需双方评审并以补丁形式追加。

### 10.1 架构级（3 项）

| 编号 | 决策 | 最终锁定 |
|------|------|---------|
| **D-01** | GuWorldSystem 与 WorldEventSystem 合并？ | **A. 合并为世界状态机（WorldStateMachine）** |
| **D-02** | QiPlayer 拆分方式？ | **A. 拆为三个 ModPlayer（QiResourcePlayer / QiRealmPlayer / QiTalentPlayer）** |
| **D-03** | 事件总线实现？ | **A. 自建轻量级事件队列（MVA 最简字典，P1 扩展优先级）** |

### 10.2 玩家层机制（5 项）

| 编号 | 决策 | 最终锁定 |
|------|------|---------|
| **D-04** | 空窍容量限制？ | **C. 两者并存（占据额度软约束 + 格子数硬上限）** |
| **D-05** | 已炼化蛊虫死亡处理？ | **A. 本命蛊保留，其余按忠诚度（<40% 可能损失，≥40% 保留）** |
| **D-06** | 忠诚度阈值？ | **MVA 两段式：<40% 可能损失，≥40% 保留；P2 再细化三段** |
| **D-20** | 玩家死亡惩罚层级？ | **C. 黑暗（真元清空 + 蛊虫休眠 + 可交互尸体 + NPC 可搜尸 + 死亡日志）** |
| **D-21** | 尸体存在时间？ | **A. 与 NPC 尸体一致（区域规则）+ 玩家有尸体位置标记** |

### 10.3 NPC 层机制（7 项）

| 编号 | 决策 | 最终锁定 |
|------|------|---------|
| **D-07** | 城镇 NPC 可击杀？ | **A. 可击杀，永不复活；族长复活为「昂贵后悔药」（巨量资源 + 修为跌落）** |
| **D-08** | 背刺判定条件？ | **MVA = B（非战斗状态 + 近距离）；P2 = A+B（增加背后锥形 + 置信度>0.7）** |
| **D-09** | 搜尸范围？ | **B. 背包 + 附近 5 格箱子** |
| **D-22** | 能否看到谁搜了尸体？ | **MVA = B（死亡日志）；P2 = C（需智道/侦查能力追踪）** |
| **D-23** | 新增 PowerStructureSystem？ | **同意，P2，世界层子模块，嵌套于 WorldStateMachine** |
| **D-24** | 参选资格？ | **B/C 组合：声望 + 完成特定委托 / 现任家老举荐，不能只刷声望** |
| **D-27** | 小世界内安全规则？ | **B. 名义安全：公开战斗触发守卫围攻，暗杀/下蛊/偷窃可行，被发现后全族通缉** |

### 10.4 战斗层机制（4 项）

| 编号 | 决策 | 最终锁定 |
|------|------|---------|
| **D-10** | 杀招休眠范围？ | **A. 仅参与者休眠** |
| **D-11** | 媒介武器数量？ | **A. 唯一一把蛊道媒介** |
| **D-12** | 平 A 齐射模式？ | **A. 同时散射** |
| **D-13** | 道痕冲突检测时机？ | **A. 装备时缓存（O(1)），使用时查缓存** |

### 10.5 经济层机制（3 项）

| 编号 | 决策 | 最终锁定 |
|------|------|---------|
| **D-14** | 元石掉落方式？ | **A. 完全移除怪物掉落，仅搜尸/矿脉/交易** |
| **D-15** | 基地搜刮逻辑？ | **A. 只攻击在场且携带高价值物品的玩家，不翻箱子** |
| **D-16** | 动态定价浮动？ | **C. 完全自由（无上限），NPC 坐地起价，玩家可选择动手或忍受** |

### 10.6 叙事层机制（3 项）

| 编号 | 决策 | 最终锁定 |
|------|------|---------|
| **D-17** | 开局触发条件？ | **A. 击杀第一只野蛊（主路径）；空窍石作为保底辅助** |
| **D-18** | 凡人阶段时长？ | **A. 短暂过渡（10 分钟内）** |
| **D-19** | 任务系统是否保留？ | **B. 完全放弃任务面板，纯自发博弈；悬赏告示存在但无面板** |

### 10.7 驻地/小世界机制（2 项）

| 编号 | 决策 | 最终锁定 |
|------|------|---------|
| **D-25** | 家族领地物理形式？ | **A. 驻地（大世界生物群系）+ 领地内部（小世界）** |
| **D-26** | SubworldLibrary 优先级？ | **B. 提升为 P1；MVA 只做 1 个示例（古月族地），P1 确立模板** |

---

## 11. MVA 阶段系统簇（P0 合并实现）

| 簇名 | 包含系统 | 合并后名称 | 说明 |
|------|---------|-----------|------|
| **簇 A** | BeliefSystem + ArchetypeSystem + StrategySystem | **NpcBrain** | MVA 硬编码在 GuYuePatrolGuMaster 中，验证通过后抽象为基类 |
| **簇 B** | InvestigationChain + KillTrace + AssassinationSystem | **NpcDeathHandler** | MVA 只发布 NPC 死亡事件 + 基础悬赏，P2 再拆分完整调查链 |

---

## 12. 修订历史

| 日期 | 版本 | 修订内容 |
|------|------|---------|
| 2026-04-27 | v1.0 FINAL | 锁定全部 27 项决策；新增 PowerStructureSystem；SubworldLibrary 提升为 P1；新增驻地/小世界安全分层；新增玩家死亡惩罚三层；新增 MVA 系统簇 |
| 2026-04-27 | v1.0.1 | **清理与基础设施构建**：① 移除 GuWorldSystem 中未使用的 `WorldEvent` 类及 `ActiveEvents` 字段，添加 D-01 TODO 注释；② 清理 GlobalLoots.cs 全部注释代码；③ 删除 33 个 `.png~` 备份文件及 1 个 `.bak` 文件；④ 确认 build.txt 无冲突标记；⑤ 实现 D-03 自建轻量级事件队列：`Common/Events/IGuWorldEvent.cs`（标记接口 + 4 个预定义事件类型）+ `Common/Events/EventBus.cs`（Publish/Subscribe/Unsubscribe/Clear） |

---

> 本文档由第一层架构师维护，经 fsx 与小 d 共同确认后锁定。  
> 任何变更必须以补丁形式追加到「修订历史」节，并重新编号决策项。  
> 本文档锁定后，立即启动 L2 接口设计。
