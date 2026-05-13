using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class DreamTrail : ITrail
    {
        public class BubbleDreamParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float WobblePhase;
            public float WobbleSpeed;
            public float ExpandSpeed;
            public float Iridescence;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 3f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float pop = Progress > 0.85f ? (1f - (Progress - 0.85f) / 0.15f) : 1f;
                    return MathF.Max(0f, fadeIn * fadeOut * pop * 0.4f);
                }
            }

            public float CurrentScale
            {
                get
                {
                    float expand = MathF.Min(1f, Progress * ExpandSpeed);
                    float wobble = 1f + MathF.Sin(WobblePhase) * 0.05f;
                    return (Scale + (MaxScale - Scale) * expand) * wobble;
                }
            }

            public bool ShouldPop => Progress > 0.85f;

            public BubbleDreamParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float expandSpeed, float iridescence, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                ExpandSpeed = expandSpeed;
                WobblePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                WobbleSpeed = Main.rand.NextFloat(0.03f, 0.08f);
                Iridescence = iridescence;
                Color = color;
            }
        }

        public class RippleWaveParticle
        {
            public Vector2 Position;
            public float Radius;
            public float MaxRadius;
            public float Width;
            public int Life;
            public int MaxLife;
            public float ExpandSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.5f);
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

            public float CurrentWidth => Width * (1f - Progress * 0.6f);

            public RippleWaveParticle(Vector2 pos, int life, float radius, float maxRadius, float width, float expandSpeed, Color color)
            {
                Position = pos;
                MaxLife = life;
                Life = life;
                Radius = radius;
                MaxRadius = maxRadius;
                Width = width;
                ExpandSpeed = expandSpeed;
                Color = color;
            }
        }

        public class PhantomButterflyParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float WingPhase;
            public float WingSpeed;
            public float WanderAngle;
            public float WanderSpeed;
            public float DissolveSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float dissolve = MathF.Max(0f, 1f - Progress * DissolveSpeed);
                    return MathF.Max(0f, fadeIn * dissolve * 0.65f);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.2f);

            public float WingOpen => 0.3f + 0.7f * MathF.Abs(MathF.Sin(WingPhase));

            public PhantomButterflyParticle(Vector2 pos, Vector2 vel, int life, float scale, float wingSpeed, float dissolveSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                WingPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                WingSpeed = wingSpeed;
                WanderAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                WanderSpeed = Main.rand.NextFloat(0.02f, 0.05f);
                DissolveSpeed = dissolveSpeed;
                Color = color;
            }
        }

        public string Name { get; set; } = "DreamTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(200, 150, 220, 140);

        public int MaxBubbles { get; set; } = 10;
        public int BubbleLife { get; set; } = 55;
        public float BubbleStartSize { get; set; } = 0.3f;
        public float BubbleEndSize { get; set; } = 1.6f;
        public float BubbleSpawnChance { get; set; } = 0.025f;
        public float BubbleExpandSpeed { get; set; } = 1.5f;
        public float BubbleDriftSpeed { get; set; } = 0.08f;
        public float BubbleIridescence { get; set; } = 0.7f;
        public Color BubbleColor { get; set; } = new Color(200, 160, 240, 200);

        public int MaxRipples { get; set; } = 6;
        public int RippleLife { get; set; } = 40;
        public float RippleStartRadius { get; set; } = 2f;
        public float RippleEndRadius { get; set; } = 30f;
        public float RippleWidth { get; set; } = 0.4f;
        public float RippleSpawnChance { get; set; } = 0.02f;
        public float RippleExpandSpeed { get; set; } = 2f;
        public Color RippleColor { get; set; } = new Color(180, 140, 220, 180);

        public int MaxButterflies { get; set; } = 8;
        public int ButterflyLife { get; set; } = 50;
        public float ButterflySize { get; set; } = 0.6f;
        public float ButterflySpawnChance { get; set; } = 0.03f;
        public float ButterflyWingSpeed { get; set; } = 0.12f;
        public float ButterflyDissolveSpeed { get; set; } = 1.2f;
        public float ButterflyDriftSpeed { get; set; } = 0.15f;
        public Color ButterflyColor { get; set; } = new Color(220, 180, 255, 220);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<BubbleDreamParticle> bubbles = new();
        private List<RippleWaveParticle> ripples = new();
        private List<PhantomButterflyParticle> butterflies = new();

        private GhostTrail _ghostTrail;

        private Texture2D _bubbleTex;
        private Texture2D _rippleTex;
        private Texture2D _butterflyTex;
        private Texture2D _ghostTex;

        private Vector2 _lastCenter;

        public bool HasContent => bubbles.Count > 0 || ripples.Count > 0 || butterflies.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_bubbleTex != null) return;
            _bubbleTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/DreamTrail/DreamTrailBubble").Value;
            _rippleTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/DreamTrail/DreamTrailRipple").Value;
            _butterflyTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/DreamTrail/DreamTrailButterfly").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/DreamTrail/DreamTrailGhost").Value;
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

            if (bubbles.Count < MaxBubbles && Main.rand.NextFloat() < BubbleSpawnChance)
                SpawnBubble(center, velocity, moveDir);

            if (ripples.Count < MaxRipples && Main.rand.NextFloat() < RippleSpawnChance)
                SpawnRipple(center, velocity, moveDir);

            if (butterflies.Count < MaxButterflies && Main.rand.NextFloat() < ButterflySpawnChance)
                SpawnButterfly(center, velocity, moveDir);

            for (int i = bubbles.Count - 1; i >= 0; i--)
            {
                var b = bubbles[i];
                b.WobblePhase += b.WobbleSpeed;
                b.Velocity *= 0.98f;
                b.Velocity.Y -= 0.01f;
                b.Position += b.Velocity;
                b.Life--;
                if (b.Life <= 0) bubbles.RemoveAt(i);
            }

            for (int i = ripples.Count - 1; i >= 0; i--)
            {
                var r = ripples[i];
                r.Position += (_lastCenter - r.Position) * 0.02f;
                r.Life--;
                if (r.Life <= 0) ripples.RemoveAt(i);
            }

            for (int i = butterflies.Count - 1; i >= 0; i--)
            {
                var bf = butterflies[i];
                bf.WingPhase += bf.WingSpeed;
                bf.WanderAngle += bf.WanderSpeed;
                Vector2 wander = new Vector2(MathF.Cos(bf.WanderAngle), MathF.Sin(bf.WanderAngle)) * 0.3f;
                bf.Velocity = bf.Velocity * 0.96f + wander * 0.04f;
                bf.Velocity.Y -= 0.01f;
                bf.Position += bf.Velocity;
                bf.Life--;
                if (bf.Life <= 0) butterflies.RemoveAt(i);
            }
        }

        private void SpawnBubble(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(8f, 8f);
            Vector2 drift = Main.rand.NextVector2Circular(BubbleDriftSpeed, BubbleDriftSpeed);
            Vector2 vel = drift + new Vector2(0f, -0.1f);
            float startSize = BubbleStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = BubbleEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            float expandSpeed = BubbleExpandSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float iridescence = BubbleIridescence * Main.rand.NextFloat(0.5f, 1f);
            Color color = BubbleColor * Main.rand.NextFloat(0.5f, 1f);
            bubbles.Add(new BubbleDreamParticle(pos, vel, BubbleLife, startSize, endSize, expandSpeed, iridescence, color));
        }

        private void SpawnRipple(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(4f, 4f);
            float endRadius = RippleEndRadius * Main.rand.NextFloat(0.7f, 1.3f);
            float width = RippleWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float expandSpeed = RippleExpandSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = RippleColor * Main.rand.NextFloat(0.5f, 1f);
            ripples.Add(new RippleWaveParticle(pos, RippleLife, RippleStartRadius, endRadius, width, expandSpeed, color));
        }

        private void SpawnButterfly(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(6f, 6f);
            Vector2 inertia = -velocity * InertiaFactor * 0.1f;
            Vector2 drift = Main.rand.NextVector2Circular(ButterflyDriftSpeed, ButterflyDriftSpeed);
            Vector2 vel = inertia + drift + new Vector2(0f, -0.2f);
            float scale = ButterflySize * Main.rand.NextFloat(0.6f, 1.4f);
            float wingSpeed = ButterflyWingSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float dissolveSpeed = ButterflyDissolveSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = ButterflyColor * Main.rand.NextFloat(0.5f, 1f);
            butterflies.Add(new PhantomButterflyParticle(pos, vel, ButterflyLife, scale, wingSpeed, dissolveSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (ripples.Count > 0 && _rippleTex != null)
            {
                Vector2 rippleOrigin = _rippleTex.Size() * 0.5f;
                var sortedRipples = ripples.OrderBy(r => r.Life);
                foreach (var r in sortedRipples)
                {
                    Color drawColor = r.Color * r.Alpha;
                    Vector2 pos = r.Position - Main.screenPosition;
                    float scaleX = r.CurrentRadius / (_rippleTex.Width * 0.5f);
                    float scaleY = r.CurrentWidth;
                    Vector2 scale = new Vector2(scaleX, scaleY);
                    sb.Draw(_rippleTex, pos, null, drawColor, 0f,
                        rippleOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (bubbles.Count > 0 && _bubbleTex != null)
            {
                Vector2 bubbleOrigin = _bubbleTex.Size() * 0.5f;
                var sortedBubbles = bubbles.OrderBy(b => b.Life);
                foreach (var b in sortedBubbles)
                {
                    float hueShift = b.Progress * b.Iridescence;
                    float hue = (0.75f + hueShift) % 1f;
                    Color iridescent = Main.hslToRgb(hue, 0.4f, 0.8f);
                    Color drawColor = Color.Lerp(iridescent, b.Color, 0.6f) * b.Alpha;
                    Vector2 pos = b.Position - Main.screenPosition;
                    sb.Draw(_bubbleTex, pos, null, drawColor, b.WobblePhase * 0.5f,
                        bubbleOrigin, b.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (butterflies.Count > 0 && _butterflyTex != null)
            {
                Vector2 bfOrigin = _butterflyTex.Size() * 0.5f;
                var sortedBF = butterflies.OrderBy(bf => bf.Life);
                foreach (var bf in sortedBF)
                {
                    Color drawColor = bf.Color * bf.Alpha;
                    Vector2 pos = bf.Position - Main.screenPosition;
                    float scaleX = bf.CurrentScale * bf.WingOpen;
                    float scaleY = bf.CurrentScale;
                    Vector2 scale = new Vector2(scaleX, scaleY);
                    float angle = bf.WanderAngle;
                    sb.Draw(_butterflyTex, pos, null, drawColor, angle,
                        bfOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            bubbles.Clear();
            ripples.Clear();
            butterflies.Clear();
            _ghostTrail?.Clear();
        }
    }
}
