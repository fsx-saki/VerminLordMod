using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 多功能法阵绘制器 — 支持双重同心圆法阵绘制、中心光晕、光照、粒子环绕。
    ///
    /// 支持两种模式：
    /// - 蓄力模式（chargeProgress 0~1）：半径和颜色随蓄力进度变化
    /// - 固定模式（chargeProgress = -1）：使用固定半径和颜色
    ///
    /// 使用方式（蓄力模式，如 BloodHandprintsProj）：
    /// <code>
    /// private CircleArrayDrawer _circleDrawer;
    ///
    /// public override void OnSpawn(IEntitySource source) {
    ///     _circleDrawer = new CircleArrayDrawer(
    ///         texPath: "VerminLordMod/Content/Projectiles/BloodC",
    ///         outerRadius: 90f, outerRadiusCharge: 60f,
    ///         innerRadius: 60f, innerRadiusCharge: 45f,
    ///         colorOuter: new Color(100, 0, 0, 80),
    ///         colorInner: new Color(255, 50, 50, 180),
    ///         rotationSpeed1: 0.03f, rotationSpeed2: -0.024f);
    ///     _circleDrawer.Init();
    /// }
    ///
    /// public override void AI() { _circleDrawer.Update(); }
    ///
    /// public override bool PreDraw(ref Color lightColor) {
    ///     _circleDrawer.Draw(sb, center, chargeProgress);
    ///     return false;
    /// }
    /// </code>
    ///
    /// 使用方式（固定模式，如 BlueBirdCircleProj）：
    /// <code>
    /// private CircleArrayDrawer _circleDrawer;
    ///
    /// public override void OnSpawn(IEntitySource source) {
    ///     _circleDrawer = new CircleArrayDrawer(
    ///         texPath: "VerminLordMod/Content/Projectiles/BloodC",
    ///         outerRadius: 100f, innerRadius: 65f,
    ///         colorOuter: new Color(100, 180, 255, 150),
    ///         colorInner: new Color(150, 220, 255, 180),
    ///         rotationSpeed1: 0.12f, rotationSpeed2: -0.09f)
    ///     {
    ///         EnableLighting = true,
    ///         LightColor = new Vector3(0.2f, 0.4f, 0.8f),
    ///         EnableDualGlow = true,
    ///         GlowColor1 = new Color(80, 180, 255, 120),
    ///         GlowColor2 = new Color(50, 120, 255, 40),
    ///         GlowScale1 = 0.35f,
    ///         GlowScale2 = 0.7f,
    ///     };
    ///     _circleDrawer.Init();
    /// }
    /// </code>
    /// </summary>
    public class CircleArrayDrawer
    {
        // ===== 贴图配置 =====

        /// <summary>法阵贴图路径</summary>
        public string TexturePath { get; set; }

        // ===== 半径配置（蓄力模式：base + charge * progress） =====

        /// <summary>外圈基础半径</summary>
        public float OuterRadius { get; set; }

        /// <summary>外圈蓄力增量半径（蓄力模式下有效）</summary>
        public float OuterRadiusCharge { get; set; }

        /// <summary>内圈基础半径</summary>
        public float InnerRadius { get; set; }

        /// <summary>内圈蓄力增量半径（蓄力模式下有效）</summary>
        public float InnerRadiusCharge { get; set; }

        // ===== 颜色配置 =====

        /// <summary>外圈颜色</summary>
        public Color ColorOuter { get; set; }

        /// <summary>内圈颜色</summary>
        public Color ColorInner { get; set; }

        /// <summary>蓄力结束颜色（蓄力模式下与 ColorOuter/ColorInner 插值）</summary>
        public Color? ColorOuterEnd { get; set; }

        /// <summary>蓄力结束内圈颜色</summary>
        public Color? ColorInnerEnd { get; set; }

        // ===== 旋转配置 =====

        /// <summary>外圈旋转速度（弧度/帧）</summary>
        public float RotationSpeed1 { get; set; }

        /// <summary>内圈旋转速度（弧度/帧，负值=反向）</summary>
        public float RotationSpeed2 { get; set; }

        // ===== 透明度配置 =====

        /// <summary>外圈透明度倍率</summary>
        public float OuterAlphaMultiplier { get; set; } = 1.0f;

        /// <summary>内圈透明度倍率</summary>
        public float InnerAlphaMultiplier { get; set; } = 1.0f;

        // ===== 中心光点配置 =====

        /// <summary>是否绘制中心光点</summary>
        public bool DrawCenterGlow { get; set; } = true;

        /// <summary>中心光点大小倍率</summary>
        public float CenterGlowScale { get; set; } = 0.3f;

        /// <summary>中心光点颜色</summary>
        public Color CenterGlowColor { get; set; } = new Color(255, 100, 100, 120);

        // ===== 双层光晕配置（BlueBirdCircleProj 风格） =====

        /// <summary>是否启用双层光晕</summary>
        public bool EnableDualGlow { get; set; } = false;

        /// <summary>内层光晕颜色</summary>
        public Color GlowColor1 { get; set; } = new Color(80, 180, 255, 120);

        /// <summary>外层光晕颜色</summary>
        public Color GlowColor2 { get; set; } = new Color(50, 120, 255, 40);

        /// <summary>内层光晕缩放</summary>
        public float GlowScale1 { get; set; } = 0.35f;

        /// <summary>外层光晕缩放（相对 GlowScale1 的倍数）</summary>
        public float GlowScale2 { get; set; } = 2.0f;

        // ===== 光照配置 =====

        /// <summary>是否启用光照</summary>
        public bool EnableLighting { get; set; } = false;

        /// <summary>光照颜色</summary>
        public Vector3 LightColor { get; set; } = Vector3.Zero;

        // ===== 粒子环绕配置 =====

        /// <summary>是否启用粒子环绕</summary>
        public bool EnableParticleRing { get; set; } = false;

        /// <summary>粒子类型</summary>
        public int ParticleDustType { get; set; } = DustID.Ice;

        /// <summary>粒子生成概率（1/N 每帧）</summary>
        public int ParticleChance { get; set; } = 2;

        /// <summary>粒子大小</summary>
        public float ParticleScale { get; set; } = 1.0f;

        /// <summary>粒子透明度</summary>
        public int ParticleAlpha { get; set; } = 50;

        /// <summary>粒子速度倍率</summary>
        public float ParticleVelocityMultiplier { get; set; } = 0.5f;

        // ===== 运行时状态 =====

        private Texture2D _texture;
        private float _rotation1;
        private float _rotation2;

        /// <summary>当前外圈旋转角度</summary>
        public float CurrentRotation1 => _rotation1;

        /// <summary>当前内圈旋转角度</summary>
        public float CurrentRotation2 => _rotation2;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CircleArrayDrawer(
            string texPath = "VerminLordMod/Content/Projectiles/BloodC",
            float outerRadius = 90f,
            float outerRadiusCharge = 0f,
            float innerRadius = 60f,
            float innerRadiusCharge = 0f,
            Color? colorOuter = null,
            Color? colorInner = null,
            float rotationSpeed1 = 0.03f,
            float rotationSpeed2 = -0.024f)
        {
            TexturePath = texPath;
            OuterRadius = outerRadius;
            OuterRadiusCharge = outerRadiusCharge;
            InnerRadius = innerRadius;
            InnerRadiusCharge = innerRadiusCharge;
            ColorOuter = colorOuter ?? new Color(100, 0, 0, 80);
            ColorInner = colorInner ?? new Color(255, 50, 50, 180);
            RotationSpeed1 = rotationSpeed1;
            RotationSpeed2 = rotationSpeed2;
        }

        /// <summary>初始化旋转角度</summary>
        public void Init(float initialRotation1 = 0f, float initialRotation2 = 1.5708f)
        {
            _rotation1 = initialRotation1;
            _rotation2 = initialRotation2;
        }

        /// <summary>更新旋转角度（在 AI 中调用）</summary>
        public void Update()
        {
            _rotation1 += RotationSpeed1;
            _rotation2 += RotationSpeed2;
        }

        /// <summary>
        /// 更新光照（在 AI 中调用）
        /// </summary>
        public void UpdateLighting(Vector2 center)
        {
            if (EnableLighting && LightColor != Vector3.Zero)
            {
                Lighting.AddLight(center, LightColor.X, LightColor.Y, LightColor.Z);
            }
        }

        /// <summary>
        /// 更新粒子环绕（在 AI 中调用）
        /// </summary>
        public void UpdateParticleRing(Vector2 center)
        {
            if (!EnableParticleRing) return;

            if (Main.rand.NextBool(ParticleChance))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Main.rand.NextFloat(InnerRadius, OuterRadius);
                Vector2 offset = angle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustDirect(
                    center + offset - new Vector2(4f), 8, 8,
                    ParticleDustType, 0f, 0f, ParticleAlpha, default, ParticleScale);
                d.noGravity = true;
                d.velocity = -offset.SafeNormalize(Vector2.Zero) * ParticleVelocityMultiplier;
            }
        }

        /// <summary>
        /// 绘制法阵
        /// </summary>
        /// <param name="sb">SpriteBatch</param>
        /// <param name="center">法阵中心位置（世界坐标）</param>
        /// <param name="chargeProgress">蓄力进度 0~1，固定模式传 -1</param>
        public void Draw(SpriteBatch sb, Vector2 center, float chargeProgress = -1f)
        {
            EnsureTexture();

            bool isChargeMode = chargeProgress >= 0f;
            float progress = isChargeMode ? MathHelper.Clamp(chargeProgress, 0f, 1f) : 1f;

            // 计算半径
            float outerRadius = OuterRadius + (isChargeMode ? progress * OuterRadiusCharge : 0f);
            float innerRadius = InnerRadius + (isChargeMode ? progress * InnerRadiusCharge : 0f);

            // 计算颜色
            Color outerColor = ColorOuter;
            Color innerColor = ColorInner;
            if (isChargeMode && ColorOuterEnd.HasValue)
                outerColor = Color.Lerp(ColorOuter, ColorOuterEnd.Value, progress);
            if (isChargeMode && ColorInnerEnd.HasValue)
                innerColor = Color.Lerp(ColorInner, ColorInnerEnd.Value, progress);

            Vector2 origin = _texture.Size() * 0.5f;
            Vector2 screenPos = center - Main.screenPosition;

            // 外圈
            float scale1 = outerRadius * 2f / _texture.Width;
            sb.Draw(_texture, screenPos, null,
                outerColor * OuterAlphaMultiplier, _rotation1,
                origin, scale1, SpriteEffects.None, 0f);

            // 内圈（反向旋转）
            float scale2 = innerRadius * 2f / _texture.Width;
            sb.Draw(_texture, screenPos, null,
                innerColor * InnerAlphaMultiplier, _rotation2,
                origin, scale2, SpriteEffects.None, 0f);

            // 中心光点（蓄力模式）
            if (DrawCenterGlow && isChargeMode)
            {
                float glowAlpha = 0.3f + progress * 0.7f;
                sb.Draw(_texture, screenPos, null,
                    CenterGlowColor * glowAlpha, 0f,
                    origin, CenterGlowScale, SpriteEffects.None, 0f);
            }

            // 双层光晕（固定模式，BlueBirdCircleProj 风格）
            if (EnableDualGlow)
            {
                // 内层光晕
                sb.Draw(_texture, screenPos, null,
                    GlowColor1, 0f,
                    origin, GlowScale1, SpriteEffects.None, 0f);

                // 外层光晕（更大更淡）
                sb.Draw(_texture, screenPos, null,
                    GlowColor2, 0f,
                    origin, GlowScale1 * GlowScale2, SpriteEffects.None, 0f);
            }
        }

        private void EnsureTexture()
        {
            if (_texture == null || _texture.IsDisposed)
            {
                _texture = ModContent.Request<Texture2D>(TexturePath,
                    AssetRequestMode.ImmediateLoad).Value;
            }
        }
    }
}
