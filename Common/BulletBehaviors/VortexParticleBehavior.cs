using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 漩涡粒子行为 — 为弹幕生成旋转水花/能量粒子效果。
    ///
    /// 支持两种模式：
    /// - <b>Ring 模式</b>（默认）：粒子排列在固定圈数上旋转，适合简洁的漩涡效果
    /// - <b>Cloud 模式</b>：大量粒子在圆形范围内随机散布，旋转并汇聚向中心，
    ///   适合浓密的漩涡/黑洞/龙卷风效果
    ///
    /// Cloud 模式使用方式：
    /// <code>
    /// Behaviors.Add(new VortexParticleBehavior
    /// {
    ///     UseCloudMode = true,
    ///     CloudParticleCount = 30,       // 每帧生成粒子数（控制疏密）
    ///     CloudRadius = 60f,             // 散布半径
    ///     CloudRotationSpeed = 0.08f,    // 旋转速度
    ///     CloudConvergenceSpeed = 0.02f, // 汇聚速度
    ///     CloudInnerBias = 1.5f,         // 中心偏向（>1=中心更密）
    ///     DustType = DustID.Water,
    ///     ColorStart = new Color(60, 180, 255, 150),
    ///     ColorEnd = new Color(100, 220, 255, 200),
    /// });
    /// </code>
    /// </summary>
    public class VortexParticleBehavior : IBulletBehavior
    {
        public string Name => "VortexParticle";

        // ===== Cloud 模式参数 =====

        /// <summary>启用 Cloud 模式（随机散布+旋转+汇聚），false=使用 Ring 模式</summary>
        public bool UseCloudMode { get; set; } = false;

        /// <summary>Cloud 模式每帧生成的粒子数（控制疏密）</summary>
        public int CloudParticleCount { get; set; } = 20;

        /// <summary>Cloud 模式粒子散布半径（像素）</summary>
        public float CloudRadius { get; set; } = 60f;

        /// <summary>Cloud 模式旋转速度（弧度/帧），粒子切向速度 = 距离 × 此值</summary>
        public float CloudRotationSpeed { get; set; } = 0.06f;

        /// <summary>Cloud 模式汇聚速度，粒子径向向内速度 = 距离 × 此值</summary>
        public float CloudConvergenceSpeed { get; set; } = 0.015f;

        /// <summary>Cloud 模式中心偏向（1=均匀分布，>1=中心更密，<1=边缘更密）</summary>
        public float CloudInnerBias { get; set; } = 1.2f;

        /// <summary>Cloud 模式粒子大小范围</summary>
        public Vector2 CloudScaleRange { get; set; } = new Vector2(0.4f, 0.9f);

        // ===== Cloud 汇聚线（Streamer）参数 =====

        /// <summary>Cloud 模式每帧生成的汇聚线粒子数（0=禁用）</summary>
        public int CloudStreamerCount { get; set; } = 0;

        /// <summary>汇聚线粒子颜色（深色不透明，形成可见的"银河臂"）</summary>
        public Color CloudStreamerColor { get; set; } = new Color(20, 60, 180, 255);

        /// <summary>汇聚线粒子大小范围</summary>
        public Vector2 CloudStreamerScale { get; set; } = new Vector2(0.7f, 1.3f);

        /// <summary>汇聚线螺旋臂数量（类似银河旋臂）</summary>
        public int CloudStreamerArms { get; set; } = 3;

        /// <summary>汇聚线螺旋紧密度（越大旋臂越卷）</summary>
        public float CloudStreamerTightness { get; set; } = 0.03f;

        /// <summary>汇聚线臂宽度（粒子偏离螺旋线的随机范围）</summary>
        public float CloudStreamerWidth { get; set; } = 12f;

        // ===== 外圈参数 =====

        /// <summary>外圈粒子数量（0=禁用外圈）</summary>
        public int OuterRingCount { get; set; } = 6;

        /// <summary>外圈半径（像素）</summary>
        public float OuterRingRadius { get; set; } = 40f;

        /// <summary>外圈半径脉动幅度（0=无脉动）</summary>
        public float OuterRingPulseAmplitude { get; set; } = 10f;

        /// <summary>外圈脉动频率</summary>
        public float OuterRingPulseFrequency { get; set; } = 0.03f;

        // ===== 内圈参数 =====

        /// <summary>内圈粒子数量（0=禁用内圈）</summary>
        public int InnerRingCount { get; set; } = 4;

        /// <summary>内圈半径（像素）</summary>
        public float InnerRingRadius { get; set; } = 20f;

        /// <summary>内圈反向旋转（true=与外圈反向）</summary>
        public bool InnerRingReverse { get; set; } = true;

        /// <summary>内圈旋转速度倍率（相对于外圈）</summary>
        public float InnerRingSpeedMultiplier { get; set; } = 0.7f;

        // ===== 粒子参数 =====

        /// <summary>旋转速度（弧度/帧）</summary>
        public float RotationSpeed { get; set; } = 0.05f;

        /// <summary>Dust 类型</summary>
        public int DustType { get; set; } = DustID.Water;

        /// <summary>内圈 Dust 类型（-1=使用 DustType）</summary>
        public int InnerDustType { get; set; } = -1;

        /// <summary>粒子起始颜色</summary>
        public Color ColorStart { get; set; } = new Color(60, 180, 255, 150);

        /// <summary>粒子结束颜色（中心光晕颜色）</summary>
        public Color ColorEnd { get; set; } = new Color(100, 220, 255, 200);

        /// <summary>粒子大小范围（最小~最大）</summary>
        public Vector2 ParticleScaleRange { get; set; } = new Vector2(0.5f, 0.9f);

        /// <summary>内圈粒子大小范围</summary>
        public Vector2 InnerParticleScaleRange { get; set; } = new Vector2(0.4f, 0.7f);

        // ===== 额外效果 =====

        /// <summary>是否生成气泡（向上飘）</summary>
        public bool SpawnBubbles { get; set; } = true;

        /// <summary>气泡生成间隔（帧）</summary>
        public int BubbleInterval { get; set; } = 5;

        /// <summary>气泡范围（相对于弹幕中心）</summary>
        public float BubbleRange { get; set; } = 40f;

        /// <summary>是否生成中心光晕</summary>
        public bool SpawnCenterGlow { get; set; } = true;

        /// <summary>中心光晕生成间隔（帧）</summary>
        public int CenterGlowInterval { get; set; } = 3;

        /// <summary>中心光晕范围</summary>
        public float CenterGlowRange { get; set; } = 15f;

        /// <summary>是否启用光照</summary>
        public bool EnableLight { get; set; } = true;

        /// <summary>光照颜色</summary>
        public Vector3 LightColor { get; set; } = new Vector3(0.1f, 0.3f, 0.6f);

        /// <summary>位置偏移</summary>
        public Vector2 PositionOffset { get; set; } = Vector2.Zero;

        // 内部状态
        private float _rotationAngle;
        private int _timer;

        public VortexParticleBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _rotationAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            _timer = 0;
        }

        public void Update(Projectile projectile)
        {
            _timer++;
            _rotationAngle += RotationSpeed;

            Vector2 center = projectile.Center + PositionOffset;

            if (UseCloudMode)
            {
                SpawnCloudParticles(center);
            }
            else
            {
                if (OuterRingCount > 0)
                    SpawnOuterRing(center);

                if (InnerRingCount > 0)
                    SpawnInnerRing(center);
            }

            if (SpawnCenterGlow && _timer % CenterGlowInterval == 0)
            {
                SpawnCenterGlowEffect(center);
            }

            if (SpawnBubbles && _timer % BubbleInterval == 0)
            {
                SpawnBubbleEffect(center);
            }

            if (EnableLight && LightColor != Vector3.Zero)
            {
                Lighting.AddLight(center, LightColor.X, LightColor.Y, LightColor.Z);
            }
        }

        private void SpawnCloudParticles(Vector2 center)
        {
            for (int i = 0; i < CloudParticleCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);

                float t = Main.rand.NextFloat();
                float radius = (float)System.Math.Pow(t, 1.0 / CloudInnerBias) * CloudRadius;

                Vector2 offset = angle.ToRotationVector2() * radius;
                Vector2 pos = center + offset;

                Vector2 tangentDir = new Vector2(-offset.Y, offset.X);
                if (tangentDir != Vector2.Zero)
                    tangentDir.Normalize();

                Vector2 radialDir = offset.LengthSquared() > 0.1f
                    ? Vector2.Normalize(-offset)
                    : Vector2.Zero;

                Vector2 velocity = tangentDir * (radius * CloudRotationSpeed)
                                 + radialDir * (radius * CloudConvergenceSpeed);

                float lifeProgress = 1f - (radius / CloudRadius);
                Color color = Color.Lerp(ColorStart, ColorEnd, lifeProgress);

                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustType,
                    velocity,
                    0,
                    color,
                    Main.rand.NextFloat(CloudScaleRange.X, CloudScaleRange.Y)
                );
                d.noGravity = true;
            }

            if (CloudStreamerCount > 0)
                SpawnStreamerParticles(center);
        }

        private void SpawnStreamerParticles(Vector2 center)
        {
            float armAngleStep = MathHelper.TwoPi / CloudStreamerArms;

            for (int i = 0; i < CloudStreamerCount; i++)
            {
                int arm = i % CloudStreamerArms;
                float armBaseAngle = _rotationAngle * 0.5f + arm * armAngleStep;

                float t = (float)i / CloudStreamerCount;
                float radius = CloudRadius * (0.15f + t * 0.85f);

                float spiralAngle = armBaseAngle - radius * CloudStreamerTightness;

                float jitterAngle = Main.rand.NextFloat(-0.3f, 0.3f);
                float jitterRadius = Main.rand.NextFloat(-CloudStreamerWidth, CloudStreamerWidth);
                float finalRadius = radius + jitterRadius;
                if (finalRadius < 2f) finalRadius = 2f;

                Vector2 offset = (spiralAngle + jitterAngle).ToRotationVector2() * finalRadius;
                Vector2 pos = center + offset;

                Vector2 tangentDir = new Vector2(-offset.Y, offset.X);
                if (tangentDir != Vector2.Zero)
                    tangentDir.Normalize();

                Vector2 radialDir = offset.LengthSquared() > 0.1f
                    ? Vector2.Normalize(-offset)
                    : Vector2.Zero;

                Vector2 velocity = tangentDir * (finalRadius * CloudRotationSpeed * 1.2f)
                                 + radialDir * (finalRadius * CloudConvergenceSpeed * 1.5f);

                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustType,
                    velocity,
                    0,
                    CloudStreamerColor,
                    Main.rand.NextFloat(CloudStreamerScale.X, CloudStreamerScale.Y)
                );
                d.noGravity = true;
            }
        }

        /// <summary>
        /// 生成外圈旋转粒子
        /// </summary>
        private void SpawnOuterRing(Vector2 center)
        {
            for (int i = 0; i < OuterRingCount; i++)
            {
                float angle = _rotationAngle + MathHelper.TwoPi * i / OuterRingCount;
                float radius = OuterRingRadius;
                if (OuterRingPulseAmplitude > 0f)
                {
                    radius += (float)System.Math.Sin(_timer * OuterRingPulseFrequency + i * 0.5f) * OuterRingPulseAmplitude;
                }
                Vector2 pos = center + angle.ToRotationVector2() * radius;

                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustType,
                    angle.ToRotationVector2() * 0.3f,
                    0,
                    ColorStart,
                    Main.rand.NextFloat(ParticleScaleRange.X, ParticleScaleRange.Y)
                );
                d.noGravity = true;
            }
        }

        /// <summary>
        /// 生成内圈旋转粒子
        /// </summary>
        private void SpawnInnerRing(Vector2 center)
        {
            float innerAngle = _rotationAngle * InnerRingSpeedMultiplier;
            if (InnerRingReverse)
                innerAngle = -innerAngle;

            int dustType = InnerDustType >= 0 ? InnerDustType : DustType;

            for (int i = 0; i < InnerRingCount; i++)
            {
                float angle = innerAngle + MathHelper.TwoPi * i / InnerRingCount;
                float radius = InnerRingRadius;
                if (OuterRingPulseAmplitude > 0f)
                {
                    radius += (float)System.Math.Sin(_timer * OuterRingPulseFrequency * 1.5f + i * 0.8f) * OuterRingPulseAmplitude * 0.5f;
                }
                Vector2 pos = center + angle.ToRotationVector2() * radius;

                Dust d = Dust.NewDustPerfect(
                    pos,
                    dustType,
                    angle.ToRotationVector2() * 0.2f,
                    0,
                    ColorEnd,
                    Main.rand.NextFloat(InnerParticleScaleRange.X, InnerParticleScaleRange.Y)
                );
                d.noGravity = true;
            }
        }

        /// <summary>
        /// 生成中心光晕效果
        /// </summary>
        private void SpawnCenterGlowEffect(Vector2 center)
        {
            Vector2 glowPos = center + Main.rand.NextVector2Circular(CenterGlowRange, CenterGlowRange);
            Dust d = Dust.NewDustPerfect(
                glowPos,
                DustType,
                Main.rand.NextVector2Circular(0.5f, 0.5f),
                0,
                ColorEnd,
                Main.rand.NextFloat(0.8f, 1.5f)
            );
            d.noGravity = true;
        }

        /// <summary>
        /// 生成气泡效果
        /// </summary>
        private void SpawnBubbleEffect(Vector2 center)
        {
            Vector2 bubblePos = center + Main.rand.NextVector2Circular(BubbleRange, BubbleRange);
            Dust d = Dust.NewDustPerfect(
                bubblePos,
                DustType,
                new Vector2(
                    Main.rand.NextFloat(-0.3f, 0.3f),
                    -Main.rand.NextFloat(0.5f, 1.5f)
                ),
                50,
                new Color(ColorEnd.R, ColorEnd.G, ColorEnd.B, 100),
                Main.rand.NextFloat(0.3f, 0.6f)
            );
            d.noGravity = true;
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
