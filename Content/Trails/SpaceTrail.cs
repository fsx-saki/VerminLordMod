using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class SpaceTrail : ITrail
    {
        public class FoldLineParticle
        {
            public Vector2 Start;
            public Vector2 End;
            public float Width;
            public int Life;
            public int MaxLife;
            public float RevealProgress;
            public float RevealSpeed;
            public float FoldAngle;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 3f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.7f);
                }
            }

            public float CurrentWidth => Width * (1f - Progress * 0.5f);

            public float RevealedLength => MathF.Min(1f, RevealProgress) * (End - Start).Length();

            public FoldLineParticle(Vector2 start, Vector2 end, int life, float width, float revealSpeed, float foldAngle, Color color)
            {
                Start = start;
                End = end;
                MaxLife = life;
                Life = life;
                Width = width;
                RevealProgress = 0f;
                RevealSpeed = revealSpeed;
                FoldAngle = foldAngle;
                Color = color;
            }
        }

        public class WarpPointParticle
        {
            public Vector2 Position;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float DistortStrength;
            public float ExpandSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.5f);
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

            public WarpPointParticle(Vector2 pos, int life, float scale, float maxScale, float expandSpeed, float rotSpeed, float distortStrength, Color color)
            {
                Position = pos;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                ExpandSpeed = expandSpeed;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                DistortStrength = distortStrength;
                Color = color;
            }
        }

        public class MirrorShardParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float HueShift;
            public float HueSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.6f);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.3f);

            public Color CurrentColor
            {
                get
                {
                    float hue = (HueShift + Progress * HueSpeed) % 1f;
                    if (hue < 0) hue += 1f;
                    return Color;
                }
            }

            public MirrorShardParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotSpeed, float hueShift, float hueSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                HueShift = hueShift;
                HueSpeed = hueSpeed;
                Color = color;
            }
        }

        public string Name { get; set; } = "SpaceTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(160, 120, 220, 160);

        public int MaxFoldLines { get; set; } = 8;
        public int FoldLineLife { get; set; } = 40;
        public float FoldLineWidth { get; set; } = 0.5f;
        public float FoldLineSpawnChance { get; set; } = 0.04f;
        public float FoldLineRevealSpeed { get; set; } = 2.5f;
        public float FoldLineReach { get; set; } = 28f;
        public int FoldLineSegments { get; set; } = 3;
        public float FoldLineAngleRange { get; set; } = 0.8f;
        public Color FoldLineColor { get; set; } = new Color(180, 140, 240, 210);

        public int MaxWarpPoints { get; set; } = 6;
        public int WarpPointLife { get; set; } = 50;
        public float WarpPointStartSize { get; set; } = 0.3f;
        public float WarpPointEndSize { get; set; } = 1.5f;
        public float WarpPointSpawnChance { get; set; } = 0.02f;
        public float WarpPointExpandSpeed { get; set; } = 1.8f;
        public float WarpPointRotSpeed { get; set; } = 0.04f;
        public float WarpPointDistortStrength { get; set; } = 8f;
        public Color WarpPointColor { get; set; } = new Color(140, 100, 200, 180);

        public int MaxMirrorShards { get; set; } = 12;
        public int MirrorShardLife { get; set; } = 35;
        public float MirrorShardSize { get; set; } = 0.5f;
        public float MirrorShardSpawnChance { get; set; } = 0.06f;
        public float MirrorShardDriftSpeed { get; set; } = 0.1f;
        public float MirrorShardRotSpeed { get; set; } = 0.04f;
        public float MirrorShardHueSpeed { get; set; } = 0.5f;
        public Color MirrorShardColor { get; set; } = new Color(200, 180, 255, 220);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<FoldLineParticle> foldLines = new();
        private List<WarpPointParticle> warpPoints = new();
        private List<MirrorShardParticle> mirrorShards = new();

        private GhostTrail _ghostTrail;

        private Texture2D _foldLineTex;
        private Texture2D _warpPointTex;
        private Texture2D _mirrorShardTex;
        private Texture2D _ghostTex;

        private Vector2 _lastCenter;

        public bool HasContent => foldLines.Count > 0 || warpPoints.Count > 0 || mirrorShards.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_foldLineTex != null) return;
            _foldLineTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SpaceTrail/SpaceTrailFold").Value;
            _warpPointTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SpaceTrail/SpaceTrailWarp").Value;
            _mirrorShardTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SpaceTrail/SpaceTrailShard").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SpaceTrail/SpaceTrailGhost").Value;
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

            if (foldLines.Count < MaxFoldLines && Main.rand.NextFloat() < FoldLineSpawnChance)
                SpawnFoldLine(center, velocity, moveDir);

            if (warpPoints.Count < MaxWarpPoints && Main.rand.NextFloat() < WarpPointSpawnChance)
                SpawnWarpPoint(center, velocity, moveDir);

            if (mirrorShards.Count < MaxMirrorShards && Main.rand.NextFloat() < MirrorShardSpawnChance)
                SpawnMirrorShard(center, velocity, moveDir);

            for (int i = foldLines.Count - 1; i >= 0; i--)
            {
                var f = foldLines[i];
                f.RevealProgress += f.RevealSpeed * 0.016f;
                f.Start += (_lastCenter - f.Start) * 0.03f;
                f.End += (_lastCenter - f.End) * 0.02f;
                f.Life--;
                if (f.Life <= 0) foldLines.RemoveAt(i);
            }

            for (int i = warpPoints.Count - 1; i >= 0; i--)
            {
                var w = warpPoints[i];
                w.Rotation += w.RotSpeed;
                w.Position += (_lastCenter - w.Position) * 0.02f;
                w.Life--;
                if (w.Life <= 0) warpPoints.RemoveAt(i);
            }

            for (int i = mirrorShards.Count - 1; i >= 0; i--)
            {
                var m = mirrorShards[i];
                m.Rotation += m.RotSpeed;
                m.HueShift += m.HueSpeed * 0.016f;
                m.Velocity *= 0.97f;
                m.Position += m.Velocity;
                m.Life--;
                if (m.Life <= 0) mirrorShards.RemoveAt(i);
            }
        }

        private void SpawnFoldLine(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 start = center + SpawnOffset + Main.rand.NextVector2Circular(4f, 4f);
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            float reach = FoldLineReach * Main.rand.NextFloat(0.5f, 1f);
            Vector2 end = start + new Vector2(MathF.Cos(baseAngle), MathF.Sin(baseAngle)) * reach;
            float width = FoldLineWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float revealSpeed = FoldLineRevealSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float foldAngle = Main.rand.NextFloat(-FoldLineAngleRange, FoldLineAngleRange);
            Color color = FoldLineColor * Main.rand.NextFloat(0.5f, 1f);
            foldLines.Add(new FoldLineParticle(start, end, FoldLineLife, width, revealSpeed, foldAngle, color));
        }

        private void SpawnWarpPoint(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(8f, 8f);
            float startSize = WarpPointStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = WarpPointEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            float expandSpeed = WarpPointExpandSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float rotSpeed = WarpPointRotSpeed * Main.rand.NextFloat(0.5f, 1.5f) * (Main.rand.NextBool() ? 1f : -1f);
            float distortStrength = WarpPointDistortStrength * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = WarpPointColor * Main.rand.NextFloat(0.5f, 1f);
            warpPoints.Add(new WarpPointParticle(pos, WarpPointLife, startSize, endSize, expandSpeed, rotSpeed, distortStrength, color));
        }

        private void SpawnMirrorShard(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(5f, 5f);
            Vector2 inertia = -velocity * InertiaFactor * 0.15f;
            Vector2 drift = Main.rand.NextVector2Circular(MirrorShardDriftSpeed, MirrorShardDriftSpeed);
            Vector2 vel = inertia + drift;
            float scale = MirrorShardSize * Main.rand.NextFloat(0.5f, 1.5f);
            float rotSpeed = MirrorShardRotSpeed * Main.rand.NextFloat(0.5f, 2f) * (Main.rand.NextBool() ? 1f : -1f);
            float hueShift = Main.rand.NextFloat();
            float hueSpeed = MirrorShardHueSpeed * Main.rand.NextFloat(0.5f, 1.5f);
            Color color = MirrorShardColor * Main.rand.NextFloat(0.5f, 1f);
            mirrorShards.Add(new MirrorShardParticle(pos, vel, MirrorShardLife, scale, rotSpeed, hueShift, hueSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (warpPoints.Count > 0 && _warpPointTex != null)
            {
                Vector2 warpOrigin = _warpPointTex.Size() * 0.5f;
                var sortedWarps = warpPoints.OrderBy(w => w.Life);
                foreach (var w in sortedWarps)
                {
                    Color drawColor = w.Color * w.Alpha;
                    Vector2 pos = w.Position - Main.screenPosition;
                    float scaleX = w.CurrentScale * (1f + MathF.Sin(w.Rotation * 2f) * 0.15f);
                    float scaleY = w.CurrentScale * (1f - MathF.Sin(w.Rotation * 2f) * 0.15f);
                    Vector2 scale = new Vector2(scaleX, scaleY);
                    sb.Draw(_warpPointTex, pos, null, drawColor, w.Rotation,
                        warpOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (foldLines.Count > 0 && _foldLineTex != null)
            {
                Vector2 foldOrigin = new Vector2(0f, _foldLineTex.Height * 0.5f);
                var sortedFolds = foldLines.OrderBy(f => f.Life);
                foreach (var f in sortedFolds)
                {
                    Vector2 delta = f.End - f.Start;
                    float totalLength = delta.Length();
                    if (totalLength < 1f) continue;
                    float baseAngle = delta.ToRotation();
                    float revealedLen = f.RevealedLength;
                    int segCount = FoldLineSegments;
                    float segLen = revealedLen / segCount;

                    for (int i = 0; i < segCount; i++)
                    {
                        float t = i / (float)segCount;
                        Vector2 segStart = f.Start + delta.SafeNormalize(Vector2.Zero) * (totalLength * t);
                        float foldOffset = (i % 2 == 0 ? 1f : -1f) * f.FoldAngle * 8f;
                        Vector2 perp = new Vector2(-MathF.Sin(baseAngle), MathF.Cos(baseAngle)) * foldOffset;
                        Vector2 segPos = segStart + perp;

                        Color drawColor = f.Color * f.Alpha;
                        Vector2 pos = segPos - Main.screenPosition;
                        float angle = baseAngle + (i % 2 == 0 ? f.FoldAngle : -f.FoldAngle);
                        Vector2 scale = new Vector2(segLen / _foldLineTex.Width, f.CurrentWidth);
                        sb.Draw(_foldLineTex, pos, null, drawColor, angle,
                            foldOrigin, scale, SpriteEffects.None, 0);
                    }
                }
            }

            if (mirrorShards.Count > 0 && _mirrorShardTex != null)
            {
                Vector2 shardOrigin = _mirrorShardTex.Size() * 0.5f;
                var sortedShards = mirrorShards.OrderBy(m => m.Life);
                foreach (var m in sortedShards)
                {
                    float hue = (m.HueShift + m.Progress * m.HueSpeed) % 1f;
                    if (hue < 0) hue += 1f;
                    Color shiftedColor = Main.hslToRgb(hue, 0.6f, 0.7f);
                    Color drawColor = Color.Lerp(m.Color, shiftedColor, 0.7f) * m.Alpha;
                    Vector2 pos = m.Position - Main.screenPosition;
                    sb.Draw(_mirrorShardTex, pos, null, drawColor, m.Rotation,
                        shardOrigin, m.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            foldLines.Clear();
            warpPoints.Clear();
            mirrorShards.Clear();
            _ghostTrail?.Clear();
        }
    }
}
