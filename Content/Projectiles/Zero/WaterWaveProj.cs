using System;
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
    /// 水道贴地水波 — 沿地面推进的波浪，横扫路径上的敌人。
    ///
    /// 设计哲学：
    /// 水道的"势"——水往低处流，贴地推进的水波模拟了潮汐/洪水的推进感。
    /// 宽大的碰撞箱 + 贴图拖尾 = 视觉上的"浪潮"效果。
    ///
    /// 运动方式：
    /// - 沿鼠标方向贴地推进
    /// - 使用 GravityBehavior 保持贴地
    /// - 宽大的碰撞箱横扫敌人
    ///
    /// 视觉效果：
    /// - 使用 LiquidTrailBehavior（贴图碎片拖尾），水蓝渐变
    /// - 拖尾碎片有浮力（向上飘），模拟浪花飞沫
    /// - 命中时沿法线泼洒 WaterDropProj
    /// - 发光层增强视觉
    ///
    /// 行为组合：
    /// - AimBehavior: 沿鼠标方向飞行
    /// - GravityBehavior: 贴地（轻重力）
    /// - LiquidTrailBehavior: 贴图碎片拖尾
    /// - NormalBurstBehavior: 命中时沿法线泼洒水滴
    /// - GlowDrawBehavior: 发光绘制
    /// </summary>
    public class WaterWaveProj : BaseBullet
    {
        /// <summary>飞行速度</summary>
        private const float FlySpeed = 8f;

        /// <summary>最大存活时间（帧）</summary>
        private const int MaxLife = 90;

        /// <summary>波浪宽度（碰撞箱宽度）</summary>
        private const int WaveWidth = 40;

        /// <summary>波浪高度（碰撞箱高度）</summary>
        private const int WaveHeight = 20;

        protected override void RegisterBehaviors()
        {
            // 1. 沿鼠标方向飞行
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = 0f,
                EnableLight = true,
                LightColor = new Vector3(0.08f, 0.25f, 0.5f)
            });

            // 2. 轻重力（贴地）
            Behaviors.Add(new GravityBehavior(acceleration: 0.08f, maxFallSpeed: 6f)
            {
                AutoRotate = false
            });

            // 3. 贴图碎片拖尾（水蓝渐变 + 浮力，模拟浪花飞沫）
            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 60,
                FragmentLife = 20,
                SizeMultiplier = 0.5f,
                SpawnInterval = 1,
                AdaptiveDensity = true,
                AdaptiveSpeedThreshold = 3f,
                AdaptiveDensityFactor = 4f,
                AdaptiveLife = true,
                AdaptiveTargetLength = 90f,
                SpeedLifeExponent = 0.3f,
                MinFragmentLife = 4,
                ColorStart = new Color(40, 140, 255, 220),
                ColorEnd = new Color(20, 60, 180, 0),
                Buoyancy = 0.06f,
                AirResistance = 0.95f,
                InertiaFactor = 0.4f,
                SplashFactor = 0.25f,
                SplashAngle = 0.6f,
                RandomSpread = 0.8f,
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            // 4. 发光绘制
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(60, 160, 255, 100),
                GlowBaseScale = 1.4f,
                GlowLayers = 2,
                GlowAlphaMultiplier = 0.2f,
                EnableLight = true,
                LightColor = new Vector3(0.08f, 0.25f, 0.5f)
            });

            // 5. 法线崩解（命中时沿法线泼洒水滴）
            Behaviors.Add(new NormalBurstBehavior
            {
                ChildProjectileType = ModContent.ProjectileType<WaterDropProj>(),
                Count = 12,
                SpeedMin = 2f,
                SpeedMax = 6f,
                SpreadRadius = 8f,
                SpreadAngle = 0.5f,
                SideAngle = 1.0f,
                BackSplashChance = 0.03f,
                SpawnExtraDust = true,
                ExtraDustCount = 10,
                DustType = DustID.MagicMirror,
                DustColorStart = new Color(40, 140, 255, 200),
                DustColorEnd = new Color(20, 60, 180, 0),
                DustScaleMin = 0.3f,
                DustScaleMax = 0.7f,
                DustSpeedMin = 1f,
                DustSpeedMax = 3f,
                DustNoGravity = false,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = WaveWidth;
            Projectile.height = WaveHeight;
            Projectile.scale = 1.3f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 8;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            base.OnSpawned(source);
        }

        protected override void OnAI()
        {
            // 贴地时产生地面水花粒子
            if (Projectile.velocity.Y >= 0 && Main.rand.NextBool(2))
            {
                // 检测下方是否有地面
                int tileX = (int)(Projectile.Center.X / 16f);
                int tileY = (int)(Projectile.Center.Y / 16f) + 1;
                if (tileY < Main.maxTilesY)
                {
                    Tile tile = Main.tile[tileX, tileY];
                    if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        // 在地面产生水花
                        Vector2 groundPos = new Vector2(Projectile.Center.X, tileY * 16f);
                        for (int i = 0; i < 2; i++)
                        {
                            Dust d = Dust.NewDustPerfect(
                                groundPos + new Vector2(Main.rand.NextFloat(-WaveWidth/2f, WaveWidth/2f), 0),
                                DustID.MagicMirror,
                                new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, -1f)),
                                0,
                                new Color(60, 180, 255, 150),
                                Main.rand.NextFloat(0.4f, 0.8f)
                            );
                            d.noGravity = false;
                        }
                    }
                }
            }

            // 飞行时产生浪花飞沫
            if (Main.rand.NextBool(3))
            {
                Vector2 pos = Projectile.Center + new Vector2(
                    Main.rand.NextFloat(-WaveWidth/2f, WaveWidth/2f),
                    Main.rand.NextFloat(-WaveHeight/2f, WaveHeight/2f)
                );
                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.MagicMirror,
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -0.5f)),
                    0,
                    new Color(80, 200, 255, 120),
                    Main.rand.NextFloat(0.3f, 0.6f)
                );
                d.noGravity = true;
            }
        }

        protected override void OnKilled(int timeLeft)
        {
            // 波浪消散时产生水花
            for (int i = 0; i < 15; i++)
            {
                Vector2 pos = Projectile.Center + new Vector2(
                    Main.rand.NextFloat(-WaveWidth/2f, WaveWidth/2f),
                    Main.rand.NextFloat(-WaveHeight/2f, WaveHeight/2f)
                );
                float angle = Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2);
                float speed = Main.rand.NextFloat(1f, 4f);
                Vector2 vel = new Vector2(
                    (float)Math.Cos(angle) * speed * (Projectile.velocity.X > 0 ? 1 : -1),
                    (float)Math.Sin(angle) * speed - Main.rand.NextFloat(1f, 3f)
                );

                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.MagicMirror,
                    vel,
                    0,
                    new Color(60, 180, 255, 180),
                    Main.rand.NextFloat(0.4f, 0.9f)
                );
                d.noGravity = false;
            }
        }

        /// <summary>
        /// 碰到物块时销毁。
        /// </summary>
        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}