using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Accessories.One
{
	class CopperSkin : GuAccessoryItem
	{
		public static LocalizedText UsesXQiText { get; private set; }
		public static LocalizedText ControlRate { get; private set; }
		public static LocalizedText GuLevel { get; private set; }
		public override void SetStaticDefaults() {
			UsesXQiText = this.GetLocalization("UsesXQi");
			ControlRate = this.GetLocalization("ControlRate");
			GuLevel = this.GetLocalization("GuLevel");
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			
			tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
			tooltips.Insert(3, new TooltipLine(Mod, "GuLevel", GuLevel.Format(_guLevel)));
			if (controlRate > 0f) {
				tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRate.Format(controlRate)));
			}
			else {
				tooltips.Add(new TooltipLine(Mod, "ControlRate", "右键使用开始炼化"));
			}
		}
		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 28;
			Item.value = 50000;
			Item.rare = ItemRarityID.White;
			Item.accessory = true;
			Item.defense = 16;
			Item.useStyle = ItemUseStyleID.Guitar;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			var qiPlayer = player.GetModPlayer<QiPlayer>();
			qiPlayer.qiMax2 -= qiCost;
		}

	}

}
