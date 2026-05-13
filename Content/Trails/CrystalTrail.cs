using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class CrystalTrail : ITrail
    {
        public class ResonanceRingParticle
        {
            public Vector2 Position;
            public float Radius;
            public float MaxRadius;
            public float Width;
            public int Life;
            public int MaxLife;
            public float ExpandSpeed;
            public float PulsePhase;
            public float PulseSpeed;
            public int RingSegments;
            public float GapAngle;
            public float Rotation;
            public float RotSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float pulse = 0.7f + 0.3f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, fadeIn * fadeOut * pulse * 0.55f);
                }
            }

            public float CurrentRadius
            {
                get
                {
                    float expand = MathF.Min(1f, Progress * ExpandSpeed);
                    return Radius + (MaxRadius - Radius) * expand;
                }
            }

            public float CurrentWidth => Width * (1f - Progress * 0.5f);

            public ResonanceRingParticle(Vector2 pos, int life, float radius, float maxRadius, float width, float expandSpeed, int ringSegments, float gapAngle, float rotSpeed, Color color)
            {
                Position = pos;
                MaxLife = life;
                Life = life;
                Radius = radius;
                MaxRadius = maxRadius;
                Width = width;
                ExpandSpeed = expandSpeed;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = Main.rand.NextFloat(0.06f, 0.14f);
                RingSegments = ringSegments;
                GapAngle = gapAngle;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                Color = color;
            }
        }

        public class PrismShardParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float HueBase;
            public float HueRange;
            public float HueSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.7f);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.3f);

            public Color PrismColor
            {
                get
                {
                    float hue = (HueBase + Progress * HueSpeed) % 1f;
                    if (hue < 0) hue += 1f;
                    Color prismatic = Main.hslToRgb(hue, 0.8f, 0.7f);
                    return Color.Lerp(Color, prismatic, 0.6f);
                }
            }

            public PrismShardParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotSpeed, float hueBase, float hueRange, float hueSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                HueBase = hueBase;
                HueRange = hueRange;
                HueSpeed = hueSpeed;
                Color = color;
            }
        }

        public class LatticeNodeParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float ConnectRadius;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.8f);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.3f);

            public LatticeNodeParticle(Vector2 pos, Vector2 vel, int life, float scale, float connectRadius, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                ConnectRadius = connectRadius;
                Color = color;
            }
        }

        public string Name { get; set; } = "CrystalTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(180, 180, 255, 160);

        public int MaxResonanceRings { get; set; } = 5;
        public int ResonanceRingLife { get; set; } = 45;
        public float ResonanceRingStartRadius { get; set; } = 3f;
        public float ResonanceRingEndRadius { get; set; } = 25f;
        public float ResonanceRingWidth { get; set; } = 0.5f;
        public float ResonanceRingSpawnChance { get; set; } = 0.02f;
        public float ResonanceRingExpandSpeed { get; set; } = 1.8f;
        public int ResonanceRingSegments { get; set; } = 4;
        public float ResonanceRingGapAngle { get; set; } = 0.3f;
        public float ResonanceRingRotSpeed { get; set; } = 0.03f;
        public Color ResonanceRingColor { get; set; } = new Color(160, 180, 255, 200);

        public int MaxPrismShards { get; set; } = 15;
        public int PrismShardLife { get; set; } = 35;
        public float PrismShardSize { get; set; } = 0.5f;
        public float PrismShardSpawnChance { get; set; } = 0.07f;
        public float PrismShardDriftSpeed { get; set; } = 0.1f;
        public float PrismShardRotSpeed { get; set; } = 0.05f;
        public float PrismShardHueSpeed { get; set; } = 0.8f;
        public Color PrismShardColor { get; set; } = new Color(200, 200, 255, 220);

        public int MaxLatticeNodes { get; set; } = 12;
        public int LatticeNodeLife { get; set; } = 30;
        public float LatticeNodeSize { get; set; } = 0.4f;
        public float LatticeNodeSpawnChance { get; set; } = 0.08f;
        public float LatticeNodeDriftSpeed { get; set; } = 0.08f;
        public float LatticeNodeConnectRadius { get; set; } = 20f;
        public float LatticeNodeLineWidth { get; set; } = 0.3f;
        public Color LatticeNodeColor { get; set; } = new Color(180, 200, 255, 200);
        public Color LatticeLineColor { get; set; } = new Color(140, 160, 220, 150);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<ResonanceRingParticle> resonanceRings = new();
        private List<PrismShardParticle> prismShards = new();
        private List<LatticeNodeParticle> latticeNodes = new();

        private GhostTrail _ghostTrail;

        private Texture2D _resonanceRingTex;
        private Texture2D _prismShardTex;
        private Texture2D _latticeNodeTex;
        private Texture2D _ghostTex;

        private Vector2 _lastCenter;

        public bool HasContent => resonanceRings.Count > 0 || prismShards.Count > 0 || latticeNodes.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_resonanceRingTex != null) return;
            _resonanceRingTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/CrystalTrail/CrystalTrailRing").Value;
            _prismShardTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/CrystalTrail/CrystalTrailShard").Value;
            _latticeNodeTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/CrystalTrail/CrystalTrailNode").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/CrystalTrail/CrystalTrailGhost").Value;
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

            if (resonanceRings.Count < MaxResonanceRings && Main.rand.NextFloat() < ResonanceRingSpawnChance)
                SpawnResonanceRing(center, velocity, moveDir);

            if (prismShards.Count < MaxPrismShards && Main.rand.NextFloat() < PrismShardSpawnChance)
                SpawnPrismShard(center, velocity, moveDir);

            if (latticeNodes.Count < MaxLatticeNodes && Main.rand.NextFloat() < LatticeNodeSpawnChance)
                SpawnLatticeNode(center, velocity, moveDir);

            for (int i = resonanceRings.Count - 1; i >= 0; i--)
            {
                var r = resonanceRings[i];
                r.PulsePhase += r.PulseSpeed;
                r.Rotation += r.RotSpeed;
                r.Position += (_lastCenter - r.Position) * 0.02f;
                r.Life--;
                if (r.Life <= 0) resonanceRings.RemoveAt(i);
            }

            for (int i = prismShards.Count - 1; i >= 0; i--)
            {
                var p = prismShards[i];
                p.Rotation += p.RotSpeed;
                p.Velocity *= 0.97f;
                p.Position += p.Velocity;
                p.Life--;
                if (p.Life <= 0) prismShards.RemoveAt(i);
            }

            for (int i = latticeNodes.Count - 1; i >= 0; i--)
            {
                var n = latticeNodes[i];
                n.Velocity *= 0.97f;
                n.Position += n.Velocity;
                n.Life--;
                if (n.Life <= 0) latticeNodes.RemoveAt(i);
            }
        }

        private void SpawnResonanceRing(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(4f, 4f);
            float endRadius = ResonanceRingEndRadius * Main.rand.NextFloat(0.7f, 1.3f);
            float width = ResonanceRingWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float expandSpeed = ResonanceRingExpandSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            int segments = Main.rand.Next(3, ResonanceRingSegments + 1);
            float gapAngle = ResonanceRingGapAngle * Main.rand.NextFloat(0.5f, 1.5f);
            float rotSpeed = ResonanceRingRotSpeed * Main.rand.NextFloat(0.5f, 1.5f) * (Main.rand.NextBool() ? 1f : -1f);
            Color color = ResonanceRingColor * Main.rand.NextFloat(0.5f, 1f);
            resonanceRings.Add(new ResonanceRingParticle(pos, ResonanceRingLife, ResonanceRingStartRadius, endRadius, width, expandSpeed, segments, gapAngle, rotSpeed, color));
        }

        private void SpawnPrismShard(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(5f, 5f);
            Vector2 inertia = -velocity * InertiaFactor * 0.15f;
            Vector2 drift = Main.rand.NextVector2Circular(PrismShardDriftSpeed, PrismShardDriftSpeed);
            Vector2 vel = inertia + drift;
            float scale = PrismShardSize * Main.rand.NextFloat(0.5f, 1.5f);
            float rotSpeed = PrismShardRotSpeed * Main.rand.NextFloat(0.5f, 2f) * (Main.rand.NextBool() ? 1f : -1f);
            float hueBase = Main.rand.NextFloat();
            float hueRange = Main.rand.NextFloat(0.1f, 0.3f);
            float hueSpeed = PrismShardHueSpeed * Main.rand.NextFloat(0.5f, 1.5f);
            Color color = PrismShardColor * Main.rand.NextFloat(0.5f, 1f);
            prismShards.Add(new PrismShardParticle(pos, vel, PrismShardLife, scale, rotSpeed, hueBase, hueRange, hueSpeed, color));
        }

        private void SpawnLatticeNode(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(8f, 8f);
            Vector2 inertia = -velocity * InertiaFactor * 0.1f;
            Vector2 drift = Main.rand.NextVector2Circular(LatticeNodeDriftSpeed, LatticeNodeDriftSpeed);
            Vector2 vel = inertia + drift;
            float scale = LatticeNodeSize * Main.rand.NextFloat(0.6f, 1.4f);
            float connectRadius = LatticeNodeConnectRadius * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = LatticeNodeColor * Main.rand.NextFloat(0.5f, 1f);
            latticeNodes.Add(new LatticeNodeParticle(pos, vel, LatticeNodeLife, scale, connectRadius, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (resonanceRings.Count > 0 && _resonanceRingTex != null)
            {
                Vector2 ringOrigin = new Vector2(0f, _resonanceRingTex.Height * 0.5f);
                var sortedRings = resonanceRings.OrderBy(r => r.Life);
                foreach (var r in sortedRings)
                {
                    float segAngle = MathHelper.TwoPi / r.RingSegments;
                    float arcAngle = segAngle - r.GapAngle;

                    for (int i = 0; i < r.RingSegments; i++)
                    {
                        float startAngle = r.Rotation + i * segAngle;
                        int arcSteps = 8;
                        float stepAngle = arcAngle / arcSteps;

                        for (int j = 0; j < arcSteps; j++)
                        {
                            float angle = startAngle + j * stepAngle;
                            Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                            Vector2 pos = r.Position + dir * r.CurrentRadius;

                            Color drawColor = r.Color * r.Alpha;
                            Vector2 screenPos = pos - Main.screenPosition;
                            Vector2 scale = new Vector2(r.CurrentRadius * segAngle / (_resonanceRingTex.Width * 0.5f) * 0.5f, r.CurrentWidth);
                            sb.Draw(_resonanceRingTex, screenPos, null, drawColor, angle + MathHelper.PiOver2,
                                ringOrigin, scale, SpriteEffects.None, 0);
                        }
                    }
                }
            }

            if (latticeNodes.Count > 1 && _latticeNodeTex != null)
            {
                for (int i = 0; i < latticeNodes.Count; i++)
                {
                    for (int j = i + 1; j < latticeNodes.Count; j++)
                    {
                        var a = latticeNodes[i];
                        var b = latticeNodes[j];
                        float dist = Vector2.Distance(a.Position, b.Position);
                        float maxDist = MathF.Min(a.ConnectRadius, b.ConnectRadius);

                        if (dist < maxDist && dist > 1f)
                        {
                            float lineAlpha = (1f - dist / maxDist) * MathF.Min(a.Alpha, b.Alpha) * 0.5f;
                            Color lineColor = LatticeLineColor * lineAlpha;
                            Vector2 start = a.Position - Main.screenPosition;
                            Vector2 end = b.Position - Main.screenPosition;
                            float angle = (b.Position - a.Position).ToRotation();
                            float length = dist;
                            Vector2 scale = new Vector2(length / _latticeNodeTex.Width, LatticeNodeLineWidth);
                            Vector2 origin = new Vector2(0f, _latticeNodeTex.Height * 0.5f);
                            sb.Draw(_latticeNodeTex, start, null, lineColor, angle,
                                origin, scale, SpriteEffects.None, 0);
                        }
                    }
                }
            }

            if (latticeNodes.Count > 0 && _latticeNodeTex != null)
            {
                Vector2 nodeOrigin = _latticeNodeTex.Size() * 0.5f;
                var sortedNodes = latticeNodes.OrderBy(n => n.Life);
                foreach (var n in sortedNodes)
                {
                    Color drawColor = n.Color * n.Alpha;
                    Vector2 pos = n.Position - Main.screenPosition;
                    sb.Draw(_latticeNodeTex, pos, null, drawColor, 0f,
                        nodeOrigin, n.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (prismShards.Count > 0 && _prismShardTex != null)
            {
                Vector2 shardOrigin = _prismShardTex.Size() * 0.5f;
                var sortedShards = prismShards.OrderBy(p => p.Life);
                foreach (var p in sortedShards)
                {
                    Color drawColor = p.PrismColor * p.Alpha;
                    Vector2 pos = p.Position - Main.screenPosition;
                    sb.Draw(_prismShardTex, pos, null, drawColor, p.Rotation,
                        shardOrigin, p.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            resonanceRings.Clear();
            prismShards.Clear();
            latticeNodes.Clear();
            _ghostTrail?.Clear();
        }
    }
}
