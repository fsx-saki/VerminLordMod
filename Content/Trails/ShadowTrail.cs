using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class ShadowTrail : ITrail
    {
        public class ShadowTendrilParticle
        {
            public Vector2 Root;
            public Vector2 Tip;
            public Vector2 TipVelocity;
            public float Width;
            public int Life;
            public int MaxLife;
            public float ReachSpeed;
            public float MaxReach;
            public float RetractStart;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float retract = Progress > RetractStart ? 1f - (Progress - RetractStart) / (1f - RetractStart) : 1f;
                    return MathF.Max(0f, fadeIn * retract * 0.6f);
                }
            }

            public float CurrentWidth => Width * Alpha;

            public ShadowTendrilParticle(Vector2 root, Vector2 tipDir, int life, float width, float reachSpeed, float maxReach, float retractStart, Color color)
            {
                Root = root;
                Tip = root;
                TipVelocity = tipDir.SafeNormalize(Vector2.Zero) * reachSpeed;
                MaxLife = life;
                Life = life;
                Width = width;
                ReachSpeed = reachSpeed;
                MaxReach = maxReach;
                RetractStart = retractStart;
                Color = color;
            }
        }

        public class ShadowPoolParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpreadSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float spread = MathF.Min(1f, Progress * SpreadSpeed);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, spread * fadeOut * 0.35f);
                }
            }

            public float CurrentScale
            {
                get
                {
                    float spread = MathF.Min(1f, Progress * SpreadSpeed);
                    return Scale + (MaxScale - Scale) * spread;
                }
            }

            public ShadowPoolParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float spreadSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                SpreadSpeed = spreadSpeed;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public class ShadowCloneParticle
        {
            public Vector2 Position;
            public Vector2 Target;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float FollowStrength;
            public float Phase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float flicker = 0.3f + 0.3f * MathF.Sin(Phase);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, flicker * fadeOut);
                }
            }

            public float CurrentScale => Scale * (0.8f + 0.2f * MathF.Sin(Phase * 0.7f));

            public ShadowCloneParticle(Vector2 pos, Vector2 target, int life, float scale, float followStrength, Color color)
            {
                Position = pos;
                Target = target;
                MaxLife = life;
                Life = life;
                Scale = scale;
                FollowStrength = followStrength;
                Phase = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public string Name { get; set; } = "ShadowTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 12;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(30, 25, 50, 160);

        public int MaxTendrils { get; set; } = 10;
        public int TendrilLife { get; set; } = 40;
        public float TendrilWidth { get; set; } = 0.6f;
        public float TendrilSpawnChance { get; set; } = 0.04f;
        public float TendrilReachSpeed { get; set; } = 2.5f;
        public float TendrilMaxReach { get; set; } = 30f;
        public float TendrilRetractStart { get; set; } = 0.5f;
        public float TendrilDriftSpeed { get; set; } = 0.05f;
        public Color TendrilColor { get; set; } = new Color(40, 30, 70, 200);

        public int MaxPools { get; set; } = 8;
        public int PoolLife { get; set; } = 60;
        public float PoolStartSize { get; set; } = 0.2f;
        public float PoolEndSize { get; set; } = 2.0f;
        public float PoolSpawnChance { get; set; } = 0.02f;
        public float PoolSpreadSpeed { get; set; } = 2f;
        public float PoolDriftSpeed { get; set; } = 0.04f;
        public Color PoolColor { get; set; } = new Color(25, 20, 45, 180);

        public int MaxClones { get; set; } = 6;
        public int CloneLife { get; set; } = 35;
        public float CloneSize { get; set; } = 0.7f;
        public float CloneSpawnChance { get; set; } = 0.015f;
        public float CloneFollowStrength { get; set; } = 0.08f;
        public Color CloneColor { get; set; } = new Color(50, 40, 80, 180);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<ShadowTendrilParticle> tendrils = new();
        private List<ShadowPoolParticle> pools = new();
        private List<ShadowCloneParticle> clones = new();

        private GhostTrail _ghostTrail;

        private Texture2D _tendrilTex;
        private Texture2D _poolTex;
        private Texture2D _cloneTex;
        private Texture2D _ghostTex;

        private Vector2 _lastCenter;

        public bool HasContent => tendrils.Count > 0 || pools.Count > 0 || clones.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_tendrilTex != null) return;
            _tendrilTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/ShadowTrail/ShadowTrailTendril").Value;
            _poolTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/ShadowTrail/ShadowTrailPool").Value;
            _cloneTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/ShadowTrail/ShadowTrailClone").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/ShadowTrail/ShadowTrailGhost").Value;
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

            if (tendrils.Count < MaxTendrils && Main.rand.NextFloat() < TendrilSpawnChance)
                SpawnTendril(center, velocity, moveDir);

            if (pools.Count < MaxPools && Main.rand.NextFloat() < PoolSpawnChance)
                SpawnPool(center, velocity, moveDir);

            if (clones.Count < MaxClones && Main.rand.NextFloat() < CloneSpawnChance)
                SpawnClone(center, velocity);

            for (int i = tendrils.Count - 1; i >= 0; i--)
            {
                var t = tendrils[i];
                t.Root += Main.rand.NextVector2Circular(TendrilDriftSpeed, TendrilDriftSpeed);
                if (t.Progress < t.RetractStart)
                {
                    float dist = Vector2.Distance(t.Root, t.Tip);
                    if (dist < t.MaxReach)
                    {
                        t.Tip += t.TipVelocity;
                        t.TipVelocity = t.TipVelocity.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * 0.98f;
                    }
                }
                else
                {
                    t.Tip = Vector2.Lerp(t.Tip, t.Root, (t.Progress - t.RetractStart) / (1f - t.RetractStart) * 0.1f);
                }
                t.Life--;
                if (t.Life <= 0) tendrils.RemoveAt(i);
            }

            for (int i = pools.Count - 1; i >= 0; i--)
            {
                var p = pools[i];
                p.Velocity *= 0.98f;
                p.Position += p.Velocity;
                p.Life--;
                if (p.Life <= 0) pools.RemoveAt(i);
            }

            for (int i = clones.Count - 1; i >= 0; i--)
            {
                var c = clones[i];
                c.Phase += 0.12f;
                c.Target = _lastCenter;
                Vector2 toTarget = c.Target - c.Position;
                c.Position += toTarget * c.FollowStrength;
                c.Position += Main.rand.NextVector2Circular(0.3f, 0.3f);
                c.Life--;
                if (c.Life <= 0) clones.RemoveAt(i);
            }
        }

        private void SpawnTendril(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 root = center + SpawnOffset + Main.rand.NextVector2Circular(4f, 4f);
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 tipDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

            float width = TendrilWidth * Main.rand.NextFloat(0.6f, 1.4f);
            float reachSpeed = TendrilReachSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float maxReach = TendrilMaxReach * Main.rand.NextFloat(0.6f, 1.4f);
            float retractStart = TendrilRetractStart + Main.rand.NextFloat(-0.1f, 0.1f);
            Color color = TendrilColor * Main.rand.NextFloat(0.5f, 1f);

            tendrils.Add(new ShadowTendrilParticle(root, tipDir, TendrilLife, width, reachSpeed, maxReach, retractStart, color));
        }

        private void SpawnPool(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-6f, 6f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(5f, 5f);

            Vector2 drift = Main.rand.NextVector2Circular(PoolDriftSpeed, PoolDriftSpeed);
            float startSize = PoolStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = PoolEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            float spreadSpeed = PoolSpreadSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = PoolColor * Main.rand.NextFloat(0.5f, 1f);

            pools.Add(new ShadowPoolParticle(pos, drift, PoolLife, startSize, endSize, spreadSpeed, color));
        }

        private void SpawnClone(Vector2 center, Vector2 velocity)
        {
            Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
            Vector2 pos = center + SpawnOffset + offset;

            float scale = CloneSize * Main.rand.NextFloat(0.6f, 1.2f);
            float followStrength = CloneFollowStrength * Main.rand.NextFloat(0.6f, 1.4f);
            Color color = CloneColor * Main.rand.NextFloat(0.5f, 1f);

            clones.Add(new ShadowCloneParticle(pos, center, CloneLife, scale, followStrength, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (pools.Count > 0 && _poolTex != null)
            {
                Vector2 poolOrigin = _poolTex.Size() * 0.5f;
                var sortedPools = pools.OrderBy(p => p.Life);
                foreach (var p in sortedPools)
                {
                    Color drawColor = p.Color * p.Alpha;
                    Vector2 pos = p.Position - Main.screenPosition;
                    sb.Draw(_poolTex, pos, null, drawColor, p.Rotation,
                        poolOrigin, p.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (tendrils.Count > 0 && _tendrilTex != null)
            {
                Vector2 tendrilOrigin = new Vector2(0f, _tendrilTex.Height * 0.5f);
                var sortedTendrils = tendrils.OrderBy(t => t.Life);
                foreach (var t in sortedTendrils)
                {
                    Vector2 delta = t.Tip - t.Root;
                    float length = delta.Length();
                    if (length < 1f) continue;
                    float rotation = delta.ToRotation();
                    Vector2 scale = new Vector2(length / _tendrilTex.Width, t.CurrentWidth);

                    Color drawColor = t.Color * t.Alpha;
                    Vector2 pos = t.Root - Main.screenPosition;
                    sb.Draw(_tendrilTex, pos, null, drawColor, rotation,
                        tendrilOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (clones.Count > 0 && _cloneTex != null)
            {
                Vector2 cloneOrigin = _cloneTex.Size() * 0.5f;
                var sortedClones = clones.OrderBy(c => c.Life);
                foreach (var c in sortedClones)
                {
                    Color drawColor = c.Color * c.Alpha;
                    Vector2 pos = c.Position - Main.screenPosition;
                    sb.Draw(_cloneTex, pos, null, drawColor, 0f,
                        cloneOrigin, c.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            tendrils.Clear();
            pools.Clear();
            clones.Clear();
            _ghostTrail?.Clear();
        }
    }
}
