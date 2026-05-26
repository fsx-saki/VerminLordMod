using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转炼道功能蛊屋", "三转", "炼")]
    public class TianYuanBaoLian : ModItem
    {
        private const int QiCostPerUse = 18;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 10000;
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

            int buffType = ModContent.BuffType<TianYuanLianBuff>();
            player.AddBuff(buffType, 600);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "TianYuanLianEffect", "开启天元宝炼工坊，持续10秒"));
            tooltips.Add(new TooltipLine(Mod, "TianYuanLianQi", "真元恢复+20%，真元消耗-15%"));
            tooltips.Add(new TooltipLine(Mod, "TianYuanLianHeal", "每秒恢复2点生命（炼化滋养）"));
            tooltips.Add(new TooltipLine(Mod, "TianYuanLianQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
