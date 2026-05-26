using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转木道功能蛊屋", "一转", "木")]
    public class 凡草屋 : ModItem
    {
        private const int QiCostPerUse = 10;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 1;
            Item.value = 8000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            return qiResource.QiCurrent >= QiCostPerUse;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            int buffType = ModContent.BuffType<FanCaoWuBuff>();
            player.AddBuff(buffType, 600);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FanCaoWuEffect", "在身边创建安全区域，击退250像素内的敌人"));
            tooltips.Add(new TooltipLine(Mod, "FanCaoWuDef", "激活期间防御+5"));
            tooltips.Add(new TooltipLine(Mod, "FanCaoWuRegen", "每秒回复1点生命值"));
            tooltips.Add(new TooltipLine(Mod, "FanCaoWuDuration", "持续10秒"));
            tooltips.Add(new TooltipLine(Mod, "FanCaoWuQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
