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

        public class VoidRiftParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float OpenSpeed;
            public float AspectRatio;
            public Color EdgeColor;
            public Color CoreColor;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float openIn = MathF.Min(1f, Progress * OpenSpeed);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, openIn * fadeOut);
                }
            }

            public float CurrentScale
            {
                get
                {
                    float openProgress = MathF.Min(1f, Progress * OpenSpeed);
                    return Scale + (MaxScale - Scale) * openProgress;
                }
            }

            public Color CurrentEdgeColor => EdgeColor * Alpha;

            public Color CurrentCoreColor => CoreColor * (Alpha * 0.6f);

            public VoidRiftParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float rotation, float openSpeed, float aspectRatio, Color edgeColor, Color coreColor)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                Rotation = rotation;
                OpenSpeed = openSpeed;
                AspectRatio = aspectRatio;
                EdgeColor = edgeColor;
                CoreColor = coreColor;
            }
        }

        public class VoidInflowParticle
        {
            public Vector2 Position;
            public Vector2 Target;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float PullStrength;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = 1f - Progress * Progress;
                    return MathF.Max(0f, fadeIn * fadeOut);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.7f);

            public VoidInflowParticle(Vector2 pos, Vector2 target, int life, float scale, float pullStrength, Color color)
            {
                Position = pos;
                Target = target;
                MaxLife = life;
                Life = life;
                Scale = scale;
                PullStrength = pullStrength;
                Color = color;
            }
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

        public int MaxShards { get; set; } = 20;
        public int ShardLife { get; set; } = 25;
        public float ShardSize { get; set; } = 0.4f;
        public float ShardSpawnChance { get; set; } = 0.15f;
        public float ShardSpinSpeed { get; set; } = 0.12f;
        public float ShardDriftSpeed { get; set; } = 0.3f;
        public Color ShardColor { get; set; } = new Color(70, 15, 110, 200);

        public int MaxRifts { get; set; } = 6;
        public int RiftLife { get; set; } = 50;
        public float RiftStartSize { get; set; } = 0.2f;
        public float RiftEndSize { get; set; } = 1.5f;
        public float RiftSpawnChance { get; set; } = 0.025f;
        public float RiftOpenSpeed { get; set; } = 3f;
        public float RiftDriftSpeed { get; set; } = 0.06f;
        public float RiftAspectRatio { get; set; } = 2.5f;
        public Color RiftEdgeColor { get; set; } = new Color(140, 50, 200, 200);
        public Color RiftCoreColor { get; set; } = new Color(20, 5, 40, 180);

        public int MaxInflows { get; set; } = 30;
        public int InflowLife { get; set; } = 25;
        public float InflowSize { get; set; } = 0.35f;
        public float InflowSpawnChance { get; set; } = 0.2f;
        public float InflowPullStrength { get; set; } = 0.12f;
        public float InflowSpread { get; set; } = 25f;
        public Color InflowColor { get; set; } = new Color(100, 30, 160, 200);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<VoidOrbParticle> orbs = new();
        private List<VoidShardParticle> shards = new();
        private List<VoidRiftParticle> rifts = new();
        private List<VoidInflowParticle> inflows = new();
        private int orbCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _orbTex;
        private Texture2D _shardTex;
        private Texture2D _riftTex;
        private Texture2D _inflowTex;
        private Texture2D _ghostTex;

        private Vector2 _lastCenter;

        public bool HasContent => orbs.Count > 0 || shards.Count > 0 || rifts.Count > 0 || inflows.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_orbTex != null) return;
            _orbTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/VoidTrail/VoidTrailOrb").Value;
            _shardTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/VoidTrail/VoidTrailShard").Value;
            _riftTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/VoidTrail/VoidTrailRift").Value;
            _inflowTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/VoidTrail/VoidTrailInflow").Value;
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

            _lastCenter = center;

            if (_ghostTrail != null)
                _ghostTrail.Update(center, velocity);

            Vector2 moveDir = velocity.SafeNormalize(Vector2.UnitX);

            orbCounter++;
            if (orbCounter >= OrbSpawnInterval && orbs.Count < MaxOrbs)
            {
                orbCounter = 0;
                SpawnOrb(center, velocity, moveDir);
            }

            if (shards.Count < MaxShards && Main.rand.NextFloat() < ShardSpawnChance)
                SpawnShard(center, velocity, moveDir);

            if (rifts.Count < MaxRifts && Main.rand.NextFloat() < RiftSpawnChance)
                SpawnRift(center, velocity, moveDir);

            if (inflows.Count < MaxInflows && Main.rand.NextFloat() < InflowSpawnChance)
                SpawnInflow(center, velocity);

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

            for (int i = shards.Count - 1; i >= 0; i--)
            {
                var s = shards[i];
                s.Rotation += s.SpinSpeed;
                s.Velocity *= 0.94f;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0) shards.RemoveAt(i);
            }

            for (int i = rifts.Count - 1; i >= 0; i--)
            {
                var r = rifts[i];
                r.Velocity *= 0.98f;
                r.Position += r.Velocity;
                r.Life--;
                if (r.Life <= 0) rifts.RemoveAt(i);
            }

            for (int i = inflows.Count - 1; i >= 0; i--)
            {
                var inf = inflows[i];
                Vector2 toTarget = inf.Target - inf.Position;
                float dist = toTarget.Length();
                if (dist > 1f)
                {
                    float pullForce = inf.PullStrength * (1f + (1f - inf.Progress) * 2f);
                    inf.Position += toTarget.SafeNormalize(Vector2.Zero) * pullForce * dist * 0.05f;
                }
                inf.Life--;
                if (inf.Life <= 0 || dist < 3f) inflows.RemoveAt(i);
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

        private void SpawnRift(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-8f, 8f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(6f, 6f);

            Vector2 drift = Main.rand.NextVector2Circular(RiftDriftSpeed, RiftDriftSpeed);
            float startSize = RiftStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = RiftEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            float openSpeed = RiftOpenSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float aspectRatio = RiftAspectRatio * Main.rand.NextFloat(0.8f, 1.3f);
            Color edgeColor = RiftEdgeColor * Main.rand.NextFloat(0.6f, 1f);
            Color coreColor = RiftCoreColor * Main.rand.NextFloat(0.5f, 1f);

            rifts.Add(new VoidRiftParticle(pos, drift, RiftLife, startSize, endSize, rotation, openSpeed, aspectRatio, edgeColor, coreColor));
        }

        private void SpawnInflow(Vector2 center, Vector2 velocity)
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float dist = InflowSpread * Main.rand.NextFloat(0.5f, 1.5f);
            Vector2 pos = center + SpawnOffset + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * dist;

            float scale = Main.rand.NextFloat(0.5f, 1.2f) * InflowSize;
            float pullStrength = InflowPullStrength * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = InflowColor * Main.rand.NextFloat(0.5f, 1f);

            inflows.Add(new VoidInflowParticle(pos, center, InflowLife, scale, pullStrength, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (rifts.Count > 0 && _riftTex != null)
            {
                Vector2 riftOrigin = _riftTex.Size() * 0.5f;
                var sortedRifts = rifts.OrderBy(r => r.Life);
                foreach (var r in sortedRifts)
                {
                    Vector2 pos = r.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(r.CurrentScale * r.AspectRatio, r.CurrentScale);

                    Color coreDrawColor = r.CurrentCoreColor;
                    sb.Draw(_riftTex, pos, null, coreDrawColor, r.Rotation,
                        riftOrigin, scale, SpriteEffects.None, 0);

                    Color edgeDrawColor = r.CurrentEdgeColor;
                    sb.Draw(_riftTex, pos, null, edgeDrawColor, r.Rotation,
                        riftOrigin, scale * 1.15f, SpriteEffects.None, 0);
                }
            }

            if (inflows.Count > 0 && _inflowTex != null)
            {
                Vector2 inflowOrigin = _inflowTex.Size() * 0.5f;
                var sortedInflows = inflows.OrderBy(i => i.Life);
                foreach (var inf in sortedInflows)
                {
                    Color drawColor = inf.Color * inf.Alpha;
                    Vector2 pos = inf.Position - Main.screenPosition;
                    Vector2 toTarget = inf.Target - inf.Position;
                    float rotation = toTarget.Length() > 1f ? toTarget.ToRotation() : 0f;
                    float stretch = MathF.Max(1f, toTarget.Length() * 0.05f);
                    Vector2 scale = new Vector2(inf.CurrentScale * stretch, inf.CurrentScale);
                    sb.Draw(_inflowTex, pos, null, drawColor, rotation,
                        inflowOrigin, scale, SpriteEffects.None, 0);
                }
            }

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

        public void Clear()
        {
            orbs.Clear();
            shards.Clear();
            rifts.Clear();
            inflows.Clear();
            orbCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
