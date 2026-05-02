using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;

namespace VerminLordMod.Content.Projectiles
{
    /// <summary>
    /// 血颅弹幕 — 使用 IBulletBehavior 架构重写。
    /// 直线飞行 + 暗红色 GhostTrail 拖尾 + 飞行血尘粒子。
    /// </summary>
    public class BloodSkullProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 1. 直线飞行
            Behaviors.Add(new AimBehavior(speed: 0f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = false
            });

            // 2. 暗红色虚影拖尾（使用自身贴图）
            var trailBehavior = new TrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            };
            Behaviors.Add(trailBehavior);

            // 3. 飞行血尘粒子（替代原每帧 10 个 Dust）
            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.Blood,
                SpawnChance = 1, // 每帧必出
                DustScale = 1.0f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                RandomSpeed = 1f
            });

            // 4. 死亡粒子
            Behaviors.Add(new DustKillBehavior
            {
                DustType = DustID.Blood,
                DustCount = 15,
                DustSpeed = 3f,
                DustScale = 1.2f,
                NoGravity = true
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1.1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 10;
            Projectile.timeLeft = 180;
            Projectile.alpha = 40;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            Projectile.position += Projectile.velocity * 4;

            // 初始化暗红色虚影拖尾
            var trailBehavior = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
            if (trailBehavior != null)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>(
                    "VerminLordMod/Content/Projectiles/BloodSkullProj").Value;
                trailBehavior.TrailManager.AddGhostTrail(trailTex,
                    color: new Color(180, 20, 20),
                    maxPositions: 16,
                    widthScale: 0.6f,
                    lengthScale: 1.0f,
                    alpha: 0.7f,
                    recordInterval: 2);
            }
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Dust.NewDustDirect(target.position, target.width, target.height,
                DustID.RedMoss).noGravity = false;
        }
    }
}
