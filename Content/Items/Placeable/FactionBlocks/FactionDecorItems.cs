using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.FactionBlocks
{
    /// <summary>
    /// 古月祭坛（物品） — 古月宗的祭坛
    /// </summary>
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

    /// <summary>
    /// 白玉桌（物品） — 白玉制成的桌子
    /// </summary>
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

    /// <summary>
    /// 熊家铁砧（物品） — 熊家的铁砧
    /// </summary>
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

    /// <summary>
    /// 铁家熔炉（物品） — 铁家的冶炼熔炉
    /// </summary>
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

    /// <summary>
    /// 王家水球（物品） — 王家的水球装饰
    /// </summary>
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

    /// <summary>
    /// 赵家暗门（物品） — 赵家的秘密门
    /// </summary>
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

    /// <summary>
    /// 贾家柜台（物品） — 贾家的交易柜台
    /// </summary>
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

    /// <summary>
    /// 散修篝火（物品） — 散修营地的篝火
    /// </summary>
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

    /// <summary>
    /// 古月药罐（物品） — 古月宗的药罐
    /// </summary>
        public class GuYueMedicinePot : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.GuYueMedicinePot>();
        }
    }

    /// <summary>
    /// 白玉屏风（物品） — 白玉制成的屏风
    /// </summary>
        public class BaiJadeScreen : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.BaiJadeScreen>();
        }
    }

    /// <summary>
    /// 熊家骨图腾（物品） — 熊家的骨制图腾
    /// </summary>
        public class XiongBoneTotem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.XiongBoneTotem>();
        }
    }

    /// <summary>
    /// 铁家兵器架（物品） — 铁家的兵器架
    /// </summary>
        public class TieWeaponRack : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.TieWeaponRack>();
        }
    }

    /// <summary>
    /// 王家水盆（物品） — 王家的水盆装饰
    /// </summary>
        public class WangWaterBasin : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.WangWaterBasin>();
        }
    }

    /// <summary>
    /// 赵家影灯（物品） — 赵家的暗影灯笼
    /// </summary>
        public class ZhaoShadowLantern : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.ZhaoShadowLantern>();
        }
    }

    /// <summary>
    /// 贾家金币堆（物品） — 贾家的金币堆装饰
    /// </summary>
        public class JiaGoldCoinPile : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.JiaGoldCoinPile>();
        }
    }

    /// <summary>
    /// 散修帐篷（物品） — 散修的帐篷
    /// </summary>
        public class ScatteredTent : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FactionBlocks.ScatteredTent>();
        }
    }
}
