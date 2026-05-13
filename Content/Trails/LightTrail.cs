using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class LightTrail : ITrail
    {
        public class LightRayParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Length;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float flash = Progress < 0.1f ? Progress / 0.1f : 1f;
                    float fadeOut = 1f - Progress * Progress;
                    return MathF.Max(0f, flash * fadeOut);
                }
            }

            public float CurrentLength => Length * (1f - Progress * 0.5f);

            public float CurrentWidth => Scale * (1f - Progress * 0.6f);

            public LightRayParticle(Vector2 pos, Vector2 vel, int life, float scale, float length, float rotation, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Length = length;
                Rotation = rotation;
                Color = color;
            }
        }

        public class LightPrismParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float HueOffset;
            public float HueSpeed;
            public float Rotation;
            public float SpinSpeed;
            public int FacetCount;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 4f);
                    float fadeOut = 1f - Progress * Progress;
                    return MathF.Max(0f, fadeIn * fadeOut * 0.7f);
                }
            }

            public Color CurrentColor
            {
                get
                {
                    float hue = (HueOffset + Progress * HueSpeed) % 1f;
                    if (hue < 0) hue += 1f;
                    return ColorFromHSV(hue * 360f, 0.6f, 1f) * Alpha;
                }
            }

            private static Color ColorFromHSV(float h, float s, float v)
            {
                int hi = (int)(h / 60f) % 6;
                float f = h / 60f - (int)(h / 60f);
                float p = v * (1f - s);
                float q = v * (1f - f * s);
                float t = v * (1f - (1f - f) * s);
                return hi switch
                {
                    0 => new Color(v, t, p),
                    1 => new Color(q, v, p),
                    2 => new Color(p, v, t),
                    3 => new Color(p, q, v),
                    4 => new Color(t, p, v),
                    _ => new Color(v, p, q)
                };
            }

            public LightPrismParticle(Vector2 pos, Vector2 vel, int life, float scale, float hueOffset, float hueSpeed, float spinSpeed, int facetCount)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                HueOffset = hueOffset;
                HueSpeed = hueSpeed;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                SpinSpeed = spinSpeed;
                FacetCount = facetCount;
            }
        }

        public class LightHaloParticle
        {
            public Vector2 Position;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float ExpandSpeed;
            public Color InnerColor;
            public Color OuterColor;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float expand = MathF.Min(1f, Progress * ExpandSpeed);
                    float fadeOut = (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, expand * fadeOut * 0.5f);
                }
            }

            public float CurrentScale
            {
                get
                {
                    float expand = MathF.Min(1f, Progress * ExpandSpeed);
                    return Scale + (MaxScale - Scale) * expand;
                }
            }

            public Color CurrentInnerColor => InnerColor * Alpha;
            public Color CurrentOuterColor => OuterColor * (Alpha * 0.6f);

            public LightHaloParticle(Vector2 pos, int life, float scale, float maxScale, float expandSpeed, float rotSpeed, Color innerColor, Color outerColor)
            {
                Position = pos;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                ExpandSpeed = expandSpeed;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                InnerColor = innerColor;
                OuterColor = outerColor;
            }
        }

        public string Name { get; set; } = "LightTrail";

        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 1;
        public float GhostWidthScale { get; set; } = 0.3f;
        public float GhostLengthScale { get; set; } = 2.0f;
        public float GhostAlpha { get; set; } = 0.55f;
        public Color GhostColor { get; set; } = new Color(255, 255, 200, 200);

        public int MaxRays { get; set; } = 35;
        public int RayLife { get; set; } = 10;
        public float RayScale { get; set; } = 0.4f;
        public float RayLength { get; set; } = 22f;
        public int RaySpawnInterval { get; set; } = 1;
        public float RaySpread { get; set; } = 6f;
        public float RayDriftSpeed { get; set; } = 0.15f;
        public Color RayColor { get; set; } = new Color(255, 255, 220, 240);

        public int MaxPrisms { get; set; } = 15;
        public int PrismLife { get; set; } = 30;
        public float PrismSize { get; set; } = 0.4f;
        public float PrismSpawnChance { get; set; } = 0.12f;
        public float PrismHueSpeed { get; set; } = 3.0f;
        public float PrismSpinSpeed { get; set; } = 0.08f;
        public float PrismDriftSpeed { get; set; } = 0.12f;

        public int MaxHalos { get; set; } = 5;
        public int HaloLife { get; set; } = 45;
        public float HaloStartSize { get; set; } = 0.2f;
        public float HaloEndSize { get; set; } = 1.8f;
        public float HaloSpawnChance { get; set; } = 0.02f;
        public float HaloExpandSpeed { get; set; } = 2.5f;
        public float HaloRotSpeed { get; set; } = 0.04f;
        public Color HaloInnerColor { get; set; } = new Color(255, 255, 230, 220);
        public Color HaloOuterColor { get; set; } = new Color(255, 240, 180, 160);

        public float InertiaFactor { get; set; } = 0.15f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<LightRayParticle> rays = new();
        private List<LightPrismParticle> prisms = new();
        private List<LightHaloParticle> halos = new();
        private int rayCounter = 0;

        private GhostTrail _ghostTrail;

        private Texture2D _rayTex;
        private Texture2D _prismTex;
        private Texture2D _haloTex;
        private Texture2D _ghostTex;

        public bool HasContent => rays.Count > 0 || prisms.Count > 0 || halos.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_rayTex != null) return;
            _rayTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LightTrail/LightTrailRay").Value;
            _prismTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LightTrail/LightTrailPrism").Value;
            _haloTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LightTrail/LightTrailHalo").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LightTrail/LightTrailGhost").Value;
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

            rayCounter++;
            if (rayCounter >= RaySpawnInterval && rays.Count < MaxRays)
            {
                rayCounter = 0;
                SpawnRay(center, velocity, moveDir);
            }

            if (prisms.Count < MaxPrisms && Main.rand.NextFloat() < PrismSpawnChance)
                SpawnPrism(center, velocity, moveDir);

            if (halos.Count < MaxHalos && Main.rand.NextFloat() < HaloSpawnChance)
                SpawnHalo(center);

            for (int i = rays.Count - 1; i >= 0; i--)
            {
                var r = rays[i];
                r.Velocity *= 0.92f;
                r.Position += r.Velocity;
                r.Life--;
                if (r.Life <= 0) rays.RemoveAt(i);
            }

            for (int i = prisms.Count - 1; i >= 0; i--)
            {
                var p = prisms[i];
                p.Rotation += p.SpinSpeed;
                p.Velocity *= 0.95f;
                p.Position += p.Velocity;
                p.Life--;
                if (p.Life <= 0) prisms.RemoveAt(i);
            }

            for (int i = halos.Count - 1; i >= 0; i--)
            {
                var h = halos[i];
                h.Rotation += h.RotSpeed;
                h.Life--;
                if (h.Life <= 0) halos.RemoveAt(i);
            }
        }

        private void SpawnRay(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-RaySpread, RaySpread);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(2f, 2f);

            float baseAngle = velocity.Length() > 0.5f
                ? velocity.ToRotation() + Main.rand.NextFloat(-1.2f, 1.2f)
                : Main.rand.NextFloat(MathHelper.TwoPi);

            Vector2 drift = Main.rand.NextVector2Circular(RayDriftSpeed, RayDriftSpeed);
            float scale = Main.rand.NextFloat(0.5f, 1.3f) * RayScale;
            float length = Main.rand.NextFloat(0.6f, 1.5f) * RayLength;
            Color color = RayColor * Main.rand.NextFloat(0.6f, 1f);

            rays.Add(new LightRayParticle(pos, drift, RayLife, scale, length, baseAngle, color));
        }

        private void SpawnPrism(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-8f, 8f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(5f, 5f);

            Vector2 drift = Main.rand.NextVector2Circular(PrismDriftSpeed, PrismDriftSpeed);
            float scale = Main.rand.NextFloat(0.5f, 1.2f) * PrismSize;
            float hueOffset = Main.rand.NextFloat();
            float hueSpeed = PrismHueSpeed * Main.rand.NextFloat(0.6f, 1.4f);
            float spinSpeed = Main.rand.NextFloat(-PrismSpinSpeed, PrismSpinSpeed);
            int facetCount = Main.rand.Next(3, 7);

            prisms.Add(new LightPrismParticle(pos, drift, PrismLife, scale, hueOffset, hueSpeed, spinSpeed, facetCount));
        }

        private void SpawnHalo(Vector2 center)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(4f, 4f);
            float startSize = HaloStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = HaloEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            float expandSpeed = HaloExpandSpeed * Main.rand.NextFloat(0.7f, 1.3f);
            float rotSpeed = Main.rand.NextFloat(-HaloRotSpeed, HaloRotSpeed);
            Color innerColor = HaloInnerColor * Main.rand.NextFloat(0.7f, 1f);
            Color outerColor = HaloOuterColor * Main.rand.NextFloat(0.6f, 1f);

            halos.Add(new LightHaloParticle(pos, HaloLife, startSize, endSize, expandSpeed, rotSpeed, innerColor, outerColor));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (halos.Count > 0 && _haloTex != null)
            {
                Vector2 haloOrigin = _haloTex.Size() * 0.5f;
                var sortedHalos = halos.OrderBy(h => h.Life);
                foreach (var h in sortedHalos)
                {
                    Vector2 pos = h.Position - Main.screenPosition;

                    Color outerDrawColor = h.CurrentOuterColor;
                    sb.Draw(_haloTex, pos, null, outerDrawColor, h.Rotation,
                        haloOrigin, h.CurrentScale * 1.2f, SpriteEffects.None, 0);

                    Color innerDrawColor = h.CurrentInnerColor;
                    sb.Draw(_haloTex, pos, null, innerDrawColor, h.Rotation,
                        haloOrigin, h.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (rays.Count > 0 && _rayTex != null)
            {
                Vector2 rayOrigin = new Vector2(0f, _rayTex.Height * 0.5f);
                var sortedRays = rays.OrderBy(r => r.Life);
                foreach (var r in sortedRays)
                {
                    Color drawColor = r.Color * r.Alpha;
                    Vector2 pos = r.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(r.CurrentLength / _rayTex.Width, r.CurrentWidth);
                    sb.Draw(_rayTex, pos, null, drawColor, r.Rotation,
                        rayOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (prisms.Count > 0 && _prismTex != null)
            {
                Vector2 prismOrigin = _prismTex.Size() * 0.5f;
                var sortedPrisms = prisms.OrderBy(p => p.Life);
                foreach (var p in sortedPrisms)
                {
                    Color drawColor = p.CurrentColor;
                    Vector2 pos = p.Position - Main.screenPosition;
                    sb.Draw(_prismTex, pos, null, drawColor, p.Rotation,
                        prismOrigin, p.Scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            rays.Clear();
            prisms.Clear();
            halos.Clear();
            rayCounter = 0;
            _ghostTrail?.Clear();
        }
    }
}
