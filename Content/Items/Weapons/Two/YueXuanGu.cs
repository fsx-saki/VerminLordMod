using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Common.UI;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Items.Weapons.One;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Two
{
	/// <summary>
	/// 二转月道蛊虫 — 月旋蛊
	/// 发出绿色月刃，螺旋曲线追踪目标。
	/// 由旋风蛊和月光蛊合炼而成。
	/// </summary>
	public class YueXuanGu : MoonWeapon, IOnHitEffectProvider
	{
		protected override int qiCost => 10;
		protected override int _useTime => 20;
		protected override int _guLevel => 2;
		protected override int controlQiCost => 6;
		protected override float unitConntrolRate => 25;

		public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.LifeSteal };
		public float DoTDuration => 3f;
		public float DoTDamage => 8f;
		public float SlowPercent => 0.3f;
		public int SlowDuration => 120;
		public float ArmorShredAmount => 7f;
		public int ArmorShredDuration => 120;
		public float WeakenPercent => 0.15f;
		public float LifeStealPercent => 0.1f;

		public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 26;
			Item.damage = 20;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 3f;
			Item.crit = 3;
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 1;
			Item.value = 1500;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.shoot = ModContent.ProjectileType<YueXuanProj>();
			Item.shootSpeed = 11f;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.autoReuse = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.GetInstance<Cyclone>(), 1)
				.AddIngredient(ModContent.GetInstance<Moonlight>(), 1)
				.AddIngredient(ModContent.GetInstance<YuanS>(), 10)
				.AddOnCraftCallback(RefineRecipeCallbacks.IfFailed)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
