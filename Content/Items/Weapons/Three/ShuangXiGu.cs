using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.Combo.IceBladeStorm;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{
	class ShuangXiGu : IceSnowWeapon
	{
		protected override int _guLevel => 3;
		protected override int _useTime => 50;
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
			Item.shoot = ModContent.ProjectileType<ShuangXiProj>();
			Item.shootSpeed = 2f;

		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				if (controlRate >= 100 && player.HasBuff(ModContent.BuffType<IceBladeStorm4>())) {
					QiPlayer qiPlayer = player.GetModPlayer<QiPlayer>();
					if (qiPlayer.qiCurrent <= 7000) {
						Text.ShowTextRed(player, "真元不足！你已遭到组合技反噬！");
						for (int i = 0; i < 50; i++) {
							Dust.NewDust(position, player.width, player.height, DustID.RedMoss);
						}
						Main.LocalPlayer.Hurt(PlayerDeathReason.LegacyDefault(), Main.LocalPlayer.statLife / 4*3, 0);
						player.ClearBuff(ModContent.BuffType<IceBladeStorm4>());
						return false;
					}
					qiPlayer.qiCurrent -= 7000;
					var p1 = Projectile.NewProjectileDirect(source, position, Vector2.Zero, ModContent.ProjectileType<IceSpout>(), 1700, knockback, player.whoAmI);
					p1.scale = 4.5f;
					player.ClearBuff(ModContent.BuffType<IceBladeStorm4>());
					for (int i = 0; i < 50; i++) {
						Dust.NewDust(position, player.width, player.height, DustID.Ice);
					}
					PunchCameraModifier modifier = new PunchCameraModifier(player.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 20f, 6f, 20, 1000f, FullName);
					Main.instance.CameraModifiers.Add(modifier);
				}
				return false;
			}


			// 计算玩家中心到鼠标的向量，Main.MouseWorld就是鼠标在世界的位置
			Vector2 plrToMouse = Main.MouseWorld - player.Center;
			// 计算玩家到鼠标的向量弧度
			float r = (float)Math.Atan2(plrToMouse.Y, plrToMouse.X);
			// 五个散射弹幕 分别偏移 -10 -5 0 5 10 度
			// 5度 = pi/36 弧度

			for (int i = -7; i <= 7; i++) {
				// 发射向量的弧度，给原来的弧度加了一些偏移：-2*5 = -10， -1*5 = -5 ...
				float r2 = r + i * MathHelper.Pi / 180f;
				Vector2 shootVel = r2.ToRotationVector2()*4;
				float f = Main.rand.NextFloat(0.97f, 1.03f);
				Projectile.NewProjectile(source, position, shootVel*f, type, damage, knockback, player.whoAmI);
			}


			return false;
		}
	}
}
