using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class YinYangTrail : ITrail
    {
        public class YinYangOrbParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public bool IsYang;
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

            public float CurrentScale => Scale * (1f - Progress * 0.3f);

            public YinYangOrbParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotSpeed, bool isYang, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                IsYang = isYang;
                Color = color;
            }
        }

        public class YinYangFishParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public bool IsYang;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.7f);
                }
            }

            public float CurrentScale => Scale * (1f - Progress * 0.2f);

            public YinYangFishParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotSpeed, bool isYang, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                IsYang = isYang;
                Color = color;
            }
        }

        public class YinYangSCurveParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float CurveAmplitude;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public bool IsYang;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 3f);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * 0.6f);
                }
            }

            public float CurrentAmplitude => CurveAmplitude * (1f - Progress * 0.5f);

            public YinYangSCurveParticle(Vector2 pos, Vector2 vel, int life, float scale, float amplitude, float rotSpeed, bool isYang, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                CurveAmplitude = amplitude;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                IsYang = isYang;
                Color = color;
            }
        }

        public class YinYangOrbitDotParticle
        {
            public Vector2 Center;
            public float OrbitRadius;
            public float Angle;
            public float AngularSpeed;
            public float Scale;
            public int Life;
            public int MaxLife;
            public bool IsYang;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 6f);
                    float fadeOut = 1f - Progress;
                    return MathF.Max(0f, fadeIn * fadeOut);
                }
            }

            public Vector2 Position => Center + new Vector2(MathF.Cos(Angle), MathF.Sin(Angle)) * OrbitRadius;

            public YinYangOrbitDotParticle(Vector2 center, float radius, float angle, float angularSpeed, int life, float scale, bool isYang, Color color)
            {
                Center = center;
                OrbitRadius = radius;
                Angle = angle;
                AngularSpeed = angularSpeed;
                MaxLife = life;
                Life = life;
                Scale = scale;
                IsYang = isYang;
                Color = color;
            }
        }

        public string Name { get; set; } = "YinYangTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 10;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.4f;
        public Color GhostColor { get; set; } = new Color(200, 195, 240, 180);

        public int MaxOrbs { get; set; } = 20;
        public int OrbLife { get; set; } = 35;
        public float OrbSize { get; set; } = 0.45f;
        public int OrbSpawnInterval { get; set; } = 2;
        public float OrbRotSpeed { get; set; } = 0.08f;
        public float OrbDriftSpeed { get; set; } = 0.2f;
        public float OrbSpread { get; set; } = 5f;
        public Color OrbYinColor { get; set; } = new Color(60, 50, 100, 220);
        public Color OrbYangColor { get; set; } = new Color(230, 225, 255, 220);

        public int MaxFish { get; set; } = 8;
        public int FishLife { get; set; } = 50;
        public float FishSize { get; set; } = 0.5f;
        public float FishSpawnChance { get; set; } = 0.04f;
        public float FishRotSpeed { get; set; } = 0.12f;
        public float FishDriftSpeed { get; set; } = 0.15f;
        public Color FishYinColor { get; set; } = new Color(50, 40, 90, 200);
        public Color FishYangColor { get; set; } = new Color(220, 215, 250, 200);

        public int MaxSCurves { get; set; } = 12;
        public int SCurveLife { get; set; } = 45;
        public float SCurveSize { get; set; } = 0.5f;
        public float SCurveAmplitude { get; set; } = 12f;
        public float SCurveSpawnChance { get; set; } = 0.06f;
        public float SCurveRotSpeed { get; set; } = 0.05f;
        public float SCurveDriftSpeed { get; set; } = 0.1f;
        public Color SCurveYinColor { get; set; } = new Color(70, 55, 120, 180);
        public Color SCurveYangColor { get; set; } = new Color(210, 200, 245, 180);

        public int MaxOrbitDots { get; set; } = 24;
        public int OrbitDotLife { get; set; } = 30;
        public float OrbitDotSize { get; set; } = 0.3f;
        public float OrbitDotRadius { get; set; } = 20f;
        public float OrbitDotAngularSpeed { get; set; } = 0.08f;
        public float OrbitDotSpawnChance { get; set; } = 0.15f;
        public Color OrbitDotYinColor { get; set; } = new Color(80, 65, 140, 220);
        public Color OrbitDotYangColor { get; set; } = new Color(240, 235, 255, 220);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<YinYangOrbParticle> orbs = new();
        private List<YinYangFishParticle> fish = new();
        private List<YinYangSCurveParticle> sCurves = new();
        private List<YinYangOrbitDotParticle> orbitDots = new();
        private int orbCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _orbTex;
        private Texture2D _fishTex;
        private Texture2D _sCurveTex;
        private Texture2D _orbitDotTex;
        private Texture2D _ghostTex;

        public bool HasContent => orbs.Count > 0 || fish.Count > 0 || sCurves.Count > 0 || orbitDots.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_orbTex != null) return;
            _orbTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/YinYangTrail/YinYangTrailOrb").Value;
            _fishTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/YinYangTrail/YinYangTrailFish").Value;
            _sCurveTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/YinYangTrail/YinYangTrailSCurve").Value;
            _orbitDotTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/YinYangTrail/YinYangTrailOrbitDot").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/YinYangTrail/YinYangTrailGhost").Value;
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

            if (fish.Count < MaxFish && Main.rand.NextFloat() < FishSpawnChance)
                SpawnFish(center, velocity, moveDir);

            if (sCurves.Count < MaxSCurves && Main.rand.NextFloat() < SCurveSpawnChance)
                SpawnSCurve(center, velocity, moveDir);

            if (orbitDots.Count < MaxOrbitDots && Main.rand.NextFloat() < OrbitDotSpawnChance)
                SpawnOrbitDot(center, velocity);

            for (int i = orbs.Count - 1; i >= 0; i--)
            {
                var o = orbs[i];
                o.Rotation += o.RotSpeed;
                o.Velocity *= 0.96f;
                o.Position += o.Velocity;
                o.Life--;
                if (o.Life <= 0) orbs.RemoveAt(i);
            }

            for (int i = fish.Count - 1; i >= 0; i--)
            {
                var f = fish[i];
                f.Rotation += f.RotSpeed;
                f.Velocity *= 0.97f;
                f.Position += f.Velocity;
                f.Life--;
                if (f.Life <= 0) fish.RemoveAt(i);
            }

            for (int i = sCurves.Count - 1; i >= 0; i--)
            {
                var sc = sCurves[i];
                sc.Rotation += sc.RotSpeed;
                sc.Velocity *= 0.97f;
                sc.Position += sc.Velocity;
                sc.Life--;
                if (sc.Life <= 0) sCurves.RemoveAt(i);
            }

            for (int i = orbitDots.Count - 1; i >= 0; i--)
            {
                var od = orbitDots[i];
                od.Angle += od.AngularSpeed;
                od.Center += (center - od.Center) * 0.05f;
                od.OrbitRadius *= 1f + 0.005f;
                od.Life--;
                if (od.Life <= 0) orbitDots.RemoveAt(i);
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

            bool isYang = Main.rand.NextBool();
            float scale = Main.rand.NextFloat(0.6f, 1.2f) * OrbSize;
            float rotSpeed = Main.rand.NextFloat(-OrbRotSpeed, OrbRotSpeed);
            Color color = (isYang ? OrbYangColor : OrbYinColor) * Main.rand.NextFloat(0.6f, 1f);

            orbs.Add(new YinYangOrbParticle(pos, vel, OrbLife, scale, rotSpeed, isYang, color));
        }

        private void SpawnFish(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-8f, 8f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(5f, 5f);

            Vector2 drift = Main.rand.NextVector2Circular(FishDriftSpeed, FishDriftSpeed);
            bool isYang = Main.rand.NextBool();
            float scale = Main.rand.NextFloat(0.7f, 1.3f) * FishSize;
            float rotSpeed = Main.rand.NextFloat(-FishRotSpeed, FishRotSpeed);
            Color color = (isYang ? FishYangColor : FishYinColor) * Main.rand.NextFloat(0.5f, 1f);

            fish.Add(new YinYangFishParticle(pos, drift, FishLife, scale, rotSpeed, isYang, color));
        }

        private void SpawnSCurve(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-6f, 6f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(4f, 4f);

            Vector2 drift = Main.rand.NextVector2Circular(SCurveDriftSpeed, SCurveDriftSpeed);
            bool isYang = Main.rand.NextBool();
            float scale = Main.rand.NextFloat(0.7f, 1.3f) * SCurveSize;
            float amplitude = SCurveAmplitude * Main.rand.NextFloat(0.6f, 1.4f);
            float rotSpeed = Main.rand.NextFloat(-SCurveRotSpeed, SCurveRotSpeed);
            Color color = (isYang ? SCurveYangColor : SCurveYinColor) * Main.rand.NextFloat(0.5f, 1f);

            sCurves.Add(new YinYangSCurveParticle(pos, drift, SCurveLife, scale, amplitude, rotSpeed, isYang, color));
        }

        private void SpawnOrbitDot(Vector2 center, Vector2 velocity)
        {
            bool isYang = Main.rand.NextBool();
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float radius = OrbitDotRadius * Main.rand.NextFloat(0.6f, 1.4f);
            float angularSpeed = OrbitDotAngularSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            if (!isYang) angularSpeed = -angularSpeed;

            float scale = Main.rand.NextFloat(0.5f, 1.2f) * OrbitDotSize;
            Color color = (isYang ? OrbitDotYangColor : OrbitDotYinColor) * Main.rand.NextFloat(0.6f, 1f);

            orbitDots.Add(new YinYangOrbitDotParticle(center, radius, angle, angularSpeed, OrbitDotLife, scale, isYang, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (sCurves.Count > 0 && _sCurveTex != null)
            {
                Vector2 scOrigin = _sCurveTex.Size() * 0.5f;
                var sortedSCurves = sCurves.OrderBy(sc => sc.Life);
                foreach (var sc in sortedSCurves)
                {
                    Color drawColor = sc.Color * sc.Alpha;
                    Vector2 pos = sc.Position - Main.screenPosition;
                    SpriteEffects fx = sc.IsYang ? SpriteEffects.None : SpriteEffects.FlipVertically;
                    Vector2 scale = new Vector2(sc.CurrentAmplitude / _sCurveTex.Width, sc.Scale);
                    sb.Draw(_sCurveTex, pos, null, drawColor, sc.Rotation,
                        scOrigin, scale, fx, 0);
                }
            }

            if (fish.Count > 0 && _fishTex != null)
            {
                Vector2 fishOrigin = _fishTex.Size() * 0.5f;
                var sortedFish = fish.OrderBy(f => f.Life);
                foreach (var f in sortedFish)
                {
                    Color drawColor = f.Color * f.Alpha;
                    Vector2 pos = f.Position - Main.screenPosition;
                    SpriteEffects fx = f.IsYang ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    sb.Draw(_fishTex, pos, null, drawColor, f.Rotation,
                        fishOrigin, f.CurrentScale, fx, 0);
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
                    SpriteEffects fx = o.IsYang ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    sb.Draw(_orbTex, pos, null, drawColor, o.Rotation,
                        orbOrigin, o.CurrentScale, fx, 0);
                }
            }

            if (orbitDots.Count > 0 && _orbitDotTex != null)
            {
                Vector2 dotOrigin = _orbitDotTex.Size() * 0.5f;
                var sortedDots = orbitDots.OrderBy(d => d.Life);
                foreach (var d in sortedDots)
                {
                    Color drawColor = d.Color * d.Alpha;
                    Vector2 pos = d.Position - Main.screenPosition;
                    sb.Draw(_orbitDotTex, pos, null, drawColor, d.Angle,
                        dotOrigin, d.Scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            orbs.Clear();
            fish.Clear();
            sCurves.Clear();
            orbitDots.Clear();
            orbCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
