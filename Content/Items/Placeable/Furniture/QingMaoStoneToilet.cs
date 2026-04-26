using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.Furniture
{
	public class QingMaoStoneToilet : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.QingMaoStoneToilet>());
			Item.width = 16;
			Item.height = 24;
			Item.value = 150;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ModContent.GetInstance<QingMaoStoneBlock>(), 6)

				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
