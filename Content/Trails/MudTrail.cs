using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class MudTrail : ITrail
    {
        public class MudClodParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Gravity;
            public float Rotation;
            public float SpinSpeed;
            public bool HasLanded;
            public float Squash;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    if (HasLanded)
                    {
                        float fadeOut = (1f - Progress) * (1f - Progress);
                        return MathF.Max(0f, fadeOut * 0.8f);
                    }
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut2 = 1f - Progress * 0.3f;
                    return MathF.Max(0f, fadeIn * fadeOut2);
                }
            }

            public float CurrentSquash => HasLanded ? 1f + Squash * MathF.Min(1f, (Progress - 0.5f) * 4f) : 1f;

            public float CurrentStretch => HasLanded ? 1f - Squash * 0.5f * MathF.Min(1f, (Progress - 0.5f) * 4f) : 1f;

            public MudClodParticle(Vector2 pos, Vector2 vel, int life, float scale, float gravity, float spinSpeed, float squash, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Gravity = gravity;
                SpinSpeed = spinSpeed;
                HasLanded = false;
                Squash = squash;
                Color = color;
            }
        }

        public class GroundCrackParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Length;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float GrowSpeed;
            public int BranchCount;
            public float BranchAngle;
            public int Depth;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float growIn = MathF.Min(1f, Progress * GrowSpeed);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, growIn * fadeOut * 0.6f);
                }
            }

            public float CurrentLength => Length * MathF.Min(1f, Progress * GrowSpeed) / (1f + Depth * 0.3f);

            public float CurrentWidth => Scale * (1f - Depth * 0.3f) * (1f - Progress * 0.4f);

            public GroundCrackParticle(Vector2 pos, Vector2 vel, int life, float scale, float length, float rotation, float growSpeed, int branchCount, float branchAngle, int depth, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Length = length;
                Rotation = rotation;
                GrowSpeed = growSpeed;
                BranchCount = branchCount;
                BranchAngle = branchAngle;
                Depth = depth;
                Color = color;
            }
        }

        public class SludgeDripParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Stretch;
            public int Life;
            public int MaxLife;
            public float Gravity;
            public float DripPhase;
            public float DripSpeed;
            public bool HasDripped;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.7f);
                }
            }

            public float CurrentStretch
            {
                get
                {
                    if (HasDripped) return Stretch * 0.5f;
                    float drip = 1f + MathF.Max(0f, MathF.Sin(DripPhase)) * 1.5f;
                    return Stretch * drip;
                }
            }

            public SludgeDripParticle(Vector2 pos, Vector2 vel, int life, float scale, float stretch, float gravity, float dripSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Stretch = stretch;
                Gravity = gravity;
                DripPhase = 0f;
                DripSpeed = dripSpeed;
                HasDripped = false;
                Color = color;
            }
        }

        public string Name { get; set; } = "MudTrail";

        public BlendState BlendMode => BlendState.AlphaBlend;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.25f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.25f;
        public Color GhostColor { get; set; } = new Color(120, 80, 40, 100);

        public int MaxMudClods { get; set; } = 18;
        public int MudClodLife { get; set; } = 40;
        public float MudClodSize { get; set; } = 0.5f;
        public float MudClodSpawnChance { get; set; } = 0.15f;
        public float MudClodGravity { get; set; } = 0.18f;
        public float MudClodSpinSpeed { get; set; } = 0.05f;
        public float MudClodSquash { get; set; } = 0.4f;
        public float MudClodDriftSpeed { get; set; } = 0.3f;
        public Color MudClodColor { get; set; } = new Color(140, 95, 45, 220);

        public int MaxGroundCracks { get; set; } = 8;
        public int GroundCrackLife { get; set; } = 50;
        public float GroundCrackSize { get; set; } = 0.5f;
        public float GroundCrackLength { get; set; } = 16f;
        public float GroundCrackSpawnChance { get; set; } = 0.04f;
        public float GroundCrackGrowSpeed { get; set; } = 2.5f;
        public int GroundCrackBranchCount { get; set; } = 2;
        public float GroundCrackBranchAngle { get; set; } = 0.6f;
        public int GroundCrackMaxDepth { get; set; } = 2;
        public float GroundCrackDriftSpeed { get; set; } = 0.04f;
        public Color GroundCrackColor { get; set; } = new Color(100, 65, 30, 200);

        public int MaxSludgeDrips { get; set; } = 15;
        public int SludgeDripLife { get; set; } = 35;
        public float SludgeDripSize { get; set; } = 0.4f;
        public float SludgeDripStretch { get; set; } = 2f;
        public float SludgeDripSpawnChance { get; set; } = 0.12f;
        public float SludgeDripGravity { get; set; } = 0.12f;
        public float SludgeDripDripSpeed { get; set; } = 0.08f;
        public float SludgeDripDriftSpeed { get; set; } = 0.15f;
        public Color SludgeDripColor { get; set; } = new Color(110, 75, 35, 210);

        public float InertiaFactor { get; set; } = 0.18f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<MudClodParticle> mudClods = new();
        private List<GroundCrackParticle> groundCracks = new();
        private List<SludgeDripParticle> sludgeDrips = new();

        private GhostTrail _ghostTrail;

        private Texture2D _clodTex;
        private Texture2D _crackTex;
        private Texture2D _dripTex;
        private Texture2D _ghostTex;

        public bool HasContent => mudClods.Count > 0 || groundCracks.Count > 0 || sludgeDrips.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_clodTex != null) return;
            _clodTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MudTrail/MudTrailClod").Value;
            _crackTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MudTrail/MudTrailCrack").Value;
            _dripTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MudTrail/MudTrailDrip").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MudTrail/MudTrailGhost").Value;
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
                UseAdditiveBlend = false,
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

            if (mudClods.Count < MaxMudClods && Main.rand.NextFloat() < MudClodSpawnChance)
                SpawnMudClod(center, velocity, moveDir);

            if (groundCracks.Count < MaxGroundCracks && Main.rand.NextFloat() < GroundCrackSpawnChance)
                SpawnGroundCrack(center, velocity, moveDir, 0);

            if (sludgeDrips.Count < MaxSludgeDrips && Main.rand.NextFloat() < SludgeDripSpawnChance)
                SpawnSludgeDrip(center, velocity, moveDir);

            for (int i = mudClods.Count - 1; i >= 0; i--)
            {
                var c = mudClods[i];
                if (!c.HasLanded)
                {
                    c.Velocity.Y += c.Gravity;
                    c.Velocity.X *= 0.99f;
                    c.Position += c.Velocity;
                    c.Rotation += c.SpinSpeed;

                    if (c.Progress > 0.55f && !c.HasLanded)
                    {
                        c.HasLanded = true;
                        c.Velocity = Vector2.Zero;
                    }
                }
                else
                {
                    c.Position += c.Velocity;
                }
                c.Life--;
                if (c.Life <= 0) mudClods.RemoveAt(i);
            }

            for (int i = groundCracks.Count - 1; i >= 0; i--)
            {
                var c = groundCracks[i];
                c.Velocity *= 0.98f;
                c.Position += c.Velocity;
                c.Life--;
                if (c.Life <= 0) groundCracks.RemoveAt(i);
            }

            for (int i = sludgeDrips.Count - 1; i >= 0; i--)
            {
                var d = sludgeDrips[i];
                if (!d.HasDripped)
                {
                    d.DripPhase += d.DripSpeed;
                    if (d.DripPhase > MathHelper.Pi && !d.HasDripped)
                    {
                        d.HasDripped = true;
                        d.Velocity.Y += d.Gravity * 3f;
                    }
                }
                d.Velocity.Y += d.Gravity;
                d.Velocity.X *= 0.99f;
                d.Position += d.Velocity;
                d.Life--;
                if (d.Life <= 0) sludgeDrips.RemoveAt(i);
            }
        }

        private void SpawnMudClod(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-5f, 5f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            Vector2 drift = Main.rand.NextVector2Circular(MudClodDriftSpeed, MudClodDriftSpeed);
            Vector2 gravity = new Vector2(0f, Main.rand.NextFloat(0.5f, 1.5f));
            Vector2 vel = inertia + drift + gravity;

            float scale = Main.rand.NextFloat(0.6f, 1.4f) * MudClodSize;
            float spinSpeed = Main.rand.NextFloat(-MudClodSpinSpeed, MudClodSpinSpeed);
            Color color = MudClodColor * Main.rand.NextFloat(0.7f, 1f);

            mudClods.Add(new MudClodParticle(pos, vel, MudClodLife, scale, MudClodGravity, spinSpeed, MudClodSquash, color));
        }

        private void SpawnGroundCrack(Vector2 center, Vector2 velocity, Vector2 moveDir, int depth)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-4f, 4f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 drift = Main.rand.NextVector2Circular(GroundCrackDriftSpeed, GroundCrackDriftSpeed);
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            float length = GroundCrackLength * Main.rand.NextFloat(0.6f, 1.2f);
            float scale = GroundCrackSize * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = GroundCrackColor * Main.rand.NextFloat(0.6f, 1f);

            groundCracks.Add(new GroundCrackParticle(pos, drift, GroundCrackLife, scale, length, baseAngle, GroundCrackGrowSpeed, GroundCrackBranchCount, GroundCrackBranchAngle, depth, color));

            if (depth < GroundCrackMaxDepth && Main.rand.NextFloat() < 0.5f)
            {
                Vector2 tipPos = pos + new Vector2(MathF.Cos(baseAngle), MathF.Sin(baseAngle)) * length * 0.6f;
                for (int i = 0; i < GroundCrackBranchCount && groundCracks.Count < MaxGroundCracks; i++)
                {
                    float subAngle = baseAngle + GroundCrackBranchAngle * Main.rand.NextFloat(-1.5f, 1.5f);
                    float subLength = length * Main.rand.NextFloat(0.3f, 0.6f);
                    groundCracks.Add(new GroundCrackParticle(tipPos, drift * 0.5f, GroundCrackLife - 8, scale * 0.7f, subLength, subAngle, GroundCrackGrowSpeed, 1, GroundCrackBranchAngle, depth + 1, color * 0.8f));
                }
            }
        }

        private void SpawnSludgeDrip(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-4f, 4f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.2f;
            Vector2 drift = Main.rand.NextVector2Circular(SludgeDripDriftSpeed, SludgeDripDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.6f, 1.3f) * SludgeDripSize;
            float stretch = Main.rand.NextFloat(0.8f, 1.5f) * SludgeDripStretch;
            Color color = SludgeDripColor * Main.rand.NextFloat(0.6f, 1f);

            sludgeDrips.Add(new SludgeDripParticle(pos, vel, SludgeDripLife, scale, stretch, SludgeDripGravity, SludgeDripDripSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (groundCracks.Count > 0 && _crackTex != null)
            {
                Vector2 crackOrigin = new Vector2(0f, _crackTex.Height * 0.5f);
                var sorted = groundCracks.OrderBy(c => c.Depth).ThenBy(c => c.Life);
                foreach (var c in sorted)
                {
                    Color drawColor = c.Color * c.Alpha;
                    Vector2 pos = c.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(c.CurrentLength / _crackTex.Width, c.CurrentWidth);
                    sb.Draw(_crackTex, pos, null, drawColor, c.Rotation,
                        crackOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (sludgeDrips.Count > 0 && _dripTex != null)
            {
                Vector2 dripOrigin = new Vector2(_dripTex.Width * 0.5f, 0f);
                var sorted = sludgeDrips.OrderBy(d => d.Life);
                foreach (var d in sorted)
                {
                    Color drawColor = d.Color * d.Alpha;
                    Vector2 pos = d.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(d.Scale, d.Scale * d.CurrentStretch);
                    sb.Draw(_dripTex, pos, null, drawColor, 0f,
                        dripOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (mudClods.Count > 0 && _clodTex != null)
            {
                Vector2 clodOrigin = _clodTex.Size() * 0.5f;
                var sorted = mudClods.OrderBy(c => c.Life);
                foreach (var c in sorted)
                {
                    Color drawColor = c.Color * c.Alpha;
                    Vector2 pos = c.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(c.Scale * c.CurrentSquash, c.Scale * c.CurrentStretch);
                    sb.Draw(_clodTex, pos, null, drawColor, c.Rotation,
                        clodOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            mudClods.Clear();
            groundCracks.Clear();
            sludgeDrips.Clear();
            _ghostTrail?.Clear();
        }
    }
}
