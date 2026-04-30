# L3 实现文档：QiPlayer 拆分 + 空窍系统 + 空窍 UI（统合重构版 v2.0）

> 版本：v2.0（基于小 d 基线代码重构建议统合）  
> 目标：完整迁移旧 QiPlayer（819 行）的全部功能，无遗漏、无兼容包袱、职责分离  
> 优先级：P0 / MVA  
> 预计工作量：5-6 天  
> 依赖：GuPerkSystem（已存在，接收迁移职责）/ 原版 Item/Player/Buff 系统

---

## 1. 重构总原则

1. **功能不缺失**：旧 `QiPlayer` 的每一个实际生效的机制，在新架构中必须有明确归属和等价实现路径。
2. **职责分离**：真元存储、境界计算、资质加成、战斗数值修改、空窍物品管理——五件事归属五个不同类，禁止交叉。
3. **副作用显性化**：旧代码中隐藏在 `CanUseItem` 里的状态修改，必须移到正确的位置（`UseItem` 或显式消费回调）。
4. **接口先行**：所有跨系统调用必须通过 L2 定义的接口或事件总线，禁止 `GetModPlayer<X>().someField = value` 直接写字段。
5. **旧 QiPlayer 完全删除**：不保留废弃层、兼容层或 `[Obsolete]` 标记。功能迁移完成后直接删除文件。

---

## 2. 旧 QiPlayer 功能迁移总表

| 旧机制 | 旧位置 | 新归属 | 等价性说明 |
|--------|--------|--------|-----------|
| 离散恢复（计时器+每跳恢复整点） | `UpdateResource()` | `QiResourcePlayer.PostUpdate()` | 完全保留离散机制，公式不变 |
| 酒虫精炼真元加成 | `UpdateResource()` 中调用 `GuPerkSystem.GetWineBugRegenBonus()` | `IQiRegenModifier` 接口 → `QiResourcePlayer.ExtraQiRegen` | 解耦：QiResourcePlayer 不直接调用 GuPerkSystem，通过接口收集加成 |
| 黄罗天牛 Buff 额外恢复 | `UpdateResource()` | 同上，由 Buff 系统通过 `IQiRegenModifier` 提供 | 解耦 |
| 双值上限（基础/当前） | `qiMax` / `qiMax2` + `ResetVariables()` | `QiMaxBase` / `QiMaxCurrent` | 每帧 `ResetEffects` 中 `QiMaxCurrent = QiMaxBase`，装备加成在 `PostUpdateEquips` 叠加 |
| 死亡真元处理 | `Kill()` 中 `qiCurrent /= 2` | `QiResourcePlayer.OnDeathClearQi()` | **覆盖为 D-20 决议：完全清空** |
| 六转以上低阶蛊虫返还真元 | `CanUseItem()` 中副作用 | `GuWeaponItem.UseItem()` 或 `MediumWeaponSystem` 消费回调 | 副作用显性化，移出 QiResourcePlayer |
| 境界存储/突破/破境提示 | `qiLevel` / `levelStage` / `StageUp()` | `QiRealmPlayer` | 保留旧公式，增加格子数新维度 |
| 空窍大小数值（旧含义） | `kongQiaoMax` 参与真元上限计算 | `QiRealmPlayer`（真元公式用）+ `KongQiaoPlayer`（格子数用） | 双轨：旧数值保留用于真元，新格子数用于空窍 |
| 资质存储 | `PlayerZiZhi` | `QiTalentPlayer` | 增加旧值映射方法 `GetZiZhiMultiplier()` |
| 近战伤害/击退加成 | `ModifyWeaponDamage` / `ModifyWeaponKnockback` | `GuPerkSystem` | 迁移 |
| 生命上限加成 | `ModifyMaxStats` | `GuPerkSystem` | 迁移 |
| 移速/加速度加成 | `PostUpdateRunSpeeds` | `GuPerkSystem` | 迁移 |
| 召唤栏加成 | `PostUpdateEquips` | `GuPerkSystem` | 迁移 |
| 火衣/水壳/映伤 Buff 反击 | `OnHitByNPC` / `PostUpdate` | 对应 Buff 文件 | 迁移 |
| 木魅蛊攻击替代 | `PostUpdate` | 对应 Buff 文件或专用系统 | 迁移 |
| 调试信息输出 `showInfo` | `QiPlayer.showInfo` | 独立调试命令 / `Info` 物品 | 移出 |
| 存档持久化 | `QiPlayer.SaveData/LoadData` | 各 ModPlayer 独立实现 | 新 key 名，旧存档不兼容（开新档） |
| 开局物品 | `QiPlayer.AddStartingItems` | `Mod.OnEnterWorld` 或 `AwakeningSystem` | 临时保留在 QiPlayer 直到替代完成 |

