using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Two
{
	/// <summary>
	/// 战角蛊 — 二转战道蛊虫
	/// 发射大型战弹，命中后产生大范围爆炸，附加破甲
	/// 特性：大体积(scale=1.5) + 范围爆炸(80px) + 破甲Debuff + 极强击退
	/// 战道定位：重炮型，以绝对力量碾压一切
	/// </summary>
	class WarHornGu : WarWeapon
	{
		protected override int qiCost => 8;
		protected override int _useTime => 24;
		protected override int _guLevel => 2;
		protected override int controlQiCost => 8;
		protected override float unitConntrolRate => 20;

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.Green;
			Item.maxStack = 1;
			Item.value = 115000;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.damage = 22;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 6f;
			Item.crit = 2;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<WarHornProj>();
			Item.shootSpeed = 5f;
		}
	}
}