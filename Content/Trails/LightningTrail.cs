using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class LightningTrail : ITrail
    {
        public class LightningArcParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Length;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public int BranchLevel;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float flash = Progress < 0.15f ? Progress / 0.15f : 1f;
                    float fadeOut = 1f - Progress * Progress;
                    return MathF.Max(0f, flash * fadeOut);
                }
            }

            public float CurrentLength => Length * (1f - Progress * 0.3f);

            public float CurrentWidth => Scale * (1f - BranchLevel * 0.25f) * (1f - Progress * 0.5f);

            public LightningArcParticle(Vector2 pos, Vector2 vel, int life, float scale, float length, float rotation, int branchLevel, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Length = length;
                Rotation = rotation;
                BranchLevel = branchLevel;
                Color = color;
            }
        }

        public class LightningFlashParticle
        {
            public Vector2 Position;
            public float Scale;
            public int Life;
            public int MaxLife;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float flash = 1f - Progress * Progress * Progress;
                    return MathF.Max(0f, flash);
                }
            }

            public float CurrentScale => Scale * (0.3f + 0.7f * (1f - Progress));

            public LightningFlashParticle(Vector2 pos, int life, float scale, Color color)
            {
                Position = pos;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Color = color;
            }
        }

        public class LightningFieldParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float CurveAngle;
            public float CurveSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.3f);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.4f);

            public LightningFieldParticle(Vector2 pos, Vector2 vel, int life, float scale, float curveSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                CurveAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                CurveSpeed = curveSpeed;
                Color = color;
            }
        }

        public string Name { get; set; } = "LightningTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 1;
        public float GhostWidthScale { get; set; } = 0.3f;
        public float GhostLengthScale { get; set; } = 2.0f;
        public float GhostAlpha { get; set; } = 0.6f;
        public Color GhostColor { get; set; } = new Color(200, 200, 255, 200);

        public int MaxArcs { get; set; } = 40;
        public int ArcLife { get; set; } = 8;
        public float ArcScale { get; set; } = 0.5f;
        public float ArcLength { get; set; } = 14f;
        public int ArcSpawnInterval { get; set; } = 1;
        public float ArcJitter { get; set; } = 0.8f;
        public int ArcMaxBranch { get; set; } = 2;
        public float ArcBranchChance { get; set; } = 0.3f;
        public float ArcDriftSpeed { get; set; } = 0.15f;
        public Color ArcColor { get; set; } = new Color(180, 180, 255, 240);

        public int MaxFlashes { get; set; } = 8;
        public int FlashLife { get; set; } = 4;
        public float FlashSize { get; set; } = 1.0f;
        public float FlashSpawnChance { get; set; } = 0.06f;
        public Color FlashColor { get; set; } = new Color(255, 255, 220, 255);

        public int MaxFields { get; set; } = 15;
        public int FieldLife { get; set; } = 35;
        public float FieldSize { get; set; } = 0.4f;
        public float FieldSpawnChance { get; set; } = 0.08f;
        public float FieldCurveSpeed { get; set; } = 0.06f;
        public float FieldDriftSpeed { get; set; } = 0.2f;
        public Color FieldColor { get; set; } = new Color(140, 140, 220, 160);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<LightningArcParticle> arcs = new();
        private List<LightningFlashParticle> flashes = new();
        private List<LightningFieldParticle> fields = new();
        private int arcCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _arcTex;
        private Texture2D _flashTex;
        private Texture2D _fieldTex;
        private Texture2D _ghostTex;

        public bool HasContent => arcs.Count > 0 || flashes.Count > 0 || fields.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_arcTex != null) return;
            _arcTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LightningTrail/LightningTrailArc").Value;
            _flashTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LightningTrail/LightningTrailFlash").Value;
            _fieldTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LightningTrail/LightningTrailField").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LightningTrail/LightningTrailGhost").Value;
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

            arcCounter++;
            if (arcCounter >= ArcSpawnInterval && arcs.Count < MaxArcs)
            {
                arcCounter = 0;
                SpawnArcChain(center, velocity, moveDir, 0);
            }

            if (flashes.Count < MaxFlashes && Main.rand.NextFloat() < FlashSpawnChance)
                SpawnFlash(center);

            if (fields.Count < MaxFields && Main.rand.NextFloat() < FieldSpawnChance)
                SpawnField(center, velocity, moveDir);

            for (int i = arcs.Count - 1; i >= 0; i--)
            {
                var a = arcs[i];
                a.Velocity *= 0.95f;
                a.Position += a.Velocity;
                a.Life--;
                if (a.Life <= 0) arcs.RemoveAt(i);
            }

            for (int i = flashes.Count - 1; i >= 0; i--)
            {
                var f = flashes[i];
                f.Life--;
                if (f.Life <= 0) flashes.RemoveAt(i);
            }

            for (int i = fields.Count - 1; i >= 0; i--)
            {
                var f = fields[i];
                f.CurveAngle += f.CurveSpeed;
                f.Velocity *= 0.97f;
                f.Position += f.Velocity;
                f.Life--;
                if (f.Life <= 0) fields.RemoveAt(i);
            }
        }

        private void SpawnArcChain(Vector2 center, Vector2 velocity, Vector2 moveDir, int startBranch)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(3f, 3f);
            float baseAngle = velocity.Length() > 0.5f ? velocity.ToRotation() : Main.rand.NextFloat(MathHelper.TwoPi);

            int segments = Main.rand.Next(2, 5);
            for (int s = 0; s < segments; s++)
            {
                float jitter = Main.rand.NextFloat(-ArcJitter, ArcJitter);
                float angle = baseAngle + jitter;
                float length = ArcLength * Main.rand.NextFloat(0.6f, 1.4f);
                float scale = ArcScale * Main.rand.NextFloat(0.7f, 1.3f);
                Color color = ArcColor * Main.rand.NextFloat(0.6f, 1f);

                Vector2 drift = Main.rand.NextVector2Circular(ArcDriftSpeed, ArcDriftSpeed);
                arcs.Add(new LightningArcParticle(pos, drift, ArcLife, scale, length, angle, startBranch, color));

                Vector2 tipPos = pos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * length;
                pos = tipPos;
                baseAngle = angle;

                if (s > 0 && startBranch < ArcMaxBranch && Main.rand.NextFloat() < ArcBranchChance)
                {
                    float branchAngle = angle + Main.rand.NextFloat(0.5f, 1.2f) * (Main.rand.NextBool() ? 1f : -1f);
                    float branchLength = length * Main.rand.NextFloat(0.4f, 0.7f);
                    float branchScale = scale * 0.7f;
                    Color branchColor = ArcColor * Main.rand.NextFloat(0.4f, 0.8f);

                    arcs.Add(new LightningArcParticle(pos, drift * 0.5f, ArcLife - 2, branchScale, branchLength, branchAngle, startBranch + 1, branchColor));
                }
            }
        }

        private void SpawnFlash(Vector2 center)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(5f, 5f);
            float scale = Main.rand.NextFloat(0.6f, 1.4f) * FlashSize;
            Color color = FlashColor * Main.rand.NextFloat(0.7f, 1f);

            flashes.Add(new LightningFlashParticle(pos, FlashLife, scale, color));
        }

        private void SpawnField(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-10f, 10f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(8f, 8f);

            Vector2 drift = Main.rand.NextVector2Circular(FieldDriftSpeed, FieldDriftSpeed);
            float scale = Main.rand.NextFloat(0.6f, 1.3f) * FieldSize;
            float curveSpeed = FieldCurveSpeed * Main.rand.NextFloat(0.7f, 1.3f) * (Main.rand.NextBool() ? 1f : -1f);
            Color color = FieldColor * Main.rand.NextFloat(0.5f, 1f);

            fields.Add(new LightningFieldParticle(pos, drift, FieldLife, scale, curveSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (fields.Count > 0 && _fieldTex != null)
            {
                Vector2 fieldOrigin = _fieldTex.Size() * 0.5f;
                var sortedFields = fields.OrderBy(f => f.Life);
                foreach (var f in sortedFields)
                {
                    Color drawColor = f.Color * f.Alpha;
                    Vector2 pos = f.Position - Main.screenPosition;
                    sb.Draw(_fieldTex, pos, null, drawColor, f.CurveAngle,
                        fieldOrigin, f.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (arcs.Count > 0 && _arcTex != null)
            {
                Vector2 arcOrigin = new Vector2(0f, _arcTex.Height * 0.5f);
                var sortedArcs = arcs.OrderBy(a => a.BranchLevel).ThenBy(a => a.Life);
                foreach (var a in sortedArcs)
                {
                    Color drawColor = a.Color * a.Alpha;
                    Vector2 pos = a.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(a.CurrentLength / _arcTex.Width, a.CurrentWidth);
                    sb.Draw(_arcTex, pos, null, drawColor, a.Rotation,
                        arcOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (flashes.Count > 0 && _flashTex != null)
            {
                Vector2 flashOrigin = _flashTex.Size() * 0.5f;
                var sortedFlashes = flashes.OrderBy(f => f.Life);
                foreach (var f in sortedFlashes)
                {
                    Color drawColor = f.Color * f.Alpha;
                    Vector2 pos = f.Position - Main.screenPosition;
                    sb.Draw(_flashTex, pos, null, drawColor, 0f,
                        flashOrigin, f.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            arcs.Clear();
            flashes.Clear();
            fields.Clear();
            arcCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
