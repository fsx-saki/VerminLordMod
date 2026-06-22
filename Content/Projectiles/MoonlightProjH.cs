using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Common.Effects;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 月手刀弹 — 短距离高速月刃，叠加力道斗气拖尾。
	/// 力道层：PowerTrailBehavior + MeleeSwingTrailBehavior
	/// </summary>
	public class MoonlightProjH : BaseBullet
	{
		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f)
			{
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(1.0f, 0.7f, 0.3f)
			});

			// 月道蓝尾
			Behaviors.Add(new TrailBehavior { AutoDraw = true, SuppressDefaultDraw = false });

			// 力道拖尾 — 短命弹幕用高概率
			Behaviors.Add(new PowerTrailBehavior
			{
				SuppressDefaultDraw = false,
				// 短命弹幕：每帧必出效果
				ShockWaveSpawnChance = 1.0f,
				ShockWaveStartRadius = 2f,
				ShockWaveEndRadius = 25f,
				ShockWaveWidth = 0.5f,
				ShockWaveExpandSpeed = 2.0f,
				ShockWaveColor = new Color(255, 200, 120, 200),
				AuraSpawnChance = 1.0f,
				AuraSize = 0.8f,
				AuraColor = new Color(255, 180, 100, 220),
				BurstLineSpawnChance = 1.0f,
				BurstLineLength = 25f,
				BurstLineWidth = 0.4f,
				BurstLineColor = new Color(255, 160, 80, 200),
				// Ghost trail visible
				GhostWidthScale = 0.8f,
				GhostAlpha = 0.6f,
				GhostColor = new Color(255, 180, 80, 200),
			});

			// 力道斩击弧 — 短命弹幕用高概率
			Behaviors.Add(new MeleeSwingTrailBehavior
			{
				SuppressDefaultDraw = false,
				SwingArcSpawnChance = 1.0f,
				SwingArcLength = 30f,
				SwingArcWidth = 0.5f,
				SwingArcCurlAmount = 2.0f,
				SwingArcColor = new Color(255, 220, 140, 200),
				SwingArcLife = 12,
				StabImpactSpawnChance = 1.0f,
				StabImpactSize = 0.7f,
				StabImpactColor = new Color(255, 200, 120, 220),
				StabImpactLife = 10,
				SmashRingSpawnChance = 0.5f,
				SmashRingStartRadius = 3f,
				SmashRingEndRadius = 30f,
				SmashRingWidth = 0.5f,
				SmashRingExpandSpeed = 2.5f,
				SmashRingColor = new Color(255, 180, 100, 180),
			});

			// 发光
			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(255, 220, 150),
				GlowLayers = 2,
				GlowBaseScale = 1.2f,
				GlowScaleIncrement = 0.3f,
				GlowBaseAlpha = 0.5f,
				GlowAlphaDecay = 0.15f,
				GlowAlphaMultiplier = 0.3f,
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 16; Projectile.height = 16;
			Projectile.scale = 1f; Projectile.ignoreWater = true;
			Projectile.tileCollide = true; Projectile.penetrate = 2;
			Projectile.timeLeft = 25; Projectile.alpha = 80;
			Projectile.friendly = true; Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnSpawned(IEntitySource source)
		{
			var tb = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
			if (tb != null)
			{
				var tex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTail").Value;
				tb.TrailManager.NewTrail(tex,
					color: new Color(200, 180, 255, 200),
					maxPositions: 12, widthScale: 1.2f, lengthScale: 2.0f, alpha: 0.8f, recordInterval: 1);
			}
		}

		protected override void OnAI()
		{
			Lighting.AddLight(Projectile.Center, 0.6f, 0.4f, 0.2f);
		}

		protected override void OnKilled(int timeLeft)
		{
			MoonBurstHelper.SpawnSmallBurst(Projectile.Center, new Color(255, 200, 120), 0.8f);
			MoonBurstHelper.SpawnSmallBurst(Projectile.Center, new Color(150, 200, 255), 0.5f);
		}
	}
}
