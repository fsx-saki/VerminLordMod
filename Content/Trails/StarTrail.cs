using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class StarTrail : ITrail
    {
        public class StarPointParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float BaseScale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float TwinklePhase;
            public float TwinkleSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 6f);
                    float fadeOut = 1f - Progress * Progress;
                    float twinkle = 0.7f + 0.3f * MathF.Sin(TwinklePhase);
                    return MathF.Max(0f, fadeIn * fadeOut * twinkle);
                }
            }

            public float CurrentScale => BaseScale * (1f - Progress * 0.3f);

            public StarPointParticle(Vector2 pos, Vector2 vel, int life, float scale, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                BaseScale = scale;
                Scale = scale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                TwinklePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                TwinkleSpeed = Main.rand.NextFloat(0.08f, 0.2f);
                Color = color;
            }
        }

        public class NebulaGlowParticle
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
                    float fadeIn = MathF.Min(1f, Progress * 3f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.35f);
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

            public NebulaGlowParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, Color color)
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

        public class StardustParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float TwinklePhase;
            public float TwinkleSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = 1f - Progress;
                    float twinkle = 0.4f + 0.6f * MathF.Max(0, MathF.Sin(TwinklePhase));
                    return MathF.Max(0f, fadeIn * fadeOut * twinkle);
                }
            }

            public StardustParticle(Vector2 pos, Vector2 vel, int life, float scale, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                TwinklePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                TwinkleSpeed = Main.rand.NextFloat(0.1f, 0.25f);
                Color = color;
            }
        }

        public string Name { get; set; } = "StarTrail";

        public BlendState BlendMode => BlendState.Additive;

        // ===== GhostTrail =====
        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.4f;
        public Color GhostColor { get; set; } = new Color(180, 170, 230, 180);

        // ===== Star Points =====
        public int MaxStarPoints { get; set; } = 35;
        public int StarLife { get; set; } = 50;
        public float StarSize { get; set; } = 0.45f;
        public int StarSpawnInterval { get; set; } = 2;
        public float StarDriftSpeed { get; set; } = 0.4f;
        public float StarSpread { get; set; } = 6f;
        public Color StarColor { get; set; } = new Color(220, 215, 255, 230);

        // ===== Constellation Lines =====
        public float LineMaxDistance { get; set; } = 55f;
        public float LineBreakDistance { get; set; } = 80f;
        public float LineBaseAlpha { get; set; } = 0.25f;
        public Color LineColor { get; set; } = new Color(180, 175, 230, 200);

        // ===== Nebula Glow =====
        public int MaxNebula { get; set; } = 8;
        public int NebulaLife { get; set; } = 60;
        public float NebulaStartSize { get; set; } = 0.3f;
        public float NebulaEndSize { get; set; } = 2.0f;
        public float NebulaSpawnChance { get; set; } = 0.04f;
        public float NebulaDriftSpeed { get; set; } = 0.15f;
        public Color NebulaColor { get; set; } = new Color(130, 110, 200, 120);

        // ===== Stardust Shimmer =====
        public int MaxStardust { get; set; } = 40;
        public int StardustLife { get; set; } = 35;
        public float StardustSize { get; set; } = 0.2f;
        public float StardustSpawnChance { get; set; } = 0.3f;
        public float StardustDriftSpeed { get; set; } = 0.5f;
        public Color StardustColor { get; set; } = new Color(200, 195, 255, 180);

        // ===== Common =====
        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<StarPointParticle> starPoints = new();
        private List<NebulaGlowParticle> nebulae = new();
        private List<StardustParticle> stardusts = new();
        private int starCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _starTex;
        private Texture2D _ghostTex;
        private Texture2D _dustTex;
        private Texture2D _nebulaTex;
        private Texture2D _lineTex;

        public bool HasContent => starPoints.Count > 0 || nebulae.Count > 0 || stardusts.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_starTex != null) return;
            _starTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/StarTrail/StarTrailStar").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/StarTrail/StarTrailGhost").Value;
            _dustTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/StarTrail/StarTrailDust").Value;
            _nebulaTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/StarTrail/StarTrailNebula").Value;
            _lineTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/StarTrail/StarTrailLine").Value;
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

            starCounter++;
            if (starCounter >= StarSpawnInterval && starPoints.Count < MaxStarPoints)
            {
                starCounter = 0;
                SpawnStarPoint(center, velocity, moveDir);
            }

            if (nebulae.Count < MaxNebula && Main.rand.NextFloat() < NebulaSpawnChance)
            {
                SpawnNebula(center, velocity, moveDir);
            }

            if (stardusts.Count < MaxStardust && Main.rand.NextFloat() < StardustSpawnChance)
            {
                SpawnStardust(center, velocity, moveDir);
            }

            for (int i = starPoints.Count - 1; i >= 0; i--)
            {
                var s = starPoints[i];
                s.TwinklePhase += s.TwinkleSpeed;
                s.Velocity *= 0.97f;
                s.Position += s.Velocity;
                s.Rotation += 0.005f;
                s.Life--;
                if (s.Life <= 0)
                    starPoints.RemoveAt(i);
            }

            for (int i = nebulae.Count - 1; i >= 0; i--)
            {
                var n = nebulae[i];
                n.Velocity *= 0.98f;
                n.Position += n.Velocity;
                n.Life--;
                if (n.Life <= 0)
                    nebulae.RemoveAt(i);
            }

            for (int i = stardusts.Count - 1; i >= 0; i--)
            {
                var d = stardusts[i];
                d.TwinklePhase += d.TwinkleSpeed;
                d.Velocity *= 0.96f;
                d.Position += d.Velocity;
                d.Life--;
                if (d.Life <= 0)
                    stardusts.RemoveAt(i);
            }
        }

        private void SpawnStarPoint(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-StarSpread, StarSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 drift = Main.rand.NextVector2Circular(StarDriftSpeed, StarDriftSpeed);
            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            Vector2 vel = drift + inertia;

            float scale = Main.rand.NextFloat(0.6f, 1.3f) * StarSize;
            Color color = StarColor * Main.rand.NextFloat(0.7f, 1f);

            starPoints.Add(new StarPointParticle(pos, vel, StarLife, scale, color));
        }

        private void SpawnNebula(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-10f, 10f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(8f, 8f);

            Vector2 drift = Main.rand.NextVector2Circular(NebulaDriftSpeed, NebulaDriftSpeed);
            float startSize = NebulaStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = NebulaEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = NebulaColor * Main.rand.NextFloat(0.5f, 1f);

            nebulae.Add(new NebulaGlowParticle(pos, drift, NebulaLife, startSize, endSize, color));
        }

        private void SpawnStardust(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-8f, 8f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(5f, 5f);

            Vector2 drift = Main.rand.NextVector2Circular(StardustDriftSpeed, StardustDriftSpeed);
            float scale = Main.rand.NextFloat(0.5f, 1.2f) * StardustSize;
            Color color = StardustColor * Main.rand.NextFloat(0.5f, 1f);

            stardusts.Add(new StardustParticle(pos, drift, StardustLife, scale, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (nebulae.Count > 0 && _nebulaTex != null)
            {
                Vector2 nebOrigin = _nebulaTex.Size() * 0.5f;
                var sortedNebulae = nebulae.OrderBy(n => n.Life);
                foreach (var n in sortedNebulae)
                {
                    Color drawColor = n.Color * n.Alpha;
                    Vector2 pos = n.Position - Main.screenPosition;
                    sb.Draw(_nebulaTex, pos, null, drawColor, n.Rotation,
                        nebOrigin, n.CurrentScale, SpriteEffects.None, 0);
                }
            }

            DrawConstellationLines(sb);

            if (starPoints.Count > 0 && _starTex != null)
            {
                Vector2 starOrigin = _starTex.Size() * 0.5f;
                var sortedStars = starPoints.OrderBy(s => s.Life);
                foreach (var s in sortedStars)
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    sb.Draw(_starTex, pos, null, drawColor, s.Rotation,
                        starOrigin, s.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (stardusts.Count > 0 && _dustTex != null)
            {
                Vector2 dustOrigin = _dustTex.Size() * 0.5f;
                var sortedDusts = stardusts.OrderBy(d => d.Life);
                foreach (var d in sortedDusts)
                {
                    Color drawColor = d.Color * d.Alpha;
                    Vector2 pos = d.Position - Main.screenPosition;
                    sb.Draw(_dustTex, pos, null, drawColor, 0f,
                        dustOrigin, d.Scale, SpriteEffects.None, 0);
                }
            }
        }

        private void DrawConstellationLines(SpriteBatch sb)
        {
            if (_lineTex == null || starPoints.Count < 2) return;

            Vector2 lineOrigin = new Vector2(0f, _lineTex.Height * 0.5f);

            for (int i = 0; i < starPoints.Count; i++)
            {
                var a = starPoints[i];
                if (a.Alpha < 0.05f) continue;

                for (int j = i + 1; j < starPoints.Count; j++)
                {
                    var b = starPoints[j];
                    if (b.Alpha < 0.05f) continue;

                    float dist = Vector2.Distance(a.Position, b.Position);

                    if (dist > LineBreakDistance || dist < 3f) continue;

                    float lineAlpha;
                    if (dist <= LineMaxDistance)
                    {
                        lineAlpha = LineBaseAlpha;
                    }
                    else
                    {
                        float breakProgress = (dist - LineMaxDistance) / (LineBreakDistance - LineMaxDistance);
                        lineAlpha = LineBaseAlpha * (1f - breakProgress * breakProgress);
                    }

                    float minStarAlpha = MathF.Min(a.Alpha, b.Alpha);
                    lineAlpha *= minStarAlpha;

                    if (lineAlpha < 0.01f) continue;

                    Vector2 start = a.Position - Main.screenPosition;
                    Vector2 end = b.Position - Main.screenPosition;
                    Vector2 diff = end - start;
                    float length = diff.Length();
                    if (length < 1f) continue;

                    float rotation = diff.ToRotation();
                    Vector2 scale = new Vector2(length / _lineTex.Width, 0.8f);
                    Color drawColor = LineColor * lineAlpha;

                    sb.Draw(_lineTex, start, null, drawColor, rotation,
                        lineOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            starPoints.Clear();
            nebulae.Clear();
            stardusts.Clear();
            starCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
