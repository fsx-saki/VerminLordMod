using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.Buffs.AddToEnemy;
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
    /// 酸液弹幕 — 抛物线飞行 + 液态拖尾 + 水破裂特效 + 腐蚀 debuff
    ///
    /// 使用 LiquidTrailHelper 简化液态拖尾初始化。
    /// </summary>
    public class AcidWaterProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 1. 重力（抛物线轨迹）
            Behaviors.Add(new GravityBehavior(acceleration: 0.15f, maxFallSpeed: 8f));

            // 2. 液态拖尾（LiquidTrail）
            Behaviors.Add(new TrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            // 3. 水破裂泼洒特效（消失时触发，使用 Dust 系统实现）
            Behaviors.Add(new LiquidBurstBehavior
            {
                FragmentCount = 20,
                BurstSpeed = 6f,
                SizeMultiplier = 0.8f,
                ColorStart = new Color(80, 255, 50, 200),
                ColorEnd = new Color(20, 120, 10, 0),
                DustType = DustID.MagicMirror,
                NoGravity = true,
            });

            // 4. 液体反应 — 水/蜂蜜融入、岩浆汽化、微光反弹
            Behaviors.Add(new LiquidReactionBehavior
            {
                EnableMerge = true,
                EnableVaporize = true,
                EnableShimmerBounce = true,
                SteamDustCount = 15,
                ShimmerDustCount = 10,
            });

            // 5. 酸液飞溅 — 物块水渍沾染
            Behaviors.Add(new AcidSplashBehavior
            {
                Radius = 2,
                PaintChance = 0.6f,
                PaintColor = 5, // 绿色
                AddLiquid = true,
                LiquidType = 0, // 水
                LiquidAmount = 32,
                SpawnDust = true,
                DustCount = 8,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;   // 正常碰撞物块
            Projectile.penetrate = 99;
            Projectile.timeLeft = 600;       // 10秒，足够时间长
            Projectile.alpha = 80;           // 降低本体亮度
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            // 使用 LiquidTrailHelper 初始化酸性液态拖尾
            var trail = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
            if (trail != null)
            {
                Texture2D tex = ModContent.Request<Texture2D>(
                    "VerminLordMod/Content/Projectiles/AcidWaterProj").Value;

                LiquidTrailHelper.SetupAcidTrail(trail, tex);
            }
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Acidicbuff>(), 120);
        }
    }
}
