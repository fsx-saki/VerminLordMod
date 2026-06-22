using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;

namespace VerminLordMod.Content.Projectiles
{
	/// <summary>
	/// 月光弹幕 — 死亡时产生 WindTrail 风格的三层爆散效果
	/// </summary>
	public class MoonlightProj : BaseBullet
	{
		// ===== 爆散状态 =====
		/// <summary>爆散计时器，>0 时进入爆散绘制阶段</summary>
		private int _burstTimer;
		/// <summary>爆散粒子列表</summary>
		private readonly List<BurstParticle> _particles = new();

		private class BurstParticle
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public float Scale;
			public float MaxScale;
			public float Rotation;
			public float RotSpeed;
			public float Stretch;
			public int Life;
			public int MaxLife;
			public int Type; // 0=Streak, 1=Vortex, 2=Mist
			public Color Color;
			public float Progress => 1f - (float)Life / MaxLife;
			public float Alpha
			{
				get
				{
					float fadeIn = MathF.Min(1f, Progress * 4f);
					float fadeOut = 1f - Progress * Progress;
					return MathF.Max(0f, fadeIn * fadeOut);
				}
			}
			public float CurrentScale
			{
				get
				{
					if (Type == 2) // Mist 膨胀
						return Scale + (MaxScale - Scale) * MathF.Min(1f, Progress * 3f);
					return Scale * (1f - Progress * 0.3f);
				}
			}
			public float CurrentStretch => Type == 0 ? Stretch * (1f - Progress * 0.3f) : 1f;
		}

		protected override void RegisterBehaviors()
		{
			Behaviors.Add(new AimBehavior(speed: 0f)
			{
				AutoRotate = true,
				RotationOffset = MathHelper.PiOver2,
				EnableLight = true,
				LightColor = new Vector3(1.8f, 1.9f, 2.0f)
			});

			var trailBehavior = new TrailBehavior
			{
				AutoDraw = true,
				SuppressDefaultDraw = false
			};
			Behaviors.Add(trailBehavior);

			Behaviors.Add(new GlowDrawBehavior
			{
				GlowColor = new Color(120, 200, 255),
				GlowLayers = 3,
				GlowBaseScale = 1.2f,
				GlowScaleIncrement = 0.4f,
				GlowBaseAlpha = 0.5f,
				GlowAlphaDecay = 0.15f,
				GlowAlphaMultiplier = 0.3f,
				EnableLight = false
			});
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.scale = 1.5f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Projectile.aiStyle = -1;
		}

		protected override void OnSpawned(IEntitySource source)
		{
			var trailBehavior = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
			if (trailBehavior != null)
			{
				Texture2D trailTex = ModContent.Request<Texture2D>(
					"VerminLordMod/Content/Projectiles/MoonlightProjTail").Value;
				trailBehavior.TrailManager.NewTrail(trailTex,
					color: new Color(120, 200, 255),
					maxPositions: 16,
					widthScale: 1f,
					lengthScale: 1f,
					alpha: 1f,
					recordInterval: 2,
					enableGlow: false);
			}
		}

		protected override void OnAI()
		{
			if (_burstTimer > 0)
			{
				_burstTimer--;
				// 爆散期间保持存活
				Projectile.timeLeft = Math.Max(Projectile.timeLeft, 2);
				// 不移动
				Projectile.velocity *= 0.9f;
				// 更新粒子
				for (int i = _particles.Count - 1; i >= 0; i--)
				{
					var p = _particles[i];
					p.Velocity *= 0.95f;
					p.Position += p.Velocity;
					if (p.Type == 1) p.Rotation += p.RotSpeed;
					p.Life--;
					if (p.Life <= 0) _particles.RemoveAt(i);
				}
				// 爆散结束，真正死亡
				if (_burstTimer <= 0)
				{
					Projectile.active = false;
					Projectile.timeLeft = 0;
				}
				return;
			}

			// timeLeft 即将归零时触发爆散
			if (Projectile.timeLeft <= 1)
			{
				StartBurst();
			}
		}

		/// <summary> 碰触物块时触发爆散 </summary>
		protected override bool OnTileCollided(Vector2 oldVelocity)
		{
			StartBurst();
			return false; // 不立即销毁，进入爆散阶段
		}

