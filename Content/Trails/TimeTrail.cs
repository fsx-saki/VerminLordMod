using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class TimeTrail : ITrail
    {
        public class HourglassGrainParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Gravity;
            public float TrailAlpha;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 8f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.7f);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.3f);

            public HourglassGrainParticle(Vector2 pos, Vector2 vel, int life, float scale, float gravity, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Gravity = gravity;
                TrailAlpha = 1f;
                Color = color;
            }
        }

        public class ClockHandParticle
        {
            public Vector2 Position;
            public float Length;
            public float Angle;
            public float AngularSpeed;
            public float Width;
            public int Life;
            public int MaxLife;
            public int Depth;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.6f);
                }
            }

            public float CurrentLength => Length * MathF.Min(1f, Progress * 3f);

            public float CurrentWidth => Width * (1f - Depth * 0.3f) * (1f - Progress * 0.5f);

            public ClockHandParticle(Vector2 pos, float length, float angle, float angularSpeed, float width, int life, int depth, Color color)
            {
                Position = pos;
                Length = length;
                Angle = angle;
                AngularSpeed = angularSpeed;
                Width = width;
                MaxLife = life;
                Life = life;
                Depth = depth;
                Color = color;
            }
        }

        public class AfterimageParticle
        {
            public Vector2 Position;
            public float Scale;
            public float Rotation;
            public int Life;
            public int MaxLife;
            public float InitialAlpha;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeOut = 1f - Progress;
                    return MathF.Max(0f, fadeOut * fadeOut * InitialAlpha);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.2f);

            public AfterimageParticle(Vector2 pos, float scale, float rotation, int life, float initialAlpha, Color color)
            {
                Position = pos;
                Scale = scale;
                Rotation = rotation;
                MaxLife = life;
                Life = life;
                InitialAlpha = initialAlpha;
                Color = color;
            }
        }

        public string Name { get; set; } = "TimeTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 12;
        public int GhostRecordInterval { get; set; } = 3;
        public float GhostWidthScale { get; set; } = 0.15f;
        public float GhostLengthScale { get; set; } = 1.6f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(200, 180, 100, 140);

        public int MaxGrains { get; set; } = 40;
        public int GrainLife { get; set; } = 30;
        public float GrainSize { get; set; } = 0.3f;
        public float GrainSpawnChance { get; set; } = 0.15f;
        public float GrainGravity { get; set; } = 0.12f;
        public float GrainSpread { get; set; } = 4f;
        public float GrainSpeed { get; set; } = 0.3f;
        public Color GrainColor { get; set; } = new Color(220, 190, 100, 220);

        public int MaxClockHands { get; set; } = 8;
        public int ClockHandLife { get; set; } = 45;
        public float ClockHandLength { get; set; } = 14f;
        public float ClockHandWidth { get; set; } = 0.5f;
        public float ClockHandSpawnChance { get; set; } = 0.04f;
        public float ClockHandBaseSpeed { get; set; } = 0.08f;
        public int ClockHandMaxDepth { get; set; } = 2;
        public Color ClockHandColor { get; set; } = new Color(200, 170, 80, 200);

        public int MaxAfterimages { get; set; } = 8;
        public int AfterimageLife { get; set; } = 20;
        public float AfterimageSize { get; set; } = 0.8f;
        public int AfterimageRecordInterval { get; set; } = 4;
        public float AfterimageAlpha { get; set; } = 0.5f;
        public Color AfterimageColor { get; set; } = new Color(180, 160, 80, 180);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<HourglassGrainParticle> grains = new();
        private List<ClockHandParticle> clockHands = new();
        private List<AfterimageParticle> afterimages = new();
        private int afterimageCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _grainTex;
        private Texture2D _clockHandTex;
        private Texture2D _afterimageTex;
        private Texture2D _ghostTex;

        private Vector2 _lastCenter;
        private float _lastRotation;

        public bool HasContent => grains.Count > 0 || clockHands.Count > 0 || afterimages.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_grainTex != null) return;
            _grainTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/TimeTrail/TimeTrailGrain").Value;
            _clockHandTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/TimeTrail/TimeTrailHand").Value;
            _afterimageTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/TimeTrail/TimeTrailAfterimage").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/TimeTrail/TimeTrailGhost").Value;
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
            _lastRotation = velocity.ToRotation();

            if (_ghostTrail != null)
                _ghostTrail.Update(center, velocity);

            Vector2 moveDir = velocity.SafeNormalize(Vector2.UnitX);

            if (grains.Count < MaxGrains && Main.rand.NextFloat() < GrainSpawnChance)
                SpawnGrain(center, velocity, moveDir);

            if (clockHands.Count < MaxClockHands && Main.rand.NextFloat() < ClockHandSpawnChance)
                SpawnClockHand(center, velocity, moveDir, 0);

            afterimageCounter++;
            if (afterimageCounter >= AfterimageRecordInterval && afterimages.Count < MaxAfterimages)
            {
                afterimageCounter = 0;
                afterimages.Add(new AfterimageParticle(
                    center + SpawnOffset,
                    AfterimageSize,
                    _lastRotation,
                    AfterimageLife,
                    AfterimageAlpha,
                    AfterimageColor * Main.rand.NextFloat(0.5f, 1f)
                ));
            }

            for (int i = grains.Count - 1; i >= 0; i--)
            {
                var g = grains[i];
                g.Velocity.Y += GrainGravity;
                g.Velocity.X *= 0.99f;
                g.Position += g.Velocity;
                g.Life--;
                if (g.Life <= 0) grains.RemoveAt(i);
            }

            for (int i = clockHands.Count - 1; i >= 0; i--)
            {
                var h = clockHands[i];
                h.Angle += h.AngularSpeed;
                h.Position += (_lastCenter - h.Position) * 0.03f;
                h.Life--;
                if (h.Life <= 0) clockHands.RemoveAt(i);
            }

            for (int i = afterimages.Count - 1; i >= 0; i--)
            {
                var a = afterimages[i];
                a.Life--;
                if (a.Life <= 0) afterimages.RemoveAt(i);
            }
        }

        private void SpawnGrain(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-GrainSpread, GrainSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);
            Vector2 inertia = -velocity * InertiaFactor * 0.15f;
            Vector2 drift = Main.rand.NextVector2Circular(GrainSpeed, GrainSpeed);
            Vector2 vel = inertia + drift;
            float scale = GrainSize * Main.rand.NextFloat(0.5f, 1.5f);
            Color color = GrainColor * Main.rand.NextFloat(0.5f, 1f);
            grains.Add(new HourglassGrainParticle(pos, vel, GrainLife, scale, GrainGravity, color));
        }

        private void SpawnClockHand(Vector2 center, Vector2 velocity, Vector2 moveDir, int depth)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(5f, 5f);
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float speed = ClockHandBaseSpeed * Main.rand.NextFloat(0.5f, 2f) / (1f + depth * 0.5f);
            if (Main.rand.NextBool()) speed = -speed;
            float length = ClockHandLength * Main.rand.NextFloat(0.6f, 1.2f) / (1f + depth * 0.4f);
            float width = ClockHandWidth * Main.rand.NextFloat(0.7f, 1.3f);
            int life = ClockHandLife - depth * 8;
            Color color = ClockHandColor * Main.rand.NextFloat(0.5f, 1f);
            clockHands.Add(new ClockHandParticle(pos, length, angle, speed, width, life, depth, color));

            if (depth < ClockHandMaxDepth && Main.rand.NextFloat() < 0.5f)
            {
                float subAngle = angle + Main.rand.NextFloat(-0.5f, 0.5f);
                float subSpeed = speed * Main.rand.NextFloat(1.5f, 3f);
                clockHands.Add(new ClockHandParticle(pos, length * 0.5f, subAngle, subSpeed, width * 0.7f, life - 5, depth + 1, color * 0.8f));
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (afterimages.Count > 0 && _afterimageTex != null)
            {
                Vector2 afterOrigin = _afterimageTex.Size() * 0.5f;
                var sortedAfter = afterimages.OrderBy(a => a.Life);
                foreach (var a in sortedAfter)
                {
                    Color drawColor = a.Color * a.Alpha;
                    Vector2 pos = a.Position - Main.screenPosition;
                    sb.Draw(_afterimageTex, pos, null, drawColor, a.Rotation,
                        afterOrigin, a.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (grains.Count > 0 && _grainTex != null)
            {
                Vector2 grainOrigin = _grainTex.Size() * 0.5f;
                var sortedGrains = grains.OrderBy(g => g.Life);
                foreach (var g in sortedGrains)
                {
                    Color drawColor = g.Color * g.Alpha;
                    Vector2 pos = g.Position - Main.screenPosition;
                    float angle = g.Velocity.Length() > 0.1f ? g.Velocity.ToRotation() : 0f;
                    Vector2 scale = new Vector2(g.CurrentScale * 1.5f, g.CurrentScale * 0.6f);
                    sb.Draw(_grainTex, pos, null, drawColor, angle,
                        grainOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (clockHands.Count > 0 && _clockHandTex != null)
            {
                Vector2 handOrigin = new Vector2(0f, _clockHandTex.Height * 0.5f);
                var sortedHands = clockHands.OrderBy(h => h.Depth).ThenBy(h => h.Life);
                foreach (var h in sortedHands)
                {
                    Color drawColor = h.Color * h.Alpha;
                    Vector2 pos = h.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(h.CurrentLength / _clockHandTex.Width, h.CurrentWidth);
                    sb.Draw(_clockHandTex, pos, null, drawColor, h.Angle,
                        handOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            grains.Clear();
            clockHands.Clear();
            afterimages.Clear();
            afterimageCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
