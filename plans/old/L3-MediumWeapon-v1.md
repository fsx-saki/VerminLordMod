# L3 实现文档：蛊虫施法媒介（MediumWeaponSystem）

> 版本：v1.0（基于小 d 设计要领统合）  
> 目标：实现「唯一媒介武器」，查询空窍驱动弹幕，验证杀招配方与平 A 齐射  
> 优先级：P0 / MVA  
> 预计工作量：2-3 天  
> 依赖：KongQiaoPlayer（GetActiveAttackGus）/ QiResourcePlayer（ConsumeQi）/ 现有弹幕系统

---

## 1. 交付目标

让玩家手持一把「蛊道媒介」武器，攻击时：
1. 查询空窍中启用的攻击蛊虫
2. 若匹配杀招配方（月光蛊+酒虫），释放高伤杀招弹幕，参与者休眠
3. 若不匹配，所有启用的攻击蛊同时散射发射各自的弹幕
4. 每次发射消耗真元（平 A 每只扣 10% 占据额度，杀招扣固定值）

**验收标准**：玩家能走通「炼化月光蛊+酒虫 → 启用两者 → 用媒介发射酒月斩杀招 → 杀招后两者休眠 → 等待冷却再启用」的闭环。

---

## 2. 核心原则（不可违背）

- **媒介是空壳**：自身 `Item.damage = 0`，所有伤害来自蛊虫数据
- **唯一媒介**：玩家只能持有一把（D-11）
- **查询而非持有**：媒介不"装备"蛊虫，每次攻击实时查询空窍状态
- **硬编码起步**：MVA 阶段杀招配方写死在代码里，验证通过后再抽象

---

## 3. 文件清单

| 文件 | 动作 | 说明 |
|------|------|------|
| `Content/Items/Weapons/GuMediumWeapon.cs` | **新建** | 唯一媒介武器物品，空壳触发器 |
| `Content/Projectiles/ShaZhaoJiuYueZhan.cs` | **新建** | 酒月斩杀招弹幕（高伤特效） |
| `Content/Items/Weapons/GuMediumWeapon.png` | **新建/占位** | 媒介武器贴图（可用原版木剑贴图临时替代） |
| `Content/Projectiles/ShaZhaoJiuYueZhan.png` | **新建/占位** | 杀招弹幕贴图（可用月光蛊贴图放大替代） |
| `Common/Players/KongQiaoPlayer.cs` | **修改** | 在 `TryRefineGu` 中增加 `ProjectileType` 映射（3-5 种攻击蛊） |

---

## 4. 核心数据结构

### 4.1 媒介武器物品

