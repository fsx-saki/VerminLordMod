using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.UI;
using VerminLordMod.Content.Buffs.AddToSelf.Debuff;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.Buffs.Combo.IceBladeStorm;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Items.Weapons.Two;

namespace VerminLordMod.Content.Items.Weapons.Three
{
	class CanopyGu : IceSnowWeapon
	{
		protected override int qiCost => 30;
		protected override int _guLevel => 3;
		protected override int controlQiCost => 5;
		protected override float unitConntrolRate => 20;
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
			if (player.altFunctionUse == 2) {
				return false;
			}
			player.AddBuff(ModContent.BuffType<Canopybuff>(), 700);


			return true;
		}


		public override void AddRecipes() {// 这个括号里有一个默认的参数1，你可以填任何大于0的整数。它代表了这个合成表会生成多少个物品
			CreateRecipe()
			.AddIngredient(ModContent.GetInstance<WhiteJadeGu>(), 1)
			.AddRecipeGroup("WaterDefGu", 1)
			.AddIngredient(ModContent.GetInstance<YuanS>(), 200)
			.AddOnCraftCallback(RefineRecipeCallbacks.IfFailed)
			.AddTile(TileID.WorkBenches)
			.Register();
		}

	}
}
