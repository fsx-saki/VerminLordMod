using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.Combo.GoldMoonCut;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;


namespace VerminLordMod.Content.Items.Weapons.Three
{
	class MoonHandKnife : MoonWeapon//必要继承moditem
	{
		protected override int controlQiCost => 20;
		protected override int qiCost => 3;
		protected override int _useTime => 6;
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
			Item.damage = 27;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 18f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<MoonlightProjH>();
			Item.shootSpeed = 8f;
		}


		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				if (controlRate >= 100&&player.HasBuff(ModContent.BuffType<GoldMoonCutbuff>())) {
					QiPlayer qiPlayer=player.GetModPlayer<QiPlayer>();
					if (qiPlayer.qiCurrent <= 100) {
						Text.ShowTextRed(player, "真元不足！你已遭到组合技反噬！");
						for (int i = 0; i < 50; i++) {
							Dust.NewDust(position, player.width, player.height, DustID.RedMoss);
						}
						Main.LocalPlayer.Hurt(PlayerDeathReason.LegacyDefault(), Main.LocalPlayer.statLife / 2, 0);
						player.ClearBuff(ModContent.BuffType<GoldMoonCutbuff>());
						return false;
					}
					qiPlayer.qiCurrent -= 100;
					var p1 = Projectile.NewProjectileDirect(source, position, velocity*0.3f, ModContent.ProjectileType<GoldMoonCutProj>(), 430, knockback, player.whoAmI);
					p1.scale = 4.5f;
					player.ClearBuff(ModContent.BuffType<GoldMoonCutbuff>());
					for (int i = 0; i < 50; i++) {
						Dust.NewDust(position, player.width, player.height, DustID.GoldCoin);
					}
					PunchCameraModifier modifier = new PunchCameraModifier(player.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 20f, 6f, 20, 1000f, FullName);
					Main.instance.CameraModifiers.Add(modifier);
				}
				return false;
			}
				
			var p = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);

			return false;

		}

	}
}
