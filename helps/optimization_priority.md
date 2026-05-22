# 占位实现优化优先级顺序

> 基于 313 个占位实现的综合分析，按优先级排序。
> 排序依据：基础设施依赖 → 小说核心度 → 复杂度递进 → 批量效应 → 游戏体验影响

---

## 第一阶段：基础设施与核心系统（6 项）

> **目标**：打好基础，让后续内容实现有系统支撑

| 优先级 | 名称 | 分类 | 理由 |
|--------|------|------|------|
| **#1** | [`Randommer`](Content/Randommer.cs) | 核心工具 | 随机数工具，被大量其他类依赖，必须先完善 |
| **#2** | [`VerminLordModSystem`](Content/VerminLordModSystem.cs) | ModSystem | Mod 主系统入口，全局生命周期管理 |
| **#3** | [`YuanSCurrency`](Content/Currencies/YuanSCurrency.cs) | 货币 | 经济系统基础，元石是小说核心货币 |
| **#4** | [`OneStarbuff` ~ `FiveStarbuff`](Content/Buffs/AddToSelf/Pobuff/) | Buffs | 6 个 StarBuff 可批量处理，是战斗系统基础 |
| **#5** | [`YanTongbuff`](Content/Buffs/AddToSelf/Pobuff/YanTongbuff.cs) | Buffs | 眼瞳类 Buff，与侦查/感知系统相关 |
| **#6** | [`QingMaoDroplet`](Content/Biomes/QingMaoDroplet.cs) + 背景/瀑布样式 | Biomes | 青茅山环境系统，新手村体验 |

---

## 第二阶段：简单物品批量（25 项）

> **目标**：快速出成果，积累实现模式

### 2a. Seeds（10 个）— 模式高度一致，可批量

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#7** | [`SpiritGrassSeed`](Content/Items/Seeds/SpiritGrassSeed.cs) | 灵草种子 |
| **#8** | [`HealingHerbSeed`](Content/Items/Seeds/HealingHerbSeed.cs) | 疗伤草种子 |
| **#9** | [`QiHerbSeed`](Content/Items/Seeds/QiHerbSeed.cs) | 气草种子 |
| **#10** | [`PoisonWeedSeed`](Content/Items/Seeds/PoisonWeedSeed.cs) | 毒草种子 |
| **#11** | [`MoonOrchidSeed`](Content/Items/Seeds/MoonOrchidSeed.cs) | 月兰花种子 |
| **#12** | [`RiceBagGrassSeed`](Content/Items/Seeds/RiceBagGrassSeed.cs) | 饭袋草种子 |
| **#13** | [`WineGourdFlowerSeed`](Content/Items/Seeds/WineGourdFlowerSeed.cs) | 酒囊花种子 |
| **#14** | [`SpearBambooSeed`](Content/Items/Seeds/SpearBambooSeed.cs) | 矛竹种子 |
| **#15** | [`BoneBanbooSeed`](Content/Items/Seeds/BoneBanbooSeed.cs) | 骨竹种子 |
| **#16** | [`SpiritSpringPlantSeed`](Content/Items/Seeds/SpiritSpringPlantSeed.cs) | 灵泉植物种子 |

### 2b. Banners（3 个）— 简单旗帜

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#17** | [`BladeBloodBatGuBanner`](Content/Items/Placeable/Banners/BladeBloodBatGuBanner.cs) | 刀血蝠蛊旗帜 |
| **#18** | [`LegionAntBanner`](Content/Items/Placeable/Banners/LegionAntBanner.cs) | 军团蚁旗帜 |
| **#19** | [`StrongElectricWolfBanner`](Content/Items/Placeable/Banners/StrongElectricWolfBanner.cs) | 强电狼旗帜 |

