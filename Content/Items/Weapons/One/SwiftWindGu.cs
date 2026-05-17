using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.One
{
	/// <summary>
	/// 疾风蛊 — 一转飞道蛊虫
	/// 发射疾风刃，波浪轨迹飞行，以极快速度切割敌人
	/// 特性：极高速度(shootSpeed=14) + 波浪弹道 + 快速连射(useTime=12)
	/// 飞道定位：速度型，以高频攻击压制敌人
	/// </summary>
	class SwiftWindGu : FlyingWeapon
	{
		protected override int qiCost => 2;
		protected override int _useTime => 12;
		protected override int _guLevel => 1;
		protected override int controlQiCost => 5;
		protected override float unitConntrolRate => 25;

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 1;
			Item.value = 55000;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.damage = 8;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 4f;
			Item.crit = 6;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<SwiftWindProj>();
			Item.shootSpeed = 14f;
		}
	}
}