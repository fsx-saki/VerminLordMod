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

namespace VerminLordMod.Content.Items.Accessories.Four
{
	[AutoloadEquip(EquipType.Wings)]
	class TengYunWings : GuAccessoryItem
	{

		protected override int _guLevel => 3;
		protected override int qiCost => 200;
		public static LocalizedText UsesXQiText { get; private set; }
		public static LocalizedText ControlRate { get; private set; }
		public static LocalizedText GuLevel { get; private set; }
		public override void SetStaticDefaults() {
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(180, 3f, 0.2f);
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


		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			ascentWhenFalling = 1.4f;
			//这个属性是控制玩家在下落的时候开启翅膀的下落减速度（下落时爬升率），如果设为0那么下落的时候就需要比较长的时间才能上去；

			ascentWhenRising = 2.3f;
			//是玩家切换到上升状态时的爬升率

			maxCanAscendMultiplier = 1.4f;
			//是，当 玩家上升速度 比 玩家跳跃速度*这个值 小 的时候，使用那个上升爬升率；

			maxAscentMultiplier = 2.5f;
			//是玩家可以到达的最大爬升率；

			constantAscend = 2.3f;
			//是正常飞行时翅膀的爬升率；
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			if (Main.netMode == NetmodeID.Server)
				return;

			var qiRealm = player.GetModPlayer<QiRealmPlayer>();
			var qiResource = player.GetModPlayer<QiResourcePlayer>();
			if (_guLevel > qiRealm.GuLevel && Randommer.Roll(10)) {
				Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
				player.Hurt(PlayerDeathReason.LegacyDefault(), (_guLevel - qiRealm.GuLevel) * Main.LocalPlayer.statLifeMax2 / 20, 0);
			}
			qiResource.QiMaxCurrent -= qiCost;
		}
	}
}
