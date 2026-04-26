using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ModLoader;
using VerminLordMod.Content;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons;

namespace VerminLordMod
{
	// This is a partial class, meaning some of its parts were split into other files. See VerminLordMod.*.cs for other portions.
	public partial class VerminLordMod : Mod
	{


		public const string AssetPath = $"{nameof(VerminLordMod)}/Assets/";

		public static int ExampleCustomCurrencyId;
		public static int YuanSId;

		public override void Load() {
			// Registers a new custom currency
			YuanSId = CustomCurrencyManager.RegisterCurrency(new Content.Currencies.YuanSCurrency(ModContent.ItemType<YuanS>(), 99999999L, "\u5143\u77f3"));
			//ExampleCustomCurrencyId = CustomCurrencyManager.RegisterCurrency(new Content.Currencies.ExampleCustomCurrency(ModContent.ItemType<Content.Items.ExampleItem>(), 999L, "Currency"));
			//On_Item.Prefix += OnPrefixChanged;
		}
		public override void Unload() {
			// The Unload() methods can be used for unloading/disposing/clearing special objects, unsubscribing from events, or for undoing some of your mod's actions.
			// Be sure to always write unloading code when there is a chance of some of your mod's objects being kept present inside the vanilla assembly.
			// The most common reason for that to happen comes from using events, NOT counting On.* and IL.* code-injection namespaces.
			// If you subscribe to an event - be sure to eventually unsubscribe from it.

			// NOTE: When writing unload code - be sure use 'defensive programming'. Or, in other words, you should always assume that everything in the mod you're unloading might've not even been initialized yet.
			// NOTE: There is rarely a need to null-out values of static fields, since TML aims to completely dispose mod assemblies in-between mod reloads.
			//On_Item.Prefix -= OnPrefixChanged;
		}
		// 正确的方法签名
		/*
		private bool OnPrefixChanged(On_Item.orig_Prefix orig, Item item, int prefix) {
			// 1. 保存原有状态
			//bool Controlled = (item.ModItem as GuWeaponItem)?.hasBeenControlled ?? false;
			float CRate = (item.ModItem as GuWeaponItem)?.controlRate ?? 0.0f;
			Player player = Main.LocalPlayer;
			//Text.ShowTextGreen(player,"hello"+ Controlled);
			// 2. 执行原始重铸逻辑controlRate
			orig(item, prefix);

			// 3. 恢复炼化状态
			if (item.ModItem is GuWeaponItem weapon) {
				//weapon.hasBeenControlled = true;
				if(CRate>=99f)
					weapon.controlRate = 101f;
			}
			return true;
		}
		*/
	}

	public class RefinementGlobalItem : GlobalItem
	{
		private static Dictionary<int, float> _preReforgeStates = new();

		// 重铸前保存状态
		public override void PreReforge(Item item) {
			if (item.ModItem is GuWeaponItem weapon) {
				_preReforgeStates[item.whoAmI] = weapon.controlRate;
			}
		}

		// 重铸后恢复状态
		public override void PostReforge(Item item) {
			if (_preReforgeStates.TryGetValue(item.whoAmI, out float wasRefined) &&
				item.ModItem is GuWeaponItem weapon) {
				weapon.controlRate = wasRefined;
				_preReforgeStates.Remove(item.whoAmI);

				//if (wasRefined>=99f)
				//	Main.NewText("炼化状态已保留");
			}
		}
	}
}
