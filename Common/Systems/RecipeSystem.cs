using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Walls;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Placeable;
using VerminLordMod.Content.Items.Placeable.Furniture;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // RecipeSystem — 配方系统大框
    //
    // TODO:
    //   - 填充所有蛊虫武器配方（一转到六转）
    //   - 填充所有消耗品配方（突破丹药、蛊虫食物、元石相关）
    //   - 填充所有饰品配方（翅膀、蛊甲等）
    //   - 填充所有方块/墙壁配方（建筑类）
    //   - 填充所有家具配方（青茅石家具系列）
    //   - 填充特殊配方（阵法道具、炼器材料等）
    // TODO 使用更简洁的配方注册方式
    // ============================================================
    public class RecipeSystem : ModSystem
    {
        public override void AddRecipes()
        {
            AddBuildingRecipes();
            AddOneTurnGuRecipes();
            AddConsumableRecipes();
            AddAccessoryRecipes();
            AddFurnitureRecipes();
        }

        private void AddBuildingRecipes()
        {
            // 青茅石方块 = 石头50 + 元石1 @ 工作台
            var qingMaoStone = ModContent.ItemType<Content.Items.Placeable.BoneBanbooBlock>();
            // TODO: 需要为 QingMaoStoneBlock 创建对应的 Item 类才能注册配方
            // 目前直接使用 ModTile.Item.createTile 方式

            // 元石矿石（无法合成，需在世界中自然生成）

            // 迷踪阵眼 = 元石10 @ 工作台
            var miZongRecipe = Recipe.Create(ModContent.ItemType<MiZongZhenItem>());
            miZongRecipe.AddIngredient(ModContent.ItemType<YuanS>(), 10);
            // TODO: 添加迷踪蛊虫材料
            miZongRecipe.AddTile(TileID.WorkBenches);
            miZongRecipe.Register();

            // 骨竹方块 = 竹方块50 + 元石2 @ 工作台
            // TODO: 注册配方

            // 青茅石墙配方
            // TODO: 注册配方

            // 骨竹墙配方
            // TODO: 注册配方
        }

        private void AddOneTurnGuRecipes()
        {
            var yuanS = ModContent.ItemType<YuanS>();

            var moonlight = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.Moonlight>());
            moonlight.AddIngredient(yuanS, 5);
            moonlight.AddIngredient(ItemID.FallenStar, 3);
            moonlight.AddTile(TileID.WorkBenches);
            moonlight.Register();

            var boneSpear = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.BoneSpearGu>());
            boneSpear.AddIngredient(yuanS, 5);
            boneSpear.AddIngredient(ItemID.Bone, 10);
            boneSpear.AddTile(TileID.WorkBenches);
            boneSpear.Register();

            var waterArrow = Recipe.Create(ModContent.ItemType<Content.Items.Weapons.One.WaterArrowGu>());
            waterArrow.AddIngredient(yuanS, 3);
            waterArrow.AddIngredient(ItemID.Coral, 5);
            waterArrow.AddTile(TileID.WorkBenches);
            waterArrow.Register();
        }

        private void AddConsumableRecipes()
        {
            var yuanS = ModContent.ItemType<YuanS>();

            var breakPill1 = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.FirstToSecond>());
            breakPill1.AddIngredient(yuanS, 10);
            breakPill1.AddIngredient(ItemID.Daybloom, 3);
            breakPill1.AddTile(TileID.Bottles);
            breakPill1.Register();

            var breakPill2 = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.SecondToThird>());
            breakPill2.AddIngredient(yuanS, 20);
            breakPill2.AddIngredient(ItemID.Daybloom, 5);
            breakPill2.AddIngredient(ItemID.Moonglow, 3);
            breakPill2.AddTile(TileID.Bottles);
            breakPill2.Register();

            var guFood = Recipe.Create(ModContent.ItemType<Content.Items.Consumables.WineBug>());
            guFood.AddIngredient(ItemID.Gel, 10);
            guFood.AddIngredient(ItemID.Mushroom, 5);
            guFood.AddTile(TileID.WorkBenches);
            guFood.Register();
        }

        private void AddAccessoryRecipes()
        {
        }

        private void AddFurnitureRecipes()
        {
            var yuanS = ModContent.ItemType<YuanS>();
            var qingMaoBlock = ModContent.ItemType<Content.Items.Placeable.BoneBanbooBlock>();

            var qingMaoTable = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.Furniture.QingMaoStoneTable>());
            qingMaoTable.AddIngredient(qingMaoBlock, 8);
            qingMaoTable.AddTile(TileID.WorkBenches);
            qingMaoTable.Register();

            var qingMaoChair = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.Furniture.QingMaoStoneChair>());
            qingMaoChair.AddIngredient(qingMaoBlock, 4);
            qingMaoChair.AddTile(TileID.WorkBenches);
            qingMaoChair.Register();

            var qingMaoWorkbench = Recipe.Create(ModContent.ItemType<Content.Items.Placeable.Furniture.QingMaoStoneWorkbench>());
            qingMaoWorkbench.AddIngredient(qingMaoBlock, 10);
            qingMaoWorkbench.AddTile(TileID.WorkBenches);
            qingMaoWorkbench.Register();
        }
    }
}