```csharp
public class GuMediumWeapon : ModItem
{
    public override void SetDefaults()
    {
        // === 空壳属性 ===
        Item.damage = 0;                    // 自身无伤害
        Item.DamageType = ModContent.GetInstance<InsectDamageClass>();  // 蛊术伤害类型
        Item.width = 28;
        Item.height = 28;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 25;                  // 基础攻速，实际频率由蛊虫决定
        Item.useAnimation = 25;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.knockBack = 0f;               // 击退由蛊虫决定
        Item.value = Item.sellPrice(0, 0, 0, 0);
        Item.rare = ItemRarityID.Green;
        Item.noMelee = true;               // 不执行近战碰撞
        Item.noUseGraphic = false;         // 显示挥动动画

        // 关键：不设置 Item.shoot，因为弹幕类型由空窍中的蛊虫动态决定
    }

    public override bool CanUseItem(Player player)
    {
        // 只有已开窍的玩家才能使用媒介
        var realm = player.GetModPlayer<QiRealmPlayer>();
        return realm.GuLevel > 0;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        var kongQiao = player.GetModPlayer<KongQiaoPlayer>();
        var qiResource = player.GetModPlayer<QiResourcePlayer>();

        // 1. 获取启用的攻击蛊
        var activeAttackGus = kongQiao.GetActiveAttackGus();
        if (activeAttackGus.Count == 0)
        {
            // 无蛊可用：哑火，播放空挥音效
            // 可选：消耗少量真元（空扣扳机）
            return false;
        }

        // 2. 检查杀招配方（MVA 硬编码）
        var shaZhao = MatchShaZhao(activeAttackGus);
        if (shaZhao != null && qiResource.ConsumeQi(shaZhao.QiCost, QiConsumeReason.ShaZhao))
        {
            // ===== 释放杀招 =====
            FireShaZhao(source, position, velocity, shaZhao, activeAttackGus, player);

            // D-10: 仅参与者休眠
            foreach (var gu in activeAttackGus.Where(g => shaZhao.RequiredGuTypes.Contains(g.GuTypeID)))
            {
                int index = kongQiao.KongQiao.IndexOf(gu);
                if (index >= 0) kongQiao.SetGuActive(index, false);
            }

            return false;  // 媒介自身不发射原版弹幕
        }

        // 3. 平 A 齐射（D-12: 同时散射）
        FireSalvo(source, position, velocity, activeAttackGus, qiResource, player);

        return false;  // 媒介自身不发射原版弹幕
    }

    // ===== 杀招匹配（MVA 硬编码） =====
    private ShaZhaoRecipe MatchShaZhao(List<KongQiaoSlot> activeGus)
    {
        var activeTypes = activeGus.Select(g => g.GuTypeID).ToHashSet();

        // 硬编码配方 1：月光蛊 + 酒虫 = 酒月斩
        int moonlightType = ModContent.ItemType<Moonlight>();      // 月光蛊物品 ID
        int wineBugType = ModContent.ItemType<WineBug>();          // 酒虫物品 ID

        if (activeTypes.Contains(moonlightType) && activeTypes.Contains(wineBugType))
        {
            return new ShaZhaoRecipe
            {
                Name = "酒月斩",
                RequiredGuTypes = new List<int> { moonlightType, wineBugType },
                QiCost = 30f,
                DamageMultiplier = 3f,
                MainProjectile = ModContent.ProjectileType<ShaZhaoJiuYueZhan>(),
                CooldownTicks = 300  // 5 秒冷却
            };
        }

        // MVA 阶段只有 1 条配方。P1 再扩展。
        return null;
    }

    // ===== 杀招发射 =====
    private void FireShaZhao(EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
        ShaZhaoRecipe recipe, List<KongQiaoSlot> participants, Player player)
    {
        // 计算总伤害：参与者蛊虫伤害之和 × 倍率
        int totalDamage = (int)(participants
            .Where(g => recipe.RequiredGuTypes.Contains(g.GuTypeID))
            .Sum(g => g.GuItem.damage) * recipe.DamageMultiplier);

        // 发射杀招弹幕（单发高伤）
        var proj = Projectile.NewProjectileDirect(source, position, velocity * 1.5f,
            recipe.MainProjectile, totalDamage, 6f, player.whoAmI);

        // 特效：弹幕放大、拖尾
        proj.scale = 1.5f;

        // 播放杀招音效（可用原版强力武器音效）
        SoundEngine.PlaySound(SoundID.Item20, position);
    }

    // ===== 平 A 齐射 =====
    private void FireSalvo(EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
        List<KongQiaoSlot> activeGus, QiResourcePlayer qiResource, Player player)
    {
        int count = activeGus.Count;
        float spreadAngle = 0.15f;           // 总散射弧度（约 8.6 度）
        float angleStep = count > 1 ? spreadAngle / (count - 1) : 0f;
        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < count; i++)
        {
            var gu = activeGus[i];

            // 角度微调
            float individualAngle = startAngle + angleStep * i;
            Vector2 shootVel = velocity.RotatedBy(individualAngle);

            // 速度随机波动（80% ~ 120%）
            shootVel *= 0.8f + Main.rand.NextFloat(0.4f);

            // 发射弹幕
            Projectile.NewProjectile(source, position, shootVel,
                gu.ProjectileType, gu.GuItem.damage, gu.GuItem.knockBack, player.whoAmI);

            // 每只蛊虫消耗真元：占据额度的 10%
            float cost = gu.QiOccupation * 0.1f;
            qiResource.ConsumeQi(cost, QiConsumeReason.NormalAttack);
        }
    }

    public override void AddRecipes()
    {
        // MVA 阶段：开局赠送或开窍时自动获得
        // P1 可加入合成配方
    }
}

// 杀招配方数据结构（MVA 硬编码用，P1 再抽象为系统）
public class ShaZhaoRecipe
{
    public string Name;
    public List<int> RequiredGuTypes;    // 需要的蛊虫物品 ID 列表
    public float QiCost;                 // 真元消耗
    public float DamageMultiplier;       // 伤害倍率
    public int MainProjectile;           // 杀招弹幕类型
    public int CooldownTicks;            // 冷却帧数
}
```

### 4.2 杀招弹幕（酒月斩）

