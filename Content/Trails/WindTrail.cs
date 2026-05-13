using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class WindTrail : ITrail
    {
        public class WindStreakParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Stretch;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha => MathF.Max(0f, 1f - Progress * Progress);

            public float CurrentScale => Scale * (1f - Progress * 0.5f);

            public float CurrentStretch => Stretch * (1f - Progress * 0.3f);

            public WindStreakParticle(Vector2 pos, Vector2 vel, int life, float scale, float stretch, float rotation, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Stretch = stretch;
                Rotation = rotation;
                Color = color;
            }
        }

        public class WindVortexParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float ExpandRate;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = 1f - Progress;
                    return MathF.Max(0f, fadeIn * fadeOut);
                }
            }

            public float CurrentScale => Scale * (1f + Progress * ExpandRate);

            public WindVortexParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotSpeed, float expandRate, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                RotSpeed = rotSpeed;
                ExpandRate = expandRate;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public class WindMistParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = 1f - Progress * Progress;
                    return MathF.Max(0f, fadeIn * fadeOut * 0.5f);
                }
            }

            public float CurrentScale
            {
                get
                {
                    float expand = MathF.Min(1f, Progress * 3f);
                    return Scale + (MaxScale - Scale) * expand;
                }
            }

            public WindMistParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public string Name { get; set; } = "WindTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;

        public int GhostMaxPositions { get; set; } = 10;

        public int GhostRecordInterval { get; set; } = 2;

        public float GhostWidthScale { get; set; } = 0.25f;

        public float GhostLengthScale { get; set; } = 1.8f;

        public float GhostAlpha { get; set; } = 0.5f;

        public Color GhostColor { get; set; } = new Color(160, 240, 220, 180);

        public int MaxStreaks { get; set; } = 50;

        public int StreakLife { get; set; } = 18;

        public float StreakSize { get; set; } = 0.5f;

        public float StreakStretch { get; set; } = 2.5f;

        public int StreakSpawnInterval { get; set; } = 1;

        public float StreakInertia { get; set; } = 0.15f;

        public float StreakSpread { get; set; } = 3f;

        public float StreakDrift { get; set; } = 0.3f;

        public Color StreakColor { get; set; } = new Color(180, 245, 230, 200);

        public int MaxVortex { get; set; } = 30;

        public int VortexLife { get; set; } = 35;

        public float VortexSize { get; set; } = 0.35f;

        public int VortexSpawnInterval { get; set; } = 4;

        public float VortexRotSpeed { get; set; } = 0.08f;

        public float VortexExpandRate { get; set; } = 1.5f;

        public float VortexDriftSpeed { get; set; } = 0.5f;

        public Color VortexColor { get; set; } = new Color(140, 220, 200, 180);

        public int MaxMist { get; set; } = 15;

        public int MistLife { get; set; } = 40;

        public float MistStartSize { get; set; } = 0.3f;

        public float MistEndSize { get; set; } = 1.8f;

        public float MistSpawnChance { get; set; } = 0.12f;

        public float MistDriftSpeed { get; set; } = 0.3f;

        public Color MistColor { get; set; } = new Color(180, 240, 230, 100);

        public float InertiaFactor { get; set; } = 0.2f;

        public float RandomSpread { get; set; } = 4f;

        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AdaptiveDensity { get; set; } = true;

        public float AdaptiveSpeedThreshold { get; set; } = 5f;

        public float AdaptiveDensityFactor { get; set; } = 5f;

        private List<WindStreakParticle> streaks = new();
        private List<WindVortexParticle> vortexes = new();
        private List<WindMistParticle> mists = new();
        private int streakCounter = 0;
        private int vortexCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _streakTex;
        private Texture2D _vortexTex;
        private Texture2D _mistTex;
        private Texture2D _ghostTex;

        public bool HasContent => streaks.Count > 0 || vortexes.Count > 0 || mists.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_streakTex != null) return;
            _streakTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WindTrail/WindTrailStreak").Value;
            _vortexTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WindTrail/WindTrailVortex").Value;
            _mistTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WindTrail/WindTrailMist").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WindTrail/WindTrailGhost").Value;
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

            float speed = velocity.Length();
            Vector2 moveDir = velocity.SafeNormalize(Vector2.UnitX);

            streakCounter++;
            int effectiveStreakInterval = StreakSpawnInterval;
            if (AdaptiveDensity && speed > AdaptiveSpeedThreshold)
                effectiveStreakInterval = 1;

            if (streakCounter >= effectiveStreakInterval && streaks.Count < MaxStreaks)
            {
                streakCounter = 0;
                int count = 1;
                if (AdaptiveDensity && speed > AdaptiveSpeedThreshold)
                {
                    count = Math.Clamp((int)(speed / AdaptiveDensityFactor), 1, 3);
                }
                for (int i = 0; i < count; i++)
                    SpawnStreak(center, velocity, moveDir);
            }

            vortexCounter++;
            if (vortexCounter >= VortexSpawnInterval && vortexes.Count < MaxVortex)
            {
                vortexCounter = 0;
                SpawnVortex(center, velocity, moveDir);
            }

            if (mists.Count < MaxMist && Main.rand.NextFloat() < MistSpawnChance)
            {
                SpawnMist(center, velocity, moveDir);
            }

            for (int i = streaks.Count - 1; i >= 0; i--)
            {
                var s = streaks[i];

                Vector2 drift = moveDir * StreakDrift;
                Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
                float wobble = MathF.Sin(s.Progress * MathHelper.Pi * 2f + s.Rotation) * 0.3f;
                s.Velocity = s.Velocity * 0.92f + drift + perpDir * wobble;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0)
                    streaks.RemoveAt(i);
            }

            for (int i = vortexes.Count - 1; i >= 0; i--)
            {
                var v = vortexes[i];
                v.Rotation += v.RotSpeed;
                v.Velocity *= 0.95f;
                v.Position += v.Velocity;
                v.Life--;
                if (v.Life <= 0)
                    vortexes.RemoveAt(i);
            }

            for (int i = mists.Count - 1; i >= 0; i--)
            {
                var m = mists[i];
                m.Velocity *= 0.97f;
                m.Position += m.Velocity;
                m.Life--;
                if (m.Life <= 0)
                    mists.RemoveAt(i);
            }
        }

        private void SpawnStreak(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 inertia = -velocity * StreakInertia;
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-1f, 1f) * StreakSpread;
            Vector2 random = Main.rand.NextVector2Circular(1.5f, 1.5f);
            Vector2 vel = inertia + perpDir * sideOffset * 0.3f + random;

            float scale = Main.rand.NextFloat(0.5f, 1.2f) * StreakSize;
            float stretch = Main.rand.NextFloat(0.8f, 1.5f) * StreakStretch;
            float rotation = velocity.ToRotation();
            Color color = StreakColor * Main.rand.NextFloat(0.6f, 1f);

            Vector2 pos = center + SpawnOffset + perpDir * sideOffset;
            streaks.Add(new WindStreakParticle(pos, vel, StreakLife, scale, stretch, rotation, color));
        }

        private void SpawnVortex(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-8f, 8f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(6f, 6f);

            Vector2 drift = Main.rand.NextVector2Circular(VortexDriftSpeed, VortexDriftSpeed);
            float scale = Main.rand.NextFloat(0.6f, 1.2f) * VortexSize;
            float rotSpeed = Main.rand.NextFloat(-VortexRotSpeed, VortexRotSpeed);
            if (Main.rand.NextBool())
                rotSpeed = -rotSpeed;
            float expandRate = VortexExpandRate * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = VortexColor * Main.rand.NextFloat(0.5f, 1f);

            vortexes.Add(new WindVortexParticle(pos, drift, VortexLife, scale, rotSpeed, expandRate, color));
        }

        private void SpawnMist(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-12f, 12f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(10f, 10f);

            Vector2 drift = Main.rand.NextVector2Circular(MistDriftSpeed, MistDriftSpeed);
            float startSize = MistStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = MistEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = MistColor * Main.rand.NextFloat(0.4f, 0.8f);

            mists.Add(new WindMistParticle(pos, drift, MistLife, startSize, endSize, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (_mistTex != null && mists.Count > 0)
            {
                Vector2 mistOrigin = _mistTex.Size() * 0.5f;
                var sortedMists = mists.OrderBy(m => m.Life);
                foreach (var m in sortedMists)
                {
                    Color drawColor = m.Color * m.Alpha;
                    Vector2 pos = m.Position - Main.screenPosition;
                    sb.Draw(_mistTex, pos, null, drawColor, m.Rotation,
                        mistOrigin, m.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (_streakTex != null && streaks.Count > 0)
            {
                Vector2 streakOrigin = _streakTex.Size() * 0.5f;
                var sortedStreaks = streaks.OrderBy(s => s.Life);
                foreach (var s in sortedStreaks)
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(s.CurrentStretch, s.CurrentScale);
                    sb.Draw(_streakTex, pos, null, drawColor, s.Rotation,
                        streakOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (_vortexTex != null && vortexes.Count > 0)
            {
                Vector2 vortexOrigin = _vortexTex.Size() * 0.5f;
                var sortedVortexes = vortexes.OrderBy(v => v.Life);
                foreach (var v in sortedVortexes)
                {
                    Color drawColor = v.Color * v.Alpha;
                    Vector2 pos = v.Position - Main.screenPosition;
                    sb.Draw(_vortexTex, pos, null, drawColor, v.Rotation,
                        vortexOrigin, v.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            streaks.Clear();
            vortexes.Clear();
            mists.Clear();
            streakCounter = 0;
            vortexCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
