using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using VerminLordMod.Content;

namespace VerminLordMod.Common.UI
{
	public static class RefineRecipeCallbacks
	{
		public static void IfFailed(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack) {
			if (Randommer.Roll(50)) {
				item.TurnToAir();
				Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_FromThis(), ItemID.RottenChunk, 1);
				Main.LocalPlayer.Hurt(PlayerDeathReason.LegacyDefault(), Main.LocalPlayer.statLife / 2, 0);
				Text.ShowTextRed(Main.LocalPlayer,$"炼蛊失败！");
			}
		}
		public static void IfPvp(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack) {
			Text.ShowTextRed(Main.LocalPlayer, $"还是pvp大佬");
			
		}public static void If300(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack) {
			Text.ShowTextRed(Main.LocalPlayer, $"桑百颗够吗");
			
		}public static void IfGreen(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack) {
			Text.ShowTextRed(Main.LocalPlayer, $"爱情不敌坚持泪");
			item.TurnToAir();
			Main.LocalPlayer.Hurt(PlayerDeathReason.LegacyDefault(), 1314, 0);
		}
		public static void IfRBT(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack) {
			Text.ShowTextRed(Main.LocalPlayer, $"兔兔这么可爱，你怎么能...");
		}public static void IfA(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack) {
			if (Randommer.Roll(90)) {
				item.TurnToAir();
				return;
			}
			Text.ShowTextRed(Main.LocalPlayer, $"啊？");
		}
	}
}
