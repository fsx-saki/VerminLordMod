// ============================================================
// SearchChairItem - 搜索椅（物品形态）
// 可放置在地上，玩家靠近时触发搜索 UI
// 贴图复制自 QingMaoStoneChair
// ============================================================
#nullable enable
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Debuggers.SearchChair;

/// <summary>
/// 搜索椅物品 — 放置在地上，靠近时触发搜索 UI
/// </summary>
public class SearchChairItem : ModItem
{
    public override void SetDefaults()
    {
        // 可放置为 Tile
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.SearchChairTile>());
        Item.width = 24;
        Item.height = 30;
        Item.maxStack = 99;
        Item.value = 60;
        Item.rare = ItemRarityID.White;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Wood, 4)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
