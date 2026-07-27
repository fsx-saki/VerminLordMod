// ============================================================
// 风元素弹幕 — WindBaseProj（风刃） + WindCycloneProj（旋风）
// ============================================================
//
// 【行为组合】
// WindBaseProj:
//   - AimBehavior      : 直线飞行（速度 8f）+ 绿色光芒
//   - BoomerangBehavior: 回旋镖运动（飞出 → 返回）
//   - GlowDrawBehavior : 青白色发光（2 层）
//
// WindCycloneProj（继承 WindBaseProj）:
//   - PullBehavior     : 吸引敌人（范围 120px）
//
// 【粒子效果】
// - 风刃拖尾（PRT_WindTrail）：青白色，向后飘散
// - 旋风：围绕中心旋转生成粒子
// - 死亡爆炸：8 个风粒子向四周飞散
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
    /// 风刃 — 回旋镖式风元素弹幕。
    /// 飞出后返回，带旋转和发光效果。
    /// 
    /// 【新概念：BoomerangBehavior】
    /// 回旋镖行为，分两个阶段：
    ///   1. 飞出阶段（OutwardFrames 帧）：以 OutwardSpeed 速度飞出
    ///   2. 返回阶段：以 ReturnSpeed 速度返回，ReturnAccel 加速度
    /// SpinSpeed 控制飞行中的自旋速度。
    /// 
    /// 【新概念：usesLocalNPCImmunity】
    /// 使用本地无敌帧（每个 NPC 独立计算冷却），
    /// 适合穿透型弹幕，避免同一弹幕对同一 NPC 造成多次伤害。
    /// </summary>
    [ElementInfo("Wi", "风刃")]
    public class WindBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // ── AimBehavior：直线飞行（速度 8f） ──
            // 绿色光芒（RGB 0.3, 0.7, 0.5）
            Behaviors.Add(new AimBehavior(8f)
            {
                AutoRotate = true, EnableLight = true,
                LightColor = new Vector3(0.3f, 0.7f, 0.5f)
            });

            // ── BoomerangBehavior：回旋镖运动 ──
            // OutwardSpeed=14f  : 飞出速度
            // ReturnSpeed=16f   : 返回速度（比飞出快）
            // OutwardFrames=25  : 飞出阶段持续 25 帧
            // SpinSpeed=0.4f    : 自旋速度
            // ReturnAccel=0.6f  : 返回加速度
            Behaviors.Add(new BoomerangBehavior
            {
                OutwardSpeed = 14f, ReturnSpeed = 16f, OutwardFrames = 25,
                SpinSpeed = 0.4f, ReturnAccel = 0.6f
            });

            // ── GlowDrawBehavior：青白色发光（2 层） ──
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(160, 240, 210), GlowLayers = 2,
                GlowBaseScale = 1.3f, GlowScaleIncrement = 0.3f,
                GlowBaseAlpha = 0.4f, GlowAlphaDecay = 0.12f, GlowAlphaMultiplier = 0.25f
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 18; Projectile.height = 18; Projectile.scale = 1.1f;
            Projectile.timeLeft = 180;          // 存活 3 秒
            Projectile.penetrate = -1;           // 无限穿透
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;  // 每个 NPC 独立冷却
            Projectile.localNPCHitCooldown = 10;     // 同一 NPC 冷却 10 帧
            Projectile.aiStyle = -1;
        }

        /// <summary>
        /// 每帧生成风拖尾粒子（50% 概率）。
        /// 粒子向后飘散，模拟风刃划过的气流。
        /// </summary>
        protected override void OnAI()
        {
            if (Main.rand != null && Main.rand.NextBool(2))
            {
                var p = PRTLoader.NewParticle<PRT_WindTrail>(Projectile.Center,
                    Projectile.velocity * -0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    new Color(180, 245, 230), 0.4f);
                if (p != null) p.Lifetime = 14;
            }
        }

        /// <summary>
        /// 死亡时生成 8 个风粒子向四周飞散。
        /// </summary>
        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                var p = PRTLoader.NewParticle<PRT_WindTrail>(Projectile.Center,
                    Main.rand.NextVector2Circular(6f, 6f),
                    new Color(180, 245, 230), Main.rand.NextFloat(0.3f, 0.5f));
                if (p != null) p.Lifetime = 20 + Main.rand.Next(10);
            }
        }
    }

    /// <summary>
    /// 旋风 — 继承自 WindBaseProj。
    /// 静止的旋风，吸引周围敌人。
    /// 
    /// 【与 WindBaseProj 的区别】
    /// - 覆盖 RegisterBehaviors：只有 PullBehavior，无 Aim/Boomerang
    /// - tileCollide = false：穿墙
    /// - 尺寸更大（40×40，scale=2）
    /// - 每 3 帧在周围生成旋转的粒子环
    /// 
    /// 【新概念：PullBehavior】
    /// 吸引行为，将范围内的敌人拉向中心。
    ///   - PullRange    : 吸引范围（像素）
    ///   - PullStrength : 吸引力度
    ///   - TangentFactor: 切向力（产生旋转效果）
    ///   - MaxPullSpeed : 最大吸引速度
    /// </summary>
    [ElementInfo("Wi", "旋风")]
    public class WindCycloneProj : WindBaseProj
    {
        /// <summary>粒子生成计时器</summary>
        private int _spawnTimer;

        protected override void RegisterBehaviors()
        {
            // ── PullBehavior：吸引敌人 ──
            // 范围 120px，力度 0.08，切向力 0.2（产生旋转吸入效果）
            Behaviors.Add(new PullBehavior
            {
                PullRange = 120f, PullStrength = 0.08f,
                TangentFactor = 0.2f, MaxPullSpeed = 4f
            });
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 40; Projectile.height = 40;
            Projectile.timeLeft = 120;       // 存活 2 秒
            Projectile.tileCollide = false;  // 穿墙
            Projectile.scale = 2f;
        }

        /// <summary>
        /// 每 3 帧在周围生成旋转的粒子环。
        /// 粒子沿圆周均匀分布，产生"旋风"的视觉效果。
        /// </summary>
        protected override void OnAI()
        {
            _spawnTimer++;
            if (_spawnTimer % 3 == 0)
            {
                // 随机角度
                float a = Main.rand.NextFloat(MathHelper.TwoPi);
                // 粒子位置：中心 + 角度方向 × 25px（旋风边缘）
                // 粒子速度：沿切线方向 × 0.5（缓慢旋转）
                var p = PRTLoader.NewParticle<PRT_WindTrail>(
                    Projectile.Center + a.ToRotationVector2() * 25f,
                    a.ToRotationVector2() * 0.5f,
                    new Color(180, 245, 230), 0.5f);
                if (p != null) p.Lifetime = 25;
            }
        }
    }
}
