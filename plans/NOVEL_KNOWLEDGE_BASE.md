# 蛊真人 · 小说知识库框架设计

## 核心原则（绝对遵守）

### 第一阶段：有机镜像（YAML 知识库）
- **原文是绝对的** — 在做小说镜像时，必须 100% 遵照原文，不允许任何偏差
- 仅在与原文没有任何冲突时才允许适当补全（如：原文未明确描述但合理推断的细节）
- 所有数据来源为 `novel.db`（已提取的小说全文结构化数据）
- 镜像本身不做改编、不原创、不加入游戏设计

### 第二阶段：游戏实现（C# 代码）
- **从此镜像出发**，才允许进行改编、再创作
- 改编的目的是**制作游戏性优秀的玩法**，而非为了改编而改编
- 有疑问时返回查镜像中的原文依据

---

## 一、数据层：YAML 知识库

### 目录结构
```
helps/novel_db/
├── meta.yaml                       # 数据库元信息（版本、总章节数、最后更新）
│
├── arcs/                           # 篇章
│   ├── 00_青茅山篇.yaml            # ch001-117  一转
│   ├── 01_家族争锋.yaml            # ch118-221  二转
│   ├── 02_南疆流浪.yaml            # ch222-365  三转
│   ├── 03_义天山大战.yaml          # ch366-487  四转
│   ├── 04_北原争霸.yaml            # ch488-653  五转
│   ├── 05_宿命大战.yaml            # ch654-972  六转
│   ├── 06_蛊仙之路.yaml            # ch973-1380 七转（原创部分）
│   ├── 07_道主争夺.yaml            # ch1381-1700 八转
│   ├── 08_尊者之战.yaml            # ch1701-2100 九转
│   └── 09_终局.yaml                # ch2101-     十转
│
├── chapters/                       # 章节块（每10章一组）
│   ├── ch001-010.yaml
│   ├── ch011-020.yaml
│   └── ...
│
├── entities/                       # 实体索引
│   ├── gu_index.yaml               # 全蛊虫索引
│   ├── char_index.yaml             # 全角色索引  
│   └── location_index.yaml         # 全地点索引
│
└── maps/                           # 关联映射
    ├── rank_to_arcs.yaml           # 转数 ↔ 篇章
    ├── gu_to_chapters.yaml         # 蛊虫 ↔ 出现章节
    └── char_to_chapters.yaml       # 角色 ↔ 出现章节
```

### YAML 格式规范

#### 篇章（arcs/*.yaml）
```yaml
id: qingmao_shan
title: 青茅山篇
chapter_range: [1, 117]
rank: 一转
summary: "方源重生回到青茅山古月山寨，重走蛊师之路。"

major_events:
  - id: arrival
    chapter: 1
    title: 重生
    summary: "方源携春秋蝉重生回到五百年前"
    
  - id: awakening
    chapter: 8
    title: 开窍仪式
    summary: "方源参加古月山寨开窍仪式"
    related_gu: [春秋蝉, 月光蛊]
    related_chars: [方源, 古月方正, 古月族老]

  - id: moonlight_first
    chapter: 12
    title: 月光蛊初现
    summary: "方源首次展示月光蛊，击败方正挑战"
    related_gu: [月光蛊]
    related_chars: [方源, 古月方正]

  - id: inheritance
    chapter: 85
    title: 花酒行者传承
    summary: "方源获得花酒行者传承"
    related_gu: [花酒蛊]
    related_chars: [方源, 花酒行者]

new_gu: [月光蛊, 火鸦蛊, 酒虫蛊, 花酒蛊, ...]
new_chars: [方源, 古月方正, 古月族老, 花酒行者, 贾金生, ...]
key_locations: [古月山寨, 青茅山, 三岔路口]
```

#### 章节块（chapters/ch*.yaml）
```yaml
chapter_range: [11, 20]
arc_id: qingmao_shan

entries:
  - chapter: 11
    title: 学堂授课
    content: "古月学堂传授蛊师基础知识"
    gu_mentioned: []
    char_appear: [方源, 古月导师]
    events: []

  - chapter: 12
    title: 月光蛊
    content: "方源催动月光蛊，一道银白色月刃划破夜空。这是他重生后第一次在众人面前展示实力。"
    gu_mentioned: [月光蛊]
    char_appear: [方源, 古月方正]
    events: [moonlight_first]
    first_appear_gu: [月光蛊]     # 全书中首次出现的蛊虫

  - chapter: 13
    title: 学堂交锋
    content: "方正不服挑战方源，被月光蛊击败。"
    gu_mentioned: [月光蛊, 火鸦蛊]
    char_appear: [方源, 古月方正]
    events: []
```

#### 实体索引（entities/gu_index.yaml）
```yaml
gu:
  - name: 月光蛊
    aliases: [Moonlight Gu]
    rank: 一转
    dao: 月道
    first_appear: 12
    summary: "方源第一只攻击蛊虫，月华凝刃"
    appearances: [12, 13, 15, 18, 25, 34, 42]
    game_item: Content/Items/Weapons/One/Moonlight.cs

  - name: 春秋蝉
    aliases: [Spring Autumn Cicada]
    rank: 六转
    dao: 宙道
    first_appear: 1
    summary: "逆转时间，重生回五百年前的关键蛊虫"
    appearances: [1, 2, 654, 972, ...]
    game_item: Content/Items/Weapons/Six/ChunQiuChan.cs
```

