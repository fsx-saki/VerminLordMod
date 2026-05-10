using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 风道基础弹幕 — 风刃。
    /// 风道技术储备库的"风刃"技术：
    /// - 高速直线飞行，高穿透
    /// - 产生风之粒子拖尾
    /// - 命中时产生风之爆散
    ///
    /// 行为组合：
    /// - AimBehavior: 直线飞行
    /// - DustTrailBehavior: 风之粒子拖尾
    /// - DustKillBehavior: 命中时风之爆散
    /// </summary>
    public class WindBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 1. 高速直线飞行
            Behaviors.Add(new AimBehavior(speed: 14f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 风之粒子拖尾（淡青色）
            Behaviors.Add(new DustTrailBehavior(DustID.Cloud, spawnChance: 1)
            {
                DustScale = 0.7f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                DustAlpha = 120,
                RandomSpeed = 0.3f
            });

            // 3. 命中时风之爆散
            Behaviors.Add(new DustKillBehavior(
                dustType: DustID.Cloud,
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
            Projectile.penetrate = 3; // 穿透 3 个敌人
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            // 风之微光
            Lighting.AddLight(Projectile.Center, 0.3f, 0.5f, 0.7f);
        }
    }
}
