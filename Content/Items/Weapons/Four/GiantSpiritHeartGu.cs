using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.Items.Weapons.Daos;

namespace VerminLordMod.Content.Items.Weapons.Four
{
	class GiantSpiritHeartGu : PersonWeapon
	{
		protected override int controlQiCost => 50;
		protected override int _guLevel => 4;
		protected override float unitConntrolRate => 12;
		protected override int qiCost => 5000;
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
		public override bool? UseItem(Player player) {
			var qiPlayer = player.GetModPlayer<QiPlayer>();

			qiPlayer.qiCurrent -= qiCost;
			player.AddBuff(ModContent.BuffType<GiantSpiritHeartbuff>(), 7200);
			return true;
		}
	}
}
