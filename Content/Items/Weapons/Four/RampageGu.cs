using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.UI;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Items.Weapons.Three;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Four
{
	class RampageGu : FlyingWeapon
	{
		protected override int _guLevel => 3;
		protected override int _useTime => 20;
		protected override int qiCost => 40;
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
			Item.autoReuse = false;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.damage = 0;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 6f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<ShuangXiProj>();
			Item.shootSpeed = 2f;

		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if(player.altFunctionUse==2)return false;
			Vector2 mousetPos = Main.MouseWorld;
			Vector2 playerPos = player.position;
			var r = (mousetPos - playerPos).ToRotation();
			if (-0.25*Math.PI<=r&&r< 0.25 * Math.PI) {
				player.velocity += new Vector2(20, 0);
			}
			else if(0.25 * Math.PI <= r&&r< 0.75 * Math.PI) {
				player.velocity += new Vector2(0, 20);
			}
			
			else if (-0.75 * Math.PI <= r && r < -0.25 * Math.PI) {
				player.velocity += new Vector2(0, -30);
			}
			else {
				player.velocity += new Vector2(-20, 0);
			}

			return false;
		}
		public override void AddRecipes() {// 这个括号里有一个默认的参数1，你可以填任何大于0的整数。它代表了这个合成表会生成多少个物品
			CreateRecipe()
			// 这样可以生成50个 -> CreateRecipe(50);
			// 合成材料，需要10个泥土块
			// 如果你写两种相同的材料的话，它们会分别显示而不是合并成一个
			.AddIngredient(ModContent.GetInstance<CareerGu>(), 1)
			.AddIngredient(ModContent.GetInstance<StraightCollisionGu>(), 1)
			.AddIngredient(ModContent.GetInstance<YuanS>(), 1000)
			.AddOnCraftCallback(RefineRecipeCallbacks.IfFailed)
			// 使用ModContent.ItemType<xxx>()来获取你的物品的type
			//.AddIngredient(ModContent.ItemType<SkirtSword>(), 1)
			.AddTile(TileID.WorkBenches)//制作站条件
										//.AddCondition(Condition.NearWater)//环境条件
			.Register();
		}
	}
}
