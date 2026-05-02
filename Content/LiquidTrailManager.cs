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
	/// 爆炸效果管理器（原 LiquidTrailManager 的兼容层）
	///
	/// 【拖尾功能已废弃】请使用 Content/Trails/ 下的新框架：
	///   private TrailManager trailManager = new TrailManager();
	///   trailManager.AddDefaultLiquidTrail(Projectile, colorStart, colorEnd);
	///   trailManager.Update(Projectile.Center, Projectile.velocity);
	///   trailManager.Draw(Main.spriteBatch);
	///
	/// 【爆炸效果仍在使用中】SpawnExplosion() 方法被以下投射物用于死亡爆炸效果：
	///   - LiquidFlameProjectile
	///   - StarfireProj
	///   - StarfireArcProj
	/// 通过静态 activeExplosions 列表 + UpdateAll()/DrawAll() 统一管理生命周期和渲染。
	/// VerminLordModSystem.PostUpdateEverything / PostDrawInterface 中调用了静态方法。
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
	// 【已清理】以下废弃管理器代码已被移除：
	//   - IceTrailManager（寒冰拖尾）
	//   - GalaxyTrailManager（星系拖尾）
	//   - WindTrailManager（风拖尾）
	// 如需自定义拖尾，请使用 Content/Trails/ 下的新框架并实现 ITrail 接口
	// ============================================================
}
