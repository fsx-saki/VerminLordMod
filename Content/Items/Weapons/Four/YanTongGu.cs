using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Weapons.Daos;

namespace VerminLordMod.Content.Items.Weapons.Four
{
	class YanTongGu : EatingWeapon
	{
		protected override int qiCost => 10;
		protected override int _useTime => 5;


		protected override int controlQiCost => 5;
		protected override float unitConntrolRate => 25;

		public static readonly Color OverrideColor = Color.Lerp(Color.Cyan, Color.White, 0.8f);

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
			var qiResource = player.GetModPlayer<QiResourcePlayer>();
			return qiResource.QiCurrent >= qiCost;
		}
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				return false;
			}
			var qiResource = player.GetModPlayer<QiResourcePlayer>();
			qiResource.ConsumeQi(qiCost);
			player.AddBuff(ModContent.BuffType<Content.Buffs.AddToSelf.Pobuff.YanTongbuff>(), 3600);
			return true;
		}
	}
}