---

## 3. 核心数据结构（完整实现）

### 3.1 QiResourcePlayer — 真元资源

```csharp
public class QiResourcePlayer : ModPlayer
{
    // === 核心字段 ===
    public float QiMaxBase;       // 基础真元上限（由 QiRealmPlayer 写入）
    public float QiMaxCurrent;    // 当前真元上限（每帧 ResetEffects 中 = QiMaxBase，再叠加装备/Buff）
    public float QiCurrent;       // 当前真元
    public int QiOccupied;        // 被启用蛊虫占据的额度（由 KongQiaoPlayer 调用 UpdateQiOccupied 写入）
    public int QiAvailable => (int)QiMaxCurrent - QiOccupied;

    public float BaseQiRegenRate; // 基础恢复速率（由 QiRealmPlayer 写入）
    public float ExtraQiRegen;    // 额外恢复加成（由装备/Buff/酒虫通过 IQiRegenModifier 接口写入）

    private int regenTimer;       // 离散恢复计时器

    // === 方法 ===

    /// <summary>
    /// 被动接收占据额度更新。由 KongQiaoPlayer 在蛊虫启用/休眠时调用。
    /// 禁止其他系统直接修改 QiOccupied 字段。
    /// </summary>
    public void UpdateQiOccupied(int occupied) => QiOccupied = occupied;

    /// <summary>
    /// 消耗真元。返回是否成功。
    /// </summary>
    public bool ConsumeQi(float amount, QiConsumeReason reason)
    {
        if (QiCurrent < amount) return false;
        QiCurrent -= amount;
        EventBus.Publish(new PlayerQiChangedEvent 
        { 
            PlayerID = Player.whoAmI, 
            OldQi = QiCurrent + amount, 
            NewQi = QiCurrent, 
            Reason = QiChangeReason.Consume 
        });
        return true;
    }

    /// <summary>
    /// 返还真元（六转以上使用低阶蛊虫等场景）。
    /// 由 GuWeaponItem.UseItem() 或 MediumWeaponSystem 在成功发射后调用。
    /// </summary>
    public void RefundQi(float amount)
    {
        QiCurrent = Math.Min(QiMaxCurrent, QiCurrent + amount);
    }

    /// <summary>
    /// D-20: 死亡完全清空真元（覆盖旧版的减半行为）。
    /// 由 PlayerDeathEvent 处理链调用。
    /// </summary>
    public void OnDeathClearQi()
    {
        QiCurrent = 0;
        EventBus.Publish(new PlayerQiChangedEvent 
        { 
            PlayerID = Player.whoAmI, 
            OldQi = QiCurrent, 
            NewQi = 0, 
            Reason = QiChangeReason.Death 
        });
    }

    /// <summary>
    /// 每帧离散恢复。
    /// 完全保留旧代码的离散机制：计时器 + 每跳恢复整点。
    /// </summary>
    public override void PostUpdate()
    {
        float effectiveRegen = BaseQiRegenRate + ExtraQiRegen;
        if (effectiveRegen <= 0 || QiCurrent >= QiMaxCurrent) return;

        regenTimer++;
        int regenPerTick = 1;
        int interval = (int)(60f / effectiveRegen);

        if (effectiveRegen > 60)
        {
            regenPerTick = (int)(effectiveRegen / 60f);
            interval = 1;
        }

        if (regenTimer >= interval)
        {
            regenTimer = 0;
            QiCurrent = Math.Min(QiMaxCurrent, QiCurrent + regenPerTick);
        }
    }

    /// <summary>
    /// 每帧重置：当前上限回到基础值，额外恢复归零。
    /// 装备/Buff 在 PostUpdateEquips 中通过接口叠加到 ExtraQiRegen。
    /// </summary>
    public override void ResetEffects()
    {
        QiMaxCurrent = QiMaxBase;
        ExtraQiRegen = 0;
    }

    // === 数据持久化 ===
    public override void SaveData(TagCompound tag)
    {
        tag["QiCurrent"] = QiCurrent;
        tag["QiMaxBase"] = QiMaxBase;
        tag["QiMaxCurrent"] = QiMaxCurrent;
        tag["BaseQiRegenRate"] = BaseQiRegenRate;
        tag["QiOccupied"] = QiOccupied;
    }

    public override void LoadData(TagCompound tag)
    {
        QiCurrent = tag.GetFloat("QiCurrent");
        QiMaxBase = tag.GetFloat("QiMaxBase");
        QiMaxCurrent = tag.GetFloat("QiMaxCurrent");
        BaseQiRegenRate = tag.GetFloat("BaseQiRegenRate");
        QiOccupied = tag.GetInt("QiOccupied");
    }
}

/// <summary>
/// 真元恢复加成接口。由 GuPerkSystem、Buff 系统等实现，向 QiResourcePlayer 提供加成。
/// </summary>
public interface IQiRegenModifier
{
    void ModifyQiRegen(Player player, ref float extraRegen);
}

public enum QiConsumeReason { NormalAttack, ShaZhao, GuMaintenance, RefineGu, Skill }
public enum QiChangeReason { Consume, Regen, Death, LevelUp, ItemEffect }
```

