using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 波浪形粒子体行为 — 弹幕本体由二维网格排列的粒子组成，模拟水波扩散效果。
    ///
    /// 效果描述：
    /// - 粒子在二维平面上铺开（沿飞行方向 × 垂直方向）
    /// - 沿飞行方向：纵波（疏密波）振动，粒子前后涌动
    /// - 垂直方向：粒子分布形成水波宽度，并带有横向涟漪
    /// - 密部（压缩区）：粒子聚集、变大、更亮
    /// - 疏部（稀疏区）：粒子分散、变小、更暗
    /// - 每个粒子带有随机扰动，打破网格整齐感，模拟真实水流
    /// - 模拟真实水波扩散的视觉效果
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new WaveBodyBehavior
    /// {
    ///     WaveLength = 60f,       // 波长
    ///     Amplitude = 12f,        // 纵向振动幅度
    ///     Width = 80f,            // 水波宽度（垂直方向铺开范围）
    ///     Rows = 8,               // 垂直方向粒子行数
    ///     ParticlesPerRow = 24,   // 每行粒子数（沿飞行方向）
    ///     ParticleSize = 1.0f,    // 粒子大小
    ///     WaveSpeed = 0.08f,      // 波动速度
    /// });
    /// </code>
    /// </summary>
    public class WaveBodyBehavior : IBulletBehavior
    {
        public string Name => "WaveBody";

        // ===== 可配置参数 =====

        /// <summary>波长（一个完整疏密周期的长度，像素）</summary>
        public float WaveLength { get; set; } = 60f;

        /// <summary>纵向振动幅度（粒子沿传播方向前后偏移的最大值，像素）</summary>
        public float Amplitude { get; set; } = 12f;

        /// <summary>水波宽度（垂直方向铺开范围，像素）</summary>
        public float Width { get; set; } = 80f;

        /// <summary>垂直方向粒子行数</summary>
        public int Rows { get; set; } = 8;

        /// <summary>每行粒子数（沿飞行方向）</summary>
        public int ParticlesPerRow { get; set; } = 24;

        /// <summary>粒子基础大小</summary>
        public float ParticleSize { get; set; } = 1.0f;

        /// <summary>波动速度（弧度/帧）</summary>
        public float WaveSpeed { get; set; } = 0.08f;

        /// <summary>粒子从中心向边缘的衰减速度（0=不衰减，1=完全衰减）</summary>
        public float DecayRate { get; set; } = 0.3f;

        /// <summary>密部粒子最大缩放倍数</summary>
        public float CompressionScale { get; set; } = 1.6f;

        /// <summary>疏部粒子最小缩放倍数</summary>
        public float RarefactionScale { get; set; } = 0.4f;

        /// <summary>垂直方向粒子大小衰减（边缘粒子缩小倍数）</summary>
        public float VerticalDecay { get; set; } = 0.6f;

        /// <summary>横向涟漪幅度（垂直方向粒子左右摆动幅度，像素）</summary>
        public float RippleAmplitude { get; set; } = 4f;

        // ===== 随机扰动参数 =====

        /// <summary>位置随机扰动幅度（像素），0=无扰动</summary>
        public float PositionJitter { get; set; } = 6f;

        /// <summary>大小随机扰动幅度（0~1比例），0=无扰动</summary>
        public float SizeJitter { get; set; } = 0.3f;

        /// <summary>透明度随机扰动幅度（0~1），0=无扰动</summary>
        public float AlphaJitter { get; set; } = 0.2f;

        /// <summary>颜色随机扰动幅度（0~1），0=无扰动</summary>
        public float ColorJitter { get; set; } = 0.15f;

        /// <summary>相位随机偏移幅度（弧度），0=无扰动</summary>
        public float PhaseJitter { get; set; } = 0.5f;

        /// <summary>垂直位置随机扰动幅度（像素），0=无扰动</summary>
        public float VerticalJitter { get; set; } = 4f;

        /// <summary>起始颜色（波浪中心）</summary>
        public Color ColorStart { get; set; } = new Color(30, 100, 200, 200);

        /// <summary>结束颜色（波浪边缘）</summary>
        public Color ColorEnd { get; set; } = new Color(30, 100, 200, 0);

        /// <summary>整体透明度（0~1）</summary>
        public float Alpha { get; set; } = 1.0f;

        /// <summary>是否启用光照</summary>
        public bool EnableLight { get; set; } = false;

        /// <summary>光照颜色</summary>
        public Vector3 LightColor { get; set; } = new Vector3(0.1f, 0.3f, 0.6f);

        /// <summary>是否自动生成运行时纹理（否则使用默认圆形纹理）</summary>
        public bool AutoGenerateTexture { get; set; } = true;

        /// <summary>纹理大小</summary>
        public int TextureSize { get; set; } = 12;

        // ===== 内部状态 =====

        private class WaveParticle
        {
            /// <summary>在波浪中的纵向位置偏移（-1 ~ 1，0=中心）</summary>
            public float PositionOffset;
            /// <summary>在垂直方向的位置偏移（-1 ~ 1，0=中心）</summary>
            public float VerticalOffset;
            /// <summary>当前沿传播方向的额外偏移（纵波振动）</summary>
            public float LongitudinalOffset;
            /// <summary>当前垂直方向的额外偏移（横向涟漪）</summary>
            public float LateralOffset;
            /// <summary>当前大小缩放</summary>
            public float Scale;
            /// <summary>当前透明度</summary>
            public float Alpha;
            /// <summary>旋转角度</summary>
            public float Rotation;
            // ===== 随机扰动种子（OnSpawn 时固定） =====
            /// <summary>位置扰动偏移（沿传播方向，像素）</summary>
            public float PosJitterX;
            /// <summary>位置扰动偏移（垂直方向，像素）</summary>
            public float PosJitterY;
            /// <summary>大小扰动乘数</summary>
            public float SizeJitterMul;
            /// <summary>透明度扰动偏移</summary>
            public float AlphaJitterOff;
            /// <summary>颜色扰动偏移（R/G/B 共用）</summary>
            public float ColorJitterOff;
            /// <summary>相位扰动偏移（弧度）</summary>
            public float PhaseJitterOff;
        }

        private List<WaveParticle> _particles;
        private Texture2D _particleTex;
        private bool _textureGenerated = false;
        private float _wavePhase = 0f;

        public WaveBodyBehavior() { }

        public WaveBodyBehavior(float waveLength, float amplitude, int particlesPerRow, int rows)
        {
            WaveLength = waveLength;
            Amplitude = amplitude;
            ParticlesPerRow = particlesPerRow;
            Rows = rows;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _wavePhase = 0f;
            _particles = new List<WaveParticle>(Rows * ParticlesPerRow);

            Random rand = new Random();

            // 初始化粒子 — 二维网格 + 随机扰动
            for (int row = 0; row < Rows; row++)
            {
                // 垂直方向位置：-1 ~ 1
                float vOffset = Rows > 1
                    ? (float)row / (Rows - 1) * 2f - 1f
                    : 0f;

                for (int col = 0; col < ParticlesPerRow; col++)
                {
                    float t = (float)col / (ParticlesPerRow - 1); // 0~1
                    float posOffset = (t - 0.5f) * 2f; // -1~1

                    _particles.Add(new WaveParticle
                    {
                        PositionOffset = posOffset,
                        VerticalOffset = vOffset,
                        LongitudinalOffset = 0f,
                        LateralOffset = 0f,
                        Scale = 1f,
                        Alpha = 1f,
                        Rotation = 0f,
                        // 固定随机种子（每个粒子独立，永不改变）
                        PosJitterX = (float)(rand.NextDouble() * 2f - 1f) * PositionJitter,
                        PosJitterY = (float)(rand.NextDouble() * 2f - 1f) * VerticalJitter,
                        SizeJitterMul = 1f + (float)(rand.NextDouble() * 2f - 1f) * SizeJitter,
                        AlphaJitterOff = (float)(rand.NextDouble() * 2f - 1f) * AlphaJitter,
                        ColorJitterOff = (float)(rand.NextDouble() * 2f - 1f) * ColorJitter,
                        PhaseJitterOff = (float)(rand.NextDouble() * 2f - 1f) * PhaseJitter,
                    });
                }
            }

            // 延迟生成纹理
            if (AutoGenerateTexture)
                GenerateTexture();
        }

        public void Update(Projectile projectile)
        {
            if (_particles == null || _particles.Count == 0) return;

            _wavePhase += WaveSpeed;

            // 获取弹幕的朝向（速度方向）
            float facingAngle = projectile.velocity.Length() > 0.1f
                ? projectile.velocity.ToRotation()
                : 0f;

            Vector2 forward = facingAngle.ToRotationVector2();
            Vector2 right = forward.RotatedBy(MathHelper.PiOver2); // 垂直方向

            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];

                // 粒子在波浪中的相位（0~2PI，沿传播方向）
                float t = (p.PositionOffset + 1f) * 0.5f; // 0~1
                // 加入相位扰动，每个粒子的波动节奏略有不同
                float phase = t * MathHelper.TwoPi * 2f + _wavePhase + p.PhaseJitterOff;

                // 纵波：粒子沿传播方向前后振动
                float sinPhase = (float)Math.Sin(phase);
                p.LongitudinalOffset = sinPhase * Amplitude;

                // 密部/疏部效果
                float cosPhase = (float)Math.Cos(phase);
                float compressionT = (cosPhase + 1f) * 0.5f; // 0~1
                // 基础缩放 × 疏密缩放 × 大小扰动
                p.Scale = MathHelper.Lerp(RarefactionScale, CompressionScale, compressionT) * p.SizeJitterMul;

                // 透明度也随疏密变化 + 透明度扰动
                float alphaFromCompression = MathHelper.Lerp(0.5f, 1.0f, compressionT);
                p.Alpha = MathHelper.Clamp(alphaFromCompression + p.AlphaJitterOff, 0.1f, 1.0f);

                // 横向涟漪：垂直方向粒子在水平方向上的小幅度摆动
                float lateralPhase = phase + p.VerticalOffset * MathHelper.Pi * 0.5f;
                p.LateralOffset = (float)Math.Sin(lateralPhase) * RippleAmplitude * (1f - Math.Abs(p.VerticalOffset) * 0.3f);

                // 旋转：粒子朝向传播方向
                p.Rotation = facingAngle;
            }

            // 光照
            if (EnableLight)
            {
                Lighting.AddLight(projectile.Center, LightColor.X, LightColor.Y, LightColor.Z);
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            if (_particles == null || _particles.Count == 0 || _particleTex == null)
                return true;

            Vector2 center = projectile.Center;
            Vector2 origin = _particleTex.Size() * 0.5f;

            // 获取弹幕的朝向
            float facingAngle = projectile.velocity.Length() > 0.1f
                ? projectile.velocity.ToRotation()
                : 0f;

            Vector2 forward = facingAngle.ToRotationVector2();
            Vector2 right = forward.RotatedBy(MathHelper.PiOver2);

            foreach (var p in _particles)
            {
                // 粒子世界位置 = 弹幕中心
                //   + 沿传播方向的偏移（基础位置 + 纵波振动偏移 + 位置扰动X + 横向涟漪在传播方向的分量）
                //   + 垂直方向的偏移（基础宽度 + 位置扰动Y + 横向涟漪在垂直方向的分量）
                float baseOffset = p.PositionOffset * WaveLength * 0.5f;
                float verticalBase = p.VerticalOffset * Width * 0.5f;

                // 横向涟漪：在传播方向和垂直方向都有分量
                float lateralForward = p.LateralOffset * 0.3f;
                float lateralRight = p.LateralOffset * 0.7f;

                Vector2 pos = center
                    + forward * (baseOffset + p.LongitudinalOffset + p.PosJitterX + lateralForward)
                    + right * (verticalBase + p.PosJitterY + lateralRight);

                // 边缘衰减
                float distFromCenter = Math.Abs(p.PositionOffset);
                float decay = 1f - distFromCenter * DecayRate;
                decay = Math.Max(0f, decay);

                // 垂直方向衰减（边缘粒子更小更淡）
                float vDist = Math.Abs(p.VerticalOffset);
                float vDecay = 1f - vDist * (1f - VerticalDecay);
                vDecay = Math.Max(0.2f, vDecay);

                float alpha = p.Alpha * decay * vDecay * Alpha;
                if (alpha <= 0.01f) continue;

                // 颜色：基础渐变 + 颜色扰动
                Color baseColor = Color.Lerp(ColorStart, ColorEnd, distFromCenter);
                int r = (int)(baseColor.R * (1f + p.ColorJitterOff * 0.5f));
                int g = (int)(baseColor.G * (1f + p.ColorJitterOff * 0.3f));
                int b = (int)(baseColor.B * (1f - p.ColorJitterOff * 0.4f));
                Color drawColor = new Color(
                    MathHelper.Clamp(r, 0, 255),
                    MathHelper.Clamp(g, 0, 255),
                    MathHelper.Clamp(b, 0, 255),
                    baseColor.A
                );
                drawColor *= alpha;

                float scale = p.Scale * decay * vDecay * ParticleSize;

                spriteBatch.Draw(
                    _particleTex,
                    pos - Main.screenPosition,
                    null,
                    drawColor,
                    p.Rotation,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }

            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;

        private void GenerateTexture()
        {
            if (_textureGenerated) return;
            int size = TextureSize;
            _particleTex = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            float half = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - half + 0.5f;
                    float dy = y - half + 0.5f;
                    float dist = MathF.Sqrt(dx * dx + dy * dy) / half;
                    if (dist > 1f)
                    {
                        data[y * size + x] = Color.Transparent;
                        continue;
                    }
                    // 柔和圆形
                    float a = Math.Max(0f, 1f - dist * dist);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }
            _particleTex.SetData(data);
            _textureGenerated = true;
        }
    }
}
