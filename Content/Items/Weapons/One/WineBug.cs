using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Weapons.Daos;

namespace VerminLordMod.Content.Items.Weapons.One
{
	class WineBugWeapon : PractiseWeapon
	{
		protected override int qiCost => 10;
		protected override int _useTime => 200;
		protected override float unitConntrolRate => 5;
		protected override int controlQiCost => 5;


		//不会改变的属
		public override void SetDefaults() {
			Item.width = 24;//碰撞箱宽度 一般设置为贴图宽度
			Item.height = 24;//碰撞箱高度 一般设置为贴图高度
			Item.rare = ItemRarityID.White;//物品稀有度
			Item.maxStack = 1;//最大堆叠数量 默认一个
			Item.value = 50000;//价值 为购买价格 从右向左为铜币银币金币铂金币 卖出价格是购买价格的五分之一

			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = false;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
		}

		public override bool CanUseItem(Player player) {
			var qiPlayer = player.GetModPlayer<QiPlayer>();

			if (player.altFunctionUse == 2) {
				Item.useTime = 5;
				Item.useAnimation = 5;
				Item.useStyle = ItemUseStyleID.HoldUp;
				//Main.NewText($"qiCurrent{qiPlayer.qiCurrent}");
				//Main.NewText($"controlQiCost{controlQiCost}");
				if (hasBeenControlled) {
					Text.ShowTextRed(player, "您已经完全炼化该蛊虫");
					return false;
				}
				if (qiPlayer.qiCurrent < controlQiCost) {
					Text.ShowTextRed(player, "炼化失败 真元不足");
					return false;
				}
				qiPlayer.qiCurrent -= controlQiCost;
				controlRate += unitConntrolRate;
				Text.ShowTextGreen(player, $"炼化中......当前进度{controlRate}%");
				return true;
			}
			else {

				Item.useTime = _useTime;
				Item.useAnimation = _useTime;
				Item.useStyle = _useStyle;
			}
			if (qiPlayer.qiLevel != 1) {
				Text.ShowTextRed(player, $"您不是一转蛊师,该蛊不能为你提升真元恢复速度。");
				return false;
			}
			if (qiPlayer.hasWineBug == true) {
				Text.ShowTextRed(player, $"您已经使用过酒虫,该蛊不能为你提升真元恢复速度。");
				return false;
			}
			if (!hasBeenControlled) {
				Text.ShowTextRed(player, "该蛊虫还未炼化，右键使用开始炼化");
				return false;
			}
			if (_guLevel > qiPlayer.qiLevel) {
				Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
				Main.LocalPlayer.Hurt(PlayerDeathReason.LegacyDefault(), (_guLevel - qiPlayer.qiLevel) * Main.LocalPlayer.statLifeMax2 / 20, 0);
			}
			//

			return qiPlayer.qiCurrent >= qiCost;
		}

		// Reduce resource on use
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				return false;
			}
			var qiPlayer = player.GetModPlayer<QiPlayer>();
			//var random = new Random();

			qiPlayer.hasWineBug = true;
			qiPlayer.extraQiRegen += 1;
			qiPlayer.qiCurrent -= qiCost;
			//player.GetDamage<DamageClass.Melee>()+=0.01f
			Text.ShowTextRed(player, $"使用酒虫，真元恢复永久提升1点。");
			return true;

		}

	}
}
