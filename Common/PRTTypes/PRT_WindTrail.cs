// ============================================================
// PRT 粒子 — 风刃拖尾（基础拖尾 + 正弦振荡）
// ============================================================
//
// 【与水弹拖尾的区别】
//   1. 运动：水平和垂直方向都有正弦振荡（模拟风的飘忽感）
//   2. 速度衰减更快（0.94 vs 0.96）
//   3. 颜色：青白色调（180, 245, 230）
//   4. 拉伸基础值更大（2.5 vs 1.2），拖尾更长更飘逸
//
// 【学习要点】
// 对比 PRT_WaterTrail 和本文件，理解如何通过微调参数
// 创造不同"感觉"的拖尾效果。
// ============================================================

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using InnoVault;
using InnoVault.PRT;
using Terraria.ModLoader;

namespace VerminLordMod.Common.PRTTypes
{
    /// <summary>
    /// 风刃拖尾粒子 — 青白色，带双向正弦振荡，模拟风的飘忽不定。
    /// 与水弹相比更轻灵、更飘忽。
    /// </summary>
    public class PRT_WindTrail : BasePRT
    {
        /// <summary>纹理路径</summary>
        public override string Texture => "VerminLordMod/Assets/Textures/Glows/CircleGlow";

        /// <summary>拉伸系数（控制拖尾长度）</summary>
        private float _stretch;

        // ══════════════════════════════════════════════════════
        // SetProperty()
        // ══════════════════════════════════════════════════════
        public override void SetProperty()
        {
            PRTDrawMode = PRTDrawModeEnum.AdditiveBlend;
            Scale = 0.35f;       // 比水弹略小
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

            // ── 运动：双向正弦振荡 ──
            // 水平振荡频率 0.12，垂直振荡频率 0.08
            // 不同频率产生"飘忽不定"的轨迹
            Velocity *= 0.94f;  // 比水弹衰减更快（更飘）
            Velocity += new Vector2(
                MathF.Sin(Time * 0.12f) * 0.04f,
                MathF.Cos(Time * 0.08f) * 0.02f
            );
        }

        // ══════════════════════════════════════════════════════
        // PreDraw()
        // ══════════════════════════════════════════════════════
        public override bool PreDraw(SpriteBatch sb)
        {
            var glow = ModContent.Request<Texture2D>(Texture).Value;
            if (glow == null) return false;

            float rot = Velocity.Length() > 0.5f ? Velocity.ToRotation() : Rotation;

            // 基础拉伸更大（2.5），拖尾更长更飘逸
            float stretch = 2.5f + _stretch * 4f;

            // 青白色调
            Color c = new Color(180, 245, 230) * Opacity;
            Color c2 = new Color(140, 220, 200) * (Opacity * 0.5f);

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
