using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;


namespace VerminLordMod.Content.Items.Weapons.Six
{
	class DarkArrowGu : DarkWeapon//必要继承moditem
	{
		protected override int controlQiCost => 100;
		protected override int qiCost => 600;
		protected override int _useTime => 25;
		protected override int _guLevel => 6;

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
			Item.damage = 4000;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 18f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<DarkArrow>();
			Item.shootSpeed = 10f;
		}


		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2)
				return false;
			//var p1 = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<DarkArrow>(), damage, knockback, player.whoAmI);
			//var p2 = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<DarkArrow>(), damage, knockback, player.whoAmI);
			//var p3 = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<DarkArrow>(), damage, knockback, player.whoAmI);
			Vector2 plrToMouse = Main.MouseWorld - player.Center;
			//Main.NewText("hhh");
			// 计算玩家到鼠标的向量弧度
			float r = (float)Math.Atan2(plrToMouse.Y, plrToMouse.X);
			// 五个散射弹幕 分别偏移 -10 -5 0 5 10 度
			float r1 = r + -1 * MathHelper.Pi / 18f;
			float r2 = r + 0 * MathHelper.Pi / 18f;
			float r3 = r + 1 * MathHelper.Pi / 18f;
			Vector2 shootVel1 = r1.ToRotationVector2() * 10;
			Vector2 shootVel2 = r2.ToRotationVector2() * 10;
			Vector2 shootVel3 = r3.ToRotationVector2() * 10;
			var p1 = Projectile.NewProjectileDirect(source, position, shootVel1, type, damage, 10, player.whoAmI);
			var p2 = Projectile.NewProjectileDirect(source, position, shootVel2, type, damage, 10, player.whoAmI);
			var p3 = Projectile.NewProjectileDirect(source, position, shootVel3, type, damage, 10, player.whoAmI);
			p1.scale = p2.scale = p3.scale = 0.3f;
			return false;

		}


	}
}
