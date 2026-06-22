using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Terraria.DataStructures;

namespace VerminLordMod.Content.Projectiles.Zero
{
	/// <summary>
	/// 月道模式3 — 月汐
	/// 波浪轨迹飞行，潮汐涟漪拖尾，消失时潮汐爆发。
	/// </summary>
	public class MoonTideProj : BaseBullet
	{
		private float _waveTimer;
		private float _baseAngle;

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f) { AutoRotate = true, RotationOffset = MathHelper.PiOver2 });

			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(160, 210, 255),
				GlowLayers = 3,
				GlowBaseScale = 1.8f,
				GlowScaleIncrement = 0.5f,
				GlowBaseAlpha = 0.7f,
				GlowAlphaDecay = 0.15f,
				GlowAlphaMultiplier = 0.4f,
			});

			Behaviors.Add(new MoonTrailBehavior
			{
				SuppressDefaultDraw = false,
				EnableGhostTrail = true,
				GhostWidthScale = 1.5f,
				GhostLengthScale = 2.0f,
				GhostAlpha = 0.8f,
				GhostColor = new Color(160, 200, 240, 200),
				// 新月
				CrescentSize = 1.2f,
				CrescentOrbitRadius = 24f,
				CrescentSpawnChance = 0.35f,
				CrescentColor = new Color(180, 220, 250, 240),
				// 光束
				BeamSize = 0.7f,
				BeamLength = 25f,
				BeamColor = new Color(160, 210, 245, 230),
				// 潮汐 — 核心效果，大幅强化
				MaxTides = 12,
				TideLife = 80,
				TideStartSize = 0.8f,
				TideEndSize = 5.0f,
				TideSpawnChance = 0.2f,
				TideExpandSpeed = 2.5f,
				TideWaveAmp = 2.0f,
				TideColor = new Color(150, 200, 240, 220),
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 16; Projectile.height = 16;
			Projectile.scale = 1f; Projectile.ignoreWater = true;
			Projectile.tileCollide = true; Projectile.penetrate = 3;
			Projectile.timeLeft = 150; Projectile.alpha = 0;
			Projectile.friendly = true; Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnSpawned(IEntitySource source)
		{
			_baseAngle = Projectile.velocity.ToRotation();
		}

		protected override void OnAI()
		{
			_waveTimer += 0.1f;

			float waveAmp = 3.5f;
			Vector2 baseDir = _baseAngle.ToRotationVector2();
			Vector2 perpDir = baseDir.RotatedBy(MathHelper.PiOver2);

			float speed = Projectile.velocity.Length();
			Projectile.velocity = baseDir * speed + perpDir * (float)Math.Sin(_waveTimer) * waveAmp;
			Projectile.rotation = Projectile.velocity.ToRotation();

			Lighting.AddLight(Projectile.Center, 0.3f, 0.5f, 0.8f);
		}

		protected override void OnKilled(int timeLeft)
		{
			// 潮汐爆发 — 多层涟漪
			for (int ring = 0; ring < 3; ring++)
			{
				float radius = 10f + ring * 15f;
				for (int i = 0; i < 15; i++)
				{
					float a = Main.rand.NextFloat(MathHelper.TwoPi);
					var d = a.ToRotationVector2();
					Dust dust = Dust.NewDustPerfect(Projectile.Center + d * radius,
						DustID.AncientLight, d * Main.rand.NextFloat(2f, 6f - ring * 1.5f), 30,
						new Color(140, 200, 255), Main.rand.NextFloat(1.0f, 2.0f - ring * 0.3f));
					dust.noGravity = true;
				}
			}
			for (int i = 0; i < 20; i++)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12, 12),
					DustID.BlueFairy, Main.rand.NextVector2Circular(6, 6), 40,
					default, Main.rand.NextFloat(0.8f, 1.6f));
				dust.noGravity = true;
			}
		}
	}
}
