using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.Consumables;

namespace VerminLordMod.Content.Items.Placeable.Furniture
{
	public class QingMaoStoneChair : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.QingMaoStoneChair>());
			Item.width = 12;
			Item.height = 30;
			Item.value = 150;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.GetInstance<QingMaoStoneBlock>(), 4)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
