using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.One
{
	/// <summary>
	/// 战策蛊 — 一转战术道蛊虫
	/// 发射金策弹，命中后向前方散射子母弹，附加灼烧
	/// 特性：前方分裂弹(SplashMode.Forward) + 灼烧Debuff + 极高暴击(10%)
	/// 战术道定位：策略型，以精准打击和后续效果取胜
	/// </summary>
	class BattlePlanGu : TacticalWeapon
	{
		protected override int qiCost => 3;
		protected override int _useTime => 16;
		protected override int _guLevel => 1;
		protected override int controlQiCost => 5;
		protected override float unitConntrolRate => 25;

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 1;
			Item.value = 60000;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.damage = 7;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 1f;
			Item.crit = 10;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<BattlePlanProj>();
			Item.shootSpeed = 12f;
		}
	}
}