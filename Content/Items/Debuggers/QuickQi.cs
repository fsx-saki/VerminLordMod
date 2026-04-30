using VerminLordMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Debuggers
{
	class QuickQi : ModItem
	{
		public override void SetDefaults() {

			Item.width = 24;//碰撞箱宽度 一般设置为贴图宽度
			Item.height = 24;//碰撞箱高度 一般设置为贴图高度
			Item.rare = ItemRarityID.Master;//物品稀有度
			Item.maxStack = 1;//最大堆叠数量 默认一个
			Item.value = 100;//价值 为购买价格 从右向左为铜币银币金币铂金币 卖出价格是购买价格的五分之一

			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Guitar;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;

		}

		public override bool? UseItem(Player player) {
			QiRealmPlayer qiRealm = player.GetModPlayer<QiRealmPlayer>();
			QiResourcePlayer qiResource = player.GetModPlayer<QiResourcePlayer>();
			if (qiRealm.GuLevel <= 0) {
				Main.NewText("请先开启空窍（使用挂道具）后再使用此道具！");
				return false;
			}
			// 切换快速恢复模式：如果extraQiRegen已经很大则重置为0，否则设为极大值
			if (qiResource.ExtraQiRegen >= 10000) {
				qiResource.ExtraQiRegen = 0;
				Main.NewText("快速真元恢复已关闭");
			}
			else {
				qiResource.ExtraQiRegen = 10000;
				Main.NewText("快速真元恢复已开启，每帧恢复10000真元");
			}
			return true;
		}
	}
}
