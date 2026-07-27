// ============================================================
// PRT 粒子 — 月光星点（极小光点 + 螺旋漂移）
// ============================================================
//
// 【新概念】
//   1. 螺旋漂移：用 _driftAngle 递增 + Sin/Cos 产生螺旋轨迹
//   2. 极小粒子：Scale * 0.3 绘制，适合做背景星点
//   3. 平方淡出：(1-p²) 先快后慢
//
// 【适用场景】
//   背景星点、魔法尘埃、环境粒子等
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
    /// 月光星点粒子 — 极小光点，螺旋漂移，平方淡出。
    /// 最简单的 PRT 实现，适合作为"背景星点"或"魔法尘埃"。
    /// 
    /// 【新概念】
    /// - 螺旋漂移：角度递增 → Sin/Cos 产生圆周运动分量
    /// - 平方淡出：前期保持可见，后期快速消失
    /// - 单层绘制：最简单的 PreDraw
    /// </summary>
    public class PRT_MoonSparkle : BasePRT
    {
        /// <summary>纹理路径</summary>
        public override string Texture => "VerminLordMod/Assets/Textures/Glows/CircleGlow";

        /// <summary>螺旋漂移角度（递增产生旋转）</summary>
        private float _driftAngle;

        // ══════════════════════════════════════════════════════
        // SetProperty()
        // ══════════════════════════════════════════════════════
        public override void SetProperty()
        {
            PRTDrawMode = PRTDrawModeEnum.AdditiveBlend;

            // 随机初始漂移角度，使不同粒子的螺旋相位不同
            _driftAngle = Main.rand?.NextFloat(MathHelper.TwoPi) ?? 0;
        }

        // ══════════════════════════════════════════════════════
        // AI()
        // ══════════════════════════════════════════════════════
        public override void AI()
        {
            Time++;

            float p = Time / (float)Lifetime;

            // ── 平方淡出 ──
            // (1 - p²) 曲线：前期缓慢下降，后期快速归零
            // 效果：粒子在大部分生命周期内保持可见，最后突然消失
            Opacity = (1f - p * p) * 0.8f;

            // 缩放衰减
            Scale *= 0.97f;

            // ── 螺旋漂移 ──
            // 角度递增 → 速度方向不断变化 → 产生螺旋轨迹
            _driftAngle += 0.03f;
            Velocity += new Vector2(
                MathF.Cos(_driftAngle) * 0.05f,
                MathF.Sin(_driftAngle) * 0.05f
            );
            // 速度阻尼
            Velocity *= 0.97f;
        }

        // ══════════════════════════════════════════════════════
        // PreDraw() — 单层绘制（最简单的 PreDraw）
        // ══════════════════════════════════════════════════════
        public override bool PreDraw(SpriteBatch sb)
        {
            var glow = ModContent.Request<Texture2D>(Texture).Value;
            if (glow == null) return false;

            // 淡蓝色，极小（Scale * 0.3）
            Color c = new Color(180, 220, 255) * Opacity;
            sb.Draw(glow, Position - Main.screenPosition, null, c, 0f,
                glow.Size() / 2f, Scale * 0.3f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