### 2c. Tokens（8 个）— 令牌类，模式一致

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#20** | [`交易令`](Content/Items/Tokens/交易令.cs) | 交易令牌 |
| **#21** | [`令牌信物`](Content/Items/Tokens/令牌信物.cs) | 信物令牌 |
| **#22** | [`来客令`](Content/Items/Tokens/来客令.cs) | 来客令牌 |
| **#23** | [`琉璃楼主令`](Content/Items/Tokens/琉璃楼主令.cs) | 琉璃楼主令 |
| **#24** | [`紫荆令牌`](Content/Items/Tokens/紫荆令牌.cs) | 紫荆令牌 |
| **#25** | [`通缉令`](Content/Items/Tokens/通缉令.cs) | 通缉令牌 |
| **#26** | [`铁家令牌`](Content/Items/Tokens/铁家令牌.cs) | 铁家令牌 |
| **#27** | [`铭牌`](Content/Items/Tokens/铭牌.cs) | 铭牌 |

### 2d. Books（3 个）

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#28** | [`乾坤晶壁雏形`](Content/Items/Books/乾坤晶壁雏形.cs) | 书籍 |
| **#29** | [`兽皮地图`](Content/Items/Books/兽皮地图.cs) | 书籍 |
| **#30** | [`黑白颠倒云`](Content/Items/Books/黑白颠倒云.cs) | 书籍 |

### 2e. Equipment（1 个）

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#31** | [`真绸`](Content/Items/Equipment/真绸.cs) | 装备材料 |

---

## 第三阶段：Breeding 与 Consumables（16 项）

> **目标**：完善游戏核心交互系统

### 3a. Breeding（4 个）

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#32** | [`GuBox`](Content/Items/Breeding/GuBox.cs) | 蛊盒 — 蛊虫培育核心 |
| **#33** | [`SpiritPoolBox`](Content/Items/Breeding/SpiritPoolBox.cs) | 灵池盒 |
| **#34** | [`AncestralAltar`](Content/Items/Breeding/AncestralAltar.cs) | 先祖祭坛 |
| **#35** | [`BattleArena`](Content/Items/Breeding/BattleArena.cs) | 战斗竞技场 |

### 3b. Consumables（12 个）

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#36** | [`GuFoodPellet`](Content/Items/Consumables/GuFoodPellet.cs) | 蛊食丸 — 基础消耗品 |
| **#37** | [`SpiritMeat`](Content/Items/Consumables/SpiritMeat.cs) | 灵肉 |
| **#38** | [`BloodEssence`](Content/Items/Consumables/BloodEssence.cs) | 血精 |
| **#39** | [`TenLifeGu`](Content/Items/Consumables/TenLifeGu.cs) | 十年蛊 |
| **#40** | [`HundredLifeGu`](Content/Items/Consumables/HundredLifeGu.cs) | 百年蛊 |
| **#41** | [`ThousandLifeGu`](Content/Items/Consumables/ThousandLifeGu.cs) | 千年蛊 |
| **#42** | [`DragonBallCricket`](Content/Items/Consumables/DragonBallCricket.cs) | 龙丸蛐蛐 |
| **#43** | [`FlowerPig`](Content/Items/Consumables/FlowerPig.cs) | 花豕 |
| **#44** | [`StrengthLongicorn`](Content/Items/Consumables/StrengthLongicorn.cs) | 蛮力天牛 |
| **#45** | [`JunLiGu`](Content/Items/Consumables/JunLiGu.cs) | 均力蛊 |
| **#46** | [`ShiJunLiGu`](Content/Items/Consumables/ShiJunLiGu.cs) | 十均力蛊 |
| **#47** | [`ShiJinLiGu`](Content/Items/Consumables/ShiJinLiGu.cs) | 十斤力蛊 |

---

## 第四阶段：GuYueArchitecture（17 项）

