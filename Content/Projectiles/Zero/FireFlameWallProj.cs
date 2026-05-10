using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 炎道火焰墙 — 滞留燃烧区域弹幕。
    /// 炎道技术储备库的"持续燃烧"技术：
    /// - 固定在地面位置，不移动
    /// - 持续存在一段时间，对进入范围的敌人造成伤害
    /// - 火焰柱上下跳动，模拟燃烧效果
    /// - 持续产生火焰粒子
    /// - 消失时产生小爆炸
    ///
    /// 使用场景：
    /// - 爆炸冲击波（FireExplosionProj）爆炸后在地面留下火焰墙
    /// - 陨石（FireMeteorProj）坠落后在地面留下燃烧区域
    /// - 火焰喷射（FireStreamProj）喷射到地面后形成燃烧区域
    /// </summary>
    public class FireFlameWallProj : BaseBullet
    {
        /// <summary>火焰墙持续时间（帧）</summary>
        private const int Duration = 180; // 3秒

        /// <summary>计时器</summary>
        private int _timer = 0;

        protected override void RegisterBehaviors()
        {
            // 1. 固定位置 — 弹幕生成后不移动
            Behaviors.Add(new StationaryBehavior());

            // 2. 上下浮动 — 火焰跳动效果
            Behaviors.Add(new BobBehavior
            {
                Amplitude = 6f,
                Frequency = 0.08f,
                AffectPosition = true,
                RandomizePhase = true
            });

            // 3. 渐入渐出 — 开始和结束半透明，中间最亮
            Behaviors.Add(new FadeInOutBehavior
            {
                FadeInDuration = 0.2f,
                FadeOutStart = 0.7f,
                MaxAlpha = 0,      // 最亮时 alpha=0（完全不透明）
                MinAlpha = 255,    // 完全透明时 alpha=255
                TotalLife = Duration
            });

            // 4. 范围间隔伤害 — 每 15 帧检测一次
            Behaviors.Add(new AreaDamageBehavior
            {
                HitRadius = 60f,
                HitInterval = 15,
                Knockback = 0f,
                UseLocalNPCHitCooldown = true,
                AutoSetCooldown = true,
                DirectionalKnockback = true
            });

            // 5. 死亡粒子 — 消失时产生小爆炸
            Behaviors.Add(new DustKillBehavior
            {
                DustType = DustID.Torch,
                DustCount = 8,
                DustSpeed = 3f,
                DustScale = 1.2f,
                NoGravity = true
            });

            // 6. 抑制默认贴图绘制 — 只需要粒子效果
            Behaviors.Add(new SuppressDrawBehavior());
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 不碰撞，固定位置
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = Duration;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30; // 免疫间隔
        }

        protected override void OnSpawned(IEntitySource source)
        {
            base.OnSpawned(source);
            _timer = 0;
        }

        protected override void OnAI()
        {
            _timer++;

            // 持续产生火焰粒子（每 2 帧）
            if (_timer % 2 == 0)
            {
                SpawnFlameParticles();
            }

            // 光照
            Lighting.AddLight(Projectile.Center, 1.2f, 0.4f, 0.05f);
        }

        /// <summary>
        /// 生成火焰粒子效果
        /// </summary>
        private void SpawnFlameParticles()
        {
            // 主火焰粒子（向上飘）
            for (int i = 0; i < 2; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(20f, 5f);
                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.Torch,
                    new Vector2(
                        Main.rand.NextFloat(-0.5f, 0.5f),
                        -Main.rand.NextFloat(1f, 3f)
                    ),
                    0,
                    new Color(255, 200, 80, 200),
                    Main.rand.NextFloat(0.8f, 1.5f)
                );
                d.noGravity = true;
            }

            // 火星粒子（随机飞溅）
            if (_timer % 5 == 0)
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(25f, 10f);
                Dust d = Dust.NewDustPerfect(
                    sparkPos,
                    DustID.YellowTorch,
                    Main.rand.NextVector2Circular(2f, 2f) - Vector2.UnitY * Main.rand.NextFloat(1f, 2f),
                    0,
                    new Color(255, 255, 200, 255),
                    Main.rand.NextFloat(0.3f, 0.8f)
                );
                d.noGravity = true;
            }

            // 烟雾粒子（缓慢上升）
            if (_timer % 8 == 0)
            {
                Vector2 smokePos = Projectile.Center + Main.rand.NextVector2Circular(15f, 5f);
                Dust d = Dust.NewDustPerfect(
                    smokePos,
                    DustID.Smoke,
                    new Vector2(
                        Main.rand.NextFloat(-0.3f, 0.3f),
                        -Main.rand.NextFloat(0.5f, 1.5f)
                    ),
                    100,
                    new Color(80, 80, 80, 100),
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                d.noGravity = true;
            }
        }
    }
}
