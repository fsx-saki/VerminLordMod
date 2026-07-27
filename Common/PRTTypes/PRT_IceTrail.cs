// ============================================================
// PRT 粒子 — 冰锥拖尾（基础拖尾 + 微弱重力）
// ============================================================
//
// 【与水弹拖尾的区别】
//   1. 运动：只有微弱重力（0.01），无水平摆动
//   2. 速度衰减更慢（0.97 vs 0.96），更滑
//   3. 颜色：冰蓝色调（200, 240, 255）
//   4. 拉伸基础值更小（1.0），拖尾更短更凝聚
//
// 【学习要点】
// 这是最简单的拖尾粒子——几乎没有横向扰动，
// 适合表现"直线飞行"的弹幕（如冰锥、箭矢）。
// ============================================================

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using InnoVault;
using InnoVault.PRT;

namespace VerminLordMod.Common.PRTTypes
{
    /// <summary>
    /// 冰锥拖尾粒子 — 冰蓝色，直线下落，无横向摆动。
    /// 最简单的拖尾实现，适合高速直线弹幕。
    /// </summary>
    public class PRT_IceTrail : BasePRT
    {
        /// <summary>纹理路径</summary>
        public override string Texture => "InnoVault/Assets/placeholder2";

        /// <summary>拉伸系数</summary>
        private float _stretch;

        // ══════════════════════════════════════════════════════
        // SetProperty()
        // ══════════════════════════════════════════════════════
        public override void SetProperty()
        {
            PRTDrawMode = PRTDrawModeEnum.AdditiveBlend;
            Scale = 0.3f;       // 冰锥粒子更小
            _stretch = 1f;
        }

        // ══════════════════════════════════════════════════════
        // AI()
        // ══════════════════════════════════════════════════════
        public override void AI()
        {
            Time++;
            float p = Time / (float)Lifetime;
            Opacity = (1f - p) * 0.6f;
            Scale *= 0.97f;
            _stretch = MathHelper.Lerp(_stretch, 0.3f, 0.03f);

            // ── 运动：直线下落 ──
            // 无水平摆动，只有微弱重力
            Velocity *= 0.97f;  // 衰减慢，保持滑行感
            Velocity.Y += 0.01f; // 微弱重力
        }

        // ══════════════════════════════════════════════════════
        // PreDraw()
        // ══════════════════════════════════════════════════════
        public override bool PreDraw(SpriteBatch sb)
        {
            var glow = VaultAsset.Light?.Value;
            if (glow == null) return false;

            float rot = Velocity.Length() > 0.5f ? Velocity.ToRotation() : Rotation;

            // 基础拉伸更小（1.0），拖尾更短更凝聚
            float stretch = 1.0f + _stretch * 4f;

            // 冰蓝色调
            Color c = new Color(200, 240, 255) * Opacity;
            Color c2 = new Color(120, 200, 255) * (Opacity * 0.5f);

            Vector2 sv = new Vector2(stretch * Scale * 2f, Scale * 0.5f);

            // 三层绘制
            sb.Draw(glow, Position - Main.screenPosition, null, c * 0.5f, rot,
                glow.Size() / 2f, sv, SpriteEffects.None, 0f);
            sb.Draw(glow, Position - Main.screenPosition, null, c2 * 0.6f, rot,
                glow.Size() / 2f, sv * 0.5f, SpriteEffects.None, 0f);
            sb.Draw(glow, Position - Main.screenPosition, null, Color.White * (Opacity * 0.25f), rot,
                glow.Size() / 2f, sv * 0.2f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
