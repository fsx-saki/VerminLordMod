using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class MoonTrail : ITrail
    {
        public class MoonCrescentParticle
        {
            public Vector2 Center;
            public float OrbitRadius;
            public float Angle;
            public float AngularSpeed;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Phase;
            public float PhaseSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float shimmer = 0.7f + 0.3f * MathF.Sin(Phase);
                    return MathF.Max(0f, fadeIn * fadeOut * shimmer);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.2f);

            public Vector2 Position => Center + new Vector2(MathF.Cos(Angle), MathF.Sin(Angle)) * OrbitRadius;

            public MoonCrescentParticle(Vector2 center, float orbitRadius, float angle, float angularSpeed, int life, float scale, Color color)
            {
                Center = center;
                OrbitRadius = orbitRadius;
                Angle = angle;
                AngularSpeed = angularSpeed;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Phase = Main.rand.NextFloat(MathHelper.TwoPi);
                PhaseSpeed = Main.rand.NextFloat(0.05f, 0.12f);
                Color = color;
            }
        }

        public class MoonBeamParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Length;
            public int Life;
            public int MaxLife;
            public float SwayPhase;
            public float SwaySpeed;
            public float SwayAmplitude;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = 1f - Progress * Progress;
                    return MathF.Max(0f, fadeIn * fadeOut * 0.5f);
                }
            }

            public float CurrentLength => Length * (1f - Progress * 0.3f);

            public float CurrentWidth => Scale * (1f - Progress * 0.4f);

            public MoonBeamParticle(Vector2 pos, Vector2 vel, int life, float scale, float length, float swaySpeed, float swayAmp, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Length = length;
                SwayPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                SwaySpeed = swaySpeed;
                SwayAmplitude = swayAmp;
                Color = color;
            }
        }

        public class MoonTideParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float WavePhase;
            public float WaveSpeed;
            public float WaveAmplitude;
            public float ExpandSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float expand = MathF.Min(1f, Progress * ExpandSpeed);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float wave = 0.6f + 0.4f * MathF.Sin(WavePhase);
                    return MathF.Max(0f, expand * fadeOut * wave * 0.4f);
                }
            }

            public float CurrentScale
            {
                get
                {
                    float expand = MathF.Min(1f, Progress * ExpandSpeed);
                    return Scale + (MaxScale - Scale) * expand;
                }
            }

            public MoonTideParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float expandSpeed, float waveSpeed, float waveAmp, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                ExpandSpeed = expandSpeed;
                WavePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                WaveSpeed = waveSpeed;
                WaveAmplitude = waveAmp;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public string Name { get; set; } = "MoonTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.22f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.4f;
        public Color GhostColor { get; set; } = new Color(180, 200, 230, 180);

        public int MaxCrescents { get; set; } = 16;
        public int CrescentLife { get; set; } = 40;
        public float CrescentSize { get; set; } = 0.5f;
        public float CrescentOrbitRadius { get; set; } = 18f;
        public float CrescentAngularSpeed { get; set; } = 0.06f;
        public float CrescentSpawnChance { get; set; } = 0.12f;
        public Color CrescentColor { get; set; } = new Color(200, 215, 240, 220);

        public int MaxBeams { get; set; } = 20;
        public int BeamLife { get; set; } = 25;
        public float BeamSize { get; set; } = 0.35f;
        public float BeamLength { get; set; } = 16f;
        public int BeamSpawnInterval { get; set; } = 2;
        public float BeamSwaySpeed { get; set; } = 0.1f;
        public float BeamSwayAmp { get; set; } = 0.3f;
        public float BeamSpread { get; set; } = 5f;
        public float BeamDriftSpeed { get; set; } = 0.15f;
        public Color BeamColor { get; set; } = new Color(180, 200, 235, 210);

        public int MaxTides { get; set; } = 6;
        public int TideLife { get; set; } = 55;
        public float TideStartSize { get; set; } = 0.2f;
        public float TideEndSize { get; set; } = 2.0f;
        public float TideSpawnChance { get; set; } = 0.02f;
        public float TideExpandSpeed { get; set; } = 1.8f;
        public float TideWaveSpeed { get; set; } = 0.08f;
        public float TideWaveAmp { get; set; } = 1f;
        public float TideDriftSpeed { get; set; } = 0.05f;
        public Color TideColor { get; set; } = new Color(160, 185, 220, 170);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<MoonCrescentParticle> crescents = new();
        private List<MoonBeamParticle> beams = new();
        private List<MoonTideParticle> tides = new();
        private int beamCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _crescentTex;
        private Texture2D _beamTex;
        private Texture2D _tideTex;
        private Texture2D _ghostTex;

        private Vector2 _lastCenter;

        public bool HasContent => crescents.Count > 0 || beams.Count > 0 || tides.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_crescentTex != null) return;
            _crescentTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MoonTrail/MoonTrailCrescent").Value;
            _beamTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MoonTrail/MoonTrailBeam").Value;
            _tideTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MoonTrail/MoonTrailTide").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MoonTrail/MoonTrailGhost").Value;
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

            _lastCenter = center;

            if (_ghostTrail != null)
                _ghostTrail.Update(center, velocity);

            Vector2 moveDir = velocity.SafeNormalize(Vector2.UnitX);

            if (crescents.Count < MaxCrescents && Main.rand.NextFloat() < CrescentSpawnChance)
                SpawnCrescent(center, velocity);

            beamCounter++;
            if (beamCounter >= BeamSpawnInterval && beams.Count < MaxBeams)
            {
                beamCounter = 0;
                SpawnBeam(center, velocity, moveDir);
            }

            if (tides.Count < MaxTides && Main.rand.NextFloat() < TideSpawnChance)
                SpawnTide(center, velocity, moveDir);

            for (int i = crescents.Count - 1; i >= 0; i--)
            {
                var c = crescents[i];
                c.Angle += c.AngularSpeed;
                c.Phase += c.PhaseSpeed;
                c.Center += (_lastCenter - c.Center) * 0.04f;
                c.OrbitRadius *= 1f + 0.003f;
                c.Life--;
                if (c.Life <= 0) crescents.RemoveAt(i);
            }

            for (int i = beams.Count - 1; i >= 0; i--)
            {
                var b = beams[i];
                b.SwayPhase += b.SwaySpeed;
                b.Velocity.X += MathF.Sin(b.SwayPhase) * b.SwayAmplitude * 0.05f;
                b.Velocity *= 0.97f;
                b.Position += b.Velocity;
                b.Life--;
                if (b.Life <= 0) beams.RemoveAt(i);
            }

            for (int i = tides.Count - 1; i >= 0; i--)
            {
                var t = tides[i];
                t.WavePhase += t.WaveSpeed;
                t.Velocity *= 0.98f;
                t.Position += t.Velocity;
                t.Life--;
                if (t.Life <= 0) tides.RemoveAt(i);
            }
        }

        private void SpawnCrescent(Vector2 center, Vector2 velocity)
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float orbitRadius = CrescentOrbitRadius * Main.rand.NextFloat(0.6f, 1.4f);
            float angularSpeed = CrescentAngularSpeed * Main.rand.NextFloat(0.7f, 1.3f) * (Main.rand.NextBool() ? 1f : -1f);
            float scale = Main.rand.NextFloat(0.5f, 1.2f) * CrescentSize;
            Color color = CrescentColor * Main.rand.NextFloat(0.6f, 1f);

            crescents.Add(new MoonCrescentParticle(center, orbitRadius, angle, angularSpeed, CrescentLife, scale, color));
        }

        private void SpawnBeam(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-BeamSpread, BeamSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.15f;
            Vector2 drift = Main.rand.NextVector2Circular(BeamDriftSpeed, BeamDriftSpeed);
            Vector2 fall = new Vector2(0f, Main.rand.NextFloat(0.3f, 0.8f));
            Vector2 vel = inertia + drift + fall;

            float scale = Main.rand.NextFloat(0.5f, 1.3f) * BeamSize;
            float length = Main.rand.NextFloat(0.7f, 1.4f) * BeamLength;
            float swaySpeed = BeamSwaySpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float swayAmp = BeamSwayAmp * Main.rand.NextFloat(0.6f, 1.4f);
            Color color = BeamColor * Main.rand.NextFloat(0.6f, 1f);

            beams.Add(new MoonBeamParticle(pos, vel, BeamLife, scale, length, swaySpeed, swayAmp, color));
        }

        private void SpawnTide(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(5f, 5f);

            Vector2 drift = Main.rand.NextVector2Circular(TideDriftSpeed, TideDriftSpeed);
            float startSize = TideStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = TideEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            float expandSpeed = TideExpandSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float waveSpeed = TideWaveSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float waveAmp = TideWaveAmp * Main.rand.NextFloat(0.6f, 1.4f);
            Color color = TideColor * Main.rand.NextFloat(0.5f, 1f);

            tides.Add(new MoonTideParticle(pos, drift, TideLife, startSize, endSize, expandSpeed, waveSpeed, waveAmp, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (tides.Count > 0 && _tideTex != null)
            {
                Vector2 tideOrigin = _tideTex.Size() * 0.5f;
                var sortedTides = tides.OrderBy(t => t.Life);
                foreach (var t in sortedTides)
                {
                    Color drawColor = t.Color * t.Alpha;
                    Vector2 pos = t.Position - Main.screenPosition;
                    sb.Draw(_tideTex, pos, null, drawColor, t.Rotation,
                        tideOrigin, t.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (beams.Count > 0 && _beamTex != null)
            {
                Vector2 beamOrigin = new Vector2(_beamTex.Width * 0.5f, 0f);
                var sortedBeams = beams.OrderBy(b => b.Life);
                foreach (var b in sortedBeams)
                {
                    Color drawColor = b.Color * b.Alpha;
                    Vector2 pos = b.Position - Main.screenPosition;
                    float angle = MathHelper.PiOver2 + MathF.Sin(b.SwayPhase) * b.SwayAmplitude * 0.5f;
                    Vector2 scale = new Vector2(b.CurrentWidth, b.CurrentLength / _beamTex.Height);
                    sb.Draw(_beamTex, pos, null, drawColor, angle,
                        beamOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (crescents.Count > 0 && _crescentTex != null)
            {
                Vector2 crescentOrigin = _crescentTex.Size() * 0.5f;
                var sortedCrescents = crescents.OrderBy(c => c.Life);
                foreach (var c in sortedCrescents)
                {
                    Color drawColor = c.Color * c.Alpha;
                    Vector2 pos = c.Position - Main.screenPosition;
                    float rotation = c.Angle + MathHelper.PiOver2;
                    sb.Draw(_crescentTex, pos, null, drawColor, rotation,
                        crescentOrigin, c.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            crescents.Clear();
            beams.Clear();
            tides.Clear();
            beamCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
