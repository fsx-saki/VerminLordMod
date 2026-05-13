using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class FireTrail : ITrail
    {
        public class FireTongueParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Length;
            public int Life;
            public int MaxLife;
            public float SwayPhase;
            public float SwaySpeed;
            public float SwayAmplitude;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = 1f - Progress * Progress;
                    return MathF.Max(0f, fadeIn * fadeOut);
                }
            }

            public float CurrentLength => Length * (1f - Progress * 0.4f);

            public float CurrentWidth => Scale * (1f - Progress * 0.5f);

            public Color CurrentColor
            {
                get
                {
                    float t = Progress;
                    Color start = new Color(255, 220, 80);
                    Color mid = new Color(255, 120, 20);
                    Color end = new Color(180, 30, 10);
                    Color c;
                    if (t < 0.4f)
                    {
                        float lt = t / 0.4f;
                        c = Color.Lerp(start, mid, lt);
                    }
                    else
                    {
                        float lt = (t - 0.4f) / 0.6f;
                        c = Color.Lerp(mid, end, lt);
                    }
                    return c * Alpha;
                }
            }

            public FireTongueParticle(Vector2 pos, Vector2 vel, int life, float scale, float length, float swaySpeed, float swayAmp, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Length = length;
                SwayPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                SwaySpeed = swaySpeed;
                SwayAmplitude = swayAmp;
                Color = color;
            }
        }

        public class FireEmberParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Brightness;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float flash = Progress < 0.1f ? Progress / 0.1f : 1f;
                    float fadeOut = 1f - Progress;
                    return MathF.Max(0f, flash * fadeOut * Brightness);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.3f);

            public FireEmberParticle(Vector2 pos, Vector2 vel, int life, float scale, float brightness, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Brightness = brightness;
                Color = color;
            }
        }

        public class FireAshParticle
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
                    float fadeIn = MathF.Min(1f, Progress * 3f);
                    float fadeOut = 1f - Progress;
                    return MathF.Max(0f, fadeIn * fadeOut * 0.5f);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.2f);

            public FireAshParticle(Vector2 pos, Vector2 vel, int life, float scale, float spinSpeed, Color color)
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

        public string Name { get; set; } = "FireTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.25f;
        public float GhostLengthScale { get; set; } = 1.6f;
        public float GhostAlpha { get; set; } = 0.5f;
        public Color GhostColor { get; set; } = new Color(255, 150, 50, 180);

        public int MaxTongues { get; set; } = 35;
        public int TongueLife { get; set; } = 20;
        public float TongueScale { get; set; } = 0.5f;
        public float TongueLength { get; set; } = 16f;
        public int TongueSpawnInterval { get; set; } = 1;
        public float TongueSwaySpeed { get; set; } = 0.15f;
        public float TongueSwayAmp { get; set; } = 0.4f;
        public float TongueRiseSpeed { get; set; } = 0.8f;
        public float TongueSpread { get; set; } = 5f;
        public Color TongueColor { get; set; } = new Color(255, 200, 80, 240);

        public int MaxEmbers { get; set; } = 25;
        public int EmberLife { get; set; } = 30;
        public float EmberSize { get; set; } = 0.35f;
        public float EmberSpawnChance { get; set; } = 0.2f;
        public float EmberRiseSpeed { get; set; } = 1.2f;
        public float EmberDriftSpeed { get; set; } = 0.4f;
        public Color EmberColor { get; set; } = new Color(255, 180, 50, 220);

        public int MaxAshes { get; set; } = 15;
        public int AshLife { get; set; } = 40;
        public float AshSize { get; set; } = 0.3f;
        public float AshSpawnChance { get; set; } = 0.08f;
        public float AshFallSpeed { get; set; } = 0.5f;
        public float AshDriftSpeed { get; set; } = 0.2f;
        public float AshSpinSpeed { get; set; } = 0.05f;
        public Color AshColor { get; set; } = new Color(120, 100, 80, 160);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<FireTongueParticle> tongues = new();
        private List<FireEmberParticle> embers = new();
        private List<FireAshParticle> ashes = new();
        private int tongueCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _tongueTex;
        private Texture2D _emberTex;
        private Texture2D _ashTex;
        private Texture2D _ghostTex;

        public bool HasContent => tongues.Count > 0 || embers.Count > 0 || ashes.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_tongueTex != null) return;
            _tongueTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/FireTrail/FireTrailTongue").Value;
            _emberTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/FireTrail/FireTrailEmber").Value;
            _ashTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/FireTrail/FireTrailAsh").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/FireTrail/FireTrailGhost").Value;
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

            tongueCounter++;
            if (tongueCounter >= TongueSpawnInterval && tongues.Count < MaxTongues)
            {
                tongueCounter = 0;
                SpawnTongue(center, velocity, moveDir);
            }

            if (embers.Count < MaxEmbers && Main.rand.NextFloat() < EmberSpawnChance)
                SpawnEmber(center, velocity, moveDir);

            if (ashes.Count < MaxAshes && Main.rand.NextFloat() < AshSpawnChance)
                SpawnAsh(center, velocity, moveDir);

            for (int i = tongues.Count - 1; i >= 0; i--)
            {
                var t = tongues[i];
                t.SwayPhase += t.SwaySpeed;
                float sway = MathF.Sin(t.SwayPhase) * t.SwayAmplitude;
                t.Velocity.X = t.Velocity.X * 0.9f + sway * 0.1f;
                t.Velocity.Y -= TongueRiseSpeed * 0.02f;
                t.Position += t.Velocity;
                t.Life--;
                if (t.Life <= 0) tongues.RemoveAt(i);
            }

            for (int i = embers.Count - 1; i >= 0; i--)
            {
                var e = embers[i];
                e.Velocity.Y -= EmberRiseSpeed * 0.01f;
                e.Velocity *= 0.98f;
                e.Position += e.Velocity;
                e.Life--;
                if (e.Life <= 0) embers.RemoveAt(i);
            }

            for (int i = ashes.Count - 1; i >= 0; i--)
            {
                var a = ashes[i];
                a.Rotation += a.SpinSpeed;
                a.Velocity.Y += AshFallSpeed * 0.01f;
                a.Velocity.X *= 0.99f;
                a.Position += a.Velocity;
                a.Life--;
                if (a.Life <= 0) ashes.RemoveAt(i);
            }
        }

        private void SpawnTongue(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-TongueSpread, TongueSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.2f;
            Vector2 rise = new Vector2(0f, -TongueRiseSpeed * Main.rand.NextFloat(0.5f, 1f));
            Vector2 drift = Main.rand.NextVector2Circular(0.2f, 0.2f);
            Vector2 vel = inertia + rise + drift;

            float scale = Main.rand.NextFloat(0.6f, 1.3f) * TongueScale;
            float length = Main.rand.NextFloat(0.7f, 1.4f) * TongueLength;
            float swaySpeed = TongueSwaySpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float swayAmp = TongueSwayAmp * Main.rand.NextFloat(0.6f, 1.4f);
            Color color = TongueColor * Main.rand.NextFloat(0.6f, 1f);

            tongues.Add(new FireTongueParticle(pos, vel, TongueLife, scale, length, swaySpeed, swayAmp, color));
        }

        private void SpawnEmber(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(6f, 6f);

            Vector2 rise = new Vector2(0f, -EmberRiseSpeed * Main.rand.NextFloat(0.5f, 1.5f));
            Vector2 drift = Main.rand.NextVector2Circular(EmberDriftSpeed, EmberDriftSpeed);
            Vector2 vel = rise + drift;

            float scale = Main.rand.NextFloat(0.4f, 1.2f) * EmberSize;
            float brightness = Main.rand.NextFloat(0.6f, 1f);
            Color color = EmberColor * Main.rand.NextFloat(0.6f, 1f);

            embers.Add(new FireEmberParticle(pos, vel, EmberLife, scale, brightness, color));
        }

        private void SpawnAsh(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(8f, 4f) + new Vector2(0f, -5f);

            Vector2 fall = new Vector2(0f, AshFallSpeed * Main.rand.NextFloat(0.3f, 0.8f));
            Vector2 drift = Main.rand.NextVector2Circular(AshDriftSpeed, AshDriftSpeed);
            Vector2 vel = fall + drift;

            float scale = Main.rand.NextFloat(0.5f, 1.2f) * AshSize;
            float spinSpeed = Main.rand.NextFloat(-AshSpinSpeed, AshSpinSpeed);
            Color color = AshColor * Main.rand.NextFloat(0.4f, 0.8f);

            ashes.Add(new FireAshParticle(pos, vel, AshLife, scale, spinSpeed, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (ashes.Count > 0 && _ashTex != null)
            {
                Vector2 ashOrigin = _ashTex.Size() * 0.5f;
                var sortedAshes = ashes.OrderBy(a => a.Life);
                foreach (var a in sortedAshes)
                {
                    Color drawColor = a.Color * a.Alpha;
                    Vector2 pos = a.Position - Main.screenPosition;
                    sb.Draw(_ashTex, pos, null, drawColor, a.Rotation,
                        ashOrigin, a.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (tongues.Count > 0 && _tongueTex != null)
            {
                Vector2 tongueOrigin = new Vector2(_tongueTex.Width * 0.5f, _tongueTex.Height);
                var sortedTongues = tongues.OrderBy(t => t.Life);
                foreach (var t in sortedTongues)
                {
                    Color drawColor = t.CurrentColor;
                    Vector2 pos = t.Position - Main.screenPosition;
                    float angle = -MathHelper.PiOver2 + MathF.Sin(t.SwayPhase) * t.SwayAmplitude * 0.3f;
                    Vector2 scale = new Vector2(t.CurrentWidth, t.CurrentLength / _tongueTex.Height);
                    sb.Draw(_tongueTex, pos, null, drawColor, angle,
                        tongueOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (embers.Count > 0 && _emberTex != null)
            {
                Vector2 emberOrigin = _emberTex.Size() * 0.5f;
                var sortedEmbers = embers.OrderBy(e => e.Life);
                foreach (var e in sortedEmbers)
                {
                    Color drawColor = e.Color * e.Alpha;
                    Vector2 pos = e.Position - Main.screenPosition;
                    sb.Draw(_emberTex, pos, null, drawColor, 0f,
                        emberOrigin, e.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            tongues.Clear();
            embers.Clear();
            ashes.Clear();
            tongueCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
