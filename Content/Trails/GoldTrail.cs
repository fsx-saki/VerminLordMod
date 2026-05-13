using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class GoldTrail : ITrail
    {
        public class GoldShardParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Stretch;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpinSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = 1f - Progress;
                    return MathF.Max(0f, fadeIn * fadeOut);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.4f);

            public GoldShardParticle(Vector2 pos, Vector2 vel, int life, float scale, float stretch, float rotation, float spinSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Stretch = stretch;
                Rotation = rotation;
                SpinSpeed = spinSpeed;
                Color = color;
            }
        }

        public class GoldSparkParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float TwinklePhase;
            public float TwinkleSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 6f);
                    float fadeOut = 1f - Progress * Progress;
                    float twinkle = 0.5f + 0.5f * MathF.Max(0, MathF.Sin(TwinklePhase));
                    return MathF.Max(0f, fadeIn * fadeOut * twinkle);
                }
            }

            public GoldSparkParticle(Vector2 pos, Vector2 vel, int life, float scale, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                TwinklePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                TwinkleSpeed = Main.rand.NextFloat(0.15f, 0.3f);
                Color = color;
            }
        }

        public class GoldRingParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
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
                    float fadeIn = MathF.Min(1f, Progress * 3f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.6f);
                }
            }

            public float CurrentScale
            {
                get
                {
                    float expand = MathF.Min(1f, Progress * 2f);
                    return Scale + (MaxScale - Scale) * expand;
                }
            }

            public GoldRingParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float rotSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                Color = color;
            }
        }

        public class GoldDustParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float TwinklePhase;
            public float TwinkleSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = 1f - Progress;
                    float twinkle = 0.6f + 0.4f * MathF.Max(0, MathF.Sin(TwinklePhase));
                    return MathF.Max(0f, fadeIn * fadeOut * twinkle);
                }
            }

            public GoldDustParticle(Vector2 pos, Vector2 vel, int life, float scale, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                TwinklePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                TwinkleSpeed = Main.rand.NextFloat(0.1f, 0.2f);
                Color = color;
            }
        }

        public string Name { get; set; } = "GoldTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.22f;
        public float GhostLengthScale { get; set; } = 1.6f;
        public float GhostAlpha { get; set; } = 0.45f;
        public Color GhostColor { get; set; } = new Color(255, 215, 80, 180);

        public int MaxShards { get; set; } = 40;
        public int ShardLife { get; set; } = 22;
        public float ShardSize { get; set; } = 0.5f;
        public float ShardStretch { get; set; } = 2.0f;
        public int ShardSpawnInterval { get; set; } = 1;
        public float ShardSpinSpeed { get; set; } = 0.15f;
        public float ShardDriftSpeed { get; set; } = 0.3f;
        public float ShardSpread { get; set; } = 4f;
        public Color ShardColor { get; set; } = new Color(255, 220, 100, 220);

        public int MaxSparks { get; set; } = 25;
        public int SparkLife { get; set; } = 30;
        public float SparkSize { get; set; } = 0.5f;
        public float SparkSpawnChance { get; set; } = 0.2f;
        public float SparkDriftSpeed { get; set; } = 0.25f;
        public Color SparkColor { get; set; } = new Color(255, 240, 160, 240);

        public int MaxRings { get; set; } = 6;
        public int RingLife { get; set; } = 40;
        public float RingStartSize { get; set; } = 0.3f;
        public float RingEndSize { get; set; } = 1.5f;
        public float RingSpawnChance { get; set; } = 0.03f;
        public float RingRotSpeed { get; set; } = 0.06f;
        public float RingDriftSpeed { get; set; } = 0.1f;
        public Color RingColor { get; set; } = new Color(255, 200, 60, 160);

        public int MaxDust { get; set; } = 35;
        public int DustLife { get; set; } = 28;
        public float DustSize { get; set; } = 0.2f;
        public float DustSpawnChance { get; set; } = 0.3f;
        public float DustDriftSpeed { get; set; } = 0.4f;
        public Color DustColor { get; set; } = new Color(255, 210, 80, 180);

        public float InertiaFactor { get; set; } = 0.18f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<GoldShardParticle> shards = new();
        private List<GoldSparkParticle> sparks = new();
        private List<GoldRingParticle> rings = new();
        private List<GoldDustParticle> dusts = new();
        private int shardCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _shardTex;
        private Texture2D _sparkTex;
        private Texture2D _ringTex;
        private Texture2D _dustTex;
        private Texture2D _ghostTex;

        public bool HasContent => shards.Count > 0 || sparks.Count > 0 || rings.Count > 0 || dusts.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_shardTex != null) return;
            _shardTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GoldTrail/GoldTrailShard").Value;
            _sparkTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GoldTrail/GoldTrailSpark").Value;
            _ringTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GoldTrail/GoldTrailRing").Value;
            _dustTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GoldTrail/GoldTrailDust").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GoldTrail/GoldTrailGhost").Value;
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

            shardCounter++;
            if (shardCounter >= ShardSpawnInterval && shards.Count < MaxShards)
            {
                shardCounter = 0;
                SpawnShard(center, velocity, moveDir);
            }

            if (sparks.Count < MaxSparks && Main.rand.NextFloat() < SparkSpawnChance)
                SpawnSpark(center, velocity, moveDir);

            if (rings.Count < MaxRings && Main.rand.NextFloat() < RingSpawnChance)
                SpawnRing(center, velocity, moveDir);

            if (dusts.Count < MaxDust && Main.rand.NextFloat() < DustSpawnChance)
                SpawnDust(center, velocity, moveDir);

            for (int i = shards.Count - 1; i >= 0; i--)
            {
                var s = shards[i];
                s.Rotation += s.SpinSpeed;
                s.Velocity *= 0.93f;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0) shards.RemoveAt(i);
            }

            for (int i = sparks.Count - 1; i >= 0; i--)
            {
                var s = sparks[i];
                s.TwinklePhase += s.TwinkleSpeed;
                s.Velocity *= 0.96f;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0) sparks.RemoveAt(i);
            }

            for (int i = rings.Count - 1; i >= 0; i--)
            {
                var r = rings[i];
                r.Rotation += r.RotSpeed;
                r.Velocity *= 0.97f;
                r.Position += r.Velocity;
                r.Life--;
                if (r.Life <= 0) rings.RemoveAt(i);
            }

            for (int i = dusts.Count - 1; i >= 0; i--)
            {
                var d = dusts[i];
                d.TwinklePhase += d.TwinkleSpeed;
                d.Velocity *= 0.95f;
                d.Position += d.Velocity;
                d.Life--;
                if (d.Life <= 0) dusts.RemoveAt(i);
            }
        }

        private void SpawnShard(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-ShardSpread, ShardSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor;
            Vector2 drift = Main.rand.NextVector2Circular(ShardDriftSpeed, ShardDriftSpeed);
            Vector2 vel = inertia + drift + perpDir * sideOffset * 0.1f;

            float scale = Main.rand.NextFloat(0.6f, 1.3f) * ShardSize;
            float stretch = Main.rand.NextFloat(0.8f, 1.5f) * ShardStretch;
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.3f, 0.3f);
            float spinSpeed = Main.rand.NextFloat(-ShardSpinSpeed, ShardSpinSpeed);
            Color color = ShardColor * Main.rand.NextFloat(0.6f, 1f);

            shards.Add(new GoldShardParticle(pos, vel, ShardLife, scale, stretch, rotation, spinSpeed, color));
        }

        private void SpawnSpark(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-6f, 6f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 drift = Main.rand.NextVector2Circular(SparkDriftSpeed, SparkDriftSpeed);
            float scale = Main.rand.NextFloat(0.5f, 1.2f) * SparkSize;
            Color color = SparkColor * Main.rand.NextFloat(0.7f, 1f);

            sparks.Add(new GoldSparkParticle(pos, drift, SparkLife, scale, color));
        }

        private void SpawnRing(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-8f, 8f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(6f, 6f);

            Vector2 drift = Main.rand.NextVector2Circular(RingDriftSpeed, RingDriftSpeed);
            float startSize = RingStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = RingEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            float rotSpeed = Main.rand.NextFloat(-RingRotSpeed, RingRotSpeed);
            Color color = RingColor * Main.rand.NextFloat(0.5f, 1f);

            rings.Add(new GoldRingParticle(pos, drift, RingLife, startSize, endSize, rotSpeed, color));
        }

        private void SpawnDust(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-7f, 7f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(4f, 4f);

            Vector2 drift = Main.rand.NextVector2Circular(DustDriftSpeed, DustDriftSpeed);
            float scale = Main.rand.NextFloat(0.5f, 1.2f) * DustSize;
            Color color = DustColor * Main.rand.NextFloat(0.5f, 1f);

            dusts.Add(new GoldDustParticle(pos, drift, DustLife, scale, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (rings.Count > 0 && _ringTex != null)
            {
                Vector2 ringOrigin = _ringTex.Size() * 0.5f;
                var sortedRings = rings.OrderBy(r => r.Life);
                foreach (var r in sortedRings)
                {
                    Color drawColor = r.Color * r.Alpha;
                    Vector2 pos = r.Position - Main.screenPosition;
                    sb.Draw(_ringTex, pos, null, drawColor, r.Rotation,
                        ringOrigin, r.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (dusts.Count > 0 && _dustTex != null)
            {
                Vector2 dustOrigin = _dustTex.Size() * 0.5f;
                var sortedDusts = dusts.OrderBy(d => d.Life);
                foreach (var d in sortedDusts)
                {
                    Color drawColor = d.Color * d.Alpha;
                    Vector2 pos = d.Position - Main.screenPosition;
                    sb.Draw(_dustTex, pos, null, drawColor, 0f,
                        dustOrigin, d.Scale, SpriteEffects.None, 0);
                }
            }

            if (shards.Count > 0 && _shardTex != null)
            {
                Vector2 shardOrigin = _shardTex.Size() * 0.5f;
                var sortedShards = shards.OrderBy(s => s.Life);
                foreach (var s in sortedShards)
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(s.Stretch, s.CurrentScale);
                    sb.Draw(_shardTex, pos, null, drawColor, s.Rotation,
                        shardOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (sparks.Count > 0 && _sparkTex != null)
            {
                Vector2 sparkOrigin = _sparkTex.Size() * 0.5f;
                var sortedSparks = sparks.OrderBy(s => s.Life);
                foreach (var s in sortedSparks)
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
            shards.Clear();
            sparks.Clear();
            rings.Clear();
            dusts.Clear();
            shardCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