> **目标**：完善古月村场景，提升新手村沉浸感

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#48** | [`SpiritSpring`](Content/Items/Placeable/GuYueArchitecture/SpiritSpring.cs) | 灵泉 |
| **#49** | [`HealingHerb`](Content/Items/Placeable/GuYueArchitecture/HealingHerb.cs) | 疗伤草 |
| **#50** | [`QiHerb`](Content/Items/Placeable/GuYueArchitecture/QiHerb.cs) | 气草 |
| **#51** | [`PoisonWeed`](Content/Items/Placeable/GuYueArchitecture/PoisonWeed.cs) | 毒草 |
| **#52** | [`MoonOrchid`](Content/Items/Placeable/GuYueArchitecture/MoonOrchid.cs) | 月兰花 |
| **#53** | [`RiceBagGrass`](Content/Items/Placeable/GuYueArchitecture/RiceBagGrass.cs) | 饭袋草 |
| **#54** | [`WineGourdFlower`](Content/Items/Placeable/GuYueArchitecture/WineGourdFlower.cs) | 酒囊花 |
| **#55** | [`SpearBamboo`](Content/Items/Placeable/GuYueArchitecture/SpearBamboo.cs) | 矛竹 |
| **#56** | [`GreenBambooWineJar`](Content/Items/Placeable/GuYueArchitecture/GreenBambooWineJar.cs) | 青竹酒坛 |
| **#57** | [`DragonCricketJar`](Content/Items/Placeable/GuYueArchitecture/DragonCricketJar.cs) | 龙蛐蛐罐 |
| **#58** | [`GlowingStalactite`](Content/Items/Placeable/GuYueArchitecture/GlowingStalactite.cs) | 发光钟乳石 |
| **#59** | [`RedCopperIncenseBurner`](Content/Items/Placeable/GuYueArchitecture/RedCopperIncenseBurner.cs) | 红铜香炉 |
| **#60** | [`AncestorTablet`](Content/Items/Placeable/GuYueArchitecture/AncestorTablet.cs) | 先祖牌位 |
| **#61** | [`BlackLacquerAltar`](Content/Items/Placeable/GuYueArchitecture/BlackLacquerAltar.cs) | 黑漆祭坛 |
| **#62** | [`BrownYellowFloor`](Content/Items/Placeable/GuYueArchitecture/BrownYellowFloor.cs) | 棕黄地板 |
| **#63** | [`SchoolDesk`](Content/Items/Placeable/GuYueArchitecture/SchoolDesk.cs) | 学堂书桌 |
| **#64** | [`StiltedHouse`](Content/Items/Placeable/GuYueArchitecture/StiltedHouse.cs) | 吊脚楼 |

---

## 第五阶段：Recipes（7 项）

> **目标**：完善配方/合成系统

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#65** | [`人祖传`](Content/Items/Recipes/人祖传.cs) | 小说核心典籍 |
| **#66** | [`传承印记`](Content/Items/Recipes/传承印记.cs) | 传承系统 |
| **#67** | [`天难真传`](Content/Items/Recipes/天难真传.cs) | 天难真传 |
| **#68** | [`白骨秘方`](Content/Items/Recipes/白骨秘方.cs) | 白骨秘方 |
| **#69** | [`灰骨巨书`](Content/Items/Recipes/灰骨巨书.cs) | 灰骨巨书 |
| **#70** | [`灰白石板`](Content/Items/Recipes/灰白石板.cs) | 灰白石板 |
| **#71** | [`第十三座鹰巢`](Content/Items/Recipes/第十三座鹰巢.cs) | 第十三座鹰巢 |

---

## 第六阶段：Prefixes（12 项）

