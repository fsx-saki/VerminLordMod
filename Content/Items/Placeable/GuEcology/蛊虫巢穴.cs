using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.GuEcology
{
    /// <summary>
    /// 蛊虫巢穴（物品） — 野生蛊虫的巢穴，可能有蛊虫栖息
    /// </summary>
    public class 蛊虫巢穴 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 99;
            Item.value = 100;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.GuEcology.蛊虫巢穴>();
        }
    }
}