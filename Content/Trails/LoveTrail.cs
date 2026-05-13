using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class LoveTrail : ITrail
    {
        public class RedThreadParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Length;
            public float Width;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float WavePhase;
            public float WaveSpeed;
            public float WaveAmplitude;
            public float CurlPhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 3f);
                    float fadeOut = 1f - MathF.Pow(Progress, 2.5f);
                    float pulse = 0.8f + 0.2f * MathF.Sin(CurlPhase);
                    return MathF.Max(0f, fadeIn * fadeOut * pulse * 0.7f);
                }
            }

            public float CurrentLength => Length * (1f - Progress * 0.2f);

            public float CurrentWidth => Width * (1f - Progress * 0.4f);

            public float WaveOffset => MathF.Sin(WavePhase) * WaveAmplitude;

            public RedThreadParticle(Vector2 pos, Vector2 vel, int life, float length, float width, float rotation, float waveSpeed, float waveAmplitude, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Length = length;
                Width = width;
                Rotation = rotation;
                WavePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                WaveSpeed = waveSpeed;
                WaveAmplitude = waveAmplitude;
                CurlPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public class HeartGlowParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float FloatPhase;
            public float FloatSpeed;
            public float PulsePhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float appear = MathF.Min(1f, Progress * 4f);
                    float vanish = 1f - MathF.Pow(Progress, 2f);
                    float pulse = 0.7f + 0.3f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, appear * vanish * pulse);
                }
            }

            public HeartGlowParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotation, float floatSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = rotation;
                FloatPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                FloatSpeed = floatSpeed;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public class LoveMistParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float ExpandRate;
            public float DriftAngle;
            public float DriftSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 2f);
                    float fadeOut = 1f - MathF.Pow(Progress, 3f);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.35f);
                }
            }

            public float CurrentScale => Scale * (1f + Progress * ExpandRate);

            public LoveMistParticle(Vector2 pos, Vector2 vel, int life, float scale, float expandRate, float driftAngle, float driftSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                ExpandRate = expandRate;
                DriftAngle = driftAngle;
                DriftSpeed = driftSpeed;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public string Name { get; set; } = "LoveTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.16f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(255, 180, 200, 140);

        public int MaxRedThreads { get; set; } = 12;
        public int RedThreadLife { get; set; } = 30;
        public float RedThreadLength { get; set; } = 35f;
        public float RedThreadWidth { get; set; } = 0.2f;
        public float RedThreadSpawnChance { get; set; } = 0.08f;
        public float RedThreadWaveSpeed { get; set; } = 0.06f;
        public float RedThreadWaveAmplitude { get; set; } = 2f;
        public float RedThreadDriftSpeed { get; set; } = 0.1f;
        public Color RedThreadColor { get; set; } = new Color(255, 120, 150, 220);

        public int MaxHeartGlows { get; set; } = 12;
        public int HeartGlowLife { get; set; } = 35;
        public float HeartGlowSize { get; set; } = 0.5f;
        public float HeartGlowSpawnChance { get; set; } = 0.06f;
        public float HeartGlowFloatSpeed { get; set; } = 0.3f;
        public float HeartGlowSpread { get; set; } = 5f;
        public Color HeartGlowColor { get; set; } = new Color(255, 100, 150, 200);

        public int MaxLoveMists { get; set; } = 15;
        public int LoveMistLife { get; set; } = 50;
        public float LoveMistSize { get; set; } = 0.6f;
        public float LoveMistExpandRate { get; set; } = 1.5f;
        public int LoveMistSpawnInterval { get; set; } = 4;
        public float LoveMistDriftSpeed { get; set; } = 0.4f;
        public float LoveMistSpread { get; set; } = 6f;
        public Color LoveMistColor { get; set; } = new Color(255, 150, 180, 180);

        public float InertiaFactor { get; set; } = 0.12f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<RedThreadParticle> redThreads = new();
        private List<HeartGlowParticle> heartGlows = new();
        private List<LoveMistParticle> loveMists = new();
        private int mistCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _threadTex;
        private Texture2D _heartTex;
        private Texture2D _mistTex;
        private Texture2D _ghostTex;

        public bool HasContent => redThreads.Count > 0 || heartGlows.Count > 0 || loveMists.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_threadTex != null) return;
            _threadTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LoveTrail/LoveTrailThread").Value;
            _heartTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LoveTrail/LoveTrailHeart").Value;
            _mistTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LoveTrail/LoveTrailMist").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LoveTrail/LoveTrailGhost").Value;
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

            if (redThreads.Count < MaxRedThreads && Main.rand.NextFloat() < RedThreadSpawnChance)
                SpawnRedThread(center, velocity, moveDir);

            if (heartGlows.Count < MaxHeartGlows && Main.rand.NextFloat() < HeartGlowSpawnChance)
                SpawnHeartGlow(center, velocity, moveDir);

            mistCounter++;
            if (mistCounter >= LoveMistSpawnInterval && loveMists.Count < MaxLoveMists)
            {
                mistCounter = 0;
                SpawnLoveMist(center, velocity, moveDir);
            }

            for (int i = redThreads.Count - 1; i >= 0; i--)
            {
                var t = redThreads[i];
                t.WavePhase += t.WaveSpeed;
                t.CurlPhase += 0.04f;
                t.Velocity *= 0.97f;
                t.Position += t.Velocity;
                t.Life--;
                if (t.Life <= 0) redThreads.RemoveAt(i);
            }

            for (int i = heartGlows.Count - 1; i >= 0; i--)
            {
                var h = heartGlows[i];
                h.FloatPhase += h.FloatSpeed;
                h.PulsePhase += 0.06f;
                Vector2 floatOffset = new Vector2(
                    MathF.Sin(h.FloatPhase) * 0.3f,
                    MathF.Cos(h.FloatPhase * 0.7f) * 0.2f - 0.15f
                );
                h.Velocity *= 0.96f;
                h.Position += h.Velocity + floatOffset;
                h.Life--;
                if (h.Life <= 0) heartGlows.RemoveAt(i);
            }

            for (int i = loveMists.Count - 1; i >= 0; i--)
            {
                var m = loveMists[i];
                m.Velocity *= 0.94f;
                m.Position += m.Velocity;
                m.Life--;
                if (m.Life <= 0) loveMists.RemoveAt(i);
            }
        }

        private void SpawnRedThread(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-3f, 3f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(1f, 1f);

            Vector2 drift = Main.rand.NextVector2Circular(RedThreadDriftSpeed, RedThreadDriftSpeed);
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.3f, 0.3f);
            float length = RedThreadLength * Main.rand.NextFloat(0.7f, 1.4f);
            float width = RedThreadWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float waveSpeed = RedThreadWaveSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float waveAmplitude = RedThreadWaveAmplitude * Main.rand.NextFloat(0.6f, 1.4f);
            Color color = RedThreadColor * Main.rand.NextFloat(0.6f, 1f);

            redThreads.Add(new RedThreadParticle(pos, drift, RedThreadLife, length, width, rotation, waveSpeed, waveAmplitude, color));
        }

        private void SpawnHeartGlow(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-HeartGlowSpread, HeartGlowSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            Vector2 floatUp = new Vector2(0f, -Main.rand.NextFloat(0.2f, HeartGlowFloatSpeed));
            Vector2 drift = Main.rand.NextVector2Circular(0.3f, 0.3f);
            Vector2 vel = inertia + floatUp + drift;

            float scale = Main.rand.NextFloat(0.6f, 1.3f) * HeartGlowSize;
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            float floatSpeed = HeartGlowFloatSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = HeartGlowColor * Main.rand.NextFloat(0.6f, 1f);

            heartGlows.Add(new HeartGlowParticle(pos, vel, HeartGlowLife, scale, rotation, floatSpeed, color));
        }

        private void SpawnLoveMist(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-LoveMistSpread, LoveMistSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 inertia = -velocity * InertiaFactor * 0.5f;
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 burst = angle.ToRotationVector2() * Main.rand.NextFloat(0.2f, LoveMistDriftSpeed);
            Vector2 vel = inertia + burst;

            float scale = Main.rand.NextFloat(0.5f, 1.2f) * LoveMistSize;
            float expandRate = LoveMistExpandRate * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = LoveMistColor * Main.rand.NextFloat(0.5f, 1f);

            loveMists.Add(new LoveMistParticle(pos, vel, LoveMistLife, scale, expandRate, 0f, 0f, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (loveMists.Count > 0 && _mistTex != null)
            {
                Vector2 mistOrigin = new Vector2(_mistTex.Width * 0.5f, _mistTex.Height * 0.5f);
                var sorted = loveMists.OrderBy(m => m.Life);
                foreach (var m in sorted)
                {
                    Color drawColor = m.Color * m.Alpha;
                    Vector2 pos = m.Position - Main.screenPosition;
                    sb.Draw(_mistTex, pos, null, drawColor, m.Rotation,
                        mistOrigin, m.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (redThreads.Count > 0 && _threadTex != null)
            {
                Vector2 threadOrigin = new Vector2(0f, _threadTex.Height * 0.5f);
                var sorted = redThreads.OrderBy(t => t.Life);
                foreach (var t in sorted)
                {
                    Color drawColor = t.Color * t.Alpha;
                    Vector2 pos = t.Position - Main.screenPosition;
                    Vector2 waveOffset = new Vector2(0f, t.WaveOffset);
                    Vector2 wavePos = pos + waveOffset;
                    Vector2 scale = new Vector2(t.CurrentLength / _threadTex.Width, t.CurrentWidth);
                    sb.Draw(_threadTex, wavePos, null, drawColor, t.Rotation,
                        threadOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (heartGlows.Count > 0 && _heartTex != null)
            {
                Vector2 heartOrigin = new Vector2(_heartTex.Width * 0.5f, _heartTex.Height * 0.5f);
                var sorted = heartGlows.OrderBy(h => h.Life);
                foreach (var h in sorted)
                {
                    Color drawColor = h.Color * h.Alpha;
                    Vector2 pos = h.Position - Main.screenPosition;
                    sb.Draw(_heartTex, pos, null, drawColor, h.Rotation,
                        heartOrigin, h.Scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            redThreads.Clear();
            heartGlows.Clear();
            loveMists.Clear();
            mistCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}