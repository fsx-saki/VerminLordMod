using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Weapons.Daos;

namespace VerminLordMod.Content.Items.Weapons.One
{
	class BlackPig : PractiseWeapon
	{
		protected override int qiCost => 50;


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



		// Reduce resource on use
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				return false;
			}
			var guPerk = player.GetModPlayer<GuPerkSystem>();
			if (guPerk.blackPigPower >= 100) {
				Text.ShowTextRed(player,$"当前使用了 {guPerk.blackPigPower} 次黑豕蛊,该蛊已经不能为你提升力量。");
				return false;
			}

			var random = new Random();
			if (Randommer.Roll(66)) {
				Main.LocalPlayer.Hurt(PlayerDeathReason.LegacyDefault(), 3 * guPerk.blackPigPower, 0);
			}

			guPerk.TryAddBlackPigPower(1);
			var qiResource = player.GetModPlayer<QiResourcePlayer>();
			qiResource.ConsumeQi(qiCost);
			//player.GetDamage<DamageClass.Melee>()+=0.01f
			Text.ShowTextGreen(player,$"使用白豕蛊，你的力量有所增加。当前使用了 {guPerk.blackPigPower} 次黑豕蛊。");
			return true;

		}
	}
}
