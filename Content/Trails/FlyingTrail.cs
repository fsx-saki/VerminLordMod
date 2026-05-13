using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class FlyingTrail : ITrail
    {
        public class FeatherMarkParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float SwayPhase;
            public float SwayAmount;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 3f) * (1f - Progress) * 0.7f);
            public float CurrentScale => Scale * (1f - Progress * 0.3f);

            public FeatherMarkParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotSpeed, float swayAmount, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life; Scale = scale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi); RotSpeed = rotSpeed;
                SwayPhase = Main.rand.NextFloat(MathHelper.TwoPi); SwayAmount = swayAmount; Color = color;
            }
        }

        public class WindTailParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Length;
            public float Width;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float TaperPhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 2f) * (1f - Progress) * 0.4f);
            public float CurrentLength => Length * (1f - Progress * 0.6f);
            public float CurrentWidth => Width * (1f - Progress * 0.8f);

            public WindTailParticle(Vector2 pos, Vector2 vel, int life, float length, float width, float rotation, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Length = length; Width = width; Rotation = rotation;
                TaperPhase = Main.rand.NextFloat(MathHelper.TwoPi); Color = color;
            }
        }

        public class SpeedAfterParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Stretch;
            public float Rotation;
            public float FadeSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, (1f - Progress * FadeSpeed) * 0.3f);
            public float CurrentScale => Scale * (1f - Progress * 0.5f);

            public SpeedAfterParticle(Vector2 pos, Vector2 vel, int life, float scale, float stretch, float rotation, float fadeSpeed, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Scale = scale; Stretch = stretch; Rotation = rotation; FadeSpeed = fadeSpeed; Color = color;
            }
        }

        public string Name { get; set; } = "FlyingTrail";
        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 1;
        public float GhostWidthScale { get; set; } = 0.15f;
        public float GhostLengthScale { get; set; } = 1.6f;
        public float GhostAlpha { get; set; } = 0.2f;
        public Color GhostColor { get; set; } = new Color(200, 220, 255, 100);

        public int MaxFeathers { get; set; } = 12;
        public int FeatherLife { get; set; } = 40;
        public float FeatherSize { get; set; } = 0.4f;
        public float FeatherSpawnChance { get; set; } = 0.05f;
        public float FeatherRotSpeed { get; set; } = 0.03f;
        public float FeatherSwayAmount { get; set; } = 2f;
        public float FeatherDriftSpeed { get; set; } = 0.15f;
        public Color FeatherColor { get; set; } = new Color(220, 235, 255, 210);

        public int MaxWindTails { get; set; } = 8;
        public int WindTailLife { get; set; } = 30;
        public float WindTailLength { get; set; } = 35f;
        public float WindTailWidth { get; set; } = 0.15f;
        public float WindTailSpawnChance { get; set; } = 0.06f;
        public float WindTailDriftSpeed { get; set; } = 0.05f;
        public Color WindTailColor { get; set; } = new Color(180, 210, 255, 180);

        public int MaxSpeedAfters { get; set; } = 6;
        public int SpeedAfterLife { get; set; } = 15;
        public float SpeedAfterSize { get; set; } = 0.4f;
        public float SpeedAfterStretch { get; set; } = 5f;
        public float SpeedAfterSpawnChance { get; set; } = 0.1f;
        public float SpeedAfterFadeSpeed { get; set; } = 3f;
        public Color SpeedAfterColor { get; set; } = new Color(160, 200, 255, 180);

        public float InertiaFactor { get; set; } = 0.1f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<FeatherMarkParticle> feathers = new();
        private List<WindTailParticle> windTails = new();
        private List<SpeedAfterParticle> speedAfters = new();
        private GhostTrail _ghostTrail;
        private Texture2D _featherTex, _windTex, _afterTex, _ghostTex;

        public bool HasContent => feathers.Count > 0 || windTails.Count > 0 || speedAfters.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_featherTex != null) return;
            _featherTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/FlyingTrail/FlyingTrailFeather").Value;
            _windTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/FlyingTrail/FlyingTrailWind").Value;
            _afterTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/FlyingTrail/FlyingTrailAfter").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/FlyingTrail/FlyingTrailGhost").Value;
        }

        private void EnsureGhostTrail()
        {
            if (!EnableGhostTrail || _ghostTrail != null) return;
            EnsureTextures();
            _ghostTrail = new GhostTrail
            {
                TrailTexture = _ghostTex, TrailColor = GhostColor, MaxPositions = GhostMaxPositions,
                RecordInterval = GhostRecordInterval, WidthScale = GhostWidthScale, LengthScale = GhostLengthScale,
                Alpha = GhostAlpha, UseAdditiveBlend = true, EnableGlow = false,
            };
        }

        public void Update(Vector2 center, Vector2 velocity)
        {
            EnsureTextures();
            EnsureGhostTrail();
            if (_ghostTrail != null) _ghostTrail.Update(center, velocity);

            Vector2 moveDir = velocity.SafeNormalize(Vector2.UnitX);

            if (feathers.Count < MaxFeathers && Main.rand.NextFloat() < FeatherSpawnChance)
                SpawnFeather(center, velocity, moveDir);
            if (windTails.Count < MaxWindTails && Main.rand.NextFloat() < WindTailSpawnChance)
                SpawnWindTail(center, velocity, moveDir);
            if (speedAfters.Count < MaxSpeedAfters && Main.rand.NextFloat() < SpeedAfterSpawnChance)
                SpawnSpeedAfter(center, velocity, moveDir);

            for (int i = feathers.Count - 1; i >= 0; i--)
            {
                var f = feathers[i];
                f.Rotation += f.RotSpeed; f.SwayPhase += 0.05f;
                Vector2 sway = new Vector2(MathF.Sin(f.SwayPhase) * f.SwayAmount * 0.02f, 0);
                f.Velocity *= 0.98f; f.Velocity.Y += 0.005f;
                f.Position += f.Velocity + sway; f.Life--;
                if (f.Life <= 0) feathers.RemoveAt(i);
            }
            for (int i = windTails.Count - 1; i >= 0; i--)
            {
                var w = windTails[i];
                w.Velocity *= 0.96f; w.Position += w.Velocity; w.Life--;
                if (w.Life <= 0) windTails.RemoveAt(i);
            }
            for (int i = speedAfters.Count - 1; i >= 0; i--)
            {
                var s = speedAfters[i];
                s.Velocity *= 0.9f; s.Position += s.Velocity; s.Life--;
                if (s.Life <= 0) speedAfters.RemoveAt(i);
            }
        }

        private void SpawnFeather(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(5f, 5f);
            Vector2 vel = -velocity * InertiaFactor * 0.15f + Main.rand.NextVector2Circular(FeatherDriftSpeed, FeatherDriftSpeed) + new Vector2(0f, 0.1f);
            float scale = Main.rand.NextFloat(0.6f, 1.3f) * FeatherSize;
            float rotSpeed = Main.rand.NextFloat(-FeatherRotSpeed, FeatherRotSpeed);
            float swayAmount = FeatherSwayAmount * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = FeatherColor * Main.rand.NextFloat(0.6f, 1f);
            feathers.Add(new FeatherMarkParticle(pos, vel, FeatherLife, scale, rotSpeed, swayAmount, color));
        }

        private void SpawnWindTail(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(3f, 3f);
            Vector2 vel = Main.rand.NextVector2Circular(WindTailDriftSpeed, WindTailDriftSpeed);
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.3f, 0.3f);
            float length = WindTailLength * Main.rand.NextFloat(0.7f, 1.3f);
            float width = WindTailWidth * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = WindTailColor * Main.rand.NextFloat(0.4f, 0.8f);
            windTails.Add(new WindTailParticle(pos, vel, WindTailLife, length, width, rotation, color));
        }

        private void SpawnSpeedAfter(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(2f, 2f);
            Vector2 vel = -velocity * 0.1f + Main.rand.NextVector2Circular(0.2f, 0.2f);
            float scale = Main.rand.NextFloat(0.5f, 1.2f) * SpeedAfterSize;
            float stretch = SpeedAfterStretch * Main.rand.NextFloat(0.8f, 1.2f);
            float rotation = velocity.ToRotation();
            float fadeSpeed = SpeedAfterFadeSpeed * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = SpeedAfterColor * Main.rand.NextFloat(0.4f, 0.8f);
            speedAfters.Add(new SpeedAfterParticle(pos, vel, SpeedAfterLife, scale, stretch, rotation, fadeSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null) _ghostTrail.Draw(sb);

            if (windTails.Count > 0 && _windTex != null)
            {
                Vector2 windOrigin = new Vector2(0f, _windTex.Height * 0.5f);
                foreach (var w in windTails.OrderBy(x => x.Life))
                {
                    Color drawColor = w.Color * w.Alpha;
                    Vector2 pos = w.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(w.CurrentLength / _windTex.Width, w.CurrentWidth);
                    sb.Draw(_windTex, pos, null, drawColor, w.Rotation, windOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (speedAfters.Count > 0 && _afterTex != null)
            {
                Vector2 afterOrigin = _afterTex.Size() * 0.5f;
                foreach (var s in speedAfters.OrderBy(x => x.Life))
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(s.CurrentScale * s.Stretch, s.CurrentScale);
                    sb.Draw(_afterTex, pos, null, drawColor, s.Rotation, afterOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (feathers.Count > 0 && _featherTex != null)
            {
                Vector2 featherOrigin = _featherTex.Size() * 0.5f;
                foreach (var f in feathers.OrderBy(x => x.Life))
                {
                    Color drawColor = f.Color * f.Alpha;
                    Vector2 pos = f.Position - Main.screenPosition;
                    sb.Draw(_featherTex, pos, null, drawColor, f.Rotation, featherOrigin, f.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            feathers.Clear(); windTails.Clear(); speedAfters.Clear();
            _ghostTrail?.Clear();
        }
    }
}