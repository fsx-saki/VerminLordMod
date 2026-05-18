using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ModLoader;
using VerminLordMod.Content;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons;

namespace VerminLordMod
{
	public partial class VerminLordMod : Mod
	{
		public const string AssetPath = $"{nameof(VerminLordMod)}/Assets/";

		public static int YuanSId;

		public override void Load() {
			YuanSId = CustomCurrencyManager.RegisterCurrency(new Content.Currencies.YuanSCurrency(ModContent.ItemType<YuanS>(), 99999999L, "\u5143\u77f3"));
		}

		public override void Unload() {
		}
	}

	public class RefinementGlobalItem : GlobalItem
	{
		private static Dictionary<int, float> _preReforgeStates = new();

		public override void PreReforge(Item item) {
			if (item.ModItem is GuWeaponItem weapon) {
				_preReforgeStates[item.whoAmI] = weapon.controlRate;
			}
		}

		public override void PostReforge(Item item) {
			if (_preReforgeStates.TryGetValue(item.whoAmI, out float wasRefined) &&
				item.ModItem is GuWeaponItem weapon) {
				weapon.controlRate = wasRefined;
				_preReforgeStates.Remove(item.whoAmI);
			}
		}
	}
}