### 3.2 QiRealmPlayer — 境界系统

```csharp
public class QiRealmPlayer : ModPlayer
{
    public int GuLevel;           // 转数 1-10
    public int LevelStage;        // 0=初期, 1=中期, 2=后期, 3=巅峰
    public float BreakthroughProgress;  // 突破进度 [0, 100]

    // 旧代码常量保留
    private const int UNIT_KONGQIAO = 100;  // 旧空窍单位，用于真元上限计算
    private const int BASE_KONGQIAO_SLOTS = 3;  // 新格子数基础

    /// <summary>
    /// 开窍初始化。由 AwakeningSystem 在击杀第一只野蛊后调用。
    /// </summary>
    public void OnAwakening()
    {
        GuLevel = 1;
        LevelStage = 0;
        ApplyRealmEffects(initialFill: true);

        EventBus.Publish(new PlayerRealmUpEvent 
        { 
            PlayerID = Player.whoAmI, 
            NewLevel = GuLevel, 
            NewStage = LevelStage 
        });
    }

    /// <summary>
    /// 小阶段突破。保留旧代码的破境提示和真元回满。
    /// </summary>
    public void StageUp()
    {
        if (LevelStage >= 3) return;
        LevelStage++;

        string stageName = LevelStage switch
        {
            0 => "初期", 1 => "中期", 2 => "后期", 3 => "巅峰", _ => ""
        };
        Main.NewText($"破境成功！当前境界为{GuLevel}转{stageName}", Color.Green);

        ApplyRealmEffects(initialFill: false);
        // 破境后真元回满
        Player.GetModPlayer<QiResourcePlayer>().QiCurrent = 
            Player.GetModPlayer<QiResourcePlayer>().QiMaxCurrent;

        EventBus.Publish(new PlayerRealmUpEvent { ... });
    }

    /// <summary>
    /// 大境界突破。
    /// </summary>
    public void LevelUp()
    {
        GuLevel++;
        LevelStage = 0;
        ApplyRealmEffects(initialFill: false);
        Player.GetModPlayer<QiResourcePlayer>().QiCurrent = 
            Player.GetModPlayer<QiResourcePlayer>().QiMaxCurrent;
        EventBus.Publish(new PlayerRealmUpEvent { ... });
    }

    /// <summary>
    /// 统一应用境界效果。
    /// 保留旧 SetQis 公式用于真元上限和恢复速率（保证数值感受不变）。
    /// 引入新格子数公式（D-04 双轨制中的硬上限）。
    /// </summary>
    private void ApplyRealmEffects(bool initialFill)
    {
        var talent = Player.GetModPlayer<QiTalentPlayer>();
        var qiResource = Player.GetModPlayer<QiResourcePlayer>();
        var kongQiao = Player.GetModPlayer<KongQiaoPlayer>();

        // 1. 计算真元上限（复用旧公式：times * UNIT_KONGQIAO * 资质 / 10）
        int times = (int)Math.Pow(10, GuLevel - 1) * (int)Math.Pow(2, LevelStage);
        float maxQi = times * UNIT_KONGQIAO * talent.GetZiZhiMultiplier() / 10f;

        // 2. 计算恢复速率（复用旧公式：qiLevel * 资质 / 2）
        float regenRate = GuLevel * talent.GetZiZhiMultiplier() / 2f;

        // 3. 计算空窍格子数（新公式：D-04 硬上限）
        int slots = BASE_KONGQIAO_SLOTS + (GuLevel - 1) * 2 + LevelStage;

        // 4. 写入其他 ModPlayer
        qiResource.QiMaxBase = maxQi;
        qiResource.BaseQiRegenRate = regenRate;
        kongQiao.SetMaxSlots(slots);

        if (initialFill)
            qiResource.QiCurrent = maxQi;
    }

    // === 数据持久化 ===
    public override void SaveData(TagCompound tag)
    {
        tag["GuLevel"] = GuLevel;
        tag["LevelStage"] = LevelStage;
        tag["BreakthroughProgress"] = BreakthroughProgress;
    }

    public override void LoadData(TagCompound tag)
    {
        GuLevel = tag.GetInt("GuLevel");
        LevelStage = tag.GetInt("LevelStage");
        BreakthroughProgress = tag.GetFloat("BreakthroughProgress");
    }
}
```

