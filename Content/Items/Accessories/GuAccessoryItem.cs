using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common;
using VerminLordMod.Common.Configs;
using VerminLordMod.Common.Players;
using Terraria.GameContent;
using VerminLordMod.Content.Items.Accessories;

namespace VerminLordMod.Content.Items.Accessories
{
	/// <summary>
	/// 饰品蛊基类 — 兼容旧饰品栏路径的蛊虫。
	/// 继承 GuBaseItem，统一炼化逻辑，保留饰品栏装备行为。
	/// 子类只需重写：qiCost, _guLevel, controlQiCost, unitConntrolRate 等。
	/// </summary>
	public class GuAccessoryItem : GuBaseItem, IAccCanReforge
	{
		// ===== 出装路径 =====
		public override EquipSlots EquipSlot => EquipSlots.Accessory;

		// ===== 饰品专属行为 =====

		/// <summary>
		/// 每帧当物品在饰品栏中激活时调用。
		/// 子类可重写以叠加效果、扣真元等。
		/// </summary>
			public override void OnActiveTick(Player player) { }

		public override bool CanEquipAccessory(Player player, int slot, bool modded) {
			QiResourcePlayer qiResource = player.GetModPlayer<QiResourcePlayer>();
			QiRealmPlayer qiRealm = player.GetModPlayer<QiRealmPlayer>();
			if (ModContent.GetInstance<VerminLordModConfig>().LimitSth) {
				if (qiRealm.GuLevel <= 0 || player.statDefense >= qiRealm.GuLevel * 25 + qiRealm.LevelStage * 5) {
					Text.ShowTextRed(player,"当前不能从蛊虫中获取更多防御");
					return false;
				}
			}
				
			if (!hasBeenControlled) {
				Text.ShowTextRed(player, "该蛊虫还未炼化，右键使用开始炼化");
				return false;
			}
			if (qiResource.QiCurrent < qiCost) {
				return false;
			}
			return true;
		}
	}
}
