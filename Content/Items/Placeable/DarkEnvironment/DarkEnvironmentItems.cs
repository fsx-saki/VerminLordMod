using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.DarkEnvironment
{
    /// <summary>
    /// 尸块（物品） — 尸体矿化形成的方块
    /// </summary>
        public class CorpseBlock : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.CorpseBlock>();
        }
    }

    /// <summary>
    /// 蛊尸块（物品） — 蛊虫与尸体融合的方块
    /// </summary>
        public class GuCorpseBlock : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.GuCorpseBlock>();
        }
    }

    /// <summary>
    /// 骨块（物品） — 骨骼制成的方块
    /// </summary>
        public class BoneBlock : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.BoneBlock>();
        }
    }

    /// <summary>
    /// 血石（物品） — 血液凝结形成的石头
    /// </summary>
        public class BloodStone : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.BloodStone>();
        }
    }

    /// <summary>
    /// 腐木（物品） — 腐烂的木材
    /// </summary>
        public class RotWood : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.RotWood>();
        }
    }

    /// <summary>
    /// 毒晶（物品） — 毒素结晶体
    /// </summary>
        public class PoisonCrystal : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.PoisonCrystal>();
        }
    }

    /// <summary>
    /// 魂残块（物品） — 灵魂残余凝结的方块
    /// </summary>
        public class SoulRemnantBlock : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.SoulRemnantBlock>();
        }
    }

    /// <summary>
    /// 甲壳块（物品） — 蛊虫甲壳制成的方块
    /// </summary>
        public class ChitinShell : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.ChitinShell>();
        }
    }

    /// <summary>
    /// 蛊丝网（物品） — 蛊虫吐丝结成的网
    /// </summary>
        public class GuSilkWeb : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.GuSilkWeb>();
        }
    }

    /// <summary>
    /// 诅咒之土（物品） — 被诅咒的土地
    /// </summary>
        public class CursedEarth : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.CursedEarth>();
        }
    }
}