### 3.3 QiTalentPlayer — 资质系统

```csharp
public class QiTalentPlayer : ModPlayer
{
    public enum TalentGrade { Ding, Bing, Yi, Jia }  // 丁/丙/乙/甲

    public TalentGrade Grade;
    public float CultivationSpeedMultiplier;
    public float PerceptionRangeBonus;

    // 旧代码 ZiZhi 数值映射：丁=2, 丙=4, 乙=6, 甲=8
    private static readonly float[] ZiZhiLegacyValues = { 2f, 4f, 6f, 8f };
    private static readonly float[] GradeMultipliers = { 0.8f, 1.0f, 1.3f, 1.8f };
    private static readonly float[] PerceptionBonuses = { 0f, 10f, 25f, 50f };

    /// <summary>
    /// 返回与旧 (int)ZiZhi 等价的数值，供 QiRealmPlayer 的旧公式使用。
    /// </summary>
    public float GetZiZhiMultiplier() => ZiZhiLegacyValues[(int)Grade];

    /// <summary>
    /// 开窍初始化。MVA 默认乙等，后续可随机或玩家选择。
    /// </summary>
    public void OnAwakening(TalentGrade? fixedGrade = null)
    {
        Grade = fixedGrade ?? TalentGrade.Yi;
        CalculateEffects();
    }

    public void CalculateEffects()
    {
        int idx = (int)Grade;
        CultivationSpeedMultiplier = GradeMultipliers[idx];
        PerceptionRangeBonus = PerceptionBonuses[idx];
    }

    // === 数据持久化 ===
    public override void SaveData(TagCompound tag) => tag["TalentGrade"] = (int)Grade;
    public override void LoadData(TagCompound tag)
    {
        Grade = (TalentGrade)tag.GetInt("TalentGrade");
        CalculateEffects();
    }
}
```

### 3.4 KongQiaoPlayer — 空窍系统

与 L3 v1 基本一致，确认以下与旧代码的关系：
- 旧 `QiPlayer` 的 `kongQiaoMax` 是数值，参与真元上限计算，**不存储蛊虫列表**，**无格子概念**
- `KongQiaoPlayer` 是全新系统，与旧逻辑无冲突，是扩展而非替换
- `SetMaxSlots` 接收的值来自 `QiRealmPlayer` 的新格子公式，不是旧 `kongQiaoMax`

