// ============================================================
// 虚空元素弹幕 — VoidBaseProj（虚空弹） + VoidHomingProj（追踪弹）
// ============================================================
//
// 【行为组合】
// VoidBaseProj:
//   - HomingBehavior  : 追踪敌人（速度 7f，范围 800px）
//   - PullBehavior    : 吸引敌人（范围 200px，带旋转）
//   - GlowDrawBehavior: 紫色发光（2 层）
//
// VoidHomingProj（继承 VoidBaseProj）:
//   - HomingBehavior  : 更强追踪（速度 12f，范围 1000px）
//   - tileCollide     : true（可碰撞物块）
//
// 【粒子效果】
// - 虚空拖尾（PRT_VoidTrail）：紫色，向后飘散
// - 死亡爆炸：12 个虚空粒子向四周飞散
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
    /// 虚空弹 — 基础虚空元素弹幕。
    /// 追踪敌人 + 吸引效果 + 紫色发光 + 虚空拖尾。
    /// 
    /// 【新概念：HomingBehavior】
    /// 追踪行为，自动飞向最近的敌人。
    ///   - Speed          : 追踪速度
    ///   - TrackingWeight : 转向权重（1/25 = 每帧转 1/25 的角度差）
    ///   - Range          : 检测范围（像素）
    ///   - AutoRotate     : 自动朝向速度方向
    /// 
    /// 【新概念：Projectile.alpha】
    /// 弹幕透明度（0=不透明，255=全透明）。
    /// 这里设 alpha=20 使虚空弹略微透明，增加神秘感。
    /// </summary>
    [ElementInfo("V", "虚空弹")]
    public class VoidBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // ── HomingBehavior：追踪敌人 ──
            // Speed=7f         : 追踪速度
            // TrackingWeight=1/25: 每帧转 1/25 的角度差（平滑追踪）
            // Range=800f       : 检测范围 800px
            // AutoRotate=true  : 弹幕朝向速度方向
            Behaviors.Add(new HomingBehavior
            {
                Speed = 7f, TrackingWeight = 1f / 25f,
                Range = 800f, AutoRotate = true
            });

            // ── PullBehavior：吸引敌人 ──
            // 范围 200px（比旋风更大），力度 0.12
            // TangentFactor=0.3：较强的切向力（敌人围绕弹幕旋转）
            Behaviors.Add(new PullBehavior
            {
                PullRange = 200f, PullStrength = 0.12f,
                TangentFactor = 0.3f, MaxPullSpeed = 6f
            });

            // ── GlowDrawBehavior：紫色发光（2 层） ──
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(80, 20, 130), GlowLayers = 2,
                GlowBaseScale = 1.3f, GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f, GlowAlphaDecay = 0.15f, GlowAlphaMultiplier = 0.3f
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16; Projectile.height = 16; Projectile.scale = 1f;
            Projectile.timeLeft = 300;       // 存活 5 秒
            Projectile.penetrate = 1;         // 穿透 1 个敌人
            Projectile.tileCollide = false;   // 穿墙（虚空弹不受物块限制）
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.alpha = 20;            // 略微透明
            Projectile.aiStyle = -1;
        }

        /// <summary>
        /// 每帧生成虚空拖尾粒子（50% 概率）。
        /// 粒子向后飘散，模拟虚空能量残留。
        /// </summary>
        protected override void OnAI()
        {
            if (Main.rand != null && Main.rand.NextBool(2))
            {
                var p = PRTLoader.NewParticle<PRT_VoidTrail>(Projectile.Center,
                    Projectile.velocity * -0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    new Color(90, 20, 130), 0.45f);
                if (p != null) p.Lifetime = 25;
            }
        }

        /// <summary>
        /// 死亡时生成 12 个虚空粒子向四周飞散。
        /// 粒子数量最多（比其他元素多），体现虚空能量的爆发感。
        /// </summary>
        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 12; i++)
            {
                var p = PRTLoader.NewParticle<PRT_VoidTrail>(Projectile.Center,
                    Main.rand.NextVector2Circular(5f, 5f),
                    new Color(90, 20, 130), Main.rand.NextFloat(0.3f, 0.5f));
                if (p != null) p.Lifetime = 20 + Main.rand.Next(10);
            }
        }
    }

    /// <summary>
    /// 追踪弹 — 继承自 VoidBaseProj。
    /// 更强的追踪能力，可碰撞物块。
    /// 
    /// 【与 VoidBaseProj 的区别】
    /// - 覆盖 RegisterBehaviors：只有 HomingBehavior（无 PullBehavior）
    /// - 追踪更强：速度 12f，TrackingWeight 1/15，范围 1000px
    /// - tileCollide = true（可碰撞物块）
    /// - penetrate = 3（穿透 3 个敌人）
    /// - timeLeft = 180（存活 3 秒）
    /// - 共享纹理（使用 VoidBaseProj 的纹理文件）
    /// </summary>
    [ElementInfo("V", "追踪弹")]
    public class VoidHomingProj : VoidBaseProj
    {
        /// <summary>共享父类的纹理</summary>
        public override string Texture => "VerminLordMod/Content/Projectiles/Elements/VoidBaseProj";

        protected override void RegisterBehaviors()
        {
            // ── 更强追踪 ──
            // 速度更快（12f），转向更灵敏（1/15），范围更远（1000px）
            Behaviors.Add(new HomingBehavior
            {
                Speed = 12f, TrackingWeight = 1f / 15f,
                Range = 1000f, AutoRotate = true
            });
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.penetrate = 3;     // 穿透 3 个敌人
            Projectile.timeLeft = 180;    // 存活 3 秒
            Projectile.tileCollide = true; // 可碰撞物块（不再穿墙）
        }
    }
}
