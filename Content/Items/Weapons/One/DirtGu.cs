using System;
using Terraria;
using Terraria.ID;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Weapons.Daos;

namespace VerminLordMod.Content.Items.Weapons.One
{
	class DirtGu : MudWeapon
	{
		protected override int qiCost => 10;
		protected override int _useTime => 5;


		protected override int controlQiCost => 5;
		protected override float unitConntrolRate => 25;
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
			if (player.altFunctionUse == 2)
				return false;

			//Main.NewText("secc");
			QiPlayer qiPlayer = player.GetModPlayer<QiPlayer>();
			qiPlayer.qiCurrent -= qiCost;
			Random random = new Random();

			if (Randommer.Roll(2)) {
				player.QuickSpawnItemDirect(player.GetSource_ItemUse(Item), ItemID.SiltBlock,4);
			}
			if (Randommer.Roll(50)) {
				player.QuickSpawnItemDirect(player.GetSource_ItemUse(Item), ItemID.MudBlock, random.Next(1,15));
			}
			if (Randommer.Roll(50)) {
				player.QuickSpawnItemDirect(player.GetSource_ItemUse(Item), ItemID.AshBlock, random.Next(1, 15));
			}
			if (Randommer.Roll(50)) {
				player.QuickSpawnItemDirect(player.GetSource_ItemUse(Item), ItemID.ClayBlock, random.Next(1, 15));
			}
			if (Randommer.Roll(50)) {
				player.QuickSpawnItemDirect(player.GetSource_ItemUse(Item), ItemID.SandBlock, random.Next(1, 15));
			}
			player.QuickSpawnItemDirect(player.GetSource_ItemUse(Item), ItemID.DirtBlock,random.Next(10,30));

			return true;
		}
	}
}