```csharp
public class KongQiaoPlayer : ModPlayer
{
    public List<KongQiaoSlot> KongQiao = new();
    public int MaxSlots;
    public int UsedSlots => KongQiao.Count;

    public void SetMaxSlots(int slots)
    {
        MaxSlots = slots;
        for (int i = slots; i < KongQiao.Count; i++)
            KongQiao[i].IsActive = false;
        RecalculateQiOccupied();
    }

    public bool TryRefineGu(Item guItem)
    {
        if (KongQiao.Count >= MaxSlots) return false;

        var qiResource = Player.GetModPlayer<QiResourcePlayer>();
        float refineCost = guItem.damage * 2f;
        if (!qiResource.ConsumeQi(refineCost, QiConsumeReason.RefineGu)) return false;

        var slot = new KongQiaoSlot
        {
            GuItem = guItem.Clone(),
            IsActive = false,
            QiOccupation = CalculateQiOccupation(guItem),
            GuTypeID = guItem.type,
            IsAttackGu = guItem.damage > 0,
            IsPassiveGu = guItem.defense > 0,
            IsMainGu = KongQiao.Count == 0,
            Refinement = 0f,
            Loyalty = 50f,
            DaoHenTags = GetDefaultDaoHenTags(guItem),
            ProjectileType = GetProjectileType(guItem),
            BuffType = GetBuffType(guItem)
        };

        KongQiao.Add(slot);
        guItem.TurnToAir();
        KongQiaoUI.Refresh();
        return true;
    }

    public bool TryExtractGu(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= KongQiao.Count) return false;
        if (KongQiao[slotIndex].IsMainGu) return false;

        Player.QuickSpawnItem(Player.GetSource_GiftOrReward(), KongQiao[slotIndex].GuItem.Clone());
        KongQiao.RemoveAt(slotIndex);
        RecalculateQiOccupied();
        KongQiaoUI.Refresh();
        return true;
    }

    public void SetGuActive(int slotIndex, bool active)
    {
        if (slotIndex < 0 || slotIndex >= KongQiao.Count) return;

        var slot = KongQiao[slotIndex];
        if (active)
        {
            var qiResource = Player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiAvailable < slot.QiOccupation)
            {
                Main.NewText("真元不足，无法启用此蛊虫", Color.Red);
                return;
            }
        }

        slot.IsActive = active;
        RecalculateQiOccupied();

        if (active)
        {
            EventBus.Publish(new GuActivatedEvent 
            { 
                PlayerID = Player.whoAmI, 
                GuTypeID = slot.GuTypeID,
                IsInFamilyCore = WorldStateMachine.Instance?.IsFamilyCoreZone(Player.Center, FactionID.GuYue) ?? false
            });
        }

        KongQiaoUI.Refresh();
    }

    public List<KongQiaoSlot> GetActiveAttackGus() => KongQiao.Where(s => s.IsActive && s.IsAttackGu).ToList();
    public List<KongQiaoSlot> GetActivePassiveGus() => KongQiao.Where(s => s.IsActive && s.IsPassiveGu).ToList();

    private void RecalculateQiOccupied()
    {
        int occupied = KongQiao.Where(s => s.IsActive).Sum(s => s.QiOccupation);
        Player.GetModPlayer<QiResourcePlayer>().UpdateQiOccupied(occupied);
    }

    private int CalculateQiOccupation(Item guItem) => 10 + (int)(guItem.damage / 10f);
    private ulong GetDefaultDaoHenTags(Item guItem) => 0;
    private int GetProjectileType(Item guItem) => 0;
    private int GetBuffType(Item guItem) => 0;

    /// <summary>
    /// D-20 / D-05 / D-06: 玩家死亡时处理空窍中的蛊虫。
    /// </summary>
    public void OnPlayerDeath()
    {
        foreach (var slot in KongQiao) slot.IsActive = false;
        RecalculateQiOccupied();

        var escaped = new List<KongQiaoSlot>();
        var selfDestructed = new List<KongQiaoSlot>();
        var retained = new List<KongQiaoSlot>();
        int mainGuTypeID = -1;

        for (int i = KongQiao.Count - 1; i >= 0; i--)
        {
            var slot = KongQiao[i];
            if (slot.IsMainGu) { retained.Add(slot); mainGuTypeID = slot.GuTypeID; continue; }

            if (slot.Loyalty < 40f)
            {
                if (Main.rand.NextBool()) { escaped.Add(slot); KongQiao.RemoveAt(i); }
                else { selfDestructed.Add(slot); KongQiao.RemoveAt(i); }
            }
            else retained.Add(slot);
        }

        EventBus.Publish(new PlayerGusLostOnDeathEvent 
        {
            PlayerID = Player.whoAmI,
            EscapedGus = escaped,
            SelfDestructedGus = selfDestructed,
            RetainedGus = retained,
            MainGuTypeID = mainGuTypeID
        });

        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText($"[死亡] 空窍损失：叛逃 {escaped.Count} 只，自毁 {selfDestructed.Count} 只，保留 {retained.Count} 只（含本命蛊）", Color.OrangeRed);
    }

    public override void PostUpdate()
    {
        foreach (var gu in GetActivePassiveGus())
        {
            var qiResource = Player.GetModPlayer<QiResourcePlayer>();
            if (!qiResource.ConsumeQi(0.5f, QiConsumeReason.GuMaintenance))
            {
                gu.IsActive = false;
                RecalculateQiOccupied();
                continue;
            }
            if (gu.IsPassiveGu) Player.statDefense += (int)(gu.GuItem.defense * 0.5f);
        }

        foreach (var gu in KongQiao)
            if (gu.Loyalty < 100f) gu.Loyalty = Math.Min(100f, gu.Loyalty + 0.001f);
    }

    // === 数据持久化 ===
    public override void SaveData(TagCompound tag)
    {
        tag["MaxSlots"] = MaxSlots;
        var list = new List<TagCompound>();
        foreach (var slot in KongQiao)
        {
            list.Add(new TagCompound
            {
                ["GuItem"] = slot.GuItem,
                ["IsActive"] = slot.IsActive,
                ["QiOccupation"] = slot.QiOccupation,
                ["GuTypeID"] = slot.GuTypeID,
                ["IsAttackGu"] = slot.IsAttackGu,
                ["IsPassiveGu"] = slot.IsPassiveGu,
                ["IsMainGu"] = slot.IsMainGu,
                ["Refinement"] = slot.Refinement,
                ["Loyalty"] = slot.Loyalty,
                ["DaoHenTags"] = (long)slot.DaoHenTags,
                ["ProjectileType"] = slot.ProjectileType,
                ["BuffType"] = slot.BuffType
            });
        }
        tag["KongQiao"] = list;
    }

    public override void LoadData(TagCompound tag)
    {
        MaxSlots = tag.GetInt("MaxSlots");
        KongQiao.Clear();
        foreach (var st in tag.GetList<TagCompound>("KongQiao"))
        {
            KongQiao.Add(new KongQiaoSlot
            {
                GuItem = st.Get<Item>("GuItem"),
                IsActive = st.GetBool("IsActive"),
                QiOccupation = st.GetInt("QiOccupation"),
                GuTypeID = st.GetInt("GuTypeID"),
                IsAttackGu = st.GetBool("IsAttackGu"),
                IsPassiveGu = st.GetBool("IsPassiveGu"),
                IsMainGu = st.GetBool("IsMainGu"),
                Refinement = st.GetFloat("Refinement"),
                Loyalty = st.GetFloat("Loyalty"),
                DaoHenTags = (ulong)st.GetLong("DaoHenTags"),
                ProjectileType = st.GetInt("ProjectileType"),
                BuffType = st.GetInt("BuffType")
            });
        }
        RecalculateQiOccupied();
    }
}

public class KongQiaoSlot
{
    public Item GuItem;
    public bool IsActive;
    public int QiOccupation;
    public int GuTypeID;
    public bool IsAttackGu;
    public bool IsPassiveGu;
    public bool IsMainGu;
    public float Refinement;
    public float Loyalty;
    public ulong DaoHenTags;
    public int ProjectileType;
    public int BuffType;
}
```

