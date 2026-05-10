using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 炎道爆炸冲击波 — 大范围爆炸弹幕。
    /// 炎道技术储备库的"爆炸"技术：
    /// - 飞行到目标位置后产生巨大爆炸
    /// - 爆炸产生冲击波（圆形扩散判定）
    /// - 爆炸后在地面留下滞留火焰区域（FireFlameWallProj）
    /// - 爆炸本身产生大量星火弹向四面八方飞散
    ///
    /// 行为组合：
    /// - AimBehavior: 直线飞行到目标位置
    /// - ExplosionKillBehavior: 销毁时巨大爆炸
    /// - LiquidTrailBehavior: 浓烈火焰拖尾
    /// - OnKill: 生成滞留火焰 + 星火弹散射
    /// </summary>
    public class FireExplosionProj : BaseBullet
    {
        /// <summary>飞行速度</summary>
        private const float FlySpeed = 8f;

        /// <summary>爆炸时生成的星火弹数量</summary>
        private const int SparkCount = 12;

        /// <summary>星火弹飞散速度</summary>
        private const float SparkSpeed = 6f;

        /// <summary>爆炸后生成的火焰墙数量</summary>
        private const int FlameWallCount = 4;

        protected override void RegisterBehaviors()
        {
            // 1. 直线飞行到目标位置
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
                MaxFragments = 40,
                FragmentLife = 20,
                SizeMultiplier = 0.9f,
                SpawnInterval = 1,
                ColorStart = new Color(255, 200, 80, 255),
                ColorEnd = new Color(255, 50, 0, 0),
                Buoyancy = 0.03f,
                AirResistance = 0.97f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.15f,
                SplashAngle = 0.4f,
                RandomSpread = 0.6f,
                AutoDraw = true,
                SuppressDefaultDraw = true
            });

            // 3. 销毁时巨大爆炸
            Behaviors.Add(new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = 30,
                KillSpeed = 8f,
                KillSizeMultiplier = 1.5f,
                KillFragmentLife = 35,
                ExplodeOnTileCollide = true,
                TileCollideCount = 25,
                TileCollideSpeed = 6f,
                TileCollideSizeMultiplier = 1.3f,
                TileCollideFragmentLife = 30,
                DestroyOnTileCollideExplosion = true,
                ColorStart = new Color(255, 220, 100, 255),
                ColorEnd = new Color(255, 30, 0, 0)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 99;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnKilled(int timeLeft)
        {
            // 1. 爆炸产生大量星火弹向四面八方飞散
            int fireBaseType = ModContent.ProjectileType<FireBaseProj>();
            for (int i = 0; i < SparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / SparkCount + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = SparkSpeed + Main.rand.NextFloat(-1f, 2f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    vel,
                    fireBaseType,
                    (int)(Projectile.damage * 0.5f),
                    Projectile.knockBack * 0.4f,
                    Projectile.owner
                );
            }

            // 2. 爆炸后在地面留下滞留火焰区域
            int flameWallType = ModContent.ProjectileType<FireFlameWallProj>();
            for (int i = 0; i < FlameWallCount; i++)
            {
                float angle = MathHelper.TwoPi * i / FlameWallCount;
                float distance = Main.rand.NextFloat(40f, 80f);
                Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * distance;

                // 检测地面位置（向下射线检测）
                Vector2 groundPos = FindGroundPosition(spawnPos);
                if (groundPos != Vector2.Zero)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        groundPos,
                        Vector2.Zero,
                        flameWallType,
                        (int)(Projectile.damage * 0.3f),
                        0f,
                        Projectile.owner
                    );
                }
            }
        }

        /// <summary>
        /// 从指定位置向下检测地面，返回地面上的位置。
        /// 如果找不到地面，返回原始位置。
        /// </summary>
        private Vector2 FindGroundPosition(Vector2 startPos)
        {
            int startX = (int)(startPos.X / 16f);
            int startY = (int)(startPos.Y / 16f);

            // 向下搜索最多 30 格
            for (int dy = 0; dy < 30; dy++)
            {
                int y = startY + dy;
                if (y < 0 || y >= Main.maxTilesY) break;

                Tile tile = Main.tile[startX, y];
                if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    // 找到地面，返回地面上一格的位置
                    return new Vector2(startX * 16 + 8, (y - 1) * 16 + 8);
                }
            }

            // 找不到地面，返回原始位置
            return startPos;
        }
    }
}
