// ============================================================
// 火焰元素弹幕 — FireBaseProj（火焰弹） + FireBombProj（火焰炸弹）
// ============================================================
//
// 【架构说明】
// 所有元素弹幕继承自 BaseBullet（Common/BulletBehaviors/BaseBullet.cs），
// BaseBullet 使用"组合模式"（Composition Pattern）管理弹幕行为：
//
//   BaseBullet (sealed AI)
//     ├── foreach behavior.Update()    ← 依次执行所有注册的行为
//     └── OnAI()                       ← 子类扩展点（粒子效果等）
//
// 【行为系统（IBulletBehavior）】
// 每个行为负责一个独立功能，通过 Behaviors.Add() 注册：
//   - AimBehavior      : 自动瞄准/直线飞行
//   - GravityBehavior  : 重力下落
//   - BounceBehavior   : 物块反弹
//   - GlowDrawBehavior : 发光绘制
//   - HomingBehavior   : 追踪敌人
//   - RotateBehavior   : 自动旋转
//   - PullBehavior     : 吸引/牵引效果
//   - BoomerangBehavior: 回旋镖运动
//
// 【粒子系统（PRT）】
// 弹幕通过 PRTLoader.NewParticle<T>() 生成拖尾粒子，
// 粒子类位于 Common/PRTTypes/，详见 PRT_FireTrail.cs 的教学注释。
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

namespace VerminLordMod.Content.Projectiles.Elements
{
    /// <summary>
    /// 火焰弹 — 基础火焰弹幕。
    /// 抛物线轨迹 + 反弹 + 火焰拖尾粒子。
    /// 
    /// 【行为组合】
    /// - AimBehavior    : 直线飞行（速度 0 = 由初始速度决定）
    /// - GravityBehavior: 重力下落（0.12f/帧，最大 12f）
    /// - BounceBehavior : 物块反弹（最多 2 次，保留 40% 速度）
    /// 
    /// 【粒子效果】
    /// - 主火焰（每帧必生成）：大尺寸拖尾，向后飘散
    /// - 火星（1/3 概率）：小尺寸，高速上浮
    /// - 死亡爆炸：16 个粒子向四周飞散
    /// </summary>
    [ElementInfo("F", "火焰弹")]
    public class FireBaseProj : BaseBullet
    {
        // ══════════════════════════════════════════════════════
        // RegisterBehaviors() — 注册弹幕行为
        // ══════════════════════════════════════════════════════
        /// <summary>
        /// 在此方法中通过 Behaviors.Add() 注册所有行为。
        /// 行为按添加顺序执行，顺序可能影响最终效果。
        /// 注意：此方法在 OnSpawn 时自动调用（由 BaseBullet 的 sealed OnSpawn 驱动）。
        /// </summary>
        protected override void RegisterBehaviors()
        {
            // ── AimBehavior：自动瞄准/直线飞行 ──
            // 参数 speed=0：不主动加速，保持初始速度
            // AutoRotate=true：弹幕朝向速度方向
            // RotationOffset=PiOver2：纹理朝右，旋转偏移 90°
            Behaviors.Add(new AimBehavior(0f)
            {
                AutoRotate = true, RotationOffset = MathHelper.PiOver2,
                // EnableLight = true, LightColor = new Vector3(2f, 1.2f, 0.4f)
            });

            // ── GravityBehavior：重力 ──
            // Acceleration=0.12f：每帧 Y 速度增加 0.12
            // MaxFallSpeed=12f：最大下落速度
            // AutoRotate=true：弹幕朝速度方向旋转（下落时朝下）
            Behaviors.Add(new GravityBehavior { Acceleration = 0.12f, MaxFallSpeed = 12f, AutoRotate = true });

            // ── BounceBehavior：物块反弹 ──
            // MaxBounces=2：最多反弹 2 次
            // BounceFactor=0.4f：反弹后保留 40% 速度
            Behaviors.Add(new BounceBehavior { MaxBounces = 2, BounceFactor = 0.4f, });
        }

