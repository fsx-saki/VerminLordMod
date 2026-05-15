using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.FactionBlocks
{
    public class GuYueAltar : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.GuYueAltar>();
        }
    }

    public class BaiJadeTable : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.BaiJadeTable>();
        }
    }

    public class XiongAnvil : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.XiongAnvil>();
        }
    }

    public class TieSmeltingFurnace : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.TieSmeltingFurnace>();
        }
    }

    public class WangWaterOrb : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.WangWaterOrb>();
        }
    }

    public class ZhaoSecretDoor : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.ZhaoSecretDoor>();
        }
    }

    public class JiaTradingCounter : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.JiaTradingCounter>();
        }
    }

    public class ScatteredCampfire : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.ScatteredCampfire>();
        }
    }
}
