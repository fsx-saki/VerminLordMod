// ============================================================
// 冰元素弹幕 — IceBaseProj（冰锥） + IceShardProj（冰片）
// ============================================================
//
// 【行为组合】
// IceBaseProj:
//   - AimBehavior: 直线飞行（速度 10f）+ 蓝色光芒
//   - 无重力、无反弹（冰锥直线飞行）
//
// IceShardProj（继承 IceBaseProj）:
//   - 更小（scale=0.7），穿透更多（2 次），存活更短（90 帧）
//   - 共享纹理（使用 IceBaseProj 的纹理）
//
// 【粒子效果】
// - 冰锥拖尾（PRT_IceTrail）：冰蓝色，向后飘散
// - 死亡爆炸：8 个冰粒子向四周飞散
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
    /// 冰锥 — 基础冰元素弹幕。
    /// 高速直线飞行 + 冰蓝色光芒 + 冰拖尾粒子。
    /// 
    /// 【特点】
    /// - 速度最快（10f），穿透 1 个敌人
    /// - 无重力、无反弹（直线穿透）
    /// - 最简单的行为组合（只有 AimBehavior）
    /// </summary>
    [ElementInfo("I", "冰锥")]
    public class IceBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // ── AimBehavior：直线飞行（速度 10f） ──
            // 冰元素中速度最快
            // 蓝色光芒（RGB 0.2, 0.5, 0.9）
            Behaviors.Add(new AimBehavior(10f)
            {
                AutoRotate = true, EnableLight = true,
                LightColor = new Vector3(0.2f, 0.5f, 0.9f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 12; Projectile.height = 12; Projectile.scale = 1f;
            Projectile.timeLeft = 180;       // 存活 3 秒
            Projectile.penetrate = 1;         // 穿透 1 个敌人
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.aiStyle = -1;
        }

        /// <summary>
        /// 每帧生成冰拖尾粒子（33% 概率，比火焰/水更稀疏）。
        /// 粒子向后飘散，模拟冰晶碎片。
        /// </summary>
        protected override void OnAI()
        {
            if (Main.rand != null && Main.rand.NextBool(3))
            {
                var p = PRTLoader.NewParticle<PRT_IceTrail>(Projectile.Center,
                    Projectile.velocity * -0.15f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    new Color(200, 240, 255), 0.35f);
                if (p != null) p.Lifetime = 20;
            }
        }

        /// <summary>
        /// 死亡时生成 8 个冰粒子向四周飞散。
        /// </summary>
        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                var p = PRTLoader.NewParticle<PRT_IceTrail>(Projectile.Center,
                    Main.rand.NextVector2Circular(4f, 4f),
                    new Color(200, 240, 255), Main.rand.NextFloat(0.25f, 0.45f));
                if (p != null) p.Lifetime = 25 + Main.rand.Next(10);
            }
        }
    }

    /// <summary>
    /// 冰片 — 继承自 IceBaseProj。
    /// 更小更快的冰碎片，穿透 2 个敌人。
    /// 
    /// 【与 IceBaseProj 的区别】
    /// - scale=0.7（更小）
    /// - penetrate=2（穿透 2 个敌人）
    /// - timeLeft=90（存活 1.5 秒，更短更快）
    /// - 共享纹理（使用 IceBaseProj 的纹理文件）
    /// 
    /// 【纹理共享技巧】
    /// 通过 override Texture 指向父类的纹理路径，
    /// 避免为子类创建单独的纹理文件。
    /// </summary>
    [ElementInfo("I", "冰片")]
    public class IceShardProj : IceBaseProj
    {
        /// <summary>共享父类的纹理，不需要单独的图片文件</summary>
        public override string Texture => "VerminLordMod/Content/Projectiles/Elements/IceBaseProj";

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.scale = 0.7f;     // 更小
            Projectile.penetrate = 2;     // 穿透 2 个敌人
            Projectile.timeLeft = 90;     // 存活 1.5 秒
        }
    }
}