        // ══════════════════════════════════════════════════════
        // SetDefaults() — 弹幕基础属性
        // ══════════════════════════════════════════════════════
        public override void SetDefaults()
        {
            Projectile.width = 14; Projectile.height = 14; Projectile.scale = 1f;
            Projectile.timeLeft = 120;      // 存活 120 帧（2 秒）
            Projectile.penetrate = 99;       // 穿透 99 次（几乎无限，由 OnTileCollided 控制）
            Projectile.ignoreWater = false;   // 受水影响
            Projectile.tileCollide = true;   // 与物块碰撞
            Projectile.friendly = true;      // 对敌人造成伤害
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>(); // 蛊术伤害类型
            Projectile.aiStyle = -1;         // 禁用原版 AI
        }

        // ══════════════════════════════════════════════════════
        // OnAI() — 每帧自定义逻辑（在 behaviors.Update 之后调用）
        // ══════════════════════════════════════════════════════
        /// <summary>
        /// 每帧生成火焰拖尾粒子。
        /// 两种粒子同时生成，模拟真实火焰的层次感：
        ///   1. 主火焰 — 大尺寸，向后飘（每帧必生成）
        ///   2. 火星   — 小尺寸，向上飞溅（1/3 概率）
        /// 
        /// 【PRT 生成参数说明】
        /// PRTLoader.NewParticle<T>(position, velocity, color, scale)
        ///   - position : 生成位置（世界坐标）
        ///   - velocity : 初始速度（向量）
        ///   - color    : 初始颜色（部分 PRT 会忽略，如 PRT_FireTrail 使用内部颜色渐变）
        ///   - scale    : 初始缩放
        /// 返回的粒子对象可继续修改属性（如 Lifetime）。
        /// </summary>
        protected override void OnAI()
        {
            // Main.rand 在服务器端为 null，需要判空
            if (Main.rand != null)
            {
                // ── 主火焰（每帧必生成） ──
                // 位置：弹幕中心 + 随机偏移（半径 3px 圆形范围）
                // 速度：弹幕速度反向 × 0.15（向后飘）+ 随机扰动
                // 颜色：橙黄色（PRT_FireTrail 内部会做颜色渐变，此参数实际被忽略）
                // 缩放：2f（大尺寸火焰拖尾）
                var p = PRTLoader.NewParticle<PRT_FireTrail>(Projectile.Center
                    + Main.rand.NextVector2Circular(3f, 3f),
                    Projectile.velocity * -0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    new Color(255, 200, 80), 2f);
                // 设置粒子存活帧数（随机范围增加变化）
                if (p != null) p.Lifetime = 40 + Main.rand.Next(6);

                // ── 火星（1/3 概率） ──
                if (Main.rand.NextBool(3))
                {
                    // 小尺寸（0.5~1f），随机方向飞溅
                    var s = PRTLoader.NewParticle<PRT_FireTrail>(Projectile.Center,
                        new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, 4f)),
                        new Color(255, 180, 50), Main.rand.NextFloat(0.5f, 1f));
                    if (s != null) s.Lifetime = 40 + Main.rand.Next(10);
                }
            }
        }

        // ══════════════════════════════════════════════════════
        // OnTileCollided() — 碰撞物块时的自定义逻辑
        // ══════════════════════════════════════════════════════
        /// <summary>
        /// 碰到物块时减少穿透次数。
        /// 返回 true 表示弹幕应销毁，false 表示继续存在。
        /// 
        /// 注意：此方法在 behaviors.OnTileCollide 之后调用。
        /// BounceBehavior 已处理反弹逻辑，这里只控制销毁条件。
        /// </summary>
        protected override bool OnTileCollided(Vector2 oldVelocity)
        {
            Projectile.penetrate--;
            // 穿透次数 ≤ 0 时销毁
            return Projectile.penetrate <= 0;
        }
        
        // ══════════════════════════════════════════════════════
        // OnKilled() — 弹幕销毁时的效果
        // ══════════════════════════════════════════════════════
        /// <summary>
        /// 弹幕销毁时生成爆炸粒子效果。
        /// 16 个火焰粒子向四周飞散，模拟爆炸。
        /// </summary>
        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 16; i++)
            {
                var p = PRTLoader.NewParticle<PRT_FireTrail>(Projectile.Center,
                    Main.rand.NextVector2Circular(6f, 6f),  // 随机方向飞散
                    new Color(255, 200, 80), Main.rand.NextFloat(0.5f, 1.3f));
                if (p != null) p.Lifetime = 70 + Main.rand.Next(12);
            }
        }
    }

    /// <summary>
    /// 火焰炸弹 — 继承自 FireBaseProj。
    /// 抛物线轨迹 + 反弹 + 死亡时分裂为 6 个火焰弹。
    /// 
    /// 【与 FireBaseProj 的区别】
    /// - 重力更大（0.15），下落更快
    /// - 反弹次数更少（1 次），反弹保留速度更少（30%）
    /// - 死亡时向 6 个方向发射 FireBaseProj
    /// - 粒子缩放更大，视觉效果更夸张
    /// </summary>
    [ElementInfo("F", "火焰炸弹")]
    public class FireBombProj : FireBaseProj
    {
        // ══════════════════════════════════════════════════════
        // RegisterBehaviors() — 覆盖父类的行为注册
        // ══════════════════════════════════════════════════════
        /// <summary>
        /// 注意：此处完全覆盖父类的 RegisterBehaviors，
        /// 不会继承父类的 AimBehavior/GravityBehavior/BounceBehavior。
        /// 只有 Gravity 和 Bounce，无 Aim（炸弹靠重力飞行）。
        /// </summary>
        protected override void RegisterBehaviors()
        {
            // 重力更大，下落更快
            Behaviors.Add(new GravityBehavior { Acceleration = 0.15f, MaxFallSpeed = 10f, AutoRotate = true });
            // 反弹次数更少，保留速度更少
            Behaviors.Add(new BounceBehavior { MaxBounces = 1, BounceFactor = 0.3f });
        }

        // ══════════════════════════════════════════════════════
        // OnAI() — 每帧粒子效果
        // ══════════════════════════════════════════════════════
        /// <summary>
        /// 每帧生成火焰拖尾粒子（缩放比 FireBaseProj 更大）。
        /// 两种粒子：
        ///   1. 主火焰（每帧必生成）：缩放 5f，大尺寸拖尾
        ///   2. 火星（1/2 概率）：缩放 2~3f，随机方向飞溅
        /// </summary>
        protected override void OnAI()
        {
            // Main.rand 在服务器端为 null，需要判空
            if (Main.rand != null)
            {
                // ── 主火焰（每帧必生成） ──
                // 缩放 5f：大尺寸火焰拖尾，视觉上更醒目
                var p = PRTLoader.NewParticle<PRT_FireTrail>(Projectile.Center
                    + Main.rand.NextVector2Circular(3f, 3f),
                    Projectile.velocity * -0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    new Color(255, 200, 80), 5f);
                if (p != null) p.Lifetime = 40 + Main.rand.Next(6);

                // ── 火星（1/2 概率，比 FireBaseProj 的 1/3 更高） ──
                if (Main.rand.NextBool(2))
                {
                    // 缩放 2~3f：火星也更大
                    var s = PRTLoader.NewParticle<PRT_FireTrail>(Projectile.Center,
                        new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, 4f)),
                        new Color(255, 180, 50), Main.rand.NextFloat(2f, 3f));
                    if (s != null) s.Lifetime = 40 + Main.rand.Next(10);
                }
            }
        }

        // ══════════════════════════════════════════════════════
        // OnKilled() — 分裂为 6 个火焰弹
        // ══════════════════════════════════════════════════════
        /// <summary>
        /// 炸弹销毁时：
        /// 1. 调用 base.OnKilled() 生成爆炸粒子（16 个火焰粒子）
        /// 2. 向 6 个方向各发射一个 FireBaseProj（伤害减半，击退减半）
        /// 
        /// 【分裂弹幕技巧】
        /// 使用 MathHelper.TwoPi / 6 计算 6 等分角度，
        /// a.ToRotationVector2() 将角度转为单位向量，
        /// 乘以速度得到每个子弹幕的初始速度。
        /// </summary>
        protected override void OnKilled(int timeLeft)
        {
            base.OnKilled(timeLeft); // 先执行父类的爆炸粒子效果

            // 向 6 个方向分裂
            for (int i = 0; i < 6; i++)
            {
                // 计算等分角度（0°, 60°, 120°, ...）
                float a = i * MathHelper.TwoPi / 6f;

                // 生成子弹幕
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),  // 伤害来源
                    Projectile.Center,                 // 生成位置（炸弹中心）
                    a.ToRotationVector2() * 5f,        // 速度（方向向量 × 速度）
                    ModContent.ProjectileType<FireBaseProj>(), // 子弹幕类型
                    Projectile.damage / 2,             // 伤害减半
                    Projectile.knockBack * 0.5f,       // 击退减半
                    Projectile.owner                   // 所有者
                );
            }
        }
    }

    /// <summary>
    /// 滞留火焰 — 继承自 FireBaseProj。
    /// 落地后持续燃烧，产生上升火焰粒子效果。
    /// 
    /// 【与 FireBaseProj 的区别】
    /// - timeLeft=500：存活时间更长（约 8 秒）
    /// - 无 AimBehavior：不飞行，靠重力落地
    /// - MaxBounces=-1：无限反弹（几乎不弹起，BounceFactor=0.1 几乎粘地）
    /// - 粒子有向上速度（0, -7），模拟火焰上升
    /// - 粒子存活时间较短（30 帧），快速消散
    /// </summary>
    [ElementInfo("F", "滞留火焰")]
    public class RetentionFire : FireBaseProj {
        // ══════════════════════════════════════════════════════
        // SetDefaults() — 继承父类 + 覆盖个别属性
        // ══════════════════════════════════════════════════════
        /// <summary>
        /// 先调用 base.SetDefaults() 继承 FireBaseProj 的所有默认设置，
        /// 再覆盖 timeLeft 为 500 帧（约 8 秒）。
        /// </summary>
        public override void SetDefaults()
        {
            base.SetDefaults();      // 继承 FireBaseProj 的所有默认设置
            Projectile.timeLeft = 500; // 覆盖存活时间（更长）
        }

        // ══════════════════════════════════════════════════════
        // RegisterBehaviors() — 覆盖父类的行为注册
        // ══════════════════════════════════════════════════════
        /// <summary>
        /// 只有 Gravity 和 Bounce，无 Aim（不主动飞行）。
        /// MaxBounces=-1：无限反弹（配合 BounceFactor=0.1，几乎不弹起）。
        /// </summary>
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new GravityBehavior { Acceleration = 0.15f, MaxFallSpeed = 10f, AutoRotate = true });
            Behaviors.Add(new BounceBehavior { MaxBounces = -1, BounceFactor = 0.1f });
            Behaviors.Add(new DampingBehavior
            {
                DampingX = 0.95f,  // 水平衰减慢
                DampingY = 1f,  // 垂直衰减快
            });
        }
        // ══════════════════════════════════════════════════════
        // OnAI() — 每帧粒子效果
        // ══════════════════════════════════════════════════════
        /// <summary>
        /// 每帧生成上升火焰粒子。
        /// 两种粒子：
        ///   1. 主火焰（每帧必生成）：有向上速度（0, -7），模拟火焰上升
        ///   2. 火星（1/2 概率）：随机方向飞溅
        /// 
        /// 与 FireBaseProj 的关键区别：
        /// - 粒子速度加了 new Vector2(0, -7)，火焰向上飘
        /// - 粒子存活时间较短（30 帧 / 20 帧），快速消散
        /// </summary>
        protected override void OnAI()
        {
            // Main.rand 在服务器端为 null，需要判空
            if (Main.rand != null)
            {
                // ── 主火焰（每帧必生成） ──
                // 速度 = 弹幕速度反向 × 0.15 + 向上速度 (0,-7) + 随机扰动
                // 向上速度模拟火焰自然上升
                var p = PRTLoader.NewParticle<PRT_FireTrail>(Projectile.Center
                    + Main.rand.NextVector2Circular(3f, 3f),
                    Projectile.velocity * -0.15f + new Vector2(0,-7) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    new Color(255, 200, 80), 2.7f);
                if (p != null) p.Lifetime = 30 + Main.rand.Next(6);

                // ── 火星（1/2 概率） ──
                if (Main.rand.NextBool(2))
                {
                    var s = PRTLoader.NewParticle<PRT_FireTrail>(Projectile.Center,
                        new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, 4f)),
                        new Color(255, 180, 50), Main.rand.NextFloat(2f, 3f));
                    if (s != null) s.Lifetime = 20 + Main.rand.Next(10);
                }
            }
        }
    }
}
