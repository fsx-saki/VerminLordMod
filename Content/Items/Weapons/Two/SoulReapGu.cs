using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Two
{
	/// <summary>
	/// 割魂蛊 — 二转魂道蛊虫
	/// 发射魂火弹，无视物块追踪敌人，命中吸取生命
	/// 特性：穿透物块 + 追踪(Homing) + 吸血回复(伤害1/8) + 困惑Debuff
	/// 魂道定位：生命型，以灵魂之力汲取敌人生机
	/// </summary>
	class SoulReapGu : SoulWeapon
	{
		protected override int qiCost => 7;
		protected override int _useTime => 20;
		protected override int _guLevel => 2;
		protected override int controlQiCost => 8;
		protected override float unitConntrolRate => 20;

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.Green;
			Item.maxStack = 1;
			Item.value = 100000;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.damage = 15;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 2f;
			Item.crit = 6;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<SoulReapProj>();
			Item.shootSpeed = 7f;
		}
	}
}