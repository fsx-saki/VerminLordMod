using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class UnrealTrail : ITrail
    {
        public class IllusionWaveParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float ExpandRate;
            public float DistortPhase;
            public float DistortSpeed;
            public float ShimmerPhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float ripple = MathF.Min(1f, Progress * 4f);
                    float fade = 1f - MathF.Pow(Progress, 2f);
                    float shimmer = 0.5f + 0.5f * MathF.Sin(ShimmerPhase);
                    return MathF.Max(0f, ripple * fade * shimmer * 0.5f);
                }
            }

            public float CurrentScale => Scale * (1f + Progress * ExpandRate);

            public float DistortOffset => MathF.Sin(DistortPhase) * 2f;

            public IllusionWaveParticle(Vector2 pos, Vector2 vel, int life, float scale, float expandRate, float distortSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                ExpandRate = expandRate;
                DistortPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                DistortSpeed = distortSpeed;
                ShimmerPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public class AfterImageParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float FadeDelay;
            public float DriftAngle;
            public float DriftSpeed;
            public float WobblePhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float ghostIn = MathF.Min(1f, Progress * 2f);
                    float ghostOut = 1f - MathF.Max(0f, (Progress - FadeDelay) / (1f - FadeDelay));
                    float wobble = 0.8f + 0.2f * MathF.Sin(WobblePhase);
                    return MathF.Max(0f, ghostIn * ghostOut * wobble * 0.4f);
                }
            }

            public AfterImageParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotation, float fadeDelay, float driftAngle, float driftSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = rotation;
                FadeDelay = fadeDelay;
                DriftAngle = driftAngle;
                DriftSpeed = driftSpeed;
                WobblePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public class MirrorShardParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpinSpeed;
            public float ReflectPhase;
            public float ReflectSpeed;
            public float ShatterPhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float flash = MathF.Min(1f, Progress * 6f);
                    float shatter = 1f - MathF.Pow(Progress, 2f);
                    float reflect = 0.6f + 0.4f * MathF.Sin(ReflectPhase);
                    return MathF.Max(0f, flash * shatter * reflect);
                }
            }

            public MirrorShardParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotation, float spinSpeed, float reflectSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = rotation;
                SpinSpeed = spinSpeed;
                ReflectPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                ReflectSpeed = reflectSpeed;
                ShatterPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public string Name { get; set; } = "UnrealTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 12;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.18f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.2f;
        public Color GhostColor { get; set; } = new Color(160, 180, 220, 100);

        public int MaxIllusionWaves { get; set; } = 15;
        public int IllusionWaveLife { get; set; } = 25;
        public float IllusionWaveSize { get; set; } = 0.6f;
        public float IllusionWaveExpandRate { get; set; } = 2.5f;
        public int IllusionWaveSpawnInterval { get; set; } = 3;
        public float IllusionWaveDistortSpeed { get; set; } = 0.1f;
        public float IllusionWaveSpread { get; set; } = 5f;
        public Color IllusionWaveColor { get; set; } = new Color(180, 200, 240, 200);

        public int MaxAfterImages { get; set; } = 10;
        public int AfterImageLife { get; set; } = 40;
        public float AfterImageSize { get; set; } = 0.5f;
        public float AfterImageSpawnChance { get; set; } = 0.05f;
        public float AfterImageFadeDelay { get; set; } = 0.3f;
        public float AfterImageDriftSpeed { get; set; } = 0.3f;
        public float AfterImageSpread { get; set; } = 6f;
        public Color AfterImageColor { get; set; } = new Color(140, 180, 220, 180);

        public int MaxMirrorShards { get; set; } = 12;
        public int MirrorShardLife { get; set; } = 22;
        public float MirrorShardSize { get; set; } = 0.4f;
        public float MirrorShardSpawnChance { get; set; } = 0.07f;
        public float MirrorShardSpinSpeed { get; set; } = 0.15f;
        public float MirrorShardReflectSpeed { get; set; } = 0.1f;
        public float MirrorShardSpeed { get; set; } = 2f;
        public float MirrorShardSpread { get; set; } = 4f;
        public Color MirrorShardColor { get; set; } = new Color(200, 220, 255, 230);

        public float InertiaFactor { get; set; } = 0.15f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<IllusionWaveParticle> illusionWaves = new();
        private List<AfterImageParticle> afterImages = new();
        private List<MirrorShardParticle> mirrorShards = new();
        private int waveCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _waveTex;
        private Texture2D _afterTex;
        private Texture2D _shardTex;
        private Texture2D _ghostTex;

        public bool HasContent => illusionWaves.Count > 0 || afterImages.Count > 0 || mirrorShards.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_waveTex != null) return;
            _waveTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/UnrealTrail/UnrealTrailWave").Value;
            _afterTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/UnrealTrail/UnrealTrailAfter").Value;
            _shardTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/UnrealTrail/UnrealTrailShard").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/UnrealTrail/UnrealTrailGhost").Value;
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

            waveCounter++;
            if (waveCounter >= IllusionWaveSpawnInterval && illusionWaves.Count < MaxIllusionWaves)
            {
                waveCounter = 0;
                SpawnIllusionWave(center, velocity, moveDir);
            }

            if (afterImages.Count < MaxAfterImages && Main.rand.NextFloat() < AfterImageSpawnChance)
                SpawnAfterImage(center, velocity, moveDir);

            if (mirrorShards.Count < MaxMirrorShards && Main.rand.NextFloat() < MirrorShardSpawnChance)
                SpawnMirrorShard(center, velocity, moveDir);

            for (int i = illusionWaves.Count - 1; i >= 0; i--)
            {
                var w = illusionWaves[i];
                w.DistortPhase += w.DistortSpeed;
                w.ShimmerPhase += 0.08f;
                w.Velocity *= 0.94f;
                w.Position += w.Velocity;
                w.Life--;
                if (w.Life <= 0) illusionWaves.RemoveAt(i);
            }

            for (int i = afterImages.Count - 1; i >= 0; i--)
            {
                var a = afterImages[i];
                a.WobblePhase += 0.04f;
                a.Velocity *= 0.96f;
                a.Position += a.Velocity;
                a.Life--;
                if (a.Life <= 0) afterImages.RemoveAt(i);
            }

            for (int i = mirrorShards.Count - 1; i >= 0; i--)
            {
                var s = mirrorShards[i];
                s.Rotation += s.SpinSpeed;
                s.ReflectPhase += s.ReflectSpeed;
                s.ShatterPhase += 0.08f;
                s.Velocity *= 0.95f;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0) mirrorShards.RemoveAt(i);
            }
        }

        private void SpawnIllusionWave(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-IllusionWaveSpread, IllusionWaveSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.4f;
            Vector2 drift = Main.rand.NextVector2Circular(0.5f, 0.5f);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.5f, 1.3f) * IllusionWaveSize;
            float expandRate = IllusionWaveExpandRate * Main.rand.NextFloat(0.7f, 1.3f);
            float distortSpeed = IllusionWaveDistortSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = IllusionWaveColor * Main.rand.NextFloat(0.5f, 1f);

            illusionWaves.Add(new IllusionWaveParticle(pos, vel, IllusionWaveLife, scale, expandRate, distortSpeed, color));
        }

        private void SpawnAfterImage(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-AfterImageSpread, AfterImageSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 inertia = -velocity * InertiaFactor * 0.5f;
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 drift = angle.ToRotationVector2() * Main.rand.NextFloat(0.2f, AfterImageDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.5f, 1.2f) * AfterImageSize;
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            float fadeDelay = AfterImageFadeDelay * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = AfterImageColor * Main.rand.NextFloat(0.4f, 0.8f);

            afterImages.Add(new AfterImageParticle(pos, vel, AfterImageLife, scale, rotation, fadeDelay, 0f, 0f, color));
        }

        private void SpawnMirrorShard(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-MirrorShardSpread, MirrorShardSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(1f, 1f);

            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 burst = angle.ToRotationVector2() * Main.rand.NextFloat(0.5f, MirrorShardSpeed);
            Vector2 vel = inertia + burst;

            float scale = Main.rand.NextFloat(0.5f, 1.4f) * MirrorShardSize;
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            float spinSpeed = Main.rand.NextFloat(-MirrorShardSpinSpeed, MirrorShardSpinSpeed);
            float reflectSpeed = MirrorShardReflectSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = MirrorShardColor * Main.rand.NextFloat(0.7f, 1f);

            mirrorShards.Add(new MirrorShardParticle(pos, vel, MirrorShardLife, scale, rotation, spinSpeed, reflectSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (mirrorShards.Count > 0 && _shardTex != null)
            {
                Vector2 shardOrigin = new Vector2(_shardTex.Width * 0.5f, _shardTex.Height * 0.5f);
                var sorted = mirrorShards.OrderBy(s => s.Life);
                foreach (var s in sorted)
                {
                    float reflect = 0.5f + 0.5f * MathF.Sin(s.ReflectPhase);
                    Color drawColor = s.Color * s.Alpha * (0.6f + 0.4f * reflect);
                    Vector2 pos = s.Position - Main.screenPosition;
                    sb.Draw(_shardTex, pos, null, drawColor, s.Rotation,
                        shardOrigin, s.Scale, SpriteEffects.None, 0);
                }
            }

            if (afterImages.Count > 0 && _afterTex != null)
            {
                Vector2 afterOrigin = new Vector2(_afterTex.Width * 0.5f, _afterTex.Height * 0.5f);
                var sorted = afterImages.OrderBy(a => a.Life);
                foreach (var a in sorted)
                {
                    Color drawColor = a.Color * a.Alpha;
                    Vector2 pos = a.Position - Main.screenPosition;
                    sb.Draw(_afterTex, pos, null, drawColor, a.Rotation,
                        afterOrigin, a.Scale, SpriteEffects.None, 0);
                }
            }

            if (illusionWaves.Count > 0 && _waveTex != null)
            {
                Vector2 waveOrigin = new Vector2(_waveTex.Width * 0.5f, _waveTex.Height * 0.5f);
                var sorted = illusionWaves.OrderBy(w => w.Life);
                foreach (var w in sorted)
                {
                    Color drawColor = w.Color * w.Alpha;
                    Vector2 pos = w.Position - Main.screenPosition;
                    float distort = w.DistortOffset;
                    Vector2 distortPos = pos + new Vector2(distort, distort * 0.5f);
                    sb.Draw(_waveTex, distortPos, null, drawColor, w.Rotation,
                        waveOrigin, w.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            illusionWaves.Clear();
            afterImages.Clear();
            mirrorShards.Clear();
            waveCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}