#### 实体索引（entities/char_index.yaml）
```yaml
characters:
  - name: 方源
    aliases: [Gu Yue Fang Yuan]
    role: 主角
    rank_progression: [一转, 二转, 三转, 四转, 五转, 六转, 七转, 八转, 九转]
    first_appear: 1
    summary: "重生归来的五转蛊师，携带春秋蝉回到五百年前"
    key_events: [重生, 开窍, 花酒传承, 血祭之夜, 离开青茅山, ...]
    signature_gu: [春秋蝉, 月光蛊, 酒虫蛊, ...]
```

#### 映射（maps/rank_to_arcs.yaml）
```yaml
ranks:
  - rank: 一转
    arc: qingmao_shan
    chapter_range: [1, 117]
    in_game_phase: [Arrival, AwakeningCeremony, ..., FamilyRecognition]
    novel_gu_count: 23
    game_gu_count: 12

  - rank: 二转
    arc: family_conflict
    chapter_range: [118, 221]
    in_game_phase: [PreTournament, ..., LeftQingMao]
```

---

## 二、展示层：游戏内 UI

### 整体架构
```
Common/UI/NovelUI/
├── NovelUISystem.cs         # 注册键位、管理打开/关闭
├── NovelMainPanel.cs        # 主面板（标签页容器）
├── NovelSearchResultUI.cs   # 搜索结果项
├── tabs/
│   ├── BrowseTab.cs         # 篇章浏览标签
│   ├── SearchTab.cs         # 搜索标签
│   ├── TimelineTab.cs       # 时间线标签
│   └── IndexTab.cs          # 全索引标签（蛊虫/角色/地点）
├── widgets/
│   ├── ChapterEntry.cs      # 章节目录项
│   ├── EntityCard.cs        # 实体卡片（蛊虫/角色）
│   ├── TimelineNode.cs      # 时间线节点
│   └── SearchBar.cs         # 搜索框
└── data/
    └── NovelDataLoader.cs   # YAML 加载器（读取 → C# 对象）
```

### 界面设计

#### 主窗口结构
```
┌──────────────────────────────────────────────────────┐
│  📖 蛊世界全书          [X]                         │
├──────────────────────────────────────────────────────┤
│  [📂 篇章浏览] [🔍 搜索] [🗺️ 时间线] [📇 全索引]  │
├──────────────────────────────────────────────────────┤
│                                                       │
│              (标签页内容区域)                         │
│                                                       │
├──────────────────────────────────────────────────────┤
│  📍 当前阅读进度：青茅山篇 第12章                     │
│  ⚡ 当前游戏阶段：一转·蛊师入门 → 建议阅读 ch001-117 │
└──────────────────────────────────────────────────────┘
```

#### 标签页 A：篇章浏览
```
┌──────────────────────────────────────────────────────┐
│  篇章浏览                                           │
│  ┌──────────────┐ ┌──────────────────────────────┐  │
│  │ 📂 篇章列表   │ │ 青茅山篇 (ch001-117)       │  │
│  │──────────────│ │                              │  │
│  │ ▸ 青茅山篇 ◀ │ │ 方源重生回到五百年前…        │  │
│  │ ▸ 家族争锋   │ │                              │  │
│  │ ▸ 南疆流浪   │ │ ┌──────────────────────────┐│  │
│  │ ▸ 义天山大战 │ │ │ 📖 章节列表              ││  │
│  │ ▸ 北原争霸   │ │ │ ch001 重生               ││  │
│  │ ▸ 宿命大战   │ │ │ ch002 初醒               ││  │
│  │ ▸ 蛊仙之路   │ │ │ ch003 古月山寨           ││  │
│  │ ▸ 道主争夺   │ │ │ …                        ││  │
│  │ ▸ 尊者之战   │ │ │ ▸ch012 月光蛊 ◀         ││  │
│  │ ▸ 终局       │ │ │ …                        ││  │
│  │              │ │ └──────────────────────────┘│  │
│  │              │ │                              │  │
│  │              │ │ 📜 月光蛊                    │  │
│  │              │ │ 方源催动月光蛊，一道银白色    │  │
│  │              │ │ 月刃划破夜空…                │  │
│  │              │ │                              │  │
│  │              │ │ 🐛 关联：月光蛊  |  👤 方源  │  │
│  └──────────────┘ └──────────────────────────────┘  │
└──────────────────────────────────────────────────────┘
```

#### 标签页 B：搜索
```
┌──────────────────────────────────────────────────────┐
│  [🔍 输入关键词...]      [搜蛊虫] [搜角色] [全文] │
├──────────────────────────────────────────────────────┤
│  搜索结果：12条 (0.02秒)                            │
│                                                       │
│  🐛 月光蛊                                           │
│     一转·月道 · 出场12次 · 首现ch012                │
│     "月光如水，银白光泽…"                            │
│                                                       │
│  👤 方源                                              │
│     主角 · 全篇出场                                  │
│     "重生归来的五转蛊师…"                            │
│                                                       │
│  📖 ch012 月光蛊                                      │
│     "方源催动月光蛊，一道银白色月刃…"                │
│                                                       │
│  📖 ch013 学堂交锋                                    │
│     "方正不服挑战方源…"                              │
│  ...                                                  │
└──────────────────────────────────────────────────────┘
```

