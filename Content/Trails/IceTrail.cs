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
    /// 冰系拖尾 — 由虚影拖尾 + 两种粒子构成：
    ///
    /// 0. GhostTrail（虚影拖尾）— 冰蓝色短虚影，使用 IceTrailGhost 贴图。
    ///
    /// 1. 十字星星（IceStar）— 静止滞留在弹幕经过的路径上，缓慢闪烁消失。
    ///    使用 Additive 混合呈现冰晶发光感。
    ///
    /// 2. 雪片（IceSnowflake）— 向后飞溅并受重力一簌簌大量落下，
    ///    带有水平飘摆（wobble），模拟雪花飘落。
    ///
    /// 效果描述：
    /// - 弹幕本体带冰蓝色虚影拖尾
    /// - 飞过处留下闪烁的冰晶十字星，逐渐淡出
    /// - 尾部不断洒落大量雪片，受重力下落并左右飘摆
    /// - 整体呈现冰蓝色调，Additive 混合增强寒冷发光感
    /// </summary>
    public class IceTrail : ITrail
    {
        public class IceStarParticle
        {
            public Vector2 Position;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float baseAlpha = 1f - Progress * Progress;
                    float twinkle = 0.7f + 0.3f * MathF.Sin(Progress * MathHelper.TwoPi * 3f + Rotation);
                    return MathF.Max(0f, baseAlpha * twinkle);
                }
            }

            public IceStarParticle(Vector2 pos, int life, float scale, float rotSpeed, Color color)
            {
                Position = pos;
                MaxLife = life;
                Life = life;
                Scale = scale;
                RotSpeed = rotSpeed;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public class IceSnowflakeParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float WobblePhase;
            public float WobbleSpeed;
            public float WobbleAmplitude;
            public float BaseX;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, 1f - Progress);

            public IceSnowflakeParticle(Vector2 pos, Vector2 vel, int life, float scale, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                RotSpeed = Main.rand.NextFloat(-0.02f, 0.02f);
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                WobblePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                WobbleSpeed = Main.rand.NextFloat(0.03f, 0.08f);
                WobbleAmplitude = Main.rand.NextFloat(0.3f, 1.0f);
                BaseX = pos.X;
                Color = color;
            }
        }

        public string Name { get; set; } = "IceTrail";

        public BlendState BlendMode => BlendState.Additive;

        // ===== GhostTrail 配置 =====

        public bool EnableGhostTrail { get; set; } = true;

        public int GhostMaxPositions { get; set; } = 8;

        public int GhostRecordInterval { get; set; } = 2;

        public float GhostWidthScale { get; set; } = 0.3f;

        public float GhostLengthScale { get; set; } = 1.5f;

        public float GhostAlpha { get; set; } = 0.6f;

        public Color GhostColor { get; set; } = new Color(120, 200, 255, 180);

        // ===== 十字星星配置 =====

        public int MaxStars { get; set; } = 40;

        public int StarLife { get; set; } = 30;

        public float StarSize { get; set; } = 0.4f;

        public int StarSpawnInterval { get; set; } = 3;

        public Color StarColor { get; set; } = new Color(160, 220, 255, 220);

        public float StarRotSpeed { get; set; } = 0.01f;

        public float StarSpreadWidth { get; set; } = 8f;

        // ===== 雪片配置 =====

        public int MaxSnowflakes { get; set; } = 120;

        public int SnowflakeLife { get; set; } = 25;

        public float SnowflakeSize { get; set; } = 0.2f;

        public float SnowflakeGravity { get; set; } = 0.1f;

        public float SnowflakeAirResistance { get; set; } = 0.98f;

        public Color SnowflakeColor { get; set; } = new Color(200, 240, 255, 180);

        public int SnowflakeClusterSize { get; set; } = 5;

        public float SnowflakeSplashSpeed { get; set; } = 3f;

        public float SnowflakeSplashAngle { get; set; } = 1.2f;

        public float SnowflakeSpawnChance { get; set; } = 0.7f;

        public float InertiaFactor { get; set; } = 0.3f;

        public float RandomSpread { get; set; } = 6f;

        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        // ===== 内部状态 =====

        private List<IceStarParticle> stars = new();
        private List<IceSnowflakeParticle> snowflakes = new();
        private int starCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _starTex;
        private Texture2D _snowflakeTex;
        private Texture2D _ghostTex;

        public bool HasContent => stars.Count > 0 || snowflakes.Count > 0 || (_ghostTrail?.HasContent ?? false);

        public IceTrail() { }

        private void EnsureTextures()
        {
            if (_starTex != null) return;
            _starTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/IceTrail/IceTrailStar").Value;
            _snowflakeTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/IceTrail/IceTrailSnow").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/IceTrail/IceTrailGhost").Value;
        }

        private void EnsureGhostTrail()
        {
            if (!EnableGhostTrail) return;
            if (_ghostTrail != null) return;
            EnsureTextures();

            _ghostTrail = new GhostTrail
            {
                TrailTexture = _ghostTex,
                TrailColor = GhostColor,
                MaxPositions = GhostMaxPositions,
                RecordInterval = GhostRecordInterval,
                WidthScale = GhostWidthScale,
                LengthScale = GhostLengthScale,
                Alpha = GhostAlpha,
                UseAdditiveBlend = true,
                EnableGlow = false,
            };
        }

        public void Update(Vector2 center, Vector2 velocity)
        {
            EnsureTextures();
            EnsureGhostTrail();

            if (_ghostTrail != null)
                _ghostTrail.Update(center, velocity);

            starCounter++;
            if (starCounter >= StarSpawnInterval && stars.Count < MaxStars)
            {
                starCounter = 0;
                SpawnStar(center);
            }

            if (snowflakes.Count < MaxSnowflakes && Main.rand.NextFloat() < SnowflakeSpawnChance)
            {
                SpawnSnowflakeCluster(center, velocity);
            }

            for (int i = stars.Count - 1; i >= 0; i--)
            {
                var s = stars[i];
                s.Rotation += s.RotSpeed;
                s.Life--;
                if (s.Life <= 0)
                    stars.RemoveAt(i);
            }

            for (int i = snowflakes.Count - 1; i >= 0; i--)
            {
                var sf = snowflakes[i];

                sf.WobblePhase += sf.WobbleSpeed;
                float wobbleOffset = MathF.Sin(sf.WobblePhase) * sf.WobbleAmplitude;
                sf.Position.X = sf.BaseX + wobbleOffset;

                sf.Velocity.Y += SnowflakeGravity;
                sf.Velocity *= SnowflakeAirResistance;

                sf.Position += sf.Velocity;
                sf.BaseX += sf.Velocity.X;
                sf.Rotation += sf.RotSpeed;
                sf.Life--;

                if (sf.Life <= 0)
                    snowflakes.RemoveAt(i);
            }
        }

        private void SpawnStar(Vector2 center)
        {
            float scale = Main.rand.NextFloat(0.6f, 1.2f) * StarSize;
            float rotSpeed = Main.rand.NextFloat(-StarRotSpeed, StarRotSpeed);
            Vector2 pos = center + SpawnOffset + new Vector2(
                Main.rand.NextFloat(-StarSpreadWidth, StarSpreadWidth),
                Main.rand.NextFloat(-StarSpreadWidth, StarSpreadWidth));

            stars.Add(new IceStarParticle(pos, StarLife, scale, rotSpeed, StarColor));
        }

        private void SpawnSnowflakeCluster(Vector2 center, Vector2 velocity)
        {
            Vector2 backwardDir = -velocity.SafeNormalize(Vector2.Zero);
            if (backwardDir == Vector2.Zero)
                backwardDir = Vector2.UnitY;

            int count = Main.rand.Next(1, SnowflakeClusterSize + 1);

            for (int i = 0; i < count; i++)
            {
                if (snowflakes.Count >= MaxSnowflakes) break;

                float angleOffset = Main.rand.NextFloat(-SnowflakeSplashAngle, SnowflakeSplashAngle);
                Vector2 splashDir = backwardDir.RotatedBy(angleOffset);

                Vector2 inertia = -velocity * InertiaFactor;
                Vector2 splash = splashDir * Main.rand.NextFloat(SnowflakeSplashSpeed * 0.3f, SnowflakeSplashSpeed);
                Vector2 random = Main.rand.NextVector2Circular(RandomSpread, RandomSpread);
                Vector2 vel = inertia + splash + random;

                float scale = Main.rand.NextFloat(0.6f, 1.4f) * SnowflakeSize;
                Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(8f, 8f);

                var sf = new IceSnowflakeParticle(pos, vel, SnowflakeLife, scale, SnowflakeColor);
                snowflakes.Add(sf);
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (_starTex == null || _snowflakeTex == null) return;

            if (snowflakes.Count > 0)
            {
                Vector2 snowOrigin = _snowflakeTex.Size() * 0.5f;
                var sortedSnowflakes = snowflakes.OrderBy(sf => sf.Life);

                foreach (var sf in sortedSnowflakes)
                {
                    Color drawColor = sf.Color * sf.Alpha;
                    Vector2 pos = sf.Position - Main.screenPosition;
                    sb.Draw(_snowflakeTex, pos, null, drawColor, sf.Rotation,
                        snowOrigin, sf.Scale, SpriteEffects.None, 0);
                }
            }

            if (stars.Count > 0)
            {
                Vector2 starOrigin = _starTex.Size() * 0.5f;
                var sortedStars = stars.OrderBy(s => s.Life);

                foreach (var s in sortedStars)
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    sb.Draw(_starTex, pos, null, drawColor, s.Rotation,
                        starOrigin, s.Scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            stars.Clear();
            snowflakes.Clear();
            starCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}