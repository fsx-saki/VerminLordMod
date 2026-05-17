using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.One
{
	/// <summary>
	/// 灵盾蛊 — 一转金道蛊虫
	/// 发射金灵盾弹，命中后产生大范围爆炸
	/// 特性：爆炸范围伤害(ExplosionKill) + 高伤害 + 强击退
	/// 金道定位：重装型，牺牲速度换取单发高伤害和范围效果
	/// </summary>
	class SpiritGuardGu : GoldWeapon
	{
		protected override int qiCost => 5;
		protected override int _useTime => 22;
		protected override int _guLevel => 1;
		protected override int controlQiCost => 5;
		protected override float unitConntrolRate => 25;

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 1;
			Item.value = 65000;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.damage = 12;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 5f;
			Item.crit = 2;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<SpiritGuardProj>();
			Item.shootSpeed = 7f;
		}
	}
}