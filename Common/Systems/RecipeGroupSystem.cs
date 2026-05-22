using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.Weapons.Three;

namespace VerminLordMod.Common.Systems
{
	public class RecipeGroupSystem : ModSystem {
		public override void AddRecipeGroups() {
			RecipeGroup recipeGroup = new RecipeGroup(() => "WaterDefGu",
				new int[]
				{
					ModContent.ItemType<WaterShellGu>(),
					ModContent.ItemType<WaterJiaGu>(),
				});
			recipeGroup.IconicItemId = ModContent.ItemType<WaterShellGu>();
			RecipeGroup.RegisterGroup("WaterDefGu", recipeGroup);
		}
	}
}
