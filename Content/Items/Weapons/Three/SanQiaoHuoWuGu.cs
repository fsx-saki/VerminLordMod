using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{
	/// <summary>
	/// 三转炎道蛊虫 — 三窍火屋蛊
	/// 双窍火炉蛊的进阶，拥有三个火窍，化作火屋镇守一方。
	/// </summary>
	public class SanQiaoHuoWuGu : FireWeapon, IOnHitEffectProvider
	{
		protected override int qiCost => 25;
		protected override int _useTime => 22;
		protected override int _guLevel => 3;
		protected override int controlQiCost => 12;
		protected override float unitConntrolRate => 15;

		public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
		public float DoTDuration => 4f;
		public float DoTDamage => 10f;
		public float SlowPercent => 0.15f;
		public int SlowDuration => 90;
		public float ArmorShredAmount => 6f;
		public int ArmorShredDuration => 90;
		public float WeakenPercent => 0.08f;
		public float LifeStealPercent => 0.05f;

		public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 26;
			Item.damage = 42;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 6f;
			Item.crit = 5;
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 1;
			Item.value = 5800;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 22;
			Item.useTime = 22;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.shoot = ModContent.ProjectileType<SanQiaoHuoWuProj>();
			Item.shootSpeed = 11f;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.autoReuse = true;
		}
	}
}
