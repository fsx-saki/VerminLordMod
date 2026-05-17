using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Two
{
	/// <summary>
	/// 虚裂蛊 — 二转虚道蛊虫
	/// 发射虚空之球，穿透物块，命中后径向爆散虚空碎片
	/// 特性：穿透物块 + 径向散射(SplashMode.Radial) + 强击退
	/// 虚道定位：空间型，以虚空之力撕裂空间，无视阻隔
	/// </summary>
	class VoidRipGu : VoidWeapon
	{
		protected override int qiCost => 8;
		protected override int _useTime => 25;
		protected override int _guLevel => 2;
		protected override int controlQiCost => 8;
		protected override float unitConntrolRate => 20;

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.Green;
			Item.maxStack = 1;
			Item.value = 110000;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.damage = 20;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 2f;
			Item.crit = 4;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<VoidRipProj>();
			Item.shootSpeed = 6f;
		}
	}
}