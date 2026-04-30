using VerminLordMod.Common.Players;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Projectiles;
using rail;
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
	class Info : ModItem
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
			QiResourcePlayer qiResource = player.GetModPlayer<QiResourcePlayer>();
			QiRealmPlayer qiRealm = player.GetModPlayer<QiRealmPlayer>();
			QiTalentPlayer qiTalent = player.GetModPlayer<QiTalentPlayer>();
			GuPerkSystem guPerk = player.GetModPlayer<GuPerkSystem>();

			Main.NewText($"=== 玩家信息 ===");
			Main.NewText($"境界：{qiRealm.GuLevel}转 阶段：{qiRealm.LevelStage}");
			Main.NewText($"资质：{qiTalent.Grade} 倍率：{qiTalent.GetZiZhiMultiplier():F2}");
			Main.NewText($"真元：{qiResource.QiCurrent}/{qiResource.QiMaxCurrent} 占用：{qiResource.QiOccupied}");
			Main.NewText($"恢复速率：基础={qiResource.BaseQiRegenRate} 额外={qiResource.ExtraQiRegen}");
			Main.NewText($"白豕={guPerk.whitePigPower} 黑豕={guPerk.blackPigPower} 斤力={guPerk.jinLiPower} 钧力={guPerk.junLiPower}");
			Main.NewText($"寿蛊加成={guPerk.extraAges} 移速加成={guPerk.extraSpeed:F2}");
			Main.NewText($"酒虫等级={guPerk.wineBugLevel} 召唤栏={guPerk.hasOneMinion}");
			return true;
		}
	}
}
