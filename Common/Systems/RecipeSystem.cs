using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Walls;
using VerminLordMod.Content.Items.Consumables;
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
    // ============================================================

    public class RecipeSystem : ModSystem
    {
        public override void AddRecipes()
        {
            // ===== 方块配方 =====
            AddBuildingRecipes();

            // ===== 一转蛊虫武器配方 =====
            // TODO: AddOneTurnGuRecipes();

            // ===== 消耗品配方 =====
            // TODO: AddConsumableRecipes();

            // ===== 饰品配方 =====
            // TODO: AddAccessoryRecipes();

            // ===== 家具配方 =====
            // TODO: AddFurnitureRecipes();
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

        // ===== 一转蛊虫武器配方（示例框架） =====
        // 月光蛊 = 元石5 + 星尘3 @ 空窍炼化台
        // 骨矛蛊 = 元石5 + 骨竹10 @ 空窍炼化台
        // 水箭蛊 = 元石3 + 水尘5 @ 空窍炼化台

        // ===== 突破丹药配方 =====
        // 一转→二转突破丹 = 元石10 + 灵草3 @ 炼丹炉
        // 二转→三转突破丹 = 元石20 + 灵草5 + 月兰2 @ 炼丹炉

        // ===== 饰品配方 =====
        // 铁皮肤 = 铁皮蛊 @ 空窍炼化台
        // 飞翼蛊 = 飞翼蛊材料 + 元石 @ 空窍炼化台
    }
}