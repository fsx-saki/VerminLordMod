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

namespace VerminLordMod.Content.Items.Consumables
{
	class ZizhiYi : ModItem
	{
		public override void SetDefaults() {

			Item.width = 24;//碰撞箱宽度 一般设置为贴图宽度
			Item.height = 24;//碰撞箱高度 一般设置为贴图高度
			Item.rare = ItemRarityID.Cyan;//物品稀有度
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
			QiRealmPlayer qiRealm = player.GetModPlayer<QiRealmPlayer>();
			QiTalentPlayer qiTalent = player.GetModPlayer<QiTalentPlayer>();
			if (qiRealm.GuLevel > 0)
				return false;
			qiTalent.Grade = QiTalentPlayer.TalentGrade.Yi;
			qiTalent.CalculateEffects();
			qiRealm.OnAwakening();
			return true;
		}
	}
}
