using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class WisdomTrail : ITrail
    {
        public class RuneSymbolParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpinSpeed;
            public float GlowPhase;
            public float GlowSpeed;
            public int RuneIndex;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float appear = MathF.Min(1f, Progress * 4f);
                    float vanish = 1f - MathF.Pow(Progress, 2.5f);
                    float glow = 0.6f + 0.4f * MathF.Sin(GlowPhase);
                    return MathF.Max(0f, appear * vanish * glow);
                }
            }

            public RuneSymbolParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotation, float spinSpeed, float glowSpeed, int runeIndex, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = rotation;
                SpinSpeed = spinSpeed;
                GlowPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                GlowSpeed = glowSpeed;
                RuneIndex = runeIndex;
                Color = color;
            }
        }

        public class WisdomGlowParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float ExpandRate;
            public float PulsePhase;
            public float PulseSpeed;
            public float HueShift;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float radiate = MathF.Min(1f, Progress * 3f);
                    float dim = 1f - MathF.Pow(Progress, 2f);
                    float pulse = 0.7f + 0.3f * MathF.Sin(PulsePhase);
                    return MathF.Max(0f, radiate * dim * pulse * 0.6f);
                }
            }

            public float CurrentScale => Scale * (1f + Progress * ExpandRate);

            public WisdomGlowParticle(Vector2 pos, Vector2 vel, int life, float scale, float expandRate, float pulseSpeed, float hueShift, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                ExpandRate = expandRate;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                PulseSpeed = pulseSpeed;
                HueShift = hueShift;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                Color = color;
            }
        }

        public class BookPageParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpinSpeed;
            public float FlutterPhase;
            public float FlutterSpeed;
            public float FlutterAmplitude;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float unfold = MathF.Min(1f, Progress * 3f);
                    float scatter = 1f - MathF.Pow(Progress, 3f);
                    float flutter = 0.8f + 0.2f * MathF.Sin(FlutterPhase);
                    return MathF.Max(0f, unfold * scatter * flutter * 0.5f);
                }
            }

            public BookPageParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotation, float spinSpeed, float flutterSpeed, float flutterAmplitude, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = rotation;
                SpinSpeed = spinSpeed;
                FlutterPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                FlutterSpeed = flutterSpeed;
                FlutterAmplitude = flutterAmplitude;
                Color = color;
            }
        }

        public string Name { get; set; } = "WisdomTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.16f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.25f;
        public Color GhostColor { get; set; } = new Color(200, 200, 160, 120);

        public int MaxRuneSymbols { get; set; } = 15;
        public int RuneSymbolLife { get; set; } = 30;
        public float RuneSymbolSize { get; set; } = 0.5f;
        public float RuneSymbolSpawnChance { get; set; } = 0.06f;
        public float RuneSymbolSpinSpeed { get; set; } = 0.04f;
        public float RuneSymbolGlowSpeed { get; set; } = 0.08f;
        public float RuneSymbolDriftSpeed { get; set; } = 0.3f;
        public float RuneSymbolSpread { get; set; } = 5f;
        public Color RuneSymbolColor { get; set; } = new Color(220, 210, 140, 230);

        public int MaxWisdomGlows { get; set; } = 12;
        public int WisdomGlowLife { get; set; } = 25;
        public float WisdomGlowSize { get; set; } = 0.5f;
        public float WisdomGlowExpandRate { get; set; } = 1.8f;
        public float WisdomGlowSpawnChance { get; set; } = 0.08f;
        public float WisdomGlowPulseSpeed { get; set; } = 0.07f;
        public float WisdomGlowHueShift { get; set; } = 0.02f;
        public float WisdomGlowSpread { get; set; } = 4f;
        public Color WisdomGlowColor { get; set; } = new Color(200, 200, 120, 200);

        public int MaxBookPages { get; set; } = 10;
        public int BookPageLife { get; set; } = 40;
        public float BookPageSize { get; set; } = 0.4f;
        public float BookPageSpawnChance { get; set; } = 0.04f;
        public float BookPageSpinSpeed { get; set; } = 0.06f;
        public float BookPageFlutterSpeed { get; set; } = 0.05f;
        public float BookPageFlutterAmplitude { get; set; } = 2f;
        public float BookPageDriftSpeed { get; set; } = 0.2f;
        public float BookPageSpread { get; set; } = 6f;
        public Color BookPageColor { get; set; } = new Color(180, 170, 120, 180);

        public float InertiaFactor { get; set; } = 0.1f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<RuneSymbolParticle> runeSymbols = new();
        private List<WisdomGlowParticle> wisdomGlows = new();
        private List<BookPageParticle> bookPages = new();

        private GhostTrail _ghostTrail;

        private Texture2D _runeTex;
        private Texture2D _glowTex;
        private Texture2D _pageTex;
        private Texture2D _ghostTex;

        public bool HasContent => runeSymbols.Count > 0 || wisdomGlows.Count > 0 || bookPages.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_runeTex != null) return;
            _runeTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WisdomTrail/WisdomTrailRune").Value;
            _glowTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WisdomTrail/WisdomTrailGlow").Value;
            _pageTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WisdomTrail/WisdomTrailPage").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/WisdomTrail/WisdomTrailGhost").Value;
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

            if (runeSymbols.Count < MaxRuneSymbols && Main.rand.NextFloat() < RuneSymbolSpawnChance)
                SpawnRuneSymbol(center, velocity, moveDir);

            if (wisdomGlows.Count < MaxWisdomGlows && Main.rand.NextFloat() < WisdomGlowSpawnChance)
                SpawnWisdomGlow(center, velocity, moveDir);

            if (bookPages.Count < MaxBookPages && Main.rand.NextFloat() < BookPageSpawnChance)
                SpawnBookPage(center, velocity, moveDir);

            for (int i = runeSymbols.Count - 1; i >= 0; i--)
            {
                var r = runeSymbols[i];
                r.Rotation += r.SpinSpeed;
                r.GlowPhase += r.GlowSpeed;
                r.Velocity *= 0.96f;
                r.Position += r.Velocity;
                r.Life--;
                if (r.Life <= 0) runeSymbols.RemoveAt(i);
            }

            for (int i = wisdomGlows.Count - 1; i >= 0; i--)
            {
                var g = wisdomGlows[i];
                g.PulsePhase += g.PulseSpeed;
                g.Velocity *= 0.95f;
                g.Position += g.Velocity;
                g.Life--;
                if (g.Life <= 0) wisdomGlows.RemoveAt(i);
            }

            for (int i = bookPages.Count - 1; i >= 0; i--)
            {
                var p = bookPages[i];
                p.Rotation += p.SpinSpeed;
                p.FlutterPhase += p.FlutterSpeed;
                Vector2 flutterOffset = new Vector2(
                    MathF.Sin(p.FlutterPhase) * p.FlutterAmplitude * 0.05f,
                    MathF.Cos(p.FlutterPhase * 0.8f) * p.FlutterAmplitude * 0.03f
                );
                p.Velocity *= 0.97f;
                p.Position += p.Velocity + flutterOffset;
                p.Life--;
                if (p.Life <= 0) bookPages.RemoveAt(i);
            }
        }

        private void SpawnRuneSymbol(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-RuneSymbolSpread, RuneSymbolSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.3f;
            Vector2 drift = Main.rand.NextVector2Circular(RuneSymbolDriftSpeed, RuneSymbolDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.6f, 1.3f) * RuneSymbolSize;
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            float spinSpeed = Main.rand.NextFloat(-RuneSymbolSpinSpeed, RuneSymbolSpinSpeed);
            float glowSpeed = RuneSymbolGlowSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            int runeIndex = Main.rand.Next(3);
            Color color = RuneSymbolColor * Main.rand.NextFloat(0.6f, 1f);

            runeSymbols.Add(new RuneSymbolParticle(pos, vel, RuneSymbolLife, scale, rotation, spinSpeed, glowSpeed, runeIndex, color));
        }

        private void SpawnWisdomGlow(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-WisdomGlowSpread, WisdomGlowSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            Vector2 inertia = -velocity * InertiaFactor * 0.2f;
            Vector2 drift = Main.rand.NextVector2Circular(0.4f, 0.4f);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.5f, 1.4f) * WisdomGlowSize;
            float expandRate = WisdomGlowExpandRate * Main.rand.NextFloat(0.7f, 1.3f);
            float pulseSpeed = WisdomGlowPulseSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float hueShift = WisdomGlowHueShift * Main.rand.NextFloat(0.5f, 1.5f);
            Color color = WisdomGlowColor * Main.rand.NextFloat(0.6f, 1f);

            wisdomGlows.Add(new WisdomGlowParticle(pos, vel, WisdomGlowLife, scale, expandRate, pulseSpeed, hueShift, color));
        }

        private void SpawnBookPage(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-BookPageSpread, BookPageSpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 inertia = -velocity * InertiaFactor * 0.4f;
            Vector2 drift = Main.rand.NextVector2Circular(BookPageDriftSpeed, BookPageDriftSpeed);
            Vector2 vel = inertia + drift;

            float scale = Main.rand.NextFloat(0.5f, 1.2f) * BookPageSize;
            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            float spinSpeed = Main.rand.NextFloat(-BookPageSpinSpeed, BookPageSpinSpeed);
            float flutterSpeed = BookPageFlutterSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float flutterAmplitude = BookPageFlutterAmplitude * Main.rand.NextFloat(0.6f, 1.4f);
            Color color = BookPageColor * Main.rand.NextFloat(0.5f, 1f);

            bookPages.Add(new BookPageParticle(pos, vel, BookPageLife, scale, rotation, spinSpeed, flutterSpeed, flutterAmplitude, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (bookPages.Count > 0 && _pageTex != null)
            {
                Vector2 pageOrigin = new Vector2(_pageTex.Width * 0.5f, _pageTex.Height * 0.5f);
                var sorted = bookPages.OrderBy(p => p.Life);
                foreach (var p in sorted)
                {
                    Color drawColor = p.Color * p.Alpha;
                    Vector2 pos = p.Position - Main.screenPosition;
                    sb.Draw(_pageTex, pos, null, drawColor, p.Rotation,
                        pageOrigin, p.Scale, SpriteEffects.None, 0);
                }
            }

            if (wisdomGlows.Count > 0 && _glowTex != null)
            {
                Vector2 glowOrigin = new Vector2(_glowTex.Width * 0.5f, _glowTex.Height * 0.5f);
                var sorted = wisdomGlows.OrderBy(g => g.Life);
                foreach (var g in sorted)
                {
                    Color drawColor = g.Color * g.Alpha;
                    Vector2 pos = g.Position - Main.screenPosition;
                    sb.Draw(_glowTex, pos, null, drawColor, g.Rotation,
                        glowOrigin, g.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (runeSymbols.Count > 0 && _runeTex != null)
            {
                Vector2 runeOrigin = new Vector2(_runeTex.Width * 0.5f, _runeTex.Height * 0.5f);
                var sorted = runeSymbols.OrderBy(r => r.Life);
                foreach (var r in sorted)
                {
                    Color drawColor = r.Color * r.Alpha;
                    Vector2 pos = r.Position - Main.screenPosition;
                    sb.Draw(_runeTex, pos, null, drawColor, r.Rotation,
                        runeOrigin, r.Scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            runeSymbols.Clear();
            wisdomGlows.Clear();
            bookPages.Clear();
            _ghostTrail?.Clear();
        }
    }
}