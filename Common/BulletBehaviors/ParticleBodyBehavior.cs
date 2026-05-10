using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 粒子体行为 — 用大量小粒子组成弹幕本体，不依赖贴图绘制。
    /// 粒子在弹幕位置周围翻涌，受向心力约束，模拟水球、能量球等效果。
    /// 
    /// 使用方式：
    ///   Behaviors.Add(new ParticleBodyBehavior
    ///   {
    ///       ParticleCount = 30,
    ///       BodyRadius = 20f,
    ///       ParticleSize = 6f,
    ///       ColorStart = new Color(30, 120, 255, 200),
    ///       ColorEnd = new Color(60, 180, 255, 0),
    ///       SwirlSpeed = 0.03f,
    ///       ReturnForce = 0.05f
    ///   });
    ///   Behaviors.Add(new SuppressDrawBehavior()); // 阻止贴图绘制
    /// </summary>
    public class ParticleBodyBehavior : IBulletBehavior
    {
        public string Name => "ParticleBody";

        // ===== 可配置参数 =====

        /// <summary>粒子数量</summary>
        public int ParticleCount { get; set; } = 30;

        /// <summary>水球半径（像素）</summary>
        public float BodyRadius { get; set; } = 20f;

        /// <summary>粒子基础大小</summary>
        public float ParticleSize { get; set; } = 6f;

        /// <summary>起始颜色（中心）</summary>
        public Color ColorStart { get; set; } = new Color(30, 120, 255, 200);

        /// <summary>结束颜色（边缘，透明）</summary>
        public Color ColorEnd { get; set; } = new Color(60, 180, 255, 0);

        /// <summary>旋转速度（弧度/帧），控制粒子整体旋转</summary>
        public float SwirlSpeed { get; set; } = 0.03f;

        /// <summary>向心回复力（越大粒子越紧密）</summary>
        public float ReturnForce { get; set; } = 0.05f;

        /// <summary>随机扰动强度</summary>
        public float JitterStrength { get; set; } = 0.5f;

        /// <summary>粒子大小随生命衰减</summary>
        public bool ShrinkOverLife { get; set; } = true;

        /// <summary>是否在弹幕速度方向拉伸（模拟水球飞行形变）</summary>
        public bool StretchOnMove { get; set; } = true;

        /// <summary>速度拉伸系数</summary>
        public float StretchFactor { get; set; } = 0.3f;

        /// <summary>是否启用光照</summary>
        public bool EnableLight { get; set; } = true;

        /// <summary>光照颜色 (R, G, B)</summary>
        public Vector3 LightColor { get; set; } = new Vector3(0.3f, 0.6f, 1.0f);

        /// <summary>整体透明度（0~1），控制粒子体整体的可见度</summary>
        public float Alpha { get; set; } = 1.0f;

        /// <summary>是否自动生成运行时纹理（否则使用默认圆形纹理）</summary>
        public bool AutoGenerateTexture { get; set; } = true;

        // ===== 内部状态 =====

        private class BodyParticle
        {
            public Vector2 LocalOffset;   // 相对于弹幕中心的偏移
            public Vector2 Velocity;      // 内部速度
            public float Scale;
            public float Rotation;
            public float RotSpeed;
            public float Phase;           // 用于颜色/大小变化
            public int Life;
            public int MaxLife;
        }

        private List<BodyParticle> _particles = new List<BodyParticle>();
        private Texture2D _particleTex;
        private bool _textureGenerated = false;
        private float _globalPhase = 0f;

        public ParticleBodyBehavior() { }

        public ParticleBodyBehavior(int particleCount, float bodyRadius)
        {
            ParticleCount = particleCount;
            BodyRadius = bodyRadius;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _particles.Clear();
            _globalPhase = 0f;

            // 生成初始粒子，在球体内随机分布
            for (int i = 0; i < ParticleCount; i++)
            {
                SpawnParticle(projectile);
            }
        }

        private void SpawnParticle(Projectile projectile)
        {
            // 在球体内随机位置
            float r = Main.rand.NextFloat(0.3f, 1f) * BodyRadius;
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 offset = new Vector2(
                MathF.Cos(angle) * r,
                MathF.Sin(angle) * r
            );

            var p = new BodyParticle
            {
                LocalOffset = offset,
                Velocity = Main.rand.NextVector2Circular(1f, 1f),
                Scale = Main.rand.NextFloat(0.6f, 1.2f) * ParticleSize,
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotSpeed = Main.rand.NextFloat(-0.05f, 0.05f),
                Phase = Main.rand.NextFloat(MathHelper.TwoPi),
                Life = Main.rand.Next(60, 120),
                MaxLife = 120
            };
            _particles.Add(p);
        }

        public void Update(Projectile projectile)
        {
            _globalPhase += SwirlSpeed;

            float speed = projectile.velocity.Length();

            // 更新所有粒子
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];

                // 生命周期
                p.Life--;
                if (p.Life <= 0)
                {
                    // 粒子死亡后重生，保持粒子总数恒定
                    p.Life = Main.rand.Next(60, 120);
                    p.MaxLife = 120;
                    // 重置到球体内随机位置
                    float r = Main.rand.NextFloat(0.3f, 1f) * BodyRadius;
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    p.LocalOffset = new Vector2(MathF.Cos(angle) * r, MathF.Sin(angle) * r);
                    p.Velocity = Main.rand.NextVector2Circular(1f, 1f);
                    p.Scale = Main.rand.NextFloat(0.6f, 1.2f) * ParticleSize;
                    p.Phase = Main.rand.NextFloat(MathHelper.TwoPi);
                    continue;
                }

                // 向心力：把粒子拉回球体范围内
                float dist = p.LocalOffset.Length();
                if (dist > BodyRadius)
                {
                    // 超出范围，强力拉回
                    Vector2 towardCenter = -p.LocalOffset.SafeNormalize(Vector2.Zero);
                    p.Velocity += towardCenter * ReturnForce * 2f;
                }
                else if (dist > BodyRadius * 0.3f)
                {
                    // 在范围内，轻微向心
                    Vector2 towardCenter = -p.LocalOffset.SafeNormalize(Vector2.Zero);
                    p.Velocity += towardCenter * ReturnForce * (dist / BodyRadius);
                }
                else
                {
                    // 靠近中心时，随机推开避免聚集
                    Vector2 outward = p.LocalOffset.SafeNormalize(Vector2.Zero);
                    if (outward == Vector2.Zero)
                        outward = Main.rand.NextVector2Circular(1f, 1f);
                    p.Velocity += outward * ReturnForce * 0.3f;
                }

                // 随机扰动（翻涌感）
                p.Velocity += Main.rand.NextVector2Circular(JitterStrength, JitterStrength);

                // 整体旋转（绕中心公转）
                float swirlAngle = SwirlSpeed * 2f;
                float r2 = p.LocalOffset.Length();
                Vector2 tangent = new Vector2(-p.LocalOffset.Y, p.LocalOffset.X).SafeNormalize(Vector2.Zero);
                p.Velocity += tangent * r2 * 0.005f;

                // 速度阻尼
                p.Velocity *= 0.92f;

                // 更新位置
                p.LocalOffset += p.Velocity;

                // 旋转
                p.Rotation += p.RotSpeed;
                p.Phase += 0.02f;
            }

            // 补充粒子（如果数量不足）
            while (_particles.Count < ParticleCount)
            {
                SpawnParticle(projectile);
            }

            // 光照
            if (EnableLight)
            {
                Lighting.AddLight(projectile.Center, LightColor.X, LightColor.Y, LightColor.Z);
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            _particles.Clear();
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            if (_particles.Count == 0) return true;

            // 延迟生成纹理（确保 Main.graphics 可用）
            if (!_textureGenerated && AutoGenerateTexture)
                GenerateTexture();

            if (_particleTex == null) return true;

            Vector2 center = projectile.Center - Main.screenPosition;
            float speed = projectile.velocity.Length();

            // 速度方向拉伸（水球飞行时略微形变）
            Vector2 stretch = Vector2.One;
            if (StretchOnMove && speed > 1f)
            {
                Vector2 dir = projectile.velocity.SafeNormalize(Vector2.Zero);
                float s = 1f + MathF.Min(speed * StretchFactor * 0.01f, 0.3f);
                // 在速度方向拉伸，垂直方向压缩
                stretch = new Vector2(
                    1f + (s - 1f) * Math.Abs(dir.X),
                    1f + (s - 1f) * Math.Abs(dir.Y)
                );
            }

            Vector2 origin = _particleTex.Size() * 0.5f;

            foreach (var p in _particles)
            {
                // 粒子在弹幕空间中的位置（考虑速度拉伸）
                Vector2 localPos = p.LocalOffset;
                if (StretchOnMove && speed > 1f)
                {
                    Vector2 dir = projectile.velocity.SafeNormalize(Vector2.Zero);
                    localPos = new Vector2(
                        localPos.X * (1f + MathF.Min(speed * StretchFactor * 0.005f, 0.15f) * Math.Abs(dir.X)),
                        localPos.Y * (1f + MathF.Min(speed * StretchFactor * 0.005f, 0.15f) * Math.Abs(dir.Y))
                    );
                }

                Vector2 drawPos = center + localPos;

                // 颜色：从中心到边缘渐变
                float distRatio = p.LocalOffset.Length() / BodyRadius;
                Color color = Color.Lerp(ColorStart, ColorEnd, distRatio);

                // 透明度随生命波动（翻涌闪烁感）
                float lifeAlpha = 0.7f + 0.3f * MathF.Sin(p.Phase);
                // 整体透明度控制
                color *= lifeAlpha * Alpha;

                // 大小：随生命衰减
                float sizeScale = p.Scale;
                if (ShrinkOverLife)
                {
                    float lifeProgress = 1f - (float)p.Life / p.MaxLife;
                    sizeScale *= 1f - lifeProgress * 0.4f;
                }

                spriteBatch.Draw(_particleTex, drawPos, null, color,
                    p.Rotation, origin, sizeScale, SpriteEffects.None, 0f);
            }

            return true; // 让引擎继续绘制（如果有其他行为）
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;

        private void GenerateTexture()
        {
            if (_textureGenerated) return;
            int size = 12;
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
                    float alpha = MathF.Max(0f, 1f - dist * dist);
                    byte b = (byte)(alpha * 255);
                    data[y * size + x] = new Color(b, b, b, b);
                }
            }
            _particleTex.SetData(data);
            _textureGenerated = true;
        }
    }
}