```csharp
public class ShaZhaoJiuYueZhan : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
        Projectile.penetrate = 3;           // 穿透 3 个敌人
        Projectile.timeLeft = 120;        // 2 秒存在时间
        Projectile.alpha = 50;            // 半透明
        Projectile.light = 0.8f;          // 发光
        Projectile.scale = 1.5f;
        Projectile.extraUpdates = 1;        // 额外更新，更平滑
    }

    public override void AI()
    {
        // 旋转效果
        Projectile.rotation += 0.2f;

        // 拖尾粒子（可用月光蛊的粒子效果）
        if (Main.rand.NextBool(3))
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                DustID.MagicMirror, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f,
                150, Color.Cyan, 1.2f);
        }

        // 轻微追踪（智道蛊虫特性，MVA 简化）
        // 不实现复杂追踪，保持直线但带轻微弯曲
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // 命中特效：月光爆裂
        for (int i = 0; i < 5; i++)
        {
            Dust.NewDust(target.position, target.width, target.height,
                DustID.MagicMirror, Main.rand.Next(-3, 3), Main.rand.Next(-3, 3),
                150, Color.Cyan, 1.5f);
        }

        // 施加 debuff（可选）
        // target.AddBuff(ModContent.BuffType<MoonPoisonbuff>(), 120);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        // 发光绘制（可用 Additive 混合）
        // MVA 阶段可简化，直接使用原版绘制
        return true;
    }
}
```

---

## 5. KongQiaoPlayer 的 ProjectileType 映射

在 `KongQiaoPlayer.TryRefineGu()` 中，需要为每只蛊虫赋值对应的弹幕类型。MVA 阶段只映射已实现的攻击蛊：

```csharp
// 在 KongQiaoPlayer.TryRefineGu 中，增加 ProjectileType 赋值：
slot.ProjectileType = guItem.ModItem switch
{
    Moonlight => ModContent.ProjectileType<MoonlightProj>(),
    WineBug => ModContent.ProjectileType<WineBagFlowerProj>(),
    BoneSpearGu => ModContent.ProjectileType<BoneSpear>(),
    RiverStream => ModContent.ProjectileType<WaterArrowProj>(),
    MeteorGu => ModContent.ProjectileType<StarArrowProj>(),
    _ => ProjectileID.WoodenArrowFriendly  // 默认占位：木箭
};
```

**注意**：`WineBug`（酒虫）本身不是攻击蛊，但在杀招配方中作为参与者，其 `ProjectileType` 可设为占位值（杀招释放时不使用单个蛊虫的弹幕，而是使用杀招弹幕）。

---

## 6. 真元消耗责任分配（对照表）

| 消耗场景 | 消耗量 | 扣除位置 | 备注 |
|---------|--------|---------|------|
| 每只蛊虫平 A | `QiOccupation × 0.1` | `GuMediumWeapon.FireSalvo()` | 齐射时逐只扣除 |
| 杀招释放 | 配方固定值（如 30） | `GuMediumWeapon.Shoot()` | 匹配成功后一次性扣除 |
| 蛊虫维持（被动） | 每帧 0.5 | `KongQiaoPlayer.PostUpdate()` | 已在 L3 v2 实现 |
| 炼化蛊虫 | `damage × 2` | `KongQiaoPlayer.TryRefineGu()` | 已在 L3 v2 实现 |
| 六转返还 | 原消耗量（负值） | `GuWeaponItem.UseItem()` | 不在媒介系统中 |

---

## 7. 与现有系统的衔接

### 7.1 输入（查询）

| 数据 | 来源方法 | 消费位置 |
|------|---------|---------|
| 启用的攻击蛊列表 | `KongQiaoPlayer.GetActiveAttackGus()` | `GuMediumWeapon.Shoot()` |
| 蛊虫弹幕类型 | `KongQiaoSlot.ProjectileType` | `GuMediumWeapon.FireSalvo()` |
| 蛊虫伤害 | `KongQiaoSlot.GuItem.damage` | `GuMediumWeapon.FireSalvo()` / `FireShaZhao()` |
| 蛊虫占据额度 | `KongQiaoSlot.QiOccupation` | `GuMediumWeapon.FireSalvo()`（计算消耗） |
| 当前真元 | `QiResourcePlayer.QiCurrent` | `QiResourcePlayer.ConsumeQi()` |

### 7.2 输出（事件）

| 事件 | 发布位置 | 订阅者 |
|------|---------|--------|
| `GuActivatedEvent` | `KongQiaoPlayer.SetGuActive()` | NPC 层（感知玩家催动蛊虫） |
| `PlayerQiChangedEvent` | `QiResourcePlayer.ConsumeQi()` | NPC 层（感知玩家真元波动） |

### 7.3 与 GuYuePatrolGuMaster 的联动

