# 蛊虫开发闭环指南 (Gu Development Closed-Loop Guide)

> 版本: 2026-05-17 | 基于 Behavior 系统的脱耦合开发流程

---

## 一、核心架构

```
蛊虫武器 (Gu Weapon)
  └─ 继承 GuWeaponItem / 对应Dao基类
  └─ 实现 IKinematicProvider / IOnHitEffectProvider / ITacticalTriggerProvider

弹幕 (Projectile)
  └─ 继承 BaseBullet
  └─ 仅在 RegisterBehaviors() 中组合 Behavior
  └─ 禁止重写 AI() — BaseBullet 已 sealed
  └─ 如须自定义帧逻辑，使用 OnAI() / OnPostAI()
```

### 三层接口分离

| 接口 | 职责 |
|------|------|
| `IKinematicProvider` | 发射逻辑：弹幕类型、速度、散射角度、自定义Shoot |
| `IOnHitEffectProvider` | 命中效果：DoT、减速、破甲、吸血、DaoEffectTags |
| `ITacticalTriggerProvider` | 战术触发：低血量、夜晚、浮空、真气耗尽等条件Buff |

---

## 二、Behavior 系统（弹幕行为库）

所有弹幕行为必须通过 `IBulletBehavior` 实现，严禁在 Projectile 中硬编码逻辑。


### 2.3 扩展 Behavior 系统的原则

当现有 Behavior 无法满足需求时：

1. **在 `Common/BulletBehaviors/` 创建新 Behavior 类**
2. **实现 `IBulletBehavior` 接口**（或继承已有 Behavior 扩展）
3. **命名规范**: `{功能描述}Behavior.cs`
4. **可用生命周期钩子**:
   - `OnSpawn(Projectile, IEntitySource)` — 生成时
   - `AI(Projectile)` — 每帧更新
   - `OnHitNPC(Projectile, NPC, hitInfo, damageDone)` — 命中NPC
   - `OnHitPlayer(Projectile, Player, hitInfo)` — 命中玩家
   - `OnKill(Projectile, timeLeft)` — 销毁时
   - `PreDraw(Projectile, ref lightColor)` / `PostDraw(Projectile, lightColor)` — 绘制

5. **禁止**在 Projectile 类中直接写 `OnKill`/`OnHit`/`AI` 等硬编码逻辑

---

## 三、创建新蛊虫的标准流程

### Step 1: 确定蛊虫属性
- 在 `novel.db` 中查询蛊虫信息（等级、能力、流派）--必须确认信息准确
- 确定 `DaoType`（血道/骨道/魂道/冰道/雷道/风道/...）
- 确定转数（一转→One/, 二转→Two/, 三转→Three/, 四转→Four/, 五转→Five/, 六转→Six/ 目录）
- 七转→Seven/, 八转→Eight/, 九转→Nine/ 目录（高阶仙蛊）

### Step 2: 创建武器类 (GuWeapon)
```
Content/Items/Weapons/{转数目录}/{蛊虫名}Gu.cs
```
- 继承对应的 Dao 基类（如 `BloodWeapon`, `BoneWeapon`）
- 设置 `Item.damage`, `Item.shootSpeed`, `Item.useTime` 等
- 设置 `Item.shoot = ModContent.ProjectileType<{蛊虫名}Proj>()`
- 可选：重写 `OnHitEffects` 设置 Dao 命中效果
- 可选：重写 `Triggers` 设置战术触发条件
- 添加中文注释，介绍蛊虫的功能和属性

### Step 3: 创建弹幕类 (Projectile)
```
Content/Projectiles/{蛊虫名}Proj.cs
```
- 继承 `BaseBullet`
- **仅重写 `RegisterBehaviors()`**，组合所需 Behavior
- 如需自定义逻辑，使用 `OnAI()` / `OnPostAI()`（非 sealed）

**模板**:
```csharp
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using VerminLordMod.Common.BulletBehaviors;

namespace VerminLordMod.Content.Projectiles
{
    public class MyGuProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 1. 渲染体 + 碰撞
            Behaviors.Add(new ParticleBodyBehavior(particleCount: 6, bodyRadius: 8f)
            {
                ParticleSize = 0.5f,
                ColorStart = new Color(200, 50, 50, 200),
                ColorEnd = new Color(200, 50, 50, 200),
                SwirlSpeed = 0.06f,
                ReturnForce = 0.8f,
                EnableLight = false,
            });

            // 2. 拖尾
            Behaviors.Add(new TrailBehavior(...) { ... });

            // 3. 命中效果
            Behaviors.Add(new DebuffOnHitBehavior { ... });

            // 4. 销毁效果
            Behaviors.Add(new OnKillAoEBehavior { ... });
        }
    }
}
```

### Step 4: 创建贴图（MANDATORY! tModLoader 强制要求）

**每个武器和弹幕都必须有同名同位置的 PNG 贴图！**
编译能通过但加载会失败（TML001错误）。

- 武器贴图: `Content/Items/Weapons/{转数}/{蛊虫名}Gu.png` — 24x24
- 弹幕贴图: `Content/Projectiles/{蛊虫名}Proj.png` — 16x36 或 48x24

**注意**:
- 所有 Dao 基类（`BloodWeapon`, `BoneWeapon` 等）都是 `abstract`，无需贴图
- 如果弹幕使用 `override string Texture => "Terraria/Images/..."` 引用原版贴图，则无需自己的PNG

