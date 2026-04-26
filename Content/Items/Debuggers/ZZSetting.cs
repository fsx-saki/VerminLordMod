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

namespace VerminLordMod.Content.Items.Debuggers
{
	class ZZSetting : ModItem
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
		ZiZhi ZiZhi = ZiZhi.RO;
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				if (ZiZhi == ZiZhi.RO) {
					ZiZhi = ZiZhi.RDING;
				}else if (ZiZhi == ZiZhi.RDING) {
					ZiZhi = ZiZhi.RBING;
				}
				else if (ZiZhi == ZiZhi.RBING) {
					ZiZhi = ZiZhi.RYI;
				}
				else if (ZiZhi == ZiZhi.RYI) {
					ZiZhi = ZiZhi.RJIA;
				}
				else if (ZiZhi == ZiZhi.RJIA) {
					ZiZhi = ZiZhi.RO;
				}
				Main.NewText(ZiZhi);
			}
			else {
				QiPlayer qiPlayer = player.GetModPlayer<QiPlayer>();
				qiPlayer.PlayerZiZhi = ZiZhi;
				QiPlayer.SetQis(qiPlayer);
			}
			return true;
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
	}
}
