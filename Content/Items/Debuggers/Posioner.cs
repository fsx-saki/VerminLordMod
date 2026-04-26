using VerminLordMod.Common.Players;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.Weapons;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Items.Debuggers
{
	class Posioner : ModItem
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
			Item.autoReuse = false;
			Item.useTurn = false;
			Item.UseSound = SoundID.Item1;

		}
		//public override bool? UseItem(Player player) {
		//	Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<Moon>());
		//	return true;
		//}
		public override bool? UseItem(Player player) {
			player.AddBuff(30, 3600);
			player.AddBuff(20, 3600);
			player.AddBuff(24, 3600);
			player.AddBuff(32, 3600);
			player.AddBuff(69, 3600);
			player.AddBuff(46, 3600);
			player.AddBuff(148, 3600);
			Text.ShowTextRed(player, "孩子们，别吸毒！");
			return true;
		}
	}
}
