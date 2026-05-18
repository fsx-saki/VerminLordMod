using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.Environment
{
    /// <summary>
    /// 灵药圃（物品） — 种植灵药的园圃
    /// </summary>
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

    /// <summary>
    /// 蛊虫繁殖缸（物品） — 用于繁殖蛊虫的缸
    /// </summary>
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

    /// <summary>
    /// 蒲团（物品） — 修炼打坐用的蒲团
    /// </summary>
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

    /// <summary>
    /// 灵灯（物品） — 灵气点亮的灯笼
    /// </summary>
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

    /// <summary>
    /// 蛊鼎（物品） — 炼制蛊虫的鼎
    /// </summary>
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

    /// <summary>
    /// 元石簇（物品） — 元石聚集的矿簇
    /// </summary>
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

    /// <summary>
    /// 灵花（物品） — 蕴含灵气的花朵
    /// </summary>
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

    /// <summary>
    /// 古树苔（物品） — 古老树木上的苔藓
    /// </summary>
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

    /// <summary>
    /// 蛊虫洞（物品） — 蛊虫挖掘的洞穴
    /// </summary>
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

    /// <summary>
    /// 月池（物品） — 月光照耀的水池
    /// </summary>
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

    /// <summary>
    /// 枯树（物品） — 枯萎的树木
    /// </summary>
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

    /// <summary>
    /// 灵兽穴（物品） — 灵兽栖息的洞穴
    /// </summary>
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

    /// <summary>
    /// 药草晾架（物品） — 晾干药草的架子
    /// </summary>
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
