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
    /// 血液弹幕 — 用于血手印蓄力时的血液汇聚效果。
    /// 
    /// 使用 ConvergeProjectile 基类 + LiquidTrailHelper 实现：
    /// - 从玩家身上发射向血手印弹幕，带液态血液拖尾
    /// - 到达目标后消失，产生血液飞溅粒子
    /// </summary>
    public class BloodWaterProj : ConvergeProjectile
    {
        // ConvergeProjectile 参数
        protected override int TargetProjType => ModContent.ProjectileType<BloodHandprintsProj>();
        protected override float ConvergeDistance => 20f;
        protected override float MinSpeed => 4f;
        protected override float MaxSpeed => 12f;
        protected override float LerpFactor => 0.08f;
        protected override float FreeFlightDrag => 0.98f;
        protected override float FreeFlightKillSpeed => 0.5f;

        protected override void RegisterBehaviors()
        {
            // 1. 液态拖尾（LiquidTrail）— 血液拖尾
            Behaviors.Add(new TrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            // 2. 死亡时血液飞溅
            Behaviors.Add(new LiquidBurstBehavior
            {
                FragmentCount = 8,
                BurstSpeed = 3f,
                SizeMultiplier = 0.6f,
                ColorStart = new Color(180, 20, 20, 200),
                ColorEnd = new Color(80, 0, 0, 0),
                DustType = DustID.Blood,
                NoGravity = true,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.scale = 0.5f;     // 缩小到一半
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;  // 穿过物块
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;       // 2秒最大寿命
            Projectile.alpha = 0;
            Projectile.friendly = false;     // 不对敌人造成伤害
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            // 使用 LiquidTrailHelper 初始化血液拖尾
            var trail = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
            if (trail != null)
            {
                Texture2D tex = ModContent.Request<Texture2D>(
                    "VerminLordMod/Content/Projectiles/BloodWaterProj").Value;

                LiquidTrailHelper.SetupBloodTrail(trail, tex,
                    maxFragments: 15, fragmentLife: 15,
                    sizeMultiplier: 0.3f);
            }
        }
    }
}
