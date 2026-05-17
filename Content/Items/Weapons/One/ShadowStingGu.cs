using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.One
{
	/// <summary>
	/// 影刺蛊 — 一转影道蛊虫
	/// 发射暗影尖刺，穿透敌人并向前方散射暗影碎片
	/// 特性：穿透(2) + 高暴击(8%) + 扇形碎片散射
	/// 影道定位：暗杀型，以速度和穿透弥补单发伤害
	/// </summary>
	class ShadowStingGu : ShadowWeapon
	{
		protected override int qiCost => 3;
		protected override int _useTime => 15;
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
			Item.damage = 10;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 3f;
			Item.crit = 8;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<ShadowStingProj>();
			Item.shootSpeed = 11f;
		}
	}
}