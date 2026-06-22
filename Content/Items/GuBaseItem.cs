using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Abstractions;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items
{
    /// <summary>
    /// 蛊虫统一基类 — 所有蛊虫物品的最终基类。
    ///
    /// 核心职责：
    /// 1. 统一炼化逻辑（原 GuWeaponItem / GuAccessoryItem / PassiveGuItem 三份拷贝收敛于此）
    /// 2. IGu 接口真实实现（不再硬编码死值）
    /// 3. 出装路径（EquipSlot）与转数/仙蛊标记（GuTier / IsXianGu）
    ///
    /// 子类重写：
    ///   qiCost, _guLevel, _useTime, _useStyle, controlQiCost,
    ///   unitConntrolRate, uncontrolRate, moddustType, needCtrl
    ///   Element, DaoHenTags (IGu 真实语义)
    ///   以及 OnActiveTick / CanUseItem / Shoot 等行为方法
    ///
    /// 注：为保证已迁移的 ~94 个子类**不改一行代码**即能编译通过，
    /// 虚成员命名与 GuWeaponItem / GuAccessoryItem 原有命名完全一致
    /// （小写下划线风格如 _guLevel、qiCost 等）。
    /// 新增的 GuTier / IsXianGu / Element 等为纯新增，不影响现有子类。
    /// </summary>
    public abstract class GuBaseItem : ModItem, IGu
    {
        // ===== IGu 显式实现（真实语义，不再硬编码）=====
        int IGu.GuLevel => _guLevel;
        float IGu.QiCost => qiCost;
        float IGu.ControlQiCost => controlQiCost;
        float IGu.ControlRate { get => controlRate; set => controlRate = value; }
        bool IGu.IsControlled => hasBeenControlled;
        float IGu.Loyalty { get => loyalty; set => loyalty = value; }
        GuCategory IGu.Category => category;
        GuElement IGu.Element => Element;
        ulong IGu.DaoHenTags => DaoHenTags;

        // ===== 分类/元素/道痕（子类可重写以提供真实语义）=====

        /// <summary>蛊虫分类（攻击/防御/辅助/功能/特殊），默认由出装路径决定</summary>
        protected virtual GuCategory DefaultCategory => EquipSlot switch
        {
            EquipSlots.Weapon => GuCategory.Attack,
            EquipSlots.KongQiao => GuCategory.Defense,
            _ => GuCategory.Support,
        };
        private GuCategory category;
        /// <summary>子类可重写以覆盖默认分类</summary>
        protected virtual GuCategory CategoryOverride => GuCategory.None;
        private GuCategory Category => CategoryOverride != GuCategory.None ? CategoryOverride : DefaultCategory;

        /// <summary>蛊虫元素属性，子类可重写</summary>
        protected virtual GuElement Element => GuElement.None;

        /// <summary>道痕标签位掩码，子类可重写</summary>
        protected virtual ulong DaoHenTags => 0;

        /// <summary>忠诚度 [0, 100]</summary>
        protected float loyalty = 50f;

        // ===== 转数/仙蛊标记系统（替代"目录名+_guLevel+类名后缀"三重约定） =====

        /// <summary>转数等级枚举</summary>
        public enum GuTier
        {
            /// <summary>零转 — 凡蛊入门</summary>
            Zero = 0,
            /// <summary>一转</summary>
            One = 1,
            /// <summary>二转</summary>
            Two = 2,
            /// <summary>三转</summary>
            Three = 3,
            /// <summary>四转</summary>
            Four = 4,
            /// <summary>五转</summary>
            Five = 5,
            /// <summary>六转（升仙前）</summary>
            Six = 6,
            /// <summary>七转（仙蛊）</summary>
            Seven = 7,
            /// <summary>八转</summary>
            Eight = 8,
            /// <summary>九转</summary>
            Nine = 9,
            /// <summary>仙蛊（Tier 7-9 的统一标记, 与具体数值无关）</summary>
            XianGu = 10,
        }

        /// <summary>
        /// 子类重写以标记蛊虫等级。
        /// 默认从 _guLevel 自动推算（兼容老代码）。
        /// </summary>
        public virtual GuTier Tier => _guLevel >= (int)GuTier.Seven ? GuTier.XianGu : (GuTier)_guLevel;

        /// <summary>是否为仙蛊（便捷属性）</summary>
        public bool IsXianGu => Tier == GuTier.XianGu;

        // ===== 出装路径 =====

        /// <summary>
        /// 出装方式枚举。
        /// Weapon=左键武器栏使用; KongQiao=空窍中激活;
        /// Accessory=兼容旧饰品栏路径。
        /// </summary>
        public enum EquipSlots { Weapon, KongQiao, Accessory }

        /// <summary>
        /// 子类固定返回自己的出装方式。
        /// GuWeaponItem → Weapon; PassiveGuItem → KongQiao; GuAccessoryItem → Accessory
        /// 默认 Weapon（兼容直接继承 GuBaseItem 的历史子类）。
        /// </summary>
        public virtual EquipSlots EquipSlot => EquipSlots.Weapon;

        // ===== 以下虚成员与 GuWeaponItem / GuAccessoryItem 保持命名一致，供子类重写 =====

        /// <summary>是否需要炼化</summary>
        protected virtual bool needCtrl => true;

        /// <summary>使用时消耗的真元</summary>
        protected virtual int qiCost => 7;

        /// <summary>炼化时消耗的真元</summary>
        protected virtual int controlQiCost => 10;

        /// <summary>一次炼化增加的炼化进度</summary>
        protected virtual float unitConntrolRate => 10;

        /// <summary>是否已经被炼化</summary>
        public bool hasBeenControlled = false;

        /// <summary>当前炼化进度 [0, 100]</summary>
        public float controlRate = 0f;

        /// <summary>蛊虫脱离炼化的速度（每帧减少量）</summary>
        protected virtual float uncontrolRate => 0.01f;

        /// <summary>左键使用速度（帧）</summary>
        protected virtual int _useTime => 20;

        /// <summary>左键使用动画</summary>
        protected virtual int _useStyle => ItemUseStyleID.Guitar;

        /// <summary>转数</summary>
        protected virtual int _guLevel => 1;

        /// <summary>虚影类型（-1 表示无虚影）</summary>
        protected virtual int moddustType => -1;

        // ===== 本地化 =====

        public static LocalizedText UsesXQiText { get; private set; }
        public static LocalizedText ControlRateText { get; private set; }
        public static LocalizedText GuLevelText { get; private set; }

        public override void SetStaticDefaults()
        {
            UsesXQiText = this.GetLocalization("UsesXQi");
            ControlRateText = this.GetLocalization("ControlRate");
            GuLevelText = this.GetLocalization("GuLevel");

            // 只在初始加载时设置一次 category
            if (CategoryOverride != GuCategory.None)
                category = CategoryOverride;
            else
                category = DefaultCategory;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
            tooltips.Insert(3, new TooltipLine(Mod, "GuLevel", GuLevelText.Format(_guLevel)));

            if (controlRate > 0f)
                tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRateText.Format(controlRate)));
            else if (needCtrl)
                tooltips.Add(new TooltipLine(Mod, "ControlRate", "右键使用开始炼化"));

            // 仙蛊特殊标记
            if (IsXianGu)
            {
                tooltips.Add(new TooltipLine(Mod, "XianGuTag", "[c/FFD700:✦ 仙蛊 ✦]"));
            }
        }

        // ===== 统一炼化系统 =====

        public override bool AltFunctionUse(Player player) => true;

        /// <summary>
        /// 子类可重写此方法提供左键行为（武器蛊发射弹幕等）。
        /// 返回 true 表示允许使用，false 表示阻止使用，null 表示使用默认行为。
        /// </summary>
        protected virtual bool? OnLeftClick(Player player) => null;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 5;
                Item.useAnimation = 5;
                Item.useStyle = ItemUseStyleID.HoldUp;

                // 已炼化 → 尝试炼入空窍
                if (hasBeenControlled)
                {
                    var kongQiao = player.GetModPlayer<KongQiaoPlayer>();
                    if (kongQiao.TryRefineGu(Item))
                        return false; // 炼化成功，物品已销毁
                    Text.ShowTextRed(player, "空窍已满或真元不足，无法炼入空窍");
                    return false;
                }

                var qiResource = player.GetModPlayer<QiResourcePlayer>();
                if (qiResource.QiCurrent < controlQiCost)
                {
                    Text.ShowTextRed(player, "炼化失败 真元不足");
                    return false;
                }
                qiResource.ConsumeQi(controlQiCost);
                controlRate += unitConntrolRate;
                Text.ShowTextRed(player, $"炼化中......当前进度{controlRate}%");
                return false;
            }
            else
            {
                // 左键：子类自定义行为（null 表示默认允许使用）
                return OnLeftClick(player) ?? true;
            }
        }

        public override void UpdateInventory(Player player)
        {
            if (controlRate >= 100)
                hasBeenControlled = true;

            if (hasBeenControlled)
                return;

            controlRate -= uncontrolRate;
            controlRate = Utils.Clamp(controlRate, 0, 100);
        }

        // ===== 虚影（视觉效果） =====

        protected bool hasShownGhost = false;

        public override void HoldItem(Player player)
        {
            if (!hasShownGhost && moddustType != -1 && player.inventory[58].type != Item.type)
            {
                Vector2 position = player.Center + new Vector2(0, -player.height / 2);
                Dust dust = Dust.NewDustPerfect(position, moddustType);
                dust.velocity = new Vector2(0, -2);
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
                dust.scale = 0.5f;
                dust.position += new Vector2(-dust.frame.Width / 2 * dust.scale, -dust.frame.Height / 2 * dust.scale);
                hasShownGhost = true;
            }
        }

        // ===== 行为钩子（子类重写，供武器蛊 / 饰品蛊各自实现） =====

        /// <summary>
        /// 每帧当物品在玩家背包/饰品栏/空窍中调用。
        /// 饰品蛊在此叠加效果。
        /// </summary>
        public virtual void OnActiveTick(Player player) { }

        // ===== 持久化 =====

        public override void SaveData(TagCompound tag)
        {
            tag["controlRate"] = controlRate;
            tag["hasBeenControlled"] = hasBeenControlled;
            tag["loyalty"] = loyalty;
        }

        public override void LoadData(TagCompound tag)
        {
            controlRate = tag.GetFloat("controlRate");
            hasBeenControlled = tag.GetBool("hasBeenControlled");
            if (tag.ContainsKey("loyalty"))
                loyalty = tag.GetFloat("loyalty");
        }
    }
}
