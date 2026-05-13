using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// Sky道基础弹幕 — 苍穹轨迹。
    ///
    /// 设计哲学：
    /// 天空的本质是"高远 + 浩瀚 + 变幻"。弹幕以中速直线飞行，
    /// 视觉上以苍穹拖尾（天弧 + 极光 + 天星）模拟天空的浩瀚感。
    ///
    /// 运动方式：
    /// - 中速直线飞行（AimBehavior）
    ///
    /// 视觉效果：
    /// - 苍穹拖尾：天弧 + 极光 + 天星（SkyTrailBehavior）
    ///
    /// 行为组合：
    /// - AimBehavior: 中速直线飞行
    /// - SkyTrailBehavior: 苍穹拖尾（天弧 + 极光 + 天星）
    /// </summary>
    public class SkyBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 8f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.5f, 0.9f)
            });
            Behaviors.Add(new SkyTrailBehavior
            {
                SuppressDefaultDraw = true,
                EnableGhostTrail = true,
                GhostColor = new Color(100, 140, 220, 140),
                CelestialArcColor = new Color(80, 140, 240, 200),
                AuroraBandColor = new Color(60, 180, 220, 200),
                ZenithMarkColor = new Color(160, 200, 255, 240),
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }
    }
}
