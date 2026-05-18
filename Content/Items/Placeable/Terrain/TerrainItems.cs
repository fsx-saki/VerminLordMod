using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.Terrain
{
    /// <summary>
    /// 古石径（物品） — 古老的石板路
    /// </summary>
        public class AncientStonePath : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Terrain.AncientStonePath>();
        }
    }

    /// <summary>
    /// 灵壤（物品） — 灵气充盈的土壤
    /// </summary>
        public class SpiritSoil : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Terrain.SpiritSoil>();
        }
    }

    /// <summary>
    /// 沼泽泥（物品） — 毒沼泽中的淤泥
    /// </summary>
        public class SwampMud : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Terrain.SwampMud>();
        }
    }

    /// <summary>
    /// 荒漠沙（物品） — 荒芜之地的沙土
    /// </summary>
        public class WastelandSand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Terrain.WastelandSand>();
        }
    }

    /// <summary>
    /// 寒铁地板（物品） — 寒铁制成的地板
    /// </summary>
        public class ColdIronFloor : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Terrain.ColdIronFloor>();
        }
    }

    /// <summary>
    /// 玉石砖径（物品） — 玉石铺就的小径
    /// </summary>
        public class JadeBrickPath : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Terrain.JadeBrickPath>();
        }
    }

    /// <summary>
    /// 血沙（物品） — 被鲜血浸染的沙土
    /// </summary>
        public class BloodSand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Terrain.BloodSand>();
        }
    }

    /// <summary>
    /// 暗苔石（物品） — 覆盖暗色苔藓的石头
    /// </summary>
        public class DarkMossStone : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Terrain.DarkMossStone>();
        }
    }
}
