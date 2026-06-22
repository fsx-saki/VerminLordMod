using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace VerminLordMod.Content.Projectiles.Zero
{
	/// <summary>
	/// 月道模式1 — 月光爆散
	/// 直线飞行，死亡时产生 WindTrail 风格的三层爆散效果。
	/// </summary>
	public class MoonBurstProj : BaseBullet
	{
		private int _burstTimer;
		private readonly List<BurstParticle> _particles = new();

		private class BurstParticle
		{
			public Vector2 Position, Velocity;
			public float Scale, MaxScale, Rotation, RotSpeed, Stretch;
			public int Life, MaxLife, Type;
			public Color Color;
			public float Progress => 1f - (float)Life / MaxLife;
			public float Alpha
			{
				get
				{
					float fi = MathHelper.Min(1f, Progress * 4f);
					float fo = 1f - Progress * Progress;
					return MathHelper.Max(0f, fi * fo);
				}
			}
			public float CurrentScale
			{
				get
				{
					if (Type == 2) return Scale + (MaxScale - Scale) * MathHelper.Min(1f, Progress * 3f);
					return Scale * (1f - Progress * 0.3f);
				}
			}
			public float CurrentStretch => Type == 0 ? Stretch * (1f - Progress * 0.3f) : 1f;
		}

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f)
			{
				AutoRotate = true, RotationOffset = MathHelper.PiOver2,
				EnableLight = true, LightColor = new Vector3(1.8f, 1.9f, 2.0f)
			});
			Behaviors.Add(new TrailBehavior { AutoDraw = true, SuppressDefaultDraw = false });
			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(120, 200, 255), GlowLayers = 3,
				GlowBaseScale = 1.2f, GlowScaleIncrement = 0.4f,
				GlowBaseAlpha = 0.5f, GlowAlphaDecay = 0.15f, GlowAlphaMultiplier = 0.3f,
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 16; Projectile.height = 16;
			Projectile.scale = 1.5f; Projectile.ignoreWater = true;
			Projectile.tileCollide = true; Projectile.penetrate = -1;
			Projectile.timeLeft = 60; Projectile.alpha = 0;
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
				tb.TrailManager.NewTrail(tex, color: new Color(120, 200, 255),
					maxPositions: 16, widthScale: 1f, lengthScale: 1f, alpha: 1f, recordInterval: 2);
			}
		}

		protected override void OnAI()
		{
			if (_burstTimer > 0)
			{
				_burstTimer--; Projectile.timeLeft = System.Math.Max(Projectile.timeLeft, 2);
				Projectile.velocity *= 0.9f;
				for (int i = _particles.Count - 1; i >= 0; i--)
				{
					var p = _particles[i];
					p.Velocity *= 0.95f; p.Position += p.Velocity;
					if (p.Type == 1) p.Rotation += p.RotSpeed;
					if (--p.Life <= 0) _particles.RemoveAt(i);
				}
				if (_burstTimer <= 0) { Projectile.active = false; Projectile.timeLeft = 0; }
				return;
			}
			if (Projectile.timeLeft <= 1) StartBurst();
		}

		protected override bool OnTileCollided(Vector2 oldVelocity) { StartBurst(); return false; }

		private void StartBurst()
		{
			if (_burstTimer > 0) return;
			_burstTimer = 18; Projectile.tileCollide = false;
			var c = Projectile.Center; var r = Main.rand;
			Color col = new Color(160, 210, 255);

			for (int i = 0; i < 20; i++) // Streak
			{
				float a = r.NextFloat(MathHelper.TwoPi); var d = a.ToRotationVector2();
				_particles.Add(new BurstParticle { Position = c + d * r.NextFloat(5, 15), Velocity = d * r.NextFloat(6, 14),
					Scale = r.NextFloat(0.8f, 1.4f), Stretch = r.NextFloat(2.5f, 4.5f), Rotation = a,
					Life = r.Next(8, 16), MaxLife = 16, Type = 0, Color = col * r.NextFloat(0.7f, 1f) });
			}
			for (int i = 0; i < 24; i++) // Vortex
			{
				float a = MathHelper.TwoPi / 24f * i + r.NextFloat(-0.06f, 0.06f); var d = a.ToRotationVector2();
				float o = r.NextFloat(2, 5), s = r.NextFloat(4, 8) * (r.NextBool() ? 1 : -1);
				_particles.Add(new BurstParticle { Position = c + d * r.NextFloat(10, 30),
					Velocity = d * o + d.RotatedBy(MathHelper.PiOver2) * s,
					Scale = r.NextFloat(0.5f, 1f), Rotation = r.NextFloat(MathHelper.TwoPi), RotSpeed = s * 0.02f,
					Life = r.Next(10, 18), MaxLife = 18, Type = 1,
					Color = new Color(180, 230, 255) * r.NextFloat(0.6f, 1f) });
			}
			for (int i = 0; i < 10; i++) // Mist
			{
				var d = r.NextVector2Unit();
				_particles.Add(new BurstParticle { Position = c + d * r.NextFloat(2, 8), Velocity = d * r.NextFloat(0.5f, 2),
					Scale = r.NextFloat(0.6f, 1f), MaxScale = r.NextFloat(2, 3.5f),
					Rotation = r.NextFloat(MathHelper.TwoPi), Life = r.Next(14, 20), MaxLife = 20, Type = 2,
					Color = new Color(140, 200, 255, 120) * r.NextFloat(0.5f, 0.8f) });
			}
			for (int i = 0; i < 6; i++) // Center flash
				_particles.Add(new BurstParticle { Position = c + r.NextVector2Circular(4, 4),
					Velocity = r.NextVector2Circular(0.5f, 0.5f), Scale = r.NextFloat(1.5f, 2.5f),
					Life = r.Next(4, 8), MaxLife = 8, Type = 1,
					Color = new Color(220, 240, 255) * 0.8f });
		}

		public override void PostDraw(Color lightColor)
		{
			if (_particles.Count == 0) return;
			var sb = Main.spriteBatch;
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			var sTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WindTrail/WindTrailStreak").Value;
			var vTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WindTrail/WindTrailVortex").Value;
			var mTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WindTrail/WindTrailMist").Value;

			foreach (var p in _particles)
			{
				var tex = p.Type == 0 ? sTex : p.Type == 1 ? vTex : mTex;
				var o = tex.Size() * 0.5f;
				var pos = p.Position - Main.screenPosition;
				var sc = p.Type == 0 ? new Vector2(p.CurrentStretch, p.CurrentScale) : new Vector2(p.CurrentScale);
				sb.Draw(tex, pos, null, p.Color * p.Alpha, p.Rotation, o, sc, SpriteEffects.None, 0);
			}

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}

		protected override void OnKilled(int timeLeft) => _particles.Clear();
	}
}
