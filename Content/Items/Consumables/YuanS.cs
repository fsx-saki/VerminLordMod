using VerminLordMod.Content.Items.Placeable;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.UI;

namespace VerminLordMod.Content.Items.Consumables
{

	class YuanS : ModItem
	{
		public override void SetDefaults() {

			Item.maxStack = 9999;
			Item.width = 24;//碰撞箱宽度 一般设置为贴图宽度
			Item.height = 24;//碰撞箱高度 一般设置为贴图高度
			Item.rare = ItemRarityID.Master;//物品稀有度
			Item.value = 100;//价值 为购买价格 从右向左为铜币银币金币铂金币 卖出价格是购买价格的五分之一

			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.consumable = true;
		}

		public override bool? UseItem(Player player) {
			QiPlayer qiPlayer = player.GetModPlayer<QiPlayer>();

			if (qiPlayer.qiCurrent >= qiPlayer.qiMax2&&qiPlayer.levelStage<3) {
				qiPlayer.levelStageUpRate += 7 / (qiPlayer.qiLevel + qiPlayer.levelStage * 0.2f);
				Text.ShowTextGreen(player, $"正在尝试破境...当前进度{qiPlayer.levelStageUpRate}%");
			}
			else {
				qiPlayer.qiCurrent += 16;
			}
		
			return true;
		}


		//合成表处理
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<YuanSOre>(), 2)
			.AddTile(TileID.WorkBenches)//制作站条件
			.Register();

			CreateRecipe(1314)
			.AddIngredient(ItemID.GreenCap, 5)
			.AddRecipeGroup(RecipeGroupID.GoldenCritter, 2)
			.AddIngredient(ItemID.LifeCrystal, 1)
			.AddOnCraftCallback(RefineRecipeCallbacks.IfGreen)
			.Register();

			CreateRecipe(300)
			.AddIngredient(ItemID.SkeletronTrophy,1)
			.AddIngredient(ItemID.FallenStar,300)
			.AddTile(TileID.WorkBenches)//制作站条件
			.AddOnCraftCallback(RefineRecipeCallbacks.If300)

			.Register();

			CreateRecipe(500)
			.AddIngredient(ItemID.BunnyBanner,3)
			.AddTile(TileID.WorkBenches)//制作站条件
			.AddOnCraftCallback(RefineRecipeCallbacks.IfRBT)
			.Register();

			CreateRecipe(200)
			.AddIngredient(ItemID.WoodenSword,1)
			.AddIngredient(ItemID.ZombieBanner,3)
			.AddIngredient(ItemID.GrayPressurePlate,1)
			.AddTile(TileID.WorkBenches)//制作站条件
			.AddOnCraftCallback(RefineRecipeCallbacks.IfPvp)
			.Register();

			CreateRecipe(10)
			.AddIngredient(ItemID.DirtBlock,1)
			.AddTile(TileID.WorkBenches)//制作站条件
			.AddCondition(Condition.BloodMoon)
			.AddCondition(Condition.NearWater)
			.AddCondition(Condition.NearLava)
			.AddCondition(Condition.NearHoney)
			.AddCondition(Condition.NearShimmer)
			.AddCondition(Condition.InRain)
			.AddCondition(Condition.InEvilBiome)
			.AddOnCraftCallback(RefineRecipeCallbacks.IfA)
			.Register();
		}
	}
}
