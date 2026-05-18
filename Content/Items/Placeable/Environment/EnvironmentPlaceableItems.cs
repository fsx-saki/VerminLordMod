using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.Environment
{
    /// <summary>
    /// 灵草（物品） — 灵气滋养的草
    /// </summary>
        public class SpiritGrass : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("灵草");
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.SpiritGrass>();
        }
    }

    /// <summary>
    /// 元气蘑菇（物品） — 蕴含元气的蘑菇
    /// </summary>
        public class YuanQiMushroom : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("元气蘑菇");
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.YuanQiMushroom>();
        }
    }

    /// <summary>
    /// 蛊巢（物品） — 蛊虫的巢穴
    /// </summary>
        public class GuNest : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("蛊虫巢穴");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.GuNest>();
        }
    }

    /// <summary>
    /// 毒沼植物（物品） — 生长在毒沼泽中的植物
    /// </summary>
        public class PoisonSwampPlant : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("毒沼植物");
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 24;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.PoisonSwampPlant>();
        }
    }

    /// <summary>
    /// 骨堆（物品） — 骨骼堆积物
    /// </summary>
        public class BonePile : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("白骨堆");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 12;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.BonePile>();
        }
    }

    /// <summary>
    /// 灵藤（物品） — 灵气滋养的藤蔓
    /// </summary>
        public class SpiritVine : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("灵藤");
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.SpiritVine>();
        }
    }

    public class RedCopperOre : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("红铜矿");
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.RedCopperOre>();
        }
    }

    /// <summary>
    /// 古碑文（物品） — 刻有古代文字的石碑
    /// </summary>
        public class AncientInscription : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("古老铭文");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Environment.AncientInscription>();
        }
    }
}
