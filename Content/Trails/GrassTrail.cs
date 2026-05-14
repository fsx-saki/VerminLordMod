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

        public class GrassBloomParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float Rotation;
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
                    float expand = MathF.Min(1f, Progress * 2f);
                    return Scale + (MaxScale - Scale) * expand;
                }
            }

            public GrassBloomParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public class GrassVineConnection
        {
            public Vector2 Start;
            public Vector2 End;
            public float Alpha;
            public Color Color;
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

        public int MaxBlooms { get; set; } = 5;
        public int BloomLife { get; set; } = 50;
        public float BloomStartSize { get; set; } = 0.3f;
        public float BloomEndSize { get; set; } = 1.5f;
        public float BloomSpawnChance { get; set; } = 0.02f;
        public float BloomDriftSpeed { get; set; } = 0.08f;
        public Color BloomColor { get; set; } = new Color(150, 230, 120, 140);

        public float VineMaxDistance { get; set; } = 45f;
        public float VineBreakDistance { get; set; } = 70f;
        public float VineBaseAlpha { get; set; } = 0.25f;
        public Color VineColor { get; set; } = new Color(60, 160, 50, 180);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<GrassLeafParticle> leaves = new();
        private List<GrassPollenParticle> pollens = new();
        private List<GrassBloomParticle> blooms = new();
        private int leafCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _leafTex;
        private Texture2D _pollenTex;
        private Texture2D _bloomTex;
        private Texture2D _vineTex;
        private Texture2D _ghostTex;

        public bool HasContent => leaves.Count > 0 || pollens.Count > 0 || blooms.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_leafTex != null) return;
            _leafTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GrassTrail/GrassTrailLeaf").Value;
            _pollenTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GrassTrail/GrassTrailPollen").Value;
            _bloomTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GrassTrail/GrassTrailBloom").Value;
            _vineTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/GrassTrail/GrassTrailVine").Value;
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

            if (blooms.Count < MaxBlooms && Main.rand.NextFloat() < BloomSpawnChance)
                SpawnBloom(center, velocity, moveDir);

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

            for (int i = blooms.Count - 1; i >= 0; i--)
            {
                var b = blooms[i];
                b.Velocity *= 0.98f;
                b.Position += b.Velocity;
                b.Life--;
                if (b.Life <= 0) blooms.RemoveAt(i);
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

        private void SpawnBloom(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-8f, 8f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(6f, 6f);

            Vector2 drift = Main.rand.NextVector2Circular(BloomDriftSpeed, BloomDriftSpeed);
            float startSize = BloomStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = BloomEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = BloomColor * Main.rand.NextFloat(0.5f, 1f);

            blooms.Add(new GrassBloomParticle(pos, drift, BloomLife, startSize, endSize, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (blooms.Count > 0 && _bloomTex != null)
            {
                Vector2 bloomOrigin = _bloomTex.Size() * 0.5f;
                var sortedBlooms = blooms.OrderBy(b => b.Life);
                foreach (var b in sortedBlooms)
                {
                    Color drawColor = b.Color * b.Alpha;
                    Vector2 pos = b.Position - Main.screenPosition;
                    sb.Draw(_bloomTex, pos, null, drawColor, b.Rotation,
                        bloomOrigin, b.CurrentScale, SpriteEffects.None, 0);
                }
            }

            DrawVineConnections(sb);

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

        private void DrawVineConnections(SpriteBatch sb)
        {
            if (_vineTex == null || leaves.Count < 2) return;

            Vector2 vineOrigin = new Vector2(0f, _vineTex.Height * 0.5f);

            for (int i = 0; i < leaves.Count; i++)
            {
                var a = leaves[i];
                if (a.Alpha < 0.05f) continue;

                for (int j = i + 1; j < leaves.Count; j++)
                {
                    var b = leaves[j];
                    if (b.Alpha < 0.05f) continue;

                    float dist = Vector2.Distance(a.Position, b.Position);
                    if (dist > VineBreakDistance || dist < 3f) continue;

                    float vineAlpha;
                    if (dist <= VineMaxDistance)
                    {
                        vineAlpha = VineBaseAlpha;
                    }
                    else
                    {
                        float breakProgress = (dist - VineMaxDistance) / (VineBreakDistance - VineMaxDistance);
                        vineAlpha = VineBaseAlpha * (1f - breakProgress * breakProgress);
                    }

                    float minLeafAlpha = MathF.Min(a.Alpha, b.Alpha);
                    vineAlpha *= minLeafAlpha;

                    if (vineAlpha < 0.01f) continue;

                    Vector2 start = a.Position - Main.screenPosition;
                    Vector2 end = b.Position - Main.screenPosition;
                    Vector2 diff = end - start;
                    float length = diff.Length();
                    if (length < 1f) continue;

                    float rotation = diff.ToRotation();
                    Vector2 scale = new Vector2(length / _vineTex.Width, 0.6f);
                    Color drawColor = VineColor * vineAlpha;

                    sb.Draw(_vineTex, start, null, drawColor, rotation,
                        vineOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            leaves.Clear();
            pollens.Clear();
            blooms.Clear();
            leafCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
