using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Four
{
	class TengClawGu : WoodWeapon
	{
		// The texture doesn't have the same name as the item, so this property points to it.
		protected override int _guLevel => 4;
		protected override int _useTime => 30;
		protected override int qiCost => 100;

		public override void SetDefaults() {
			// Call this method to quickly set some of the properties below.
			//Item.DefaultToWhip(ModContent.ProjectileType<ExampleWhipProjectileAdvanced>(), 20, 2, 4);

			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.damage = 80;
			Item.knockBack = 2;
			Item.rare = ItemRarityID.Purple;

			Item.shoot = ModContent.ProjectileType<QingTengGuProj>();
			Item.shootSpeed = 6;

			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.UseSound = SoundID.Item152;
			Item.channel = true; // This is used for the charging functionality. Remove it if your whip shouldn't be chargeable.
			Item.noMelee = true;
			Item.noUseGraphic = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2)
				return false;
			// 计算玩家中心到鼠标的向量，Main.MouseWorld就是鼠标在世界的位置
			Vector2 plrToMouse = Main.MouseWorld - player.Center;
			// 计算玩家到鼠标的向量弧度
			float r = (float)Math.Atan2(plrToMouse.Y, plrToMouse.X);


			for (int i = -2; i <= 2; i++) {
				// 发射向量的弧度，给原来的弧度加了一些偏移：-2*5 = -10， -1*5 = -5 ...
				float r2 = r + i * MathHelper.Pi / 72f;
				Vector2 shootVel = r2.ToRotationVector2() * 6;
				Projectile.NewProjectile(source, position, shootVel, type, damage, knockback, player.whoAmI);
			}
			return false;
		}
	}
}
