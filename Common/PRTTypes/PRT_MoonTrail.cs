// ============================================================
// PRT 粒子 — 月光拖尾（复杂：主拖尾 + 鬼影 + 尖端火花）
// ============================================================
//
// 【新概念】
//   1. 主拖尾（Main streak）：沿速度方向拉伸的长条
//   2. 鬼影拖尾（Ghost trail）：垂直于速度方向的两个副条
//      产生"十字光晕"效果
//   3. 尖端火花（Sparkle at tip）：在粒子前端的小光点，
//      用 _sparkPhase 做闪烁动画
//
// 【绘制层次（从外到内）】
//   1. 主拖尾（半透明蓝）
//   2. 内核亮条（更亮更窄）
//   3. 鬼影拖尾 × 2（垂直方向，半透明）
//   4. 尖端火花（白色闪烁）
//
// 【学习要点】
// 这是最复杂的绘制示例，展示了如何通过多层叠加
// 创造丰富的视觉效果。
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
    /// 月光拖尾粒子 — 蓝色调，含主拖尾、鬼影副条和尖端火花。
    /// 最复杂的 PRT 绘制示例，适合作为"华丽弹幕"的粒子效果。
    /// </summary>
    public class PRT_MoonTrail : BasePRT
    { 
        /// <summary>纹理路径</summary>
        public override string Texture => "VerminLordMod/Assets/Textures/Glows/StripGlow";

        /// <summary>主拖尾长度系数</summary>
        private float _streakLen;

        /// <summary>火花闪烁相位（递增角度）</summary>
        private float _sparkPhase;

        // ══════════════════════════════════════════════════════
        // SetProperty()
        // ══════════════════════════════════════════════════════
        public override void SetProperty()
        {
            PRTDrawMode = PRTDrawModeEnum.AdditiveBlend;
            _streakLen = 1f;    // 初始拖尾最长
        }

        // ══════════════════════════════════════════════════════
        // AI()
        // ══════════════════════════════════════════════════════
        public override void AI()
        {
            Time++;

            // 透明度：线性淡出，最大 0.8
            Opacity = (1f - Time / (float)Lifetime) * 0.8f;

            // 缩放和拖尾长度同步衰减
            Scale *= 0.97f;
            _streakLen *= 0.97f;

            // 火花相位递增（用于 PreDraw 中的闪烁计算）
            _sparkPhase += 0.1f;

            // ── 运动：低频双向振荡 ──
            // 频率很低（0.08/0.1），幅度很小（0.008）
            // 产生轻微的"漂浮"感
            Velocity *= 0.94f;
            Velocity += new Vector2(
                MathF.Sin(Time * 0.08f + Position.X * 0.01f) * 0.008f,
                MathF.Cos(Time * 0.1f + Position.Y * 0.01f) * 0.008f
            );
        }

        // ══════════════════════════════════════════════════════
        // PreDraw() — 四层绘制
        // ══════════════════════════════════════════════════════
        public override bool PreDraw(SpriteBatch sb)
        {
            var glow = ModContent.Request<Texture2D>(Texture).Value;
            if (glow == null) return false;

            float rot = Velocity.Length() > 0.5f ? Velocity.ToRotation() : Rotation;

            // 颜色定义
            Color baseC = new Color(180, 220, 255) * Opacity;   // 淡蓝

            // ══════════════════════════════════════════════════
            // 第1层：主拖尾（Main streak）
            // ══════════════════════════════════════════════════
            Vector2 stretch = new(Velocity.Length()*0.6f,Scale/glow.Height*_streakLen); // 拉伸比例，Y 方向拉伸
            sb.Draw(glow, Position - Main.screenPosition, null, baseC * 0.6f, rot,
                glow.Size() / 2f, stretch, SpriteEffects.None, 0f);

            // // ══════════════════════════════════════════════════
            // // 第2层：内核亮条（Inner bright core）
            // // ══════════════════════════════════════════════════
            // // 更小更亮，叠加在主拖尾上产生发光效果
            // sb.Draw(glow, Position - Main.screenPosition, null, brightC * 0.5f, rot,
            //     glow.Size() / 2f, mainScale * 0.4f, SpriteEffects.None, 0f);

            // // ══════════════════════════════════════════════════
            // // 第3层：鬼影拖尾（Ghost trail）
            // // ══════════════════════════════════════════════════
            // // 垂直于主方向的两个副条，旋转 ±0.3 弧度
            // // 产生"十字光晕"效果
            // float stretch2 = 1f + _streakLen * 3f;
            // Vector2 ghostScale = new(stretch2 * Scale * 1.2f, Scale * 0.25f);
            // sb.Draw(glow, Position - Main.screenPosition, null, baseC * 0.2f, rot + 0.3f,
            //     glow.Size() / 2f, ghostScale, SpriteEffects.None, 0f);
            // sb.Draw(glow, Position - Main.screenPosition, null, baseC * 0.2f, rot - 0.3f,
            //     glow.Size() / 2f, ghostScale, SpriteEffects.None, 0f);

            // // ══════════════════════════════════════════════════
            // // 第4层：尖端火花（Sparkle at tip）
            // // ══════════════════════════════════════════════════
            // // 在粒子前端的一个小光点，周期性闪烁
            // float spark = MathF.Sin(_sparkPhase) * 0.5f + 0.5f; // [0, 1] 闪烁
            // float sparkSize = Scale * 0.6f * spark;
            // if (sparkSize > 0.05f) // 太小就不画了，优化性能
            // {
            //     // 计算尖端位置：当前位置 + 速度方向 × 偏移量
            //     Vector2 tip = Position + Velocity.SafeNormalize(Vector2.Zero) * 8f * Scale;
            //     sb.Draw(glow, tip - Main.screenPosition, null,
            //         Color.White * (Opacity * 0.6f * spark),
            //         0f, glow.Size() / 2f, sparkSize, SpriteEffects.None, 0f);
            // }

            return false;
        }
    }
}
