using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common;
using VerminLordMod.Common.Abstractions;
using VerminLordMod.Common.Players;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Accessories
{
    /// <summary>
    /// 被动蛊基类 — 防御蛊/辅助蛊/移动蛊的统一基类。
    /// 继承 GuBaseItem，统一炼化逻辑，空窍中激活后自动生效。
    /// 不占用饰品栏。
    ///
    /// 子类重写：
    ///   _guLevel, qiCost, controlQiCost, unitConntrolRate
    ///   DefenseBonus, DamageReduction, BuffType, MoveSpeedBonus
    ///   WingCanFly, WingFlightTimeMax (移动蛊)
    /// </summary>
    public abstract class PassiveGuItem : GuBaseItem
    {
        // ===== 出装路径 =====
        public override EquipSlots EquipSlot => EquipSlots.KongQiao;

        // ===== 效果字段（子类重写，空窍中激活后生效）=====

        /// <summary>提供的防御力加成</summary>
        public virtual int DefenseBonus => Item.defense;
        /// <summary>伤害减免比例 [0, 1]</summary>
        public virtual float DamageReduction => 0f;
        /// <summary>施加的Buff类型ID（0=无）</summary>
        public virtual int BuffType => 0;
        /// <summary>移速加成</summary>
        public virtual float MoveSpeedBonus => 0f;
        /// <summary>生命上限加成</summary>
        public virtual int LifeBonus => 0;
        /// <summary>是否可飞行（移动蛊）</summary>
        public virtual bool WingCanFly => false;
        /// <summary>飞行时间上限（帧）</summary>
        public virtual int WingFlightTimeMax => 0;
        /// <summary>飞行上升速度</summary>
        public virtual float WingAscentSpeed => 0f;

        // ===== 本地化 =====
        public new static LocalizedText UsesXQiText { get; private set; }
        public new static LocalizedText ControlRateText { get; private set; }
        public new static LocalizedText GuLevelText { get; private set; }

        public override void SetStaticDefaults()
        {
            UsesXQiText = this.GetLocalization("UsesXQi");
            ControlRateText = this.GetLocalization("ControlRate");
            GuLevelText = this.GetLocalization("GuLevel");
            base.SetStaticDefaults();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
            tooltips.Insert(3, new TooltipLine(Mod, "GuLevel", GuLevelText.Format(_guLevel)));

            if (controlRate > 0f)
                tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRateText.Format(controlRate)));
            else
                tooltips.Add(new TooltipLine(Mod, "ControlRate", "右键使用开始炼化"));

            // 显示效果信息
            if (DefenseBonus > 0)
                tooltips.Add(new TooltipLine(Mod, "DefenseBonus", $"防御加成：+{DefenseBonus}"));
            if (DamageReduction > 0)
                tooltips.Add(new TooltipLine(Mod, "DamageReduction", $"伤害减免：{DamageReduction * 100}%"));
            if (BuffType > 0)
                tooltips.Add(new TooltipLine(Mod, "BuffInfo", $"增益效果"));
            if (MoveSpeedBonus > 0)
                tooltips.Add(new TooltipLine(Mod, "MoveSpeed", $"移速加成：+{MoveSpeedBonus * 100}%"));
            if (LifeBonus > 0)
                tooltips.Add(new TooltipLine(Mod, "LifeBonus", $"生命上限：+{LifeBonus}"));
            if (WingCanFly)
                tooltips.Add(new TooltipLine(Mod, "WingInfo", $"飞行能力（{WingFlightTimeMax / 60}秒）"));

            tooltips.Add(new TooltipLine(Mod, "KongQiaoHint", "[c/88CCFF:炼入空窍后激活生效，不占用饰品栏]"));
        }

        // ===== 禁止放入饰品栏 =====

        public override bool CanEquipAccessory(Player player, int slot, bool modded)
        {
            Text.ShowTextRed(player, "蛊虫需炼入空窍方可生效，不能直接装备到饰品栏");
            return false;
        }
    }
}
