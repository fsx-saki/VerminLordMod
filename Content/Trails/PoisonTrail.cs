using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class PoisonTrail : ITrail
    {
        public class SporeBubbleParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float ExpandSpeed;
            public float WobblePhase;
            public float WobbleSpeed;
            public bool HasBurst;
            public int BurstCount;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    if (HasBurst)
                    {
                        float fadeOut = 1f - Progress;
                        return MathF.Max(0f, fadeOut * fadeOut * 0.3f);
                    }
                    float fadeIn = MathF.Min(1f, Progress * 3f);
                    float fadeOut2 = (1f - Progress) * (1f - Progress);
                    float thin = Progress > 0.7f ? (1f - (Progress - 0.7f) / 0.3f) * 0.6f + 0.4f : 1f;
                    return MathF.Max(0f, fadeIn * fadeOut2 * thin * 0.5f);
                }
            }

            public float CurrentScale
            {
                get
                {
                    if (HasBurst) return Scale * (1f + Progress * 2f);
                    float expand = MathF.Min(1f, Progress * ExpandSpeed);
                    return Scale + (MaxScale - Scale) * expand;
                }
            }

            public SporeBubbleParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float expandSpeed, int burstCount, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                ExpandSpeed = expandSpeed;
                WobblePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                WobbleSpeed = Main.rand.NextFloat(0.02f, 0.06f);
                HasBurst = false;
                BurstCount = burstCount;
                Color = color;
            }
        }

        public class CorrosionDripParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Gravity;
            public bool HasLanded;
            public float CorrodeRadius;
            public float CorrodeSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    if (HasLanded)
                    {
                        float grow = MathF.Min(1f, (Progress - 0.4f) * 3f);
                        float fadeOut = 1f - Progress;
                        return MathF.Max(0f, grow * fadeOut * 0.5f);
                    }
                    float fadeIn = MathF.Min(1f, Progress * 6f);
                    float fadeOut2 = 1f - Progress * 0.3f;
                    return MathF.Max(0f, fadeIn * fadeOut2 * 0.7f);
                }
            }

            public float CurrentScale
            {
                get
                {
                    if (HasLanded)
                    {
                        float grow = MathF.Min(1f, (Progress - 0.4f) * CorrodeSpeed);
                        return Scale * (1f + grow * CorrodeRadius);
                    }
                    return Scale * (1f - Progress * 0.2f);
                }
            }

            public CorrosionDripParticle(Vector2 pos, Vector2 vel, int life, float scale, float gravity, float corrodeRadius, float corrodeSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Gravity = gravity;
                HasLanded = false;
                CorrodeRadius = corrodeRadius;
                CorrodeSpeed = corrodeSpeed;
                Color = color;
            }
        }

        public class MiasmaCloudParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float ExpandSpeed;
            public float SinkSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 2f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.35f);
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

            public MiasmaCloudParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float expandSpeed, float rotSpeed, float sinkSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                ExpandSpeed = expandSpeed;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                SinkSpeed = sinkSpeed;
                Color = color;
            }
        }

        public string Name { get; set; } = "PoisonTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(80, 180, 60, 140);

        public int MaxSporeBubbles { get; set; } = 12;
        public int SporeBubbleLife { get; set; } = 50;
        public float SporeBubbleStartSize { get; set; } = 0.3f;
        public float SporeBubbleEndSize { get; set; } = 1.2f;
        public float SporeBubbleSpawnChance { get; set; } = 0.03f;
        public float SporeBubbleExpandSpeed { get; set; } = 1.5f;
        public float SporeBubbleDriftSpeed { get; set; } = 0.06f;
        public int SporeBubbleBurstCount { get; set; } = 3;
        public Color SporeBubbleColor { get; set; } = new Color(100, 200, 60, 200);

        public int MaxCorrosionDrips { get; set; } = 15;
        public int CorrosionDripLife { get; set; } = 40;
        public float CorrosionDripSize { get; set; } = 0.4f;
        public float CorrosionDripSpawnChance { get; set; } = 0.06f;
        public float CorrosionDripGravity { get; set; } = 0.18f;
        public float CorrosionDripSpread { get; set; } = 4f;
        public float CorrosionDripCorrodeRadius { get; set; } = 2f;
        public float CorrosionDripCorrodeSpeed { get; set; } = 3f;
        public Color CorrosionDripColor { get; set; } = new Color(120, 220, 50, 220);

        public int MaxMiasmaClouds { get; set; } = 8;
        public int MiasmaCloudLife { get; set; } = 60;
        public float MiasmaCloudStartSize { get; set; } = 0.4f;
        public float MiasmaCloudEndSize { get; set; } = 2.0f;
        public float MiasmaCloudSpawnChance { get; set; } = 0.02f;
        public float MiasmaCloudExpandSpeed { get; set; } = 1.2f;
        public float MiasmaCloudRotSpeed { get; set; } = 0.015f;
        public float MiasmaCloudSinkSpeed { get; set; } = 0.05f;
        public float MiasmaCloudDriftSpeed { get; set; } = 0.1f;
        public Color MiasmaCloudColor { get; set; } = new Color(60, 160, 40, 160);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<SporeBubbleParticle> sporeBubbles = new();
        private List<CorrosionDripParticle> corrosionDrips = new();
        private List<MiasmaCloudParticle> miasmaClouds = new();

        private GhostTrail _ghostTrail;

        private Texture2D _sporeBubbleTex;
        private Texture2D _corrosionDripTex;
        private Texture2D _miasmaCloudTex;
        private Texture2D _ghostTex;

        private Vector2 _lastCenter;

        public bool HasContent => sporeBubbles.Count > 0 || corrosionDrips.Count > 0 || miasmaClouds.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_sporeBubbleTex != null) return;
            _sporeBubbleTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/PoisonTrail/PoisonTrailSpore").Value;
            _corrosionDripTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/PoisonTrail/PoisonTrailDrip").Value;
            _miasmaCloudTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/PoisonTrail/PoisonTrailMiasma").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/PoisonTrail/PoisonTrailGhost").Value;
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

            if (sporeBubbles.Count < MaxSporeBubbles && Main.rand.NextFloat() < SporeBubbleSpawnChance)
                SpawnSporeBubble(center, velocity, moveDir);

            if (corrosionDrips.Count < MaxCorrosionDrips && Main.rand.NextFloat() < CorrosionDripSpawnChance)
                SpawnCorrosionDrip(center, velocity, moveDir);

            if (miasmaClouds.Count < MaxMiasmaClouds && Main.rand.NextFloat() < MiasmaCloudSpawnChance)
                SpawnMiasmaCloud(center, velocity, moveDir);

            for (int i = sporeBubbles.Count - 1; i >= 0; i--)
            {
                var s = sporeBubbles[i];
                s.WobblePhase += s.WobbleSpeed;
                s.Velocity *= 0.98f;
                s.Velocity.Y -= 0.005f;
                s.Position += s.Velocity;

                if (!s.HasBurst && s.Progress > 0.7f)
                {
                    s.HasBurst = true;
                    s.Velocity = Vector2.Zero;
                }

                s.Life--;
                if (s.Life <= 0) sporeBubbles.RemoveAt(i);
            }

            for (int i = corrosionDrips.Count - 1; i >= 0; i--)
            {
                var d = corrosionDrips[i];
                if (!d.HasLanded)
                {
                    d.Velocity.Y += d.Gravity;
                    d.Velocity.X *= 0.99f;
                    d.Position += d.Velocity;

                    if (d.Progress > 0.5f && !d.HasLanded)
                    {
                        d.HasLanded = true;
                        d.Velocity = Vector2.Zero;
                    }
                }
                else
                {
                    d.Position += d.Velocity;
                }
                d.Life--;
                if (d.Life <= 0) corrosionDrips.RemoveAt(i);
            }

            for (int i = miasmaClouds.Count - 1; i >= 0; i--)
            {
                var m = miasmaClouds[i];
                m.Rotation += m.RotSpeed;
                m.Velocity *= 0.98f;
                m.Velocity.Y += m.SinkSpeed * 0.1f;
                m.Position += m.Velocity;
                m.Life--;
                if (m.Life <= 0) miasmaClouds.RemoveAt(i);
            }
        }

        private void SpawnSporeBubble(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(6f, 6f);
            Vector2 drift = Main.rand.NextVector2Circular(SporeBubbleDriftSpeed, SporeBubbleDriftSpeed);
            Vector2 vel = drift + new Vector2(0f, -0.08f);
            float startSize = SporeBubbleStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = SporeBubbleEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            float expandSpeed = SporeBubbleExpandSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            int burstCount = Main.rand.Next(2, SporeBubbleBurstCount + 1);
            Color color = SporeBubbleColor * Main.rand.NextFloat(0.5f, 1f);
            sporeBubbles.Add(new SporeBubbleParticle(pos, vel, SporeBubbleLife, startSize, endSize, expandSpeed, burstCount, color));
        }

        private void SpawnCorrosionDrip(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-CorrosionDripSpread, CorrosionDripSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);
            Vector2 inertia = -velocity * InertiaFactor * 0.2f;
            Vector2 drift = Main.rand.NextVector2Circular(0.3f, 0.3f);
            Vector2 vel = inertia + drift + new Vector2(0f, Main.rand.NextFloat(0.5f, 1.5f));
            float scale = CorrosionDripSize * Main.rand.NextFloat(0.5f, 1.5f);
            float corrodeRadius = CorrosionDripCorrodeRadius * Main.rand.NextFloat(0.7f, 1.3f);
            float corrodeSpeed = CorrosionDripCorrodeSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = CorrosionDripColor * Main.rand.NextFloat(0.5f, 1f);
            corrosionDrips.Add(new CorrosionDripParticle(pos, vel, CorrosionDripLife, scale, CorrosionDripGravity, corrodeRadius, corrodeSpeed, color));
        }

        private void SpawnMiasmaCloud(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(10f, 10f);
            Vector2 drift = Main.rand.NextVector2Circular(MiasmaCloudDriftSpeed, MiasmaCloudDriftSpeed);
            Vector2 vel = drift;
            float startSize = MiasmaCloudStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = MiasmaCloudEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            float expandSpeed = MiasmaCloudExpandSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float rotSpeed = MiasmaCloudRotSpeed * Main.rand.NextFloat(0.5f, 1.5f) * (Main.rand.NextBool() ? 1f : -1f);
            float sinkSpeed = MiasmaCloudSinkSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = MiasmaCloudColor * Main.rand.NextFloat(0.5f, 1f);
            miasmaClouds.Add(new MiasmaCloudParticle(pos, vel, MiasmaCloudLife, startSize, endSize, expandSpeed, rotSpeed, sinkSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (miasmaClouds.Count > 0 && _miasmaCloudTex != null)
            {
                Vector2 cloudOrigin = _miasmaCloudTex.Size() * 0.5f;
                var sortedClouds = miasmaClouds.OrderBy(m => m.Life);
                foreach (var m in sortedClouds)
                {
                    Color drawColor = m.Color * m.Alpha;
                    Vector2 pos = m.Position - Main.screenPosition;
                    sb.Draw(_miasmaCloudTex, pos, null, drawColor, m.Rotation,
                        cloudOrigin, m.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (sporeBubbles.Count > 0 && _sporeBubbleTex != null)
            {
                Vector2 sporeOrigin = _sporeBubbleTex.Size() * 0.5f;
                var sortedSpores = sporeBubbles.OrderBy(s => s.Life);
                foreach (var s in sortedSpores)
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    float wobble = 1f + MathF.Sin(s.WobblePhase) * 0.08f;
                    sb.Draw(_sporeBubbleTex, pos, null, drawColor, s.WobblePhase * 0.3f,
                        sporeOrigin, s.CurrentScale * wobble, SpriteEffects.None, 0);
                }
            }

            if (corrosionDrips.Count > 0 && _corrosionDripTex != null)
            {
                Vector2 dripOrigin = new Vector2(_corrosionDripTex.Width * 0.5f, _corrosionDripTex.Height * 0.5f);
                var sortedDrips = corrosionDrips.OrderBy(d => d.Life);
                foreach (var d in sortedDrips)
                {
                    Color drawColor = d.Color * d.Alpha;
                    Vector2 pos = d.Position - Main.screenPosition;
                    if (d.HasLanded)
                    {
                        sb.Draw(_corrosionDripTex, pos, null, drawColor, 0f,
                            dripOrigin, new Vector2(d.CurrentScale * 1.5f, d.CurrentScale * 0.5f), SpriteEffects.None, 0);
                    }
                    else
                    {
                        float angle = d.Velocity.Length() > 0.1f ? d.Velocity.ToRotation() : 0f;
                        Vector2 scale = new Vector2(d.CurrentScale * 1.3f, d.CurrentScale * 0.7f);
                        sb.Draw(_corrosionDripTex, pos, null, drawColor, angle,
                            dripOrigin, scale, SpriteEffects.None, 0);
                    }
                }
            }
        }

        public void Clear()
        {
            sporeBubbles.Clear();
            corrosionDrips.Clear();
            miasmaClouds.Clear();
            _ghostTrail?.Clear();
        }
    }
}
