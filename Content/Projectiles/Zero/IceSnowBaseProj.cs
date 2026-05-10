using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 冰道基础弹幕 — 冰晶。
    /// 冰道技术储备库的"冰晶"技术：
    /// - 直线飞行
    /// - 冰晶粒子拖尾
    /// - 命中时冰晶爆散
    /// - 附加冻结效果
    ///
    /// 行为组合：
    /// - AimBehavior: 直线飞行
    /// - DustTrailBehavior: 冰晶粒子拖尾
    /// - DustKillBehavior: 命中时冰晶爆散
    /// </summary>
    public class IceSnowBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 1. 直线飞行
            Behaviors.Add(new AimBehavior(speed: 10f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 冰晶粒子拖尾
            Behaviors.Add(new DustTrailBehavior(DustID.Ice, spawnChance: 1)
            {
                DustScale = 0.7f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                DustAlpha = 150,
                RandomSpeed = 0.3f
            });

            // 3. 命中时冰晶爆散
            Behaviors.Add(new DustKillBehavior(
                dustType: DustID.Ice,
                dustCount: 15,
                dustSpeed: 4f,
                dustScale: 1.0f
            )
            {
                NoGravity = true
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
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            // 冰晶微光
            Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 0.8f);
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 附加冻结效果
            target.AddBuff(BuffID.Frostburn, 120); // 冰霜灼烧
            target.AddBuff(BuffID.Slow, 180);       // 减速
            target.AddBuff(BuffID.Chilled, 60);     // 寒冷
        }
    }
}
