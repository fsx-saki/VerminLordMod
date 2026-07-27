// ============================================================
// PRT 粒子系统 — 火焰拖尾（最复杂示例，含完整教学注释）
// ============================================================
//
// 【PRT 系统概述】
// PRT = Particle (ParticleLibrary/InnoVault 的粒子框架)。
// 所有粒子继承自 BasePRT（位于 InnoVault.PRT 命名空间）。
//
// 【BasePRT 生命周期】（按调用顺序）
//   1. 构造方法          — 设置初始 Position、Velocity、Lifetime 等
//   2. SetProperty()     — ★ 初始化粒子属性（只调用一次）
//   3. AI()              — ★ 每帧更新逻辑（类似 ModProjectile.AI）
//   4. PreDraw(SpriteBatch) — ★ 自定义绘制（返回 false 则不再默认绘制）
//
// 【如何生成一个粒子】
//   在任意代码中调用：
//     PRTLoader.NewParticle<PRT_FireTrail>(position, velocity, lifetime);
//   或带初始 Scale：
//     PRTLoader.NewParticle<PRT_FireTrail>(position, velocity, lifetime, scale);
//   或带初始 Color：
//     PRTLoader.NewParticle<PRT_FireTrail>(position, velocity, color, scale);
//
// 【BasePRT 关键属性】
//   - Position  : 世界坐标位置（Vector2）
//   - Velocity  : 速度（Vector2），每帧自动加到 Position
//   - Lifetime  : 最大存活帧数（int）
//   - Time      : 当前已存活帧数（int），从 0 开始
//   - Opacity   : 透明度（float），默认 1.0
//   - Scale     : 缩放（float），默认 1.0
//   - Rotation  : 旋转角度（float），弧度制
//   - PRTDrawMode : 绘制模式枚举
//
// 【PRTDrawModeEnum 选项】
//   - AdditiveBlend  — 叠加混合（发光效果，火焰/魔法/光晕）
//   - AlphaBlend     — 标准透明混合（普通粒子）
//   - Custom         — 自定义混合
//
// 【通用绘制技巧】
//   1. 多层绘制：外层大而淡 + 内层小而亮 = 发光效果
//   2. 沿速度方向拉伸：用 Velocity.ToRotation() 作为旋转角
//   3. 生命周期颜色渐变：用 Time / Lifetime 做 Color.Lerp
//   4. 闪烁效果：用 Sin/Cos 做周期性变化
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
    /// 火焰拖尾粒子 — 最复杂的 PRT 示例，演示了：
    ///   - 闪烁动画（_flicker）
    ///   - 生命周期颜色渐变（黄→橙→红）
    ///   - 沿速度方向拉伸（stretch）
    ///   - 多层发光绘制（外层火焰 + 内层亮核）
    ///   - 上升加速 + 随机摆动
    /// </summary>
    public class PRT_FireTrail : BasePRT
    {
        // ── 纹理路径 ──────────────────────────────────────────
        /// <summary>
        /// 指定粒子使用的纹理。
        /// 
        /// 使用 InnoVault 内置纹理：
        ///   "InnoVault/Assets/placeholder2"  — 方形占位图
        ///   "InnoVault/Assets/Light"          — 圆形渐变发光图
        ///   "InnoVault/Assets/Star"           — 星形纹理
        /// 
        /// 使用自己的纹理：
        ///   把图片放到 Assets/Textures/ 目录下，然后写：
        ///   "VerminLordMod/Assets/Textures/MyTextureName"
        ///   （不需要文件扩展名，tModLoader 自动加载）
        /// </summary>
        public override string Texture => "VerminLordMod/Assets/Textures/Glows/CircleGlow";

        // ── 私有字段 ──────────────────────────────────────────
        /// <summary>初始缩放值，用于在 AI 中做基准衰减</summary>
        private float _startScale;

        /// <summary>闪烁系数 [0.4~1.0]，由 Sin 波驱动，模拟火焰跳动</summary>
        private float _flicker;

        // ══════════════════════════════════════════════════════
        // 阶段 1：SetProperty() — 粒子属性初始化（仅调用一次）
        // ══════════════════════════════════════════════════════
        /// <summary>
        /// 在此方法中设置粒子的初始属性。
        /// 注意：Position、Velocity、Lifetime 由 NewParticle 参数传入，
        /// 不需要在这里设置。
        /// </summary>
        public override void SetProperty()
        {
            // ── 绘制模式 ──
            // AdditiveBlend = 叠加混合，适合发光/火焰/魔法效果
            PRTDrawMode = PRTDrawModeEnum.AdditiveBlend;

            // ── 初始大小 ──
            // ★★★ 重要：Scale 的值从哪里来？ ★★★
            //
            // 情况 A：NewParticle 传入了 scale 参数
            //   PRTLoader.NewParticle<PRT_FireTrail>(pos, vel, color, 5f);
            //   → BasePRT.Scale 已经被设为 5f，这里不要再覆盖！
            //
            // 情况 B：NewParticle 没有传 scale 参数
            //   PRTLoader.NewParticle<PRT_FireTrail>(pos, vel, lifetime);
            //   → BasePRT.Scale 是默认值 1f，这里可以设一个默认值
            //
            // ★ 正确的做法：只在 Scale 还是默认值时才随机赋值
            //   这样既保留了 NewParticle 传入的值，又给没传参的情况一个默认值
            if (Scale <= 0f || Scale == 1f)
                Scale = Main.rand?.NextFloat(0.3f, 0.6f) ?? 0.4f;

            // 保存初始值，供 AI 中做衰减基准
            _startScale = Scale;
        }

        // ══════════════════════════════════════════════════════
        // 阶段 2：AI() — 每帧更新逻辑
        // ══════════════════════════════════════════════════════
        /// <summary>
        /// 每帧调用一次，类似 Update。
        /// 在这里控制粒子的运动、缩放、透明度等随时间的变化。
        /// 注意：Velocity 会自动加到 Position，不需要手动移动。
        /// </summary>
        public override void AI()
        {
            // ── 帧计数器 ──
            // Time 从 0 开始，每帧 +1，直到达到 Lifetime 后粒子自动销毁
            Time++;

            // ── 生命周期进度 ──
            // p 从 0→1，表示粒子从出生到消亡的进度
            float p = Time / (float)Lifetime;

            // ── 透明度 ──
            // 使用 (1 - p²) 曲线：前期缓慢衰减，后期快速淡出
            // 比线性淡出更自然
            Opacity = (1f - p * p) * 0.8f;

            // ── 闪烁效果 ──
            // 用 Sin 波产生周期性变化，模拟火焰跳动
            // Position.X 作为相位偏移，使不同位置的火焰闪烁不同步
            _flicker = MathF.Sin(Time * 0.3f + Position.X * 0.5f) * 0.02f + 0.7f;
            // 结果范围：0.68 ~ 0.72（0.02振幅 + 0.7偏移）

            // ── 缩放衰减 ──
            // 初始大小 × (1 - 进度×0.6) × 闪烁系数
            // 粒子越老越小，同时叠加闪烁
            Scale = _startScale * (1f - p * 0.6f) * _flicker;

            // ── 运动控制 ──
            // 上升加速（火焰自然上飘），越老上升越慢
            Velocity.Y -= 0.2f * (1f - p * 0.5f);

            // 水平随机摆动（模拟火焰被风吹动）
            Velocity.X += MathF.Sin(Time * 0.15f) * 0.07f;

            // 速度阻尼，粒子逐渐减速
            Velocity *= 0.95f;
        }

        // ══════════════════════════════════════════════════════
        // 阶段 3：PreDraw() — 自定义绘制
        // ══════════════════════════════════════════════════════
        /// <summary>
        /// 自定义绘制逻辑。
        /// 返回 false 表示"我已经自己画了，不需要默认绘制"。
        /// 返回 true 表示"画完我的内容后，再执行默认绘制"。
        /// 
        /// 参数 sb：Terraria 的 SpriteBatch，已处于 Begin 状态，
        /// 直接调用 sb.Draw() 即可。
        /// </summary>
        public override bool PreDraw(SpriteBatch sb)
        {
            // ── 获取纹理 ──
            // ★★★ 如何更换纹理 ★★★
            //
            // 方式一：InnoVault 内置纹理（无需额外文件）
            //   VaultAsset.Light?.Value        — 圆形渐变发光图（最常用）
            //   VaultAsset.placeholder?.Value  — 方形占位图
            //   VaultAsset.Star?.Value         — 星形纹理
            //
            // 方式二：使用自己的纹理（推荐）
            //   1. 将图片文件放到项目 Assets/Textures/ 目录下
            //      例如：Assets/Textures/MyGlow.png
            //   2. 在 tModLoader 中，图片会自动加载为 Texture2D
            //   3. 代码中这样引用：
            //      Texture2D glow = ModContent.Request<Texture2D>(
            //          "VerminLordMod/Assets/Textures/MyGlow").Value;
            //
            // 方式三：使用原版 Terraria 纹理
            //   Texture2D glow = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            //   Texture2D glow = Terraria.GameContent.TextureAssets.Extra[98].Value;
            //
            // 方式四：使用其他 Mod 的纹理（需建立 ModReference）
            //   if (ModLoader.TryGetMod("OtherModName", out Mod otherMod))
            //       Texture2D glow = otherMod.Assets.Request<Texture2D>(
            //           "Assets/Textures/Foo").Value;
            Texture2D glow = ModContent.Request<Texture2D>("VerminLordMod/Assets/Textures/Glows/CircleGlow").Value;
            if (glow == null) return false; // 纹理未加载则跳过

            // ── 生命周期颜色渐变 ──
            // 三段式：亮黄 → 橙 → 暗红
            float p = Time / (float)Lifetime;
            Color color;
            if (p < 0.3f)
                // 阶段1（0~30%）：亮黄白 → 亮橙
                color = Color.Lerp(new Color(255, 240, 200), new Color(255, 200, 80), p / 0.3f);
            else if (p < 0.6f)
                // 阶段2（30~60%）：亮橙 → 橙红
                color = Color.Lerp(new Color(255, 200, 80), new Color(255, 120, 40), (p - 0.3f) / 0.3f);
            else
                // 阶段3（60~100%）：橙红 → 暗红（透明度渐变为0）
                color = Color.Lerp(new Color(255, 120, 40), new Color(180, 40, 0, 0), (p - 0.6f) / 0.4f);

            // 叠加透明度和闪烁
            color *= Opacity * _flicker;

            // ── 计算旋转和拉伸 ──
            // 如果有速度，沿速度方向旋转；否则用默认 Rotation
            float rot = Velocity.Length() > 0.5f ? Velocity.ToRotation() : Rotation;

            // ── 缩放向量（sv）决定绘制大小 ──
            // ★★★ 重要：sv 必须使用 Scale，否则 NewParticle 传入的 scale 无效！ ★★★
            //
            // 选项 A：正方形比例，由 Scale 控制大小（推荐，简单直观）
            //   → NewParticle 传 scale=5 就是 5 倍大
            Vector2 sv = new(Scale, Scale);
            //
            // 选项 B：沿速度方向拉伸（火焰拖尾效果）
            //   float stretch = 1.2f + (1f - p) * 2f;
            //   Vector2 sv = new(stretch * Scale * 2f, Scale * 0.6f);
            //
            // 选项 C：固定大小，忽略 Scale（不推荐，除非你确定不需要缩放）
            //   Vector2 sv = new(1, 1);

            // ── 第1层：火焰主体（外层，半透明） ──
            sb.Draw(
                glow,                           // 纹理
                Position - Main.screenPosition,  // 屏幕坐标
                null,                           // 源矩形（null = 整张图）
                color * 0.9f,                   // 颜色 × 透明度
                rot,                            // 旋转角度
                glow.Size() / 2f,               // 原点（纹理中心）
                sv,                             // 缩放（拉伸）
                SpriteEffects.None,
                0f
            );

            // ── 第2层：火焰内核（内层，小而亮） ──
            // 用更小的缩放（sv * 0.3f）和更亮的颜色
            // 两层叠加 = 发光效果
            sb.Draw(
                glow,
                Position - Main.screenPosition,
                null,
                new Color(255, 255, 220) * (Opacity * 0.3f * _flicker),
                rot,
                glow.Size() / 2f,
                sv * 0.3f,  // 内核更小
                SpriteEffects.None,
                0f
            );

            // 返回 false 表示"我已经画完了，不需要默认绘制"
            return false;
        }
    }
}
