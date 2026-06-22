using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Two
{
	/// <summary>
	/// 二转炎道蛊虫 — 双窍火炉蛊
	/// 单窍火炭蛊的进阶，拥有两个火窍，火力更旺。
	/// </summary>
	public class ShuangQiaoHuoLuGu : FireWeapon, IOnHitEffectProvider
	{
		protected override int qiCost => 15;
		protected override int _useTime => 25;
		protected override int _guLevel => 2;
		protected override int controlQiCost => 8;
		protected override float unitConntrolRate => 20;

		public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
		public float DoTDuration => 3f;
		public float DoTDamage => 8f;
		public float SlowPercent => 0.1f;
		public int SlowDuration => 60;
		public float ArmorShredAmount => 3f;
		public int ArmorShredDuration => 60;
		public float WeakenPercent => 0.05f;
		public float LifeStealPercent => 0f;

		public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 26;
			Item.damage = 28;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 5f;
			Item.crit = 4;
			Item.rare = ItemRarityID.Green;
			Item.maxStack = 1;
			Item.value = 2500;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 25;
			Item.useTime = 25;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.shoot = ModContent.ProjectileType<ShuangQiaoHuoLuProj>();
			Item.shootSpeed = 10f;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.autoReuse = true;
		}
	}
}
