using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class QiTrail : ITrail
    {
        public class ChiStreamParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Length;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float FlowPhase;
            public float FlowSpeed;
            public float FlowAmplitude;
            public float GrowSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float growIn = MathF.Min(1f, Progress * GrowSpeed);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, growIn * fadeOut * 0.6f);
                }
            }

            public float CurrentLength => Length * MathF.Min(1f, Progress * GrowSpeed) * MathF.Max(0f, 1f - Progress * 0.3f);

            public float CurrentFlow => MathF.Sin(FlowPhase) * FlowAmplitude;

            public ChiStreamParticle(Vector2 pos, Vector2 vel, int life, float scale, float length, float rotation, float flowSpeed, float flowAmplitude, float growSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Length = length;
                Rotation = rotation;
                FlowPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                FlowSpeed = flowSpeed;
                FlowAmplitude = flowAmplitude;
                GrowSpeed = growSpeed;
                Color = color;
            }
        }

        public class AcupointParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float PulsePhase;
            public float PulseSpeed;
            public float OrbitAngle;
            public float OrbitSpeed;
            public float OrbitRadius;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float pulse = 0.5f + 0.5f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, fadeIn * fadeOut * pulse);
                }
            }

            public Vector2 OrbitOffset => new Vector2(MathF.Cos(OrbitAngle), MathF.Sin(OrbitAngle)) * OrbitRadius * (1f - Progress * 0.5f);

            public AcupointParticle(Vector2 pos, Vector2 vel, int life, float scale, float orbitSpeed, float orbitRadius, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                OrbitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                OrbitSpeed = orbitSpeed;
                OrbitRadius = orbitRadius;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = Main.rand.NextFloat(0.1f, 0.2f);
                Color = color;
            }
        }

        public class MeridianPulseParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float ExpandSpeed;
            public float PulsePhase;
            public float PulseSpeed;
            public float RingWidth;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float pulse = 0.6f + 0.4f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, fadeIn * fadeOut * pulse * 0.4f);
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

            public float CurrentRingWidth => RingWidth * (1f - Progress * 0.5f);

            public MeridianPulseParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float expandSpeed, float ringWidth, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                ExpandSpeed = expandSpeed;
                RingWidth = ringWidth;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = Main.rand.NextFloat(0.08f, 0.16f);
                Color = color;
            }
        }

        public string Name { get; set; } = "QiTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 12;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.25f;
        public float GhostLengthScale { get; set; } = 1.6f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(100, 200, 180, 140);

        public int MaxChiStreams { get; set; } = 12;
        public int ChiStreamLife { get; set; } = 35;
        public float ChiStreamSize { get; set; } = 0.3f;
        public float ChiStreamLength { get; set; } = 24f;
        public float ChiStreamSpawnChance { get; set; } = 0.08f;
        public float ChiStreamFlowSpeed { get; set; } = 0.15f;
        public float ChiStreamFlowAmplitude { get; set; } = 0.25f;
        public float ChiStreamGrowSpeed { get; set; } = 3f;
        public float ChiStreamDriftSpeed { get; set; } = 0.06f;
        public Color ChiStreamColor { get; set; } = new Color(80, 220, 180, 200);

        public int MaxAcupoints { get; set; } = 16;
        public int AcupointLife { get; set; } = 40;
        public float AcupointSize { get; set; } = 0.35f;
        public float AcupointSpawnChance { get; set; } = 0.12f;
        public float AcupointOrbitSpeed { get; set; } = 0.06f;
        public float AcupointOrbitRadius { get; set; } = 15f;
        public float AcupointDriftSpeed { get; set; } = 0.08f;
        public Color AcupointColor { get; set; } = new Color(120, 255, 200, 240);

        public int MaxMeridianPulses { get; set; } = 6;
        public int MeridianPulseLife { get; set; } = 45;
        public float MeridianPulseStartSize { get; set; } = 0.2f;
        public float MeridianPulseEndSize { get; set; } = 1.5f;
        public float MeridianPulseSpawnChance { get; set; } = 0.025f;
        public float MeridianPulseExpandSpeed { get; set; } = 2f;
        public float MeridianPulseRingWidth { get; set; } = 0.4f;
        public float MeridianPulseDriftSpeed { get; set; } = 0.04f;
        public Color MeridianPulseColor { get; set; } = new Color(60, 180, 160, 160);

        public float InertiaFactor { get; set; } = 0.12f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<ChiStreamParticle> chiStreams = new();
        private List<AcupointParticle> acupoints = new();
        private List<MeridianPulseParticle> meridianPulses = new();

        private GhostTrail _ghostTrail;

        private Texture2D _streamTex;
        private Texture2D _acupointTex;
        private Texture2D _pulseTex;
        private Texture2D _ghostTex;

        public bool HasContent => chiStreams.Count > 0 || acupoints.Count > 0 || meridianPulses.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_streamTex != null) return;
            _streamTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/QiTrail/QiTrailStream").Value;
            _acupointTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/QiTrail/QiTrailAcupoint").Value;
            _pulseTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/QiTrail/QiTrailPulse").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/QiTrail/QiTrailGhost").Value;
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

            if (chiStreams.Count < MaxChiStreams && Main.rand.NextFloat() < ChiStreamSpawnChance)
                SpawnChiStream(center, velocity, moveDir);

            if (acupoints.Count < MaxAcupoints && Main.rand.NextFloat() < AcupointSpawnChance)
                SpawnAcupoint(center, velocity, moveDir);

            if (meridianPulses.Count < MaxMeridianPulses && Main.rand.NextFloat() < MeridianPulseSpawnChance)
                SpawnMeridianPulse(center, velocity, moveDir);

            for (int i = chiStreams.Count - 1; i >= 0; i--)
            {
                var s = chiStreams[i];
                s.FlowPhase += s.FlowSpeed;
                s.Rotation += s.CurrentFlow * 0.03f;
                s.Velocity *= 0.97f;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0) chiStreams.RemoveAt(i);
            }

            for (int i = acupoints.Count - 1; i >= 0; i--)
            {
                var a = acupoints[i];
                a.PulsePhase += a.PulseSpeed;
                a.OrbitAngle += a.OrbitSpeed;
                a.Velocity *= 0.97f;
                a.Position += a.Velocity;
                a.Life--;
                if (a.Life <= 0) acupoints.RemoveAt(i);
            }

            for (int i = meridianPulses.Count - 1; i >= 0; i--)
            {
                var m = meridianPulses[i];
                m.PulsePhase += m.PulseSpeed;
                m.Velocity *= 0.98f;
                m.Position += m.Velocity;
                m.Life--;
                if (m.Life <= 0) meridianPulses.RemoveAt(i);
            }
        }

        private void SpawnChiStream(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-5f, 5f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 drift = Main.rand.NextVector2Circular(ChiStreamDriftSpeed, ChiStreamDriftSpeed);
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.3f, 0.3f);
            float length = ChiStreamLength * Main.rand.NextFloat(0.6f, 1.4f);
            float scale = ChiStreamSize * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = ChiStreamColor * Main.rand.NextFloat(0.6f, 1f);

            chiStreams.Add(new ChiStreamParticle(pos, drift, ChiStreamLife, scale, length, rotation, ChiStreamFlowSpeed, ChiStreamFlowAmplitude, ChiStreamGrowSpeed, color));
        }

        private void SpawnAcupoint(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-6f, 6f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(4f, 4f);

            Vector2 drift = Main.rand.NextVector2Circular(AcupointDriftSpeed, AcupointDriftSpeed);
            float scale = AcupointSize * Main.rand.NextFloat(0.7f, 1.3f);
            float orbitSpeed = AcupointOrbitSpeed * Main.rand.NextFloat(0.7f, 1.3f) * (Main.rand.NextBool() ? 1f : -1f);
            float orbitRadius = AcupointOrbitRadius * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = AcupointColor * Main.rand.NextFloat(0.6f, 1f);

            acupoints.Add(new AcupointParticle(pos, drift, AcupointLife, scale, orbitSpeed, orbitRadius, color));
        }

        private void SpawnMeridianPulse(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-4f, 4f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 drift = Main.rand.NextVector2Circular(MeridianPulseDriftSpeed, MeridianPulseDriftSpeed);
            float startSize = MeridianPulseStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = MeridianPulseEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = MeridianPulseColor * Main.rand.NextFloat(0.5f, 1f);

            meridianPulses.Add(new MeridianPulseParticle(pos, drift, MeridianPulseLife, startSize, endSize, MeridianPulseExpandSpeed, MeridianPulseRingWidth, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (meridianPulses.Count > 0 && _pulseTex != null)
            {
                Vector2 pulseOrigin = _pulseTex.Size() * 0.5f;
                var sorted = meridianPulses.OrderBy(m => m.Life);
                foreach (var m in sorted)
                {
                    Color drawColor = m.Color * m.Alpha;
                    Vector2 pos = m.Position - Main.screenPosition;
                    float ringScale = m.CurrentScale;
                    float widthScale = m.CurrentRingWidth;
                    sb.Draw(_pulseTex, pos, null, drawColor, 0f,
                        pulseOrigin, new Vector2(ringScale, ringScale), SpriteEffects.None, 0);
                }
            }

            if (chiStreams.Count > 0 && _streamTex != null)
            {
                Vector2 streamOrigin = new Vector2(0f, _streamTex.Height * 0.5f);
                var sorted = chiStreams.OrderBy(s => s.Life);
                foreach (var s in sorted)
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(s.CurrentLength / _streamTex.Width, s.Scale);
                    sb.Draw(_streamTex, pos, null, drawColor, s.Rotation,
                        streamOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (acupoints.Count > 0 && _acupointTex != null)
            {
                Vector2 acupointOrigin = _acupointTex.Size() * 0.5f;
                var sorted = acupoints.OrderBy(a => a.Life);
                foreach (var a in sorted)
                {
                    Color drawColor = a.Color * a.Alpha;
                    Vector2 drawPos = a.Position + a.OrbitOffset - Main.screenPosition;
                    sb.Draw(_acupointTex, drawPos, null, drawColor, a.OrbitAngle,
                        acupointOrigin, a.Scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            chiStreams.Clear();
            acupoints.Clear();
            meridianPulses.Clear();
            _ghostTrail?.Clear();
        }
    }
}
