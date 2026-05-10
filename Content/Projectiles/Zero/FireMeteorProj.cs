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
    /// 炎道陨石 — 从天而降的巨大火球。
    /// 炎道技术储备库的"陨石/天降"技术：
    /// - 在鼠标上方高空生成，带重力加速下落
    /// - 下落过程中拖尾越来越浓烈
    /// - 落地时产生巨大爆炸
    /// - 爆炸后在地面留下多个火焰墙（FireFlameWallProj）
    /// - 爆炸产生大量星火弹（FireBaseProj）向四面八方飞散
    ///
    /// 行为组合：
    /// - GravityBehavior: 强重力加速下落
    /// - LiquidTrailBehavior: 浓烈火焰拖尾（下落时越来越浓）
    /// - ExplosionKillBehavior: 落地时巨大爆炸
    /// - OnKill: 生成火焰墙 + 星火弹散射
    /// </summary>
    public class FireMeteorProj : BaseBullet
    {
        /// <summary>重力加速度</summary>
        private const float GravityAccel = 0.4f;

        /// <summary>最大下落速度</summary>
        private const float MaxFallSpeed = 24f;

        /// <summary>爆炸时生成的星火弹数量</summary>
        private const int SparkCount = 16;

        /// <summary>星火弹飞散速度</summary>
        private const float SparkSpeed = 8f;

        /// <summary>爆炸后生成的火焰墙数量</summary>
        private const int FlameWallCount = 6;

        /// <summary>拖尾行为引用（用于动态更新大小）</summary>
        private LiquidTrailBehavior _trailBehavior;

        /// <summary>爆炸行为引用</summary>
        private ExplosionKillBehavior _explosionBehavior;

        protected override void RegisterBehaviors()
        {
            // 1. 强重力（陨石加速下落）
            Behaviors.Add(new GravityBehavior(acceleration: GravityAccel, maxFallSpeed: MaxFallSpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 浓烈火焰拖尾（下落时越来越浓）
            _trailBehavior = new LiquidTrailBehavior
            {
                MaxFragments = 60,
                FragmentLife = 30,
                SizeMultiplier = 0.8f,
                SpawnInterval = 1,
                ColorStart = new Color(255, 220, 100, 255),
                ColorEnd = new Color(255, 30, 0, 0),
                Buoyancy = 0.01f,
                AirResistance = 0.98f,
                InertiaFactor = 0.2f,
                SplashFactor = 0.1f,
                SplashAngle = 0.3f,
                RandomSpread = 0.4f,
                AutoDraw = true,
                SuppressDefaultDraw = true
            };
            Behaviors.Add(_trailBehavior);

            // 3. 落地时巨大爆炸
            _explosionBehavior = new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = 35,
                KillSpeed = 10f,
                KillSizeMultiplier = 2.0f,
                KillFragmentLife = 40,
                ExplodeOnTileCollide = true,
                TileCollideCount = 30,
                TileCollideSpeed = 8f,
                TileCollideSizeMultiplier = 1.8f,
                TileCollideFragmentLife = 35,
                DestroyOnTileCollideExplosion = true,
                ColorStart = new Color(255, 240, 120, 255),
                ColorEnd = new Color(255, 30, 0, 0)
            };
            Behaviors.Add(_explosionBehavior);
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.scale = 1.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 99;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            // 下落速度越快，拖尾越浓烈
            float speed = Projectile.velocity.Length();
            if (_trailBehavior != null && _trailBehavior.Trail != null)
            {
                // 速度越快，碎片越大、存活时间越长
                float speedRatio = MathHelper.Clamp(speed / MaxFallSpeed, 0f, 1f);
                _trailBehavior.Trail.SizeMultiplier = MathHelper.Lerp(0.6f, 1.5f, speedRatio);
                _trailBehavior.Trail.FragmentLife = (int)MathHelper.Lerp(20, 40, speedRatio);
            }

            // 下落速度越快，光照越强
            float lightIntensity = MathHelper.Clamp(0.5f + speed / MaxFallSpeed * 1.5f, 0.5f, 2.0f);
            Lighting.AddLight(Projectile.Center, lightIntensity * 1.2f, lightIntensity * 0.4f, lightIntensity * 0.05f);

            // 高速下落时产生尾迹粒子
            if (speed > 8f && Main.rand.NextBool(3))
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(15f, 15f) - Projectile.velocity.SafeNormalize(Vector2.Zero) * 20f;
                Dust d = Dust.NewDustPerfect(
                    sparkPos,
                    DustID.YellowTorch,
                    Main.rand.NextVector2Circular(1f, 1f),
                    0,
                    new Color(255, 255, 200, 255),
                    Main.rand.NextFloat(0.5f, 1.2f)
                );
                d.noGravity = true;
            }
        }

        protected override void OnKilled(int timeLeft)
        {
            // 1. 爆炸产生大量星火弹向四面八方飞散
            int fireBaseType = ModContent.ProjectileType<FireBaseProj>();
            for (int i = 0; i < SparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / SparkCount + Main.rand.NextFloat(-0.15f, 0.15f);
                float speed = SparkSpeed + Main.rand.NextFloat(-2f, 3f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    vel,
                    fireBaseType,
                    (int)(Projectile.damage * 0.4f),
                    Projectile.knockBack * 0.3f,
                    Projectile.owner
                );
            }

            // 2. 爆炸后在地面留下多个火焰墙
            int flameWallType = ModContent.ProjectileType<FireFlameWallProj>();
            for (int i = 0; i < FlameWallCount; i++)
            {
                float angle = MathHelper.TwoPi * i / FlameWallCount;
                float distance = Main.rand.NextFloat(50f, 120f);
                Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * distance;

                // 检测地面位置
                Vector2 groundPos = FindGroundPosition(spawnPos);
                if (groundPos != Vector2.Zero)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        groundPos,
                        Vector2.Zero,
                        flameWallType,
                        (int)(Projectile.damage * 0.25f),
                        0f,
                        Projectile.owner
                    );
                }
            }
        }

        /// <summary>
        /// 从指定位置向下检测地面，返回地面上的位置。
        /// </summary>
        private Vector2 FindGroundPosition(Vector2 startPos)
        {
            int startX = (int)(startPos.X / 16f);
            int startY = (int)(startPos.Y / 16f);

            for (int dy = 0; dy < 30; dy++)
            {
                int y = startY + dy;
                if (y < 0 || y >= Main.maxTilesY) break;

                Tile tile = Main.tile[startX, y];
                if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    return new Vector2(startX * 16 + 8, (y - 1) * 16 + 8);
                }
            }

            return startPos;
        }
    }
}