#### 标签页 C：时间线
```
┌──────────────────────────────────────────────────────┐
│  时间线 · 青茅山篇 (ch001-117)                      │
├──────────────────────────────────────────────────────┤
│                                                       │
│  ch001 ── 重生 ─────────── 🐛 春秋蝉                 │
│    │                                                  │
│  ch008 ── 开窍仪式 ──────── 👤 族老                   │
│    │                                                  │
│  ch012 ── 月光蛊初现 ────── 🐛 月光蛊 ★首次         │
│    │                      👤 方正                     │
│  ch013 ── 学堂交锋 ────────                           │
│    │                                                  │
│  ...                                                  │
│    │                                                  │
│  ch085 ── 花酒传承 ──────── 🐛 花酒蛊                 │
│                                                        │
│  🟢 已实现游戏内容    🟡 部分实现    🔴 未实现       │
└──────────────────────────────────────────────────────┘
```

#### 标签页 D：全索引
```
┌──────────────────────────────────────────────────────┐
│  [🐛 蛊虫] [👤 角色] [📍 地点]  ↕ 按转数筛选     │
├──────────────────────────────────────────────────────┤
│                                                       │
│  一转蛊虫 (12/23 已实装)                             │
│  ┌────────────────────────────────────────────────┐  │
│  │ 🐛 月光蛊 · 月道  ✅ 已实装                    │  │
│  │ 🐛 火鸦蛊 · 炎道  ❌ 未实装                    │  │
│  │ 🐛 酒虫蛊 · 酒道  ❌ 未实装                    │  │
│  │ ...                                            │  │
│  └────────────────────────────────────────────────┘  │
│  二转蛊虫 (8/35 已实装)                             │
│  ┌────────────────────────────────────────────────┐  │
│  │ ...                                            │  │
│  └────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────┘
```

---

## 三、数据加载层

### NovelDataLoader.cs
```csharp
// YAML → C# 对象，运行时一次性加载到内存
// 使用 YamlDotNet 解析（tModLoader 自带）
// 数据缓存，不频繁读盘

NovelDataLoader.Load("arcs/00_青茅山篇.yaml") → ArcData
NovelDataLoader.Search("春秋蝉") → List<SearchResult>
NovelDataLoader.GetChapterRange(12, 15) → List<ChapterEntry>
NovelDataLoader.GetGUIndex() → List<GUEntry>
```

### 搜索策略
```
关键词 → 同时搜索 entities/ 索引 + arcs 事件标题 + chapters 内容摘要
→ 按相关性排序（蛊虫/角色名优先 > 事件标题 > 内容摘要）
→ 限制最多 50 条结果
→ 高亮匹配关键词
```

---

## 四、与传统系统的关系

| 现有系统 | 与本系统的关系 |
|---|---|
| `StoryManager`（游戏剧情推进） | 独立运行，不依赖本系统。本系统可读取其阶段数据显示"建议阅读" |
| `GuDropRegistry`（蛊虫掉落） | 本系统可引用其数据统计"已实装/未实装" |
| `finish.db`（物品追踪） | 未来可替代为本系统的 YAML 实体索引 |

---

## 五、实现优先级

### P0 — 基础骨架
- [ ] `helps/novel_db/` 目录结构 + 示例 YAML（青茅山篇前20章）
- [ ] `NovelDataLoader.cs` YAML 加载
- [ ] `NovelUISystem.cs` 主面板框架 + 快捷键
- [ ] 搜索标签页（基本功能）

### P1 — 内容建设
- [ ] 青茅山篇完整 YAML（ch001-117）
- [ ] 蛊虫索引（一转）
- [ ] 篇章浏览标签页

### P2 — 完整功能
- [ ] 全篇章 YAML
- [ ] 时间线标签页
- [ ] 全索引标签页
- [ ] 游戏进度联动（高亮推荐章节）

---

## 六、开发指南

### 添加新章节数据
```yaml
# 在对应的 chapters/chXXX-YYY.yaml 中添加条目
- chapter: 85
  title: 花酒行者传承
  content: "简短摘要（1-2句话概括关键事件）"
  gu_mentioned: [花酒蛊]
  char_appear: [方源, 花酒行者]
  events: [inheritance]
```

### 添加新蛊虫索引
```yaml
# 在 entities/gu_index.yaml 中添加
- name: 新蛊虫
  rank: N转
  dao: X道
  first_appear: chapter_number
  summary: "一句话描述"
```

### 规则
- 每章 content 不超过 100 字摘要——不复制原文
- events 引用 arcs 中定义的 major_events id
- first_appear_gu 只在全书首次出现时标记
- 关联映射（maps/）数据从 entities + arcs 自动生成，可手动补充
