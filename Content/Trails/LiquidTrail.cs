using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace VerminLordMod.Content.Trails
{
	/// <summary>
	/// 液态火焰拖尾 - 原 LiquidTrailManager 风格
	/// 弹幕经过处产生飞溅碎片，模拟液态火焰效果
	/// </summary>
	public class LiquidTrail : ITrail
	{
		// ===== 碎片类 =====
		public class Fragment
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public float Scale;
			public int Life;
			public int MaxLife;
			public float Rotation;
			public float RotSpeed;

			public Fragment(Vector2 position, Vector2 velocity, int life, float scale, float rotSpeed)
			{
				Position = position;
				Velocity = velocity;
				MaxLife = life;
				Life = life;
				Scale = scale;
				RotSpeed = rotSpeed;
				Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
			}
		}

		// ===== 可配置参数 =====

		public string Name { get; set; } = "LiquidTrail";

		/// <summary>最大碎片数量</summary>
		public int MaxFragments { get; set; } = 40;

		/// <summary>碎片存活时间（帧）</summary>
		public int FragmentLife { get; set; } = 30;

		/// <summary>生成间隔（帧）</summary>
		public int SpawnInterval { get; set; } = 2;

		/// <summary>碎片大小倍率</summary>
		public float SizeMultiplier { get; set; } = 1f;

		/// <summary>拖尾贴图（null则使用默认贴图）</summary>
		public Texture2D TrailTexture { get; set; } = null;

		/// <summary>碎片初始颜色（年轻）</summary>
		public Color ColorStart { get; set; } = new Color(255, 80, 30, 255);

		/// <summary>碎片结束颜色（年老）</summary>
		public Color ColorEnd { get; set; } = new Color(60, 10, 20, 0);

		/// <summary>空气阻力系数</summary>
		public float AirResistance { get; set; } = 0.92f;

		/// <summary>浮力系数（正值向上）</summary>
		public float Buoyancy { get; set; } = 0.08f;

		/// <summary>反向惯性系数</summary>
		public float InertiaFactor { get; set; } = 0.25f;

		/// <summary>侧向飞溅系数</summary>
		public float SplashFactor { get; set; } = 0.3f;

		/// <summary>侧向飞溅角度范围</summary>
		public float SplashAngle { get; set; } = 0.8f;

		/// <summary>随机扩散范围</summary>
		public float RandomSpread { get; set; } = 1.2f;

		/// <summary>生成位置偏移</summary>
		public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

		// ===== 内部状态 =====

		private List<Fragment> fragments = new List<Fragment>();
		private int spawnCounter = 0;

		public bool HasContent => fragments.Count > 0;

		public void Update(Vector2 center, Vector2 velocity)
		{
			spawnCounter++;
			if (spawnCounter % SpawnInterval == 0 && fragments.Count < MaxFragments)
			{
				SpawnFragment(center, velocity);
			}

			for (int i = fragments.Count - 1; i >= 0; i--)
			{
				var f = fragments[i];
				f.Velocity *= AirResistance;
				f.Velocity.Y -= Buoyancy;
				f.Position += f.Velocity;
				f.Rotation += f.RotSpeed;
				f.Life--;
				if (f.Life <= 0)
					fragments.RemoveAt(i);
			}
		}

		private void SpawnFragment(Vector2 center, Vector2 velocity)
		{
			Vector2 inertia = -velocity * InertiaFactor;
			Vector2 splash = velocity.RotatedByRandom(SplashAngle) * SplashFactor;
			Vector2 random = Main.rand.NextVector2Circular(RandomSpread, RandomSpread);
			Vector2 vel = inertia + splash + random;
			float scale = Main.rand.NextFloat(0.6f, 1.0f) * SizeMultiplier;
			float rotSpeed = Main.rand.NextFloat(-0.15f, 0.15f);
			fragments.Add(new Fragment(center + SpawnOffset, vel, FragmentLife, scale, rotSpeed));
		}

		public void Draw(SpriteBatch sb)
		{
			if (fragments.Count == 0) return;

			Texture2D tex = TrailTexture;
			if (tex == null) return;

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			var sorted = fragments.OrderBy(f => f.Life);
			foreach (var f in sorted)
			{
				float progress = 1f - (float)f.Life / f.MaxLife;
				Color fragColor = Color.Lerp(ColorStart, ColorEnd, progress);
				float scale = f.Scale * (1f - progress * progress);
				sb.Draw(tex, f.Position - Main.screenPosition, null, fragColor,
					f.Rotation, tex.Size() * 0.5f, scale, SpriteEffects.None, 0);
			}

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}

		public void Clear()
		{
			fragments.Clear();
			spawnCounter = 0;
		}
	}
}
