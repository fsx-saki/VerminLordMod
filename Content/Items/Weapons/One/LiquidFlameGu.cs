using Microsoft.Xna.Framework;
using System;
using System.Linq;
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
	class LiquidFlameGu : StarWeapon
	{
		protected override int qiCost => 8;
		protected override int _useTime => 20;

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
			Item.damage = 40;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 4f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<IceFlameProjectile>(); // 测试用寒冰拖尾
			Item.shootSpeed = 10f;
		}
		public override void UpdateInventory(Player player) {
			int power = 0;
			foreach (var buff in Starbuff.starbuffs) {
				if (player.buffType.Contains<int>(buff.Type)) {
					power = buff.Power > power ? buff.Power : power;
				}
			}
			Item.damage = Item.OriginalDamage * (int)Math.Pow(2, power);
			base.UpdateInventory(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2)
				return false;
			return true;
		}
	}
}
