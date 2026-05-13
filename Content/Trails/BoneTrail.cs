using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class BoneTrail : ITrail
    {
        public class RibCageParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float ArcSpan;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float GrowSpeed;
            public float PulsePhase;
            public float PulseSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float growIn = MathF.Min(1f, Progress * GrowSpeed);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float pulse = 0.7f + 0.3f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, growIn * fadeOut * pulse * 0.55f);
                }
            }

            public float CurrentArc => ArcSpan * MathF.Min(1f, Progress * GrowSpeed);

            public RibCageParticle(Vector2 pos, Vector2 vel, int life, float scale, float arcSpan, float rotation, float growSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                ArcSpan = arcSpan;
                Rotation = rotation;
                GrowSpeed = growSpeed;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = Main.rand.NextFloat(0.05f, 0.12f);
                Color = color;
            }
        }

        public class MarrowGlowParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float PulsePhase;
            public float PulseSpeed;
            public float ExpandSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float pulse = 0.5f + 0.5f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, fadeIn * fadeOut * pulse * 0.5f);
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

            public MarrowGlowParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float expandSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                ExpandSpeed = expandSpeed;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = Main.rand.NextFloat(0.08f, 0.18f);
                Color = color;
            }
        }

        public class BoneSpikeParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Length;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpinSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 6f);
                    float fadeOut = 1f - Progress;
                    return MathF.Max(0f, fadeIn * fadeOut);
                }
            }

            public float CurrentLength => Length * MathF.Min(1f, Progress * 4f) * MathF.Max(0f, 1f - Progress * 0.3f);

            public BoneSpikeParticle(Vector2 pos, Vector2 vel, int life, float scale, float length, float rotation, float spinSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Length = length;
                Rotation = rotation;
                SpinSpeed = spinSpeed;
                Color = color;
            }
        }

        public string Name { get; set; } = "BoneTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(180, 175, 160, 140);

        public int MaxRibCages { get; set; } = 8;
        public int RibCageLife { get; set; } = 50;
        public float RibCageScale { get; set; } = 0.5f;
        public float RibCageArcSpan { get; set; } = MathHelper.Pi * 0.8f;
        public float RibCageSpawnChance { get; set; } = 0.04f;
        public float RibCageGrowSpeed { get; set; } = 2.5f;
        public float RibCageDriftSpeed { get; set; } = 0.06f;
        public Color RibCageColor { get; set; } = new Color(200, 195, 180, 200);

        public int MaxMarrowGlows { get; set; } = 20;
        public int MarrowGlowLife { get; set; } = 35;
        public float MarrowGlowStartSize { get; set; } = 0.2f;
        public float MarrowGlowEndSize { get; set; } = 1.2f;
        public float MarrowGlowSpawnChance { get; set; } = 0.15f;
        public float MarrowGlowExpandSpeed { get; set; } = 2.5f;
        public float MarrowGlowDriftSpeed { get; set; } = 0.1f;
        public Color MarrowGlowColor { get; set; } = new Color(140, 200, 180, 180);

        public int MaxBoneSpikes { get; set; } = 25;
        public int BoneSpikeLife { get; set; } = 22;
        public float BoneSpikeSize { get; set; } = 0.4f;
        public float BoneSpikeLength { get; set; } = 16f;
        public float BoneSpikeSpawnChance { get; set; } = 0.2f;
        public float BoneSpikeSpinSpeed { get; set; } = 0.08f;
        public float BoneSpikeDriftSpeed { get; set; } = 0.25f;
        public Color BoneSpikeColor { get; set; } = new Color(220, 215, 200, 220);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<RibCageParticle> ribCages = new();
        private List<MarrowGlowParticle> marrowGlows = new();
        private List<BoneSpikeParticle> boneSpikes = new();

        private GhostTrail _ghostTrail;

        private Texture2D _ribTex;
        private Texture2D _marrowTex;
        private Texture2D _spikeTex;
        private Texture2D _ghostTex;

        public bool HasContent => ribCages.Count > 0 || marrowGlows.Count > 0 || boneSpikes.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_ribTex != null) return;
            _ribTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/BoneTrail/BoneTrailRib").Value;
            _marrowTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/BoneTrail/BoneTrailMarrow").Value;
            _spikeTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/BoneTrail/BoneTrailSpike").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/BoneTrail/BoneTrailGhost").Value;
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

            if (ribCages.Count < MaxRibCages && Main.rand.NextFloat() < RibCageSpawnChance)
                SpawnRibCage(center, velocity, moveDir);

            if (marrowGlows.Count < MaxMarrowGlows && Main.rand.NextFloat() < MarrowGlowSpawnChance)
                SpawnMarrowGlow(center, velocity, moveDir);

            if (boneSpikes.Count < MaxBoneSpikes && Main.rand.NextFloat() < BoneSpikeSpawnChance)
                SpawnBoneSpike(center, velocity, moveDir);

            for (int i = ribCages.Count - 1; i >= 0; i--)
            {
                var r = ribCages[i];
                r.PulsePhase += r.PulseSpeed;
                r.Velocity *= 0.98f;
                r.Position += r.Velocity;
                r.Life--;
                if (r.Life <= 0) ribCages.RemoveAt(i);
            }

            for (int i = marrowGlows.Count - 1; i >= 0; i--)
            {
                var m = marrowGlows[i];
                m.PulsePhase += m.PulseSpeed;
                m.Velocity *= 0.97f;
                m.Position += m.Velocity;
                m.Life--;
                if (m.Life <= 0) marrowGlows.RemoveAt(i);
            }

            for (int i = boneSpikes.Count - 1; i >= 0; i--)
            {
                var s = boneSpikes[i];
                s.Rotation += s.SpinSpeed;
                s.Velocity *= 0.96f;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0) boneSpikes.RemoveAt(i);
            }
        }

        private void SpawnRibCage(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-5f, 5f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 drift = Main.rand.NextVector2Circular(RibCageDriftSpeed, RibCageDriftSpeed);
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
            float arcSpan = RibCageArcSpan * Main.rand.NextFloat(0.7f, 1.3f);
            float scale = RibCageScale * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = RibCageColor * Main.rand.NextFloat(0.6f, 1f);

            ribCages.Add(new RibCageParticle(pos, drift, RibCageLife, scale, arcSpan, rotation, RibCageGrowSpeed, color));
        }

        private void SpawnMarrowGlow(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-6f, 6f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(4f, 4f);

            Vector2 drift = Main.rand.NextVector2Circular(MarrowGlowDriftSpeed, MarrowGlowDriftSpeed);
            float startSize = MarrowGlowStartSize * Main.rand.NextFloat(0.7f, 1.3f);
            float endSize = MarrowGlowEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = MarrowGlowColor * Main.rand.NextFloat(0.5f, 1f);

            marrowGlows.Add(new MarrowGlowParticle(pos, drift, MarrowGlowLife, startSize, endSize, MarrowGlowExpandSpeed, color));
        }

        private void SpawnBoneSpike(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-4f, 4f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            Vector2 drift = Main.rand.NextVector2Circular(BoneSpikeDriftSpeed, BoneSpikeDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.6f, 1.3f) * BoneSpikeSize;
            float length = Main.rand.NextFloat(0.7f, 1.3f) * BoneSpikeLength;
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            float spinSpeed = Main.rand.NextFloat(-BoneSpikeSpinSpeed, BoneSpikeSpinSpeed);
            Color color = BoneSpikeColor * Main.rand.NextFloat(0.6f, 1f);

            boneSpikes.Add(new BoneSpikeParticle(pos, vel, BoneSpikeLife, scale, length, rotation, spinSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (marrowGlows.Count > 0 && _marrowTex != null)
            {
                Vector2 marrowOrigin = _marrowTex.Size() * 0.5f;
                var sorted = marrowGlows.OrderBy(m => m.Life);
                foreach (var m in sorted)
                {
                    Color drawColor = m.Color * m.Alpha;
                    Vector2 pos = m.Position - Main.screenPosition;
                    sb.Draw(_marrowTex, pos, null, drawColor, 0f,
                        marrowOrigin, m.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (ribCages.Count > 0 && _ribTex != null)
            {
                Vector2 ribOrigin = new Vector2(0f, _ribTex.Height * 0.5f);
                var sorted = ribCages.OrderBy(r => r.Life);
                foreach (var r in sorted)
                {
                    Color drawColor = r.Color * r.Alpha;
                    Vector2 pos = r.Position - Main.screenPosition;
                    float arcFraction = r.CurrentArc / (MathHelper.Pi * 0.8f);
                    Vector2 scale = new Vector2(arcFraction * r.Scale * 20f / _ribTex.Width, r.Scale);
                    sb.Draw(_ribTex, pos, null, drawColor, r.Rotation,
                        ribOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (boneSpikes.Count > 0 && _spikeTex != null)
            {
                Vector2 spikeOrigin = new Vector2(0f, _spikeTex.Height * 0.5f);
                var sorted = boneSpikes.OrderBy(s => s.Life);
                foreach (var s in sorted)
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(s.CurrentLength / _spikeTex.Width, s.Scale);
                    sb.Draw(_spikeTex, pos, null, drawColor, s.Rotation,
                        spikeOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            ribCages.Clear();
            marrowGlows.Clear();
            boneSpikes.Clear();
            _ghostTrail?.Clear();
        }
    }
}
