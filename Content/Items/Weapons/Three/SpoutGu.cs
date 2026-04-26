using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Buffs.Combo.IceBladeStorm;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{
	class SpoutGu : WindWeapon
	{
		protected override int _guLevel => 3;
		protected override int _useTime => 50;
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
			Item.damage = 90;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 6f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<Spout>();
			Item.shootSpeed = 0f;

		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				if (controlRate >= 100 && player.HasBuff(ModContent.BuffType<IceBladeStorm3>())) {
					player.ClearBuff(ModContent.BuffType<IceBladeStorm3>());
					player.AddBuff(ModContent.BuffType<IceBladeStorm4>(), 300);
					//PunchCameraModifier modifier = new PunchCameraModifier(player.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 20f, 6f, 20, 1000f, FullName);
					//Main.instance.CameraModifiers.Add(modifier);
				}
				return false;
			}

			var p = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);

			return false;

		}
	}
}
