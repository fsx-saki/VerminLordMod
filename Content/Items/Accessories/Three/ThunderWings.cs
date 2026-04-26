using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Accessories.Three
{
	[AutoloadEquip(EquipType.Wings)]
	class ThunderWings : GuAccessoryItem
	{

		protected override int _guLevel => 3;
		protected override int qiCost => 15;
		public static LocalizedText UsesXQiText { get; private set; }
		public static LocalizedText ControlRate { get; private set; }
		public static LocalizedText GuLevel { get; private set; }
		public override void SetStaticDefaults() {
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(3, 10f, 10f);
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
			Item.width = 22;
			Item.height = 20;
			Item.value = 50000;
			Item.rare = ItemRarityID.Blue;
			Item.accessory = true;
			Item.useStyle = ItemUseStyleID.Guitar;
		}


		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			ascentWhenFalling = 0.85f; // Falling glide speed
			ascentWhenRising = 0.15f; // Rising speed
			maxCanAscendMultiplier = 1f;
			maxAscentMultiplier = 3f;
			constantAscend = 0.135f;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			if (Main.netMode == NetmodeID.Server)
				return;

			var qiPlayer = player.GetModPlayer<QiPlayer>();
			if (_guLevel > qiPlayer.qiLevel && Randommer.Roll(10)) {
				Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
				player.Hurt(PlayerDeathReason.LegacyDefault(), (_guLevel - qiPlayer.qiLevel) * Main.LocalPlayer.statLifeMax2 / 20, 0);
			}
			qiPlayer.qiMax2 -= qiCost;
		}
	}
}
