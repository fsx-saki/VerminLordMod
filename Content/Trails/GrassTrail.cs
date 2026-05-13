using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class GrassTrail : ITrail
    {
        public class GrassLeafParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float FlutterPhase;
            public float FlutterSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = 1f - Progress;
                    return MathF.Max(0f, fadeIn * fadeOut);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.3f);

            public GrassLeafParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                FlutterPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                FlutterSpeed = Main.rand.NextFloat(0.05f, 0.12f);
                Color = color;
            }
        }

        public class GrassPollenParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float TwinklePhase;
            public float TwinkleSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = 1f - Progress;
                    float twinkle = 0.5f + 0.5f * MathF.Max(0, MathF.Sin(TwinklePhase));
                    return MathF.Max(0f, fadeIn * fadeOut * twinkle);
                }
            }

            public GrassPollenParticle(Vector2 pos, Vector2 vel, int life, float scale, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                TwinklePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                TwinkleSpeed = Main.rand.NextFloat(0.08f, 0.18f);
                Color = color;
            }
        }

        public class GrassBranchParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Length;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float GrowSpeed;
            public int Depth;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float growIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, growIn * fadeOut);
                }
            }

            public float CurrentLength => Length * MathF.Min(1f, Progress * GrowSpeed);

            public float CurrentWidth => Scale * (1f - Progress * 0.3f) * (1f - Depth * 0.2f);

            public GrassBranchParticle(Vector2 pos, Vector2 vel, int life, float scale, float length, float rotation, float growSpeed, int depth, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Length = length;
                Rotation = rotation;
                GrowSpeed = growSpeed;
                Depth = depth;
                Color = color;
            }
        }

        public class GrassPetalParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpinSpeed;
            public float SpiralPhase;
            public float SpiralSpeed;
            public float SpiralRadius;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 3f);
                    float fadeOut = 1f - Progress;
                    return MathF.Max(0f, fadeIn * fadeOut * 0.8f);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.2f);

            public GrassPetalParticle(Vector2 pos, Vector2 vel, int life, float scale, float spinSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                SpinSpeed = spinSpeed;
                SpiralPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                SpiralSpeed = Main.rand.NextFloat(0.03f, 0.08f);
                SpiralRadius = Main.rand.NextFloat(3f, 8f);
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public string Name { get; set; } = "GrassTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(80, 200, 60, 160);

        public int MaxLeaves { get; set; } = 30;
        public int LeafLife { get; set; } = 40;
        public float LeafSize { get; set; } = 0.5f;
        public int LeafSpawnInterval { get; set; } = 2;
        public float LeafRotSpeed { get; set; } = 0.06f;
        public float LeafDriftSpeed { get; set; } = 0.25f;
        public float LeafSpread { get; set; } = 5f;
        public Color LeafColor { get; set; } = new Color(80, 200, 60, 210);

        public int MaxPollen { get; set; } = 35;
        public int PollenLife { get; set; } = 30;
        public float PollenSize { get; set; } = 0.25f;
        public float PollenSpawnChance { get; set; } = 0.25f;
        public float PollenDriftSpeed { get; set; } = 0.35f;
        public Color PollenColor { get; set; } = new Color(200, 230, 80, 200);

        public int MaxBranches { get; set; } = 15;
        public int BranchLife { get; set; } = 40;
        public float BranchSize { get; set; } = 0.5f;
        public float BranchLength { get; set; } = 20f;
        public float BranchSpawnChance { get; set; } = 0.06f;
        public float BranchGrowSpeed { get; set; } = 3f;
        public float BranchDriftSpeed { get; set; } = 0.08f;
        public int BranchMaxDepth { get; set; } = 2;
        public float BranchSubAngle { get; set; } = 0.6f;
        public Color BranchColor { get; set; } = new Color(60, 160, 50, 200);

        public int MaxPetals { get; set; } = 12;
        public int PetalLife { get; set; } = 50;
        public float PetalSize { get; set; } = 0.45f;
        public float PetalSpawnChance { get; set; } = 0.04f;
        public float PetalSpinSpeed { get; set; } = 0.04f;
        public float PetalDriftSpeed { get; set; } = 0.15f;
        public Color PetalColor { get; set; } = new Color(255, 180, 200, 200);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<GrassLeafParticle> leaves = new();
        private List<GrassPollenParticle> pollens = new();
        private List<GrassBranchParticle> branches = new();
        private List<GrassPetalParticle> petals = new();
        private int leafCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _leafTex;
        private Texture2D _pollenTex;
        private Texture2D _branchTex;
        private Texture2D _petalTex;
        private Texture2D _ghostTex;

        public bool HasContent => leaves.Count > 0 || pollens.Count > 0 || branches.Count > 0 || petals.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_leafTex != null) return;
            _leafTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GrassTrail/GrassTrailLeaf").Value;
            _pollenTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GrassTrail/GrassTrailPollen").Value;
            _branchTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GrassTrail/GrassTrailBranch").Value;
            _petalTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GrassTrail/GrassTrailPetal").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GrassTrail/GrassTrailGhost").Value;
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

            leafCounter++;
            if (leafCounter >= LeafSpawnInterval && leaves.Count < MaxLeaves)
            {
                leafCounter = 0;
                SpawnLeaf(center, velocity, moveDir);
            }

            if (pollens.Count < MaxPollen && Main.rand.NextFloat() < PollenSpawnChance)
                SpawnPollen(center, velocity, moveDir);

            if (branches.Count < MaxBranches && Main.rand.NextFloat() < BranchSpawnChance)
                SpawnBranch(center, velocity, moveDir, 0);

            if (petals.Count < MaxPetals && Main.rand.NextFloat() < PetalSpawnChance)
                SpawnPetal(center, velocity, moveDir);

            for (int i = leaves.Count - 1; i >= 0; i--)
            {
                var l = leaves[i];
                l.FlutterPhase += l.FlutterSpeed;
                l.Rotation += l.RotSpeed + MathF.Sin(l.FlutterPhase) * 0.02f;
                Vector2 flutter = new Vector2(MathF.Sin(l.FlutterPhase) * 0.15f, -0.05f);
                l.Velocity = l.Velocity * 0.95f + flutter;
                l.Position += l.Velocity;
                l.Life--;
                if (l.Life <= 0) leaves.RemoveAt(i);
            }

            for (int i = pollens.Count - 1; i >= 0; i--)
            {
                var p = pollens[i];
                p.TwinklePhase += p.TwinkleSpeed;
                p.Velocity *= 0.96f;
                p.Position += p.Velocity;
                p.Life--;
                if (p.Life <= 0) pollens.RemoveAt(i);
            }

            for (int i = branches.Count - 1; i >= 0; i--)
            {
                var b = branches[i];
                b.Velocity *= 0.98f;
                b.Position += b.Velocity;
                b.Life--;
                if (b.Life <= 0) branches.RemoveAt(i);
            }

            for (int i = petals.Count - 1; i >= 0; i--)
            {
                var p = petals[i];
                p.SpiralPhase += p.SpiralSpeed;
                p.Rotation += p.SpinSpeed;
                Vector2 spiralOffset = new Vector2(
                    MathF.Cos(p.SpiralPhase) * p.SpiralRadius * 0.05f,
                    MathF.Sin(p.SpiralPhase) * p.SpiralRadius * 0.05f
                );
                p.Velocity = p.Velocity * 0.97f + spiralOffset + new Vector2(0f, 0.02f);
                p.Position += p.Velocity;
                p.Life--;
                if (p.Life <= 0) petals.RemoveAt(i);
            }
        }

        private void SpawnLeaf(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-LeafSpread, LeafSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            Vector2 drift = Main.rand.NextVector2Circular(LeafDriftSpeed, LeafDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.6f, 1.3f) * LeafSize;
            float rotSpeed = Main.rand.NextFloat(-LeafRotSpeed, LeafRotSpeed);
            Color color = LeafColor * Main.rand.NextFloat(0.6f, 1f);

            leaves.Add(new GrassLeafParticle(pos, vel, LeafLife, scale, rotSpeed, color));
        }

        private void SpawnPollen(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-7f, 7f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(4f, 4f);

            Vector2 drift = Main.rand.NextVector2Circular(PollenDriftSpeed, PollenDriftSpeed);
            float scale = Main.rand.NextFloat(0.5f, 1.2f) * PollenSize;
            Color color = PollenColor * Main.rand.NextFloat(0.5f, 1f);

            pollens.Add(new GrassPollenParticle(pos, drift, PollenLife, scale, color));
        }

        private void SpawnBranch(Vector2 center, Vector2 velocity, Vector2 moveDir, int depth)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-6f, 6f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 drift = Main.rand.NextVector2Circular(BranchDriftSpeed, BranchDriftSpeed);
            float baseAngle = -MathHelper.PiOver2 + Main.rand.NextFloat(-0.8f, 0.8f);
            float length = BranchLength * Main.rand.NextFloat(0.6f, 1.2f) / (1f + depth * 0.5f);
            float growSpeed = BranchGrowSpeed * Main.rand.NextFloat(0.8f, 1.3f);
            float scale = BranchSize * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = BranchColor * Main.rand.NextFloat(0.5f, 1f);

            branches.Add(new GrassBranchParticle(pos, drift, BranchLife, scale, length, baseAngle, growSpeed, depth, color));

            if (depth < BranchMaxDepth && Main.rand.NextFloat() < 0.5f)
            {
                Vector2 tipPos = pos + new Vector2(MathF.Cos(baseAngle), MathF.Sin(baseAngle)) * length * 0.7f;
                float subAngle1 = baseAngle + BranchSubAngle * Main.rand.NextFloat(0.5f, 1.5f);
                float subLength = length * Main.rand.NextFloat(0.4f, 0.7f);

                branches.Add(new GrassBranchParticle(tipPos, drift * 0.5f, BranchLife - 5, scale * 0.7f, subLength, subAngle1, growSpeed, depth + 1, color * 0.8f));

                if (Main.rand.NextFloat() < 0.4f)
                {
                    float subAngle2 = baseAngle - BranchSubAngle * Main.rand.NextFloat(0.5f, 1.5f);
                    float subLength2 = length * Main.rand.NextFloat(0.3f, 0.6f);

                    branches.Add(new GrassBranchParticle(tipPos, drift * 0.5f, BranchLife - 8, scale * 0.6f, subLength2, subAngle2, growSpeed, depth + 1, color * 0.7f));
                }
            }
        }

        private void SpawnPetal(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-8f, 8f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(5f, 5f);

            Vector2 inertia = -velocity * InertiaFactor * 0.2f;
            Vector2 drift = Main.rand.NextVector2Circular(PetalDriftSpeed, PetalDriftSpeed);
            Vector2 vel = inertia + drift + new Vector2(0f, -0.3f);

            float scale = Main.rand.NextFloat(0.6f, 1.3f) * PetalSize;
            float spinSpeed = Main.rand.NextFloat(-PetalSpinSpeed, PetalSpinSpeed);
            Color color = PetalColor * Main.rand.NextFloat(0.5f, 1f);

            petals.Add(new GrassPetalParticle(pos, vel, PetalLife, scale, spinSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (branches.Count > 0 && _branchTex != null)
            {
                Vector2 branchOrigin = new Vector2(0f, _branchTex.Height * 0.5f);
                var sortedBranches = branches.OrderBy(b => b.Depth).ThenBy(b => b.Life);
                foreach (var b in sortedBranches)
                {
                    Color drawColor = b.Color * b.Alpha;
                    Vector2 pos = b.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(b.CurrentLength / _branchTex.Width, b.CurrentWidth);
                    sb.Draw(_branchTex, pos, null, drawColor, b.Rotation,
                        branchOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (leaves.Count > 0 && _leafTex != null)
            {
                Vector2 leafOrigin = _leafTex.Size() * 0.5f;
                var sortedLeaves = leaves.OrderBy(l => l.Life);
                foreach (var l in sortedLeaves)
                {
                    Color drawColor = l.Color * l.Alpha;
                    Vector2 pos = l.Position - Main.screenPosition;
                    sb.Draw(_leafTex, pos, null, drawColor, l.Rotation,
                        leafOrigin, l.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (petals.Count > 0 && _petalTex != null)
            {
                Vector2 petalOrigin = _petalTex.Size() * 0.5f;
                var sortedPetals = petals.OrderBy(p => p.Life);
                foreach (var p in sortedPetals)
                {
                    Color drawColor = p.Color * p.Alpha;
                    Vector2 pos = p.Position - Main.screenPosition;
                    sb.Draw(_petalTex, pos, null, drawColor, p.Rotation,
                        petalOrigin, p.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (pollens.Count > 0 && _pollenTex != null)
            {
                Vector2 pollenOrigin = _pollenTex.Size() * 0.5f;
                var sortedPollens = pollens.OrderBy(p => p.Life);
                foreach (var p in sortedPollens)
                {
                    Color drawColor = p.Color * p.Alpha;
                    Vector2 pos = p.Position - Main.screenPosition;
                    sb.Draw(_pollenTex, pos, null, drawColor, 0f,
                        pollenOrigin, p.Scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            leaves.Clear();
            pollens.Clear();
            branches.Clear();
            petals.Clear();
            leafCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
