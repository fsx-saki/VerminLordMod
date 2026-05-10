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

		/// <summary>
		/// 拖尾需要的混合模式。
		/// 由 ITrail 接口声明，TrailManager 自动处理切换。
		/// LiquidTrail 默认使用 Additive 混合模式。
		/// </summary>
		public BlendState BlendMode => BlendState.Additive;

		/// <summary>最大碎片数量</summary>
		public int MaxFragments { get; set; } = 40;

		/// <summary>碎片存活时间（帧）</summary>
		public int FragmentLife { get; set; } = 30;

		/// <summary>基础生成间隔（帧），速度慢时的间隔</summary>
		public int SpawnInterval { get; set; } = 2;

		/// <summary>
		/// 是否启用速度自适应密度。
		/// 启用后，速度快时每帧生成多个碎片，避免拖尾断裂。
		/// </summary>
		public bool AdaptiveDensity { get; set; } = true;

		/// <summary>
		/// 速度自适应密度阈值下限（像素/帧）。
		/// 速度低于此值时使用 SpawnInterval。
		/// </summary>
		public float AdaptiveSpeedThreshold { get; set; } = 4f;

		/// <summary>
		/// 速度自适应密度系数。
		/// 每达到此速度值，每帧多生成一个碎片。
		/// 例如：系数=6f，速度=18 → 每帧生成 18/6=3 个碎片
		/// </summary>
		public float AdaptiveDensityFactor { get; set; } = 6f;

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

		/// <summary>
		/// 是否启用速度自适应存活时间。
		/// 启用后，速度快时碎片存活时间缩短，保持拖尾总长度大致恒定。
		/// 拖尾目标长度 ≈ AdaptiveTargetLength（像素）。
		/// </summary>
		public bool AdaptiveLife { get; set; } = true;

		/// <summary>
		/// 拖尾目标长度（像素）。
		/// 自适应存活时间模式下，拖尾长度 ≈ 此值。
		/// 设为 0 表示不限制。
		/// </summary>
		public float AdaptiveTargetLength { get; set; } = 60f;

		/// <summary>
		/// 速度对存活时间的影响指数（0~1）。
		/// 0 = 速度不影响存活时间（完全使用 FragmentLife）
		/// 1 = 完全线性影响（currentLife = targetLength / speed）
		/// 推荐值 0.3~0.5，让快速移动时拖尾变长但不至于断裂。
		/// </summary>
		public float SpeedLifeExponent { get; set; } = 0.4f;

		/// <summary>碎片存活时间下限（帧），防止存活时间过短导致拖尾消失</summary>
		public int MinFragmentLife { get; set; } = 4;

		// ===== 内部状态 =====

		private List<Fragment> fragments = new List<Fragment>();
		private int spawnCounter = 0;

		public bool HasContent => fragments.Count > 0;

		public void Update(Vector2 center, Vector2 velocity)
		{
			float speed = velocity.Length();

			// 计算当前帧应使用的碎片存活时间
			int currentLife = FragmentLife;
			if (AdaptiveLife && AdaptiveTargetLength > 0f && speed > 0.5f)
			{
				// 拖尾长度 ≈ speed^SpeedLifeExponent * currentLife
				// SpeedLifeExponent 控制速度对存活时间的影响强度
				// SpeedLifeExponent=0.4 时，速度翻倍存活时间只缩短约 24%
				// 相比原来的线性缩短（速度翻倍存活时间减半），快速移动时拖尾明显更长
				float effectiveSpeed = MathF.Pow(speed, SpeedLifeExponent);
				currentLife = (int)(AdaptiveTargetLength / effectiveSpeed);
				currentLife = Math.Clamp(currentLife, MinFragmentLife, FragmentLife);
			}

			if (AdaptiveDensity && speed > AdaptiveSpeedThreshold)
			{
				// 速度自适应模式：速度快时每帧生成多个碎片
				int spawnCount = (int)(speed / AdaptiveDensityFactor);
				spawnCount = Math.Clamp(spawnCount, 1, 5); // 限制每帧最多5个，防止性能问题
				for (int i = 0; i < spawnCount; i++)
				{
					if (fragments.Count < MaxFragments)
						SpawnFragment(center, velocity, currentLife);
				}
			}
			else
			{
				// 传统固定间隔模式
				spawnCounter++;
				if (spawnCounter % SpawnInterval == 0 && fragments.Count < MaxFragments)
				{
					SpawnFragment(center, velocity, currentLife);
				}
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

		private void SpawnFragment(Vector2 center, Vector2 velocity, int life)
		{
			Vector2 inertia = -velocity * InertiaFactor;
			Vector2 splash = velocity.RotatedByRandom(SplashAngle) * SplashFactor;
			Vector2 random = Main.rand.NextVector2Circular(RandomSpread, RandomSpread);
			Vector2 vel = inertia + splash + random;
			float scale = Main.rand.NextFloat(0.6f, 1.0f) * SizeMultiplier;
			float rotSpeed = Main.rand.NextFloat(-0.15f, 0.15f);
			fragments.Add(new Fragment(center + SpawnOffset, vel, life, scale, rotSpeed));
		}

		public void Draw(SpriteBatch sb)
		{
			if (fragments.Count == 0) return;

			Texture2D tex = TrailTexture;
			if (tex == null) return;

			// 注意：Additive 混合模式由 TrailManager.Draw() 统一管理
			// 此处不再自行开关 SpriteBatch

			var sorted = fragments.OrderBy(f => f.Life);
			foreach (var f in sorted)
			{
				float progress = 1f - (float)f.Life / f.MaxLife;
				Color fragColor = Color.Lerp(ColorStart, ColorEnd, progress);
				float scale = f.Scale * (1f - progress * progress);
				sb.Draw(tex, f.Position - Main.screenPosition, null, fragColor,
					f.Rotation, tex.Size() * 0.5f, scale, SpriteEffects.None, 0);
			}
		}

		public void Clear()
		{
			fragments.Clear();
			spawnCounter = 0;
		}
	}
}