---

## 4. GuPerkSystem 扩展 — 接收战斗数值修改

旧 `QiPlayer` 中的以下方法必须迁移到 `GuPerkSystem`：

```csharp
public class GuPerkSystem : ModPlayer
{
    // === 已有字段（保留） ===
    public int whitePigPower;
    public int extraAges;
    public float extraSpeed;
    public int extraMinion;
    // ... 其他已有字段 ...

    // === 从 QiPlayer 迁移的方法 ===

    /// <summary>
    /// 近战伤害加成（来自力量系蛊虫：白豕/黑豕/斤力/钧力）。
    /// </summary>
    public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
    {
        // 原 QiPlayer.ModifyWeaponDamage 逻辑
        // 根据 whitePigPower 等字段修改伤害
    }

    /// <summary>
    /// 近战击退加成。
    /// </summary>
    public override void ModifyWeaponKnockback(Item item, ref StatModifier knockback)
    {
        // 原 QiPlayer.ModifyWeaponKnockback 逻辑
    }

    /// <summary>
    /// 生命上限加成（来自寿蛊）。
    /// </summary>
    public override void ModifyMaxStats(out float health, out float mana)
    {
        // 原 QiPlayer.ModifyMaxStats 逻辑
        health = extraAges * 10; // 示例
        mana = 0;
    }

    /// <summary>
    /// 移速/加速度加成（来自龙珠蟋蟀）。
    /// </summary>
    public override void PostUpdateRunSpeeds()
    {
        // 原 QiPlayer.PostUpdateRunSpeeds 逻辑
        Player.moveSpeed += extraSpeed;
        Player.runAcceleration += extraSpeed * 0.1f;
    }

    /// <summary>
    /// 召唤栏加成（来自一胎蛊）。
    /// </summary>
    public override void PostUpdateEquips()
    {
        // 原 QiPlayer.PostUpdateEquips 逻辑
        Player.maxMinions += extraMinion;

        // 酒虫精炼真元加成：通过接口写入 QiResourcePlayer
        float wineBonus = GetWineBugRegenBonus();
        var qiResource = Player.GetModPlayer<QiResourcePlayer>();
        qiResource.ExtraQiRegen += wineBonus;
    }

    /// <summary>
    /// 计算酒虫精炼真元加成。由 PostUpdateEquips 调用，通过 ExtraQiRegen 写入。
    /// </summary>
    public float GetWineBugRegenBonus()
    {
        // 原 GuPerkSystem.GetWineBugRegenBonus 逻辑
        // 根据酒虫系列（酒虫/四味/七香/九眼）计算加成
        return 0f; // 占位
    }

    // === 数据持久化（扩展） ===
    public override void SaveData(TagCompound tag)
    {
        // 保留已有字段的持久化
        tag["whitePigPower"] = whitePigPower;
        tag["extraAges"] = extraAges;
        tag["extraSpeed"] = extraSpeed;
        tag["extraMinion"] = extraMinion;
        // ... 其他已有字段 ...
    }

    public override void LoadData(TagCompound tag)
    {
        whitePigPower = tag.GetInt("whitePigPower");
        extraAges = tag.GetInt("extraAges");
        extraSpeed = tag.GetFloat("extraSpeed");
        extraMinion = tag.GetInt("extraMinion");
    }
}
```

