using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class SkyTrail : ITrail
    {
        public class CelestialArcParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float ArcSpan;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float GrowSpeed;
            public float PulsePhase;
            public float PulseSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float growIn = MathF.Min(1f, Progress * GrowSpeed);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float pulse = 0.6f + 0.4f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, growIn * fadeOut * pulse * 0.5f);
                }
            }

            public float CurrentArc => ArcSpan * MathF.Min(1f, Progress * GrowSpeed);

            public CelestialArcParticle(Vector2 pos, Vector2 vel, int life, float scale, float arcSpan, float rotation, float growSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                ArcSpan = arcSpan;
                Rotation = rotation;
                GrowSpeed = growSpeed;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = Main.rand.NextFloat(0.04f, 0.1f);
                Color = color;
            }
        }

        public class AuroraBandParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Length;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float WavePhase;
            public float WaveSpeed;
            public float WaveAmplitude;
            public float HueShift;
            public float HueSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 2f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.45f);
                }
            }

            public float CurrentLength => Length * MathF.Min(1f, Progress * 2.5f) * MathF.Max(0f, 1f - Progress * 0.4f);

            public float CurrentWave => MathF.Sin(WavePhase) * WaveAmplitude;

            public float CurrentHue => HueShift + Progress * HueSpeed;

            public AuroraBandParticle(Vector2 pos, Vector2 vel, int life, float scale, float length, float rotation, float waveSpeed, float waveAmplitude, float hueShift, float hueSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Length = length;
                Rotation = rotation;
                WavePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                WaveSpeed = waveSpeed;
                WaveAmplitude = waveAmplitude;
                HueShift = hueShift;
                HueSpeed = hueSpeed;
                Color = color;
            }
        }

        public class ZenithMarkParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpinSpeed;
            public float PulsePhase;
            public float PulseSpeed;
            public float CrossLength;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 6f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float pulse = 0.5f + 0.5f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, fadeIn * fadeOut * pulse);
                }
            }

            public float CurrentCrossLength => CrossLength * MathF.Min(1f, Progress * 5f) * MathF.Max(0f, 1f - Progress * 0.3f);

            public ZenithMarkParticle(Vector2 pos, Vector2 vel, int life, float scale, float spinSpeed, float crossLength, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                SpinSpeed = spinSpeed;
                CrossLength = crossLength;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = Main.rand.NextFloat(0.08f, 0.16f);
                Color = color;
            }
        }

        public string Name { get; set; } = "SkyTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 12;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.6f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(100, 140, 220, 140);

        public int MaxCelestialArcs { get; set; } = 6;
        public int CelestialArcLife { get; set; } = 55;
        public float CelestialArcScale { get; set; } = 0.5f;
        public float CelestialArcSpan { get; set; } = MathHelper.Pi * 0.6f;
        public float CelestialArcSpawnChance { get; set; } = 0.03f;
        public float CelestialArcGrowSpeed { get; set; } = 2f;
        public float CelestialArcDriftSpeed { get; set; } = 0.04f;
        public Color CelestialArcColor { get; set; } = new Color(80, 140, 240, 200);

        public int MaxAuroraBands { get; set; } = 10;
        public int AuroraBandLife { get; set; } = 45;
        public float AuroraBandSize { get; set; } = 0.3f;
        public float AuroraBandLength { get; set; } = 28f;
        public float AuroraBandSpawnChance { get; set; } = 0.07f;
        public float AuroraBandWaveSpeed { get; set; } = 0.1f;
        public float AuroraBandWaveAmplitude { get; set; } = 0.3f;
        public float AuroraBandHueSpeed { get; set; } = 1.5f;
        public float AuroraBandDriftSpeed { get; set; } = 0.06f;
        public Color AuroraBandColor { get; set; } = new Color(60, 180, 220, 200);

        public int MaxZenithMarks { get; set; } = 12;
        public int ZenithMarkLife { get; set; } = 25;
        public float ZenithMarkSize { get; set; } = 0.4f;
        public float ZenithMarkSpawnChance { get; set; } = 0.1f;
        public float ZenithMarkSpinSpeed { get; set; } = 0.04f;
        public float ZenithMarkCrossLength { get; set; } = 14f;
        public float ZenithMarkDriftSpeed { get; set; } = 0.12f;
        public Color ZenithMarkColor { get; set; } = new Color(160, 200, 255, 240);

        public float InertiaFactor { get; set; } = 0.12f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<CelestialArcParticle> celestialArcs = new();
        private List<AuroraBandParticle> auroraBands = new();
        private List<ZenithMarkParticle> zenithMarks = new();

        private GhostTrail _ghostTrail;

        private Texture2D _arcTex;
        private Texture2D _auroraTex;
        private Texture2D _zenithTex;
        private Texture2D _ghostTex;

        public bool HasContent => celestialArcs.Count > 0 || auroraBands.Count > 0 || zenithMarks.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_arcTex != null) return;
            _arcTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SkyTrail/SkyTrailArc").Value;
            _auroraTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SkyTrail/SkyTrailAurora").Value;
            _zenithTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SkyTrail/SkyTrailZenith").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SkyTrail/SkyTrailGhost").Value;
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

            if (celestialArcs.Count < MaxCelestialArcs && Main.rand.NextFloat() < CelestialArcSpawnChance)
                SpawnCelestialArc(center, velocity, moveDir);

            if (auroraBands.Count < MaxAuroraBands && Main.rand.NextFloat() < AuroraBandSpawnChance)
                SpawnAuroraBand(center, velocity, moveDir);

            if (zenithMarks.Count < MaxZenithMarks && Main.rand.NextFloat() < ZenithMarkSpawnChance)
                SpawnZenithMark(center, velocity, moveDir);

            for (int i = celestialArcs.Count - 1; i >= 0; i--)
            {
                var a = celestialArcs[i];
                a.PulsePhase += a.PulseSpeed;
                a.Velocity *= 0.98f;
                a.Position += a.Velocity;
                a.Life--;
                if (a.Life <= 0) celestialArcs.RemoveAt(i);
            }

            for (int i = auroraBands.Count - 1; i >= 0; i--)
            {
                var b = auroraBands[i];
                b.WavePhase += b.WaveSpeed;
                b.Rotation += b.CurrentWave * 0.02f;
                b.Velocity *= 0.97f;
                b.Position += b.Velocity;
                b.Life--;
                if (b.Life <= 0) auroraBands.RemoveAt(i);
            }

            for (int i = zenithMarks.Count - 1; i >= 0; i--)
            {
                var z = zenithMarks[i];
                z.PulsePhase += z.PulseSpeed;
                z.Rotation += z.SpinSpeed;
                z.Velocity *= 0.96f;
                z.Position += z.Velocity;
                z.Life--;
                if (z.Life <= 0) zenithMarks.RemoveAt(i);
            }
        }

        private void SpawnCelestialArc(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-6f, 6f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(4f, 4f);

            Vector2 drift = Main.rand.NextVector2Circular(CelestialArcDriftSpeed, CelestialArcDriftSpeed);
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
            float arcSpan = CelestialArcSpan * Main.rand.NextFloat(0.6f, 1.4f);
            float scale = CelestialArcScale * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = CelestialArcColor * Main.rand.NextFloat(0.6f, 1f);

            celestialArcs.Add(new CelestialArcParticle(pos, drift, CelestialArcLife, scale, arcSpan, rotation, CelestialArcGrowSpeed, color));
        }

        private void SpawnAuroraBand(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-7f, 7f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(5f, 5f);

            Vector2 drift = Main.rand.NextVector2Circular(AuroraBandDriftSpeed, AuroraBandDriftSpeed);
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.3f, 0.3f);
            float length = AuroraBandLength * Main.rand.NextFloat(0.6f, 1.4f);
            float scale = AuroraBandSize * Main.rand.NextFloat(0.7f, 1.3f);
            float hueShift = Main.rand.NextFloat(0f, 360f);
            Color color = AuroraBandColor * Main.rand.NextFloat(0.6f, 1f);

            auroraBands.Add(new AuroraBandParticle(pos, drift, AuroraBandLife, scale, length, rotation, AuroraBandWaveSpeed, AuroraBandWaveAmplitude, hueShift, AuroraBandHueSpeed, color));
        }

        private void SpawnZenithMark(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-5f, 5f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 inertia = -velocity * InertiaFactor * 0.2f;
            Vector2 drift = Main.rand.NextVector2Circular(ZenithMarkDriftSpeed, ZenithMarkDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.7f, 1.3f) * ZenithMarkSize;
            float crossLength = Main.rand.NextFloat(0.7f, 1.3f) * ZenithMarkCrossLength;
            float spinSpeed = Main.rand.NextFloat(-ZenithMarkSpinSpeed, ZenithMarkSpinSpeed);
            Color color = ZenithMarkColor * Main.rand.NextFloat(0.6f, 1f);

            zenithMarks.Add(new ZenithMarkParticle(pos, vel, ZenithMarkLife, scale, spinSpeed, crossLength, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (auroraBands.Count > 0 && _auroraTex != null)
            {
                Vector2 auroraOrigin = new Vector2(0f, _auroraTex.Height * 0.5f);
                var sorted = auroraBands.OrderBy(b => b.Life);
                foreach (var b in sorted)
                {
                    float hue = (b.CurrentHue % 360f) / 360f;
                    Color shiftedColor = ColorFromHSV(hue, 0.6f, 1f) * b.Alpha;
                    Vector2 pos = b.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(b.CurrentLength / _auroraTex.Width, b.Scale);
                    sb.Draw(_auroraTex, pos, null, shiftedColor, b.Rotation,
                        auroraOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (celestialArcs.Count > 0 && _arcTex != null)
            {
                Vector2 arcOrigin = new Vector2(0f, _arcTex.Height * 0.5f);
                var sorted = celestialArcs.OrderBy(a => a.Life);
                foreach (var a in sorted)
                {
                    Color drawColor = a.Color * a.Alpha;
                    Vector2 pos = a.Position - Main.screenPosition;
                    float arcFraction = a.CurrentArc / (MathHelper.Pi * 0.6f);
                    Vector2 scale = new Vector2(arcFraction * a.Scale * 22f / _arcTex.Width, a.Scale);
                    sb.Draw(_arcTex, pos, null, drawColor, a.Rotation,
                        arcOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (zenithMarks.Count > 0 && _zenithTex != null)
            {
                Vector2 zenithOrigin = _zenithTex.Size() * 0.5f;
                var sorted = zenithMarks.OrderBy(z => z.Life);
                foreach (var z in sorted)
                {
                    Color drawColor = z.Color * z.Alpha;
                    Vector2 pos = z.Position - Main.screenPosition;
                    float crossScale = z.CurrentCrossLength / 14f * z.Scale;
                    sb.Draw(_zenithTex, pos, null, drawColor, z.Rotation,
                        zenithOrigin, crossScale, SpriteEffects.None, 0);
                }
            }
        }

        private static Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = (int)(hue * 6f) % 6;
            float f = hue * 6f - (int)(hue * 6f);
            float p = value * (1f - saturation);
            float q = value * (1f - f * saturation);
            float t = value * (1f - (1f - f) * saturation);

            float r, g, b;
            switch (hi)
            {
                case 0: r = value; g = t; b = p; break;
                case 1: r = q; g = value; b = p; break;
                case 2: r = p; g = value; b = t; break;
                case 3: r = p; g = q; b = value; break;
                case 4: r = t; g = p; b = value; break;
                default: r = value; g = p; b = q; break;
            }

            return new Color((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        public void Clear()
        {
            celestialArcs.Clear();
            auroraBands.Clear();
            zenithMarks.Clear();
            _ghostTrail?.Clear();
        }
    }
}
