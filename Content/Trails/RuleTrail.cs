using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class RuleTrail : ITrail
    {
        public class GridNodeParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float PulsePhase;
            public float PulseSpeed;
            public float GridX;
            public float GridY;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 5f) * (1f - Progress) * (0.7f + 0.3f * MathF.Sin(PulsePhase)));
            public float CurrentScale => Scale * (0.8f + 0.2f * MathF.Sin(PulsePhase));

            public GridNodeParticle(Vector2 pos, Vector2 vel, int life, float scale, float pulseSpeed, float gridX, float gridY, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Scale = scale; PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi); PulseSpeed = pulseSpeed;
                GridX = gridX; GridY = gridY; Color = color;
            }
        }

        public class RulerMarkParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Length;
            public float Width;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float TickPhase;
            public float TickSpacing;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 4f) * (1f - Progress) * 0.6f);
            public float CurrentLength => Length * (1f - Progress * 0.3f);
            public float CurrentWidth => Width * (1f - Progress * 0.4f);

            public RulerMarkParticle(Vector2 pos, Vector2 vel, int life, float length, float width, float rotation, float tickSpacing, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Length = length; Width = width; Rotation = rotation;
                TickPhase = Main.rand.NextFloat(MathHelper.TwoPi); TickSpacing = tickSpacing; Color = color;
            }
        }

        public class OrderRingParticle
        {
            public Vector2 Position;
            public float Radius;
            public float MaxRadius;
            public float Width;
            public int Life;
            public int MaxLife;
            public float ExpandSpeed;
            public float Rotation;
            public float RotSpeed;
            public int SegmentCount;
            public float SegmentPhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 3f) * (1f - Progress) * 0.5f);
            public float CurrentRadius => Radius + (MaxRadius - Radius) * MathF.Min(1f, Progress * ExpandSpeed);
            public float CurrentWidth => Width * (1f - Progress * 0.3f);

            public OrderRingParticle(Vector2 pos, int life, float radius, float maxRadius, float width, float expandSpeed, float rotSpeed, int segmentCount, Color color)
            {
                Position = pos; MaxLife = life; Life = life; Radius = radius; MaxRadius = maxRadius;
                Width = width; ExpandSpeed = expandSpeed; Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed; SegmentCount = segmentCount; SegmentPhase = Main.rand.NextFloat(MathHelper.TwoPi); Color = color;
            }
        }

        public string Name { get; set; } = "RuleTrail";
        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 6;
        public int GhostRecordInterval { get; set; } = 3;
        public float GhostWidthScale { get; set; } = 0.12f;
        public float GhostLengthScale { get; set; } = 1.2f;
        public float GhostAlpha { get; set; } = 0.2f;
        public Color GhostColor { get; set; } = new Color(180, 200, 230, 100);

        public int MaxGridNodes { get; set; } = 12;
        public int GridNodeLife { get; set; } = 30;
        public float GridNodeSize { get; set; } = 0.3f;
        public float GridNodeSpawnChance { get; set; } = 0.08f;
        public float GridNodePulseSpeed { get; set; } = 0.12f;
        public float GridNodeDriftSpeed { get; set; } = 0.15f;
        public float GridNodeSpread { get; set; } = 8f;
        public Color GridNodeColor { get; set; } = new Color(150, 200, 255, 200);

        public int MaxRulerMarks { get; set; } = 8;
        public int RulerMarkLife { get; set; } = 25;
        public float RulerMarkLength { get; set; } = 20f;
        public float RulerMarkWidth { get; set; } = 0.15f;
        public float RulerMarkSpawnChance { get; set; } = 0.05f;
        public float RulerMarkTickSpacing { get; set; } = 4f;
        public float RulerMarkDriftSpeed { get; set; } = 0.08f;
        public Color RulerMarkColor { get; set; } = new Color(180, 210, 240, 180);

        public int MaxOrderRings { get; set; } = 3;
        public int OrderRingLife { get; set; } = 45;
        public float OrderRingStartRadius { get; set; } = 2f;
        public float OrderRingEndRadius { get; set; } = 30f;
        public float OrderRingWidth { get; set; } = 0.3f;
        public float OrderRingSpawnChance { get; set; } = 0.02f;
        public float OrderRingExpandSpeed { get; set; } = 0.8f;
        public float OrderRingRotSpeed { get; set; } = 0.04f;
        public int OrderRingSegmentCount { get; set; } = 8;
        public Color OrderRingColor { get; set; } = new Color(120, 180, 255, 160);

        public float InertiaFactor { get; set; } = 0.12f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<GridNodeParticle> gridNodes = new();
        private List<RulerMarkParticle> rulerMarks = new();
        private List<OrderRingParticle> orderRings = new();
        private GhostTrail _ghostTrail;
        private Texture2D _nodeTex, _markTex, _ringTex, _ghostTex;

        public bool HasContent => gridNodes.Count > 0 || rulerMarks.Count > 0 || orderRings.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_nodeTex != null) return;
            _nodeTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/RuleTrail/RuleTrailNode").Value;
            _markTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/RuleTrail/RuleTrailMark").Value;
            _ringTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/RuleTrail/RuleTrailRing").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/RuleTrail/RuleTrailGhost").Value;
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

            if (gridNodes.Count < MaxGridNodes && Main.rand.NextFloat() < GridNodeSpawnChance)
                SpawnGridNode(center, velocity, moveDir);
            if (rulerMarks.Count < MaxRulerMarks && Main.rand.NextFloat() < RulerMarkSpawnChance)
                SpawnRulerMark(center, velocity, moveDir);
            if (orderRings.Count < MaxOrderRings && Main.rand.NextFloat() < OrderRingSpawnChance)
                SpawnOrderRing(center, velocity, moveDir);

            for (int i = gridNodes.Count - 1; i >= 0; i--)
            {
                var n = gridNodes[i];
                n.PulsePhase += n.PulseSpeed;
                n.Velocity *= 0.96f; n.Position += n.Velocity; n.Life--;
                if (n.Life <= 0) gridNodes.RemoveAt(i);
            }
            for (int i = rulerMarks.Count - 1; i >= 0; i--)
            {
                var m = rulerMarks[i];
                m.TickPhase += 0.08f;
                m.Velocity *= 0.97f; m.Position += m.Velocity; m.Life--;
                if (m.Life <= 0) rulerMarks.RemoveAt(i);
            }
            for (int i = orderRings.Count - 1; i >= 0; i--)
            {
                var r = orderRings[i];
                r.Rotation += r.RotSpeed; r.SegmentPhase += 0.05f;
                r.Life--;
                if (r.Life <= 0) orderRings.RemoveAt(i);
            }
        }

        private void SpawnGridNode(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(GridNodeSpread, GridNodeSpread);
            Vector2 vel = -velocity * InertiaFactor * 0.3f + Main.rand.NextVector2Circular(GridNodeDriftSpeed, GridNodeDriftSpeed);
            float scale = Main.rand.NextFloat(0.6f, 1.3f) * GridNodeSize;
            float pulseSpeed = GridNodePulseSpeed * Main.rand.NextFloat(0.7f, 1.5f);
            float gridX = MathF.Round(pos.X / 8f) * 8f;
            float gridY = MathF.Round(pos.Y / 8f) * 8f;
            Color color = GridNodeColor * Main.rand.NextFloat(0.6f, 1f);
            gridNodes.Add(new GridNodeParticle(pos, vel, GridNodeLife, scale, pulseSpeed, gridX, gridY, color));
        }

        private void SpawnRulerMark(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(2f, 2f);
            Vector2 vel = Main.rand.NextVector2Circular(RulerMarkDriftSpeed, RulerMarkDriftSpeed);
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.2f, 0.2f);
            float length = RulerMarkLength * Main.rand.NextFloat(0.7f, 1.3f);
            float width = RulerMarkWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float tickSpacing = RulerMarkTickSpacing * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = RulerMarkColor * Main.rand.NextFloat(0.6f, 1f);
            rulerMarks.Add(new RulerMarkParticle(pos, vel, RulerMarkLife, length, width, rotation, tickSpacing, color));
        }

        private void SpawnOrderRing(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset;
            float radius = OrderRingStartRadius;
            float maxRadius = OrderRingEndRadius * Main.rand.NextFloat(0.7f, 1.3f);
            float width = OrderRingWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float expandSpeed = OrderRingExpandSpeed * Main.rand.NextFloat(0.8f, 1.2f);
            float rotSpeed = Main.rand.NextFloat(-OrderRingRotSpeed, OrderRingRotSpeed);
            int segments = OrderRingSegmentCount + Main.rand.Next(-1, 2);
            Color color = OrderRingColor * Main.rand.NextFloat(0.6f, 1f);
            orderRings.Add(new OrderRingParticle(pos, OrderRingLife, radius, maxRadius, width, expandSpeed, rotSpeed, segments, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null) _ghostTrail.Draw(sb);

            if (orderRings.Count > 0 && _ringTex != null)
            {
                Vector2 ringOrigin = _ringTex.Size() * 0.5f;
                foreach (var r in orderRings.OrderBy(x => x.Life))
                {
                    Color drawColor = r.Color * r.Alpha;
                    Vector2 pos = r.Position - Main.screenPosition;
                    float radius = r.CurrentRadius;
                    float circumference = MathHelper.TwoPi * radius;
                    float segmentArc = circumference / r.SegmentCount;
                    float scaleX = radius * 2f / _ringTex.Width;
                    float scaleY = r.CurrentWidth;
                    for (int s = 0; s < r.SegmentCount; s++)
                    {
                        float angle = r.Rotation + (float)s / r.SegmentCount * MathHelper.TwoPi;
                        float segAlpha = 0.5f + 0.5f * MathF.Sin(r.SegmentPhase + s * 0.8f);
                        Color segColor = drawColor * segAlpha;
                        Vector2 segPos = pos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
                        sb.Draw(_ringTex, segPos, null, segColor, angle, ringOrigin, new Vector2(scaleX, scaleY), SpriteEffects.None, 0);
                    }
                }
            }

            if (rulerMarks.Count > 0 && _markTex != null)
            {
                Vector2 markOrigin = new Vector2(0f, _markTex.Height * 0.5f);
                foreach (var m in rulerMarks.OrderBy(x => x.Life))
                {
                    Color drawColor = m.Color * m.Alpha;
                    Vector2 pos = m.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(m.CurrentLength / _markTex.Width, m.CurrentWidth);
                    sb.Draw(_markTex, pos, null, drawColor, m.Rotation, markOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (gridNodes.Count > 0 && _nodeTex != null)
            {
                Vector2 nodeOrigin = _nodeTex.Size() * 0.5f;
                foreach (var n in gridNodes.OrderBy(x => x.Life))
                {
                    Color drawColor = n.Color * n.Alpha;
                    Vector2 pos = n.Position - Main.screenPosition;
                    sb.Draw(_nodeTex, pos, null, drawColor, 0f, nodeOrigin, n.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            gridNodes.Clear(); rulerMarks.Clear(); orderRings.Clear();
            _ghostTrail?.Clear();
        }
    }
}