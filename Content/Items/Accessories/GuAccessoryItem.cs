using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common;
using VerminLordMod.Common.Configs;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Accessories
{
	/// <summary>
	/// 需要重写的字段：
	/// <para>使用时消耗的真元qiCost = 10</para>
	/// <para>炼化时消耗的真元controlQiCos = 10</para>
	/// <para>一次炼化增加的炼化进度unitConntrolRate = 10</para>
	/// <para>蛊虫脱离炼化的速度uncontrolRate = 0.01f</para>
	/// <para>转数_guLevel = 1</para>
	/// </summary>
	class GuAccessoryItem :ModItem,IAccCanReforge
	{
		/// <summary>
		/// 使用时占据的真元
		/// </summary>
		protected virtual int qiCost => 10;
		/// <summary>
		/// 炼化时消耗的真元
		/// </summary>
		protected virtual int controlQiCost => 10;
		/// <summary>
		/// 一次炼化增加的炼化进度
		/// </summary>
		protected virtual float unitConntrolRate => 10;
		/// <summary>
		/// 是否已经被炼化
		/// </summary>
		protected bool hasBeenControlled = false;
		/// <summary>
		/// 当前炼化进度
		/// </summary>
		protected float controlRate = 0f;
		/// <summary>
		/// 蛊虫脱离炼化的速度
		/// </summary>
		protected virtual float uncontrolRate => 0.01f;
		/// <summary>
		/// 转数
		/// </summary>
		protected virtual int _guLevel => 1;
		//protected int qiCost = 20; // Add our custom resource cost

		//public static LocalizedText UsesXQiText { get; private set; }
		//public static LocalizedText ControlRate { get; private set; }

		//public override void SetStaticDefaults() {
		//	UsesXQiText = this.GetLocalization("UsesXQi");
		//	ControlRate = this.GetLocalization("ControlRate");
		//}
		//public override void ModifyTooltips(List<TooltipLine> tooltips) {
		//	
		//	tooltips.Insert(3, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
		//	if (controlRate > 0f) {
		//		tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRate.Format(controlRate)));
		//	}
		//	else {
		//		tooltips.Add(new TooltipLine(Mod, "ControlRate", "右键使用开始炼化"));
		//	}
		//}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool CanUseItem(Player player) {
			QiResourcePlayer qiResource = player.GetModPlayer<QiResourcePlayer>();
			if (player.altFunctionUse == 2) {
				Item.useTime = 5;
				Item.useAnimation = 5;
				Item.useStyle = ItemUseStyleID.HoldUp;
				if (hasBeenControlled) {
					// 已炼化 → 尝试炼入空窍
					var kongQiao = player.GetModPlayer<KongQiaoPlayer>();
					if (kongQiao.TryRefineGu(Item)) {
						return false; // 炼化成功，物品已销毁
					}
					else {
						Text.ShowTextRed(player, "空窍已满或真元不足，无法炼入空窍");
						return false;
					}
				}
				if (qiResource.QiCurrent < controlQiCost) {
					Text.ShowTextRed(player, "炼化失败 真元不足");
					return false;
				}
				qiResource.ConsumeQi(controlQiCost);
				controlRate += unitConntrolRate;
				Text.ShowTextRed(player,$"炼化中......当前进度{controlRate}%");
				return true;
			}
			return false;
		}
		//public override void RightClick(Player player) {
		//	QiPlayer qiPlayer = player.GetModPlayer<QiPlayer>();
		//	if (player.altFunctionUse == 2) {
		//		//Main.NewText($"qiCurrent{qiPlayer.qiCurrent}");
		//		//Main.NewText($"controlQiCost{controlQiCost}");
		//		if (hasBeenControlled) {
		//			Main.NewText("您已经完全炼化该蛊虫");
		//		}
		//		if (qiPlayer.qiCurrent < controlQiCost) {
		//			Main.NewText("炼化失败 真元不足");
		//		}
		//		qiPlayer.qiCurrent -= controlQiCost;
		//		controlRate += unitConntrolRate;
		//		Main.NewText($"炼化中......当前进度{controlRate}%");
		//	}
		//}
		public override void UpdateInventory(Player player) {
			if (controlRate == 100) {
				hasBeenControlled = true;
			}
			if (hasBeenControlled)
				return;
			controlRate -= uncontrolRate;
			controlRate = Utils.Clamp(controlRate, 0, 100);
		}

		

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
		public override void SaveData(TagCompound tag) {
			tag["controlRate"] = controlRate;
			tag["hasBeenControlled"] = hasBeenControlled;
		}
		public override void LoadData(TagCompound tag) {
			controlRate = tag.GetFloat("controlRate");
			hasBeenControlled = tag.GetBool("hasBeenControlled");
		}
	}
}
