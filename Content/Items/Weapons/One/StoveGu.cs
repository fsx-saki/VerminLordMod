using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.One
{
	/// <summary>
	/// 一转炎道蛊虫 — 单窍火炭蛊
	/// 可蕴藏火气，用于进攻，是窍火系列的基础。
	/// </summary>
	public class StoveGu : FireWeapon, IOnHitEffectProvider
	{
		protected override int qiCost => 5;
		protected override int _useTime => 30;
		protected override int _guLevel => 1;
		protected override int controlQiCost => 3;
		protected override float unitConntrolRate => 30;

		public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
		public float DoTDuration => 3f;
		public float DoTDamage => 5f;
		public float SlowPercent => 0f;
		public int SlowDuration => 0;
		public float ArmorShredAmount => 0f;
		public int ArmorShredDuration => 0;
		public float WeakenPercent => 0f;
		public float LifeStealPercent => 0f;

		public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 1;
			Item.value = 50000;

			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = false;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.damage = 90;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 6f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<StoveProj>();
			Item.shootSpeed = 14f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2)
				return false;

			Vector2 plrToMouse = Main.MouseWorld - player.Center;
			float r = (float)Math.Atan2(plrToMouse.Y, plrToMouse.X);

			for (int i = -4; i <= 4; i++)
			{
				float r2 = r + i * MathHelper.Pi / 180f;
				Vector2 shootVel = r2.ToRotationVector2() * 4;
				float f = Main.rand.NextFloat(0.9f, 1.1f);
				Projectile.NewProjectile(source, position, shootVel * f, type, damage, knockback, player.whoAmI);
			}

			return false;
		}
	}
}
