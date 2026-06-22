using Terraria.ID;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
	/// <summary>
	/// 月道模式5 — 月爆
	/// 在目标位置召唤月光爆裂。
	/// 短时间滞留造成范围伤害，产生月华扩散视觉。
	/// </summary>
	public class MoonNovaProj : BaseBullet
	{
		private int _hitTimer;

		protected override void RegisterBehaviors()
		{
			// 不移动，只停留
			Behaviors.Add(new AimBehavior(speed: 0f) { AutoRotate = true });
		}

		public override void SetDefaults()
		{
			Projectile.width = 80; Projectile.height = 80;
			Projectile.scale = 1f; Projectile.ignoreWater = true;
			Projectile.tileCollide = false; Projectile.penetrate = -1;
			Projectile.timeLeft = 30; Projectile.alpha = 120;
			Projectile.friendly = true; Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		protected override void OnAI()
		{
			_hitTimer++;
			Projectile.alpha = (int)(120 * (1f - (float)_hitTimer / 30f));
			Projectile.scale = 1f + (float)_hitTimer / 30f * 0.5f;

			// 扩散月华光效
			Lighting.AddLight(Projectile.Center, 0.4f, 0.5f, 0.8f);

			// 持续产生月尘
			if (_hitTimer % 3 == 0)
			{
				var d = Dust.NewDustPerfect(
					Projectile.Center + Main.rand.NextVector2Circular(40, 40),
					DustID.AncientLight,
					Main.rand.NextVector2Circular(2, 2), 30,
					new Color(140, 200, 255), Main.rand.NextFloat(0.8f, 1.5f));
				d.noGravity = true;
			}
		}

		protected override void OnKilled(int timeLeft)
		{
			// 最终爆散
			for (int i = 0; i < 30; i++)
			{
				float a = Main.rand.NextFloat(MathHelper.TwoPi);
				var d = a.ToRotationVector2();
				float speed = Main.rand.NextFloat(3f, 10f);
				Dust dust = Dust.NewDustPerfect(
					Projectile.Center + d * Main.rand.NextFloat(10, 30),
					DustID.AncientLight, d * speed, 50,
					new Color(160, 210, 255), Main.rand.NextFloat(1.0f, 2.0f));
				dust.noGravity = true;
			}
			for (int i = 0; i < 15; i++)
			{
				Dust dust = Dust.NewDustPerfect(
					Projectile.Center + Main.rand.NextVector2Circular(10, 10),
					DustID.BlueFairy, Main.rand.NextVector2Circular(4, 4), 30,
					default, Main.rand.NextFloat(1.0f, 1.8f));
				dust.noGravity = true;
			}
		}
	}
}