**快速生成占位贴图的 Python 脚本**:
```python
from PIL import Image, ImageDraw
import math

def make_texture(path, size, color, shape="circle"):
    img = Image.new("RGBA", size, (0,0,0,0))
    d = ImageDraw.Draw(img)
    cx, cy = size[0]//2, size[1]//2
    r = min(size)//2 - 1
    if shape == "circle":
        d.ellipse([cx-r, cy-r, cx+r, cy+r], fill=color)
    elif shape == "tall_oval":
        d.ellipse([cx-size[0]//3, 2, cx+size[0]//3, size[1]-2], fill=color)
    img.save(path)

# 武器 24x24
make_texture("MyGu.png", (24,24), (200,50,50,255), "circle")
# 弹幕 16x36
make_texture("MyGuProj.png", (16,36), (200,50,50,255), "tall_oval")
```

### Step 5: 注册本地化
在 `Localization/zh-Hans_Mods.VerminLordMod.hjson` 和 `en-US_Mods.VerminLordMod.hjson` 中添加物品名和Tooltip。


### Step 6: 编译验证
```bash
cd ModSources/VerminLordMod
dotnet build VerminLordMod.csproj 2>&1 | grep "error CS"
```
确保零 C# 编译错误。

### Step 7: 将已经实现的蛊虫存储到新数据库
- 在 `finish.db` 中添加新的蛊虫记录--必须确认信息准确
- 确保所有属性都正确填写

---

## 四、常见问题与解决方案

### Q1: "继承成员 BaseBullet.AI() 是密封的，无法进行重写"
**原因**: BaseBullet 的 `AI()` 是 `sealed override` 的。
**解决**: 使用 `protected override void OnAI()` 代替。

### Q2: "未能找到类型或命名空间名 BoneWeapon（或其他Dao基类）"
**原因**: 缺少 using 指令。
**解决**: 添加 `using VerminLordMod.Content.Items.Weapons.Daos;`

### Q3: "不实现接口成员 IOnHitEffectProvider.CustomOnHitNPC"
**原因**: Gu 武器类实现了接口但未提供默认实现。
**解决**: 添加 `public void CustomOnHitNPC(...) { }` 空实现。

### Q4: "当前上下文中不存在名称 Vector2 / Color"
**原因**: 缺少 `using Microsoft.Xna.Framework;`

### Q5: "未能找到类型或命名空间名 IEntitySource"
**原因**: 缺少 `using Terraria.DataStructures;`（BaseBullet.OnSpawn 签名需要）

### Q6: "TML001: Image loading failed / Stream was too long"
**原因**: 武器或弹幕缺少同名同位置的 PNG 贴图。
**解决**: 确保每个 `.cs` 武器/弹幕文件旁有对应的 `.png` 文件。abstract 类不需要。

---

## 五、本次重构总结 (2026-05-17)

### 新增 Behavior（扩展系统而非硬编码）

| 新增文件 | 用途 | 替代的硬编码 |
|----------|------|-------------|
| `Common/BulletBehaviors/OnKillAoEBehavior.cs` | 销毁时AOE伤害+Debuff | FrostDemonProj / BoneThornProj 的 OnKill 硬编码 |
| `Common/BulletBehaviors/OnKillProjectileBurstBehavior.cs` | 销毁时发射子弹幕 | FrostDemonProj / BoneThornProj 的子碎片生成 |
| `Common/BulletBehaviors/DebuffOnHitBehavior.cs` | 命中附加Debuff | 各 Proj 的 OnHitNPC 硬编码 |
| `Common/BulletBehaviors/DustOnHitBehavior.cs` | 命中粉尘效果 | 各 Proj 的 HitDust 硬编码 |
| `Common/BulletBehaviors/ScaleOverLifeBehavior.cs` | 生命周期缩放+光照 | FrostDemonProj 的缩放/光照逻辑 |

### 修改的 Behavior（增强复用性）

| 文件 | 改动 |
|------|------|
| `Common/BulletBehaviors/ChargeProjectileBehavior.cs` | 新增 `ChargePositionOffset` 支持固定头顶蓄力位置（BloodDropperProj） |

### 重构的弹幕（全部改为纯 Behavior 组合）

| 弹幕 | 使用的 Behavior |
|------|----------------|
| `BoneThornProj.cs` | ParticleBody + BoneTrail + DebuffOnHit + OnKillAoE + OnKillProjectileBurst |
| `BoneSpikeProj.cs` | ParticleBody + BoneTrail + Gravity + DustKill |
| `FrostDemonProj.cs` | ScaleOverLife + GlowLight + OnKillAoE + OnKillProjectileBurst |
| `IceBombProj.cs` | ParticleBody + IceTrail + ExplosionKill + IceCrystalPlace |
| `BloodDropperProj.cs` | ChargeProjectile(固定头顶) + ParticleBody + BloodTrail + DebuffOnHit |
| `BloodFrenzyProj.cs` | ParticleBody + BloodTrail + DebuffOnHit + Homing + DustOnHit |
| `GhostFaceProj.cs` | ParticleBody + SoulTrail + DebuffOnHit + Homing + Fade |
| `WindFlowerProj.cs` | ParticleBody + WindTrail + DebuffOnHit + Gravity |
| `ThunderWingProj.cs` | ParticleBody + LightningTrail + DebuffOnHit + Homing + GlowLight |
| `FlyingBoneShieldProj.cs` | ParticleBody + BoneTrail + Bounce + DebuffOnHit + DustOnHit |
| `FlyingSoulProj.cs` | ParticleBody + SoulTrail + DebuffOnHit + Homing + Fade |

### 新增贴图（共20张）
- 10 张武器贴图 (`Three/`, `Four/`, `Five/` 目录)
- 10 张弹幕贴图 (`Content/Projectiles/`)

### 编译结果
- **C# 编译**: 零错误，完全通过
- **TML001 打包**: 预存问题，与本次改动无关