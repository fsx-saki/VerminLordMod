using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 水滴弹幕 — 用于水弹崩解时的飞溅效果。
    ///
    /// 使用 BaseBullet + WaterTrailBehavior 实现：
    /// - 受重力影响，碰到物块会反弹（最多 2 次），不能攻击 NPC
    /// - 带 WaterTrail 拖尾，消失时产生水花飞溅粒子
    /// </summary>
    public class WaterDropProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 1. 水系拖尾（WaterTrail）
            Behaviors.Add(new WaterTrailBehavior
            {
                MaxFragments = 8,
                ParticleLife = 10,
                SizeMultiplier = 0.4f,
                SpawnChance = 0.6f,
                SplashSpeed = 0.2f,
                SplashAngle = 0.1f,
                InertiaFactor = 0.02f,
                RandomSpread = 0.5f,
                Gravity = 0.15f,
                AirResistance = 0.97f,
                BubbleChance = 0.3f,
                BubbleSizeMultiplier = 1.5f,
                ColorStart = new Color(30, 100, 200, 200),
                ColorEnd = new Color(30, 100, 200, 0),
                AutoDraw = true,
                SuppressDefaultDraw = true,
                // offsetY = /2f
            });

            // 2. 重力
            Behaviors.Add(new GravityBehavior
            {
                Acceleration = 0.2f,
                MaxFallSpeed = 8f,
                AutoRotate = true,
                RotationOffset = 0f,
            });

            // 3. 碰撞反弹（最多 2 次）
            Behaviors.Add(new BounceBehavior
            {
                MaxBounces = 2,
                BounceFactor = 0.4f,
                KillOnMaxBounces = true,
                TriggerKillOnMaxBounces = true,
                StopOnLowSpeed = true,
                LowSpeedThreshold = 0.5f,
                TimeLeftAfterStop = 15,
            });

            // 4. 死亡时水花飞溅
            Behaviors.Add(new LiquidBurstBehavior
            {
                FragmentCount = 4,
                BurstSpeed = 1.5f,
                SizeMultiplier = 0.3f,
                ColorStart = new Color(30, 100, 200, 200),
                ColorEnd = new Color(30, 100, 200, 0),
                DustType = DustID.MagicMirror,
                NoGravity = false,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.scale = 0.3f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.alpha = 0;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }
    }
}