> **目标**：完善蛊虫词缀系统

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#72** | [`ActivePrefix`](Content/Prefixes/ActivePrefix.cs) | 活跃词缀 |
| **#73** | [`AutismPrefix`](Content/Prefixes/AutismPrefix.cs) | 自闭词缀 |
| **#74** | [`ColeopteraPrefix`](Content/Prefixes/ColeopteraPrefix.cs) | 鞘翅词缀 |
| **#75** | [`CurlingUpPrefix`](Content/Prefixes/CurlingUpPrefix.cs) | 蜷缩词缀 |
| **#76** | [`DyingPrefix`](Content/Prefixes/DyingPrefix.cs) | 垂死词缀 |
| **#77** | [`ExtroversionPrefix`](Content/Prefixes/ExtroversionPrefix.cs) | 外向词缀 |
| **#78** | [`IntrovertPrefix`](Content/Prefixes/IntrovertPrefix.cs) | 内向词缀 |
| **#79** | [`MildPrefix`](Content/Prefixes/MildPrefix.cs) | 温和词缀 |
| **#80** | [`SharpClawPrefix`](Content/Prefixes/SharpClawPrefix.cs) | 利爪词缀 |
| **#81** | [`SharpTeethPrefix`](Content/Prefixes/SharpTeethPrefix.cs) | 利齿词缀 |
| **#82** | [`ShyPrefix`](Content/Prefixes/ShyPrefix.cs) | 害羞词缀 |
| **#83** | [`StretchPrefix`](Content/Prefixes/StretchPrefix.cs) | 伸展词缀 |

---

## 第七阶段：Dao 基类（51 项）

> **目标**：完善 51 个 Dao 流派基类，为后续蛊虫实现提供完整基类体系
> **注意**：这些是基类（如 `PowerWeapon`、`WaterWeapon` 等），完善它们会影响所有继承的子类蛊虫