		private void StartBurst()
		{
			if (_burstTimer > 0) return; // 已在爆散中
			_burstTimer = 18; // 爆散持续 18 帧

			Projectile.tileCollide = false;
			Projectile.penetrate = Projectile.penetrate; // 保持穿透不减少

			Vector2 center = Projectile.Center;
			Color moonColor = new Color(160, 210, 255);
			var rand = Main.rand;

			// === Streak 层：向外高速射出的光痕 ===
			for (int i = 0; i < 20; i++)
			{
				float angle = rand.NextFloat(MathHelper.TwoPi);
				Vector2 dir = angle.ToRotationVector2();
				float speed = rand.NextFloat(6f, 14f);
				_particles.Add(new BurstParticle
				{
					Position = center + dir * rand.NextFloat(5f, 15f),
					Velocity = dir * speed,
					Scale = rand.NextFloat(0.8f, 1.4f),
					Stretch = rand.NextFloat(2.5f, 4.5f),
					Rotation = angle,
					Life = rand.Next(8, 16),
					MaxLife = 16,
					Type = 0,
					Color = moonColor * rand.NextFloat(0.7f, 1f),
				});
			}

			// === Vortex 层：旋转扩散的光旋 ===
			for (int i = 0; i < 24; i++)
			{
				float angle = MathHelper.TwoPi / 24f * i + rand.NextFloat(-0.06f, 0.06f);
				Vector2 dir = angle.ToRotationVector2();
				float outward = rand.NextFloat(2f, 5f);
				float spin = rand.NextFloat(4f, 8f) * (rand.NextBool() ? 1f : -1f);
				_particles.Add(new BurstParticle
				{
					Position = center + dir * rand.NextFloat(10f, 30f),
					Velocity = dir * outward + dir.RotatedBy(MathHelper.PiOver2) * spin,
					Scale = rand.NextFloat(0.5f, 1.0f),
					Rotation = rand.NextFloat(MathHelper.TwoPi),
					RotSpeed = spin * 0.02f,
					Life = rand.Next(10, 18),
					MaxLife = 18,
					Type = 1,
					Color = new Color(180, 230, 255) * rand.NextFloat(0.6f, 1f),
				});
			}

			// === Mist 层：缓慢膨胀的光雾 ===
			for (int i = 0; i < 10; i++)
			{
				Vector2 dir = rand.NextVector2Unit();
				float speed = rand.NextFloat(0.5f, 2f);
				_particles.Add(new BurstParticle
				{
					Position = center + dir * rand.NextFloat(2f, 8f),
					Velocity = dir * speed,
					Scale = rand.NextFloat(0.6f, 1.0f),
					MaxScale = rand.NextFloat(2.0f, 3.5f),
					Rotation = rand.NextFloat(MathHelper.TwoPi),
					Life = rand.Next(14, 20),
					MaxLife = 20,
					Type = 2,
					Color = new Color(140, 200, 255, 120) * rand.NextFloat(0.5f, 0.8f),
				});
			}

			// === 中心光爆 ===
			for (int i = 0; i < 6; i++)
			{
				_particles.Add(new BurstParticle
				{
					Position = center + rand.NextVector2Circular(4f, 4f),
					Velocity = rand.NextVector2Circular(0.5f, 0.5f),
					Scale = rand.NextFloat(1.5f, 2.5f),
					Rotation = 0,
					Life = rand.Next(4, 8),
					MaxLife = 8,
					Type = 1,
					Color = new Color(220, 240, 255) * 0.8f,
				});
			}
		}

		public override void PostDraw(Color lightColor)
		{
			// 没有爆散粒子时跳过
			if (_particles.Count == 0) return;

			// 爆散粒子使用 Additive 混合绘制
			var sb = Main.spriteBatch;
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
				SamplerState.AnisotropicClamp, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			// 加载 WindTrail 纹理
			Texture2D streakTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WindTrail/WindTrailStreak").Value;
			Texture2D vortexTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WindTrail/WindTrailVortex").Value;
			Texture2D mistTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WindTrail/WindTrailMist").Value;

			foreach (var p in _particles)
			{
				Texture2D tex;
				Vector2 origin;
				switch (p.Type)
				{
					case 0: tex = streakTex; origin = streakTex.Size() * 0.5f; break;
					case 1: tex = vortexTex; origin = vortexTex.Size() * 0.5f; break;
					default: tex = mistTex; origin = mistTex.Size() * 0.5f; break;
				}

				Color drawColor = p.Color * p.Alpha;
				Vector2 pos2 = p.Position - Main.screenPosition;
				Vector2 scale = p.Type == 0
					? new Vector2(p.CurrentStretch, p.CurrentScale)
					: new Vector2(p.CurrentScale);

				sb.Draw(tex, pos2, null, drawColor, p.Rotation, origin, scale, SpriteEffects.None, 0f);
			}

			// 切回正常混合
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				SamplerState.AnisotropicClamp, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}

		protected override void OnKilled(int timeLeft)
		{
			_particles.Clear();
		}
	}
}
