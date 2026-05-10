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
    /// 炎道火焰喷射 — 持续喷射的火焰弹幕。
    /// 炎道技术储备库的"持续喷射/火焰吐息"技术：
    /// - 弹幕在玩家前方持续存在，每帧向前喷射火焰
    /// - 火焰对路径上的敌人造成多次伤害
    /// - 喷射到地面/墙壁时产生火焰墙（FireFlameWallProj）
    /// - 持续一段时间后自动消失
    ///
    /// 行为组合：
    /// - AimBehavior: 沿鼠标方向飞行
    /// - LiquidTrailBehavior: 浓烈火焰拖尾
    /// - 自定义 OnAI: 每帧检测碰撞 + 生成火焰墙
    /// </summary>
    public class FireStreamProj : BaseBullet
    {
        /// <summary>飞行速度</summary>
        private const float FlySpeed = 16f;

        /// <summary>最大存活时间（帧）</summary>
        private const int MaxLife = 40;

        /// <summary>火焰墙生成间隔（帧）</summary>
        private const int FlameWallInterval = 8;

        /// <summary>伤害检测间隔（帧）</summary>
        private const int HitInterval = 5;

        /// <summary>伤害半径（像素）</summary>
        private const float HitRadius = 30f;

        /// <summary>计时器</summary>
        private int _timer = 0;

        /// <summary>已命中敌人的集合</summary>
        private System.Collections.Generic.HashSet<int> _hitNPCs = new System.Collections.Generic.HashSet<int>();

        /// <summary>火焰墙生成计时器</summary>
        private int _flameWallTimer = 0;

        protected override void RegisterBehaviors()
        {
            // 1. 沿鼠标方向高速飞行
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.5f, 0.5f, 0.05f)
            });

            // 2. 浓烈火焰拖尾
            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 30,
                FragmentLife = 12,
                SizeMultiplier = 0.7f,
                SpawnInterval = 1,
                ColorStart = new Color(255, 220, 100, 255),
                ColorEnd = new Color(255, 50, 0, 0),
                Buoyancy = 0.02f,
                AirResistance = 0.97f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.15f,
                SplashAngle = 0.4f,
                RandomSpread = 0.6f,
                AutoDraw = true,
                SuppressDefaultDraw = true
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 0;
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
            _flameWallTimer = 0;
            _hitNPCs.Clear();
        }

        protected override void OnAI()
        {
            _timer++;
            _flameWallTimer++;

            // 1. 每帧产生火焰粒子
            SpawnFlameParticles();

            // 2. 间隔检测范围内的敌人
            if (_timer % HitInterval == 0)
            {
                DamageEnemiesInRange();
            }

            // 3. 间隔在地面生成火焰墙
            if (_flameWallTimer >= FlameWallInterval)
            {
                _flameWallTimer = 0;
                TrySpawnFlameWall();
            }

            // 4. 光照
            Lighting.AddLight(Projectile.Center, 1.5f, 0.5f, 0.05f);
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
                                Knockback = 2f,
                                HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1,
                                Crit = crit
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 尝试在地面生成火焰墙
        /// </summary>
        private void TrySpawnFlameWall()
        {
            // 检测弹幕下方是否有地面
            int tileX = (int)(Projectile.Center.X / 16f);
            int tileY = (int)(Projectile.Center.Y / 16f);

            for (int dy = 0; dy < 5; dy++)
            {
                int y = tileY + dy;
                if (y < 0 || y >= Main.maxTilesY) break;

                Tile tile = Main.tile[tileX, y];
                if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    // 在地面位置生成火焰墙
                    Vector2 groundPos = new Vector2(tileX * 16 + 8, (y - 1) * 16 + 8);
                    int flameWallType = ModContent.ProjectileType<FireFlameWallProj>();
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        groundPos,
                        Vector2.Zero,
                        flameWallType,
                        (int)(Projectile.damage * 0.5f),
                        0f,
                        Projectile.owner
                    );
                    break;
                }
            }
        }

        /// <summary>
        /// 生成火焰粒子效果
        /// </summary>
        private void SpawnFlameParticles()
        {
            // 主火焰粒子
            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.Torch,
                    Projectile.velocity * Main.rand.NextFloat(0.2f, 0.5f) + Main.rand.NextVector2Circular(1f, 1f),
                    0,
                    new Color(255, 200, 80, 200),
                    Main.rand.NextFloat(0.6f, 1.2f)
                );
                d.noGravity = true;
            }

            // 火星粒子
            if (_timer % 3 == 0)
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Dust d = Dust.NewDustPerfect(
                    sparkPos,
                    DustID.YellowTorch,
                    Projectile.velocity * Main.rand.NextFloat(0.1f, 0.3f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0,
                    new Color(255, 255, 200, 255),
                    Main.rand.NextFloat(0.3f, 0.6f)
                );
                d.noGravity = true;
            }
        }

        protected override void OnKilled(int timeLeft)
        {
            // 消失时产生小爆炸
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Torch,
                    vel,
                    0,
                    new Color(255, 150, 50, 200),
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                d.noGravity = true;
            }
        }
    }
}
