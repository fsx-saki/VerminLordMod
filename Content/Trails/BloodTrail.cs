using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class BloodTrail : ITrail
    {
        public class BloodDropParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Gravity;
            public bool HasSplattered;
            public float Stretch;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    if (HasSplattered)
                    {
                        float fadeOut = 1f - Progress;
                        return MathF.Max(0f, fadeOut * fadeOut);
                    }
                    float fadeIn = MathF.Min(1f, Progress * 6f);
                    float fadeOut2 = 1f - Progress * 0.5f;
                    return MathF.Max(0f, fadeIn * fadeOut2);
                }
            }

            public float CurrentScale => HasSplattered ? Scale * (1f + Progress * 0.5f) : Scale * (1f - Progress * 0.2f);

            public float CurrentStretch => HasSplattered ? 1f : Stretch * (1f - Progress * 0.3f);

            public BloodDropParticle(Vector2 pos, Vector2 vel, int life, float scale, float gravity, float stretch, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Gravity = gravity;
                HasSplattered = false;
                Stretch = stretch;
                Color = color;
            }
        }

        public class BloodVeinParticle
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
                    return MathF.Max(0f, growIn * fadeOut * pulse);
                }
            }

            public float CurrentLength => Length * MathF.Min(1f, Progress * GrowSpeed);

            public float CurrentWidth => Scale * (1f - Depth * 0.25f) * (1f - Progress * 0.4f);

            public BloodVeinParticle(Vector2 pos, Vector2 vel, int life, float scale, float length, float rotation, float growSpeed, int depth, Color color)
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
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = Main.rand.NextFloat(0.06f, 0.14f);
                Color = color;
            }
        }

        public class BloodMistParticle
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
            public Color Color;

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

            public BloodMistParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float expandSpeed, float rotSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                ExpandSpeed = expandSpeed;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                Color = color;
            }
        }

        public string Name { get; set; } = "BloodTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.25f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.4f;
        public Color GhostColor { get; set; } = new Color(180, 30, 30, 180);

        public int MaxDrops { get; set; } = 30;
        public int DropLife { get; set; } = 35;
        public float DropSize { get; set; } = 0.5f;
        public int DropSpawnInterval { get; set; } = 1;
        public float DropGravity { get; set; } = 0.15f;
        public float DropStretch { get; set; } = 2.5f;
        public float DropSpread { get; set; } = 5f;
        public float DropSpeed { get; set; } = 0.4f;
        public Color DropColor { get; set; } = new Color(200, 30, 30, 230);

        public int MaxVeins { get; set; } = 12;
        public int VeinLife { get; set; } = 45;
        public float VeinSize { get; set; } = 0.5f;
        public float VeinLength { get; set; } = 18f;
        public float VeinSpawnChance { get; set; } = 0.05f;
        public float VeinGrowSpeed { get; set; } = 3f;
        public float VeinDriftSpeed { get; set; } = 0.06f;
        public int VeinMaxDepth { get; set; } = 2;
        public float VeinSubAngle { get; set; } = 0.7f;
        public Color VeinColor { get; set; } = new Color(160, 20, 40, 210);

        public int MaxMists { get; set; } = 15;
        public int MistLife { get; set; } = 50;
        public float MistStartSize { get; set; } = 0.3f;
        public float MistEndSize { get; set; } = 1.8f;
        public float MistSpawnChance { get; set; } = 0.03f;
        public float MistExpandSpeed { get; set; } = 2f;
        public float MistRotSpeed { get; set; } = 0.02f;
        public float MistDriftSpeed { get; set; } = 0.08f;
        public Color MistColor { get; set; } = new Color(140, 20, 30, 160);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<BloodDropParticle> drops = new();
        private List<BloodVeinParticle> veins = new();
        private List<BloodMistParticle> mists = new();
        private int dropCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _dropTex;
        private Texture2D _veinTex;
        private Texture2D _mistTex;
        private Texture2D _ghostTex;

        public bool HasContent => drops.Count > 0 || veins.Count > 0 || mists.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_dropTex != null) return;
            _dropTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/BloodTrail/BloodTrailDrop").Value;
            _veinTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/BloodTrail/BloodTrailVein").Value;
            _mistTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/BloodTrail/BloodTrailMist").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/BloodTrail/BloodTrailGhost").Value;
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

            dropCounter++;
            if (dropCounter >= DropSpawnInterval && drops.Count < MaxDrops)
            {
                dropCounter = 0;
                SpawnDrop(center, velocity, moveDir);
            }

            if (veins.Count < MaxVeins && Main.rand.NextFloat() < VeinSpawnChance)
                SpawnVein(center, velocity, moveDir, 0);

            if (mists.Count < MaxMists && Main.rand.NextFloat() < MistSpawnChance)
                SpawnMist(center, velocity, moveDir);

            for (int i = drops.Count - 1; i >= 0; i--)
            {
                var d = drops[i];
                if (!d.HasSplattered)
                {
                    d.Velocity.Y += DropGravity;
                    d.Velocity.X *= 0.99f;
                    d.Position += d.Velocity;

                    if (d.Progress > 0.6f && !d.HasSplattered)
                    {
                        d.HasSplattered = true;
                        d.Velocity = Vector2.Zero;
                    }
                }
                else
                {
                    d.Position += d.Velocity;
                }
                d.Life--;
                if (d.Life <= 0) drops.RemoveAt(i);
            }

            for (int i = veins.Count - 1; i >= 0; i--)
            {
                var v = veins[i];
                v.PulsePhase += v.PulseSpeed;
                v.Velocity *= 0.98f;
                v.Position += v.Velocity;
                v.Life--;
                if (v.Life <= 0) veins.RemoveAt(i);
            }

            for (int i = mists.Count - 1; i >= 0; i--)
            {
                var m = mists[i];
                m.Rotation += m.RotSpeed;
                m.Velocity *= 0.97f;
                m.Position += m.Velocity;
                m.Life--;
                if (m.Life <= 0) mists.RemoveAt(i);
            }
        }

        private void SpawnDrop(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-DropSpread, DropSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            Vector2 drift = Main.rand.NextVector2Circular(DropSpeed, DropSpeed);
            Vector2 gravity = new Vector2(0f, Main.rand.NextFloat(0.5f, 1.5f));
            Vector2 vel = inertia + drift + gravity;

            float scale = Main.rand.NextFloat(0.5f, 1.3f) * DropSize;
            float stretch = Main.rand.NextFloat(0.8f, 1.5f) * DropStretch;
            Color color = DropColor * Main.rand.NextFloat(0.6f, 1f);

            drops.Add(new BloodDropParticle(pos, vel, DropLife, scale, DropGravity, stretch, color));
        }

        private void SpawnVein(Vector2 center, Vector2 velocity, Vector2 moveDir, int depth)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-5f, 5f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 drift = Main.rand.NextVector2Circular(VeinDriftSpeed, VeinDriftSpeed);
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            float length = VeinLength * Main.rand.NextFloat(0.6f, 1.2f) / (1f + depth * 0.5f);
            float growSpeed = VeinGrowSpeed * Main.rand.NextFloat(0.8f, 1.3f);
            float scale = VeinSize * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = VeinColor * Main.rand.NextFloat(0.5f, 1f);

            veins.Add(new BloodVeinParticle(pos, drift, VeinLife, scale, length, baseAngle, growSpeed, depth, color));

            if (depth < VeinMaxDepth && Main.rand.NextFloat() < 0.55f)
            {
                Vector2 tipPos = pos + new Vector2(MathF.Cos(baseAngle), MathF.Sin(baseAngle)) * length * 0.65f;
                float subAngle1 = baseAngle + VeinSubAngle * Main.rand.NextFloat(0.5f, 1.5f);
                float subLength = length * Main.rand.NextFloat(0.4f, 0.7f);

                veins.Add(new BloodVeinParticle(tipPos, drift * 0.5f, VeinLife - 5, scale * 0.7f, subLength, subAngle1, growSpeed, depth + 1, color * 0.8f));

                if (Main.rand.NextFloat() < 0.4f)
                {
                    float subAngle2 = baseAngle - VeinSubAngle * Main.rand.NextFloat(0.5f, 1.5f);
                    float subLength2 = length * Main.rand.NextFloat(0.3f, 0.6f);

                    veins.Add(new BloodVeinParticle(tipPos, drift * 0.5f, VeinLife - 8, scale * 0.6f, subLength2, subAngle2, growSpeed, depth + 1, color * 0.7f));
                }
            }
        }

        private void SpawnMist(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-8f, 8f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(6f, 6f);

            Vector2 drift = Main.rand.NextVector2Circular(MistDriftSpeed, MistDriftSpeed);
            float startSize = MistStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = MistEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            float expandSpeed = MistExpandSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float rotSpeed = Main.rand.NextFloat(-MistRotSpeed, MistRotSpeed);
            Color color = MistColor * Main.rand.NextFloat(0.5f, 1f);

            mists.Add(new BloodMistParticle(pos, drift, MistLife, startSize, endSize, expandSpeed, rotSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (mists.Count > 0 && _mistTex != null)
            {
                Vector2 mistOrigin = _mistTex.Size() * 0.5f;
                var sortedMists = mists.OrderBy(m => m.Life);
                foreach (var m in sortedMists)
                {
                    Color drawColor = m.Color * m.Alpha;
                    Vector2 pos = m.Position - Main.screenPosition;
                    sb.Draw(_mistTex, pos, null, drawColor, m.Rotation,
                        mistOrigin, m.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (veins.Count > 0 && _veinTex != null)
            {
                Vector2 veinOrigin = new Vector2(0f, _veinTex.Height * 0.5f);
                var sortedVeins = veins.OrderBy(v => v.Depth).ThenBy(v => v.Life);
                foreach (var v in sortedVeins)
                {
                    Color drawColor = v.Color * v.Alpha;
                    Vector2 pos = v.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(v.CurrentLength / _veinTex.Width, v.CurrentWidth);
                    sb.Draw(_veinTex, pos, null, drawColor, v.Rotation,
                        veinOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (drops.Count > 0 && _dropTex != null)
            {
                Vector2 dropOrigin = new Vector2(_dropTex.Width * 0.5f, _dropTex.Height * 0.5f);
                var sortedDrops = drops.OrderBy(d => d.Life);
                foreach (var d in sortedDrops)
                {
                    Color drawColor = d.Color * d.Alpha;
                    Vector2 pos = d.Position - Main.screenPosition;
                    float angle = d.HasSplattered ? 0f : d.Velocity.ToRotation();
                    Vector2 scale = d.HasSplattered
                        ? new Vector2(d.CurrentScale * 1.5f, d.CurrentScale * 0.6f)
                        : new Vector2(d.CurrentScale * d.CurrentStretch, d.CurrentScale);
                    sb.Draw(_dropTex, pos, null, drawColor, angle,
                        dropOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            drops.Clear();
            veins.Clear();
            mists.Clear();
            dropCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
