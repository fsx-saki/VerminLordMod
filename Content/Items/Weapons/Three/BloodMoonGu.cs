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
using VerminLordMod.Content.Items.Weapons.Two;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{
	class BloodMoonGu : BloodWeapon
	{
		protected override int qiCost => 30;
		protected override int _useTime => 30;
		protected override int _guLevel => 3;

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
			Item.damage = 40;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 6f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<BloodMoonProj>();
			Item.shootSpeed = 10f;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {

			if (base.Shoot(player, source, position, velocity, type, damage, knockback) && Main.netMode != NetmodeID.Server) {

				int a = Main.LocalPlayer.statLife / 4;
				damage += a * 2;
				Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback);
				projectile.scale *= a / 75f > 0.5f ? a / 75f : 0.5f;
				Main.LocalPlayer.Hurt(PlayerDeathReason.LegacyDefault(), a, 0);
				for (int i = 0; i < 10; i++) {
					Dust.NewDustDirect(player.position, player.width, player.height, DustID.Blood).velocity *= 2;
				}
			}
			return false;
		}

		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.GetInstance<MoonlightPro>(), 1)
			.AddIngredient(ModContent.GetInstance<BloodQiGu>(), 1)
			.AddIngredient(ModContent.GetInstance<YuanS>(), 100)
			.AddOnCraftCallback(RefineRecipeCallbacks.IfFailed)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
