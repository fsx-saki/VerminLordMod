# VerminLordMod 项目基础介绍（Agent 参考）

## 一、项目概述

VerminLordMod 是基于 tModLoader（Terraria 模组框架）开发的《蛊真人》小说改编 Mod。
项目将小说中的蛊修体系（蛊虫、真元、境界、道痕、空窍、炼化等核心概念）转化为 Terraria 游戏机制。

---

## 二、核心架构

### 2.1 蛊虫武器继承链

```
ModItem
  └── GuWeaponItem          ← 蛊虫武器基类（实现 IGu, IWeaponCanReforge）
        ├── DaoWeapon        ← 道系核心基类（实现 IKinematicProvider, IOnHitEffectProvider, ITacticalTriggerProvider）
        │     ├── MoonWeapon     ← 月道
        │     ├── FireWeapon     ← 火道
        │     ├── BloodWeapon    ← 血道
        │     ├── IceSnowWeapon  ← 冰雪道
        │     ├── LightningWeapon← 雷道
        │     ├── PowerWeapon    ← 力道
        │     ├── EatingWeapon   ← 食道
        │     ├── VariationWeapon← 变化道
        │     └── ... (共52个道系基类，每个仅覆盖 moddustType)
        └── [具体蛊虫武器]   ← 如 Moonlight, IceAwlGu, HuoLongGu 等
```

### 2.2 GuWeaponItem 基类核心机制

| 字段 | 说明 | 默认值 |
|------|------|--------|
| `qiCost` | 使用消耗真元 | 7 |
| `controlQiCost` | 炼化消耗真元 | 10 |
| `unitConntrolRate` | 一次炼化增加进度 | 10 |
| `uncontrolRate` | 脱离炼化速度 | 0.01f |
| `_useTime` | 左键使用速度 | 20 |
| `_guLevel` | 转数 | 1 |
| `hasBeenControlled` | 是否已炼化 | false |
| `controlRate` | 炼化进度 0-100 | 0f |

**核心逻辑**：
- **左键使用**：检查炼化状态→检查境界→消耗真元→发射弹幕
- **右键炼化**：消耗真元→增加炼化进度→满100%后可炼入空窍
- **炼化衰减**：未炼化时 controlRate 自然下降
- **反噬机制**：强行使用高转蛊虫会扣血
- **空窍炼入**：炼化完成后右键可炼入 KongQiaoPlayer

### 2.3 DaoWeapon 基类扩展

DaoWeapon 在 GuWeaponItem 基础上增加了：
- **DaoType 属性**：标记道途类型
- **IKinematicProvider**：弹道参数（ShootSpeed, ShootCount, SpreadAngle 等）
- **IOnHitEffectProvider**：命中效果（DoT, 减速, 碎甲, 虚弱, 吸血）
- **ITacticalTriggerProvider**：战术触发器（OnLowHealth, OnNightTime 等）
- **道痕增幅**：Shoot 时自动乘以 DaoHenPlayer 的倍率

### 2.4 弹幕行为系统

```
ModProjectile
  └── BaseBullet            ← 弹幕基类（行为组合模式）
        └── RegisterBehaviors() 中注册行为：
              ├── AimBehavior        直线飞行
              ├── HomingBehavior     追踪飞行
              ├── TrailBehavior      虚影拖尾
              ├── LiquidTrailBehavior 液体拖尾（如火龙身体）
              ├── GlowDrawBehavior   发光绘制
              ├── DebuffOnHitBehavior 命中上Debuff
              ├── DustOnHitBehavior  命中粒子
              ├── KillDustBurstBehavior 死亡爆炸粒子
              └── DustKillBehavior   死亡粒子
```

---

## 三、核心系统

### 3.1 真元系统 (QiResourcePlayer)

- **QiMaxCurrent**：真元上限 = `10^(GuLevel-1) * 2^LevelStage * 100 * 资质 / 10`
- **QiCurrent**：当前真元
- **QiOccupied**：被空窍蛊虫占据的额度
- **ConsumeQi(amount)**：消耗真元，不足返回 false
- **恢复速率**：`GuLevel * 资质倍率 / 2`

### 3.2 境界系统 (QiRealmPlayer)

- **GuLevel**：转数 1-10
- **LevelStage**：0=初期, 1=中期, 2=后期, 3=巅峰
- **空窍格子**：`3 + (GuLevel-1)*2 + LevelStage`

### 3.3 空窍系统 (KongQiaoPlayer)

- 蛊虫炼化/取出/启用/休眠
- 真元占据计算
- 死亡处理（忠诚度<40 → 叛逃/自毁，本命蛊保留）

### 3.4 道痕系统 (DaoHenPlayer)

- 50+ → 1.1x, 200+ → 1.25x, 500+ → 1.5x
- 按道途分类，影响对应道系蛊虫伤害

### 3.5 进化系统 (GuEvolutionSystem)

- 一转→九转进化路径
- 成功率受境界和本命蛊羁绊影响
- 失败惩罚：降阶/死亡

### 3.6 养殖系统 (GuBreedingSystem)

- 蛊盒/蛊鼎/灵池/斗蛊台/祭坛
- 喂食→培养→竞争→收获/进化

### 3.7 蛊病系统 (GuDiseaseSystem)

- 毒蛊/寄生/控心/噬血/蚀骨/夺魂/瘟疫/蛊热
- 严重程度分级，可传染

---

## 四、道途类型 (DaoType) 完整列表

