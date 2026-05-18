using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.Resources
{
    /// <summary>
    /// 元石矿（物品） — 蕴含灵气的元石矿石
    /// </summary>
        public class YuanStoneOre : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Resources.YuanStoneOre>();
        }
    }

    /// <summary>
    /// 灵脉石（物品） — 灵脉中凝结的矿石
    /// </summary>
        public class SpiritVeinStone : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Resources.SpiritVeinStone>();
        }
    }

    /// <summary>
    /// 药园土壤（物品） — 适合种植灵草的土壤
    /// </summary>
        public class HerbGardenSoil : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Resources.HerbGardenSoil>();
        }
    }

    /// <summary>
    /// 毒晶矿石（物品） — 毒素结晶形成的矿石
    /// </summary>
        public class PoisonCrystalOre : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Resources.PoisonCrystalOre>();
        }
    }

    /// <summary>
    /// 魂精矿石（物品） — 蕴含魂力的矿石
    /// </summary>
        public class SoulEssenceOre : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Resources.SoulEssenceOre>();
        }
    }

    /// <summary>
    /// 赤铜矿脉（物品） — 赤铜矿的矿脉形态
    /// </summary>
        public class ResourceRedCopperOre : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Resources.ResourceRedCopperOre>();
        }
    }

    /// <summary>
    /// 暗铁矿石（物品） — 暗属性铁矿石
    /// </summary>
        public class DarkIronOre : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Resources.DarkIronOre>();
        }
    }

    /// <summary>
    /// 灵骨矿石（物品） — 灵兽骨骼矿化后的矿石
    /// </summary>
        public class SpiritBoneOre : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Resources.SpiritBoneOre>();
        }
    }

    /// <summary>
    /// 蛊卵簇（物品） — 蛊虫卵聚集的矿脉
    /// </summary>
        public class GuEggCluster : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Resources.GuEggCluster>();
        }
    }
}
