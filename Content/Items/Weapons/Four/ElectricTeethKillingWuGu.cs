using Microsoft.Xna.Framework;
using Mono.Cecil;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.UI;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Items.Weapons.Three;
using VerminLordMod.Content.Items.Weapons.Two;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Four
{
	class ElectricTeethKillingWuGu : GoldWeapon
	{
		protected override int _guLevel => 4;
		protected override int _useTime => 5;
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
			Item.damage = 90;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 6f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<ElectricTeethKillingWuProj>();
			Item.shootSpeed = 3f;

		}
		List<Projectile> projectiles = new List<Projectile>();
		int maxProjNum = 40;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {

			return false;
		}
		
		public override void HoldItem(Player player) {
			if (player.controlUseItem && player.altFunctionUse != 2 && hasBeenControlled) {
				if (projectiles.Count < maxProjNum) {
					for (int i = 0; i < maxProjNum; i++) {
						Projectile proj = Projectile.NewProjectileDirect(null, player.Center , Vector2.Zero, ModContent.ProjectileType<ElectricTeethKillingWuProj>(), 45, 0, player.whoAmI);
						ElectricTeethKillingWuProj sawProj = proj.ModProjectile as ElectricTeethKillingWuProj;
						sawProj.timer = i * 5;
						projectiles.Add(proj);
					}
				}
			}
			else {
				foreach (Projectile projectile in projectiles) {
					projectile.Kill();
				}
				projectiles.Clear();
			}
			if (player.dead) {
				foreach (Projectile projectile in projectiles) {
					projectile.Kill();
				}
				projectiles.Clear();
			}
		}

		public override void AddRecipes() {// 这个括号里有一个默认的参数1，你可以填任何大于0的整数。它代表了这个合成表会生成多少个物品
			CreateRecipe()
			.AddIngredient(ModContent.GetInstance<SawtoothGoldenWuGu>(), 1)
			.AddIngredient(ItemID.CopperBar, 64)
			.AddIngredient(ItemID.MeteoriteBar, 64)
			.AddIngredient(ModContent.GetInstance<YuanS>(), 200)
			.AddOnCraftCallback(RefineRecipeCallbacks.IfFailed)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
