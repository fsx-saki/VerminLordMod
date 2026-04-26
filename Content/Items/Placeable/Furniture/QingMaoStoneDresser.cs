using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.Furniture
{
	public class QingMaoStoneDresser : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.QingMaoStoneDresser>());

			Item.width = 26;
			Item.height = 22;
			Item.value = 500;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Dresser)
				.AddIngredient(ModContent.GetInstance<QingMaoStoneBlock>(), 16)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}