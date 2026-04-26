using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Placeable.Furniture
{
	public class QingMaoStoneSink : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.QingMaoStoneSink>());
			Item.width = 24;
			Item.height = 30;
			Item.value = 3000;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ModContent.GetInstance<QingMaoStoneBlock>(), 6)
				.AddIngredient(ItemID.WaterBucket, 6)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
