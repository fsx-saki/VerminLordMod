using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace VerminLordMod.Content.Trails
{
	/// <summary>
	/// 虚影拖尾 - 原 Fucs 风格，记录历史位置并绘制拉伸贴图
	/// 适用于：弹幕拖尾、玩家移动拖尾等
	/// </summary>
	public class GhostTrail : ITrail
	{
		// ===== 可配置参数 =====

		/// <summary>拖尾名称</summary>
		public string Name { get; set; } = "GhostTrail";

		/// <summary>记录位置的最大数量</summary>
		public int MaxPositions { get; set; } = 16;

		/// <summary>记录间隔（帧），每N帧记录一次位置</summary>
		public int RecordInterval { get; set; } = 2;

		/// <summary>拖尾贴图</summary>
		public Texture2D TrailTexture { get; set; }

		/// <summary>拖尾颜色</summary>
		public Color TrailColor { get; set; } = new Color(150, 220, 255);

		/// <summary>整体透明度 (0~1)</summary>
		public float Alpha { get; set; } = 0.8f;

		/// <summary>拖尾段宽度缩放</summary>
		public float WidthScale { get; set; } = 0.4f;

		/// <summary>拖尾段长度缩放</summary>
		public float LengthScale { get; set; } = 2f;

		/// <summary>固定偏移量（用于修正贴图宽度导致的视觉偏移）</summary>
		public Vector2 Offset { get; set; } = Vector2.Zero;

		/// <summary>是否使用 Additive 混合模式</summary>
		public bool UseAdditiveBlend { get; set; } = true;

		/// <summary>是否启用发光效果（在拖尾位置绘制发光点）</summary>
		public bool EnableGlow { get; set; } = false;

		/// <summary>发光颜色（null则使用TrailColor）</summary>
		public Color? GlowColor { get; set; } = null;

		/// <summary>发光大小</summary>
		public float GlowScale { get; set; } = 1f;

		// ===== 内部状态 =====

		private Vector2[] positions;
		private int frameCounter = 0;

		public GhostTrail()
		{
			positions = new Vector2[MaxPositions];
		}

		public bool HasContent => positions != null && positions.Length > 0;

		public void Update(Vector2 center, Vector2 velocity)
		{
			if (positions == null || positions.Length != MaxPositions)
				positions = new Vector2[MaxPositions];

			// 每隔 RecordInterval 帧记录一次位置
			if (frameCounter % RecordInterval == 0)
			{
				for (int i = positions.Length - 1; i > 0; i--)
					positions[i] = positions[i - 1];
			}
			positions[0] = center;
			frameCounter++;
		}

		public void Draw(SpriteBatch sb)
		{
			if (positions == null || positions.Length < 2)
				return;

			Texture2D tex = TrailTexture;
			if (tex == null) return;

			if (UseAdditiveBlend)
			{
				sb.End();
				sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
					DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			// 绘制拖尾段
			for (int i = 1; i < positions.Length; i++)
			{
				Vector2 start = positions[i - 1] + Offset;
				Vector2 end = positions[i] + Offset;
				if (start == Vector2.Zero || end == Vector2.Zero) continue;

				Vector2 diff = end - start;
				float segLength = diff.Length();
				if (segLength == 0) continue;

				float rotation = diff.ToRotation();
				float fadeAlpha = (1f - (float)i / positions.Length) * Alpha;
				Color drawColor = TrailColor * fadeAlpha * 0.8f;

				Vector2 scale = new Vector2(LengthScale, WidthScale);
				Vector2 drawPos = (start + end) / 2f - Main.screenPosition;
				sb.Draw(tex, drawPos, null, drawColor, rotation,
					tex.Size() * 0.5f, scale, SpriteEffects.None, 0);
			}

			// 发光效果
			if (EnableGlow && positions[0] != Vector2.Zero)
			{
				Color glowColor = GlowColor ?? TrailColor;
				Vector2 pos = positions[0] - Main.screenPosition;
				Vector2 origin = tex.Size() * 0.5f;
				for (int i = 0; i < 3; i++)
				{
					float gs = GlowScale * (1.2f + i * 0.4f);
					float ga = 0.5f - i * 0.15f;
					sb.Draw(tex, pos, null, glowColor * ga, 0f, origin, gs, SpriteEffects.None, 0);
				}
			}

			if (UseAdditiveBlend)
			{
				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
					DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		public void Clear()
		{
			if (positions != null)
				Array.Clear(positions, 0, positions.Length);
			frameCounter = 0;
		}
	}
}
