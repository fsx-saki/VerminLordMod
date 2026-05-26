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
    public class 茅草屋 : ModItem
    {
        private const int QiCostPerUse = 8;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 1;
            Item.value = 5000;
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

            int buffType = ModContent.BuffType<MaoCaoWuBuff>();
            player.AddBuff(buffType, 600);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MaoCaoWuEffect", "在身边创建安全区域，击退200像素内的敌人"));
            tooltips.Add(new TooltipLine(Mod, "MaoCaoWuDef", "激活期间防御+3"));
            tooltips.Add(new TooltipLine(Mod, "MaoCaoWuDuration", "持续10秒"));
            tooltips.Add(new TooltipLine(Mod, "MaoCaoWuQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