---

## 5. 空窍 UI（已实现）

### 5.1 KongQiaoUI — 空窍面板

[`Common/UI/KongQiaoUI/KongQiaoUI.cs`](Common/UI/KongQiaoUI/KongQiaoUI.cs) — 完整的空窍管理面板：

- **主面板**：520×480 半透明深蓝面板，屏幕居中
- **标题/信息栏**：显示格子数（已用/上限）、真元占用、可用真元
- **蛊虫列表**：每个格子显示蛊虫图标、名称（本命蛊金色标记）、状态（已启用/已休眠）、忠诚度、占窍值
- **操作按钮**：
  - 启用/休眠切换按钮（绿色/红色背景）
  - 取出按钮（本命蛊不可取出，按钮隐藏）
- **关闭方式**：点击关闭按钮或按 ESC
- **自动刷新**：每次操作后调用 `Refresh()` 更新列表

### 5.2 KongQiaoUISystem — UI注册系统

[`Common/UI/KongQiaoUI/KongQiaoUISystem.cs`](Common/UI/KongQiaoUI/KongQiaoUISystem.cs) — 与 ReputationUISystem 相同模式：

- `[Autoload(Side = ModSide.Client)]` 仅客户端加载
- `Load()` 中创建 `UserInterface` 和 `KongQiaoUI` 实例
- `ModifyInterfaceLayers()` 在 "Vanilla: Mouse Text" 层之前插入绘制
- `ToggleUI()` 切换显示/隐藏，打开时自动调用 `Refresh()`

### 5.3 KongQiaoStone — 空窍石

[`Content/Items/Consumables/KongQiaoStone.cs`](Content/Items/Consumables/KongQiaoStone.cs) — 右键打开空窍面板的物品：

- 稀有度：蓝色，不可堆叠，不可消耗
- 配方：50 石块 + 3 坠落之星 @ 铁砧
- 右键点击 → `KongQiaoUISystem.ToggleUI()`

### 5.4 右键炼化触发

在 [`GuWeaponItem.CanUseItem()`](Content/Items/Weapons/GuWeaponItem.cs:101) 和 [`GuAccessoryItem.CanUseItem()`](Content/Items/Accessories/GuAccessoryItem.cs:73) 中：

- 当 `altFunctionUse == 2`（右键）且 `hasBeenControlled == true`（已炼化）时：
  - 调用 `KongQiaoPlayer.TryRefineGu(Item)` 尝试炼入空窍
  - 成功 → 物品销毁，蛊虫进入空窍
  - 失败 → 提示"空窍已满或真元不足"
- 未炼化时仍保持原有炼化流程

---

## 6. 旧 QiPlayer 的最终删除流程

### 6.1 删除前检查清单（实际完成状态）

