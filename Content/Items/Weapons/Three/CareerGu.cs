using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{
	class CareerGu : FlyingWeapon
	{
		protected override int _guLevel => 3;
		protected override int _useTime => 35;
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
			if (Math.Cos(r) <= 0) {
				player.velocity += new Vector2(-15, 0);
			}
			else {
				player.velocity += new Vector2(15, 0);
			}
			
			return false;
		}
	}
}
