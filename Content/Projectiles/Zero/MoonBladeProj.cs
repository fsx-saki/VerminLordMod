using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
	/// <summary>
	/// 月道模式0 — 月刃
	/// 带有劈砍刀光的直线月刃，死亡时爆散。
	/// </summary>
	public class MoonBladeProj : BaseBullet
	{
		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f) { AutoRotate = true, RotationOffset = MathHelper.PiOver2,
				EnableLight = true, LightColor = new Vector3(0.6f, 0.8f, 1.2f) });

			Behaviors.Add(new MoonTrailBehavior
			{
				SuppressDefaultDraw = false,
				EnableGhostTrail = true,
				// 宽刀光拖尾 — 营造劈砍感
				GhostMaxPositions = 16,
				GhostRecordInterval = 1,
				GhostWidthScale = 2.5f,
				GhostLengthScale = 2.5f,
				GhostAlpha = 0.9f,
				GhostColor = new Color(180, 220, 255, 220),
				// 新月弧光
				CrescentSize = 1.5f,
				CrescentOrbitRadius = 28f,
				CrescentSpawnChance = 0.35f,
				CrescentLife = 30,
				CrescentColor = new Color(200, 230, 255, 240),
				// 光束刀芒
				BeamSize = 0.8f,
				BeamLength = 30f,
				BeamSpawnInterval = 1,
				BeamLife = 18,
				BeamColor = new Color(200, 230, 255, 230),
				// 潮汐余波
				TideStartSize = 0.5f,
				TideEndSize = 3.5f,
				TideSpawnChance = 0.1f,
				TideColor = new Color(180, 210, 240, 210),
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.scale = 1.2f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = 3;
			Projectile.timeLeft = 90;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnAI()
		{
			Lighting.AddLight(Projectile.Center, 0.3f, 0.5f, 0.8f);
		}

		protected override void OnKilled(int timeLeft)
		{
			for (int i = 0; i < 25; i++)
			{
				float a = Main.rand.NextFloat(MathHelper.TwoPi);
				var d = a.ToRotationVector2();
				float sp = Main.rand.NextFloat(3f, 9f);
				Dust dust = Dust.NewDustPerfect(Projectile.Center + d * Main.rand.NextFloat(5, 20),
					DustID.AncientLight, d * sp, 40,
					new Color(160, 210, 255), Main.rand.NextFloat(1.0f, 2.0f));
				dust.noGravity = true;
			}
			for (int i = 0; i < 15; i++)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8),
					DustID.BlueFairy, Main.rand.NextVector2Circular(5, 5), 30,
					default, Main.rand.NextFloat(0.8f, 1.5f));
				dust.noGravity = true;
			}
		}
	}
}
