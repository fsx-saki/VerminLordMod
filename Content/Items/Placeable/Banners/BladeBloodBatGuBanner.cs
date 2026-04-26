using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;
using VerminLordMod.Content.Tiles.Banners;

namespace VerminLordMod.Content.Items.Placeable.Banners
{
	public class BladeBloodBatGuBanner : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<EnemyBanner>(), (int)EnemyBanner.StyleID.BladeBloodBatGu);
			Item.width = 10;
			Item.height = 24;
			Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(silver: 10));
		}
	}
}
