using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.Environment
{
    /// <summary>
    /// 铁壳花（物品） — 铁壳花瓣坚硬如甲，供养花粉兔
    /// </summary>
    public class 铁壳花 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 24;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.铁壳花>();
        }
    }
}
