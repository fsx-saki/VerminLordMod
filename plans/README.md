# VerminLordMod - AI 辅助编码指南

## 项目定位
基于《蛊真人》小说的 Terraria tModLoader 模组。核心是【蛊虫】系统。

## 关键架构

### 蛊虫物品继承链
```
ModItem
  └── GuBaseItem (IGu)          ← 所有蛊虫的基座，统一提供炼制/控蛊/真元系统
       ├── GuWeaponItem          ← 手持武器型蛊虫（攻击间隔、真元消耗）
       │    └── DaoWeapon (元素道系武器)  ← IOnHitEffectProvider（命中效果）
       │         ├── FireWeapon / WaterWeapon / WindWeapon / MoonWeapon / ...
       │         └── 具体武器蛊
       ├── GuAccessoryItem       ← 饰品型蛊虫
       ├── PassiveGuItem         ← 空窍被动蛊
       └── GuHouseItem (IGuHouse) ← 仙蛊屋
```

### 弹幕系统（BaseBullet 行为组合）
所有新弹幕使用 `BaseBullet` + 行为组合（而非继承 ModProjectile）：
```csharp
class MyProj : BaseBullet {
    protected override void RegisterBehaviors() {
        Behaviors.Add(new AimBehavior(...));      // 飞行
        Behaviors.Add(new TrailBehavior(...));     // 拖尾
        Behaviors.Add(new GlowDrawBehavior(...));  // 发光
        // + DustTrail / DebuffOnHit / DustOnHit / MoonTrail / IceTrail / PowerTrail ...
    }
}
```

### 真元系统
- `QiResourcePlayer` — 真元值
- `QiRealmPlayer` — 境界/转数
- 武器每次使用消耗 `qiCost` 真元

### 空窍系统
- `KongQiaoPlayer` — 空窍状态，`KongQiaoSlot` 列表存储已炼化蛊虫
- 界面：`KongQiaoUI`
- 蛊虫可取出（包括本命蛊，取出后下一个自动晋升）

### NPC 对话系统
- `GuMasterBase` (`ModNPC`, `IGuMasterAI`) — 蛊师NPC基类
- `DialogueTreeManager` — 对话树系统（节点/选项/效果）
- `DialogueTreeUI` — 对话树UI面板

### 公共工具（可直接调用）
- `MoonBurstHelper.SpawnBurst(pos, color, scale)` — 月光三层爆散
- `MoonBurstHelper.SpawnSmallBurst(pos, color, scale)` — 小型爆散
- 尸体系统已改为 `CorpseBag`（宝藏袋式），不再使用弹幕实体

## 编码规范
- **新弹幕一律用 BaseBullet**，不要用 ModProjectile
- 使用 `using VerminLordMod.Common.BulletBehaviors;` 引用行为
- 贴图放在同目录下同名 .png
- 物品贴图 24×24，弹幕贴图 48×24（月光蛊新月弧形状）

## 当前完成状态
- ✅ 月道全系列蛊虫已实装（15+ 物品）
- ✅ MoonBaseGu 月道技术储备（6种攻击模式，R键切换）
- ✅ 邀月蛊三段式追踪
- ✅ 霜霖月蛊 + 月手刀（力道叠加） + 月影蛊 + 月旋蛊 etc.
- ✅ 尸体系统改造为 CorpseBag
- ✅ 对话系统修复（信念持久化）
- ✅ 本命蛊可取出
- ✅ 武器真元消耗和攻击间隔修复

## 开始工作
开发新蛊虫的顺序：
1. 在对应转数目录创建物品类（例：`Content/Items/Weapons/Three/MyNewGu.cs`）
2. 在 `Content/Projectiles/` 创建弹幕类（继承 BaseBullet）
3. 在 `Localization/zh-Hans_Mods.VerminLordMod.hjson` 添加本地化
4. 贴图用月光蛊改色（24×24物品 / 48×24弹幕）
