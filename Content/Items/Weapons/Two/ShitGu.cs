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
			if (player.HasBuff(BuffID.Bleeding)) {
				player.ClearBuff(BuffID.Bleeding);
			}
			else if (player.HasBuff(BuffID.Poisoned)) {
				player.ClearBuff(BuffID.Poisoned);
			}
			else if (player.HasBuff(BuffID.OnFire)) {
				player.ClearBuff(BuffID.OnFire);
			}
			else if (player.HasBuff(BuffID.Slow)) {
				player.ClearBuff(BuffID.Slow);
			}
			else if (player.HasBuff(BuffID.Ichor)) {
				player.ClearBuff(BuffID.Ichor);
			}
			else if (player.HasBuff(BuffID.Chilled)) {
				player.ClearBuff(BuffID.Chilled);
			}
			else if (player.HasBuff(BuffID.Rabies)) {
				player.ClearBuff(BuffID.Rabies);
			}
			return false;
		}
	}
}
