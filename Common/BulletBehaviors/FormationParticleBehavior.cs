using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 法阵粒子行为 — 为弹幕生成法阵风格的旋转粒子效果。
    ///
    /// 适用于法阵、光环、结界等需要多层旋转粒子特效的弹幕。
    /// 支持外圈旋转环、内圈反向旋转环、符文光点、中心光晕、气泡等效果。
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new FormationParticleBehavior
    /// {
    ///     OuterRingCount = 8,           // 外圈粒子数
    ///     OuterRingRadius = 70f,        // 外圈半径
    ///     OuterRingPulseAmplitude = 8f, // 外圈脉动幅度
    ///     InnerRingCount = 5,           // 内圈粒子数
    ///     InnerRingRadius = 32f,        // 内圈半径
    ///     RotationSpeed = 0.04f,        // 旋转速度
    ///     DustType = DustID.Water,
    ///     SpawnBubbles = true,
    ///     SpawnRuneDots = true,
    ///     SpawnCenterGlow = true,
    /// });
    /// </code>
    /// </summary>
    public class FormationParticleBehavior : IBulletBehavior
    {
        public string Name => "FormationParticle";

        // ===== 外圈参数 =====

        /// <summary>外圈粒子数量（0=禁用外圈）</summary>
        public int OuterRingCount { get; set; } = 8;

        /// <summary>外圈半径（像素）</summary>
        public float OuterRingRadius { get; set; } = 70f;

        /// <summary>外圈半径脉动幅度（0=无脉动）</summary>
        public float OuterRingPulseAmplitude { get; set; } = 8f;

        /// <summary>外圈脉动频率</summary>
        public float OuterRingPulseFrequency { get; set; } = 0.05f;

        // ===== 内圈参数 =====

        /// <summary>内圈粒子数量（0=禁用内圈）</summary>
        public int InnerRingCount { get; set; } = 5;

        /// <summary>内圈半径（像素）</summary>
        public float InnerRingRadius { get; set; } = 32f;

        /// <summary>内圈反向旋转（true=与外圈反向）</summary>
        public bool InnerRingReverse { get; set; } = true;

        /// <summary>内圈旋转速度倍率（相对于外圈）</summary>
        public float InnerRingSpeedMultiplier { get; set; } = 0.7f;

        /// <summary>内圈半径脉动幅度</summary>
        public float InnerRingPulseAmplitude { get; set; } = 5f;

        /// <summary>内圈脉动频率</summary>
        public float InnerRingPulseFrequency { get; set; } = 0.07f;

        // ===== 粒子参数 =====

        /// <summary>旋转速度（弧度/帧）</summary>
        public float RotationSpeed { get; set; } = 0.04f;

        /// <summary>外圈 Dust 类型</summary>
        public int OuterDustType { get; set; } = DustID.Water;

        /// <summary>内圈 Dust 类型（-1=使用 OuterDustType）</summary>
        public int InnerDustType { get; set; } = -1;

        /// <summary>符文光点 Dust 类型（-1=使用 OuterDustType）</summary>
        public int RuneDustType { get; set; } = -1;

        /// <summary>中心光晕 Dust 类型（-1=使用 OuterDustType）</summary>
        public int CenterGlowDustType { get; set; } = -1;

        /// <summary>气泡 Dust 类型（-1=使用 OuterDustType）</summary>
        public int BubbleDustType { get; set; } = -1;

        /// <summary>外圈粒子颜色</summary>
        public Color OuterColor { get; set; } = new Color(60, 180, 255, 150);

        /// <summary>内圈粒子颜色</summary>
        public Color InnerColor { get; set; } = new Color(100, 220, 255, 120);

        /// <summary>符文光点颜色</summary>
        public Color RuneColor { get; set; } = new Color(150, 230, 255, 200);

        /// <summary>中心光晕颜色</summary>
        public Color CenterGlowColor { get; set; } = new Color(80, 200, 255, 100);

        /// <summary>气泡颜色</summary>
        public Color BubbleColor { get; set; } = new Color(150, 230, 255, 80);

        /// <summary>外圈粒子大小范围</summary>
        public Vector2 OuterScaleRange { get; set; } = new Vector2(0.5f, 0.8f);

        /// <summary>内圈粒子大小范围</summary>
        public Vector2 InnerScaleRange { get; set; } = new Vector2(0.4f, 0.7f);

        /// <summary>符文光点大小范围</summary>
        public Vector2 RuneScaleRange { get; set; } = new Vector2(0.6f, 1.2f);

        /// <summary>中心光晕大小范围</summary>
        public Vector2 CenterGlowScaleRange { get; set; } = new Vector2(0.8f, 1.5f);

        /// <summary>气泡大小范围</summary>
        public Vector2 BubbleScaleRange { get; set; } = new Vector2(0.3f, 0.5f);

        // ===== 额外效果开关 =====

        /// <summary>是否生成符文光点（随机在法阵范围内闪烁）</summary>
        public bool SpawnRuneDots { get; set; } = true;

        /// <summary>符文光点生成间隔（帧）</summary>
        public int RuneDotInterval { get; set; } = 3;

        /// <summary>符文光点范围（相对于法阵半径的倍率）</summary>
        public float RuneDotRadiusMultiplier { get; set; } = 0.7f;

        /// <summary>是否生成中心光晕</summary>
        public bool SpawnCenterGlow { get; set; } = true;

        /// <summary>中心光晕生成间隔（帧）</summary>
        public int CenterGlowInterval { get; set; } = 2;

        /// <summary>中心光晕范围</summary>
        public float CenterGlowRange { get; set; } = 10f;

        /// <summary>是否生成气泡（向上飘）</summary>
        public bool SpawnBubbles { get; set; } = true;

        /// <summary>气泡生成间隔（帧）</summary>
        public int BubbleInterval { get; set; } = 8;

        /// <summary>气泡范围（相对于法阵半径的倍率）</summary>
        public float BubbleRadiusMultiplier { get; set; } = 0.6f;

        /// <summary>是否启用光照</summary>
        public bool EnableLight { get; set; } = true;

        /// <summary>光照颜色</summary>
        public Vector3 LightColor { get; set; } = new Vector3(0.08f, 0.25f, 0.5f);

        /// <summary>位置偏移</summary>
        public Vector2 PositionOffset { get; set; } = Vector2.Zero;

        // 内部状态
        private float _rotationAngle;
        private int _timer;

        public FormationParticleBehavior() { }

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

            // 1. 外圈旋转粒子
            if (OuterRingCount > 0)
            {
                SpawnOuterRing(center);
            }

            // 2. 内圈旋转粒子
            if (InnerRingCount > 0)
            {
                SpawnInnerRing(center);
            }

            // 3. 符文光点
            if (SpawnRuneDots && _timer % RuneDotInterval == 0)
            {
                SpawnRuneDotEffect(center);
            }

            // 4. 中心光晕
            if (SpawnCenterGlow && _timer % CenterGlowInterval == 0)
            {
                SpawnCenterGlowEffect(center);
            }

            // 5. 气泡
            if (SpawnBubbles && _timer % BubbleInterval == 0)
            {
                SpawnBubbleEffect(center);
            }

            // 6. 光照
            if (EnableLight && LightColor != Vector3.Zero)
            {
                Lighting.AddLight(center, LightColor.X, LightColor.Y, LightColor.Z);
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
                    OuterDustType,
                    angle.ToRotationVector2() * 0.3f,
                    0,
                    OuterColor,
                    Main.rand.NextFloat(OuterScaleRange.X, OuterScaleRange.Y)
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

            int dustType = InnerDustType >= 0 ? InnerDustType : OuterDustType;

            for (int i = 0; i < InnerRingCount; i++)
            {
                float angle = innerAngle + MathHelper.TwoPi * i / InnerRingCount;
                float radius = InnerRingRadius;
                if (InnerRingPulseAmplitude > 0f)
                {
                    radius += (float)System.Math.Sin(_timer * InnerRingPulseFrequency + i * 0.8f) * InnerRingPulseAmplitude;
                }
                Vector2 pos = center + angle.ToRotationVector2() * radius;

                Dust d = Dust.NewDustPerfect(
                    pos,
                    dustType,
                    angle.ToRotationVector2() * 0.2f,
                    0,
                    InnerColor,
                    Main.rand.NextFloat(InnerScaleRange.X, InnerScaleRange.Y)
                );
                d.noGravity = true;
            }
        }

        /// <summary>
        /// 生成符文光点（随机在法阵范围内闪烁）
        /// </summary>
        private void SpawnRuneDotEffect(Vector2 center)
        {
            float runeRadius = OuterRingRadius * RuneDotRadiusMultiplier;
            int dustType = RuneDustType >= 0 ? RuneDustType : OuterDustType;

            Vector2 pos = center + Main.rand.NextVector2Circular(runeRadius, runeRadius);
            Dust d = Dust.NewDustPerfect(
                pos,
                dustType,
                Main.rand.NextVector2Circular(0.3f, 0.3f),
                0,
                RuneColor,
                Main.rand.NextFloat(RuneScaleRange.X, RuneScaleRange.Y)
            );
            d.noGravity = true;
        }

        /// <summary>
        /// 生成中心光晕
        /// </summary>
        private void SpawnCenterGlowEffect(Vector2 center)
        {
            int dustType = CenterGlowDustType >= 0 ? CenterGlowDustType : OuterDustType;

            Vector2 pos = center + Main.rand.NextVector2Circular(CenterGlowRange, CenterGlowRange);
            Dust d = Dust.NewDustPerfect(
                pos,
                dustType,
                Main.rand.NextVector2Circular(0.5f, 0.5f),
                0,
                CenterGlowColor,
                Main.rand.NextFloat(CenterGlowScaleRange.X, CenterGlowScaleRange.Y)
            );
            d.noGravity = true;
        }

        /// <summary>
        /// 生成气泡（向上飘）
        /// </summary>
        private void SpawnBubbleEffect(Vector2 center)
        {
            float bubbleRadius = OuterRingRadius * BubbleRadiusMultiplier;
            int dustType = BubbleDustType >= 0 ? BubbleDustType : OuterDustType;

            Vector2 pos = center + Main.rand.NextVector2Circular(bubbleRadius, bubbleRadius);
            Dust d = Dust.NewDustPerfect(
                pos,
                dustType,
                new Vector2(
                    Main.rand.NextFloat(-0.2f, 0.2f),
                    -Main.rand.NextFloat(0.3f, 0.8f)
                ),
                50,
                BubbleColor,
                Main.rand.NextFloat(BubbleScaleRange.X, BubbleScaleRange.Y)
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
