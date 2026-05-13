using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class MetalTrail : ITrail
    {
        public class GrindSparkParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Brightness;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float flash = MathF.Min(1f, Progress * 8f);
                    float fade = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, flash * fade * Brightness);
                }
            }

            public GrindSparkParticle(Vector2 pos, Vector2 vel, int life, float scale, float brightness, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Brightness = brightness;
                Color = color;
            }
        }

        public class MetalShardParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Stretch;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpinSpeed;
            public float ReflectPhase;
            public float ReflectSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut);
                }
            }

            public float CurrentReflect => 0.5f + 0.5f * MathF.Sin(ReflectPhase);

            public MetalShardParticle(Vector2 pos, Vector2 vel, int life, float scale, float stretch, float rotation, float spinSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Stretch = stretch;
                Rotation = rotation;
                SpinSpeed = spinSpeed;
                ReflectPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                ReflectSpeed = Main.rand.NextFloat(0.1f, 0.25f);
                Color = color;
            }
        }

        public class WhetStreakParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Length;
            public float Width;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float GrowSpeed;
            public float ShrinkSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float growIn = MathF.Min(1f, Progress * GrowSpeed);
                    float shrinkOut = 1f - MathF.Max(0f, (Progress - 0.5f) * ShrinkSpeed);
                    return MathF.Max(0f, growIn * shrinkOut * 0.7f);
                }
            }

            public float CurrentLength => Length * MathF.Min(1f, Progress * GrowSpeed) * MathF.Max(0f, 1f - MathF.Max(0f, Progress - 0.6f) * 2.5f);

            public float CurrentWidth => Width * (1f - Progress * 0.5f);

            public WhetStreakParticle(Vector2 pos, Vector2 vel, int life, float length, float width, float rotation, float growSpeed, float shrinkSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Length = length;
                Width = width;
                Rotation = rotation;
                GrowSpeed = growSpeed;
                ShrinkSpeed = shrinkSpeed;
                Color = color;
            }
        }

        public string Name { get; set; } = "MetalTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.18f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(200, 180, 120, 160);

        public int MaxGrindSparks { get; set; } = 50;
        public int GrindSparkLife { get; set; } = 12;
        public float GrindSparkSize { get; set; } = 0.4f;
        public int GrindSparkSpawnInterval { get; set; } = 1;
        public float GrindSparkSpeed { get; set; } = 3.5f;
        public float GrindSparkSpread { get; set; } = 6f;
        public Color GrindSparkColor { get; set; } = new Color(255, 230, 150, 255);

        public int MaxShards { get; set; } = 18;
        public int ShardLife { get; set; } = 30;
        public float ShardSize { get; set; } = 0.5f;
        public float ShardStretch { get; set; } = 2.5f;
        public float ShardSpawnChance { get; set; } = 0.12f;
        public float ShardSpinSpeed { get; set; } = 0.2f;
        public float ShardDriftSpeed { get; set; } = 0.3f;
        public Color ShardColor { get; set; } = new Color(220, 200, 140, 230);

        public int MaxWhetStreaks { get; set; } = 10;
        public int WhetStreakLife { get; set; } = 18;
        public float WhetStreakLength { get; set; } = 22f;
        public float WhetStreakWidth { get; set; } = 0.3f;
        public float WhetStreakSpawnChance { get; set; } = 0.06f;
        public float WhetStreakGrowSpeed { get; set; } = 4f;
        public float WhetStreakShrinkSpeed { get; set; } = 3f;
        public float WhetStreakDriftSpeed { get; set; } = 0.08f;
        public Color WhetStreakColor { get; set; } = new Color(240, 220, 160, 200);

        public float InertiaFactor { get; set; } = 0.18f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<GrindSparkParticle> grindSparks = new();
        private List<MetalShardParticle> shards = new();
        private List<WhetStreakParticle> whetStreaks = new();
        private int sparkCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _sparkTex;
        private Texture2D _shardTex;
        private Texture2D _streakTex;
        private Texture2D _ghostTex;

        public bool HasContent => grindSparks.Count > 0 || shards.Count > 0 || whetStreaks.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_sparkTex != null) return;
            _sparkTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MetalTrail/MetalTrailSpark").Value;
            _shardTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MetalTrail/MetalTrailShard").Value;
            _streakTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MetalTrail/MetalTrailStreak").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MetalTrail/MetalTrailGhost").Value;
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

            Vector2 moveDir = velocity.SafeNormalize(Vector2.UnitX);

            sparkCounter++;
            if (sparkCounter >= GrindSparkSpawnInterval && grindSparks.Count < MaxGrindSparks)
            {
                sparkCounter = 0;
                SpawnGrindSpark(center, velocity, moveDir);
            }

            if (shards.Count < MaxShards && Main.rand.NextFloat() < ShardSpawnChance)
                SpawnShard(center, velocity, moveDir);

            if (whetStreaks.Count < MaxWhetStreaks && Main.rand.NextFloat() < WhetStreakSpawnChance)
                SpawnWhetStreak(center, velocity, moveDir);

            for (int i = grindSparks.Count - 1; i >= 0; i--)
            {
                var s = grindSparks[i];
                s.Velocity *= 0.92f;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0) grindSparks.RemoveAt(i);
            }

            for (int i = shards.Count - 1; i >= 0; i--)
            {
                var s = shards[i];
                s.Rotation += s.SpinSpeed;
                s.ReflectPhase += s.ReflectSpeed;
                s.Velocity *= 0.96f;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0) shards.RemoveAt(i);
            }

            for (int i = whetStreaks.Count - 1; i >= 0; i--)
            {
                var w = whetStreaks[i];
                w.Velocity *= 0.97f;
                w.Position += w.Velocity;
                w.Life--;
                if (w.Life <= 0) whetStreaks.RemoveAt(i);
            }
        }

        private void SpawnGrindSpark(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-GrindSparkSpread, GrindSparkSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(1f, 1f);

            Vector2 inertia = -velocity * InertiaFactor * 0.5f;
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 burst = angle.ToRotationVector2() * Main.rand.NextFloat(0.5f, GrindSparkSpeed);
            Vector2 vel = inertia + burst;

            float scale = Main.rand.NextFloat(0.3f, 1.2f) * GrindSparkSize;
            float brightness = Main.rand.NextFloat(0.6f, 1f);
            Color color = GrindSparkColor * Main.rand.NextFloat(0.7f, 1f);

            grindSparks.Add(new GrindSparkParticle(pos, vel, GrindSparkLife, scale, brightness, color));
        }

        private void SpawnShard(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-4f, 4f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            Vector2 drift = Main.rand.NextVector2Circular(ShardDriftSpeed, ShardDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.6f, 1.4f) * ShardSize;
            float stretch = Main.rand.NextFloat(0.8f, 1.5f) * ShardStretch;
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            float spinSpeed = Main.rand.NextFloat(-ShardSpinSpeed, ShardSpinSpeed);
            Color color = ShardColor * Main.rand.NextFloat(0.6f, 1f);

            shards.Add(new MetalShardParticle(pos, vel, ShardLife, scale, stretch, rotation, spinSpeed, color));
        }

        private void SpawnWhetStreak(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-3f, 3f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 drift = Main.rand.NextVector2Circular(WhetStreakDriftSpeed, WhetStreakDriftSpeed);
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.3f, 0.3f);
            float length = WhetStreakLength * Main.rand.NextFloat(0.7f, 1.3f);
            float width = WhetStreakWidth * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = WhetStreakColor * Main.rand.NextFloat(0.6f, 1f);

            whetStreaks.Add(new WhetStreakParticle(pos, drift, WhetStreakLife, length, width, rotation, WhetStreakGrowSpeed, WhetStreakShrinkSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (whetStreaks.Count > 0 && _streakTex != null)
            {
                Vector2 streakOrigin = new Vector2(0f, _streakTex.Height * 0.5f);
                var sorted = whetStreaks.OrderBy(w => w.Life);
                foreach (var w in sorted)
                {
                    Color drawColor = w.Color * w.Alpha;
                    Vector2 pos = w.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(w.CurrentLength / _streakTex.Width, w.CurrentWidth);
                    sb.Draw(_streakTex, pos, null, drawColor, w.Rotation,
                        streakOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (shards.Count > 0 && _shardTex != null)
            {
                Vector2 shardOrigin = new Vector2(_shardTex.Width * 0.5f, _shardTex.Height * 0.5f);
                var sorted = shards.OrderBy(s => s.Life);
                foreach (var s in sorted)
                {
                    float reflect = s.CurrentReflect;
                    Color drawColor = s.Color * s.Alpha * (0.6f + 0.4f * reflect);
                    Vector2 pos = s.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(s.Scale * s.Stretch, s.Scale);
                    sb.Draw(_shardTex, pos, null, drawColor, s.Rotation,
                        shardOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (grindSparks.Count > 0 && _sparkTex != null)
            {
                Vector2 sparkOrigin = new Vector2(_sparkTex.Width * 0.5f, _sparkTex.Height * 0.5f);
                var sorted = grindSparks.OrderBy(s => s.Life);
                foreach (var s in sorted)
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    sb.Draw(_sparkTex, pos, null, drawColor, 0f,
                        sparkOrigin, s.Scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            grindSparks.Clear();
            shards.Clear();
            whetStreaks.Clear();
            sparkCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
