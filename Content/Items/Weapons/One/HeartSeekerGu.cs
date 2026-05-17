using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.One
{
	/// <summary>
	/// 寻心蛊 — 一转情道蛊虫
	/// 发射追踪心形弹，命中附加魅惑效果
	/// 特性：追踪弹(Homing) + 魅惑Debuff(Lovestruck) + 心形粒子拖尾
	/// 情道定位：控制型，以情感操控削弱敌人战斗力
	/// </summary>
	class HeartSeekerGu : LoveWeapon
	{
		protected override int qiCost => 4;
		protected override int _useTime => 18;
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
			Item.damage = 9;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 2f;
			Item.crit = 4;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<HeartSeekerProj>();
			Item.shootSpeed = 8f;
		}
	}
}