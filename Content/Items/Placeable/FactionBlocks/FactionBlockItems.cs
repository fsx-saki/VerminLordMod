using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.FactionBlocks
{
    /// <summary>
    /// 古月木块（物品） — 古月宗的木制方块
    /// </summary>
        public class GuYueWoodBlock : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.GuYueWoodBlock>();
        }
    }

    /// <summary>
    /// 古月竹台（物品） — 古月宗的竹制平台
    /// </summary>
        public class GuYueBambooPlatform : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.GuYueBambooPlatform>();
        }
    }

    /// <summary>
    /// 白玉方块（物品） — 白玉制成的方块
    /// </summary>
        public class BaiJadeBlock : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.BaiJadeBlock>();
        }
    }

    /// <summary>
    /// 白银砖（物品） — 白银色的砖块
    /// </summary>
        public class BaiSilverBrick : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.BaiSilverBrick>();
        }
    }

    /// <summary>
    /// 熊家暗石（物品） — 熊家的暗色石块
    /// </summary>
        public class XiongDarkStone : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.XiongDarkStone>();
        }
    }

    /// <summary>
    /// 熊家铁梁（物品） — 熊家的铁制梁柱
    /// </summary>
        public class XiongIronBeam : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.XiongIronBeam>();
        }
    }

    /// <summary>
    /// 铁家锻造石（物品） — 铁家的锻造石
    /// </summary>
        public class TieForgeStone : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.TieForgeStone>();
        }
    }

    /// <summary>
    /// 王家水晶砖（物品） — 王家的水晶砖
    /// </summary>
        public class WangCrystalBlock : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.WangCrystalBlock>();
        }
    }

    /// <summary>
    /// 赵家影砖（物品） — 赵家的暗影砖
    /// </summary>
        public class ZhaoShadowBrick : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.ZhaoShadowBrick>();
        }
    }

    /// <summary>
    /// 贾家金丝砖（物品） — 贾家的金丝装饰砖
    /// </summary>
        public class JiaGoldSilkBlock : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.JiaGoldSilkBlock>();
        }
    }

    /// <summary>
    /// 散修泥砖（物品） — 散修使用的泥砖
    /// </summary>
        public class ScatteredMudBrick : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.ScatteredMudBrick>();
        }
    }

    /// <summary>
    /// 古月竹墙（物品） — 古月宗的竹墙
    /// </summary>
        public class GuYueBambooWall : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.GuYueBambooWall>();
        }
    }

    /// <summary>
    /// 白玉柱（物品） — 白玉制成的柱子
    /// </summary>
        public class BaiJadePillar : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.BaiJadePillar>();
        }
    }

    /// <summary>
    /// 铁家锻造墙（物品） — 铁家的锻造墙壁
    /// </summary>
        public class TieForgeWall : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.TieForgeWall>();
        }
    }

    /// <summary>
    /// 王家水晶柱（物品） — 王家的水晶柱
    /// </summary>
        public class WangCrystalPillar : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.WangCrystalPillar>();
        }
    }

    /// <summary>
    /// 赵家影幕（物品） — 赵家的暗影幕帘
    /// </summary>
        public class ZhaoShadowCurtain : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.ZhaoShadowCurtain>();
        }
    }

    /// <summary>
    /// 贾家丝绸幕（物品） — 贾家的丝绸幕帘
    /// </summary>
        public class JiaSilkCurtain : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.JiaSilkCurtain>();
        }
    }
}
