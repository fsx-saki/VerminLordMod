using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Four
{
	/// <summary>
	/// 四转炎道蛊虫 — 四窍火楼蛊
	/// 三窍火屋蛊的进阶，拥有四个火窍，化作高楼镇压八方。
	/// </summary>
	public class SiQiaoHuoLouGu : FireWeapon, IOnHitEffectProvider
	{
		protected override int qiCost => 40;
		protected override int _useTime => 18;
		protected override int _guLevel => 4;
		protected override int controlQiCost => 20;
		protected override float unitConntrolRate => 12;

		public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
		public float DoTDuration => 4f;
		public float DoTDamage => 12f;
		public float SlowPercent => 0.2f;
		public int SlowDuration => 120;
		public float ArmorShredAmount => 9f;
		public int ArmorShredDuration => 120;
		public float WeakenPercent => 0.1f;
		public float LifeStealPercent => 0.08f;

		public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 26;
			Item.damage = 62;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 7f;
			Item.crit = 6;
			Item.rare = ItemRarityID.LightRed;
			Item.maxStack = 1;
			Item.value = 15000;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 18;
			Item.useTime = 18;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.shoot = ModContent.ProjectileType<SiQiaoHuoLouProj>();
			Item.shootSpeed = 12f;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.autoReuse = true;
		}
	}
}
