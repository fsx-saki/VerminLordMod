using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.One
{
	class Moonlight : MoonWeapon//必要继承moditem
	{
		//物品属性
		public override void SetDefaults() {
			//静态属性
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.White;
			Item.value = 50000;

			//使用属性
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.damage = 20;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 4f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<MoonlightProj>();
			Item.shootSpeed = 7f;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2)
				return false;

			if (player.HasBuff<Minilightbuff>()) {
				var p = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<MoonlightProj>(), damage * 2, knockback * 2, player.whoAmI);
				p.scale = 1f;
			}
			else {
				var p = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<MoonlightProj>(), damage * 2, knockback * 2, player.whoAmI);
				p.scale = 0.5f;
			}
			return false;

		}
	}
}
