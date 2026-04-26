using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.DataStructures;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
    /// <summary>
    /// 蛊虫消耗品基类
    /// 统一处理：真元消耗、蛊虫等级检查、强行调动惩罚
    /// 子类只需重写 <see cref="ApplyEffect(Player, QiPlayer)"/> 实现具体效果
    /// </summary>
    public abstract class GuConsumableItem : ModItem
    {
        /// <summary>真元消耗量</summary>
        public abstract int QiCost { get; }

        /// <summary>蛊虫等级（一转=1，二转=2...）</summary>
        public abstract int GuLevel { get; }

        /// <summary>是否可堆叠（默认 true）</summary>
        public virtual bool IsStackable => true;

        /// <summary>使用后是否消耗物品（默认 true）</summary>
        public virtual bool IsConsumed => true;

        /// <summary>使用动画类型</summary>
        public virtual int UseStyleType => ItemUseStyleID.HoldUp;

        /// <summary>使用音效</summary>
        public virtual SoundStyle? UseSoundStyle => SoundID.Item1;

        public static LocalizedText UsesXQiText { get; private set; }
        public static LocalizedText GuLevelText { get; private set; }

        public override void SetStaticDefaults()
        {
            UsesXQiText = this.GetLocalization("UsesXQi");
            GuLevelText = this.GetLocalization("GuLevel");
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = IsStackable ? Item.CommonMaxStack : 1;
            Item.value = Item.sellPrice(0, 20, 0, 0);

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = UseStyleType;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.UseSound = UseSoundStyle;
            Item.consumable = IsConsumed;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(QiCost)));
            tooltips.Insert(3, new TooltipLine(Mod, "GuLevel", GuLevelText.Format(GuLevel)));
        }

        public override bool CanUseItem(Player player)
        {
            var qiPlayer = player.GetModPlayer<QiPlayer>();
            if (qiPlayer.qiCurrent < QiCost)
                return false;

            // 让子类有机会阻止使用
            return CanApplyEffect(player, qiPlayer);
        }

        /// <summary>
        /// 子类可重写此方法添加额外的使用条件
        /// </summary>
        protected virtual bool CanApplyEffect(Player player, QiPlayer qiPlayer)
        {
            return true;
        }

        public override bool? UseItem(Player player)
        {
            var qiPlayer = player.GetModPlayer<QiPlayer>();

            // 强行调动高转蛊虫惩罚
            if (GuLevel > qiPlayer.qiLevel)
            {
                Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
                int damage = (GuLevel - qiPlayer.qiLevel) * player.statLifeMax2 / 10;
                player.Hurt(PlayerDeathReason.LegacyDefault(), damage, 0);
            }

            // 扣除真元
            qiPlayer.qiCurrent -= QiCost;

            // 应用具体效果
            ApplyEffect(player, qiPlayer);

            return true;
        }

        /// <summary>
        /// 子类实现具体的蛊虫效果
        /// </summary>
        protected abstract void ApplyEffect(Player player, QiPlayer qiPlayer);
    }
}
