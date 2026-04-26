using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;


namespace VerminLordMod.Content.Items.Weapons.Two
{
	class ShiningGu : WoodWeapon//必要继承moditem
	{
		protected override int controlQiCost => 5;
		protected override int qiCost => 10;
		protected override int _useTime => 15;
		protected override int _guLevel => 2;
		protected override float unitConntrolRate => 101;
		protected override bool needCtrl => false;


		//物品属性
		public override void SetDefaults() {
			//静态属性
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 64;
			Item.value = 50000;

			//使用属性
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.damage = 65;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 0f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<ShiningProj>();
			Item.shootSpeed = 6f;
			Item.consumable = true;
			
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2)
				return false;
			return true;
		}
	}
}