搬道(Ban)、血道(Blood)、骨道(Bone)、魅道(Charm)、云道(Cloud)、暗道(Dark)、御道(Draw)、梦道(Dream)、食道(Eating)、火道(Fire)、飞道(Flying)、金道(Gold)、冰雪道(IceSnow)、情报道(Info)、杀道(Killing)、刀道(Knife)、生死道(LifeDeath)、光道(Light)、雷道(Lightning)、情道(Love)、运道(Luck)、月道(Moon)、泥道(Mud)、丹道(Pellet)、人道(Person)、毒道(Poison)、力道(Power)、修道(Practise)、气道(Qi)、规则道(Rule)、影道(Shadow)、天道(Sky)、奴道(Slave)、魂道(Soul)、空间道(Space)、星道(Star)、盗道(Stealing)、成败道(SuccessFailure)、剑道(Sword)、战术道(Tactical)、时间道(Time)、虚道(Unreal)、变道(Variation)、音道(Voice)、虚空道(Void)、战道(War)、水道(Water)、风道(Wind)、智道(Wisdom)、木道(Wood)、阴阳道(YinYang)

---

## 五、已有实现参考

### 一转蛊虫示例：Moonlight（月光蛊）
- 继承 MoonWeapon，damage=20，shootSpeed=7f
- 弹幕：AimBehavior + TrailBehavior + GlowDrawBehavior + DustKillBehavior
- 穿透99次，存活60帧

### 二转蛊虫示例：IceAwlGu（冰锥蛊）
- 继承 IceSnowWeapon，damage=50，shootSpeed=10f
- Shoot：五方向散射（-10°~+10°间隔5°）
- 弹幕：AimBehavior + TrailBehavior，穿透3次，命中 Chilled 240帧

### 四转蛊虫示例：HuoLongGu（火龙蛊）
- 继承 FireWeapon + IOnHitEffectProvider，damage=80
- 弹幕：AimBehavior + LiquidTrailBehavior + DebuffOnHitBehavior + DustOnHitBehavior + KillDustBurstBehavior
- 命中：OnFire 300帧 + CursedInferno 120帧

### 五转蛊虫示例：XueHeMangGu（血河蟒蛊）
- 继承 BloodWeapon + IOnHitEffectProvider，damage=85
- 弹幕：HomingBehavior（700像素追踪）+ LiquidTrailBehavior + GlowDrawBehavior
- 命中：DoT + LifeSteal + Slow + ArmorShred + Weaken

---

## 六、Special 目录占位蛊虫现状

**272个蛊虫物品全部为占位实现**，仅包含 `SetDefaults()` 设置基础属性。
这些物品直接继承 `ModItem`（而非 `GuWeaponItem`），没有炼化系统、真元消耗或战斗逻辑。

### 按稀有度/转数分布

| 稀有度 | 转数范围 | 数量 | value |
|--------|---------|------|-------|
| White | 凡蛊/未定 | 87 | 500-1000 |
| Green | 一~二转 | 6 | 5000 |
| Orange | 二~三转 | 13 | 10000 |
| LightRed | 三~四转 | 18 | 20000 |
| Pink | 四~五转 | 12 | 50000 |
| LightPurple | 五~六转 | 36 | 100000 |
| Lime | 七转 | 47 | 500000 |
| Cyan | 八转 | 41 | 1000000 |
| Purple | 九转 | 7 | 5000000 |
| Red | 概念级 | 5 | 10000000 |

---

## 七、实现新蛊虫的标准流程

1. **确定蛊虫属性**：转数、道途、分类（攻击/防御/辅助/功能/特殊）
2. **选择基类**：
   - 攻击蛊 → 继承对应道系 `XxxWeapon`（如 FireWeapon, MoonWeapon）
   - 防御蛊 → 继承 GuWeaponItem，重写防御逻辑
   - 辅助蛊 → 继承 GuWeaponItem，实现 ISupportGu
   - 特殊蛊 → 根据具体需求定制
3. **设置物品属性**：damage, qiCost, controlQiCost, _guLevel, _useTime 等
4. **创建弹幕**（攻击蛊）：继承 BaseBullet，注册行为
5. **实现命中效果**（高转）：实现 IOnHitEffectProvider
6. **注册进化路径**（如需要）：在 GuEvolutionSystem.RegisterEvolutionPaths() 中添加
7. **添加 [ImplStatus] 标记**：标记实现状态

---

## 八、关键文件路径

| 用途 | 路径 |
|------|------|
| 蛊虫武器基类 | Content/Items/Weapons/GuWeaponItem.cs |
| 道系核心基类 | Content/Items/Weapons/Daos/DaoWeapon.cs |
| 道系武器基类 | Content/Items/Weapons/Daos/{DaoName}Weapon.cs |
| 弹幕基类 | Content/Projectiles/ (BaseBullet) |
| 真元系统 | Common/Players/QiResourcePlayer.cs |
| 境界系统 | Common/Players/QiRealmPlayer.cs |
| 空窍系统 | Common/Players/KongQiaoPlayer.cs |
| 道痕系统 | Common/Players/DaoHenPlayer.cs |
| 进化系统 | Common/Systems/GuEvolutionSystem.cs |
| 养殖系统 | Common/Systems/GuBreedingSystem.cs |
| 蛊病系统 | Common/Systems/GuDiseaseSystem.cs |
| IGu 接口 | Common/Abstractions/IGu.cs |
| IOnHitEffectProvider | Common/GuBehaviors/IOnHitEffectProvider.cs |
| DaoType 枚举 | Common/GuBehaviors/DaoType.cs |
| 占位蛊虫目录 | Content/Items/Special/ |
| 已实现武器目录 | Content/Items/Weapons/One~Nine/ |
