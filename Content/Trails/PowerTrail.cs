using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class PowerTrail : ITrail
    {
        public class ShockWaveParticle
        {
            public Vector2 Position;
            public float Radius;
            public float MaxRadius;
            public float Width;
            public int Life;
            public int MaxLife;
            public float ExpandSpeed;
            public float PulsePhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 3f) * (1f - Progress) * 0.6f);
            public float CurrentRadius => Radius + (MaxRadius - Radius) * MathF.Min(1f, Progress * ExpandSpeed);
            public float CurrentWidth => Width * (1f - Progress * 0.3f);

            public ShockWaveParticle(Vector2 pos, int life, float radius, float maxRadius, float width, float expandSpeed, Color color)
            {
                Position = pos; MaxLife = life; Life = life; Radius = radius; MaxRadius = maxRadius;
                Width = width; ExpandSpeed = expandSpeed; PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi); Color = color;
            }
        }

        public class PowerAuraParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float PulsePhase;
            public float BurstPhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 5f) * (1f - Progress) * (0.6f + 0.4f * MathF.Abs(MathF.Sin(PulsePhase))));
            public float CurrentScale => Scale * (1f + BurstPhase * Progress);

            public PowerAuraParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotSpeed, float burstPhase, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life; Scale = scale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi); RotSpeed = rotSpeed;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi); BurstPhase = burstPhase; Color = color;
            }
        }

        public class BurstLineParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Length;
            public float Width;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float FadeSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, (1f - Progress * FadeSpeed) * 0.7f);
            public float CurrentLength => Length * (1f - Progress * 0.5f);
            public float CurrentWidth => Width * (1f - Progress * 0.6f);

            public BurstLineParticle(Vector2 pos, Vector2 vel, int life, float length, float width, float rotation, float fadeSpeed, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Length = length; Width = width; Rotation = rotation; FadeSpeed = fadeSpeed; Color = color;
            }
        }

        public string Name { get; set; } = "PowerTrail";
        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(255, 180, 80, 140);

        public int MaxShockWaves { get; set; } = 4;
        public int ShockWaveLife { get; set; } = 40;
        public float ShockWaveStartRadius { get; set; } = 3f;
        public float ShockWaveEndRadius { get; set; } = 35f;
        public float ShockWaveWidth { get; set; } = 0.5f;
        public float ShockWaveSpawnChance { get; set; } = 0.015f;
        public float ShockWaveExpandSpeed { get; set; } = 2.5f;
        public Color ShockWaveColor { get; set; } = new Color(255, 200, 80, 200);

        public int MaxAuras { get; set; } = 8;
        public int AuraLife { get; set; } = 25;
        public float AuraSize { get; set; } = 0.6f;
        public float AuraSpawnChance { get; set; } = 0.08f;
        public float AuraRotSpeed { get; set; } = 0.1f;
        public float AuraBurstPhase { get; set; } = 0.5f;
        public float AuraDriftSpeed { get; set; } = 0.3f;
        public Color AuraColor { get; set; } = new Color(255, 180, 60, 220);

        public int MaxBurstLines { get; set; } = 12;
        public int BurstLineLife { get; set; } = 20;
        public float BurstLineLength { get; set; } = 20f;
        public float BurstLineWidth { get; set; } = 0.2f;
        public float BurstLineSpawnChance { get; set; } = 0.1f;
        public float BurstLineFadeSpeed { get; set; } = 2f;
        public float BurstLineDriftSpeed { get; set; } = 0.8f;
        public Color BurstLineColor { get; set; } = new Color(255, 160, 50, 200);

        public float InertiaFactor { get; set; } = 0.2f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<ShockWaveParticle> shockWaves = new();
        private List<PowerAuraParticle> auras = new();
        private List<BurstLineParticle> burstLines = new();
        private GhostTrail _ghostTrail;
        private Texture2D _shockTex, _auraTex, _burstTex, _ghostTex;

        public bool HasContent => shockWaves.Count > 0 || auras.Count > 0 || burstLines.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_shockTex != null) return;
            _shockTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/PowerTrail/PowerTrailShock").Value;
            _auraTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/PowerTrail/PowerTrailAura").Value;
            _burstTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/PowerTrail/PowerTrailBurst").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/PowerTrail/PowerTrailGhost").Value;
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

            if (shockWaves.Count < MaxShockWaves && Main.rand.NextFloat() < ShockWaveSpawnChance)
                SpawnShockWave(center, velocity, moveDir);
            if (auras.Count < MaxAuras && Main.rand.NextFloat() < AuraSpawnChance)
                SpawnAura(center, velocity, moveDir);
            if (burstLines.Count < MaxBurstLines && Main.rand.NextFloat() < BurstLineSpawnChance)
                SpawnBurstLine(center, velocity, moveDir);

            for (int i = shockWaves.Count - 1; i >= 0; i--)
            {
                var s = shockWaves[i];
                s.PulsePhase += 0.08f;
                s.Position += (center - s.Position) * 0.01f; s.Life--;
                if (s.Life <= 0) shockWaves.RemoveAt(i);
            }
            for (int i = auras.Count - 1; i >= 0; i--)
            {
                var a = auras[i];
                a.Rotation += a.RotSpeed; a.PulsePhase += 0.12f;
                a.Velocity *= 0.94f; a.Position += a.Velocity; a.Life--;
                if (a.Life <= 0) auras.RemoveAt(i);
            }
            for (int i = burstLines.Count - 1; i >= 0; i--)
            {
                var b = burstLines[i];
                b.Velocity *= 0.92f; b.Position += b.Velocity; b.Life--;
                if (b.Life <= 0) burstLines.RemoveAt(i);
            }
        }

        private void SpawnShockWave(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(2f, 2f);
            float endRadius = ShockWaveEndRadius * Main.rand.NextFloat(0.7f, 1.3f);
            float width = ShockWaveWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float expandSpeed = ShockWaveExpandSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = ShockWaveColor * Main.rand.NextFloat(0.5f, 1f);
            shockWaves.Add(new ShockWaveParticle(pos, ShockWaveLife, ShockWaveStartRadius, endRadius, width, expandSpeed, color));
        }

        private void SpawnAura(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(3f, 3f);
            Vector2 vel = -velocity * InertiaFactor * 0.4f + Main.rand.NextVector2Circular(AuraDriftSpeed, AuraDriftSpeed);
            float scale = Main.rand.NextFloat(0.6f, 1.3f) * AuraSize;
            float rotSpeed = Main.rand.NextFloat(-AuraRotSpeed, AuraRotSpeed);
            float burstPhase = AuraBurstPhase * Main.rand.NextFloat(0.5f, 1.5f);
            Color color = AuraColor * Main.rand.NextFloat(0.6f, 1f);
            auras.Add(new PowerAuraParticle(pos, vel, AuraLife, scale, rotSpeed, burstPhase, color));
        }

        private void SpawnBurstLine(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(2f, 2f);
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(0.5f, BurstLineDriftSpeed);
            float length = BurstLineLength * Main.rand.NextFloat(0.5f, 1.2f);
            float width = BurstLineWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float fadeSpeed = BurstLineFadeSpeed * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = BurstLineColor * Main.rand.NextFloat(0.5f, 1f);
            burstLines.Add(new BurstLineParticle(pos, vel, BurstLineLife, length, width, angle, fadeSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null) _ghostTrail.Draw(sb);

            if (shockWaves.Count > 0 && _shockTex != null)
            {
                Vector2 shockOrigin = _shockTex.Size() * 0.5f;
                foreach (var s in shockWaves.OrderBy(x => x.Life))
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    float scaleX = s.CurrentRadius / (_shockTex.Width * 0.5f);
                    float scaleY = s.CurrentWidth;
                    sb.Draw(_shockTex, pos, null, drawColor, 0f, shockOrigin, new Vector2(scaleX, scaleY), SpriteEffects.None, 0);
                }
            }

            if (burstLines.Count > 0 && _burstTex != null)
            {
                Vector2 burstOrigin = new Vector2(0f, _burstTex.Height * 0.5f);
                foreach (var b in burstLines.OrderBy(x => x.Life))
                {
                    Color drawColor = b.Color * b.Alpha;
                    Vector2 pos = b.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(b.CurrentLength / _burstTex.Width, b.CurrentWidth);
                    sb.Draw(_burstTex, pos, null, drawColor, b.Rotation, burstOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (auras.Count > 0 && _auraTex != null)
            {
                Vector2 auraOrigin = _auraTex.Size() * 0.5f;
                foreach (var a in auras.OrderBy(x => x.Life))
                {
                    Color drawColor = a.Color * a.Alpha;
                    Vector2 pos = a.Position - Main.screenPosition;
                    sb.Draw(_auraTex, pos, null, drawColor, a.Rotation, auraOrigin, a.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            shockWaves.Clear(); auras.Clear(); burstLines.Clear();
            _ghostTrail?.Clear();
        }
    }
}