| 优先级 | 名称 | Dao | 文件 |
|--------|------|-----|------|
| **#84** | [`PowerWeapon`](Content/Items/Weapons/Daos/PowerWeapon.cs) | 力 | 核心 Dao，已有部分实现 |
| **#85** | [`WaterWeapon`](Content/Items/Weapons/Daos/WaterWeapon.cs) | 水 | 基础 Dao |
| **#86** | [`FireWeapon`](Content/Items/Weapons/Daos/FireWeapon.cs) | 火 | 基础 Dao |
| **#87** | [`WoodWeapon`](Content/Items/Weapons/Daos/WoodWeapon.cs) | 木 | 基础 Dao |
| **#88** | [`StarWeapon`](Content/Items/Weapons/Daos/StarWeapon.cs) | 星 | 基础 Dao |
| **#89** | [`MudWeapon`](Content/Items/Weapons/Daos/MudWeapon.cs) | 土 | 基础 Dao |
| **#90** | [`EatingWeapon`](Content/Items/Weapons/Daos/EatingWeapon.cs) | 食 | 基础 Dao |
| **#91** | [`WindWeapon`](Content/Items/Weapons/Daos/WindWeapon.cs) | 风 | 基础 Dao |
| **#92** | [`LightningWeapon`](Content/Items/Weapons/Daos/LightningWeapon.cs) | 雷 | 基础 Dao |
| **#93** | [`IceSnowWeapon`](Content/Items/Weapons/Daos/IceSnowWeapon.cs) | 冰雪 | 基础 Dao |
| **#94** | [`SwordWeapon`](Content/Items/Weapons/Daos/SwordWeapon.cs) | 剑 | 基础 Dao |
| **#95** | [`SoulWeapon`](Content/Items/Weapons/Daos/SoulWeapon.cs) | 魂 | 基础 Dao |
| **#96** | [`GoldWeapon`](Content/Items/Weapons/Daos/GoldWeapon.cs) | 金 | 基础 Dao |
| **#97** | [`DarkWeapon`](Content/Items/Weapons/Daos/DarkWeapon.cs) | 暗 | 基础 Dao |
| **#98** | [`LightWeapon`](Content/Items/Weapons/Daos/LightWeapon.cs) | 光 | 基础 Dao |
| **#99** | [`PoisonWeapon`](Content/Items/Weapons/Daos/PoisonWeapon.cs) | 毒 | 基础 Dao |
| **#100** | [`BloodWeapon`](Content/Items/Weapons/Daos/BloodWeapon.cs) | 血 | 基础 Dao |
| **#101** | [`BoneWeapon`](Content/Items/Weapons/Daos/BoneWeapon.cs) | 骨 | 基础 Dao |
| **#102** | [`MoonWeapon`](Content/Items/Weapons/Daos/MoonWeapon.cs) | 月 | 基础 Dao |
| **#103** | [`CloudWeapon`](Content/Items/Weapons/Daos/CloudWeapon.cs) | 云 | 基础 Dao |
| **#104** | [`ShadowWeapon`](Content/Items/Weapons/Daos/ShadowWeapon.cs) | 影 | 基础 Dao |
| **#105** | [`QiWeapon`](Content/Items/Weapons/Daos/QiWeapon.cs) | 气 | 基础 Dao |
| **#106** | [`SkyWeapon`](Content/Items/Weapons/Daos/SkyWeapon.cs) | 天 | 基础 Dao |
| **#107** | [`TimeWeapon`](Content/Items/Weapons/Daos/TimeWeapon.cs) | 时间 | 高级 Dao |
| **#108** | [`SpaceWeapon`](Content/Items/Weapons/Daos/SpaceWeapon.cs) | 空间 | 高级 Dao |
| **#109** | [`VoidWeapon`](Content/Items/Weapons/Daos/VoidWeapon.cs) | 虚空 | 高级 Dao |
| **#110** | [`LifeDeathWeapon`](Content/Items/Weapons/Daos/LifeDeathWeapon.cs) | 生死 | 高级 Dao |
| **#111** | [`YinYangWeapon`](Content/Items/Weapons/Daos/YinYangWeapon.cs) | 阴阳 | 高级 Dao |
| **#112** | [`DreamWeapon`](Content/Items/Weapons/Daos/DreamWeapon.cs) | 梦 | 高级 Dao |
| **#113** | [`WisdomWeapon`](Content/Items/Weapons/Daos/WisdomWeapon.cs) | 智 | 高级 Dao |
| **#114** | [`LuckWeapon`](Content/Items/Weapons/Daos/LuckWeapon.cs) | 运 | 高级 Dao |
| **#115** | [`RuleWeapon`](Content/Items/Weapons/Daos/RuleWeapon.cs) | 律 | 高级 Dao |
| **#116** | [`WarWeapon`](Content/Items/Weapons/Daos/WarWeapon.cs) | 战 | 高级 Dao |
| **#117** | [`KillingWeapon`](Content/Items/Weapons/Daos/KillingWeapon.cs) | 杀 | 高级 Dao |
| **#118** | [`SlaveWeapon`](Content/Items/Weapons/Daos/SlaveWeapon.cs) | 奴 | 高级 Dao |
| **#119** | [`LoveWeapon`](Content/Items/Weapons/Daos/LoveWeapon.cs) | 爱 | 高级 Dao |
| **#120** | [`VoiceWeapon`](Content/Items/Weapons/Daos/VoiceWeapon.cs) | 音 | 高级 Dao |
| **#121** | [`PersonWeapon`](Content/Items/Weapons/Daos/PersonWeapon.cs) | 人 | 高级 Dao |
| **#122** | [`FlyingWeapon`](Content/Items/Weapons/Daos/FlyingWeapon.cs) | 飞 | 高级 Dao |
| **#123** | [`StealingWeapon`](Content/Items/Weapons/Daos/StealingWeapon.cs) | 偷 | 高级 Dao |
| **#124** | [`VariationWeapon`](Content/Items/Weapons/Daos/VariationWeapon.cs) | 变化 | 高级 Dao |
| **#125** | [`UnrealWeapon`](Content/Items/Weapons/Daos/UnrealWeapon.cs) | 虚幻 | 高级 Dao |
| **#126** | [`InfoWeapon`](Content/Items/Weapons/Daos/InfoWeapon.cs) | 信息 | 高级 Dao |
| **#127** | [`CharmWeapon`](Content/Items/Weapons/Daos/CharmWeapon.cs) | 媚 | 高级 Dao |
| **#128** | [`KnifeWeapon`](Content/Items/Weapons/Daos/KnifeWeapon.cs) | 刀 | 高级 Dao |
| **#129** | [`TacticalWeapon`](Content/Items/Weapons/Daos/TacticalWeapon.cs) | 战术 | 高级 Dao |
| **#130** | [`DrawWeapon`](Content/Items/Weapons/Daos/DrawWeapon.cs) | 画 | 高级 Dao |
| **#131** | [`PractiseWeapon`](Content/Items/Weapons/Daos/PractiseWeapon.cs) | 修炼 | 高级 Dao |
| **#132** | [`PelletWeapon`](Content/Items/Weapons/Daos/PelletWeapon.cs) | 丸 | 高级 Dao |
| **#133** | [`BanWeapon`](Content/Items/Weapons/Daos/BanWeapon.cs) | ban | 特殊 |
| **#134** | [`SuccessFailureWeapon`](Content/Items/Weapons/Daos/SuccessFailureWeapon.cs) | 成败 | 特殊 |

