using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.Environment
{
    public class SpiritHerbPlot : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.SpiritHerbPlot>();
        }
    }

    public class GuBreedingVat : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.GuBreedingVat>();
        }
    }

    public class MeditationCushion : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.MeditationCushion>();
        }
    }

    public class SpiritLantern : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 28; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.SpiritLantern>();
        }
    }

    public class GuCauldron : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.GuCauldron>();
        }
    }

    public class YuanStoneCluster : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.YuanStoneCluster>();
        }
    }

    public class SpiritFlower : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 24; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.SpiritFlower>();
        }
    }

    public class AncientTreeMoss : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.AncientTreeMoss>();
        }
    }

    public class GuWormHole : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.GuWormHole>();
        }
    }

    public class MoonlitPond : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.MoonlitPond>();
        }
    }

    public class WitheredTree : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 36; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.WitheredTree>();
        }
    }

    public class SpiritBeastDen : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.SpiritBeastDen>();
        }
    }

    public class HerbDryingRack : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 28; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.HerbDryingRack>();
        }
    }
}