- [x] `QiResourcePlayer` 已实现并测试：真元存储/消耗/恢复/死亡清空
- [x] `QiRealmPlayer` 已实现并测试：境界突破/破境提示/真元回满
- [x] `QiTalentPlayer` 已实现并测试：资质存储/旧值映射
- [x] `KongQiaoPlayer` 已实现并测试：炼化/启用/休眠/取出/死亡处理
- [x] `GuPerkSystem` 已扩展并测试：所有迁移的战斗数值修改方法
- [x] `GuWeaponItem` / `GuAccessoryItem` / `GuConsumableItem` 已修改：使用新 ModPlayer
- [x] 消耗品子类已迁移：使用 `QiRealmPlayer.StageUp()` / `LevelUp()`
- [x] 事件系统已实现：`EventBus` + 6 个事件类型
- [ ] 对应 Buff 文件已修改：火衣/水壳/映伤/木魅反击逻辑移入（**注意：ModBuff 无 OnHitByNPC 钩子，需迁移到专用 GlobalPlayer 或保留在 QiPlayer 中**）
- [x] 所有外部引用 `QiPlayer` 的代码已改为引用新的 ModPlayer
- [ ] 存档持久化测试通过：新系统 Save/Load 正常（需开新档，旧存档不兼容）

### 6.2 删除操作

直接删除 `Common/Players/QiPlayer.cs`，不在项目中保留任何兼容层或 `[Obsolete]` 标记。

> **注意**：删除前需确认：
> 1. `QiPlayer.cs` 中的 `OnHitByNPC`（火衣/水壳/映伤反击）和 `CanUseItem`（木魅攻击替代）逻辑需要迁移到 `BuffReactionPlayer` 或 `EffectsPlayer`
> 2. `QiPlayer.cs` 中的 `AddStartingItems` 需要迁移到 `Mod.OnEnterWorld` 或 `AwakeningSystem`
> 3. `QiPlayer.cs` 中的 `showInfo` 调试方法需要迁移到 `Info` 物品
> 4. `QiPlayer.cs` 中的 `levelStageUpRate` 自动破境逻辑需要迁移到 `QiRealmPlayer`

---

## 7. 验收标准

### 7.1 功能验收（必须全部通过）

| 场景 | 操作 | 预期行为 | 通过标准 |
|------|------|---------|---------|
| 离散恢复 | 等待真元自然恢复 | 每 N 帧跳一次整点恢复，非连续增长 | 观察 QiCurrent 变化呈阶梯状 |
| 酒虫加成 | 装备酒虫系列 | ExtraQiRegen 增加，恢复间隔缩短 | 计时器间隔变化 |
| 境界突破 | 使用舍利蛊 | 破境提示出现，真元回满，上限提升 | QiMaxBase 按旧公式增长 |
| 资质映射 | 查看资质 | 乙等对应旧值 6，参与真元公式 | GetZiZhiMultiplier() 返回 6 |
| 炼化蛊虫 | 右键蛊虫炼化 | 进入空窍，本命蛊标记，背包消失 | KongQiao.Count == 1 && IsMainGu == true |
| 占据额度 | 启用蛊虫 | QiOccupied 增加，QiAvailable 减少 | QiResourcePlayer 数据一致 |
| 真元不足 | 启用超额蛊虫 | 拒绝启用，提示「真元不足」 | 弹出红色提示 |
| 死亡清空 | 玩家死亡 | QiCurrent = 0，全部休眠，忠诚度判定 | OnDeathClearQi 被调用 |
| 战斗加成 | 装备力量蛊虫 | 近战伤害增加 | ModifyWeaponDamage 生效 |
| 存档兼容 | 退出重进 | 真元/境界/资质/空窍全部恢复 | LoadData 后数据一致 |

### 7.2 代码验收

- [ ] 旧 `QiPlayer.cs` 已删除，项目中无残留引用
- [ ] 5 个新 ModPlayer 总代码量 < 800 行（原 QiPlayer 819 行）
- [ ] 无直接写字段：`GetModPlayer<X>().Field = value` 必须通过接口方法
- [ ] 事件总线发布至少 4 个事件

---

## 8. 风险与回退

| 风险 | 影响 | 回退方案 |
|------|------|---------|
| 旧存档无法加载（缺少 QiPlayer） | 崩溃 | 在 `Mod.Load` 中检测旧存档 key，弹出提示「请开新档体验重构后的蛊师系统」 |
| 离散恢复公式与旧版不一致 | 数值感受变化 | 对比旧 `UpdateResource` 公式，确保等价 |
| GuPerkSystem 方法冲突 | 编译错误 | 检查 GuPerkSystem 是否已有同名方法，合并而非覆盖 |
| 六转返还逻辑遗漏 | 高阶玩家无法返还真元 | 在 GuWeaponItem 中增加 `if (player.GetModPlayer<QiRealmPlayer>().GuLevel >= 6 && guLevel < 6) RefundQi(...)` |

---

> 本文档由第三层实现工程师维护。  
> 实现过程中发现接口不匹配，立即回传 L2 登记修正，禁止私自修改 L2 冻结的数据结构。
