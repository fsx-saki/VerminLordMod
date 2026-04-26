using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content
{
	/// <summary>
	/// 【已废弃】液态拖尾管理器 - 请使用 Content/Trails/ 下的新框架
	/// 
	/// 新框架使用方式：
	///   private TrailManager trailManager = new TrailManager();
	///   trailManager.AddDefaultLiquidTrail(Projectile, colorStart, colorEnd);
	///   trailManager.Update(Projectile.Center, Projectile.velocity);
	///   trailManager.Draw(Main.spriteBatch);
	/// 
	/// 此类仅保留爆炸效果（SpawnExplosion）的兼容层
	/// </summary>
	public class LiquidTrailManager
	{
		public class Fragment
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public float Scale;
			public int Life;
			public int MaxLife;
			public float Rotation;
			public float RotSpeed;

			public Fragment(Vector2 position, Vector2 velocity, int life, float scale, float rotSpeed) {
				Position = position;
				Velocity = velocity;
				MaxLife = life;
				Life = life;
				Scale = scale;
				RotSpeed = rotSpeed;
				Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
			}
		}

		private List<Fragment> fragments = new List<Fragment>();
		private int spawnCounter = 0;

		private class ExplosionEffect
		{
			public List<Fragment> Fragments = new List<Fragment>();
			public Vector2 Center;
			public float SizeMultiplier;
			public Color ColorStart;
			public Color ColorEnd;
			public Texture2D Texture;
			public int TotalLife;
			public int MaxLife;

			public ExplosionEffect(Vector2 center, float sizeMult, Color colorStart, Color colorEnd, Texture2D tex, int life) {
				Center = center;
				SizeMultiplier = sizeMult;
				ColorStart = colorStart;
				ColorEnd = colorEnd;
				Texture = tex;
				TotalLife = life;
				MaxLife = life;
			}

			public bool Update() {
				for (int i = Fragments.Count - 1; i >= 0; i--) {
					var f = Fragments[i];
					f.Velocity *= 0.92f;
					f.Velocity.Y -= 0.08f;
					f.Position += f.Velocity;
					f.Rotation += f.RotSpeed;
					f.Life--;
					if (f.Life <= 0) {
						Fragments.RemoveAt(i);
					}
				}
				TotalLife--;
				return TotalLife > 0 || Fragments.Count > 0;
			}

			public void Draw(SpriteBatch sb) {
				if (Fragments.Count == 0) return;

				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp,
					DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				var sorted = Fragments.OrderBy(f => f.Life);
				foreach (var f in sorted) {
					float progress = 1f - (float)f.Life / f.MaxLife;
					Color fragColor = Color.Lerp(ColorStart, ColorEnd, progress);
					float scale = f.Scale * (1f - progress * progress);

					sb.Draw(Texture, f.Position - Main.screenPosition, null, fragColor,
						f.Rotation, Texture.Size() * 0.5f, scale, SpriteEffects.None, 0);
				}

				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
					DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		private static List<ExplosionEffect> activeExplosions = new List<ExplosionEffect>();

		public IReadOnlyList<Fragment> Fragments => fragments;

		public int MaxFragments { get; set; } = 40;
		public int FragmentLife { get; set; } = 30;
		public int SpawnInterval { get; set; } = 2;
		public float SizeMultiplier { get; set; } = 1f;
		public Texture2D TrailTexture { get; set; } = null;
		public Color ColorStart { get; set; } = new Color(255, 80, 30, 255);
		public Color ColorEnd { get; set; } = new Color(60, 10, 20, 0);
		public float AirResistance { get; set; } = 0.92f;
		public float Buoyancy { get; set; } = 0.08f;
		public float InertiaFactor { get; set; } = 0.25f;
		public float SplashFactor { get; set; } = 0.3f;
		public float SplashAngle { get; set; } = 0.8f;
		public float RandomSpread { get; set; } = 1.2f;
		public Texture2D ExplosionTexture { get; set; } = null;
		public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

		public void SpawnFragment(Vector2 projCenter, Vector2 projVelocity) {
			Vector2 inertia = -projVelocity * InertiaFactor;
			Vector2 splash = projVelocity.RotatedByRandom(SplashAngle) * SplashFactor;
			Vector2 random = Main.rand.NextVector2Circular(RandomSpread, RandomSpread);
			Vector2 vel = inertia + splash + random;
			float scale = Main.rand.NextFloat(0.6f, 1.0f) * SizeMultiplier;
			float rotSpeed = Main.rand.NextFloat(-0.15f, 0.15f);
			fragments.Add(new Fragment(projCenter + SpawnOffset, vel, FragmentLife, scale, rotSpeed));
		}

		public void Update(Vector2 projCenter, Vector2 projVelocity) {
			spawnCounter++;
			if (spawnCounter % SpawnInterval == 0 && fragments.Count < MaxFragments) {
				SpawnFragment(projCenter, projVelocity);
			}
			for (int i = fragments.Count - 1; i >= 0; i--) {
				var f = fragments[i];
				f.Velocity *= AirResistance;
				f.Velocity.Y -= Buoyancy;
				f.Position += f.Velocity;
				f.Rotation += f.RotSpeed;
				f.Life--;
				if (f.Life <= 0) {
					fragments.RemoveAt(i);
				}
			}
		}

		public bool Draw(SpriteBatch sb, Projectile proj, Texture2D trailTex = null, float fragScale = 1f) {
			if (fragments.Count == 0)
				return false;

			Texture2D tex = trailTex ?? TextureAssets.Projectile[proj.type].Value;

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			var sorted = fragments.OrderBy(f => f.Life);

			foreach (var f in sorted) {
				float progress = 1f - (float)f.Life / f.MaxLife;
				Color fragColor = Color.Lerp(ColorStart, ColorEnd, progress);
				float scale = f.Scale * (1f - progress * progress) * fragScale;

				sb.Draw(tex, f.Position - Main.screenPosition, null, fragColor,
					f.Rotation, tex.Size() * 0.5f, scale, SpriteEffects.None, 0);
			}

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			return true;
		}

		public void Clear() {
			fragments.Clear();
		}

		public void SpawnExplosion(Vector2 center, int count = 20, float speed = 4f, int? life = null, float scale = 1f, float skipChance = 0f) {
			int lifeTime = life ?? FragmentLife;
			Texture2D tex = ExplosionTexture ?? TrailTexture ?? TextureAssets.Projectile[ProjectileID.HolyArrow].Value;
			var explosion = new ExplosionEffect(center, scale, ColorStart, ColorEnd, tex, lifeTime * 2);

			for (int i = 0; i < count; i++) {
				if (skipChance > 0 && Main.rand.NextFloat() < skipChance)
					continue;
				Vector2 vel = Main.rand.NextVector2Circular(speed, speed);
				float fragScale = Main.rand.NextFloat(0.6f, 1.0f) * SizeMultiplier;
				float rotSpeed = Main.rand.NextFloat(-0.2f, 0.2f);
				explosion.Fragments.Add(new Fragment(center, vel, lifeTime, fragScale, rotSpeed));
			}

			activeExplosions.Add(explosion);
		}

		public static void UpdateAll() {
			for (int i = activeExplosions.Count - 1; i >= 0; i--) {
				if (!activeExplosions[i].Update()) {
					activeExplosions.RemoveAt(i);
				}
			}
		}

		public static void DrawAll(SpriteBatch sb) {
			foreach (var exp in activeExplosions) {
				exp.Draw(sb);
			}
		}
	}

	// ============================================================
	// 【已废弃】以下三个管理器已不再使用
	// 请使用 Content/Trails/ 下的新框架
	// 如需自定义拖尾，请实现 ITrail 接口
	// ============================================================

	///*
	///// <summary>
	///// 【已废弃】寒冰拖尾管理器 - 请使用新框架
	///// </summary>
	//public class IceTrailManager
	//{
	//	public class IceShard
	//	{
	//		public Vector2 Position;
	//		public Vector2 Velocity;
	//		public float Size;
	//		public float Life;
	//		public float MaxLife;
	//		public float Rotation;
	//		public float RotSpeed;
	//		public float SpawnAge;
	//		public int Generation;
	//		public float SparkleSeed;
	//		public float FrostSeed;

	//		public IceShard(Vector2 position, Vector2 velocity, int life, float size, float rotSpeed, int gen) {
	//			Position = position;
	//			Velocity = velocity;
	//			MaxLife = life;
	//			Life = life;
	//			Size = size;
	//			RotSpeed = rotSpeed;
	//			Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
	//			SpawnAge = 0;
	//			Generation = gen;
	//			SparkleSeed = Main.rand.NextFloat(100f);
	//			FrostSeed = Main.rand.NextFloat(10f);
	//		}
	//	}

	//	private List<IceShard> shards = new List<IceShard>();
	//	private int spawnCounter = 0;
	//	private int frameCounter = 0;

	//	public IReadOnlyList<IceShard> Shards => shards;

	//	public int MaxShards { get; set; } = 120;
	//	public int ShardLife { get; set; } = 25;
	//	public int SpawnInterval { get; set; } = 1;
	//	public float SizeMultiplier { get; set; } = 0.5f;
	//	public Texture2D TrailTexture { get; set; } = null;

	//	public Color FrostColor { get; set; } = new Color(180, 230, 255, 200);
	//	public Color CoreColor { get; set; } = new Color(220, 250, 255, 180);
	//	public Color HighlightColor { get; set; } = new Color(255, 255, 255, 255);

	//	public float Gravity { get; set; } = 0.08f;
	//	public float AirResistance { get; set; } = 0.95f;
	//	public float ShatterProbability { get; set; } = 0.12f;
	//	public int MaxGeneration { get; set; } = 2;

	//	public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

	//	public void SpawnShard(Vector2 projCenter, Vector2 projVelocity) {
	//		float speed = projVelocity.Length();
	//		float baseAngle = (float)Math.Atan2(-projVelocity.Y, -projVelocity.X);
	//		float spreadAngle = baseAngle + Main.rand.NextFloat(-0.6f, 0.6f);
	//		float backSpeed = Main.rand.NextFloat(0.15f, 0.35f) * speed;
	//		Vector2 vel = new Vector2((float)Math.Cos(spreadAngle), (float)Math.Sin(spreadAngle)) * backSpeed;
	//		vel += Main.rand.NextVector2Circular(0.3f, 0.3f);
	//		float size = Main.rand.NextFloat(0.4f, 0.7f) * SizeMultiplier;
	//		float rotSpeed = Main.rand.NextFloat(-0.08f, 0.08f);
	//		shards.Add(new IceShard(projCenter + SpawnOffset, vel, ShardLife, size, rotSpeed, 0));
	//	}

	//	public void Update(Vector2 projCenter, Vector2 projVelocity) {
	//		frameCounter++;
	//		spawnCounter++;
	//		if (spawnCounter % SpawnInterval == 0 && shards.Count < MaxShards) {
	//			SpawnShard(projCenter, projVelocity);
	//		}
	//		for (int i = shards.Count - 1; i >= 0; i--) {
	//			var s = shards[i];
	//			s.SpawnAge++;
	//			s.Life--;
	//			s.Velocity *= AirResistance;
	//			s.Velocity.Y += Gravity;
	//			s.Position += s.Velocity;
	//			s.Rotation += s.RotSpeed;
	//			float ageRatio = s.SpawnAge / s.MaxLife;
	//			if (ageRatio > 0.5f && s.Generation < MaxGeneration) {
	//				float shatterChance = ShatterProbability * (ageRatio - 0.5f) * 2f;
	//				if (Main.rand.NextFloat() < shatterChance) {
	//					int pieces = Main.rand.Next(2, 4);
	//					for (int j = 0; j < pieces; j++) {
	//						Vector2 pieceVel = Main.rand.NextVector2Circular(1.5f, 1.5f);
	//						pieceVel += s.Velocity * 0.3f;
	//						pieceVel.Y += 0.5f;
	//						float pieceSize = s.Size * Main.rand.NextFloat(0.25f, 0.4f);
	//						float rotSpeed = Main.rand.NextFloat(-0.15f, 0.15f);
	//						int newLife = (int)(s.Life * Main.rand.NextFloat(0.3f, 0.5f));
	//						shards.Add(new IceShard(s.Position, pieceVel, newLife, pieceSize, rotSpeed, s.Generation + 1));
	//					}
	//					shards.RemoveAt(i);
	//					continue;
	//				}
	//			}
	//			if (s.Life <= 0) {
	//				if (s.Generation < MaxGeneration && Main.rand.NextFloat() < 0.5f) {
	//					int pieces = Main.rand.Next(1, 3);
	//					for (int j = 0; j < pieces; j++) {
	//						Vector2 pieceVel = Main.rand.NextVector2Circular(1.5f, 1.5f);
	//						pieceVel.Y += 0.5f;
	//						float pieceSize = s.Size * 0.25f;
	//						shards.Add(new IceShard(s.Position, pieceVel, 10, pieceSize, 0, s.Generation + 1));
	//					}
	//				}
	//				shards.RemoveAt(i);
	//			}
	//		}
	//	}

	//	public void AddShard(Vector2 position, Vector2 velocity, int life, float size, float rotSpeed, int gen = 0) {
	//		if (shards.Count < MaxShards + 40) {
	//			shards.Add(new IceShard(position, velocity, life, size, rotSpeed, gen));
	//		}
	//	}

	//	public void DeathBurst(Vector2 center, int bigCount = 20, int smallCount = 15) {
	//		for (int i = 0; i < bigCount; i++) {
	//			Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
	//			vel.Y += 1f;
	//			float size = Main.rand.NextFloat(0.4f, 0.8f) * SizeMultiplier;
	//			AddShard(center, vel, 20, size, Main.rand.NextFloat(-0.2f, 0.2f), 0);
	//		}
	//		for (int i = 0; i < smallCount; i++) {
	//			Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
	//			vel.Y += 2f;
	//			float size = Main.rand.NextFloat(0.15f, 0.3f) * SizeMultiplier;
	//			AddShard(center, vel, 12, size, Main.rand.NextFloat(-0.3f, 0.3f), 1);
	//		}
	//	}

	//	public bool Draw(SpriteBatch sb, Projectile proj, Texture2D trailTex = null, float fragScale = 1f) {
	//		if (shards.Count == 0) return false;
	//		Texture2D tex = trailTex ?? TextureAssets.Projectile[proj.type].Value;
	//		var sorted = shards.OrderBy(s => s.Life).ToList();
	//		sb.End();
	//		sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp,
	//			DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	//		foreach (var s in sorted) {
	//			float lifeRatio = s.Life / s.MaxLife;
	//			float frostVar = (float)Math.Sin(s.FrostSeed * 2f) * 0.2f + 0.8f;
	//			float glowAlpha = lifeRatio * 0.25f * frostVar;
	//			Color glowColor = new Color(150, 210, 255, 0) * glowAlpha;
	//			float glowScale = s.Size * 2.5f * fragScale;
	//			sb.Draw(tex, s.Position - Main.screenPosition, null, glowColor,
	//				s.Rotation, tex.Size() * 0.5f, glowScale, SpriteEffects.None, 0);
	//		}
	//		sb.End();
	//		sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
	//			DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	//		foreach (var s in sorted) {
	//			float lifeRatio = s.Life / s.MaxLife;
	//			float ageRatio = s.SpawnAge / s.MaxLife;
	//			float alpha = lifeRatio * (1f - ageRatio * 0.5f) * 0.7f;
	//			Color coreColor = Color.Lerp(CoreColor, FrostColor, ageRatio);
	//			coreColor *= alpha;
	//			float coreScale = s.Size * fragScale;
	//			sb.Draw(tex, s.Position - Main.screenPosition, null, coreColor,
	//				s.Rotation, tex.Size() * 0.5f, coreScale, SpriteEffects.None, 0);
	//		}
	//		sb.End();
	//		sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp,
	//			DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	//		foreach (var s in sorted) {
	//			float lifeRatio = s.Life / s.MaxLife;
	//			float sparkle = (float)Math.Sin(frameCounter * 0.5f + s.SparkleSeed * 3f) * 0.5f + 0.5f;
	//			sparkle = (float)Math.Pow(sparkle, 3);
	//			float highlightAlpha = lifeRatio * sparkle * 0.6f;
	//			Color highlightColor = HighlightColor * highlightAlpha;
	//			float highlightScale = s.Size * 0.25f * fragScale;
	//			sb.Draw(tex, s.Position - Main.screenPosition, null, highlightColor,
	//				0f, tex.Size() * 0.5f, highlightScale, SpriteEffects.None, 0);
	//		}
	//		sb.End();
	//		sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
	//			DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	//		return true;
	//	}

	//	public void Clear() {
	//		shards.Clear();
	//	}
	//}

	///// <summary>
	///// 【已废弃】星系拖尾管理器 - 请使用新框架
	///// </summary>
	//public class GalaxyTrailManager
	//{
	//	public class StarPoint
	//	{
	//		public Vector2 Position;
	//		public Vector2 Velocity;
	//		public float Life;
	//		public float MaxLife;
	//		public float Size;
	//		public float Brightness;

	//		public StarPoint(Vector2 position, Vector2 velocity, int life, float size) {
	//			Position = position;
	//			Velocity = velocity;
	//			MaxLife = life;
	//			Life = life;
	//			Size = size;
	//			Brightness = 1f;
	//		}
	//	}

	//	private List<StarPoint> points = new List<StarPoint>();
	//	private int spawnCounter = 0;

	//	public IReadOnlyList<StarPoint> Points => points;

	//	public int MaxPoints { get; set; } = 50;
	//	public int PointLife { get; set; } = 120;
	//	public int SpawnInterval { get; set; } = 2;
	//	public float SizeMultiplier { get; set; } = 1f;
	//	public float LineMaxDistance { get; set; } = 20f;
	//	public int MaxLineCount { get; set; } = 30;
	//	public float FadeDelayRatio { get; set; } = 0.4f;

	//	public Color ColorCore { get; set; } = new Color(255, 255, 200, 255);
	//	public Color ColorEdge { get; set; } = new Color(100, 80, 200, 0);

	//	public float AirResistance { get; set; } = 0.995f;
	//	public float DriftSpeed { get; set; } = 0.008f;

	//	public Vector2 SpawnOffset { get; set; } = Vector2.Zero;
	//	public Texture2D TrailTexture { get; set; } = null;

	//	public void SpawnPoint(Vector2 projCenter, Vector2 projVelocity) {
	//		Vector2 drift = projVelocity * -0.15f;
	//		drift += Main.rand.NextVector2Circular(0.5f, 0.5f);
	//		float size = Main.rand.NextFloat(0.3f, 0.6f) * SizeMultiplier;
	//		points.Add(new StarPoint(projCenter + SpawnOffset, drift, PointLife, size));
	//	}

	//	public void Update(Vector2 projCenter, Vector2 projVelocity) {
	//		spawnCounter++;
	//		if (spawnCounter % SpawnInterval == 0 && points.Count < MaxPoints) {
	//			SpawnPoint(projCenter, projVelocity);
	//		}
	//		for (int i = points.Count - 1; i >= 0; i--) {
	//			var p = points[i];
	//			if (p.Velocity.Length() > 4f) {
	//				p.Velocity.Normalize();
	//				p.Velocity *= 4f;
	//			}
	//			p.Velocity *= AirResistance;
	//			p.Position += p.Velocity;
	//			p.Life--;
	//			float progress = 1f - p.Life / p.MaxLife;
	//			if (progress < FadeDelayRatio) {
	//				p.Brightness = 1f;
	//			} else {
	//				float fadeProgress = (progress - FadeDelayRatio) / (1f - FadeDelayRatio);
	//				p.Brightness = MathHelper.Lerp(1f, 0f, fadeProgress * fadeProgress);
	//			}
	//			if (p.Life <= 0) {
	//				points.RemoveAt(i);
	//			}
	//		}
	//	}

	//	public bool Draw(SpriteBatch sb, Projectile proj, Texture2D trailTex = null) {
	//		if (points.Count < 2) return false;
	//		Texture2D tex = trailTex ?? TextureAssets.Projectile[proj.type].Value;
	//		sb.End();
	//		sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp,
	//			DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	//		int lineCount = 0;
	//		for (int i = 1; i < points.Count && lineCount < MaxLineCount; i++) {
	//			var p0 = points[i - 1];
	//			var p1 = points[i];
	//			float dist = (p1.Position - p0.Position).Length();
	//			if (dist > 0 && dist < LineMaxDistance) {
	//				float alpha0 = p0.Brightness;
	//				float alpha1 = p1.Brightness;
	//				float alpha = alpha0 * alpha1 * 0.35f;
	//				alpha *= (1f - dist / LineMaxDistance);
	//				Color lineColor = Color.Lerp(ColorCore, ColorEdge, (p0.Life / p0.MaxLife + p1.Life / p1.MaxLife) * 0.5f);
	//				lineColor *= alpha;
	//				DrawLine(sb, p0.Position - Main.screenPosition, p1.Position - Main.screenPosition, lineColor, 0.4f);
	//				lineCount++;
	//			}
	//		}
	//		foreach (var p in points) {
	//			float progress = 1f - p.Life / p.MaxLife;
	//			Color starColor = Color.Lerp(ColorCore, ColorEdge, progress);
	//			starColor *= p.Brightness;
	//			float scale = p.Size;
	//			sb.Draw(tex, p.Position - Main.screenPosition, null, starColor,
	//				0f, tex.Size() * 0.5f, scale, SpriteEffects.None, 0);
	//		}
	//		sb.End();
	//		sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
	//			DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	//		return true;
	//	}

	//	private void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, Color color, float thickness) {
	//		float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
	//		float length = (end - start).Length();
	//		sb.Draw(TextureAssets.MagicPixel.Value, start, null, color,
	//			angle, new Vector2(0, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0);
	//	}

	//	public void Clear() {
	//		points.Clear();
	//	}
	//}

	///// <summary>
	///// 【已废弃】风拖尾管理器 - 请使用新框架
	///// </summary>
	//public class WindTrailManager
	//{
	//	public class WindSegment
	//	{
	//		public Vector2 Position;
	//		public Vector2 PreviousPosition;
	//		public float Width;
	//		public float Life;
	//		public float MaxLife;
	//		public float Curve;

	//		public WindSegment(Vector2 position, Vector2 prevPosition, float width, int life) {
	//			Position = position;
	//			PreviousPosition = prevPosition;
	//			Width = width;
	//			MaxLife = life;
	//			Life = life;
	//			Curve = 0f;
	//		}
	//	}

	//	private List<WindSegment> segments = new List<WindSegment>();
	//	private int spawnCounter = 0;

	//	public IReadOnlyList<WindSegment> Segments => segments;

	//	public int MaxSegments { get; set; } = 120;
	//	public int SegmentLife { get; set; } = 60;
	//	public int SpawnInterval { get; set; } = 1;
	//	public float WidthMultiplier { get; set; } = 1f;
	//	public float InitialWidth { get; set; } = 8f;
	//	public float FinalWidth { get; set; } = 20f;

	//	public Color ColorStart { get; set; } = new Color(255, 255, 255, 180);
	//	public Color ColorEnd { get; set; } = new Color(200, 220, 255, 0);

	//	public float WindSpeed { get; set; } = 1f;
	//	public float CurveStrength { get; set; } = 0.3f;
	//	public float SpreadFactor { get; set; } = 0.5f;

	//	public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

	//	public void SpawnSegment(Vector2 projCenter, Vector2 projVelocity) {
	//		float width = InitialWidth * WidthMultiplier;
	//		int life = SegmentLife;
	//		segments.Add(new WindSegment(projCenter + SpawnOffset, projCenter + SpawnOffset, width, life));
	//	}

	//	public void Update(Vector2 projCenter, Vector2 projVelocity) {
	//		spawnCounter++;
	//		if (spawnCounter % SpawnInterval == 0 && segments.Count < MaxSegments) {
	//			SpawnSegment(projCenter, projVelocity);
	//		}
	//		for (int i = segments.Count - 1; i >= 0; i--) {
	//			var seg = segments[i];
	//			float progress = 1f - seg.Life / seg.MaxLife;
	//			seg.Width = MathHelper.Lerp(InitialWidth * WidthMultiplier, FinalWidth * WidthMultiplier, progress);
	//			if (segments.Count > 1 && i < segments.Count - 1) {
	//				var next = segments[i + 1];
	//				Vector2 dir = next.Position - seg.Position;
	//				float perpX = -dir.Y;
	//				float perpY = dir.X;
	//				float perpLen = (float)Math.Sqrt(perpX * perpX + perpY * perpY);
	//				if (perpLen > 0) {
	//					perpX /= perpLen;
	//					perpY /= perpLen;
	//				}
	//				float curveOffset = (float)Math.Sin(seg.Life * 0.1f + i * 0.5f) * CurveStrength * (1f - progress);
	//			 seg.Position.X += perpX * curveOffset;
	//			 seg.Position.Y += perpY * curveOffset;
	//			}
	//			seg.PreviousPosition = seg.Position;
	//			seg.Position += projVelocity * 0.1f;
	//			seg.Life--;
	//			if (seg.Life <= 0) {
	//				segments.RemoveAt(i);
	//			}
	//		}
	//	}

	//	public bool Draw(SpriteBatch sb, Projectile proj, Texture2D trailTex = null) {
	//		if (segments.Count < 2) return false;
	//		Texture2D tex = trailTex ?? TextureAssets.Projectile[proj.type].Value;
	//		sb.End();
	//		sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp,
	//			DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	//		for (int i = 1; i < segments.Count; i++) {
	//			var prev = segments[i - 1];
	//			var curr = segments[i];
	//			float progress = 1f - curr.Life / curr.MaxLife;
	//			Color segColor = Color.Lerp(ColorStart, ColorEnd, progress);
	//			float width = curr.Width;
	//			DrawCurvedSegment(sb, prev.Position - Main.screenPosition, curr.Position - Main.screenPosition, segColor, width);
	//		}
	//		sb.End();
	//		sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
	//			DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
	//		return true;
	//	}

	//	private void DrawCurvedSegment(SpriteBatch sb, Vector2 start, Vector2 end, Color color, float width) {
	//		Vector2 dir = end - start;
	//		float length = dir.Length();
	//		if (length < 1f) return;
	//		float angle = (float)Math.Atan2(dir.Y, dir.X);
	//		Rectangle rect = new Rectangle(0, 0, (int)length, (int)width);
	//		sb.Draw(TextureAssets.MagicPixel.Value, start, rect, color, angle, new Vector2(0, 0.5f), 1f, SpriteEffects.None, 0);
	//	}

	//	public void Clear() {
	//		segments.Clear();
	//	}
	//}
	//*/
}
