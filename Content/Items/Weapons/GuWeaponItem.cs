using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Dusts;
using VerminLordMod.Common.Abstractions;
using VerminLordMod.Content.Items.Weapons;

namespace VerminLordMod.Content.Items.Weapons
{
	/// <summary>
	/// 武器蛊基类 — 所有可手持攻击的蛊虫。
	/// 继承 GuBaseItem，统一炼化逻辑，保留武器专属行为。
	/// 子类只需重写：qiCost, _guLevel, _useTime, _useStyle, moddustType 等。
	/// </summary>
	public class GuWeaponItem : GuBaseItem, IWeaponCanReforge
	{
		// ===== 出装路径 =====
		public override EquipSlots EquipSlot => EquipSlots.Weapon;

			// ===== 武器专属虚字段（子类可重写）=====
			/// <summary>左键使用速度（帧）</summary>
			protected override int _useTime => 20;
			/// <summary>左键使用动画</summary>
			protected override int _useStyle => ItemUseStyleID.Guitar;
			/// <summary>虚影类型（-1 表示无虚影）</summary>
			protected override int moddustType => -1;
			/// <summary>是否需要炼化</summary>
			protected override bool needCtrl => true;

		// ===== 便捷访问（兼容旧代码）=====
		public int GetGuLevel() => _guLevel;
		public int GetQiCost() => qiCost;

		// ===== 本地化 =====
		public new static LocalizedText UsesXQiText { get; private set; }
		public new static LocalizedText ControlRateText { get; private set; }
		public new static LocalizedText GuLevelText { get; private set; }

		public override void SetStaticDefaults() {
			UsesXQiText = this.GetLocalization("UsesXQi");
			ControlRateText = this.GetLocalization("ControlRate");
			GuLevelText = this.GetLocalization("GuLevel");
			base.SetStaticDefaults();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
			tooltips.Insert(3, new TooltipLine(Mod, "GuLevel", GuLevelText.Format(_guLevel)));
			if (controlRate > 0f) {
				if (needCtrl) tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRateText.Format(controlRate)));
			}
			else {
				if(needCtrl) tooltips.Add(new TooltipLine(Mod, "ControlRate", "右键使用开始炼化"));
			}
		}

		// ===== 左键行为：武器蛊发射弹幕 =====
		protected override bool? OnLeftClick(Player player) {
			var qiResource = player.GetModPlayer<QiResourcePlayer>();
			var qiRealm = player.GetModPlayer<QiRealmPlayer>();

			Item.useTime = _useTime;
			Item.useAnimation = _useTime;
			Item.useStyle = _useStyle;

			if (!hasBeenControlled && needCtrl) {
				Text.ShowTextRed(player, "该蛊虫还未炼化，右键使用开始炼化");
				return false;
			}
			if (_guLevel > qiRealm.GuLevel) {
				Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
				Main.LocalPlayer.Hurt(PlayerDeathReason.LegacyDefault(), (_guLevel - qiRealm.GuLevel) * Main.LocalPlayer.statLifeMax2 / 10, 0);
			}
			return qiResource.QiCurrent >= qiCost;
		}

		public override bool? UseItem(Player player) {
			var qiResource = player.GetModPlayer<QiResourcePlayer>();
			qiResource.ConsumeQi(qiCost);
			return true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2)
				return false;
			return true;
		}

		public override void UpdateInventory(Player player) {
			if (player.HeldItem.type != Item.type) {
				hasShownGhost = false;
			}
			base.UpdateInventory(player);
		}

		public override void HoldItem(Player player) {
			if (!hasShownGhost && moddustType != -1 && player.inventory[58].type != Item.type) {
				Vector2 position = player.Center + new Vector2(0, -player.height / 2);
				Dust dust = Dust.NewDustPerfect(position, moddustType);
				dust.velocity = new Vector2(0, -2);
				dust.noGravity = true;
				dust.fadeIn = 0.5f;
				dust.scale = 0.5f;
				dust.position += new Vector2(-dust.frame.Width / 2 * dust.scale, -dust.frame.Height / 2 * dust.scale);
				hasShownGhost = true;
			}
		}
	}
}
