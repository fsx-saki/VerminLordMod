using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.Walls
{
    public class GuYueWoodWall : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 7;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createWall = ModContent.WallType<Tiles.Walls.GuYueWoodWall>();
        }
    }

    public class BaiJadeWall : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 7;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createWall = ModContent.WallType<Tiles.Walls.BaiJadeWall>();
        }
    }

    public class XiongStoneWall : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 7;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createWall = ModContent.WallType<Tiles.Walls.XiongStoneWall>();
        }
    }

    public class TieIronWall : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 7;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createWall = ModContent.WallType<Tiles.Walls.TieIronWall>();
        }
    }

    public class WangCrystalWall : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 7;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createWall = ModContent.WallType<Tiles.Walls.WangCrystalWall>();
        }
    }

    public class ZhaoShadowWall : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 7;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createWall = ModContent.WallType<Tiles.Walls.ZhaoShadowWall>();
        }
    }

    public class JiaGoldSilkWall : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 7;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createWall = ModContent.WallType<Tiles.Walls.JiaGoldSilkWall>();
        }
    }

    public class ScatteredMudWall : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 7;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createWall = ModContent.WallType<Tiles.Walls.ScatteredMudWall>();
        }
    }

    public class BoneWall : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 7;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createWall = ModContent.WallType<Tiles.Walls.BoneWall>();
        }
    }

    public class FleshWallItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 7;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createWall = ModContent.WallType<Tiles.Walls.FleshWallUnsafe>();
        }
    }
}
