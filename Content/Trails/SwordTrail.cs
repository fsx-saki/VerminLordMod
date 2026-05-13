using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class SwordTrail : ITrail
    {
        public class SwordGlowParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Stretch;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpinSpeed;
            public float PulsePhase;
            public float PulseSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float flashIn = MathF.Min(1f, Progress * 6f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float pulse = 0.8f + 0.2f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, flashIn * fadeOut * pulse);
                }
            }

            public SwordGlowParticle(Vector2 pos, Vector2 vel, int life, float scale, float stretch, float rotation, float spinSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Stretch = stretch;
                Rotation = rotation;
                SpinSpeed = spinSpeed;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = Main.rand.NextFloat(0.05f, 0.12f);
                Color = color;
            }
        }

        public class SwordQiParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float DriftAngle;
            public float DriftSpeed;
            public float WobblePhase;
            public float WobbleAmplitude;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 3f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float wobble = 1f - 0.3f * MathF.Abs(MathF.Sin(WobblePhase));
                    return MathF.Max(0f, fadeIn * fadeOut * wobble);
                }
            }

            public SwordQiParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotation, float driftAngle, float driftSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = rotation;
                DriftAngle = driftAngle;
                DriftSpeed = driftSpeed;
                WobblePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                WobbleAmplitude = Main.rand.NextFloat(0.5f, 1.5f);
                Color = color;
            }
        }

        public class SwordScarParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Length;
            public float Width;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float CurlPhase;
            public float CurlAmount;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float appear = MathF.Min(1f, Progress * 4f);
                    float vanish = 1f - MathF.Pow(Progress, 3f);
                    return MathF.Max(0f, appear * vanish * 0.6f);
                }
            }

            public float CurrentLength => Length * (1f - Progress * 0.3f);

            public float CurrentWidth => Width * (1f - Progress * 0.7f);

            public float CurrentCurl => CurlAmount * MathF.Sin(CurlPhase * Progress * MathHelper.TwoPi);

            public SwordScarParticle(Vector2 pos, Vector2 vel, int life, float length, float width, float rotation, float curlAmount, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Length = length;
                Width = width;
                Rotation = rotation;
                CurlPhase = Main.rand.NextFloat(0.5f, 1.5f);
                CurlAmount = curlAmount;
                Color = color;
            }
        }

        public string Name { get; set; } = "SwordTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.15f;
        public float GhostLengthScale { get; set; } = 1.3f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(180, 200, 255, 140);

        public int MaxSwordGlows { get; set; } = 15;
        public int SwordGlowLife { get; set; } = 20;
        public float SwordGlowSize { get; set; } = 0.6f;
        public float SwordGlowStretch { get; set; } = 3f;
        public float SwordGlowSpawnChance { get; set; } = 0.15f;
        public float SwordGlowSpinSpeed { get; set; } = 0.15f;
        public float SwordGlowDriftSpeed { get; set; } = 0.4f;
        public Color SwordGlowColor { get; set; } = new Color(200, 220, 255, 230);

        public int MaxSwordQis { get; set; } = 25;
        public int SwordQiLife { get; set; } = 35;
        public float SwordQiSize { get; set; } = 0.3f;
        public int SwordQiSpawnInterval { get; set; } = 2;
        public float SwordQiDriftSpeed { get; set; } = 0.6f;
        public float SwordQiSpread { get; set; } = 5f;
        public Color SwordQiColor { get; set; } = new Color(160, 200, 255, 180);

        public int MaxSwordScars { get; set; } = 8;
        public int SwordScarLife { get; set; } = 25;
        public float SwordScarLength { get; set; } = 30f;
        public float SwordScarWidth { get; set; } = 0.25f;
        public float SwordScarSpawnChance { get; set; } = 0.05f;
        public float SwordScarCurlAmount { get; set; } = 3f;
        public float SwordScarDriftSpeed { get; set; } = 0.05f;
        public Color SwordScarColor { get; set; } = new Color(140, 180, 255, 200);

        public float InertiaFactor { get; set; } = 0.15f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<SwordGlowParticle> swordGlows = new();
        private List<SwordQiParticle> swordQis = new();
        private List<SwordScarParticle> swordScars = new();
        private int qiCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _glowTex;
        private Texture2D _qiTex;
        private Texture2D _scarTex;
        private Texture2D _ghostTex;

        public bool HasContent => swordGlows.Count > 0 || swordQis.Count > 0 || swordScars.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_glowTex != null) return;
            _glowTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SwordTrail/SwordTrailGlow").Value;
            _qiTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SwordTrail/SwordTrailQi").Value;
            _scarTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SwordTrail/SwordTrailScar").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SwordTrail/SwordTrailGhost").Value;
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

            if (swordGlows.Count < MaxSwordGlows && Main.rand.NextFloat() < SwordGlowSpawnChance)
                SpawnSwordGlow(center, velocity, moveDir);

            qiCounter++;
            if (qiCounter >= SwordQiSpawnInterval && swordQis.Count < MaxSwordQis)
            {
                qiCounter = 0;
                SpawnSwordQi(center, velocity, moveDir);
            }

            if (swordScars.Count < MaxSwordScars && Main.rand.NextFloat() < SwordScarSpawnChance)
                SpawnSwordScar(center, velocity, moveDir);

            for (int i = swordGlows.Count - 1; i >= 0; i--)
            {
                var g = swordGlows[i];
                g.Rotation += g.SpinSpeed;
                g.PulsePhase += g.PulseSpeed;
                g.Velocity *= 0.95f;
                g.Position += g.Velocity;
                g.Life--;
                if (g.Life <= 0) swordGlows.RemoveAt(i);
            }

            for (int i = swordQis.Count - 1; i >= 0; i--)
            {
                var q = swordQis[i];
                q.WobblePhase += 0.08f;
                Vector2 wobbleOffset = new Vector2(
                    MathF.Sin(q.WobblePhase) * q.WobbleAmplitude,
                    MathF.Cos(q.WobblePhase * 0.7f) * q.WobbleAmplitude * 0.5f
                );
                q.Velocity *= 0.97f;
                q.Position += q.Velocity + wobbleOffset * 0.05f;
                q.Life--;
                if (q.Life <= 0) swordQis.RemoveAt(i);
            }

            for (int i = swordScars.Count - 1; i >= 0; i--)
            {
                var s = swordScars[i];
                s.Velocity *= 0.98f;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0) swordScars.RemoveAt(i);
            }
        }

        private void SpawnSwordGlow(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-3f, 3f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(1f, 1f);

            Vector2 inertia = -velocity * InertiaFactor * 0.4f;
            Vector2 drift = Main.rand.NextVector2Circular(SwordGlowDriftSpeed, SwordGlowDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.6f, 1.3f) * SwordGlowSize;
            float stretch = Main.rand.NextFloat(0.8f, 1.5f) * SwordGlowStretch;
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
            float spinSpeed = Main.rand.NextFloat(-SwordGlowSpinSpeed, SwordGlowSpinSpeed);
            Color color = SwordGlowColor * Main.rand.NextFloat(0.7f, 1f);

            swordGlows.Add(new SwordGlowParticle(pos, vel, SwordGlowLife, scale, stretch, rotation, spinSpeed, color));
        }

        private void SpawnSwordQi(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-SwordQiSpread, SwordQiSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            float driftAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 drift = driftAngle.ToRotationVector2() * Main.rand.NextFloat(0.2f, SwordQiDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.5f, 1.2f) * SwordQiSize;
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Color color = SwordQiColor * Main.rand.NextFloat(0.5f, 1f);

            swordQis.Add(new SwordQiParticle(pos, vel, SwordQiLife, scale, rotation, driftAngle, SwordQiDriftSpeed, color));
        }

        private void SpawnSwordScar(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-2f, 2f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(1f, 1f);

            Vector2 drift = Main.rand.NextVector2Circular(SwordScarDriftSpeed, SwordScarDriftSpeed);
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.2f, 0.2f);
            float length = SwordScarLength * Main.rand.NextFloat(0.7f, 1.4f);
            float width = SwordScarWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float curlAmount = SwordScarCurlAmount * Main.rand.NextFloat(0.6f, 1.2f);
            Color color = SwordScarColor * Main.rand.NextFloat(0.6f, 1f);

            swordScars.Add(new SwordScarParticle(pos, drift, SwordScarLife, length, width, rotation, curlAmount, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (swordScars.Count > 0 && _scarTex != null)
            {
                Vector2 scarOrigin = new Vector2(0f, _scarTex.Height * 0.5f);
                var sorted = swordScars.OrderBy(s => s.Life);
                foreach (var s in sorted)
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    float curlOffset = s.CurrentCurl;
                    Vector2 perpDir = new Vector2(-MathF.Sin(s.Rotation), MathF.Cos(s.Rotation));
                    Vector2 curlPos = pos + perpDir * curlOffset;
                    Vector2 scale = new Vector2(s.CurrentLength / _scarTex.Width, s.CurrentWidth);
                    sb.Draw(_scarTex, curlPos, null, drawColor, s.Rotation,
                        scarOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (swordGlows.Count > 0 && _glowTex != null)
            {
                Vector2 glowOrigin = new Vector2(_glowTex.Width * 0.5f, _glowTex.Height * 0.5f);
                var sorted = swordGlows.OrderBy(g => g.Life);
                foreach (var g in sorted)
                {
                    Color drawColor = g.Color * g.Alpha;
                    Vector2 pos = g.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(g.Scale * g.Stretch, g.Scale);
                    sb.Draw(_glowTex, pos, null, drawColor, g.Rotation,
                        glowOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (swordQis.Count > 0 && _qiTex != null)
            {
                Vector2 qiOrigin = new Vector2(_qiTex.Width * 0.5f, _qiTex.Height * 0.5f);
                var sorted = swordQis.OrderBy(q => q.Life);
                foreach (var q in sorted)
                {
                    Color drawColor = q.Color * q.Alpha;
                    Vector2 pos = q.Position - Main.screenPosition;
                    sb.Draw(_qiTex, pos, null, drawColor, q.Rotation,
                        qiOrigin, q.Scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            swordGlows.Clear();
            swordQis.Clear();
            swordScars.Clear();
            qiCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}