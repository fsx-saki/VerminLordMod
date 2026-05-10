using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    /// <summary>
    /// 水系拖尾 — 模拟水弹尾端脱落的水滴和泡泡。
    /// 
    /// 使用运行时生成的圆形渐变贴图（中心亮边缘半透明），
    /// 通过 AlphaBlend 混合呈现水滴/泡泡的实在质感。
    /// 粒子较大、颜色深蓝、不拉伸，模拟水珠而非水雾。
    /// 
    /// 效果描述：
    /// - 弹幕经过处产生水滴和泡泡，向后脱落
    /// - 水滴受重力下落，呈弧形轨迹
    /// - 粒子保持圆形，不拉伸，呈现水珠的饱满感
    /// - 颜色从深蓝渐变到浅蓝透明
    /// - 使用 AlphaBlend 混合，呈现水的实在质感
    /// </summary>
    public class WaterTrail : ITrail
    {
        // ===== 粒子类 =====
        public class WaterParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public Color Color;

            public WaterParticle(Vector2 position, Vector2 velocity, int life, float scale, float rotSpeed, Color color)
            {
                Position = position;
                Velocity = velocity;
                MaxLife = life;
                Life = life;
                Scale = scale;
                RotSpeed = rotSpeed;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }

            /// <summary>存活进度 (0~1)，0=刚生成，1=即将消失</summary>
            public float Progress => 1f - (float)Life / MaxLife;

            /// <summary>当前透明度 — 随存活时间线性衰减</summary>
            public float Alpha
            {
                get
                {
                    return MathF.Max(0f, 1f - Progress);
                }
            }

            /// <summary>当前缩放 — 保持圆形，不拉伸，随生命衰减略微缩小</summary>
            public float CurrentScale => Scale * (1f - Progress * 0.3f);

            /// <summary>更新物理</summary>
            public virtual void UpdatePhysics()
            {
                Position += Velocity;
                Rotation += RotSpeed;
                Life--;
            }
        }

        // ===== WaterTrail 可配置参数 =====

        public string Name { get; set; } = "WaterTrail";

        /// <summary>
        /// 使用 AlphaBlend 混合模式，呈现水的实在质感而非发光感。
        /// </summary>
        public BlendState BlendMode => BlendState.AlphaBlend;

        /// <summary>最大粒子总数</summary>
        public int MaxFragments { get; set; } = 80;

        /// <summary>粒子存活时间（帧）</summary>
        public int ParticleLife { get; set; } = 25;

        /// <summary>基础生成间隔（帧），速度慢时的间隔</summary>
        public int SpawnInterval { get; set; } = 1;

        /// <summary>粒子大小倍率</summary>
        public float SizeMultiplier { get; set; } = 1.0f;

        /// <summary>粒子生成概率（0~1），每帧生成时决定是否生成</summary>
        public float SpawnChance { get; set; } = 0.8f;

        /// <summary>尾部飞溅速度</summary>
        public float SplashSpeed { get; set; } = 4f;

        /// <summary>尾部飞溅角度范围（弧度）</summary>
        public float SplashAngle { get; set; } = 1.5f;

        /// <summary>反向惯性系数（向后飞溅的强度）</summary>
        public float InertiaFactor { get; set; } = 0.4f;

        /// <summary>随机扩散范围</summary>
        public float RandomSpread { get; set; } = 2f;

        /// <summary>起始颜色（深蓝，水滴感）</summary>
        public Color ColorStart { get; set; } = new Color(30, 100, 200, 200);

        /// <summary>结束颜色（浅蓝透明）</summary>
        public Color ColorEnd { get; set; } = new Color(30, 100, 200, 200);

        /// <summary>是否启用速度自适应密度</summary>
        public bool AdaptiveDensity { get; set; } = true;

        /// <summary>速度自适应密度阈值</summary>
        public float AdaptiveSpeedThreshold { get; set; } = 3f;

        /// <summary>速度自适应密度系数</summary>
        public float AdaptiveDensityFactor { get; set; } = 4f;

        /// <summary>生成位置偏移</summary>
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        /// <summary>重力系数（正值向下）</summary>
        public float Gravity { get; set; } = 0.12f;

        /// <summary>空气阻力系数</summary>
        public float AirResistance { get; set; } = 0.96f;

        /// <summary>是否启用速度自适应存活时间</summary>
        public bool AdaptiveLife { get; set; } = true;

        /// <summary>拖尾目标长度（像素）</summary>
        public float AdaptiveTargetLength { get; set; } = 60f;

        /// <summary>速度对存活时间的影响指数（0~1）</summary>
        public float SpeedLifeExponent { get; set; } = 0.35f;

        /// <summary>粒子存活时间下限（帧）</summary>
        public int MinParticleLife { get; set; } = 8;

        /// <summary>泡泡生成概率（0~1），偶尔生成一个泡泡而非水滴</summary>
        public float BubbleChance { get; set; } = 0.15f;

        /// <summary>泡泡大小倍率（相对于水滴）</summary>
        public float BubbleSizeMultiplier { get; set; } = 1.8f;

        // ===== 内部状态 =====

        private List<WaterParticle> fragments = new List<WaterParticle>();
        private int spawnCounter = 0;

        // 运行时生成的圆形渐变贴图
        private Texture2D _particleTex;
        private bool _textureGenerated = false;

        public bool HasContent => fragments.Count > 0;

        public WaterTrail()
        {
            // 贴图在首次使用时生成
        }

        /// <summary>
        /// 生成圆形渐变贴图（中心亮、边缘半透明）。
        /// 尺寸 24x24，适合 AlphaBlend 混合的水滴/泡泡效果。
        /// </summary>
        private void GenerateSoftParticleTexture()
        {
            if (_textureGenerated) return;

            int size = 24;
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

                    // 纯色圆形：所有像素统一 Color(30, 100, 200, 200)
                    data[y * size + x] = new Color(30, 100, 200, 200);
                }
            }

            _particleTex.SetData(data);
            _textureGenerated = true;
        }

        public void Update(Vector2 center, Vector2 velocity)
        {
            if (!_textureGenerated)
                GenerateSoftParticleTexture();

            float speed = velocity.Length();

            // 计算当前帧应使用的粒子存活时间
            int currentLife = ParticleLife;
            if (AdaptiveLife && AdaptiveTargetLength > 0f && speed > 0.5f)
            {
                float effectiveSpeed = MathF.Pow(speed, SpeedLifeExponent);
                currentLife = (int)(AdaptiveTargetLength / effectiveSpeed);
                currentLife = Math.Clamp(currentLife, MinParticleLife, ParticleLife);
            }

            // 速度自适应密度
            if (AdaptiveDensity && speed > AdaptiveSpeedThreshold)
            {
                int spawnCount = (int)(speed / AdaptiveDensityFactor);
                spawnCount = Math.Clamp(spawnCount, 1, 4);
                for (int i = 0; i < spawnCount; i++)
                {
                    if (fragments.Count < MaxFragments && Main.rand.NextFloat() < SpawnChance)
                        SpawnParticle(center, velocity, currentLife);
                }
            }
            else
            {
                spawnCounter++;
                if (spawnCounter % SpawnInterval == 0 && fragments.Count < MaxFragments && Main.rand.NextFloat() < SpawnChance)
                {
                    SpawnParticle(center, velocity, currentLife);
                }
            }

            // 更新所有粒子
            for (int i = fragments.Count - 1; i >= 0; i--)
            {
                var p = fragments[i];
                p.Velocity.Y += Gravity; // 重力
                p.Velocity *= AirResistance; // 空气阻力
                p.UpdatePhysics();
                if (p.Life <= 0)
                    fragments.RemoveAt(i);
            }
        }

        private void SpawnParticle(Vector2 center, Vector2 velocity, int life)
        {
            float speed = velocity.Length();

            // 反向方向（弹幕尾部方向）
            Vector2 backwardDir = -velocity.SafeNormalize(Vector2.Zero);
            if (backwardDir == Vector2.Zero)
                backwardDir = Vector2.UnitY;

            // 尾部飞溅：向后 + 随机侧向偏移
            float splashAngleOffset = Main.rand.NextFloat(-SplashAngle, SplashAngle);
            Vector2 splashDir = backwardDir.RotatedBy(splashAngleOffset);

            // 反向惯性（向后飞溅）
            Vector2 inertia = -velocity * InertiaFactor;
            Vector2 splash = splashDir * Main.rand.NextFloat(SplashSpeed * 0.5f, SplashSpeed * 1.5f);
            Vector2 random = Main.rand.NextVector2Circular(RandomSpread, RandomSpread);
            Vector2 vel = inertia + splash + random;

            // 是否生成泡泡（更大、更圆、更透明）
            bool isBubble = Main.rand.NextFloat() < BubbleChance;

            // 粒子大小：水滴适中，泡泡更大
            float scale;
            if (isBubble)
            {
                scale = Main.rand.NextFloat(0.5f, 0.9f) * SizeMultiplier * BubbleSizeMultiplier;
            }
            else
            {
                scale = Main.rand.NextFloat(0.4f, 0.8f) * SizeMultiplier;
            }

            float rotSpeed = Main.rand.NextFloat(-0.03f, 0.03f);

            // 颜色：深蓝到浅蓝，带随机变化
            float colorT = Main.rand.NextFloat(0f, 0.4f);
            Color color = Color.Lerp(ColorStart, ColorEnd, colorT);

            // 泡泡更透明一些
            if (isBubble)
            {
                color *= 0.7f;
            }

            fragments.Add(new WaterParticle(
                center + SpawnOffset + Main.rand.NextVector2Circular(3f, 3f),
                vel, life, scale, rotSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (fragments.Count == 0) return;
            if (!_textureGenerated) return;
            if (_particleTex == null) return;

            // 按存活时间排序，让较新的粒子在上层
            var sorted = fragments.OrderBy(f => f.Life);

            Vector2 origin = _particleTex.Size() * 0.5f;

            foreach (var p in sorted)
            {
                Color drawColor = p.Color * p.Alpha;
                Vector2 pos = p.Position - Main.screenPosition;
                float scale = p.CurrentScale;

                sb.Draw(_particleTex, pos, null, drawColor, p.Rotation,
                    origin, scale, SpriteEffects.None, 0);
            }
        }

        public void Clear()
        {
            fragments.Clear();
            spawnCounter = 0;
        }
    }
}
