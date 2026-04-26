using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.Furniture
{
	public class QingMaoStoneClock : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.QingMaoStoneClock>());
			Item.width = 26;
			Item.height = 22;
			Item.value = 500;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ModContent.GetInstance<QingMaoStoneBlock>(), 13)
				.AddIngredient(ItemID.Glass, 6)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}