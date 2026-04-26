using VerminLordMod.Content.Tiles.Furniture;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace VerminLordMod.Content.Items.Placeable.Furniture
{
	public class QingMaoStoneDoor : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<QingMaoStoneDoorClosed>());
			Item.width = 14;
			Item.height = 28;
			Item.value = 150;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ModContent.GetInstance<QingMaoStoneBlock>(), 6)

				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}