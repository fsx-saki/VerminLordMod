using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class LifeDeathTrail : ITrail
    {
        public class WitherPetalParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpinSpeed;
            public float Gravity;
            public float CurlPhase;
            public float CurlSpeed;
            public float CurlAmplitude;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float wither = 1f - Progress * 0.6f;
                    return MathF.Max(0f, fadeIn * fadeOut * wither);
                }
            }

            public float CurrentCurl => MathF.Sin(CurlPhase) * CurlAmplitude;

            public WitherPetalParticle(Vector2 pos, Vector2 vel, int life, float scale, float spinSpeed, float gravity, float curlSpeed, float curlAmplitude, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                SpinSpeed = spinSpeed;
                Gravity = gravity;
                CurlPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                CurlSpeed = curlSpeed;
                CurlAmplitude = curlAmplitude;
                Color = color;
            }
        }

        public class BloomFlowerParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float PetalCount;
            public float BloomSpeed;
            public float PulsePhase;
            public float PulseSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float bloomIn = MathF.Min(1f, Progress * BloomSpeed);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float pulse = 0.7f + 0.3f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, bloomIn * fadeOut * pulse * 0.5f);
                }
            }

            public float CurrentScale
            {
                get
                {
                    float bloom = MathF.Min(1f, Progress * BloomSpeed);
                    return Scale + (MaxScale - Scale) * bloom;
                }
            }

            public float CurrentBloom => MathF.Min(1f, Progress * BloomSpeed);

            public BloomFlowerParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float rotation, float petalCount, float bloomSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                Rotation = rotation;
                PetalCount = petalCount;
                BloomSpeed = bloomSpeed;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = Main.rand.NextFloat(0.06f, 0.14f);
                Color = color;
            }
        }

        public class SamsaraRingParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float ExpandSpeed;
            public float HalfLifeRatio;
            public Color LifeColor;
            public Color DeathColor;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 3f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.4f);
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

            public Color CurrentColor
            {
                get
                {
                    float t = (MathF.Sin(Rotation * 2f) + 1f) * 0.5f;
                    return Color.Lerp(DeathColor, LifeColor, t);
                }
            }

            public SamsaraRingParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float rotSpeed, float expandSpeed, float halfLifeRatio, Color lifeColor, Color deathColor)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                ExpandSpeed = expandSpeed;
                HalfLifeRatio = halfLifeRatio;
                LifeColor = lifeColor;
                DeathColor = deathColor;
            }
        }

        public string Name { get; set; } = "LifeDeathTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.22f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(180, 160, 200, 140);

        public int MaxWitherPetals { get; set; } = 20;
        public int WitherPetalLife { get; set; } = 40;
        public float WitherPetalSize { get; set; } = 0.5f;
        public float WitherPetalSpawnChance { get; set; } = 0.15f;
        public float WitherPetalSpinSpeed { get; set; } = 0.06f;
        public float WitherPetalGravity { get; set; } = 0.08f;
        public float WitherPetalCurlSpeed { get; set; } = 0.1f;
        public float WitherPetalCurlAmplitude { get; set; } = 0.4f;
        public float WitherPetalDriftSpeed { get; set; } = 0.2f;
        public Color WitherPetalColor { get; set; } = new Color(160, 80, 100, 200);

        public int MaxBloomFlowers { get; set; } = 8;
        public int BloomFlowerLife { get; set; } = 55;
        public float BloomFlowerStartSize { get; set; } = 0.2f;
        public float BloomFlowerEndSize { get; set; } = 1.2f;
        public float BloomFlowerSpawnChance { get; set; } = 0.04f;
        public float BloomFlowerBloomSpeed { get; set; } = 2f;
        public float BloomFlowerDriftSpeed { get; set; } = 0.05f;
        public Color BloomFlowerColor { get; set; } = new Color(120, 220, 140, 200);

        public int MaxSamsaraRings { get; set; } = 5;
        public int SamsaraRingLife { get; set; } = 50;
        public float SamsaraRingStartSize { get; set; } = 0.3f;
        public float SamsaraRingEndSize { get; set; } = 1.6f;
        public float SamsaraRingSpawnChance { get; set; } = 0.02f;
        public float SamsaraRingRotSpeed { get; set; } = 0.04f;
        public float SamsaraRingExpandSpeed { get; set; } = 1.8f;
        public float SamsaraRingDriftSpeed { get; set; } = 0.04f;
        public Color SamsaraRingLifeColor { get; set; } = new Color(100, 220, 130, 180);
        public Color SamsaraRingDeathColor { get; set; } = new Color(180, 60, 90, 180);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<WitherPetalParticle> witherPetals = new();
        private List<BloomFlowerParticle> bloomFlowers = new();
        private List<SamsaraRingParticle> samsaraRings = new();

        private GhostTrail _ghostTrail;

        private Texture2D _petalTex;
        private Texture2D _bloomTex;
        private Texture2D _ringTex;
        private Texture2D _ghostTex;

        public bool HasContent => witherPetals.Count > 0 || bloomFlowers.Count > 0 || samsaraRings.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_petalTex != null) return;
            _petalTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LifeDeathTrail/LifeDeathTrailPetal").Value;
            _bloomTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LifeDeathTrail/LifeDeathTrailBloom").Value;
            _ringTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LifeDeathTrail/LifeDeathTrailRing").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LifeDeathTrail/LifeDeathTrailGhost").Value;
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

            if (witherPetals.Count < MaxWitherPetals && Main.rand.NextFloat() < WitherPetalSpawnChance)
                SpawnWitherPetal(center, velocity, moveDir);

            if (bloomFlowers.Count < MaxBloomFlowers && Main.rand.NextFloat() < BloomFlowerSpawnChance)
                SpawnBloomFlower(center, velocity, moveDir);

            if (samsaraRings.Count < MaxSamsaraRings && Main.rand.NextFloat() < SamsaraRingSpawnChance)
                SpawnSamsaraRing(center, velocity, moveDir);

            for (int i = witherPetals.Count - 1; i >= 0; i--)
            {
                var p = witherPetals[i];
                p.CurlPhase += p.CurlSpeed;
                p.Rotation += p.SpinSpeed + p.CurrentCurl * 0.05f;
                p.Velocity.Y += p.Gravity;
                p.Velocity.X *= 0.99f;
                p.Position += p.Velocity;
                p.Life--;
                if (p.Life <= 0) witherPetals.RemoveAt(i);
            }

            for (int i = bloomFlowers.Count - 1; i >= 0; i--)
            {
                var b = bloomFlowers[i];
                b.PulsePhase += b.PulseSpeed;
                b.Velocity *= 0.97f;
                b.Position += b.Velocity;
                b.Life--;
                if (b.Life <= 0) bloomFlowers.RemoveAt(i);
            }

            for (int i = samsaraRings.Count - 1; i >= 0; i--)
            {
                var r = samsaraRings[i];
                r.Rotation += r.RotSpeed;
                r.Velocity *= 0.98f;
                r.Position += r.Velocity;
                r.Life--;
                if (r.Life <= 0) samsaraRings.RemoveAt(i);
            }
        }

        private void SpawnWitherPetal(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-5f, 5f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 inertia = -velocity * InertiaFactor * 0.2f;
            Vector2 drift = Main.rand.NextVector2Circular(WitherPetalDriftSpeed, WitherPetalDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.6f, 1.3f) * WitherPetalSize;
            float spinSpeed = Main.rand.NextFloat(-WitherPetalSpinSpeed, WitherPetalSpinSpeed);
            Color color = WitherPetalColor * Main.rand.NextFloat(0.6f, 1f);

            witherPetals.Add(new WitherPetalParticle(pos, vel, WitherPetalLife, scale, spinSpeed, WitherPetalGravity, WitherPetalCurlSpeed, WitherPetalCurlAmplitude, color));
        }

        private void SpawnBloomFlower(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-6f, 6f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(4f, 4f);

            Vector2 drift = Main.rand.NextVector2Circular(BloomFlowerDriftSpeed, BloomFlowerDriftSpeed);
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            float startSize = BloomFlowerStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = BloomFlowerEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = BloomFlowerColor * Main.rand.NextFloat(0.6f, 1f);

            bloomFlowers.Add(new BloomFlowerParticle(pos, drift, BloomFlowerLife, startSize, endSize, rotation, 5f, BloomFlowerBloomSpeed, color));
        }

        private void SpawnSamsaraRing(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-4f, 4f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 drift = Main.rand.NextVector2Circular(SamsaraRingDriftSpeed, SamsaraRingDriftSpeed);
            float startSize = SamsaraRingStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = SamsaraRingEndSize * Main.rand.NextFloat(0.8f, 1.2f);

            samsaraRings.Add(new SamsaraRingParticle(pos, drift, SamsaraRingLife, startSize, endSize, SamsaraRingRotSpeed, SamsaraRingExpandSpeed, 0.5f, SamsaraRingLifeColor, SamsaraRingDeathColor));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (samsaraRings.Count > 0 && _ringTex != null)
            {
                Vector2 ringOrigin = _ringTex.Size() * 0.5f;
                var sorted = samsaraRings.OrderBy(r => r.Life);
                foreach (var r in sorted)
                {
                    Color drawColor = r.CurrentColor * r.Alpha;
                    Vector2 pos = r.Position - Main.screenPosition;
                    sb.Draw(_ringTex, pos, null, drawColor, r.Rotation,
                        ringOrigin, r.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (bloomFlowers.Count > 0 && _bloomTex != null)
            {
                Vector2 bloomOrigin = _bloomTex.Size() * 0.5f;
                var sorted = bloomFlowers.OrderBy(b => b.Life);
                foreach (var b in sorted)
                {
                    Color drawColor = b.Color * b.Alpha;
                    Vector2 pos = b.Position - Main.screenPosition;
                    sb.Draw(_bloomTex, pos, null, drawColor, b.Rotation,
                        bloomOrigin, b.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (witherPetals.Count > 0 && _petalTex != null)
            {
                Vector2 petalOrigin = _petalTex.Size() * 0.5f;
                var sorted = witherPetals.OrderBy(p => p.Life);
                foreach (var p in sorted)
                {
                    Color drawColor = p.Color * p.Alpha;
                    Vector2 pos = p.Position - Main.screenPosition;
                    sb.Draw(_petalTex, pos, null, drawColor, p.Rotation,
                        petalOrigin, p.Scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            witherPetals.Clear();
            bloomFlowers.Clear();
            samsaraRings.Clear();
            _ghostTrail?.Clear();
        }
    }
}
