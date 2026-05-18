using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.Environment
{
    /// <summary>
    /// 血藤（物品） — 暗红色的吸血藤蔓，触碰会吸取灵气
    /// </summary>
    public class 血藤 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 24;
            Item.maxStack = 999;
            Item.value = 20;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.血藤>();
        }
    }
}