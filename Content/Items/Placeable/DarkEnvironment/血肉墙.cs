using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.DarkEnvironment
{
    /// <summary>
    /// 血肉墙（物品） — 血肉凝固形成的墙壁，令人不寒而栗
    /// </summary>
    public class 血肉墙 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 999;
            Item.value = 50;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.血肉墙>();
        }
    }
}