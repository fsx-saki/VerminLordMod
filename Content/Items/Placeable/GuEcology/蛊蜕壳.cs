using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuEcology
{
    /// <summary>
    /// 蛊蜕壳（物品） — 蛊虫蜕下的外壳，可用于炼制
    /// </summary>
    public class 蛊蜕壳 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 99;
            Item.value = 100;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.GuEcology.蛊蜕壳>();
        }
    }
}