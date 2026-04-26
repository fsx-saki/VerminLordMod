using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Consumables
{
	class ZizhiJia : ModItem
	{
		public override void SetDefaults() {

			Item.width = 24;//碰撞箱宽度 一般设置为贴图宽度
			Item.height = 24;//碰撞箱高度 一般设置为贴图高度
			Item.rare = ItemRarityID.Purple;//物品稀有度
			Item.maxStack = 1;//最大堆叠数量 默认一个
			//Item.value = 100;//价值 为购买价格 从右向左为铜币银币金币铂金币 卖出价格是购买价格的五分之一

			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Guitar;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.consumable = true;
			
		}
		 
		public override bool? UseItem(Player player) {
			QiPlayer qiPlayer = player.GetModPlayer<QiPlayer>();
			if (qiPlayer.qiEnabled)
				return false;
			qiPlayer.qiEnabled = true;
			qiPlayer.PlayerZiZhi = ZiZhi.RJIA;
			qiPlayer.qiLevel = 1;
			QiPlayer.SetQis(qiPlayer);
			return true;
		}
	}
}
