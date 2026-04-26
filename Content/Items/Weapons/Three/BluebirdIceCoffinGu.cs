using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;


namespace VerminLordMod.Content.Items.Weapons.Three
{
	class BluebirdIceCoffinGu : IceSnowWeapon//必要继承moditem
	{
		protected override int controlQiCost => 20;
		protected override int qiCost => 250;
		protected override int _useTime => 30;
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
			Item.autoReuse = false;
			Item.useTurn = false;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.damage = 70;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 0;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<BlueBird>();
			Item.shootSpeed = 8f;
		}

	}
}
