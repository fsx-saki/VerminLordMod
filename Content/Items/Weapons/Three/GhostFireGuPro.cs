using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.UI;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Items.Weapons.Two;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{
	class GhostFireGuPro : SoulWeapon
	{

		protected override int controlQiCost => 20;
		protected override int qiCost => 10;
		protected override int _useTime => 26;
		protected override int _guLevel => 3;


		public override void SetDefaults() {
			Item.width = 24; // The width of the item's hitbox.
			Item.height = 24; // The height of the item's hitbox.

			Item.useStyle = ItemUseStyleID.Shoot; // The way the item is used (e.g. swinging, throwing, etc.)
			Item.useTime = 25; // All vanilla yoyos have a useTime of 25.
			Item.useAnimation = 25; // All vanilla yoyos have a useAnimation of 25.
			Item.noMelee = true; // This makes it so the item doesn't do damage to enemies (the projectile does that).
			Item.noUseGraphic = true; // Makes the item invisible while using it (the projectile is the visible part).
			Item.UseSound = SoundID.Item1; // The sound that will play when the item is used.

			Item.damage = 120; // The amount of damage the item does to an enemy or player.
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 2.5f; // The amount of knockback the item inflicts.
			Item.crit = 8; // The percent chance for the weapon to deal a critical strike. Defaults to 4.
			Item.channel = true; // Set to true for items that require the attack button to be held out (e.g. yoyos and magic missile weapons)
			//Item.rare = ModContent.RarityType<ExampleModRarity>(); // The item's rarity. This changes the color of the item's name.
			Item.value = Item.buyPrice(gold: 1); // The amount of money that the item is can be bought for.

			Item.shoot = ModContent.ProjectileType<GhostFireProj>(); // Which projectile this item will shoot. We set this to our corresponding projectile.
			Item.shootSpeed = 16f; // The velocity of the shot projectile.			
		}

		public override void AddRecipes() {// 这个括号里有一个默认的参数1，你可以填任何大于0的整数。它代表了这个合成表会生成多少个物品
			CreateRecipe()
			// 这样可以生成50个 -> CreateRecipe(50);
			// 合成材料，需要10个泥土块
			// 如果你写两种相同的材料的话，它们会分别显示而不是合并成一个
			.AddIngredient(ModContent.GetInstance<GhostFireGu>(), 1)
			.AddIngredient(ItemID.SoulofNight, 99)
			.AddIngredient(ModContent.GetInstance<YuanS>(), 200)
			.AddOnCraftCallback(RefineRecipeCallbacks.IfFailed)
			// 使用ModContent.ItemType<xxx>()来获取你的物品的type
			//.AddIngredient(ModContent.ItemType<SkirtSword>(), 1)
			.AddTile(TileID.WorkBenches)//制作站条件
										//.AddCondition(Condition.NearWater)//环境条件
			.Register();
		}
	}
}
