using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.Items.Weapons.Daos;

namespace VerminLordMod.Content.Items.Weapons.One
{
	class Minilight : LightWeapon
	{

		protected override int controlQiCost => 5;
		protected override float unitConntrolRate => 20;
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

		public new bool CanUseItem(Player player) {
			var qiPlayer = player.GetModPlayer<QiPlayer>();

			if (player.altFunctionUse == 2) {
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
				controlRate += 20;
				Text.ShowTextGreen(player, $"炼化中......当前进度{controlRate}%");
				return true;
			}
			if (!hasBeenControlled) {
				Text.ShowTextRed(player, "该蛊虫还未炼化，右键使用开始炼化");
				return false;
			}
			if (player.HasBuff<Minilightbuff>())
				return false;
			bool res = qiPlayer.qiCurrent >= qiCost;
			if (res)
				player.AddBuff(ModContent.BuffType<Minilightbuff>(), 1000);
			return res;
		}
	}
}
