using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转木道功能蛊", "一转", "木")]
    public class BoZhongZhu : ModItem
    {
        private const int QiCostPerUse = 5;

        private static readonly int[] PlantItems =
        {
            ItemID.Daybloom,
            ItemID.Moonglow,
            ItemID.Blinkroot,
            ItemID.Deathweed,
            ItemID.Waterleaf,
            ItemID.Fireblossom,
            ItemID.Shiverthorn,
        };

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 20;
            Item.value = 500;
            Item.consumable = true;
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

            int selectedItem = PlantItems[Main.rand.Next(PlantItems.Length)];
            int itemIndex = Item.NewItem(null, (int)player.Center.X, (int)player.Center.Y, 0, 0, selectedItem, 1);

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex);
            }

            Text.ShowTextGreen(player, $"播种猪：种下了 {Lang.GetItemNameValue(selectedItem)}！");

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "BoZhongZhuEffect", "随机获得一株草药（日耀花、月光花、闪烁根等）"));
            tooltips.Add(new TooltipLine(Mod, "BoZhongZhuQiCost", $"消耗真元：{QiCostPerUse}"));
            tooltips.Add(new TooltipLine(Mod, "BoZhongZhuConsumable", "一次性消耗品"));
        }
    }
}
