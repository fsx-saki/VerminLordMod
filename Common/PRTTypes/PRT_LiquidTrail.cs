// ============================================================
// PRT 粒子 — 液态拖尾（颜色渐变 + 上浮飘散）
// ============================================================
//
// 【与基础拖尾的区别】
//   1. 颜色渐变：从 _startColor 到 _endColor 随时间 Lerp
//   2. 透明度：平方淡出 (1-p)²，先快后慢
//   3. 运动：持续上浮（Velocity.Y 持续减小），无重力
//   4. 只有两层绘制（无白色核心层）
//
// 【适用场景】
//   岩浆/火焰弹幕的残留液滴、药水飞溅等
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
    /// 液态拖尾粒子 — 从黄到红的颜色渐变，持续上浮飘散。
    /// 适合表现岩浆、火焰弹幕的残留液滴效果。
    /// 
    /// 【新概念】
    /// - Color.Lerp(a, b, t)：颜色插值，t=0 返回 a，t=1 返回 b
    /// - 平方淡出：比线性淡出更自然，前期保持可见，后期快速消失
    /// </summary>
    public class PRT_LiquidTrail : BasePRT
    {
        /// <summary>纹理路径</summary>
        public override string Texture => "VerminLordMod/Assets/Textures/Glows/CircleGlow";

        /// <summary>起始颜色（亮黄）</summary>
        private Color _startColor;

        /// <summary>结束颜色（暗红，透明度为0）</summary>
        private Color _endColor;

        // ══════════════════════════════════════════════════════
        // SetProperty()
        // ══════════════════════════════════════════════════════
        public override void SetProperty()
        {
            PRTDrawMode = PRTDrawModeEnum.AdditiveBlend;
            Scale = 0.5f;

            // 颜色渐变端点
            _startColor = new Color(255, 220, 100);   // 亮黄
            _endColor = new Color(255, 30, 0, 0);     // 暗红（Alpha=0）
        }

        // ══════════════════════════════════════════════════════
        // AI()
        // ══════════════════════════════════════════════════════
        public override void AI()
        {
            Time++;
            float p = Time / (float)Lifetime;

            // ── 平方淡出 ──
            // (1-p)² 曲线：前期缓慢下降，后期快速归零
            // 比线性淡出更自然，粒子在大部分生命周期内保持可见
            Opacity = (1f - p) * (1f - p);

            // 缩放衰减
            Scale *= 0.98f;

            // ── 运动：上浮飘散 ──
            // 持续上浮（模拟热液滴上升）
            Velocity.Y -= 0.04f;
            // 速度阻尼
            Velocity *= 0.96f;
            // 水平正弦摆动（相位加入 Position.X 使不同粒子不同步）
            Velocity += new Vector2(
                MathF.Sin(Time * 0.1f + Position.X * 0.01f) * 0.02f,
                0
            );
        }

        // ══════════════════════════════════════════════════════
        // PreDraw()
        // ══════════════════════════════════════════════════════
        public override bool PreDraw(SpriteBatch sb)
        {
            var glow = ModContent.Request<Texture2D>(Texture).Value;
            if (glow == null) return false;

            // ── 颜色插值 ──
            // 随着 p 从 0→1，颜色从亮黄渐变到暗红
            float p = Time / (float)Lifetime;
            Color c = Color.Lerp(_startColor, _endColor, p) * Opacity;

            float rot = Velocity.Length() > 0.5f ? Velocity.ToRotation() : Rotation;

            // 拉伸：越老的粒子拉伸越长
            float stretch = 1.2f + (1f - p) * 2f;
            Vector2 sv = new(stretch * Scale * 2f, Scale * 0.4f);

            // ── 两层绘制 ──
            // 第1层：主体
            sb.Draw(glow, Position - Main.screenPosition, null, c * 0.6f, rot,
                glow.Size() / 2f, sv, SpriteEffects.None, 0f);
            // 第2层：内层（更亮，更小）
            sb.Draw(glow, Position - Main.screenPosition, null, c * 0.3f, rot,
                glow.Size() / 2f, sv * 0.6f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
