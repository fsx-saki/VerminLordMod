using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace VerminLordMod.Content.Items.Consumables
{
	class GoldShari : ModItem, IGu
	{
		//舍利蛊唯一需要改变的属性就是转数
		private int _guLevel = 4;
		public static LocalizedText GuLevel { get; private set; }
		public override void SetStaticDefaults() {
			GuLevel = this.GetLocalization("GuLevel");
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.Insert(2, new TooltipLine(Mod, "GuLevel", GuLevel.Format(_guLevel)));
		}
		public override void SetDefaults() {
			Item.width = 24;//碰撞箱宽度 一般设置为贴图宽度
			Item.height = 24;//碰撞箱高度 一般设置为贴图高度
			Item.rare = ItemRarityID.White;//物品稀有度
			Item.maxStack = 9999;//最大堆叠数量 默认一个
			Item.value = 50000;//价值 为购买价格 从右向左为铜币银币金币铂金币 卖出价格是购买价格的五分之一


			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.DrinkOld;
			Item.autoReuse = false;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.consumable = true;
		}
		public override bool CanUseItem(Player player) {
			var qiPlayer = player.GetModPlayer<QiPlayer>();
			//舍利蛊不能跨越阶段使用
			bool res = qiPlayer.qiLevel == _guLevel;
			//不能在巅峰的时候用
			bool res2 = qiPlayer.levelStage < 3;
			return res && res2;
		}
		public override bool? UseItem(Player player) {
			var qiPlayer = player.GetModPlayer<QiPlayer>();
			qiPlayer.levelStageUpRate = 0;
			qiPlayer.levelStage += 1;
			Text.ShowTextGreen(player, "晋升成功！");
			return true;
		}
	}
}
