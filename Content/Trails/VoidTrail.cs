using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class VoidTrail : ITrail
    {
        public class VoidOrbParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float PulsePhase;
            public float PulseSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    float pulse = 0.7f + 0.3f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, fadeIn * fadeOut * pulse);
                }
            }

            public float CurrentScale => Scale * (1f + MathF.Sin(PulsePhase) * 0.1f);

            public VoidOrbParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = Main.rand.NextFloat(0.06f, 0.14f);
                Color = color;
            }
        }

        public class VoidDistortionParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 3f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.5f);
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

            public VoidDistortionParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float rotSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                Color = color;
            }
        }

        public class VoidShardParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
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
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = 1f - Progress;
                    return MathF.Max(0f, fadeIn * fadeOut);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.4f);

            public VoidShardParticle(Vector2 pos, Vector2 vel, int life, float scale, float spinSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                SpinSpeed = spinSpeed;
                Color = color;
            }
        }

        public class VoidTendrilConnection
        {
            public Vector2 Start;
            public Vector2 End;
            public float Alpha;
            public Color Color;
        }

        public string Name { get; set; } = "VoidTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.35f;
        public Color GhostColor { get; set; } = new Color(80, 20, 130, 160);

        public int MaxOrbs { get; set; } = 25;
        public int OrbLife { get; set; } = 40;
        public float OrbSize { get; set; } = 0.5f;
        public int OrbSpawnInterval { get; set; } = 2;
        public float OrbRotSpeed { get; set; } = 0.06f;
        public float OrbDriftSpeed { get; set; } = 0.2f;
        public float OrbSpread { get; set; } = 5f;
        public Color OrbColor { get; set; } = new Color(80, 15, 140, 220);

        public int MaxDistortions { get; set; } = 5;
        public int DistortionLife { get; set; } = 50;
        public float DistortionStartSize { get; set; } = 0.3f;
        public float DistortionEndSize { get; set; } = 2.0f;
        public float DistortionSpawnChance { get; set; } = 0.02f;
        public float DistortionRotSpeed { get; set; } = 0.05f;
        public float DistortionDriftSpeed { get; set; } = 0.08f;
        public Color DistortionColor { get; set; } = new Color(90, 20, 130, 160);

        public int MaxShards { get; set; } = 20;
        public int ShardLife { get; set; } = 25;
        public float ShardSize { get; set; } = 0.4f;
        public float ShardSpawnChance { get; set; } = 0.15f;
        public float ShardSpinSpeed { get; set; } = 0.12f;
        public float ShardDriftSpeed { get; set; } = 0.3f;
        public Color ShardColor { get; set; } = new Color(70, 15, 110, 200);

        public float TendrilMaxDistance { get; set; } = 50f;
        public float TendrilBreakDistance { get; set; } = 80f;
        public float TendrilBaseAlpha { get; set; } = 0.2f;
        public Color TendrilColor { get; set; } = new Color(70, 15, 110, 180);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<VoidOrbParticle> orbs = new();
        private List<VoidDistortionParticle> distortions = new();
        private List<VoidShardParticle> shards = new();
        private int orbCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _orbTex;
        private Texture2D _distortionTex;
        private Texture2D _shardTex;
        private Texture2D _tendrilTex;
        private Texture2D _ghostTex;

        public bool HasContent => orbs.Count > 0 || distortions.Count > 0 || shards.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_orbTex != null) return;
            _orbTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/VoidTrail/VoidTrailOrb").Value;
            _distortionTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/VoidTrail/VoidTrailDistortion").Value;
            _shardTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/VoidTrail/VoidTrailShard").Value;
            _tendrilTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/VoidTrail/VoidTrailTendril").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/VoidTrail/VoidTrailGhost").Value;
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

            orbCounter++;
            if (orbCounter >= OrbSpawnInterval && orbs.Count < MaxOrbs)
            {
                orbCounter = 0;
                SpawnOrb(center, velocity, moveDir);
            }

            if (distortions.Count < MaxDistortions && Main.rand.NextFloat() < DistortionSpawnChance)
                SpawnDistortion(center, velocity, moveDir);

            if (shards.Count < MaxShards && Main.rand.NextFloat() < ShardSpawnChance)
                SpawnShard(center, velocity, moveDir);

            for (int i = orbs.Count - 1; i >= 0; i--)
            {
                var o = orbs[i];
                o.Rotation += o.RotSpeed;
                o.PulsePhase += o.PulseSpeed;
                o.Velocity *= 0.96f;
                o.Position += o.Velocity;
                o.Life--;
                if (o.Life <= 0) orbs.RemoveAt(i);
            }

            for (int i = distortions.Count - 1; i >= 0; i--)
            {
                var d = distortions[i];
                d.Rotation += d.RotSpeed;
                d.Velocity *= 0.98f;
                d.Position += d.Velocity;
                d.Life--;
                if (d.Life <= 0) distortions.RemoveAt(i);
            }

            for (int i = shards.Count - 1; i >= 0; i--)
            {
                var s = shards[i];
                s.Rotation += s.SpinSpeed;
                s.Velocity *= 0.94f;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0) shards.RemoveAt(i);
            }
        }

        private void SpawnOrb(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-OrbSpread, OrbSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            Vector2 drift = Main.rand.NextVector2Circular(OrbDriftSpeed, OrbDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.6f, 1.2f) * OrbSize;
            float rotSpeed = Main.rand.NextFloat(-OrbRotSpeed, OrbRotSpeed);
            Color color = OrbColor * Main.rand.NextFloat(0.5f, 1f);

            orbs.Add(new VoidOrbParticle(pos, vel, OrbLife, scale, rotSpeed, color));
        }

        private void SpawnDistortion(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-8f, 8f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(6f, 6f);

            Vector2 drift = Main.rand.NextVector2Circular(DistortionDriftSpeed, DistortionDriftSpeed);
            float startSize = DistortionStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = DistortionEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            float rotSpeed = Main.rand.NextFloat(-DistortionRotSpeed, DistortionRotSpeed);
            Color color = DistortionColor * Main.rand.NextFloat(0.5f, 1f);

            distortions.Add(new VoidDistortionParticle(pos, drift, DistortionLife, startSize, endSize, rotSpeed, color));
        }

        private void SpawnShard(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-5f, 5f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 inertia = -velocity * InertiaFactor * 0.2f;
            Vector2 drift = Main.rand.NextVector2Circular(ShardDriftSpeed, ShardDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.5f, 1.2f) * ShardSize;
            float spinSpeed = Main.rand.NextFloat(-ShardSpinSpeed, ShardSpinSpeed);
            Color color = ShardColor * Main.rand.NextFloat(0.5f, 1f);

            shards.Add(new VoidShardParticle(pos, vel, ShardLife, scale, spinSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (distortions.Count > 0 && _distortionTex != null)
            {
                Vector2 distOrigin = _distortionTex.Size() * 0.5f;
                var sortedDistortions = distortions.OrderBy(d => d.Life);
                foreach (var d in sortedDistortions)
                {
                    Color drawColor = d.Color * d.Alpha;
                    Vector2 pos = d.Position - Main.screenPosition;
                    sb.Draw(_distortionTex, pos, null, drawColor, d.Rotation,
                        distOrigin, d.CurrentScale, SpriteEffects.None, 0);
                }
            }

            DrawTendrilConnections(sb);

            if (orbs.Count > 0 && _orbTex != null)
            {
                Vector2 orbOrigin = _orbTex.Size() * 0.5f;
                var sortedOrbs = orbs.OrderBy(o => o.Life);
                foreach (var o in sortedOrbs)
                {
                    Color drawColor = o.Color * o.Alpha;
                    Vector2 pos = o.Position - Main.screenPosition;
                    sb.Draw(_orbTex, pos, null, drawColor, o.Rotation,
                        orbOrigin, o.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (shards.Count > 0 && _shardTex != null)
            {
                Vector2 shardOrigin = _shardTex.Size() * 0.5f;
                var sortedShards = shards.OrderBy(s => s.Life);
                foreach (var s in sortedShards)
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    sb.Draw(_shardTex, pos, null, drawColor, s.Rotation,
                        shardOrigin, s.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        private void DrawTendrilConnections(SpriteBatch sb)
        {
            if (_tendrilTex == null || orbs.Count < 2) return;

            Vector2 tendrilOrigin = new Vector2(0f, _tendrilTex.Height * 0.5f);

            for (int i = 0; i < orbs.Count; i++)
            {
                var a = orbs[i];
                if (a.Alpha < 0.05f) continue;

                for (int j = i + 1; j < orbs.Count; j++)
                {
                    var b = orbs[j];
                    if (b.Alpha < 0.05f) continue;

                    float dist = Vector2.Distance(a.Position, b.Position);
                    if (dist > TendrilBreakDistance || dist < 3f) continue;

                    float tendrilAlpha;
                    if (dist <= TendrilMaxDistance)
                    {
                        tendrilAlpha = TendrilBaseAlpha;
                    }
                    else
                    {
                        float breakProgress = (dist - TendrilMaxDistance) / (TendrilBreakDistance - TendrilMaxDistance);
                        tendrilAlpha = TendrilBaseAlpha * (1f - breakProgress * breakProgress);
                    }

                    float minOrbAlpha = MathF.Min(a.Alpha, b.Alpha);
                    tendrilAlpha *= minOrbAlpha;

                    if (tendrilAlpha < 0.01f) continue;

                    Vector2 start = a.Position - Main.screenPosition;
                    Vector2 end = b.Position - Main.screenPosition;
                    Vector2 diff = end - start;
                    float length = diff.Length();
                    if (length < 1f) continue;

                    float rotation = diff.ToRotation();
                    Vector2 scale = new Vector2(length / _tendrilTex.Width, 0.5f);
                    Color drawColor = TendrilColor * tendrilAlpha;

                    sb.Draw(_tendrilTex, start, null, drawColor, rotation,
                        tendrilOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            orbs.Clear();
            distortions.Clear();
            shards.Clear();
            orbCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
