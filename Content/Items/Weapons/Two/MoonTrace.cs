using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.UI;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Items.Weapons.One;
using VerminLordMod.Content.Projectiles;


namespace VerminLordMod.Content.Items.Weapons.Two
{
	class MoonTrace : MoonWeapon//必要继承moditem
	{
		protected override int controlQiCost => 20;
		protected override int qiCost => 12;
		protected override int _useTime => 17;
		protected override int _guLevel => 2;
		//物品属性
		public override void SetDefaults() {
			//静态属性
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 1;
			Item.value = 50000;

			//使用属性
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.damage = 20;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 18f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<MoonlightProj>();
			Item.shootSpeed = 20f;
		}
		//合成表处理
		public override void AddRecipes() {// 这个括号里有一个默认的参数1，你可以填任何大于0的整数。它代表了这个合成表会生成多少个物品
			CreateRecipe()
			// 这样可以生成50个 -> CreateRecipe(50);
			// 合成材料，需要10个泥土块
			// 如果你写两种相同的材料的话，它们会分别显示而不是合并成一个
			.AddIngredient(ModContent.GetInstance<TraceStoneGu>(), 1)
			.AddIngredient(ModContent.GetInstance<Moonlight>(), 1)
			.AddIngredient(ModContent.GetInstance<YuanS>(), 10)
			.AddOnCraftCallback(RefineRecipeCallbacks.IfFailed)
			// 使用ModContent.ItemType<xxx>()来获取你的物品的type
			//.AddIngredient(ModContent.ItemType<SkirtSword>(), 1)
			.AddTile(TileID.WorkBenches)//制作站条件
										//.AddCondition(Condition.NearWater)//环境条件
			.Register();
		}

		//public override void OnCraft(Recipe recipe) {
		//	if (Randommer.Roll(50)) {

		//	}
		//}
		//[Obsolete]
		//public override void OnCraft(Recipe recipe) {
		//	recipe.ReplaceResult(ItemID.StoneBlock, 1);
		//	Main.NewText($"炼蛊失败！");

		//}

		//public override d

	}
}
