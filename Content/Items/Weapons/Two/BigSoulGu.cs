using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.UI;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Items.Weapons.One;

namespace VerminLordMod.Content.Items.Weapons.Two
{
	class BigSoulGu : SoulWeapon
	{
		protected override int controlQiCost => 5;
		protected override int qiCost => 10;
		protected override int _guLevel => 2;
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

			if (player.altFunctionUse == 2)
				return false;


			var qiPlayer = player.GetModPlayer<QiPlayer>();
			
			player.AddBuff(ModContent.BuffType<BigSoulbuff>(), 7200);
			qiPlayer.qiCurrent -= qiCost;
			//player.GetDamage<DamageClass.Melee>()+=0.01f

			return true;
		}

		public override void AddRecipes() {
			CreateRecipe()
			// 这样可以生成50个 -> CreateRecipe(50);
			// 合成材料，需要10个泥土块
			// 如果你写两种相同的材料的话，它们会分别显示而不是合并成一个
			.AddIngredient(ModContent.GetInstance<LittleSoulGu>(), 1)
			.AddIngredient(ModContent.GetInstance<YuanS>(), 100)
			.AddOnCraftCallback(RefineRecipeCallbacks.IfFailed)
			// 使用ModContent.ItemType<xxx>()来获取你的物品的type
			//.AddIngredient(ModContent.ItemType<SkirtSword>(), 1)
			.AddTile(TileID.WorkBenches)//制作站条件
										//.AddCondition(Condition.NearWater)//环境条件
			.Register();
		}
	}
}
