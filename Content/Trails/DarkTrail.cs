using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class DarkTrail : ITrail
    {
        public class CorruptionPatchParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpreadSpeed;
            public float PulsePhase;
            public float PulseSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float spread = MathF.Min(1f, Progress * SpreadSpeed);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float pulse = 0.6f + 0.4f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, spread * fadeOut * pulse * 0.45f);
                }
            }

            public float CurrentScale
            {
                get
                {
                    float spread = MathF.Min(1f, Progress * SpreadSpeed);
                    return Scale + (MaxScale - Scale) * spread;
                }
            }

            public CorruptionPatchParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float spreadSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                SpreadSpeed = spreadSpeed;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = Main.rand.NextFloat(0.04f, 0.1f);
                Color = color;
            }
        }

        public class DarkChainParticle
        {
            public Vector2 Start;
            public Vector2 End;
            public Vector2 EndVelocity;
            public float Width;
            public int Life;
            public int MaxLife;
            public int LinkCount;
            public float DragStrength;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.7f);
                }
            }

            public float CurrentWidth => Width * (1f - Progress * 0.5f);

            public DarkChainParticle(Vector2 start, Vector2 end, int life, float width, int linkCount, float dragStrength, Color color)
            {
                Start = start;
                End = end;
                MaxLife = life;
                Life = life;
                Width = width;
                LinkCount = linkCount;
                DragStrength = dragStrength;
                EndVelocity = Vector2.Zero;
                Color = color;
            }
        }

        public class CurseMarkParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpinSpeed;
            public float RevealSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float reveal = MathF.Min(1f, Progress * RevealSpeed);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, reveal * fadeOut * 0.6f);
                }
            }

            public float CurrentScale => Scale * MathF.Min(1f, Progress * RevealSpeed);

            public CurseMarkParticle(Vector2 pos, Vector2 vel, int life, float scale, float spinSpeed, float revealSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                SpinSpeed = spinSpeed;
                RevealSpeed = revealSpeed;
                Color = color;
            }
        }

        public string Name { get; set; } = "DarkTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(60, 15, 90, 160);

        public int MaxPatches { get; set; } = 10;
        public int PatchLife { get; set; } = 55;
        public float PatchStartSize { get; set; } = 0.2f;
        public float PatchEndSize { get; set; } = 2.2f;
        public float PatchSpawnChance { get; set; } = 0.025f;
        public float PatchSpreadSpeed { get; set; } = 1.8f;
        public float PatchDriftSpeed { get; set; } = 0.03f;
        public Color PatchColor { get; set; } = new Color(50, 10, 80, 180);

        public int MaxChains { get; set; } = 8;
        public int ChainLife { get; set; } = 35;
        public float ChainWidth { get; set; } = 0.5f;
        public float ChainSpawnChance { get; set; } = 0.04f;
        public int ChainLinkCount { get; set; } = 5;
        public float ChainDragStrength { get; set; } = 0.06f;
        public float ChainReach { get; set; } = 25f;
        public Color ChainColor { get; set; } = new Color(80, 25, 120, 200);

        public int MaxCurseMarks { get; set; } = 12;
        public int CurseMarkLife { get; set; } = 40;
        public float CurseMarkSize { get; set; } = 0.5f;
        public float CurseMarkSpawnChance { get; set; } = 0.06f;
        public float CurseMarkSpinSpeed { get; set; } = 0.03f;
        public float CurseMarkRevealSpeed { get; set; } = 3f;
        public float CurseMarkDriftSpeed { get; set; } = 0.08f;
        public Color CurseMarkColor { get; set; } = new Color(100, 30, 150, 220);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<CorruptionPatchParticle> patches = new();
        private List<DarkChainParticle> chains = new();
        private List<CurseMarkParticle> curseMarks = new();

        private GhostTrail _ghostTrail;

        private Texture2D _patchTex;
        private Texture2D _chainTex;
        private Texture2D _curseMarkTex;
        private Texture2D _ghostTex;

        private Vector2 _lastCenter;

        public bool HasContent => patches.Count > 0 || chains.Count > 0 || curseMarks.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_patchTex != null) return;
            _patchTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/DarkTrail/DarkTrailPatch").Value;
            _chainTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/DarkTrail/DarkTrailChain").Value;
            _curseMarkTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/DarkTrail/DarkTrailCurseMark").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/DarkTrail/DarkTrailGhost").Value;
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

            if (patches.Count < MaxPatches && Main.rand.NextFloat() < PatchSpawnChance)
                SpawnPatch(center, velocity, moveDir);

            if (chains.Count < MaxChains && Main.rand.NextFloat() < ChainSpawnChance)
                SpawnChain(center, velocity, moveDir);

            if (curseMarks.Count < MaxCurseMarks && Main.rand.NextFloat() < CurseMarkSpawnChance)
                SpawnCurseMark(center, velocity, moveDir);

            for (int i = patches.Count - 1; i >= 0; i--)
            {
                var p = patches[i];
                p.PulsePhase += p.PulseSpeed;
                p.Velocity *= 0.98f;
                p.Position += p.Velocity;
                p.Life--;
                if (p.Life <= 0) patches.RemoveAt(i);
            }

            for (int i = chains.Count - 1; i >= 0; i--)
            {
                var c = chains[i];
                Vector2 toCenter = _lastCenter - c.End;
                c.EndVelocity += toCenter * c.DragStrength;
                c.EndVelocity *= 0.92f;
                c.End += c.EndVelocity;
                c.Start += (_lastCenter - c.Start) * 0.05f;
                c.Life--;
                if (c.Life <= 0) chains.RemoveAt(i);
            }

            for (int i = curseMarks.Count - 1; i >= 0; i--)
            {
                var m = curseMarks[i];
                m.Rotation += m.SpinSpeed;
                m.Velocity *= 0.97f;
                m.Position += m.Velocity;
                m.Life--;
                if (m.Life <= 0) curseMarks.RemoveAt(i);
            }
        }

        private void SpawnPatch(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(8f, 8f);
            Vector2 drift = Main.rand.NextVector2Circular(PatchDriftSpeed, PatchDriftSpeed);
            float startSize = PatchStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = PatchEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            float spreadSpeed = PatchSpreadSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = PatchColor * Main.rand.NextFloat(0.5f, 1f);
            patches.Add(new CorruptionPatchParticle(pos, drift, PatchLife, startSize, endSize, spreadSpeed, color));
        }

        private void SpawnChain(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 start = center + SpawnOffset + Main.rand.NextVector2Circular(3f, 3f);
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 end = start + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * ChainReach * Main.rand.NextFloat(0.5f, 1f);
            float width = ChainWidth * Main.rand.NextFloat(0.7f, 1.3f);
            int linkCount = Main.rand.Next(3, ChainLinkCount + 1);
            float dragStrength = ChainDragStrength * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = ChainColor * Main.rand.NextFloat(0.5f, 1f);
            chains.Add(new DarkChainParticle(start, end, ChainLife, width, linkCount, dragStrength, color));
        }

        private void SpawnCurseMark(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(6f, 6f);
            Vector2 drift = Main.rand.NextVector2Circular(CurseMarkDriftSpeed, CurseMarkDriftSpeed);
            float scale = CurseMarkSize * Main.rand.NextFloat(0.6f, 1.4f);
            float spinSpeed = Main.rand.NextFloat(-CurseMarkSpinSpeed, CurseMarkSpinSpeed);
            float revealSpeed = CurseMarkRevealSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = CurseMarkColor * Main.rand.NextFloat(0.5f, 1f);
            curseMarks.Add(new CurseMarkParticle(pos, drift, CurseMarkLife, scale, spinSpeed, revealSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (patches.Count > 0 && _patchTex != null)
            {
                Vector2 patchOrigin = _patchTex.Size() * 0.5f;
                var sortedPatches = patches.OrderBy(p => p.Life);
                foreach (var p in sortedPatches)
                {
                    Color drawColor = p.Color * p.Alpha;
                    Vector2 pos = p.Position - Main.screenPosition;
                    sb.Draw(_patchTex, pos, null, drawColor, p.Rotation,
                        patchOrigin, p.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (chains.Count > 0 && _chainTex != null)
            {
                Vector2 chainOrigin = new Vector2(0f, _chainTex.Height * 0.5f);
                var sortedChains = chains.OrderBy(c => c.Life);
                foreach (var c in sortedChains)
                {
                    Vector2 delta = c.End - c.Start;
                    float totalLength = delta.Length();
                    if (totalLength < 1f) continue;
                    float baseAngle = delta.ToRotation();
                    float linkLength = totalLength / c.LinkCount;

                    for (int i = 0; i < c.LinkCount; i++)
                    {
                        float t = i / (float)c.LinkCount;
                        Vector2 linkPos = Vector2.Lerp(c.Start, c.End, t);
                        float wobble = MathF.Sin(t * MathHelper.Pi * 3 + c.Progress * 8f) * 3f;
                        Vector2 perp = new Vector2(-MathF.Sin(baseAngle), MathF.Cos(baseAngle)) * wobble;
                        linkPos += perp;

                        Color drawColor = c.Color * c.Alpha;
                        Vector2 pos = linkPos - Main.screenPosition;
                        Vector2 scale = new Vector2(linkLength / _chainTex.Width, c.CurrentWidth);
                        float angle = baseAngle + MathF.Sin(t * MathHelper.Pi * 3 + c.Progress * 8f) * 0.2f;
                        sb.Draw(_chainTex, pos, null, drawColor, angle,
                            chainOrigin, scale, SpriteEffects.None, 0);
                    }
                }
            }

            if (curseMarks.Count > 0 && _curseMarkTex != null)
            {
                Vector2 markOrigin = _curseMarkTex.Size() * 0.5f;
                var sortedMarks = curseMarks.OrderBy(m => m.Life);
                foreach (var m in sortedMarks)
                {
                    Color drawColor = m.Color * m.Alpha;
                    Vector2 pos = m.Position - Main.screenPosition;
                    sb.Draw(_curseMarkTex, pos, null, drawColor, m.Rotation,
                        markOrigin, m.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            patches.Clear();
            chains.Clear();
            curseMarks.Clear();
            _ghostTrail?.Clear();
        }
    }
}