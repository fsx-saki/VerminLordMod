using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.DarkEnvironment
{
    /// <summary>
    /// 蛊阵（物品） — 蛊虫阵法的基石
    /// </summary>
        public class GuFormationTile : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28; Item.height = 28; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.GuFormationTile>();
        }
    }

    /// <summary>
    /// 血池（物品） — 充满鲜血的池子
    /// </summary>
        public class BloodPoolTile : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.BloodPoolTile>();
        }
    }

    /// <summary>
    /// 尸茧（物品） — 尸体被蛊丝包裹形成的茧
    /// </summary>
        public class CorpseCocoon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 28; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.CorpseCocoon>();
        }
    }

    /// <summary>
    /// 魂笼（物品） — 囚禁灵魂的笼子
    /// </summary>
        public class SoulCage : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 28; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.SoulCage>();
        }
    }

    /// <summary>
    /// 血肉墙（物品） — 血肉凝固形成的墙壁
    /// </summary>
        public class FleshWall : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.FleshWall>();
        }
    }

    /// <summary>
    /// 蛊祭坛（物品） — 进行蛊虫祭祀的祭坛
    /// </summary>
        public class GuSacrificeAltar : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.GuSacrificeAltar>();
        }
    }

    /// <summary>
    /// 枯萎器官（物品） — 枯萎的生物器官
    /// </summary>
        public class WitheredOrgan : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.WitheredOrgan>();
        }
    }

    /// <summary>
    /// 毒雾发射器（物品） — 释放毒雾的装置
    /// </summary>
        public class PoisonMistEmitter : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.PoisonMistEmitter>();
        }
    }

    /// <summary>
    /// 蛊墓碑（物品） — 蛊师墓地的碑石
    /// </summary>
        public class GuTombstone : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 24; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.GuTombstone>();
        }
    }

    /// <summary>
    /// 封灵石（物品） — 封印灵力的石头
    /// </summary>
        public class SpiritSealStone : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12; Item.height = 12; Item.maxStack = 999;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 15; Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing; Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DarkEnvironment.SpiritSealStone>();
        }
    }
}
