using VerminLordMod.Common.Players;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Debuggers
{
	class PeppaPig : ModItem//未完成
	{

		private int qiCost; // Add our custom resource cost

		public static LocalizedText UsesXQiText { get; private set; }
		//不会改变的属性
		public override void SetStaticDefaults() {
			UsesXQiText = this.GetLocalization("UsesXQi");
		}
		public override void SetDefaults() {
			Item.width = 24;//碰撞箱宽度 一般设置为贴图宽度
			Item.height = 24;//碰撞箱高度 一般设置为贴图高度
			Item.rare = ItemRarityID.White;//物品稀有度
			Item.maxStack = 1;//最大堆叠数量 默认一个
			Item.value = 100;//价值 为购买价格 从右向左为铜币银币金币铂金币 卖出价格是购买价格的五分之一


			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = false;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			qiCost = 10;
		}

		//public override bool CanUseItem(Player player) {
		//	player.AddBuff(ModContent.BuffType<Minilightbuff>(),1800);
		//	return true;
		//}



		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			
			tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
		}

		// Make sure you can't use the item if you don't have enough resource
		public override bool CanUseItem(Player player) {
			var qiResource = player.GetModPlayer<QiResourcePlayer>();
			//if(qiResource == null) return false;
			bool res = qiResource.QiCurrent >= qiCost;
			//if (res)qiPlayer.whitePigs += 0.01f;
			return res;
		}

		// Reduce resource on use
		public override bool? UseItem(Player player) {
			var guPerk = player.GetModPlayer<GuPerkSystem>();
			// 直接使用反射或循环添加，因为 whitePigPower 的 setter 是 private
			for (int i = 0; i < 100; i++) {
				guPerk.TryAddWhitePigPower(1);
			}
			//qiResource.ConsumeQi(qiCost);
			//player.GetDamage<DamageClass.Melee>()+=0.01f
			Main.NewText($"使用小猪佩奇，你的力量有所增加。相当于已经使用了 {guPerk.whitePigPower} 只白豕蛊。");
			return true;
		}
	}
}
