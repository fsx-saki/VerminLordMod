using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class KillingTrail : ITrail
    {
        public class BloodStreakParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Length;
            public float Width;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float DripPhase;
            public float DripSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float burst = MathF.Min(1f, Progress * 6f);
                    float fade = 1f - MathF.Pow(Progress, 2f);
                    float drip = 0.8f + 0.2f * MathF.Sin(DripPhase);
                    return MathF.Max(0f, burst * fade * drip);
                }
            }

            public float CurrentLength => Length * (1f - Progress * 0.5f);

            public float CurrentWidth => Width * (1f - Progress * 0.6f);

            public BloodStreakParticle(Vector2 pos, Vector2 vel, int life, float length, float width, float rotation, float dripSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Length = length;
                Width = width;
                Rotation = rotation;
                DripPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                DripSpeed = dripSpeed;
                Color = color;
            }
        }

        public class KillingAuraParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float ExpandRate;
            public float PulsePhase;
            public float PulseSpeed;
            public float DistortPhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float surge = MathF.Min(1f, Progress * 5f);
                    float recede = 1f - MathF.Pow(Progress, 2.5f);
                    float pulse = 0.6f + 0.4f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, surge * recede * pulse);
                }
            }

            public float CurrentScale => Scale * (1f + Progress * ExpandRate);

            public KillingAuraParticle(Vector2 pos, Vector2 vel, int life, float scale, float expandRate, float pulseSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                ExpandRate = expandRate;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = pulseSpeed;
                DistortPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public class DeathShadowParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float FadeDelay;
            public float ShimmerPhase;
            public float ShimmerSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float appear = MathF.Min(1f, Progress * 2f);
                    float vanish = 1f - MathF.Max(0f, (Progress - FadeDelay) / (1f - FadeDelay));
                    float shimmer = 0.7f + 0.3f * MathF.Sin(ShimmerPhase);
                    return MathF.Max(0f, appear * vanish * shimmer * 0.5f);
                }
            }

            public DeathShadowParticle(Vector2 pos, Vector2 vel, int life, float scale, float fadeDelay, float shimmerSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                FadeDelay = fadeDelay;
                ShimmerPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                ShimmerSpeed = shimmerSpeed;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public string Name { get; set; } = "KillingTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.14f;
        public float GhostLengthScale { get; set; } = 1.3f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(180, 60, 60, 120);

        public int MaxBloodStreaks { get; set; } = 15;
        public int BloodStreakLife { get; set; } = 22;
        public float BloodStreakLength { get; set; } = 28f;
        public float BloodStreakWidth { get; set; } = 0.22f;
        public float BloodStreakSpawnChance { get; set; } = 0.1f;
        public float BloodStreakDripSpeed { get; set; } = 0.08f;
        public float BloodStreakDriftSpeed { get; set; } = 0.15f;
        public Color BloodStreakColor { get; set; } = new Color(200, 40, 40, 230);

        public int MaxKillingAuras { get; set; } = 12;
        public int KillingAuraLife { get; set; } = 20;
        public float KillingAuraSize { get; set; } = 0.6f;
        public float KillingAuraExpandRate { get; set; } = 2f;
        public float KillingAuraSpawnChance { get; set; } = 0.07f;
        public float KillingAuraPulseSpeed { get; set; } = 0.1f;
        public float KillingAuraSpread { get; set; } = 4f;
        public Color KillingAuraColor { get; set; } = new Color(180, 30, 30, 200);

        public int MaxDeathShadows { get; set; } = 10;
        public int DeathShadowLife { get; set; } = 45;
        public float DeathShadowSize { get; set; } = 0.7f;
        public float DeathShadowSpawnChance { get; set; } = 0.04f;
        public float DeathShadowFadeDelay { get; set; } = 0.5f;
        public float DeathShadowShimmerSpeed { get; set; } = 0.05f;
        public float DeathShadowSpread { get; set; } = 5f;
        public Color DeathShadowColor { get; set; } = new Color(100, 20, 40, 180);

        public float InertiaFactor { get; set; } = 0.2f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<BloodStreakParticle> bloodStreaks = new();
        private List<KillingAuraParticle> killingAuras = new();
        private List<DeathShadowParticle> deathShadows = new();

        private GhostTrail _ghostTrail;

        private Texture2D _streakTex;
        private Texture2D _auraTex;
        private Texture2D _shadowTex;
        private Texture2D _ghostTex;

        public bool HasContent => bloodStreaks.Count > 0 || killingAuras.Count > 0 || deathShadows.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_streakTex != null) return;
            _streakTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/KillingTrail/KillingTrailStreak").Value;
            _auraTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/KillingTrail/KillingTrailAura").Value;
            _shadowTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/KillingTrail/KillingTrailShadow").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/KillingTrail/KillingTrailGhost").Value;
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

            if (bloodStreaks.Count < MaxBloodStreaks && Main.rand.NextFloat() < BloodStreakSpawnChance)
                SpawnBloodStreak(center, velocity, moveDir);

            if (killingAuras.Count < MaxKillingAuras && Main.rand.NextFloat() < KillingAuraSpawnChance)
                SpawnKillingAura(center, velocity, moveDir);

            if (deathShadows.Count < MaxDeathShadows && Main.rand.NextFloat() < DeathShadowSpawnChance)
                SpawnDeathShadow(center, velocity, moveDir);

            for (int i = bloodStreaks.Count - 1; i >= 0; i--)
            {
                var b = bloodStreaks[i];
                b.DripPhase += b.DripSpeed;
                b.Velocity *= 0.96f;
                b.Position += b.Velocity;
                b.Life--;
                if (b.Life <= 0) bloodStreaks.RemoveAt(i);
            }

            for (int i = killingAuras.Count - 1; i >= 0; i--)
            {
                var a = killingAuras[i];
                a.PulsePhase += a.PulseSpeed;
                a.DistortPhase += 0.06f;
                a.Velocity *= 0.93f;
                a.Position += a.Velocity;
                a.Life--;
                if (a.Life <= 0) killingAuras.RemoveAt(i);
            }

            for (int i = deathShadows.Count - 1; i >= 0; i--)
            {
                var d = deathShadows[i];
                d.ShimmerPhase += d.ShimmerSpeed;
                d.Velocity *= 0.95f;
                d.Position += d.Velocity;
                d.Life--;
                if (d.Life <= 0) deathShadows.RemoveAt(i);
            }
        }

        private void SpawnBloodStreak(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-3f, 3f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(1f, 1f);

            Vector2 inertia = -velocity * InertiaFactor * 0.4f;
            Vector2 drift = Main.rand.NextVector2Circular(BloodStreakDriftSpeed, BloodStreakDriftSpeed);
            Vector2 vel = inertia + drift;

            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.3f, 0.3f);
            float length = BloodStreakLength * Main.rand.NextFloat(0.7f, 1.4f);
            float width = BloodStreakWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float dripSpeed = BloodStreakDripSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = BloodStreakColor * Main.rand.NextFloat(0.7f, 1f);

            bloodStreaks.Add(new BloodStreakParticle(pos, vel, BloodStreakLife, length, width, rotation, dripSpeed, color));
        }

        private void SpawnKillingAura(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-KillingAuraSpread, KillingAuraSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            Vector2 drift = Main.rand.NextVector2Circular(0.5f, 0.5f);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.6f, 1.4f) * KillingAuraSize;
            float expandRate = KillingAuraExpandRate * Main.rand.NextFloat(0.7f, 1.3f);
            float pulseSpeed = KillingAuraPulseSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = KillingAuraColor * Main.rand.NextFloat(0.6f, 1f);

            killingAuras.Add(new KillingAuraParticle(pos, vel, KillingAuraLife, scale, expandRate, pulseSpeed, color));
        }

        private void SpawnDeathShadow(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-DeathShadowSpread, DeathShadowSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 inertia = -velocity * InertiaFactor * 0.5f;
            Vector2 drift = Main.rand.NextVector2Circular(0.3f, 0.3f);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.5f, 1.3f) * DeathShadowSize;
            float fadeDelay = DeathShadowFadeDelay * Main.rand.NextFloat(0.8f, 1.2f);
            float shimmerSpeed = DeathShadowShimmerSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = DeathShadowColor * Main.rand.NextFloat(0.5f, 1f);

            deathShadows.Add(new DeathShadowParticle(pos, vel, DeathShadowLife, scale, fadeDelay, shimmerSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (deathShadows.Count > 0 && _shadowTex != null)
            {
                Vector2 shadowOrigin = new Vector2(_shadowTex.Width * 0.5f, _shadowTex.Height * 0.5f);
                var sorted = deathShadows.OrderBy(d => d.Life);
                foreach (var d in sorted)
                {
                    Color drawColor = d.Color * d.Alpha;
                    Vector2 pos = d.Position - Main.screenPosition;
                    sb.Draw(_shadowTex, pos, null, drawColor, d.Rotation,
                        shadowOrigin, d.Scale, SpriteEffects.None, 0);
                }
            }

            if (killingAuras.Count > 0 && _auraTex != null)
            {
                Vector2 auraOrigin = new Vector2(_auraTex.Width * 0.5f, _auraTex.Height * 0.5f);
                var sorted = killingAuras.OrderBy(a => a.Life);
                foreach (var a in sorted)
                {
                    Color drawColor = a.Color * a.Alpha;
                    Vector2 pos = a.Position - Main.screenPosition;
                    sb.Draw(_auraTex, pos, null, drawColor, a.Rotation,
                        auraOrigin, a.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (bloodStreaks.Count > 0 && _streakTex != null)
            {
                Vector2 streakOrigin = new Vector2(0f, _streakTex.Height * 0.5f);
                var sorted = bloodStreaks.OrderBy(b => b.Life);
                foreach (var b in sorted)
                {
                    Color drawColor = b.Color * b.Alpha;
                    Vector2 pos = b.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(b.CurrentLength / _streakTex.Width, b.CurrentWidth);
                    sb.Draw(_streakTex, pos, null, drawColor, b.Rotation,
                        streakOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            bloodStreaks.Clear();
            killingAuras.Clear();
            deathShadows.Clear();
            _ghostTrail?.Clear();
        }
    }
}