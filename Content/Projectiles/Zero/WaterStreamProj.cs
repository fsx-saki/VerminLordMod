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
    /// 水道水柱 — 持续喷射的水流弹幕。
    /// 水道技术储备库的"持续喷射/水柱"技术：
    /// - 弹幕在玩家前方持续存在，每帧向前喷射水流
    /// - 水流对路径上的敌人造成多次伤害并击退
    /// - 水流碰到物块时产生水花飞溅
    /// - 持续一段时间后自动消失
    ///
    /// 行为组合：
    /// - AimBehavior: 沿鼠标方向飞行
    /// - WaterTrailBehavior: 水系拖尾（大量水滴飞溅）
    /// - LiquidBurstBehavior: 销毁时水花爆裂
    /// - 自定义 OnAI: 每帧检测碰撞 + 产生水花粒子
    /// </summary>
    public class WaterStreamProj : BaseBullet
    {
        /// <summary>飞行速度</summary>
        private const float FlySpeed = 14f;

        /// <summary>最大存活时间（帧）</summary>
        private const int MaxLife = 50;

        /// <summary>伤害检测间隔（帧）</summary>
        private const int HitInterval = 4;

        /// <summary>伤害半径（像素）</summary>
        private const float HitRadius = 28f;

        /// <summary>计时器</summary>
        private int _timer = 0;

        protected override void RegisterBehaviors()
        {
            // 1. 沿鼠标方向高速飞行
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 水系拖尾 — 水雾喷射（模拟水柱喷射）
            Behaviors.Add(new WaterTrailBehavior
            {
                MaxFragments = 60,
                ParticleLife = 20,
                SizeMultiplier = 0.4f,
                SplashSpeed = 5f,
                SplashAngle = 1.8f,
                InertiaFactor = 0.2f,
                Gravity = 0.1f,
                AirResistance = 0.93f,
                // StretchFactor = 0.7f,
                ColorStart = new Color(120, 220, 255, 220),
                ColorEnd = new Color(40, 120, 200, 0),
                AutoDraw = true,
                SuppressDefaultDraw = true
            });

            // 3. 销毁时水花爆裂
            Behaviors.Add(new LiquidBurstBehavior(12, 5f)
            {
                ColorStart = new Color(80, 200, 255, 200),
                ColorEnd = new Color(30, 100, 200, 0),
                SizeMultiplier = 0.7f,
                NoGravity = false
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.scale = 0.9f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = HitInterval * 2;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            base.OnSpawned(source);
            _timer = 0;
        }

        protected override void OnAI()
        {
            _timer++;

            // 1. 每帧产生水花粒子
            SpawnWaterParticles();

            // 2. 间隔检测范围内的敌人
            if (_timer % HitInterval == 0)
            {
                DamageEnemiesInRange();
            }

            // 3. 光照（微弱水光）
            Lighting.AddLight(Projectile.Center, 0.1f, 0.3f, 0.6f);
        }

        /// <summary>
        /// 检测范围内的敌人并造成伤害
        /// </summary>
        private void DamageEnemiesInRange()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist <= HitRadius)
                    {
                        int damage = Projectile.damage;
                        Player owner = Main.player[Projectile.owner];
                        if (owner != null && owner.active)
                        {
                            bool crit = Main.rand.Next(100) < Projectile.CritChance;
                            npc.StrikeNPC(new NPC.HitInfo
                            {
                                Damage = damage,
                                Knockback = 3f, // 水柱击退效果
                                HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1,
                                Crit = crit
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 生成水花粒子效果
        /// </summary>
        private void SpawnWaterParticles()
        {
            // 主水花粒子
            for (int i = 0; i < 2; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.Water,
                    Projectile.velocity * Main.rand.NextFloat(0.2f, 0.5f) + Main.rand.NextVector2Circular(1f, 1f),
                    0,
                    new Color(80, 200, 255, 200),
                    Main.rand.NextFloat(0.8f, 1.5f)
                );
                d.noGravity = true;
            }

            // 水滴粒子（有重力，下落）
            if (_timer % 3 == 0)
            {
                Vector2 dropPos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                Dust d = Dust.NewDustPerfect(
                    dropPos,
                    DustID.Water,
                    new Vector2(
                        Main.rand.NextFloat(-1f, 1f),
                        Main.rand.NextFloat(1f, 3f)
                    ),
                    50,
                    new Color(60, 180, 255, 180),
                    Main.rand.NextFloat(0.4f, 0.8f)
                );
                d.noGravity = false;
            }
        }

        protected override void OnKilled(int timeLeft)
        {
            // 消失时产生水花
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Water,
                    vel,
                    0,
                    new Color(80, 200, 255, 200),
                    Main.rand.NextFloat(0.6f, 1.2f)
                );
                d.noGravity = true;
            }
        }
    }
}
