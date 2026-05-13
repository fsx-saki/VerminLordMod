using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class YinYangTrail : ITrail
    {
        public class YinYangOrbParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public bool IsYang;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = 1f - Progress * Progress;
                    return MathF.Max(0f, fadeIn * fadeOut);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.3f);

            public YinYangOrbParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotSpeed, bool isYang, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                IsYang = isYang;
                Color = color;
            }
        }

        public class YinYangFishParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public bool IsYang;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.7f);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.2f);

            public YinYangFishParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotSpeed, bool isYang, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                IsYang = isYang;
                Color = color;
            }
        }

        public class YinYangRippleParticle
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
                    return MathF.Max(0f, fadeIn * fadeOut * 0.5f);
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

            public YinYangRippleParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float rotSpeed, Color color)
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

        public class YinYangSparkParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float TwinklePhase;
            public float TwinkleSpeed;
            public bool IsYang;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 6f);
                    float fadeOut = 1f - Progress;
                    float twinkle = 0.5f + 0.5f * MathF.Max(0, MathF.Sin(TwinklePhase));
                    return MathF.Max(0f, fadeIn * fadeOut * twinkle);
                }
            }

            public YinYangSparkParticle(Vector2 pos, Vector2 vel, int life, float scale, bool isYang, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                IsYang = isYang;
                TwinklePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                TwinkleSpeed = Main.rand.NextFloat(0.1f, 0.25f);
                Color = color;
            }
        }

        public string Name { get; set; } = "YinYangTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.4f;
        public Color GhostColor { get; set; } = new Color(200, 195, 240, 180);

        public int MaxOrbs { get; set; } = 20;
        public int OrbLife { get; set; } = 35;
        public float OrbSize { get; set; } = 0.45f;
        public int OrbSpawnInterval { get; set; } = 2;
        public float OrbRotSpeed { get; set; } = 0.08f;
        public float OrbDriftSpeed { get; set; } = 0.2f;
        public float OrbSpread { get; set; } = 5f;
        public Color OrbYinColor { get; set; } = new Color(60, 50, 100, 220);
        public Color OrbYangColor { get; set; } = new Color(230, 225, 255, 220);

        public int MaxFish { get; set; } = 8;
        public int FishLife { get; set; } = 50;
        public float FishSize { get; set; } = 0.5f;
        public float FishSpawnChance { get; set; } = 0.04f;
        public float FishRotSpeed { get; set; } = 0.12f;
        public float FishDriftSpeed { get; set; } = 0.15f;
        public Color FishYinColor { get; set; } = new Color(50, 40, 90, 200);
        public Color FishYangColor { get; set; } = new Color(220, 215, 250, 200);

        public int MaxRipples { get; set; } = 6;
        public int RippleLife { get; set; } = 45;
        public float RippleStartSize { get; set; } = 0.3f;
        public float RippleEndSize { get; set; } = 1.8f;
        public float RippleSpawnChance { get; set; } = 0.025f;
        public float RippleRotSpeed { get; set; } = 0.04f;
        public float RippleDriftSpeed { get; set; } = 0.08f;
        public Color RippleColor { get; set; } = new Color(160, 150, 220, 150);

        public int MaxSparks { get; set; } = 30;
        public int SparkLife { get; set; } = 25;
        public float SparkSize { get; set; } = 0.25f;
        public float SparkSpawnChance { get; set; } = 0.25f;
        public float SparkDriftSpeed { get; set; } = 0.35f;
        public Color SparkYinColor { get; set; } = new Color(80, 70, 140, 200);
        public Color SparkYangColor { get; set; } = new Color(220, 215, 255, 200);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<YinYangOrbParticle> orbs = new();
        private List<YinYangFishParticle> fish = new();
        private List<YinYangRippleParticle> ripples = new();
        private List<YinYangSparkParticle> sparks = new();
        private int orbCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _orbTex;
        private Texture2D _fishTex;
        private Texture2D _rippleTex;
        private Texture2D _sparkTex;
        private Texture2D _ghostTex;

        public bool HasContent => orbs.Count > 0 || fish.Count > 0 || ripples.Count > 0 || sparks.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_orbTex != null) return;
            _orbTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/YinYangTrail/YinYangTrailOrb").Value;
            _fishTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/YinYangTrail/YinYangTrailFish").Value;
            _rippleTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/YinYangTrail/YinYangTrailRipple").Value;
            _sparkTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/YinYangTrail/YinYangTrailSpark").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/YinYangTrail/YinYangTrailGhost").Value;
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

            orbCounter++;
            if (orbCounter >= OrbSpawnInterval && orbs.Count < MaxOrbs)
            {
                orbCounter = 0;
                SpawnOrb(center, velocity, moveDir);
            }

            if (fish.Count < MaxFish && Main.rand.NextFloat() < FishSpawnChance)
                SpawnFish(center, velocity, moveDir);

            if (ripples.Count < MaxRipples && Main.rand.NextFloat() < RippleSpawnChance)
                SpawnRipple(center, velocity, moveDir);

            if (sparks.Count < MaxSparks && Main.rand.NextFloat() < SparkSpawnChance)
                SpawnSpark(center, velocity, moveDir);

            for (int i = orbs.Count - 1; i >= 0; i--)
            {
                var o = orbs[i];
                o.Rotation += o.RotSpeed;
                o.Velocity *= 0.96f;
                o.Position += o.Velocity;
                o.Life--;
                if (o.Life <= 0) orbs.RemoveAt(i);
            }

            for (int i = fish.Count - 1; i >= 0; i--)
            {
                var f = fish[i];
                f.Rotation += f.RotSpeed;
                f.Velocity *= 0.97f;
                f.Position += f.Velocity;
                f.Life--;
                if (f.Life <= 0) fish.RemoveAt(i);
            }

            for (int i = ripples.Count - 1; i >= 0; i--)
            {
                var r = ripples[i];
                r.Rotation += r.RotSpeed;
                r.Velocity *= 0.98f;
                r.Position += r.Velocity;
                r.Life--;
                if (r.Life <= 0) ripples.RemoveAt(i);
            }

            for (int i = sparks.Count - 1; i >= 0; i--)
            {
                var s = sparks[i];
                s.TwinklePhase += s.TwinkleSpeed;
                s.Velocity *= 0.95f;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0) sparks.RemoveAt(i);
            }
        }

        private void SpawnOrb(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-OrbSpread, OrbSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            Vector2 drift = Main.rand.NextVector2Circular(OrbDriftSpeed, OrbDriftSpeed);
            Vector2 vel = inertia + drift;

            bool isYang = Main.rand.NextBool();
            float scale = Main.rand.NextFloat(0.6f, 1.2f) * OrbSize;
            float rotSpeed = Main.rand.NextFloat(-OrbRotSpeed, OrbRotSpeed);
            Color color = (isYang ? OrbYangColor : OrbYinColor) * Main.rand.NextFloat(0.6f, 1f);

            orbs.Add(new YinYangOrbParticle(pos, vel, OrbLife, scale, rotSpeed, isYang, color));
        }

        private void SpawnFish(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-8f, 8f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(5f, 5f);

            Vector2 drift = Main.rand.NextVector2Circular(FishDriftSpeed, FishDriftSpeed);
            bool isYang = Main.rand.NextBool();
            float scale = Main.rand.NextFloat(0.7f, 1.3f) * FishSize;
            float rotSpeed = Main.rand.NextFloat(-FishRotSpeed, FishRotSpeed);
            Color color = (isYang ? FishYangColor : FishYinColor) * Main.rand.NextFloat(0.5f, 1f);

            fish.Add(new YinYangFishParticle(pos, drift, FishLife, scale, rotSpeed, isYang, color));
        }

        private void SpawnRipple(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-6f, 6f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(4f, 4f);

            Vector2 drift = Main.rand.NextVector2Circular(RippleDriftSpeed, RippleDriftSpeed);
            float startSize = RippleStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = RippleEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            float rotSpeed = Main.rand.NextFloat(-RippleRotSpeed, RippleRotSpeed);
            Color color = RippleColor * Main.rand.NextFloat(0.5f, 1f);

            ripples.Add(new YinYangRippleParticle(pos, drift, RippleLife, startSize, endSize, rotSpeed, color));
        }

        private void SpawnSpark(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-6f, 6f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 drift = Main.rand.NextVector2Circular(SparkDriftSpeed, SparkDriftSpeed);
            bool isYang = Main.rand.NextBool();
            float scale = Main.rand.NextFloat(0.5f, 1.2f) * SparkSize;
            Color color = (isYang ? SparkYangColor : SparkYinColor) * Main.rand.NextFloat(0.5f, 1f);

            sparks.Add(new YinYangSparkParticle(pos, drift, SparkLife, scale, isYang, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (ripples.Count > 0 && _rippleTex != null)
            {
                Vector2 rippleOrigin = _rippleTex.Size() * 0.5f;
                var sortedRipples = ripples.OrderBy(r => r.Life);
                foreach (var r in sortedRipples)
                {
                    Color drawColor = r.Color * r.Alpha;
                    Vector2 pos = r.Position - Main.screenPosition;
                    sb.Draw(_rippleTex, pos, null, drawColor, r.Rotation,
                        rippleOrigin, r.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (fish.Count > 0 && _fishTex != null)
            {
                Vector2 fishOrigin = _fishTex.Size() * 0.5f;
                var sortedFish = fish.OrderBy(f => f.Life);
                foreach (var f in sortedFish)
                {
                    Color drawColor = f.Color * f.Alpha;
                    Vector2 pos = f.Position - Main.screenPosition;
                    SpriteEffects fx = f.IsYang ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    sb.Draw(_fishTex, pos, null, drawColor, f.Rotation,
                        fishOrigin, f.CurrentScale, fx, 0);
                }
            }

            if (orbs.Count > 0 && _orbTex != null)
            {
                Vector2 orbOrigin = _orbTex.Size() * 0.5f;
                var sortedOrbs = orbs.OrderBy(o => o.Life);
                foreach (var o in sortedOrbs)
                {
                    Color drawColor = o.Color * o.Alpha;
                    Vector2 pos = o.Position - Main.screenPosition;
                    SpriteEffects fx = o.IsYang ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    sb.Draw(_orbTex, pos, null, drawColor, o.Rotation,
                        orbOrigin, o.CurrentScale, fx, 0);
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
            orbs.Clear();
            fish.Clear();
            ripples.Clear();
            sparks.Clear();
            orbCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
