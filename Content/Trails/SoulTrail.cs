using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class SoulTrail : ITrail
    {
        public class SoulFlameParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float FlickerPhase;
            public float FlickerSpeed;
            public float WanderAngle;
            public float WanderSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float flicker = 0.7f + 0.3f * MathF.Sin(FlickerPhase);
                    return MathF.Max(0f, fadeIn * fadeOut * flicker * 0.65f);
                }
            }

            public float CurrentScale
            {
                get
                {
                    float flicker = 0.85f + 0.15f * MathF.Sin(FlickerPhase * 1.3f);
                    return Scale * (1f - Progress * 0.3f) * flicker;
                }
            }

            public SoulFlameParticle(Vector2 pos, Vector2 vel, int life, float scale, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                FlickerPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                FlickerSpeed = Main.rand.NextFloat(0.12f, 0.25f);
                WanderAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                WanderSpeed = Main.rand.NextFloat(0.02f, 0.06f);
                Color = color;
            }
        }

        public class EtherealChainParticle
        {
            public Vector2 Start;
            public Vector2 End;
            public Vector2 EndVelocity;
            public float Width;
            public int Life;
            public int MaxLife;
            public int SegmentCount;
            public float DragStrength;
            public float PulsePhase;
            public float PulseSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 3f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float pulse = 0.6f + 0.4f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, fadeIn * fadeOut * pulse * 0.55f);
                }
            }

            public float CurrentWidth => Width * (1f - Progress * 0.4f);

            public EtherealChainParticle(Vector2 start, Vector2 end, int life, float width, int segmentCount, float dragStrength, Color color)
            {
                Start = start;
                End = end;
                MaxLife = life;
                Life = life;
                Width = width;
                SegmentCount = segmentCount;
                DragStrength = dragStrength;
                EndVelocity = Vector2.Zero;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = Main.rand.NextFloat(0.06f, 0.12f);
                Color = color;
            }
        }

        public class WillOWispParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float LeadAngle;
            public float LeadSpeed;
            public float OrbitRadius;
            public float OrbitSpeed;
            public float OrbitPhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.8f);
                }
            }

            public float CurrentScale
            {
                get
                {
                    float grow = MathF.Min(1f, Progress * 3f);
                    return Scale + (MaxScale - Scale) * grow;
                }
            }

            public WillOWispParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float leadAngle, float orbitRadius, float orbitSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                LeadAngle = leadAngle;
                LeadSpeed = Main.rand.NextFloat(0.03f, 0.07f);
                OrbitRadius = orbitRadius;
                OrbitSpeed = orbitSpeed;
                OrbitPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public string Name { get; set; } = "SoulTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(120, 180, 255, 160);

        public int MaxFlames { get; set; } = 18;
        public int FlameLife { get; set; } = 40;
        public float FlameSize { get; set; } = 0.6f;
        public float FlameSpawnChance { get; set; } = 0.08f;
        public float FlameWanderAmp { get; set; } = 0.5f;
        public float FlameDriftSpeed { get; set; } = 0.12f;
        public Color FlameColor { get; set; } = new Color(140, 200, 255, 230);

        public int MaxChains { get; set; } = 6;
        public int ChainLife { get; set; } = 35;
        public float ChainWidth { get; set; } = 0.4f;
        public float ChainSpawnChance { get; set; } = 0.03f;
        public int ChainSegmentCount { get; set; } = 6;
        public float ChainDragStrength { get; set; } = 0.05f;
        public float ChainReach { get; set; } = 22f;
        public Color ChainColor { get; set; } = new Color(100, 160, 240, 200);

        public int MaxWisps { get; set; } = 5;
        public int WispLife { get; set; } = 50;
        public float WispStartSize { get; set; } = 0.3f;
        public float WispEndSize { get; set; } = 0.8f;
        public float WispSpawnChance { get; set; } = 0.015f;
        public float WispOrbitRadius { get; set; } = 20f;
        public float WispOrbitSpeed { get; set; } = 0.06f;
        public float WispLeadSpeed { get; set; } = 2f;
        public Color WispColor { get; set; } = new Color(180, 220, 255, 240);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<SoulFlameParticle> flames = new();
        private List<EtherealChainParticle> chains = new();
        private List<WillOWispParticle> wisps = new();

        private GhostTrail _ghostTrail;

        private Texture2D _flameTex;
        private Texture2D _chainTex;
        private Texture2D _wispTex;
        private Texture2D _ghostTex;

        private Vector2 _lastCenter;
        private Vector2 _lastVelocity;

        public bool HasContent => flames.Count > 0 || chains.Count > 0 || wisps.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_flameTex != null) return;
            _flameTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SoulTrail/SoulTrailFlame").Value;
            _chainTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SoulTrail/SoulTrailChain").Value;
            _wispTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SoulTrail/SoulTrailWisp").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/SoulTrail/SoulTrailGhost").Value;
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
            _lastVelocity = velocity;

            if (_ghostTrail != null)
                _ghostTrail.Update(center, velocity);

            Vector2 moveDir = velocity.SafeNormalize(Vector2.UnitX);

            if (flames.Count < MaxFlames && Main.rand.NextFloat() < FlameSpawnChance)
                SpawnFlame(center, velocity, moveDir);

            if (chains.Count < MaxChains && Main.rand.NextFloat() < ChainSpawnChance)
                SpawnChain(center, velocity, moveDir);

            if (wisps.Count < MaxWisps && Main.rand.NextFloat() < WispSpawnChance)
                SpawnWisp(center, velocity, moveDir);

            for (int i = flames.Count - 1; i >= 0; i--)
            {
                var f = flames[i];
                f.FlickerPhase += f.FlickerSpeed;
                f.WanderAngle += f.WanderSpeed;
                Vector2 wander = new Vector2(MathF.Cos(f.WanderAngle), MathF.Sin(f.WanderAngle)) * FlameWanderAmp;
                f.Velocity = f.Velocity * 0.95f + wander * 0.05f;
                f.Velocity.Y -= 0.02f;
                f.Position += f.Velocity;
                f.Life--;
                if (f.Life <= 0) flames.RemoveAt(i);
            }

            for (int i = chains.Count - 1; i >= 0; i--)
            {
                var c = chains[i];
                c.PulsePhase += c.PulseSpeed;
                Vector2 toCenter = _lastCenter - c.End;
                c.EndVelocity += toCenter * c.DragStrength;
                c.EndVelocity *= 0.9f;
                c.End += c.EndVelocity;
                c.Start += (_lastCenter - c.Start) * 0.04f;
                c.Life--;
                if (c.Life <= 0) chains.RemoveAt(i);
            }

            for (int i = wisps.Count - 1; i >= 0; i--)
            {
                var w = wisps[i];
                w.OrbitPhase += w.OrbitSpeed;
                w.LeadAngle += w.LeadSpeed;
                Vector2 leadOffset = new Vector2(MathF.Cos(w.LeadAngle), MathF.Sin(w.LeadAngle)) * w.OrbitRadius;
                Vector2 target = _lastCenter + _lastVelocity.SafeNormalize(Vector2.UnitX) * WispLeadSpeed + leadOffset;
                Vector2 toTarget = target - w.Position;
                w.Velocity = w.Velocity * 0.9f + toTarget * 0.1f;
                w.Position += w.Velocity;
                w.Life--;
                if (w.Life <= 0) wisps.RemoveAt(i);
            }
        }

        private void SpawnFlame(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(6f, 6f);
            Vector2 inertia = -velocity * InertiaFactor * 0.2f;
            Vector2 drift = Main.rand.NextVector2Circular(FlameDriftSpeed, FlameDriftSpeed);
            Vector2 vel = inertia + drift + new Vector2(0f, -0.3f);
            float scale = FlameSize * Main.rand.NextFloat(0.6f, 1.4f);
            Color color = FlameColor * Main.rand.NextFloat(0.5f, 1f);
            flames.Add(new SoulFlameParticle(pos, vel, FlameLife, scale, color));
        }

        private void SpawnChain(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 start = center + SpawnOffset + Main.rand.NextVector2Circular(3f, 3f);
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 end = start + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * ChainReach * Main.rand.NextFloat(0.4f, 1f);
            float width = ChainWidth * Main.rand.NextFloat(0.7f, 1.3f);
            int segmentCount = Main.rand.Next(3, ChainSegmentCount + 1);
            float dragStrength = ChainDragStrength * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = ChainColor * Main.rand.NextFloat(0.5f, 1f);
            chains.Add(new EtherealChainParticle(start, end, ChainLife, width, segmentCount, dragStrength, color));
        }

        private void SpawnWisp(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(5f, 5f);
            float leadAngle = velocity.ToRotation() + Main.rand.NextFloat(-0.8f, 0.8f);
            float orbitRadius = WispOrbitRadius * Main.rand.NextFloat(0.6f, 1.4f);
            float orbitSpeed = WispOrbitSpeed * Main.rand.NextFloat(0.7f, 1.3f) * (Main.rand.NextBool() ? 1f : -1f);
            float startSize = WispStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = WispEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = WispColor * Main.rand.NextFloat(0.6f, 1f);
            wisps.Add(new WillOWispParticle(pos, velocity * 0.3f, WispLife, startSize, endSize, leadAngle, orbitRadius, orbitSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

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
                    float segLength = totalLength / c.SegmentCount;

                    for (int i = 0; i < c.SegmentCount; i++)
                    {
                        float t = i / (float)c.SegmentCount;
                        Vector2 segPos = Vector2.Lerp(c.Start, c.End, t);
                        float wave = MathF.Sin(t * MathHelper.Pi * 2f + c.PulsePhase) * 4f;
                        Vector2 perp = new Vector2(-MathF.Sin(baseAngle), MathF.Cos(baseAngle)) * wave;
                        segPos += perp;

                        Color drawColor = c.Color * c.Alpha;
                        Vector2 pos = segPos - Main.screenPosition;
                        Vector2 scale = new Vector2(segLength / _chainTex.Width, c.CurrentWidth);
                        float angle = baseAngle + MathF.Sin(t * MathHelper.Pi * 2f + c.PulsePhase) * 0.15f;
                        sb.Draw(_chainTex, pos, null, drawColor, angle,
                            chainOrigin, scale, SpriteEffects.None, 0);
                    }
                }
            }

            if (flames.Count > 0 && _flameTex != null)
            {
                Vector2 flameOrigin = _flameTex.Size() * 0.5f;
                var sortedFlames = flames.OrderBy(f => f.Life);
                foreach (var f in sortedFlames)
                {
                    Color drawColor = f.Color * f.Alpha;
                    Vector2 pos = f.Position - Main.screenPosition;
                    sb.Draw(_flameTex, pos, null, drawColor, f.WanderAngle * 0.3f,
                        flameOrigin, f.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (wisps.Count > 0 && _wispTex != null)
            {
                Vector2 wispOrigin = _wispTex.Size() * 0.5f;
                var sortedWisps = wisps.OrderBy(w => w.Life);
                foreach (var w in sortedWisps)
                {
                    Color drawColor = w.Color * w.Alpha;
                    Vector2 pos = w.Position - Main.screenPosition;
                    sb.Draw(_wispTex, pos, null, drawColor, w.OrbitPhase,
                        wispOrigin, w.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            flames.Clear();
            chains.Clear();
            wisps.Clear();
            _ghostTrail?.Clear();
        }
    }
}
