using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Debuff;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Two
{
	class ThreeStepGrassGu : FlyingWeapon
	{
		protected override int _guLevel => 2;
		protected override int _useTime => 10;
		protected override int qiCost => 60;
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
		public override bool CanUseItem(Player player) {
			if (player.HasBuff(ModContent.BuffType<ThreeStepGrassGuCDbuff>())) {
				Text.ShowTextRed(player, "冷却中......");
				return false;
			}
			return base.CanUseItem(player);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if(player.altFunctionUse==2)return false;
			QiPlayer qiPlayer=player.GetModPlayer<QiPlayer>();
			
			
			qiPlayer.ThreeStepGrassCounter++;
			Vector2 mousetPos = Main.MouseWorld;
			Vector2 playerPos = player.position;
			player.AddBuff(ModContent.BuffType<ThreeStepGrassbuff>(), 240);
			player.velocity +=(Vector2.Normalize(mousetPos - playerPos))*12;
			if (qiPlayer.ThreeStepGrassCounter >= 3) {
				qiPlayer.ThreeStepGrassCounter = 0;
				player.AddBuff(ModContent.BuffType<ThreeStepGrassGuCDbuff>(), 7200);
			}

			return false;
		}

	}
}
