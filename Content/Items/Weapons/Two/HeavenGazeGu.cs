using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Two
{
	/// <summary>
	/// 天眼蛊 — 二转天道蛊虫
	/// 发射天光射线，极速穿透多个敌人，高暴击
	/// 特性：极高速度(shootSpeed=16) + 穿透(3) + 前方散射 + 高暴击(8%)
	/// 天道定位：审判型，以天光之威打击邪恶
	/// </summary>
	class HeavenGazeGu : SkyWeapon
	{
		protected override int qiCost => 7;
		protected override int _useTime => 22;
		protected override int _guLevel => 2;
		protected override int controlQiCost => 8;
		protected override float unitConntrolRate => 20;

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.Green;
			Item.maxStack = 1;
			Item.value = 105000;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.damage = 18;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 4f;
			Item.crit = 8;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<HeavenGazeProj>();
			Item.shootSpeed = 10f;
		}
	}
}