using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Content.Buffs.AddToSelf.Debuff;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Five
{
	class TianDiHongYinGu : VoiceWeapon
	{
		protected override int qiCost => 300;
		protected override int _useTime => 30;
		protected override int _guLevel => 5;

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
			Item.damage = 300;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 3f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<TianDiHongYinProj>();
			Item.shootSpeed = 5f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2||player.HasBuff(ModContent.BuffType<TianDiHongYinCDbuff>()))
				return false;
			if (player.HasBuff(ModContent.BuffType<TianDiHongYinbuff>())) {
				if (Main.netMode != NetmodeID.Server) {
					Main.LocalPlayer.Hurt(PlayerDeathReason.LegacyDefault(), Main.LocalPlayer.statLife / 3, 0);
					player.ClearBuff(ModContent.BuffType<TianDiHongYinbuff>());
					player.AddBuff(ModContent.BuffType<TianDiHongYinCDbuff>(),1800);
				}
			}
			else {
				if (Main.netMode != NetmodeID.Server&&Randommer.Roll(25)) {
					player.AddBuff(ModContent.BuffType<TianDiHongYinbuff>(), 600);
				}
			}
			return true;
		}
	}
}
