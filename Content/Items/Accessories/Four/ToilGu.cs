using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Accessories.Four
{
	class ToilGu : GuAccessoryItem
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
		protected override int _guLevel => 4;
		protected override int qiCost => 200;
		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 28;
			Item.value = 50000;
			Item.rare = ItemRarityID.White;
			Item.accessory = true;
			Item.defense = 20;
			Item.useStyle = ItemUseStyleID.Guitar;

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
			player.GetDamage(DamageClass.Melee) += (float)Math.Pow(1.002,(player.statLifeMax2-player.statLife));
		}
	}

}
