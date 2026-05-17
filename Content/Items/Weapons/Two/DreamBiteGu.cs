using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Two
{
	/// <summary>
	/// 噬梦蛊 — 二转梦道蛊虫
	/// 发射梦境泡泡，无视物块追踪敌人，命中附加困惑
	/// 特性：穿透物块(tileCollide=false) + 追踪(Homing) + 困惑Debuff
	/// 梦道定位：幻术型，无视物理障碍，以虚幻之力侵蚀敌人心智
	/// </summary>
	class DreamBiteGu : DreamWeapon
	{
		protected override int qiCost => 6;
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
			Item.damage = 16;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 3f;
			Item.crit = 6;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<DreamBiteProj>();
			Item.shootSpeed = 9f;
		}
	}
}