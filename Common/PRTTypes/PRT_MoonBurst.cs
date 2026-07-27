// ============================================================
// PRT 粒子 — 月光爆裂（脉冲动画 + 十字星芒）
// ============================================================
//
// 【新概念】
//   1. 脉冲动画（Pulse）：先膨胀再缩小
//      用 MathF.Sin(progress * π) 产生 0→1→0 的波形
//   2. 十字星芒（Cross star）：四个方向的小光点
//      用循环 + 角度偏移实现
//   3. 分段淡出：前 30% 保持全透明，后 70% 线性淡出
//
// 【适用场景】
//   命中爆炸、技能释放、魔法阵闪光等
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
    /// 月光爆裂粒子 — 脉冲缩放 + 十字星芒 + 分段淡出。
    /// 适合作为"命中爆炸"或"技能释放"的闪光效果。
    /// 
    /// 【新概念】
    /// - 脉冲：Scale 随时间先增大后减小（爆炸扩散感）
    /// - 星芒：四个方向的小光点围绕中心旋转
    /// - 分段淡出：前段保持全亮，后段逐渐消失
    /// </summary>
    public class PRT_MoonBurst : BasePRT
    {
        /// <summary>纹理路径</summary>
        public override string Texture => "InnoVault/Assets/placeholder2";

        /// <summary>初始缩放（由 NewParticle 传入时决定）</summary>
        private float _startScale;

        /// <summary>旋转相位（用于星芒旋转和颜色微变）</summary>
        private float _phase;

        // ══════════════════════════════════════════════════════
        // SetProperty()
        // ══════════════════════════════════════════════════════
        public override void SetProperty()
        {
            PRTDrawMode = PRTDrawModeEnum.AdditiveBlend;

            // 保存 NewParticle 传入的初始 Scale
            _startScale = Scale;
        }

        // ══════════════════════════════════════════════════════
        // AI()
        // ══════════════════════════════════════════════════════
        public override void AI()
        {
            Time++;
            _phase += 0.08f;

            // 生命周期进度 [0, 1]
            float progress = Time / (float)Lifetime;

            // ── 脉冲缩放 ──
            // 用 Sin(progress * π) 产生 0→1→0 的波形
            // 乘以 0.3 振幅 + 0.7 偏移 = 范围 [0.4, 1.0]
            // 效果：先膨胀到 1.0 倍，再缩小到 0.4 倍
            float pulse = MathF.Sin(progress * MathHelper.Pi) * 0.3f + 0.7f;
            Scale = _startScale * pulse;

            // ── 分段淡出 ──
            // 前 30% 生命周期：完全不透明（爆炸瞬间最亮）
            // 后 70% 生命周期：线性淡出到 0
            if (progress < 0.3f)
                Opacity = 1f;
            else
                Opacity = 1f - (progress - 0.3f) / 0.7f;

            // 速度阻尼（爆炸扩散后逐渐停止）
            Velocity *= 0.93f;
        }

        // ══════════════════════════════════════════════════════
        // PreDraw() — 主光斑 + 内核亮点 + 十字星芒
        // ══════════════════════════════════════════════════════
        public override bool PreDraw(SpriteBatch sb)
        {
            var glow = VaultAsset.Light?.Value;
            if (glow == null) return false;

            // 颜色：淡蓝到白，随 _phase 轻微变化
            Color c = Color.Lerp(new Color(200, 230, 255), Color.White, _phase * 0.3f) * Opacity;

            float rot = Velocity.Length() > 0.5f ? Velocity.ToRotation() : Rotation;

            // ══════════════════════════════════════════════════
            // 第1层：主光斑
            // ══════════════════════════════════════════════════
            sb.Draw(glow, Position - Main.screenPosition, null, c * 0.6f, rot,
                glow.Size() / 2f, Scale * 1.2f, SpriteEffects.None, 0f);

            // ══════════════════════════════════════════════════
            // 第2层：内核亮点
            // ══════════════════════════════════════════════════
            sb.Draw(glow, Position - Main.screenPosition, null, Color.White * (Opacity * 0.5f), rot,
                glow.Size() / 2f, Scale * 0.4f, SpriteEffects.None, 0f);

            // ══════════════════════════════════════════════════
            // 第3层：十字星芒（四个方向的小光点）
            // ══════════════════════════════════════════════════
            // 星芒大小：随 _phase 闪烁（*2 频率）
            float starSize = Scale * 0.3f * (0.5f + MathF.Sin(_phase * 2f) * 0.3f);
            if (starSize > 0.05f) // 太小就不画了
            {
                for (int i = 0; i < 4; i++)
                {
                    // 四个方向：0°, 90°, 180°, 270°
                    // 加上 _phase * 0.5 使星芒缓慢旋转
                    float angle = i * MathHelper.PiOver2 + _phase * 0.5f;

                    // 星芒偏移位置：从中心沿 angle 方向偏移
                    Vector2 offset = angle.ToRotationVector2() * Scale * 8f;

                    sb.Draw(glow, Position + offset - Main.screenPosition, null,
                        c * 0.4f, angle, glow.Size() / 2f, starSize, SpriteEffects.None, 0f);
                }
            }

            return false;
        }
    }
}
