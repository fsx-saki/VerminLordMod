using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Common.Players
{
	/// <summary>
	/// 特效播放器 - 处理所有玩家身上的Buff视觉效果
	/// </summary>
	public class EffectsPlayer : ModPlayer
	{
		// 旋风特效相关
		private Vector2[] breezeTrailPos = new Vector2[12];
		private int breezeTrailTimer = 0;
		private bool hasBreezeBuff = false;

		public override void PostUpdate() {
			// 只为自己的玩家处理
			if (Player != Main.LocalPlayer) return;

			hasBreezeBuff = Player.HasBuff(ModContent.BuffType<BreezeWheelbuff>());

			// 旋风buff特效 - 只更新数据
			if (hasBreezeBuff) {
				// 更新拖尾位置
				breezeTrailTimer++;
				if (breezeTrailTimer % 2 == 0) {
					for (int i = breezeTrailPos.Length - 1; i > 0; i--) {
						breezeTrailPos[i] = breezeTrailPos[i - 1];
					}
					breezeTrailPos[0] = Player.Center;
				}
			}
		}

		/// <summary>
		/// 绘制特效（由PlayerEffectDrawSystem调用）
		/// </summary>
		public void DrawEffects(SpriteBatch sb) {
			if (!hasBreezeBuff || Player == null) return;
			DrawBreezeWheelEffect(sb, Player);
		}

		private void DrawBreezeWheelEffect(SpriteBatch sb, Player player) {
			// 获取玩家当前速度
			Vector2 velocity = player.velocity;
			float speed = velocity.Length();

			// 速度越大，特效越大（最小0.3，最大2.5）
			float speedScale = MathHelper.Lerp(0.3f, 2.5f, MathHelper.Clamp(speed / 10f, 0f, 1f));

			Texture2D tex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/CycloneProj").Value;
			Color cycloneColor = new Color(70, 255, 160); // 绿色旋风

			// 旋转角度（随风旋转）
			float rotation = Main.GameUpdateCount * 0.05f;

			sb.End();
			sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			Vector2 pos = player.Center - Main.screenPosition;
			Vector2 origin = tex.Size() * 0.5f;

			// 绘制多层旋风发光效果
			for (int i = 0; i < 4; i++) {
				float layerScale = speedScale * (0.8f + i * 0.4f);
				float layerAlpha = 0.6f - i * 0.12f;
				float rotOffset = i * MathHelper.PiOver4; // 每层偏转45度

				Color glowColor = cycloneColor * layerAlpha;
				Main.EntitySpriteDraw(tex, pos, null, glowColor,
					rotation + rotOffset, origin, layerScale, SpriteEffects.None, 0);
			}

			// 绘制拖尾
			Texture2D trailTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTail").Value;
			DrawBreezeTrail(sb, trailTex, speedScale);

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawBreezeTrail(SpriteBatch sb, Texture2D trailTex, float scale) {
			for (int i = 1; i < breezeTrailPos.Length; i++) {
				Vector2 start = breezeTrailPos[i - 1] - Main.screenPosition;
				Vector2 end = breezeTrailPos[i] - Main.screenPosition;
				if (start == Vector2.Zero || end == Vector2.Zero) continue;

				Vector2 diff = end - start;
				float segLength = diff.Length();
				if (segLength < 2f) continue;

				float rotation = diff.ToRotation();
				float fadeAlpha = (1f - (float)i / breezeTrailPos.Length) * 0.5f;
				Color trailColor = new Color(70, 255, 160) * fadeAlpha;

				float width = scale * 0.8f;
				float length = segLength * 0.5f;
				Vector2 drawPos = (start + end) / 2f;

				sb.Draw(trailTex, drawPos, null, trailColor, rotation,
					trailTex.Size() * 0.5f, new Vector2(length, width), SpriteEffects.None, 0);
			}
		}
	}
}
