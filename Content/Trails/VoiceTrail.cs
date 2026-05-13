using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class VoiceTrail : ITrail
    {
        public class SoundWaveParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float ExpandRate;
            public float RingPhase;
            public float RingSpeed;
            public float Thickness;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float ring = MathF.Min(1f, Progress * 5f);
                    float decay = 1f - MathF.Pow(Progress, 2f);
                    float pulse = 0.6f + 0.4f * MathF.Sin(RingPhase);
                    return MathF.Max(0f, ring * decay * pulse * 0.6f);
                }
            }

            public float CurrentScale => Scale * (1f + Progress * ExpandRate);

            public float CurrentThickness => Thickness * (1f - Progress * 0.5f);

            public SoundWaveParticle(Vector2 pos, Vector2 vel, int life, float scale, float expandRate, float ringSpeed, float thickness, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                ExpandRate = expandRate;
                RingPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                RingSpeed = ringSpeed;
                Thickness = thickness;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public class MusicNoteParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float FloatPhase;
            public float FloatSpeed;
            public float FloatAmplitude;
            public float BobPhase;
            public int NoteIndex;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float appear = MathF.Min(1f, Progress * 3f);
                    float vanish = 1f - MathF.Pow(Progress, 2.5f);
                    float bob = 0.7f + 0.3f * MathF.Sin(BobPhase);
                    return MathF.Max(0f, appear * vanish * bob);
                }
            }

            public MusicNoteParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotation, float floatSpeed, float floatAmplitude, int noteIndex, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = rotation;
                FloatPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                FloatSpeed = floatSpeed;
                FloatAmplitude = floatAmplitude;
                BobPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                NoteIndex = noteIndex;
                Color = color;
            }
        }

        public class ResonanceParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float VibratePhase;
            public float VibrateSpeed;
            public float VibrateAmplitude;
            public float PulsePhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float resonate = MathF.Min(1f, Progress * 5f);
                    float dampen = 1f - MathF.Pow(Progress, 3f);
                    float vibrate = 0.5f + 0.5f * MathF.Sin(VibratePhase);
                    return MathF.Max(0f, resonate * dampen * vibrate);
                }
            }

            public float CurrentScale => Scale * (1f + Progress * 0.5f) * (0.8f + 0.2f * MathF.Sin(PulsePhase));

            public ResonanceParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotation, float vibrateSpeed, float vibrateAmplitude, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = rotation;
                VibratePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                VibrateSpeed = vibrateSpeed;
                VibrateAmplitude = vibrateAmplitude;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public string Name { get; set; } = "VoiceTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.15f;
        public float GhostLengthScale { get; set; } = 1.3f;
        public float GhostAlpha { get; set; } = 0.25f;
        public Color GhostColor { get; set; } = new Color(180, 160, 220, 120);

        public int MaxSoundWaves { get; set; } = 12;
        public int SoundWaveLife { get; set; } = 22;
        public float SoundWaveSize { get; set; } = 0.5f;
        public float SoundWaveExpandRate { get; set; } = 3f;
        public int SoundWaveSpawnInterval { get; set; } = 4;
        public float SoundWaveRingSpeed { get; set; } = 0.12f;
        public float SoundWaveThickness { get; set; } = 0.3f;
        public float SoundWaveSpread { get; set; } = 4f;
        public Color SoundWaveColor { get; set; } = new Color(200, 180, 240, 200);

        public int MaxMusicNotes { get; set; } = 10;
        public int MusicNoteLife { get; set; } = 35;
        public float MusicNoteSize { get; set; } = 0.45f;
        public float MusicNoteSpawnChance { get; set; } = 0.05f;
        public float MusicNoteFloatSpeed { get; set; } = 0.04f;
        public float MusicNoteFloatAmplitude { get; set; } = 3f;
        public float MusicNoteRiseSpeed { get; set; } = 0.3f;
        public float MusicNoteSpread { get; set; } = 5f;
        public Color MusicNoteColor { get; set; } = new Color(220, 200, 255, 220);

        public int MaxResonances { get; set; } = 15;
        public int ResonanceLife { get; set; } = 18;
        public float ResonanceSize { get; set; } = 0.35f;
        public float ResonanceSpawnChance { get; set; } = 0.1f;
        public float ResonanceVibrateSpeed { get; set; } = 0.15f;
        public float ResonanceVibrateAmplitude { get; set; } = 1.5f;
        public float ResonanceSpeed { get; set; } = 1.5f;
        public float ResonanceSpread { get; set; } = 3f;
        public Color ResonanceColor { get; set; } = new Color(160, 140, 220, 230);

        public float InertiaFactor { get; set; } = 0.12f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<SoundWaveParticle> soundWaves = new();
        private List<MusicNoteParticle> musicNotes = new();
        private List<ResonanceParticle> resonances = new();
        private int waveCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _waveTex;
        private Texture2D _noteTex;
        private Texture2D _resonanceTex;
        private Texture2D _ghostTex;

        public bool HasContent => soundWaves.Count > 0 || musicNotes.Count > 0 || resonances.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_waveTex != null) return;
            _waveTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/VoiceTrail/VoiceTrailWave").Value;
            _noteTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/VoiceTrail/VoiceTrailNote").Value;
            _resonanceTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/VoiceTrail/VoiceTrailResonance").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/VoiceTrail/VoiceTrailGhost").Value;
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

            waveCounter++;
            if (waveCounter >= SoundWaveSpawnInterval && soundWaves.Count < MaxSoundWaves)
            {
                waveCounter = 0;
                SpawnSoundWave(center, velocity, moveDir);
            }

            if (musicNotes.Count < MaxMusicNotes && Main.rand.NextFloat() < MusicNoteSpawnChance)
                SpawnMusicNote(center, velocity, moveDir);

            if (resonances.Count < MaxResonances && Main.rand.NextFloat() < ResonanceSpawnChance)
                SpawnResonance(center, velocity, moveDir);

            for (int i = soundWaves.Count - 1; i >= 0; i--)
            {
                var w = soundWaves[i];
                w.RingPhase += w.RingSpeed;
                w.Velocity *= 0.95f;
                w.Position += w.Velocity;
                w.Life--;
                if (w.Life <= 0) soundWaves.RemoveAt(i);
            }

            for (int i = musicNotes.Count - 1; i >= 0; i--)
            {
                var n = musicNotes[i];
                n.FloatPhase += n.FloatSpeed;
                n.BobPhase += 0.05f;
                Vector2 floatOffset = new Vector2(
                    MathF.Sin(n.FloatPhase) * n.FloatAmplitude * 0.04f,
                    MathF.Cos(n.FloatPhase * 0.7f) * n.FloatAmplitude * 0.03f - 0.1f
                );
                n.Velocity *= 0.97f;
                n.Position += n.Velocity + floatOffset;
                n.Life--;
                if (n.Life <= 0) musicNotes.RemoveAt(i);
            }

            for (int i = resonances.Count - 1; i >= 0; i--)
            {
                var r = resonances[i];
                r.VibratePhase += r.VibrateSpeed;
                r.PulsePhase += 0.1f;
                Vector2 vibrateOffset = new Vector2(
                    MathF.Sin(r.VibratePhase) * r.VibrateAmplitude * 0.05f,
                    MathF.Cos(r.VibratePhase) * r.VibrateAmplitude * 0.05f
                );
                r.Velocity *= 0.93f;
                r.Position += r.Velocity + vibrateOffset;
                r.Life--;
                if (r.Life <= 0) resonances.RemoveAt(i);
            }
        }

        private void SpawnSoundWave(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-SoundWaveSpread, SoundWaveSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            Vector2 drift = Main.rand.NextVector2Circular(0.4f, 0.4f);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.5f, 1.3f) * SoundWaveSize;
            float expandRate = SoundWaveExpandRate * Main.rand.NextFloat(0.7f, 1.3f);
            float ringSpeed = SoundWaveRingSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float thickness = SoundWaveThickness * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = SoundWaveColor * Main.rand.NextFloat(0.5f, 1f);

            soundWaves.Add(new SoundWaveParticle(pos, vel, SoundWaveLife, scale, expandRate, ringSpeed, thickness, color));
        }

        private void SpawnMusicNote(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-MusicNoteSpread, MusicNoteSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.2f;
            Vector2 rise = new Vector2(0f, -Main.rand.NextFloat(0.2f, MusicNoteRiseSpeed));
            Vector2 drift = Main.rand.NextVector2Circular(0.2f, 0.2f);
            Vector2 vel = inertia + rise + drift;

            float scale = Main.rand.NextFloat(0.6f, 1.3f) * MusicNoteSize;
            float rotation = Main.rand.NextFloat(-0.2f, 0.2f);
            float floatSpeed = MusicNoteFloatSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float floatAmplitude = MusicNoteFloatAmplitude * Main.rand.NextFloat(0.6f, 1.4f);
            int noteIndex = Main.rand.Next(3);
            Color color = MusicNoteColor * Main.rand.NextFloat(0.6f, 1f);

            musicNotes.Add(new MusicNoteParticle(pos, vel, MusicNoteLife, scale, rotation, floatSpeed, floatAmplitude, noteIndex, color));
        }

        private void SpawnResonance(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-ResonanceSpread, ResonanceSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(1f, 1f);

            Vector2 inertia = -velocity * InertiaFactor * 0.4f;
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 burst = angle.ToRotationVector2() * Main.rand.NextFloat(0.3f, ResonanceSpeed);
            Vector2 vel = inertia + burst;

            float scale = Main.rand.NextFloat(0.5f, 1.4f) * ResonanceSize;
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            float vibrateSpeed = ResonanceVibrateSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float vibrateAmplitude = ResonanceVibrateAmplitude * Main.rand.NextFloat(0.6f, 1.4f);
            Color color = ResonanceColor * Main.rand.NextFloat(0.7f, 1f);

            resonances.Add(new ResonanceParticle(pos, vel, ResonanceLife, scale, rotation, vibrateSpeed, vibrateAmplitude, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (resonances.Count > 0 && _resonanceTex != null)
            {
                Vector2 resOrigin = new Vector2(_resonanceTex.Width * 0.5f, _resonanceTex.Height * 0.5f);
                var sorted = resonances.OrderBy(r => r.Life);
                foreach (var r in sorted)
                {
                    Color drawColor = r.Color * r.Alpha;
                    Vector2 pos = r.Position - Main.screenPosition;
                    sb.Draw(_resonanceTex, pos, null, drawColor, r.Rotation,
                        resOrigin, r.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (musicNotes.Count > 0 && _noteTex != null)
            {
                Vector2 noteOrigin = new Vector2(_noteTex.Width * 0.5f, _noteTex.Height * 0.5f);
                var sorted = musicNotes.OrderBy(n => n.Life);
                foreach (var n in sorted)
                {
                    Color drawColor = n.Color * n.Alpha;
                    Vector2 pos = n.Position - Main.screenPosition;
                    sb.Draw(_noteTex, pos, null, drawColor, n.Rotation,
                        noteOrigin, n.Scale, SpriteEffects.None, 0);
                }
            }

            if (soundWaves.Count > 0 && _waveTex != null)
            {
                Vector2 waveOrigin = new Vector2(_waveTex.Width * 0.5f, _waveTex.Height * 0.5f);
                var sorted = soundWaves.OrderBy(w => w.Life);
                foreach (var w in sorted)
                {
                    Color drawColor = w.Color * w.Alpha;
                    Vector2 pos = w.Position - Main.screenPosition;
                    float thickness = w.CurrentThickness;
                    Vector2 scale = new Vector2(w.CurrentScale, thickness);
                    sb.Draw(_waveTex, pos, null, drawColor, w.Rotation,
                        waveOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            soundWaves.Clear();
            musicNotes.Clear();
            resonances.Clear();
            waveCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}