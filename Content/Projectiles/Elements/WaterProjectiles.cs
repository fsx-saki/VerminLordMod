// ============================================================
// 水元素弹幕 — WaterBaseProj（水弹） + WaterSlashProj（水刃）
// ============================================================
//
// 【行为组合】
// WaterBaseProj:
//   - AimBehavior    : 直线飞行 + 蓝色光芒
//   - GravityBehavior: 微弱重力（0.08f）
//   - GlowDrawBehavior: 蓝色发光绘制（2 层）
//
// WaterSlashProj（继承 WaterBaseProj）:
//   - AimBehavior    : 直线飞行（速度 8f）
//   - RotateBehavior : 自动旋转（0.15f/帧）
//   - GlowDrawBehavior: 蓝色发光绘制（3 层，更大更亮）
//
// 【粒子效果】
// - 水弹拖尾（PRT_WaterTrail）：蓝色，向后飘散
// - 死亡爆炸：10 个水粒子向四周飞散
// ============================================================

using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Common.PRTTypes;
using VerminLordMod.Content.Items.QuestItems;
using InnoVault.PRT;

namespace VerminLordMod.Content.Projectiles.Elements
{
    /// <summary>
    /// 水弹 — 基础水元素弹幕。
    /// 直线飞行 + 微弱重力 + 蓝色发光 + 水拖尾粒子。
    /// 
    /// 【新概念：GlowDrawBehavior】
    /// 这是一个发光绘制行为，在弹幕周围绘制多层光晕。
    ///   - GlowColor       : 发光颜色
    ///   - GlowLayers      : 光晕层数（越多越亮）
    ///   - GlowBaseScale   : 基础缩放
    ///   - GlowScaleIncrement: 每层缩放增量
    ///   - GlowBaseAlpha   : 基础透明度
    ///   - GlowAlphaDecay  : 每层透明度衰减
    ///   - GlowAlphaMultiplier: 透明度乘数
    /// </summary>
    [ElementInfo("W", "水弹")]
    public class WaterBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // ── AimBehavior：直线飞行 ──
            // speed=0：保持初始速度
            // 蓝色光芒（RGB 0.3, 0.6, 1.0）
            Behaviors.Add(new AimBehavior(0f)
            {
                AutoRotate = true, RotationOffset = MathHelper.PiOver2,
                EnableLight = true, LightColor = new Vector3(0.3f, 0.6f, 1.0f)
            });

            // ── GravityBehavior：微弱重力 ──
            // 比火焰弹（0.12）更轻，下落更慢
            Behaviors.Add(new GravityBehavior { Acceleration = 0.08f, MaxFallSpeed = 6f });

            // ── GlowDrawBehavior：发光绘制 ──
            // 2 层蓝色光晕，基础缩放 1.2，每层 +0.3
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(30, 100, 200), GlowLayers = 2,
                GlowBaseScale = 1.2f, GlowScaleIncrement = 0.3f,
                GlowBaseAlpha = 0.4f, GlowAlphaDecay = 0.1f, GlowAlphaMultiplier = 0.3f
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16; Projectile.height = 16; Projectile.scale = 1.5f;
            Projectile.timeLeft = 300;      // 存活 5 秒
            Projectile.penetrate = 1;        // 穿透 1 个敌人
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.aiStyle = -1;
        }

        /// <summary>
        /// 每帧生成水拖尾粒子（50% 概率）。
        /// 粒子速度 = 弹幕速度反向 × 0.15 + 随机扰动
        /// 产生"水花向后飞溅"的效果。
        /// </summary>
        protected override void OnAI()
        {
            if (Main.rand != null && Main.rand.NextBool(2))
            {
                var p = PRTLoader.NewParticle<PRT_WaterTrail>(Projectile.Center,
                    Projectile.velocity * -0.15f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    new Color(60, 150, 255), 0.4f);
                if (p != null) p.Lifetime = 18;
            }
        }

        /// <summary>
        /// 死亡时生成 10 个水粒子向四周飞散。
        /// </summary>
        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                var p = PRTLoader.NewParticle<PRT_WaterTrail>(Projectile.Center,
                    Main.rand.NextVector2Circular(4f, 4f),
                    new Color(60, 150, 255), Main.rand.NextFloat(0.3f, 0.5f));
                if (p != null) p.Lifetime = 20 + Main.rand.Next(10);
            }
        }
    }

    /// <summary>
    /// 水刃 — 继承自 WaterBaseProj。
    /// 高速旋转的水刃，穿透无限，存活时间短。
    /// 
    /// 【与 WaterBaseProj 的区别】
    /// - 有 RotateBehavior 自动旋转
    /// - 发光 3 层（更华丽）
    /// - 穿透无限（penetrate = -1）
    /// - 存活时间短（45 帧 = 0.75 秒）
    /// - 无重力（覆盖 RegisterBehaviors，不添加 GravityBehavior）
    /// </summary>
    [ElementInfo("W", "水刃")]
    public class WaterSlashProj : WaterBaseProj
    {
        protected override void RegisterBehaviors()
        {
            // ── AimBehavior：直线飞行（速度 8f） ──
            // 注意：这里覆盖了父类的 RegisterBehaviors，
            // 不会继承父类的 AimBehavior(0f) + GravityBehavior + GlowDrawBehavior
            Behaviors.Add(new AimBehavior(8f) { AutoRotate = true });

            // ── RotateBehavior：自动旋转 ──
            // 每帧旋转 0.15 弧度，产生"回旋刃"效果
            Behaviors.Add(new RotateBehavior { RotationSpeed = 0.15f });

            // ── GlowDrawBehavior：3 层发光（比水弹更华丽） ──
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(30, 100, 200), GlowLayers = 3,
                GlowBaseScale = 1.5f, GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f, GlowAlphaDecay = 0.15f, GlowAlphaMultiplier = 0.3f
            });
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.penetrate = -1;   // 无限穿透
            Projectile.scale = 1.8f;     // 更大
            Projectile.timeLeft = 45;    // 存活 0.75 秒（快速斩击）
        }
    }
}
