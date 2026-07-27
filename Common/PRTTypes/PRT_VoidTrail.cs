// ============================================================
// PRT 粒子 — 虚空弹拖尾（基础拖尾 + 双向振荡）
// ============================================================
//
// 【与其他拖尾的区别】
//   1. 运动：双向正弦振荡（类似风），但频率更低（0.07/0.09）
//   2. 速度衰减最快（0.93），粒子很快减速
//   3. 颜色：紫色调（90, 20, 130），暗黑风格
//   4. 初始 Scale 最大（0.45），粒子更大
//
// 【学习要点】
// 对比所有拖尾粒子的参数，理解如何通过调整：
//   - 速度衰减率 → 控制拖尾长度
//   - 振荡频率/幅度 → 控制轨迹飘忽程度
//   - 颜色 → 控制视觉风格
// 来创造不同"元素感"的粒子效果。
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
    /// 虚空弹拖尾粒子 — 紫色调，低速振荡，暗黑风格。
    /// 速度衰减最快，适合表现"能量消散"的感觉。
    /// </summary>
    public class PRT_VoidTrail : BasePRT
    {
        /// <summary>纹理路径</summary>
        public override string Texture => "VerminLordMod/Assets/Textures/Glows/CircleGlow";

        /// <summary>拉伸系数</summary>
        private float _stretch;

        // ══════════════════════════════════════════════════════
        // SetProperty()
        // ══════════════════════════════════════════════════════
        public override void SetProperty()
        {
            PRTDrawMode = PRTDrawModeEnum.AdditiveBlend;
            Scale = 0.45f;      // 虚空粒子最大
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

            // ── 运动：低频双向振荡 ──
            // 频率 0.07/0.09，比风更慢，产生"沉重"的飘动感
            Velocity *= 0.93f;  // 衰减最快，能量快速消散
            Velocity += new Vector2(
                MathF.Sin(Time * 0.07f) * 0.02f,
                MathF.Cos(Time * 0.09f) * 0.02f
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

            float stretch = 1.3f + _stretch * 4f;

            // 暗紫色调
            Color c = new Color(90, 20, 130) * Opacity;
            Color c2 = new Color(70, 15, 110) * (Opacity * 0.5f);

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
