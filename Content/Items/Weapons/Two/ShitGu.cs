using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;


namespace VerminLordMod.Content.Items.Weapons.Two
{
	class ShitGu : PoisonWeapon//必要继承moditem
	{
		protected override int controlQiCost => 20;
		protected override int qiCost => 50;
		protected override int _useTime => 50;
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
			Item.damage = 0;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 0f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<AcidWaterProj>();
			Item.shootSpeed = 5f;
		}




		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2)
				return false;
			Projectile projectile =Projectile.NewProjectileDirect(source, player.Center, Vector2.Zero, ProjectileID.ToiletEffect,0,0);
			if (player.HasBuff(30)) {
				player.ClearBuff(30);
			}
			else if (player.HasBuff(20)) {
				player.ClearBuff(20);
			}
			else if (player.HasBuff(24)) {
				player.ClearBuff(24);
			}
			else if (player.HasBuff(32)) {
				player.ClearBuff(32);
			}
			else if (player.HasBuff(69)) {
				player.ClearBuff(69);
			}
			else if (player.HasBuff(46)) {
				player.ClearBuff(46);
			}
			else if (player.HasBuff(148)) {
				player.ClearBuff(148);
			}
			return false;
		}
	}
}
