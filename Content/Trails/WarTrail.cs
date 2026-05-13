using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class WarTrail : ITrail
    {
        public class GunSmokeParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float ExpandRate;
            public float FadeDelay;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 3f);
                    float fadeOut = 1f - MathF.Max(0f, (Progress - FadeDelay) / (1f - FadeDelay));
                    return MathF.Max(0f, fadeIn * fadeOut * 0.5f);
                }
            }

            public float CurrentScale => Scale * (1f + Progress * ExpandRate);

            public GunSmokeParticle(Vector2 pos, Vector2 vel, int life, float scale, float expandRate, float fadeDelay, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                ExpandRate = expandRate;
                FadeDelay = fadeDelay;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public class ShrapnelParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpinSpeed;
            public float TumblePhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float appear = MathF.Min(1f, Progress * 5f);
                    float vanish = 1f - MathF.Pow(Progress, 2f);
                    return MathF.Max(0f, appear * vanish);
                }
            }

            public ShrapnelParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotation, float spinSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = rotation;
                SpinSpeed = spinSpeed;
                TumblePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public class WarFlameParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float FlickerPhase;
            public float FlickerSpeed;
            public float RiseSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float burst = MathF.Min(1f, Progress * 8f);
                    float fade = 1f - MathF.Pow(Progress, 1.5f);
                    float flicker = 0.7f + 0.3f * MathF.Sin(FlickerPhase);
                    return MathF.Max(0f, burst * fade * flicker);
                }
            }

            public float CurrentScale => Scale * (1f + Progress * 1.5f) * (1f - Progress * 0.3f);

            public WarFlameParticle(Vector2 pos, Vector2 vel, int life, float scale, float riseSpeed, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                RiseSpeed = riseSpeed;
                FlickerPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                FlickerSpeed = Main.rand.NextFloat(0.1f, 0.2f);
                Color = color;
            }
        }

        public string Name { get; set; } = "WarTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.25f;
        public Color GhostColor { get; set; } = new Color(180, 120, 80, 120);

        public int MaxGunSmokes { get; set; } = 20;
        public int GunSmokeLife { get; set; } = 40;
        public float GunSmokeSize { get; set; } = 0.8f;
        public float GunSmokeExpandRate { get; set; } = 2f;
        public float GunSmokeFadeDelay { get; set; } = 0.4f;
        public int GunSmokeSpawnInterval { get; set; } = 3;
        public float GunSmokeSpeed { get; set; } = 1.5f;
        public float GunSmokeSpread { get; set; } = 4f;
        public Color GunSmokeColor { get; set; } = new Color(160, 140, 120, 200);

        public int MaxShrapnels { get; set; } = 20;
        public int ShrapnelLife { get; set; } = 25;
        public float ShrapnelSize { get; set; } = 0.35f;
        public float ShrapnelSpawnChance { get; set; } = 0.1f;
        public float ShrapnelSpeed { get; set; } = 4f;
        public float ShrapnelSpinSpeed { get; set; } = 0.3f;
        public Color ShrapnelColor { get; set; } = new Color(200, 180, 140, 230);

        public int MaxWarFlames { get; set; } = 15;
        public int WarFlameLife { get; set; } = 18;
        public float WarFlameSize { get; set; } = 0.5f;
        public float WarFlameSpawnChance { get; set; } = 0.08f;
        public float WarFlameRiseSpeed { get; set; } = 0.5f;
        public float WarFlameSpread { get; set; } = 3f;
        public Color WarFlameColor { get; set; } = new Color(255, 160, 60, 255);

        public float InertiaFactor { get; set; } = 0.2f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<GunSmokeParticle> gunSmokes = new();
        private List<ShrapnelParticle> shrapnels = new();
        private List<WarFlameParticle> warFlames = new();
        private int smokeCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _smokeTex;
        private Texture2D _shrapnelTex;
        private Texture2D _flameTex;
        private Texture2D _ghostTex;

        public bool HasContent => gunSmokes.Count > 0 || shrapnels.Count > 0 || warFlames.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_smokeTex != null) return;
            _smokeTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WarTrail/WarTrailSmoke").Value;
            _shrapnelTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WarTrail/WarTrailShrapnel").Value;
            _flameTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WarTrail/WarTrailFlame").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WarTrail/WarTrailGhost").Value;
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

            smokeCounter++;
            if (smokeCounter >= GunSmokeSpawnInterval && gunSmokes.Count < MaxGunSmokes)
            {
                smokeCounter = 0;
                SpawnGunSmoke(center, velocity, moveDir);
            }

            if (shrapnels.Count < MaxShrapnels && Main.rand.NextFloat() < ShrapnelSpawnChance)
                SpawnShrapnel(center, velocity, moveDir);

            if (warFlames.Count < MaxWarFlames && Main.rand.NextFloat() < WarFlameSpawnChance)
                SpawnWarFlame(center, velocity, moveDir);

            for (int i = gunSmokes.Count - 1; i >= 0; i--)
            {
                var s = gunSmokes[i];
                s.Velocity *= 0.95f;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0) gunSmokes.RemoveAt(i);
            }

            for (int i = shrapnels.Count - 1; i >= 0; i--)
            {
                var s = shrapnels[i];
                s.Rotation += s.SpinSpeed;
                s.TumblePhase += 0.1f;
                s.Velocity *= 0.93f;
                s.Position += s.Velocity;
                s.Life--;
                if (s.Life <= 0) shrapnels.RemoveAt(i);
            }

            for (int i = warFlames.Count - 1; i >= 0; i--)
            {
                var f = warFlames[i];
                f.FlickerPhase += f.FlickerSpeed;
                f.Velocity.Y -= f.RiseSpeed * 0.02f;
                f.Velocity *= 0.94f;
                f.Position += f.Velocity;
                f.Life--;
                if (f.Life <= 0) warFlames.RemoveAt(i);
            }
        }

        private void SpawnGunSmoke(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-GunSmokeSpread, GunSmokeSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.6f;
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 burst = angle.ToRotationVector2() * Main.rand.NextFloat(0.3f, GunSmokeSpeed);
            Vector2 vel = inertia + burst;

            float scale = Main.rand.NextFloat(0.5f, 1.3f) * GunSmokeSize;
            float expandRate = GunSmokeExpandRate * Main.rand.NextFloat(0.7f, 1.3f);
            float fadeDelay = GunSmokeFadeDelay * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = GunSmokeColor * Main.rand.NextFloat(0.6f, 1f);

            gunSmokes.Add(new GunSmokeParticle(pos, vel, GunSmokeLife, scale, expandRate, fadeDelay, color));
        }

        private void SpawnShrapnel(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-3f, 3f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(1f, 1f);

            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 burst = angle.ToRotationVector2() * Main.rand.NextFloat(1f, ShrapnelSpeed);
            Vector2 vel = inertia + burst;

            float scale = Main.rand.NextFloat(0.5f, 1.5f) * ShrapnelSize;
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            float spinSpeed = Main.rand.NextFloat(-ShrapnelSpinSpeed, ShrapnelSpinSpeed);
            Color color = ShrapnelColor * Main.rand.NextFloat(0.6f, 1f);

            shrapnels.Add(new ShrapnelParticle(pos, vel, ShrapnelLife, scale, rotation, spinSpeed, color));
        }

        private void SpawnWarFlame(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-WarFlameSpread, WarFlameSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(1f, 1f);

            Vector2 inertia = -velocity * InertiaFactor * 0.2f;
            Vector2 rise = new Vector2(0f, -Main.rand.NextFloat(0.5f, WarFlameRiseSpeed));
            Vector2 drift = Main.rand.NextVector2Circular(0.5f, 0.5f);
            Vector2 vel = inertia + rise + drift;

            float scale = Main.rand.NextFloat(0.6f, 1.4f) * WarFlameSize;
            float riseSpeed = WarFlameRiseSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = WarFlameColor * Main.rand.NextFloat(0.7f, 1f);

            warFlames.Add(new WarFlameParticle(pos, vel, WarFlameLife, scale, riseSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (gunSmokes.Count > 0 && _smokeTex != null)
            {
                Vector2 smokeOrigin = new Vector2(_smokeTex.Width * 0.5f, _smokeTex.Height * 0.5f);
                var sorted = gunSmokes.OrderBy(s => s.Life);
                foreach (var s in sorted)
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    float scale = s.CurrentScale;
                    sb.Draw(_smokeTex, pos, null, drawColor, s.Rotation,
                        smokeOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (shrapnels.Count > 0 && _shrapnelTex != null)
            {
                Vector2 shrapnelOrigin = new Vector2(_shrapnelTex.Width * 0.5f, _shrapnelTex.Height * 0.5f);
                var sorted = shrapnels.OrderBy(s => s.Life);
                foreach (var s in sorted)
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    sb.Draw(_shrapnelTex, pos, null, drawColor, s.Rotation,
                        shrapnelOrigin, s.Scale, SpriteEffects.None, 0);
                }
            }

            if (warFlames.Count > 0 && _flameTex != null)
            {
                Vector2 flameOrigin = new Vector2(_flameTex.Width * 0.5f, _flameTex.Height * 0.5f);
                var sorted = warFlames.OrderBy(f => f.Life);
                foreach (var f in sorted)
                {
                    Color drawColor = f.Color * f.Alpha;
                    Vector2 pos = f.Position - Main.screenPosition;
                    float scale = f.CurrentScale;
                    sb.Draw(_flameTex, pos, null, drawColor, 0f,
                        flameOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            gunSmokes.Clear();
            shrapnels.Clear();
            warFlames.Clear();
            smokeCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}