using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class CharmTrail : ITrail
    {
        public class HeartPulseParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float PulsePhase;
            public float PulseSpeed;
            public float FloatAngle;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 4f) * (1f - Progress) * (0.7f + 0.3f * MathF.Sin(PulsePhase)));
            public float CurrentScale => Scale * (1f + 0.3f * MathF.Sin(PulsePhase));

            public HeartPulseParticle(Vector2 pos, Vector2 vel, int life, float scale, float pulseSpeed, float floatAngle, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Scale = scale; PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi); PulseSpeed = pulseSpeed;
                FloatAngle = floatAngle; Color = color;
            }
        }

        public class EnchantRingParticle
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
            public float PulsePhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 3f) * (1f - Progress) * 0.5f);
            public float CurrentRadius => Radius + (MaxRadius - Radius) * MathF.Min(1f, Progress * ExpandSpeed);
            public float CurrentWidth => Width * (1f - Progress * 0.4f);

            public EnchantRingParticle(Vector2 pos, int life, float radius, float maxRadius, float width, float expandSpeed, float rotSpeed, Color color)
            {
                Position = pos; MaxLife = life; Life = life; Radius = radius; MaxRadius = maxRadius;
                Width = width; ExpandSpeed = expandSpeed; Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed; PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi); Color = color;
            }
        }

        public class LureMistParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float DriftAngle;
            public float WobblePhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 2f) * (1f - Progress * Progress) * 0.35f);
            public float CurrentScale => Scale * (1f + Progress * 0.8f);

            public LureMistParticle(Vector2 pos, Vector2 vel, int life, float scale, float driftAngle, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Scale = scale; DriftAngle = driftAngle; WobblePhase = Main.rand.NextFloat(MathHelper.TwoPi); Color = color;
            }
        }

        public string Name { get; set; } = "CharmTrail";
        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.18f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(255, 180, 210, 140);

        public int MaxHearts { get; set; } = 10;
        public int HeartLife { get; set; } = 35;
        public float HeartSize { get; set; } = 0.5f;
        public float HeartSpawnChance { get; set; } = 0.06f;
        public float HeartPulseSpeed { get; set; } = 0.08f;
        public float HeartDriftSpeed { get; set; } = 0.2f;
        public Color HeartColor { get; set; } = new Color(255, 140, 180, 220);

        public int MaxRings { get; set; } = 5;
        public int RingLife { get; set; } = 45;
        public float RingStartRadius { get; set; } = 2f;
        public float RingEndRadius { get; set; } = 25f;
        public float RingWidth { get; set; } = 0.3f;
        public float RingSpawnChance { get; set; } = 0.02f;
        public float RingExpandSpeed { get; set; } = 2f;
        public float RingRotSpeed { get; set; } = 0.03f;
        public Color RingColor { get; set; } = new Color(255, 160, 200, 180);

        public int MaxMists { get; set; } = 15;
        public int MistLife { get; set; } = 50;
        public float MistSize { get; set; } = 0.6f;
        public float MistSpawnChance { get; set; } = 0.08f;
        public float MistDriftSpeed { get; set; } = 0.1f;
        public Color MistColor { get; set; } = new Color(255, 180, 220, 160);

        public float InertiaFactor { get; set; } = 0.1f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<HeartPulseParticle> hearts = new();
        private List<EnchantRingParticle> rings = new();
        private List<LureMistParticle> mists = new();
        private GhostTrail _ghostTrail;
        private Texture2D _heartTex, _ringTex, _mistTex, _ghostTex;

        public bool HasContent => hearts.Count > 0 || rings.Count > 0 || mists.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_heartTex != null) return;
            _heartTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/CharmTrail/CharmTrailHeart").Value;
            _ringTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/CharmTrail/CharmTrailRing").Value;
            _mistTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/CharmTrail/CharmTrailMist").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/CharmTrail/CharmTrailGhost").Value;
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

            if (hearts.Count < MaxHearts && Main.rand.NextFloat() < HeartSpawnChance)
                SpawnHeart(center, velocity, moveDir);
            if (rings.Count < MaxRings && Main.rand.NextFloat() < RingSpawnChance)
                SpawnRing(center, velocity, moveDir);
            if (mists.Count < MaxMists && Main.rand.NextFloat() < MistSpawnChance)
                SpawnMist(center, velocity, moveDir);

            for (int i = hearts.Count - 1; i >= 0; i--)
            {
                var h = hearts[i];
                h.PulsePhase += h.PulseSpeed;
                h.Velocity *= 0.97f; h.Velocity.Y -= 0.005f;
                h.Position += h.Velocity; h.Life--;
                if (h.Life <= 0) hearts.RemoveAt(i);
            }
            for (int i = rings.Count - 1; i >= 0; i--)
            {
                var r = rings[i];
                r.Rotation += r.RotSpeed; r.PulsePhase += 0.05f;
                r.Position += (center - r.Position) * 0.02f; r.Life--;
                if (r.Life <= 0) rings.RemoveAt(i);
            }
            for (int i = mists.Count - 1; i >= 0; i--)
            {
                var m = mists[i];
                m.WobblePhase += 0.03f;
                Vector2 wobble = new Vector2(MathF.Sin(m.WobblePhase), MathF.Cos(m.WobblePhase * 0.7f)) * 0.1f;
                m.Velocity *= 0.99f; m.Position += m.Velocity + wobble; m.Life--;
                if (m.Life <= 0) mists.RemoveAt(i);
            }
        }

        private void SpawnHeart(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(4f, 4f);
            Vector2 vel = -velocity * InertiaFactor * 0.2f + Main.rand.NextVector2Circular(HeartDriftSpeed, HeartDriftSpeed) + new Vector2(0f, -0.1f);
            float scale = Main.rand.NextFloat(0.6f, 1.3f) * HeartSize;
            float pulseSpeed = HeartPulseSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float floatAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            Color color = HeartColor * Main.rand.NextFloat(0.6f, 1f);
            hearts.Add(new HeartPulseParticle(pos, vel, HeartLife, scale, pulseSpeed, floatAngle, color));
        }

        private void SpawnRing(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(3f, 3f);
            float endRadius = RingEndRadius * Main.rand.NextFloat(0.7f, 1.3f);
            float width = RingWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float expandSpeed = RingExpandSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float rotSpeed = RingRotSpeed * Main.rand.NextFloat(0.5f, 1.5f) * (Main.rand.NextBool() ? 1f : -1f);
            Color color = RingColor * Main.rand.NextFloat(0.5f, 1f);
            rings.Add(new EnchantRingParticle(pos, RingLife, RingStartRadius, endRadius, width, expandSpeed, rotSpeed, color));
        }

        private void SpawnMist(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(6f, 6f);
            Vector2 vel = Main.rand.NextVector2Circular(MistDriftSpeed, MistDriftSpeed) + new Vector2(0f, -0.05f);
            float scale = Main.rand.NextFloat(0.5f, 1.5f) * MistSize;
            float driftAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            Color color = MistColor * Main.rand.NextFloat(0.3f, 0.7f);
            mists.Add(new LureMistParticle(pos, vel, MistLife, scale, driftAngle, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null) _ghostTrail.Draw(sb);

            if (mists.Count > 0 && _mistTex != null)
            {
                Vector2 mistOrigin = _mistTex.Size() * 0.5f;
                foreach (var m in mists.OrderBy(x => x.Life))
                {
                    Color drawColor = m.Color * m.Alpha;
                    Vector2 pos = m.Position - Main.screenPosition;
                    sb.Draw(_mistTex, pos, null, drawColor, m.DriftAngle, mistOrigin, m.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (rings.Count > 0 && _ringTex != null)
            {
                Vector2 ringOrigin = _ringTex.Size() * 0.5f;
                foreach (var r in rings.OrderBy(x => x.Life))
                {
                    Color drawColor = r.Color * r.Alpha;
                    Vector2 pos = r.Position - Main.screenPosition;
                    float scaleX = r.CurrentRadius / (_ringTex.Width * 0.5f);
                    float scaleY = r.CurrentWidth;
                    sb.Draw(_ringTex, pos, null, drawColor, r.Rotation, ringOrigin, new Vector2(scaleX, scaleY), SpriteEffects.None, 0);
                }
            }

            if (hearts.Count > 0 && _heartTex != null)
            {
                Vector2 heartOrigin = _heartTex.Size() * 0.5f;
                foreach (var h in hearts.OrderBy(x => x.Life))
                {
                    Color drawColor = h.Color * h.Alpha;
                    Vector2 pos = h.Position - Main.screenPosition;
                    sb.Draw(_heartTex, pos, null, drawColor, h.FloatAngle, heartOrigin, h.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            hearts.Clear(); rings.Clear(); mists.Clear();
            _ghostTrail?.Clear();
        }
    }
}