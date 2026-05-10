using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    /// <summary>
    /// 血液滴弹幕 — 用于血手印销毁时的爆炸飞溅效果。
    ///
    /// 使用 BaseBullet + LiquidTrailHelper 实现：
    /// - 受重力影响，碰到物块会反弹（最多 3 次），不能攻击 NPC
    /// - 带明显的液态血液拖尾，消失时产生血液飞溅粒子
    ///
    /// OnTileCollide 由 BaseBullet 自动委托给 BounceBehavior，
    /// 无需手动重写。
    /// </summary>
    public class BloodDropProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 1. 液态拖尾（LiquidTrail）— 明显的血液拖尾
            Behaviors.Add(new TrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            // 2. 重力
            Behaviors.Add(new GravityBehavior
            {
                Acceleration = 0.25f,
                MaxFallSpeed = 10f,
                AutoRotate = true,
                RotationOffset = 0f,
            });

            // 3. 碰撞反弹（最多 3 次）
            // OnTileCollide 由 BaseBullet 自动委托给 BounceBehavior.OnTileCollide
            Behaviors.Add(new BounceBehavior
            {
                MaxBounces = 3,
                BounceFactor = 0.5f,
                KillOnMaxBounces = true,
                TriggerKillOnMaxBounces = true,
                StopOnLowSpeed = true,
                LowSpeedThreshold = 1f,
                TimeLeftAfterStop = 20,
            });

            // 4. 死亡时血液飞溅
            Behaviors.Add(new LiquidBurstBehavior
            {
                FragmentCount = 6,
                BurstSpeed = 2f,
                SizeMultiplier = 0.4f,
                ColorStart = new Color(180, 20, 20, 200),
                ColorEnd = new Color(80, 0, 0, 0),
                DustType = DustID.Blood,
                NoGravity = true,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.scale = 0.4f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;       // 碰撞物块
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;           // 3秒最大寿命
            Projectile.alpha = 0;
            Projectile.friendly = false;         // 不能攻击 NPC
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // 使用 LiquidTrailHelper 初始化血液拖尾
            var trail = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
            if (trail != null)
            {
                Texture2D tex = ModContent.Request<Texture2D>(
                    "VerminLordMod/Content/Projectiles/BloodDropProj").Value;

                LiquidTrailHelper.SetupBloodTrail(trail, tex,
                    maxFragments: 12, fragmentLife: 12,
                    sizeMultiplier: 0.35f);
            }
        }
    }
}
