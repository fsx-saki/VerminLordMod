using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class KnifeTrail : ITrail
    {
        public class BladeFlashParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Stretch;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpinSpeed;
            public float PulsePhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 8f) * (1f - Progress) * (0.8f + 0.2f * MathF.Sin(PulsePhase)));

            public BladeFlashParticle(Vector2 pos, Vector2 vel, int life, float scale, float stretch, float rotation, float spinSpeed, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Scale = scale; Stretch = stretch; Rotation = rotation; SpinSpeed = spinSpeed;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi); Color = color;
            }
        }

        public class CuttingMarkParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Length;
            public float Width;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float FadeInSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * FadeInSpeed) * (1f - Progress * Progress) * 0.5f);
            public float CurrentLength => Length * (1f - Progress * 0.2f);
            public float CurrentWidth => Width * (1f - Progress * 0.5f);

            public CuttingMarkParticle(Vector2 pos, Vector2 vel, int life, float length, float width, float rotation, float fadeInSpeed, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Length = length; Width = width; Rotation = rotation; FadeInSpeed = fadeInSpeed; Color = color;
            }
        }

        public class EdgeShardParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float FadeSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, (1f - Progress * FadeSpeed) * 0.7f);
            public float CurrentScale => Scale * (1f - Progress * 0.4f);

            public EdgeShardParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotSpeed, float fadeSpeed, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Scale = scale; Rotation = Main.rand.NextFloat(MathHelper.TwoPi); RotSpeed = rotSpeed; FadeSpeed = fadeSpeed; Color = color;
            }
        }

        public string Name { get; set; } = "KnifeTrail";
        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 6;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.12f;
        public float GhostLengthScale { get; set; } = 1.2f;
        public float GhostAlpha { get; set; } = 0.25f;
        public Color GhostColor { get; set; } = new Color(190, 200, 220, 120);

        public int MaxBladeFlashes { get; set; } = 12;
        public int BladeFlashLife { get; set; } = 18;
        public float BladeFlashSize { get; set; } = 0.5f;
        public float BladeFlashStretch { get; set; } = 4f;
        public float BladeFlashSpawnChance { get; set; } = 0.12f;
        public float BladeFlashSpinSpeed { get; set; } = 0.2f;
        public float BladeFlashDriftSpeed { get; set; } = 0.3f;
        public Color BladeFlashColor { get; set; } = new Color(200, 215, 240, 230);

        public int MaxCuttingMarks { get; set; } = 6;
        public int CuttingMarkLife { get; set; } = 30;
        public float CuttingMarkLength { get; set; } = 25f;
        public float CuttingMarkWidth { get; set; } = 0.2f;
        public float CuttingMarkSpawnChance { get; set; } = 0.04f;
        public float CuttingMarkFadeInSpeed { get; set; } = 5f;
        public float CuttingMarkDriftSpeed { get; set; } = 0.03f;
        public Color CuttingMarkColor { get; set; } = new Color(160, 180, 220, 200);

        public int MaxEdgeShards { get; set; } = 20;
        public int EdgeShardLife { get; set; } = 22;
        public float EdgeShardSize { get; set; } = 0.35f;
        public float EdgeShardSpawnChance { get; set; } = 0.18f;
        public float EdgeShardRotSpeed { get; set; } = 0.15f;
        public float EdgeShardFadeSpeed { get; set; } = 1.5f;
        public float EdgeShardDriftSpeed { get; set; } = 0.5f;
        public Color EdgeShardColor { get; set; } = new Color(180, 195, 230, 200);

        public float InertiaFactor { get; set; } = 0.12f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<BladeFlashParticle> bladeFlashes = new();
        private List<CuttingMarkParticle> cuttingMarks = new();
        private List<EdgeShardParticle> edgeShards = new();
        private GhostTrail _ghostTrail;
        private Texture2D _flashTex, _cutTex, _shardTex, _ghostTex;

        public bool HasContent => bladeFlashes.Count > 0 || cuttingMarks.Count > 0 || edgeShards.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_flashTex != null) return;
            _flashTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/KnifeTrail/KnifeTrailFlash").Value;
            _cutTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/KnifeTrail/KnifeTrailCut").Value;
            _shardTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/KnifeTrail/KnifeTrailShard").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/KnifeTrail/KnifeTrailGhost").Value;
        }

        private void EnsureGhostTrail()
        {
            if (!EnableGhostTrail || _ghostTrail != null) return;
            EnsureTextures();
            _ghostTrail = new GhostTrail
            {
                TrailTexture = _ghostTex, TrailColor = GhostColor, MaxPositions = GhostMaxPositions,
                RecordInterval = GhostRecordInterval, WidthScale = GhostWidthScale, LengthScale = GhostLengthScale,
                Alpha = GhostAlpha, UseAdditiveBlend = true, EnableGlow = false,
            };
        }

        public void Update(Vector2 center, Vector2 velocity)
        {
            EnsureTextures();
            EnsureGhostTrail();
            if (_ghostTrail != null) _ghostTrail.Update(center, velocity);

            Vector2 moveDir = velocity.SafeNormalize(Vector2.UnitX);

            if (bladeFlashes.Count < MaxBladeFlashes && Main.rand.NextFloat() < BladeFlashSpawnChance)
                SpawnBladeFlash(center, velocity, moveDir);
            if (cuttingMarks.Count < MaxCuttingMarks && Main.rand.NextFloat() < CuttingMarkSpawnChance)
                SpawnCuttingMark(center, velocity, moveDir);
            if (edgeShards.Count < MaxEdgeShards && Main.rand.NextFloat() < EdgeShardSpawnChance)
                SpawnEdgeShard(center, velocity, moveDir);

            for (int i = bladeFlashes.Count - 1; i >= 0; i--)
            {
                var b = bladeFlashes[i];
                b.Rotation += b.SpinSpeed; b.PulsePhase += 0.1f;
                b.Velocity *= 0.93f; b.Position += b.Velocity; b.Life--;
                if (b.Life <= 0) bladeFlashes.RemoveAt(i);
            }
            for (int i = cuttingMarks.Count - 1; i >= 0; i--)
            {
                var c = cuttingMarks[i];
                c.Velocity *= 0.97f; c.Position += c.Velocity; c.Life--;
                if (c.Life <= 0) cuttingMarks.RemoveAt(i);
            }
            for (int i = edgeShards.Count - 1; i >= 0; i--)
            {
                var e = edgeShards[i];
                e.Rotation += e.RotSpeed; e.Velocity *= 0.96f; e.Position += e.Velocity; e.Life--;
                if (e.Life <= 0) edgeShards.RemoveAt(i);
            }
        }

        private void SpawnBladeFlash(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(2f, 2f);
            Vector2 vel = -velocity * InertiaFactor * 0.3f + Main.rand.NextVector2Circular(BladeFlashDriftSpeed, BladeFlashDriftSpeed);
            float scale = Main.rand.NextFloat(0.6f, 1.3f) * BladeFlashSize;
            float stretch = Main.rand.NextFloat(0.8f, 1.5f) * BladeFlashStretch;
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.3f, 0.3f);
            float spinSpeed = Main.rand.NextFloat(-BladeFlashSpinSpeed, BladeFlashSpinSpeed);
            Color color = BladeFlashColor * Main.rand.NextFloat(0.7f, 1f);
            bladeFlashes.Add(new BladeFlashParticle(pos, vel, BladeFlashLife, scale, stretch, rotation, spinSpeed, color));
        }

        private void SpawnCuttingMark(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(1f, 1f);
            Vector2 vel = Main.rand.NextVector2Circular(CuttingMarkDriftSpeed, CuttingMarkDriftSpeed);
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.15f, 0.15f);
            float length = CuttingMarkLength * Main.rand.NextFloat(0.7f, 1.3f);
            float width = CuttingMarkWidth * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = CuttingMarkColor * Main.rand.NextFloat(0.6f, 1f);
            cuttingMarks.Add(new CuttingMarkParticle(pos, vel, CuttingMarkLife, length, width, rotation, CuttingMarkFadeInSpeed, color));
        }

        private void SpawnEdgeShard(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(3f, 3f);
            Vector2 vel = -velocity * InertiaFactor * 0.5f + Main.rand.NextVector2Circular(EdgeShardDriftSpeed, EdgeShardDriftSpeed);
            float scale = Main.rand.NextFloat(0.5f, 1.4f) * EdgeShardSize;
            float rotSpeed = Main.rand.NextFloat(-EdgeShardRotSpeed, EdgeShardRotSpeed);
            float fadeSpeed = EdgeShardFadeSpeed * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = EdgeShardColor * Main.rand.NextFloat(0.6f, 1f);
            edgeShards.Add(new EdgeShardParticle(pos, vel, EdgeShardLife, scale, rotSpeed, fadeSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null) _ghostTrail.Draw(sb);

            if (cuttingMarks.Count > 0 && _cutTex != null)
            {
                Vector2 cutOrigin = new Vector2(0f, _cutTex.Height * 0.5f);
                foreach (var c in cuttingMarks.OrderBy(x => x.Life))
                {
                    Color drawColor = c.Color * c.Alpha;
                    Vector2 pos = c.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(c.CurrentLength / _cutTex.Width, c.CurrentWidth);
                    sb.Draw(_cutTex, pos, null, drawColor, c.Rotation, cutOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (bladeFlashes.Count > 0 && _flashTex != null)
            {
                Vector2 flashOrigin = _flashTex.Size() * 0.5f;
                foreach (var b in bladeFlashes.OrderBy(x => x.Life))
                {
                    Color drawColor = b.Color * b.Alpha;
                    Vector2 pos = b.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(b.Scale * b.Stretch, b.Scale);
                    sb.Draw(_flashTex, pos, null, drawColor, b.Rotation, flashOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (edgeShards.Count > 0 && _shardTex != null)
            {
                Vector2 shardOrigin = _shardTex.Size() * 0.5f;
                foreach (var e in edgeShards.OrderBy(x => x.Life))
                {
                    Color drawColor = e.Color * e.Alpha;
                    Vector2 pos = e.Position - Main.screenPosition;
                    sb.Draw(_shardTex, pos, null, drawColor, e.Rotation, shardOrigin, e.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            bladeFlashes.Clear(); cuttingMarks.Clear(); edgeShards.Clear();
            _ghostTrail?.Clear();
        }
    }
}