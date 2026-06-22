using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace VerminLordMod.Common.Effects
{
	/// <summary>
	/// 月光爆散效果 — 一次性调用的三层爆散（Streak + Vortex + Mist）
	/// 使用 Dust 实现，不依赖多帧绘制。
	/// </summary>
	public static class MoonBurstHelper
	{
		/// <summary>
		/// 在指定位置生成月光爆散效果。
		/// </summary>
		/// <param name="center">爆散中心</param>
		/// <param name="color">主色调（默认月蓝色）</param>
		/// <param name="scale">整体缩放（1.0=标准）</param>
		public static void SpawnBurst(Vector2 center, Color? color = null, float scale = 1f)
		{
			Color moonColor = color ?? new Color(160, 210, 255);
			var rand = Main.rand;

			// === Streak 层：高速光痕线 ===
			for (int i = 0; i < (int)(20 * scale); i++)
			{
				float angle = rand.NextFloat(MathHelper.TwoPi);
				Vector2 dir = angle.ToRotationVector2();
				float speed = rand.NextFloat(6f, 14f) * scale;
				Dust d = Dust.NewDustPerfect(
					center + dir * rand.NextFloat(5f, 15f) * scale,
					DustID.AncientLight,
					dir * speed, 60, moonColor, rand.NextFloat(1.0f, 1.8f) * scale);
				d.noGravity = true;

				// 副痕
				Vector2 perp = dir.RotatedBy(MathHelper.PiOver2);
				Dust d2 = Dust.NewDustPerfect(
					center + (dir + perp * rand.NextFloat(-0.3f, 0.3f)) * rand.NextFloat(5f, 15f) * scale,
					DustID.AncientLight,
					(dir + perp * rand.NextFloat(-0.2f, 0.2f)) * speed * 0.7f,
					40, moonColor * 0.6f, rand.NextFloat(0.7f, 1.2f) * scale);
				d2.noGravity = true;
			}

			// === Vortex 层：旋转光旋 ===
			for (int i = 0; i < (int)(24 * scale); i++)
			{
				float angle = MathHelper.TwoPi / 24f * i + rand.NextFloat(-0.06f, 0.06f);
				Vector2 dir = angle.ToRotationVector2();
				float outward = rand.NextFloat(2f, 5f) * scale;
				float spin = rand.NextFloat(3f, 7f) * scale * (rand.NextBool() ? 1f : -1f);
				Vector2 vel = dir * outward + dir.RotatedBy(MathHelper.PiOver2) * spin;

				Dust d = Dust.NewDustPerfect(
					center + dir * rand.NextFloat(10f, 30f) * scale,
					DustID.BlueFairy,
					vel, 80, moonColor, rand.NextFloat(1.0f, 1.8f) * scale);
				d.noGravity = true;

				if (scale > 0.7f)
				{
					Dust d2 = Dust.NewDustPerfect(
						center + dir * rand.NextFloat(5f, 12f) * scale,
						DustID.BlueFairy,
						vel * 0.5f, 60, moonColor * 0.5f, rand.NextFloat(0.5f, 0.8f) * scale);
					d2.noGravity = true;
				}
			}

			// === Mist 层：膨胀光雾 ===
			for (int i = 0; i < (int)(10 * scale); i++)
			{
				Vector2 dir = rand.NextVector2Unit();
				Dust d = Dust.NewDustPerfect(
					center + dir * rand.NextFloat(2f, 8f) * scale,
					DustID.MagicMirror,
					dir * rand.NextFloat(0.5f, 2f) * scale, 30,
					new Color(140, 200, 255, 120), rand.NextFloat(1.5f, 3.0f) * scale);
				d.noGravity = true;
				d.alpha = 100;
			}

			// === 中心光爆 ===
			for (int i = 0; i < (int)(6 * scale); i++)
			{
				Dust d = Dust.NewDustPerfect(
					center + rand.NextVector2Circular(4f, 4f) * scale,
					DustID.AncientLight,
					rand.NextVector2Circular(1f, 1f), 0,
					new Color(220, 240, 255), rand.NextFloat(2.0f, 3.5f) * scale);
				d.noGravity = true;
				d.alpha = 50;
			}
		}

		/// <summary>
		/// 小型爆散（用于快速/穿透弹幕的简洁版）
		/// </summary>
		public static void SpawnSmallBurst(Vector2 center, Color? color = null, float scale = 1f)
		{
			Color moonColor = color ?? new Color(160, 210, 255);
			var rand = Main.rand;

			for (int i = 0; i < (int)(12 * scale); i++)
			{
				Vector2 dir = rand.NextVector2Unit();
				float speed = rand.NextFloat(4f, 10f) * scale;
				Dust d = Dust.NewDustPerfect(
					center + dir * rand.NextFloat(3f, 12f) * scale,
					DustID.AncientLight,
					dir * speed, 40, moonColor, rand.NextFloat(0.8f, 1.5f) * scale);
				d.noGravity = true;
			}
			for (int i = 0; i < (int)(8 * scale); i++)
			{
				Dust d = Dust.NewDustPerfect(
					center + rand.NextVector2Circular(6f, 6f) * scale,
					DustID.BlueFairy,
					rand.NextVector2Circular(4f, 4f) * scale, 30,
					moonColor, rand.NextFloat(0.6f, 1.2f) * scale);
				d.noGravity = true;
			}
		}
	}
}