---

## 第八阶段：Materials（106 项）

> **目标**：完善所有材料物品
> **注意**：这是最大的批次，建议按子类别分批处理

### 8a. 小说核心材料（约 20 项）

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#135** | [`仙元石`](Content/Items/Materials/仙元石.cs) | 修仙核心货币 |
| **#136** | [`天晶`](Content/Items/Materials/天晶.cs) | 核心材料 |
| **#137** | [`金精`](Content/Items/Materials/金精.cs) | 核心材料 |
| **#138** | [`地煞粉`](Content/Items/Materials/地煞粉.cs) | 核心材料 |
| **#139** | [`裂天粉`](Content/Items/Materials/裂天粉.cs) | 核心材料 |
| **#140** | [`星辰石`](Content/Items/Materials/星辰石.cs) | 核心材料 |
| **#141** | [`紫金石`](Content/Items/Materials/紫金石.cs) | 核心材料 |
| **#142** | [`紫金化石`](Content/Items/Materials/紫金化石.cs) | 核心材料 |
| **#143** | [`赤练金`](Content/Items/Materials/赤练金.cs) | 核心材料 |
| **#144** | [`霸铜`](Content/Items/Materials/霸铜.cs) | 核心材料 |
| **#145** | [`正天银`](Content/Items/Materials/正天银.cs) | 核心材料 |
| **#146** | [`黑油`](Content/Items/Materials/黑油.cs) | 核心材料 |
| **#147** | [`黑金铠甲`](Content/Items/Materials/黑金铠甲.cs) | 核心材料 |
| **#148** | [`暗流金钢`](Content/Items/Materials/暗流金钢.cs) | 核心材料 |
| **#149** | [`青铜面具`](Content/Items/Materials/青铜面具.cs) | 核心材料 |
| **#150** | [`无常石`](Content/Items/Materials/无常石.cs) | 核心材料 |
| **#151** | [`火炭石`](Content/Items/Materials/火炭石.cs) | 核心材料 |
| **#152** | [`炎鸿石`](Content/Items/Materials/炎鸿石.cs) | 核心材料 |
| **#153** | [`物品寒玉`](Content/Items/Materials/物品寒玉.cs) | 核心材料 |
| **#154** | [`珍珠土`](Content/Items/Materials/珍珠土.cs) | 核心材料 |

