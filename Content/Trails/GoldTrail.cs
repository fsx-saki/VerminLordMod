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
        public class GoldBladeEdgeParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Length;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 6f);
                    float fadeOut = 1f - Progress;
                    return MathF.Max(0f, fadeIn * fadeOut);
                }
            }

            public float CurrentLength => Length * (1f - Progress * 0.3f);

            public GoldBladeEdgeParticle(Vector2 pos, Vector2 vel, int life, float scale, float length, float rotation, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Length = length;
                Rotation = rotation;
                Color = color;
            }
        }

        public class GoldPrismGlowParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float HueOffset;
            public float HueSpeed;
            public Color BaseColor;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = 1f - Progress * Progress;
                    return MathF.Max(0f, fadeIn * fadeOut * 0.7f);
                }
            }

            public Color CurrentColor
            {
                get
                {
                    float hue = (HueOffset + Progress * HueSpeed) % 1f;
                    if (hue < 0) hue += 1f;
                    return ColorFromHSV(hue * 360f, 0.8f, 1f) * Alpha;
                }
            }

            private static Color ColorFromHSV(float h, float s, float v)
            {
                int hi = (int)(h / 60f) % 6;
                float f = h / 60f - (int)(h / 60f);
                float p = v * (1f - s);
                float q = v * (1f - f * s);
                float t = v * (1f - (1f - f) * s);
                return hi switch
                {
                    0 => new Color(v, t, p),
                    1 => new Color(q, v, p),
                    2 => new Color(p, v, t),
                    3 => new Color(p, q, v),
                    4 => new Color(t, p, v),
                    _ => new Color(v, p, q)
                };
            }

            public GoldPrismGlowParticle(Vector2 pos, Vector2 vel, int life, float scale, float hueOffset, float hueSpeed)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                HueOffset = hueOffset;
                HueSpeed = hueSpeed;
            }
        }

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
            public int FacetSides;
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

            public GoldShardParticle(Vector2 pos, Vector2 vel, int life, float scale, float stretch, float rotation, float spinSpeed, int facetSides, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Stretch = stretch;
                Rotation = rotation;
                SpinSpeed = spinSpeed;
                FacetSides = facetSides;
                Color = color;
            }
        }

        public class GoldFlashParticle
        {
            public Vector2 Position;
            public float Scale;
            public int Life;
            public int MaxLife;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float flash = 1f - Progress * Progress * Progress;
                    return MathF.Max(0f, flash);
                }
            }

            public float CurrentScale => Scale * (0.5f + 0.5f * (1f - Progress));

            public GoldFlashParticle(Vector2 pos, int life, float scale, Color color)
            {
                Position = pos;
                MaxLife = life;
                Life = life;
                Scale = scale;
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

        public int MaxBlades { get; set; } = 40;
        public int BladeLife { get; set; } = 12;
        public float BladeScale { get; set; } = 0.6f;
        public float BladeLength { get; set; } = 18f;
        public int BladeSpawnInterval { get; set; } = 1;
        public float BladeDriftSpeed { get; set; } = 0.2f;
        public float BladeSpread { get; set; } = 5f;
        public Color BladeColor { get; set; } = new Color(255, 230, 140, 240);

        public int MaxPrisms { get; set; } = 20;
        public int PrismLife { get; set; } = 28;
        public float PrismSize { get; set; } = 0.45f;
        public float PrismSpawnChance { get; set; } = 0.18f;
        public float PrismDriftSpeed { get; set; } = 0.15f;
        public float PrismHueSpeed { get; set; } = 2.0f;

        public int MaxShards { get; set; } = 15;
        public int ShardLife { get; set; } = 25;
        public float ShardSize { get; set; } = 0.6f;
        public float ShardStretch { get; set; } = 2.2f;
        public float ShardSpawnChance { get; set; } = 0.08f;
        public float ShardSpinSpeed { get; set; } = 0.1f;
        public float ShardDriftSpeed { get; set; } = 0.25f;
        public Color ShardColor { get; set; } = new Color(255, 220, 100, 220);

        public int MaxFlashes { get; set; } = 8;
        public int FlashLife { get; set; } = 6;
        public float FlashSize { get; set; } = 0.8f;
        public float FlashSpawnChance { get; set; } = 0.04f;
        public Color FlashColor { get; set; } = new Color(255, 250, 220, 255);

        public float InertiaFactor { get; set; } = 0.18f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<GoldBladeEdgeParticle> blades = new();
        private List<GoldPrismGlowParticle> prisms = new();
        private List<GoldShardParticle> shards = new();
        private List<GoldFlashParticle> flashes = new();
        private int bladeCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _bladeTex;
        private Texture2D _prismTex;
        private Texture2D _shardTex;
        private Texture2D _flashTex;
        private Texture2D _ghostTex;

        public bool HasContent => blades.Count > 0 || prisms.Count > 0 || shards.Count > 0 || flashes.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_bladeTex != null) return;
            _bladeTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GoldTrail/GoldTrailBlade").Value;
            _prismTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GoldTrail/GoldTrailPrism").Value;
            _shardTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GoldTrail/GoldTrailShard").Value;
            _flashTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GoldTrail/GoldTrailFlash").Value;
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

            bladeCounter++;
            if (bladeCounter >= BladeSpawnInterval && blades.Count < MaxBlades)
            {
                bladeCounter = 0;
                SpawnBlade(center, velocity, moveDir);
            }

            if (prisms.Count < MaxPrisms && Main.rand.NextFloat() < PrismSpawnChance)
                SpawnPrism(center, velocity, moveDir);

            if (shards.Count < MaxShards && Main.rand.NextFloat() < ShardSpawnChance)
                SpawnShard(center, velocity, moveDir);

            if (flashes.Count < MaxFlashes && Main.rand.NextFloat() < FlashSpawnChance)
                SpawnFlash(center);

            for (int i = blades.Count - 1; i >= 0; i--)
            {
                var b = blades[i];
                b.Velocity *= 0.92f;
                b.Position += b.Velocity;
                b.Life--;
                if (b.Life <= 0) blades.RemoveAt(i);
            }

            for (int i = prisms.Count - 1; i >= 0; i--)
            {
                var p = prisms[i];
                p.Velocity *= 0.95f;
                p.Position += p.Velocity;
                p.Life--;
                if (p.Life <= 0) prisms.RemoveAt(i);
            }

            for (int i = shards.Count - 1; i >= 0; i--)
            {
                var s = shards[i];
                s.Rotation += s.SpinSpeed;
                s.Velocity *= 0.93f;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0) shards.RemoveAt(i);
            }

            for (int i = flashes.Count - 1; i >= 0; i--)
            {
                var f = flashes[i];
                f.Life--;
                if (f.Life <= 0) flashes.RemoveAt(i);
            }
        }

        private void SpawnBlade(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-BladeSpread, BladeSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.2f;
            Vector2 drift = Main.rand.NextVector2Circular(BladeDriftSpeed, BladeDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.6f, 1.3f) * BladeScale;
            float length = Main.rand.NextFloat(0.7f, 1.4f) * BladeLength;
            float angleOffset = Main.rand.NextFloat(-0.8f, 0.8f);
            float rotation = velocity.ToRotation() + MathHelper.PiOver2 + angleOffset;
            Color color = BladeColor * Main.rand.NextFloat(0.6f, 1f);

            blades.Add(new GoldBladeEdgeParticle(pos, vel, BladeLife, scale, length, rotation, color));
        }

        private void SpawnPrism(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-8f, 8f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(4f, 4f);

            Vector2 drift = Main.rand.NextVector2Circular(PrismDriftSpeed, PrismDriftSpeed);
            float scale = Main.rand.NextFloat(0.5f, 1.2f) * PrismSize;
            float hueOffset = Main.rand.NextFloat();
            float hueSpeed = PrismHueSpeed * Main.rand.NextFloat(0.6f, 1.4f);

            prisms.Add(new GoldPrismGlowParticle(pos, drift, PrismLife, scale, hueOffset, hueSpeed));
        }

        private void SpawnShard(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-4f, 4f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor;
            Vector2 drift = Main.rand.NextVector2Circular(ShardDriftSpeed, ShardDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.7f, 1.4f) * ShardSize;
            float stretch = Main.rand.NextFloat(0.8f, 1.5f) * ShardStretch;
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.3f, 0.3f);
            float spinSpeed = Main.rand.NextFloat(-ShardSpinSpeed, ShardSpinSpeed);
            int facetSides = Main.rand.Next(3, 6);
            Color color = ShardColor * Main.rand.NextFloat(0.6f, 1f);

            shards.Add(new GoldShardParticle(pos, vel, ShardLife, scale, stretch, rotation, spinSpeed, facetSides, color));
        }

        private void SpawnFlash(Vector2 center)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(6f, 6f);
            float scale = Main.rand.NextFloat(0.6f, 1.3f) * FlashSize;
            Color color = FlashColor * Main.rand.NextFloat(0.7f, 1f);

            flashes.Add(new GoldFlashParticle(pos, FlashLife, scale, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (prisms.Count > 0 && _prismTex != null)
            {
                Vector2 prismOrigin = _prismTex.Size() * 0.5f;
                var sortedPrisms = prisms.OrderBy(p => p.Life);
                foreach (var p in sortedPrisms)
                {
                    Color drawColor = p.CurrentColor;
                    Vector2 pos = p.Position - Main.screenPosition;
                    sb.Draw(_prismTex, pos, null, drawColor, 0f,
                        prismOrigin, p.Scale, SpriteEffects.None, 0);
                }
            }

            if (blades.Count > 0 && _bladeTex != null)
            {
                Vector2 bladeOrigin = new Vector2(_bladeTex.Width * 0.5f, _bladeTex.Height * 0.5f);
                var sortedBlades = blades.OrderBy(b => b.Life);
                foreach (var b in sortedBlades)
                {
                    Color drawColor = b.Color * b.Alpha;
                    Vector2 pos = b.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(b.CurrentLength / _bladeTex.Width, b.Scale);
                    sb.Draw(_bladeTex, pos, null, drawColor, b.Rotation,
                        bladeOrigin, scale, SpriteEffects.None, 0);
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

            if (flashes.Count > 0 && _flashTex != null)
            {
                Vector2 flashOrigin = _flashTex.Size() * 0.5f;
                var sortedFlashes = flashes.OrderBy(f => f.Life);
                foreach (var f in sortedFlashes)
                {
                    Color drawColor = f.Color * f.Alpha;
                    Vector2 pos = f.Position - Main.screenPosition;
                    sb.Draw(_flashTex, pos, null, drawColor, 0f,
                        flashOrigin, f.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            blades.Clear();
            prisms.Clear();
            shards.Clear();
            flashes.Clear();
            bladeCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
