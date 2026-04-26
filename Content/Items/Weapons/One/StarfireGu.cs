using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.One
{
	class StarfireGu : StarWeapon
	{
		protected override int _guLevel => 2;
		protected override int _useTime => 4;

		// 扫射计时器
		private int sweepTimer = 0;
		// 抛射弹幕计时器（受重力的）
		private int arcTimer = 0;
		//物品属性
		public override void SetDefaults() {
			//静态属性
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 1;
			Item.value = 50000;

			//使用属性
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = false;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.damage = 30;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 6f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<StarfireProj>();
			Item.shootSpeed = 15f;

		}
		public override void UpdateInventory(Player player) {
			int power = 0;
			foreach (var buff in Starbuff.starbuffs) {
				if (player.buffType.Contains<int>(buff.Type)) {
					power = buff.Power > power ? buff.Power : power;
				}
			}
			Item.damage = Item.OriginalDamage * (int)Math.Pow(2, power);

			// 更新扫射计时器
			if (player.controlUseItem && player.HeldItem.type == Item.type) {
				sweepTimer++;
				arcTimer++;
			} else {
				sweepTimer = 0;
				arcTimer = 0;
			}

			base.UpdateInventory(player);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2)
				return false;

			// 计算玩家中心到鼠标的向量
			Vector2 plrToMouse = Main.MouseWorld - player.Center;
			float baseAngle = (float)Math.Atan2(plrToMouse.Y, plrToMouse.X);

			// 点射：sweepTimer 小于 UseTime 时为单发
			if (sweepTimer <= _useTime) {
				float speed = Item.shootSpeed;
				Vector2 shootVel = baseAngle.ToRotationVector2() * speed;
				Projectile.NewProjectile(source, position, shootVel, type, damage, knockback, player.whoAmI);
				return false;
			}

			// 抛射弹幕：随时间发射高角度受重力影响的火焰
			if (arcTimer % 20 == 0 && sweepTimer > 20 + _useTime) {
				int arcCount = 1 + (int)((sweepTimer - _useTime) / 200f);
				arcCount = Math.Min(arcCount, 3);
				for (int i = 0; i < arcCount; i++) {
					float arcAngle = -MathHelper.Pi / 2 + Main.rand.NextFloat(-MathHelper.Pi / 16, MathHelper.Pi / 16);
					float speed = Main.rand.NextFloat(10f, 14f);
					Vector2 arcVel = arcAngle.ToRotationVector2() * speed;
					Projectile.NewProjectile(source, position, arcVel, 
						ModContent.ProjectileType<StarfireArcProj>(), damage, knockback, player.whoAmI);
				}
			}

			// 减慢发射频率（每12帧发射一次）
			float holdTime = sweepTimer - _useTime;
			if (holdTime % 12 != 1) {
				return false;
			}

			float sweepSpeed = 0.06f; // 更慢的扫射速度
			float sweepAngle = (float)Math.Sin(holdTime * sweepSpeed) * MathHelper.Pi / 12; // ±15度扫射

			// 强度从1倍增加到3倍（10秒 = 600帧达到最大）
			float maxPower = 3f;
			float power = Math.Min(holdTime / 600f, maxPower);

			// 基础参数（再减半）
			int baseCount = 1;
			int projectileCount = (int)(baseCount + baseCount * power); // 1 -> 3

			// 随机散布而非均匀分布
			for (int i = 0; i < projectileCount; i++) {
				float angle = baseAngle + sweepAngle + Main.rand.NextFloat(-MathHelper.Pi / 16f, MathHelper.Pi / 16f);
				float speed = Main.rand.NextFloat(8f, 12f) * (0.8f + power * 0.2f);
				Vector2 shootVel = angle.ToRotationVector2() * speed;
				Projectile.NewProjectile(source, position, shootVel, type, damage, knockback, player.whoAmI);
			}

			// 远距火焰（再减半）
			int farCount = 1 + (int)(power);
			for (int i = 0; i < farCount; i++) {
				float angle = baseAngle + sweepAngle + Main.rand.NextFloat(-MathHelper.Pi / 10f, MathHelper.Pi / 10f);
				float speed = Main.rand.NextFloat(14f, 18f);
				Vector2 shootVel = angle.ToRotationVector2() * speed;
				Projectile.NewProjectile(source, position, shootVel, type, damage, knockback, player.whoAmI);
			}

			return false;
		}

	}
}