### 8b. 生物材料（约 30 项）

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#155** | [`万目大明牛`](Content/Items/Materials/万目大明牛.cs) | 生物材料 |
| **#156** | [`九宫鹤`](Content/Items/Materials/九宫鹤.cs) | 生物材料 |
| **#157** | [`九子龙图`](Content/Items/Materials/九子龙图.cs) | 生物材料 |
| **#158** | [`六头大蛇黑血`](Content/Items/Materials/六头大蛇黑血.cs) | 生物材料 |
| **#159** | [`军团蚁`](Content/Items/Materials/军团蚁.cs) | 生物材料 |
| **#160** | [`刺脊星龙鱼`](Content/Items/Materials/刺脊星龙鱼.cs) | 生物材料 |
| **#161** | [`合鸣鹰`](Content/Items/Materials/合鸣鹰.cs) | 生物材料 |
| **#162** | [`夜狼皇`](Content/Items/Materials/夜狼皇.cs) | 生物材料 |
| **#163** | [`小箭尾雕`](Content/Items/Materials/小箭尾雕.cs) | 生物材料 |
| **#164** | [`巨音贝`](Content/Items/Materials/巨音贝.cs) | 生物材料 |
| **#165** | [`幽火龙蟒`](Content/Items/Materials/幽火龙蟒.cs) | 生物材料 |
| **#166** | [`斑虎蜜蜂蜂翅`](Content/Items/Materials/斑虎蜜蜂蜂翅.cs) | 生物材料 |
| **#167** | [`板栗牦牛`](Content/Items/Materials/板栗牦牛.cs) | 生物材料 |
| **#168** | [`混天鹰`](Content/Items/Materials/混天鹰.cs) | 生物材料 |
| **#169** | [`追虹鹰`](Content/Items/Materials/追虹鹰.cs) | 生物材料 |
| **#170** | [`铁冠鹰`](Content/Items/Materials/铁冠鹰.cs) | 生物材料 |
| **#171** | [`铁喙飞鹤`](Content/Items/Materials/铁喙飞鹤.cs) | 生物材料 |
| **#172** | [`风狼王`](Content/Items/Materials/风狼王.cs) | 生物材料 |
| **#173** | [`马群`](Content/Items/Materials/马群.cs) | 生物材料 |
| **#174** | [`黄玉狮子`](Content/Items/Materials/黄玉狮子.cs) | 生物材料 |
| **#175** | [`鸡年兽`](Content/Items/Materials/鸡年兽.cs) | 生物材料 |
| **#176** | [`长恨蛛`](Content/Items/Materials/长恨蛛.cs) | 生物材料 |
| **#177** | [`附天菌`](Content/Items/Materials/附天菌.cs) | 生物材料 |
| **#178** | [`雪莲花精`](Content/Items/Materials/雪莲花精.cs) | 生物材料 |
| **#179** | [`青矛竹`](Content/Items/Materials/青矛竹.cs) | 生物材料 |
| **#180** | [`骨刺`](Content/Items/Materials/骨刺.cs) | 生物材料 |
| **#181** | [`鬼嘤音`](Content/Items/Materials/鬼嘤音.cs) | 生物材料 |
| **#182** | [`魅蓝电影`](Content/Items/Materials/魅蓝电影.cs) | 生物材料 |
| **#183** | [`天龙完整骨骼`](Content/Items/Materials/天龙完整骨骼.cs) | 生物材料 |
| **#184** | [`走肉树`](Content/Items/Materials/走肉树.cs) | 生物材料 |

### 8c. 鱼/水生物材料（约 10 项）

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#185** | [`散文鲤`](Content/Items/Materials/散文鲤.cs) | 鱼材料 |
| **#186** | [`金龙鱼`](Content/Items/Materials/金龙鱼.cs) | 鱼材料 |
| **#187** | [`银龙鱼`](Content/Items/Materials/银龙鱼.cs) | 鱼材料 |
| **#188** | [`铜龙鱼`](Content/Items/Materials/铜龙鱼.cs) | 鱼材料 |
| **#189** | [`铁龙鱼`](Content/Items/Materials/铁龙鱼.cs) | 鱼材料 |
| **#190** | [`真武鲤`](Content/Items/Materials/真武鲤.cs) | 鱼材料 |
| **#191** | [`物品龙鱼`](Content/Items/Materials/物品龙鱼.cs) | 鱼材料 |
| **#192** | [`物品龙鳞土`](Content/Items/Materials/物品龙鳞土.cs) | 水材料 |
| **#193** | [`珠线水`](Content/Items/Materials/珠线水.cs) | 水材料 |
| **#194** | [`水核`](Content/Items/Materials/水核.cs) | 水材料 |

