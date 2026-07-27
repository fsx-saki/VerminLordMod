// ============================================================
// 月刃弹幕 — MoonBaseProj
//
// 元素弹幕系统的"月刃"（Moon Blade）实现。
// 继承自 BaseBullet（行为系统基类），使用 AimBehavior 实现
// 自机狙直线飞行，配合月光拖尾粒子和星点粒子营造
// 华丽的月光视觉效果。
//
// 【行为概述】
// 1. 沿初始方向直线飞行，自动旋转朝向速度方向
// 2. 每帧在弹幕尾部生成月光拖尾粒子（PRT_MoonTrail）
// 3. 每帧有 50% 概率生成月光星点粒子（PRT_MoonSparkle）
// 4. 撞击物块后销毁
// 5. 销毁时爆发大量星点粒子（19个）
// ============================================================

using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Common.PRTTypes;
using InnoVault.PRT;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.QuestItems;
using Microsoft.Xna.Framework.Graphics;

namespace VerminLordMod.Content.Projectiles.Elements
{
    /// <summary>
    /// 月刃弹幕 — 元素弹幕系统 "月刃" 元素的具体实现。
    /// 使用 <see cref="ElementInfoAttribute"/> 标记，可在 ElementTester 测试菜单中
    /// 以图标 "M"、名称 "月刃" 被选中和生成。
    /// 继承 <see cref="BaseBullet"/>，采用行为组合模式管理弹幕逻辑。
    /// </summary>
    [ElementInfo("M", "月刃")]
    public class MoonBaseProj : BaseBullet
    {
        /// <summary>
        /// 注册弹幕行为到行为列表。
        /// 此方法在弹幕生成时（<see cref="BaseBullet.OnSpawn"/>）被基类自动调用。
        /// 当前注册了 <see cref="AimBehavior"/>，速度为 0（保持初始速度不变），
        /// 并启用自动旋转，旋转偏移为 π/2（使弹幕纹理尖端朝前）。
        /// </summary>
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(0f)
            {
                AutoRotate = true,          // 自动旋转到速度方向
                RotationOffset = MathHelper.PiOver2, // 偏移 +90°，使弹幕尖端指向前方
            });
        }

        /// <summary>
        /// 设置弹幕默认属性。
        /// 由 Terraria 引擎在弹幕生成时自动调用。
        /// </summary>
        public override void SetDefaults()
        {
            Projectile.width = 14;          // 碰撞箱宽度 14 像素
            Projectile.height = 14;         // 碰撞箱高度 14 像素
            Projectile.scale = 1f;          // 缩放比例 1x
            Projectile.timeLeft = 120;      // 存活时间 120 帧（约 2 秒，60fps）
            Projectile.penetrate = 99;      // 穿透次数 99（几乎无限，实际由 OnTileCollided 控制销毁时机）
            Projectile.ignoreWater = false;  // 不忽略水（进入水中会减速）
            Projectile.tileCollide = true;   // 启用物块碰撞
            Projectile.friendly = true;      // 对敌人造成伤害（友方弹幕）
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>(); // 伤害类型：蛊术伤害
            Projectile.aiStyle = -1;         // 禁用原版 AI 样式，使用自定义行为系统
        }

        /// <summary>
        /// 纹理尺寸偏移量。
        /// 用于将粒子生成位置偏移到弹幕尾部，使拖尾效果从弹幕末端开始。
        /// X = 16 像素的水平偏移（假定弹幕纹理朝右时，+X 方向为前端）。
        /// </summary>
        private Vector2 offset = new Vector2(16, 0);

        /// <summary>
        /// 弹幕纹理引用。
        /// 在静态构造时加载，用于后续计算拖尾粒子的生成位置（纹理高度的一半）。
        /// </summary>
        private Texture2D Texturew = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/Elements/MoonBaseProj").Value;

        /// <summary>
        /// 自定义 AI 逻辑（由基类 <see cref="BaseBullet.AI"/> 在每帧行为循环后调用）。
        /// 负责生成月光特效粒子：
        /// 1. 月光拖尾粒子（PRT_MoonTrail）— 每帧生成，位于弹幕尾部
        /// 2. 月光星点粒子（PRT_MoonSparkle）— 每帧 50% 概率生成，随机飘落
        /// 
        /// 注意：Main.rand 在服务器端为 null，必须判空以避免空引用异常。
        /// </summary>
        protected override void OnAI()
        {
            // Main.rand 仅在客户端和单人模式下可用，服务器端为 null
            if (Main.rand != null)
            {
                // ── 生成月光拖尾粒子 ──
                // 位置：弹幕中心 + offset（前端方向）- 速度方向归一化 × 纹理高度的一半（回到尾部）
                // 速度：沿速度反方向，轻缓飘动（-0.15 倍）
                // 颜色：暖金色 (255, 200, 80)
                // 大小：纹理宽度 × 0.9
                var p = PRTLoader.NewParticle<PRT_MoonTrail>(
                    Projectile.Center + offset - Vector2.Normalize(Projectile.velocity) * Texturew.Size().Y / 2f,
                    Projectile.velocity * -0.15f,
                    new Color(255, 200, 80),
                    Texturew.Size().X * 0.9f
                );
                // 随机化粒子存活帧数：40~45 帧，使拖尾长度有自然变化
                if (p != null) p.Lifetime = 40 + Main.rand.Next(6);

                // ── 生成月光星点粒子（50% 概率） ──
                if (Main.rand.NextBool(2))
                {
                    // 位置：弹幕中心 + offset
                    // 速度：水平轻微随机漂移（±0.05），垂直随机下落（-4 ~ 4）
                    // 颜色：暖金色偏暗 (255, 180, 50)
                    // 大小：随机 0.1 ~ 0.3（小尺寸星点）
                    var s = PRTLoader.NewParticle<PRT_MoonSparkle>(
                        Projectile.Center + offset,
                        new Vector2(Main.rand.NextFloat(-0.05f, 0.05f), Main.rand.NextFloat(-0.05f, 0.05f)),
                        new Color(255, 180, 50),
                        Main.rand.NextFloat(0.1f, 0.3f)
                    );
                    // 随机化粒子存活帧数：60~69 帧
                    if (s != null) s.Lifetime = 60 + Main.rand.Next(10);
                }
            }
        }

        /// <summary>
        /// 物块碰撞回调（由基类 <see cref="BaseBullet.OnTileCollide"/> 在行为循环后调用）。
        /// 返回 true 表示弹幕应销毁，false 表示弹幕继续存在。
        /// 
        /// 当前实现：无论碰撞到什么物块，均返回 true（弹幕立即销毁）。
        /// 这使月刃在碰到墙壁/地面后消失，而不是反弹或穿透。
        /// </summary>
        /// <param name="oldVelocity">碰撞前的速度向量，可用于计算反弹</param>
        /// <returns>true → 销毁弹幕</returns>
        protected override bool OnTileCollided(Vector2 oldVelocity)
        {
            return true;
        }

        /// <summary>
        /// 弹幕销毁回调（由基类 <see cref="BaseBullet.OnKill"/> 在行为循环后调用）。
        /// 弹幕消失时爆发 19 个月光星点粒子，模拟碎裂/消散效果。
        /// 
        /// 粒子的生成位置在以弹幕中心为圆心、半径 25 像素的随机圆内，
        /// 速度极慢（±0.05），营造缓缓飘散的视觉效果。
        /// 存活时间 90~99 帧（约 1.5 秒），比飞行中的星点更持久。
        /// </summary>
        /// <param name="timeLeft">弹幕剩余时间（通常为 0，表示自然消亡或被销毁）</param>
        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 19; i++)
            {
                // 位置：弹幕中心 + offset 周围半径 25 像素的随机圆内
                // 速度：极缓慢的随机漂移（±0.05 像素/帧）
                // 颜色：暖金色 (255, 180, 50)
                // 大小：随机 0.1 ~ 0.3
                var s = PRTLoader.NewParticle<PRT_MoonSparkle>(
                    Projectile.Center + offset + new Vector2(
                        Main.rand.NextFloat(-25f, 25f),
                        Main.rand.NextFloat(-25f, 25f)
                    ),
                    new Vector2(
                        Main.rand.NextFloat(-0.05f, 0.05f),
                        Main.rand.NextFloat(-0.05f, 0.05f)
                    ),
                    new Color(255, 180, 50),
                    Main.rand.NextFloat(0.1f, 0.3f)
                );
                // 存活时间：90~99 帧，比飞行中的星点持续时间更长
                if (s != null) s.Lifetime = 90 + Main.rand.Next(10);
            }
        }
    }

}
