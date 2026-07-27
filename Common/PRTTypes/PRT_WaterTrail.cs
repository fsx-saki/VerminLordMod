// ============================================================
// PRT 粒子 — 水弹拖尾（基础拖尾模板）
// ============================================================
//
// 【学习要点】
// 这是一个"标准拖尾粒子"的模板，其他元素拖尾（风、冰、虚空）
// 都采用完全相同的结构，仅颜色和运动参数不同。
//
// 【拖尾粒子通用模式】
//   1. SetProperty() 中设置初始 Scale 和 _stretch
//   2. AI() 中：Time++ → 计算进度 p → 衰减 Opacity/Scale → 
//      衰减 _stretch → 衰减 Velocity + 添加微扰
//   3. PreDraw() 中：沿速度方向拉伸 → 多层绘制（外层+内层+核心）
//
// 【如何自定义一个新拖尾粒子】
//   复制本文件，修改：
//   - 类名
//   - AI() 中的运动参数（速度衰减率、重力、摆动幅度）
//   - PreDraw() 中的颜色和拉伸系数
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
    /// 水弹拖尾粒子 — 蓝色调，带轻微重力下落和水平摆动。
    /// 作为"基础拖尾"的参考实现，其他元素拖尾均模仿此结构。
    /// </summary>
    public class PRT_WaterTrail : BasePRT
    {
        /// <summary>纹理路径（使用 InnoVault 内置圆形渐变图）</summary>
        public override string Texture => "VerminLordMod/Assets/Textures/Glows/CircleGlow";

        /// <summary>
        /// 拉伸系数。
        /// 从 1.0 逐渐衰减到 0.3，控制拖尾长度随生命周期的变化。
        /// </summary>
        private float _stretch;

        // ══════════════════════════════════════════════════════
        // SetProperty() — 初始化
        // ══════════════════════════════════════════════════════
        public override void SetProperty()
        {
            // 叠加混合模式（发光效果）
            PRTDrawMode = PRTDrawModeEnum.AdditiveBlend;

            // 初始大小
            Scale = 0.4f;

            // 初始拉伸系数（1.0 = 最长）
            _stretch = 1f;
        }

        // ══════════════════════════════════════════════════════
        // AI() — 每帧更新
        // ══════════════════════════════════════════════════════
        public override void AI()
        {
            // 帧计数器递增
            Time++;

            // 生命周期进度 [0, 1]
            float p = Time / (float)Lifetime;

            // 透明度：线性淡出，最大 0.6（半透明）
            Opacity = (1f - p) * 0.6f;

            // 缩放：每帧缩小 3%（指数衰减）
            Scale *= 0.97f;

            // 拉伸系数：逐渐趋向 0.3（拖尾越来越短）
            _stretch = MathHelper.Lerp(_stretch, 0.3f, 0.03f);

            // ── 运动 ──
            // 速度阻尼（每帧保留 96%）
            Velocity *= 0.96f;
            // 轻微重力（下落）
            Velocity.Y += 0.02f;
            // 水平正弦摆动（模拟水流波动）
            Velocity += new Vector2(MathF.Sin(Time * 0.1f) * 0.01f, 0);
        }

        // ══════════════════════════════════════════════════════
        // PreDraw() — 自定义绘制
        // ══════════════════════════════════════════════════════
        public override bool PreDraw(SpriteBatch sb)
        {
            // 获取发光纹理
            var glow = ModContent.Request<Texture2D>(Texture).Value;
            if (glow == null) return false;

            // 旋转角度：沿速度方向
            float rot = Velocity.Length() > 0.5f ? Velocity.ToRotation() : Rotation;

            // 拉伸长度 = 基础长度 + 拉伸系数 × 4
            float stretch = 1.2f + _stretch * 4f;

            // ── 颜色定义 ──
            // 外层：蓝色（60, 150, 255）
            Color c = new Color(60, 150, 255) * Opacity;
            // 内层：深蓝（30, 100, 200）
            Color c2 = new Color(30, 100, 200) * (Opacity * 0.5f);

            // 缩放向量：(长轴, 短轴) — 沿速度方向拉伸，垂直方向压扁
            Vector2 sv = new Vector2(stretch * Scale * 2f, Scale * 0.5f);

            // ── 三层绘制（从外到内） ──
            // 第1层：主体（半透明蓝）
            sb.Draw(glow, Position - Main.screenPosition, null, c * 0.5f, rot,
                glow.Size() / 2f, sv, SpriteEffects.None, 0f);
            // 第2层：中间层（深蓝）
            sb.Draw(glow, Position - Main.screenPosition, null, c2 * 0.6f, rot,
                glow.Size() / 2f, sv * 0.5f, SpriteEffects.None, 0f);
            // 第3层：核心亮点（白色）
            sb.Draw(glow, Position - Main.screenPosition, null, Color.White * (Opacity * 0.25f), rot,
                glow.Size() / 2f, sv * 0.2f, SpriteEffects.None, 0f);

            return false; // 禁止默认绘制
        }
    }
}
