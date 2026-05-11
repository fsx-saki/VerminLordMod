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
    /// 水道水法阵 — 在鼠标位置生成的水系法阵，源源不断释放双绞线水龙追踪鼠标。
    ///
    /// 设计哲学：
    /// - 固定在地面位置，持续一段时间
    /// - 使用 FormationParticleBehavior 绘制法阵粒子效果（旋转水环 + 符文光点）
    /// - 以双绞线（双螺旋）模式持续释放粒子体水球（WaterFormationDragonProj）
    /// - 水球使用 ConstrainedSteerBehavior 平滑追踪鼠标，形成"水龙"蜿蜒效果
    /// - 两股水流 180° 对置旋转，交织成双绞线
    ///
    /// 行为组合：
    /// - StationaryBehavior: 固定位置
    /// - FadeInOutBehavior: 渐入渐出
    /// - BobBehavior: 轻微上下浮动（法阵呼吸感）
    /// - FormationParticleBehavior: 法阵粒子效果
    /// - SuppressDrawBehavior: 阻止默认贴图
    /// - 自定义 OnAI: 双绞线生成水龙弹
    /// </summary>
    public class WaterFormationProj : BaseBullet
    {
        /// <summary>法阵持续时间（帧）</summary>
        private const int Duration = 180;

        /// <summary>双绞线水弹生成间隔（帧）</summary>
        private const int SpawnInterval = 5;

        /// <summary>双绞线旋转速度（弧度/帧）</summary>
        private const float HelixRotationSpeed = 0.12f;

        /// <summary>法阵半径（像素）</summary>
        private const float FormationRadius = 70f;

        /// <summary>水弹生成半径（法阵边缘）</summary>
        private const float SpawnRadius = 55f;

        /// <summary>水弹初始速度</summary>
        private const float BoltSpeed = 6f;

        /// <summary>双绞线当前旋转角度</summary>
        private float _helixAngle = 0f;

        /// <summary>计时器</summary>
        private int _timer = 0;

        protected override void RegisterBehaviors()
        {
            // 1. 固定位置 — 法阵生成后不移动
            Behaviors.Add(new StationaryBehavior());

            // 2. 渐入渐出 — 开始和结束半透明
            Behaviors.Add(new FadeInOutBehavior
            {
                FadeInDuration = 0.15f,
                FadeOutStart = 0.75f,
                MaxAlpha = 0,
                MinAlpha = 200,
                TotalLife = Duration
            });

            // 3. 轻微上下浮动 — 法阵呼吸感
            Behaviors.Add(new BobBehavior
            {
                Amplitude = 3f,
                Frequency = 0.06f,
                AffectPosition = true,
                RandomizePhase = true
            });

            // 4. 法阵粒子效果（替代原来的硬编码 SpawnFormationParticles）
            Behaviors.Add(new FormationParticleBehavior
            {
                OuterRingCount = 8,
                OuterRingRadius = FormationRadius,
                OuterRingPulseAmplitude = 8f,
                OuterRingPulseFrequency = 0.05f,
                InnerRingCount = 5,
                InnerRingRadius = FormationRadius * 0.45f,
                InnerRingReverse = true,
                InnerRingSpeedMultiplier = 0.7f,
                InnerRingPulseAmplitude = 5f,
                InnerRingPulseFrequency = 0.07f,
                RotationSpeed = 0.04f,
                OuterDustType = DustID.Water,
                InnerDustType = DustID.MagicMirror,
                OuterColor = new Color(60, 180, 255, 150),
                InnerColor = new Color(100, 220, 255, 120),
                RuneColor = new Color(150, 230, 255, 200),
                CenterGlowColor = new Color(80, 200, 255, 100),
                BubbleColor = new Color(150, 230, 255, 80),
                SpawnRuneDots = true,
                RuneDotInterval = 3,
                RuneDotRadiusMultiplier = 0.7f,
                SpawnCenterGlow = true,
                CenterGlowInterval = 2,
                CenterGlowRange = 10f,
                SpawnBubbles = true,
                BubbleInterval = 8,
                BubbleRadiusMultiplier = 0.6f,
                EnableLight = true,
                LightColor = new Vector3(0.08f, 0.25f, 0.5f),
            });

            // 5. 阻止默认贴图绘制 — 法阵本体由 FormationParticleBehavior 粒子组成
            Behaviors.Add(new SuppressDrawBehavior());

            // 6. 销毁粉尘爆发 — 法阵消失时产生多层水花爆裂
            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new System.Collections.Generic.List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new KillDustBurstBehavior.DustBurstLayer
                    {
                        Count = 20,
                        DustType = DustID.Water,
                        Color = new Color(80, 200, 255, 200),
                        ScaleMin = 0.8f,
                        ScaleMax = 1.5f,
                        NoGravity = true,
                        SpreadRadius = 15f,
                        UseCircularVelocity = true,
                        SpeedMin = 2f,
                        SpeedMax = 6f,
                    },
                    new KillDustBurstBehavior.DustBurstLayer
                    {
                        Count = 8,
                        DustType = DustID.Water,
                        Color = new Color(60, 180, 255, 150),
                        Alpha = 50,
                        ScaleMin = 0.4f,
                        ScaleMax = 0.8f,
                        NoGravity = false,
                        SpreadRadius = 10f,
                        UseCircularVelocity = false,
                        VelXMin = -3f,
                        VelXMax = 3f,
                        VelYMin = -4f,
                        VelYMax = -1f,
                    },
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            base.OnSpawned(source);
            _timer = 0;
            _helixAngle = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void OnAI()
        {
            _timer++;
            _helixAngle += HelixRotationSpeed;

            if (_timer % SpawnInterval == 0 && _timer < Duration * 0.9f)
            {
                SpawnDragonBolts();
            }
        }

        private void SpawnDragonBolts()
        {
            int boltType = ModContent.ProjectileType<WaterFormationDragonProj>();
            IEntitySource source = Main.player[Projectile.owner]?.GetSource_FromThis();
            Vector2 mousePos = Main.MouseWorld;

            for (int strand = 0; strand < 2; strand++)
            {
                float angle = _helixAngle + strand * MathHelper.Pi;
                Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * SpawnRadius;

                Vector2 toMouse = mousePos - spawnPos;
                Vector2 vel = toMouse.SafeNormalize(Vector2.Zero) * BoltSpeed;

                Projectile.NewProjectile(
                    source,
                    spawnPos,
                    vel,
                    boltType,
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }
    }
}