### 8d. 植物/自然材料（约 15 项）

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#195** | [`地老木`](Content/Items/Materials/地老木.cs) | 植物材料 |
| **#196** | [`缘木`](Content/Items/Materials/缘木.cs) | 植物材料 |
| **#197** | [`月兰花`](Content/Items/Materials/月兰花.cs) | 植物材料 |
| **#198** | [`斑斓霸王花`](Content/Items/Materials/斑斓霸王花.cs) | 植物材料 |
| **#199** | [`脉轮瓜`](Content/Items/Materials/脉轮瓜.cs) | 植物材料 |
| **#200** | [`气功果核`](Content/Items/Materials/气功果核.cs) | 植物材料 |
| **#201** | [`物品神鹿果`](Content/Items/Materials/物品神鹿果.cs) | 植物材料 |
| **#202** | [`流光果`](Content/Items/Materials/流光果.cs) | 植物材料 |
| **#203** | [`石猴毫毛`](Content/Items/Materials/石猴毫毛.cs) | 植物材料 |
| **#204** | [`星夜黏涎`](Content/Items/Materials/星夜黏涎.cs) | 植物材料 |
| **#205** | [`星意`](Content/Items/Materials/星意.cs) | 植物材料 |
| **#206** | [`心境`](Content/Items/Materials/心境.cs) | 植物材料 |
| **#207** | [`天露`](Content/Items/Materials/天露.cs) | 植物材料 |
| **#208** | [`天柱风`](Content/Items/Materials/天柱风.cs) | 自然材料 |
| **#209** | [`六声茶`](Content/Items/Materials/六声茶.cs) | 植物材料 |

### 8e. 建筑/场所材料（约 15 项）

| 优先级 | 名称 | 文件 |
|--------|------|------|
| **#210** | [`中央大殿`](Content/Items/Materials/中央大殿.cs) | 建筑材料 |
| **#211** | [`五光山`](Content/Items/Materials/五光山.cs) | 建筑材料 |
| **#212** | [`五相赌斗门`](Content/Items/Materials/五相赌斗门.cs) | 建筑材料 |
| **#213** | [`人海雏形`](Content/Items/Materials/人海雏形.cs) | 建筑材料 |
| **#214** | [`血海雏形`](Content/Items/Materials/血海雏形.cs) | 建筑材料 |
| **#215** | [`炼道大阵`](Content/Items/Materials/炼道大阵.cs) | 建筑材料 |
| **#216** | [`超级仙阵`](Content/Items/Materials/超级仙阵.cs) | 建筑材料 |
| **#217** | [`金晓大殿`](Content/Items/Materials/金晓大殿.cs) | 建筑材料 |
| **#218** | [`镇魔塔`](Content/Items/Materials/镇魔塔.cs) | 建筑材料 |
| **#219** | [`眠云棺椁`](Content/Items/Materials/眠云棺椁.cs) | 建筑材料 |
| **#220** | [`棺材`](Content/Items/Materials/棺材.cs) | 建筑材料 |
| **#221** | [`飞沙地洞`](Content/Items/Materials/飞沙地洞.cs) | 建筑材料 |
| **#222** | [`漂游炼巢`](Content/Items/Materials/漂游炼巢.cs) | 建筑材料 |
| **#223** | [`功德方尖碑`](Content/Items/Materials/功德方尖碑.cs) | 建筑材料 |
| **#224** | [`市井`](Content/Items/Materials/市井.cs) | 建筑材料 |

### 8f. 榜单/功能材料（约 7 项）

| 优先级 | 名称 | 文件 |
|--------