当玩家使用媒介攻击时：
1. `KongQiaoPlayer.SetGuActive(true)` 发布 `GuActivatedEvent`
2. 巡逻蛊师的 `PerceptionSystem` 感知到玩家催动了月光蛊/酒虫
3. `NpcBrain` 更新信念：Confidence 上升，若玩家真元低则 RiskThreshold 下降
4. 下次决策可能转为 `DirectRaid` 或 `Test`

---

## 8. 验收标准

### 8.1 功能验收（必须全部通过）

| 场景 | 操作 | 预期行为 | 通过标准 |
|------|------|---------|---------|
| 媒介获取 | 开局/开窍 | 背包中获得一把「蛊道媒介」 | `Item.type == ModContent.ItemType<GuMediumWeapon>()` |
| 空窍无蛊 | 手持媒介攻击 | 无弹幕发射，哑火 | `Shoot()` 返回 false，无 Projectile 生成 |
| 单蛊平 A | 启用月光蛊，攻击 | 发射 1 发月光弹幕，消耗 3 点真元 | 弹幕类型为 MoonlightProj，QiCurrent 减少 |
| 双蛊齐射 | 启用月光蛊+骨枪蛊，攻击 | 发射 2 发弹幕，角度散射 | 两发弹幕角度不同，均造成伤害 |
| 杀招触发 | 启用月光蛊+酒虫，攻击 | 发射酒月斩杀招弹幕，高伤大特效 | 弹幕类型为 ShaZhaoJiuYueZhan，伤害 = (月光+酒虫伤害)×3 |
| 杀招休眠 | 释放酒月斩后 | 月光蛊和酒虫自动休眠 | `IsActive == false`，QiOccupied 归零 |
| 真元不足杀招 | 真元 < 30，尝试释放杀招 | 杀招失败，转为平 A 齐射 | 发射两发普通弹幕，不触发杀招 |
| 冷却后再战 | 等待 5 秒，重新启用蛊虫 | 可再次释放杀招 | 杀招成功，参与者再次休眠 |
| 媒介唯一 | 尝试获得第二把媒介 | 无法获得或自动合并 | 背包中只有 1 把 GuMediumWeapon |

### 8.2 体验验收

- [ ] 玩家感受到「媒介是空壳」——伤害数字来自蛊虫，不是武器
- [ ] 玩家感受到「杀招有代价」——释放后蛊虫休眠，真元大消耗
- [ ] 玩家感受到「配置即策略」——启用哪些蛊虫决定了攻击模式
- [ ] 散射有「弹幕覆盖」的爽快感，不是单发精准射击

### 8.3 代码验收

- [ ] `GuMediumWeapon.cs` 行数 < 200 行（只做查询和调度，不处理具体弹幕逻辑）
- [ ] `Shoot()` 方法第一行有注释：`// MVA 硬编码：P1 时抽取到 ShaZhaoRecipeSystem`
- [ ] 无硬编码魔法数字（配方数据提取为字段或结构体）
- [ ] 事件总线发布至少 1 个事件（GuActivated 已在 KongQiaoPlayer 中实现）

---

## 9. 风险与回退

| 风险 | 影响 | 回退方案 |
|------|------|---------|
| 弹幕角度计算错误导致全射向同一方向 | 体验问题 | 检查 `RotatedBy` 参数，确保 `spreadAngle` 和 `angleStep` 计算正确 |
| 杀招伤害过高（秒杀 Boss） | 平衡问题 | 降低 `DamageMultiplier`（从 3 降至 2），或增加 QiCost |
| 齐射时真元消耗过快导致空蓝 | 体验问题 | 降低平 A 消耗比例（从 10% 降至 5%），或增加 QiRegenRate |
| 媒介武器丢失（死亡掉落） | 进度阻断 | 媒介武器标记为「不可掉落」，死亡时保留在背包；或开局自动补发 |

---

## 10. 下一步（验证通过后）

1. 抽象 `ShaZhaoRecipe` 为通用数据结构，支持 JSON/配置加载
2. 扩展配方字典：从硬编码 1 条扩展到 5-10 条
3. 增加杀招冷却 UI 提示（蛊虫图标灰色 + 倒计时）
4. 增加弹幕追踪、穿透、爆裂等特效参数（由蛊虫数据驱动）
5. 接入 `DaoHenConflictSystem`（P2）：相斥道痕的蛊虫同时启用时，杀招失败或效果衰减

---

> 本文档由第三层实现工程师维护。  
> 实现过程中发现接口不匹配，立即回传 L2 登记修正，禁止私自修改 L2 冻结的数据结构。
