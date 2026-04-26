using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
	/// <summary>
	/// 拖尾便捷工厂 - 提供静态方法快速创建和配置各种拖尾
	/// </summary>
	public static class TrailHelper
	{
		/// <summary>
		/// 为弹幕创建一个 GhostTrail（虚影拖尾）并自动添加到 TrailManager
		/// </summary>
		/// <param name="manager">拖尾管理器</param>
		/// <param name="trailTex">拖尾贴图（null则使用弹幕贴图）</param>
		/// <param name="color">拖尾颜色</param>
		/// <param name="maxPositions">最大位置数</param>
		/// <param name="widthScale">宽度缩放</param>
		/// <param name="lengthScale">长度缩放</param>
		/// <param name="alpha">透明度</param>
		/// <param name="recordInterval">记录间隔（帧）</param>
		/// <param name="enableGlow">是否启用发光</param>
		/// <returns>创建的 GhostTrail 实例</returns>
		public static GhostTrail AddGhostTrail(this TrailManager manager,
			Texture2D trailTex,
			Color? color = null,
			int maxPositions = 16,
			float widthScale = 0.4f,
			float lengthScale = 2f,
			float alpha = 0.8f,
			int recordInterval = 2,
			bool enableGlow = false)
		{
			var trail = new GhostTrail
			{
				TrailTexture = trailTex,
				TrailColor = color ?? new Color(150, 220, 255),
				MaxPositions = maxPositions,
				WidthScale = widthScale,
				LengthScale = lengthScale,
				Alpha = alpha,
				RecordInterval = recordInterval,
				EnableGlow = enableGlow,
				UseAdditiveBlend = true
			};
			manager.Add(trail);
			return trail;
		}

		/// <summary>
		/// 为弹幕创建一个 LiquidTrail（液态火焰拖尾）并自动添加到 TrailManager
		/// </summary>
		/// <param name="manager">拖尾管理器</param>
		/// <param name="trailTex">拖尾贴图（null则使用弹幕贴图）</param>
		/// <param name="colorStart">起始颜色</param>
		/// <param name="colorEnd">结束颜色</param>
		/// <param name="maxFragments">最大碎片数</param>
		/// <param name="fragmentLife">碎片存活帧数</param>
		/// <param name="sizeMultiplier">大小倍率</param>
		/// <param name="spawnInterval">生成间隔（帧）</param>
		/// <returns>创建的 LiquidTrail 实例</returns>
		public static LiquidTrail AddLiquidTrail(this TrailManager manager,
			Texture2D trailTex,
			Color? colorStart = null,
			Color? colorEnd = null,
			int maxFragments = 40,
			int fragmentLife = 30,
			float sizeMultiplier = 1f,
			int spawnInterval = 2)
		{
			var trail = new LiquidTrail
			{
				TrailTexture = trailTex,
				ColorStart = colorStart ?? new Color(255, 80, 30, 255),
				ColorEnd = colorEnd ?? new Color(60, 10, 20, 0),
				MaxFragments = maxFragments,
				FragmentLife = fragmentLife,
				SizeMultiplier = sizeMultiplier,
				SpawnInterval = spawnInterval
			};
			manager.Add(trail);
			return trail;
		}

		/// <summary>
		/// 为弹幕快速创建默认的发光虚影拖尾（兼容原 Fucs.DrawGlowingProjectile 风格）
		/// </summary>
		/// <param name="manager">拖尾管理器</param>
		/// <param name="projectile">弹幕实例（用于获取默认贴图）</param>
		/// <param name="color">拖尾颜色</param>
		/// <param name="widthScale">宽度缩放</param>
		/// <param name="lengthScale">长度缩放</param>
		/// <returns>创建的 GhostTrail 实例</returns>
		public static GhostTrail AddDefaultGlowTrail(this TrailManager manager, Projectile projectile,
			Color? color = null, float widthScale = 0.4f, float lengthScale = 2f)
		{
			Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[projectile.type].Value;
			return manager.AddGhostTrail(tex, color ?? new Color(150, 220, 255),
				maxPositions: 16, widthScale: widthScale, lengthScale: lengthScale,
				alpha: 0.8f, recordInterval: 2, enableGlow: true);
		}

		/// <summary>
		/// 为弹幕快速创建默认的液态火焰拖尾（兼容原 LiquidTrailManager 风格）
		/// </summary>
		/// <param name="manager">拖尾管理器</param>
		/// <param name="projectile">弹幕实例（用于获取默认贴图）</param>
		/// <param name="colorStart">起始颜色</param>
		/// <param name="colorEnd">结束颜色</param>
		/// <returns>创建的 LiquidTrail 实例</returns>
		public static LiquidTrail AddDefaultLiquidTrail(this TrailManager manager, Projectile projectile,
			Color? colorStart = null, Color? colorEnd = null)
		{
			Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[projectile.type].Value;
			return manager.AddLiquidTrail(tex, colorStart, colorEnd);
		}

		/// <summary>
		/// 获取或创建指定类型的拖尾（如果已存在则返回现有实例）
		/// </summary>
		public static T GetOrAdd<T>(this TrailManager manager) where T : class, ITrail, new()
		{
			var existing = manager.Get<T>();
			if (existing != null)
				return existing;
			var trail = new T();
			manager.Add(trail);
			return trail;
		}
	}
}
