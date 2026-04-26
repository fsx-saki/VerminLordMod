using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.Consumables;

namespace VerminLordMod.Content.Items.Placeable.Furniture
{
	public class QingMaoStoneBed : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.QingMaoStoneBed>());
			Item.width = 28;
			Item.height = 20;
			Item.value = 2000;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.GetInstance<QingMaoStoneBlock>(), 15)
			.AddIngredient(ItemID.Silk, 5)
							.AddTile(TileID.WorkBenches)

				.Register();
		}
	}
}