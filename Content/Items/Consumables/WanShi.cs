using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.Weapons;
using VerminLordMod.Content.Items.Accessories.One;

namespace VerminLordMod.Content.Items.Consumables
{
	class WanShi : ModItem
	{
		public override void SetDefaults() {
			Item.width = 24;//碰撞箱宽度 一般设置为贴图宽度
			Item.height = 24;//碰撞箱高度 一般设置为贴图高度
			Item.rare = ItemRarityID.White;//物品稀有度
			Item.maxStack = 9999;//最大堆叠数量 默认一个
			Item.value = 100;//价值 为购买价格 从右向左为铜币银币金币铂金币 卖出价格是购买价格的五分之一

		}

		public override bool CanRightClick() {
			return true;
		}


		// 这个函数的执行条件跟上面的RightClick是一致的
		//public override void ModifyItemLoot(ItemLoot itemLoot) {
		//	// 给战利品池子添加：不受玩家幸运影响的有1/7概率掉落的猪鲨翅膀，这个方法有四个参数，物品id，掉率分母，也就是1/分母的掉率
		//	// 最小掉落数量，默认1，最大掉落数量，默认1
		//	itemLoot.Add(ItemDropRule.NotScalingWithLuck(ItemID.FishronWings, 7));
		//	// 添加：受玩家幸运影响的，1 / 2 = 50%概率掉落（这个概率将被幸运修正提高）的绿藻矿，30~60个
		//	itemLoot.Add(ItemDropRule.Common(ItemID.ChlorophyteOre, 2, 30, 60));
		//	// 添加：掉落基于世纪之花价值的钱币
		//	itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(NPCID.Plantera));
		//}

		public override void ModifyItemLoot(ItemLoot itemLoot) {
			itemLoot.Add(ItemDropRule.Common(ItemID.StoneBlock, 2, 10, 10));
			//itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<StoneSkin>(), 50, 1));
			foreach(int i in GuLists.GusCanFromWanShi) {
				itemLoot.Add(ItemDropRule.Common(i, 1000, 1));
			}
			//itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<StoneSkin>(), 1, 10));
			//itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<StoneSkin>(), 1, 10));

		}
	}
}
