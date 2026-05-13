using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class CloudTrail : ITrail
    {
        public class CloudPuffParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float ExpandSpeed;
            public float Fluffiness;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 2.5f);
                    float fadeOut = (1f - Progress) * (1f - Progress) * (1f - Progress);
                    return MathF.Max(0f, fadeIn * fadeOut * Fluffiness);
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

            public CloudPuffParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float expandSpeed, float rotSpeed, float fluffiness, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                ExpandSpeed = expandSpeed;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotSpeed = rotSpeed;
                Fluffiness = fluffiness;
                Color = color;
            }
        }

        public class VaporWispParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Length;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float WavePhase;
            public float WaveSpeed;
            public float WaveAmplitude;
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

            public float CurrentLength => Length * MathF.Min(1f, Progress * 3f) * MathF.Max(0f, 1f - Progress * 0.5f);

            public float CurrentWave => MathF.Sin(WavePhase) * WaveAmplitude;

            public VaporWispParticle(Vector2 pos, Vector2 vel, int life, float scale, float length, float rotation, float waveSpeed, float waveAmplitude, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                Length = length;
                Rotation = rotation;
                WavePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                WaveSpeed = waveSpeed;
                WaveAmplitude = waveAmplitude;
                Color = color;
            }
        }

        public class CondensationParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float MaxScale;
            public int Life;
            public int MaxLife;
            public float ExpandSpeed;
            public float ShrinkStart;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;

            public float Alpha
            {
                get
                {
                    float fadeIn = MathF.Min(1f, Progress * 5f);
                    float fadeOut = Progress > ShrinkStart ? (1f - (Progress - ShrinkStart) / (1f - ShrinkStart)) : 1f;
                    return MathF.Max(0f, fadeIn * fadeOut * 0.35f);
                }
            }

            public float CurrentScale
            {
                get
                {
                    float expand = MathF.Min(1f, Progress * ExpandSpeed);
                    float shrink = Progress > ShrinkStart ? 1f - (Progress - ShrinkStart) / (1f - ShrinkStart) * 0.3f : 1f;
                    return (Scale + (MaxScale - Scale) * expand) * shrink;
                }
            }

            public CondensationParticle(Vector2 pos, Vector2 vel, int life, float scale, float maxScale, float expandSpeed, float shrinkStart, Color color)
            {
                Position = pos;
                Velocity = vel;
                MaxLife = life;
                Life = life;
                Scale = scale;
                MaxScale = maxScale;
                ExpandSpeed = expandSpeed;
                ShrinkStart = shrinkStart;
                Color = color;
            }
        }

        public string Name { get; set; } = "CloudTrail";

        public BlendState BlendMode => BlendState.AlphaBlend;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 12;
        public int GhostRecordInterval { get; set; } = 3;
        public float GhostWidthScale { get; set; } = 0.3f;
        public float GhostLengthScale { get; set; } = 1.8f;
        public float GhostAlpha { get; set; } = 0.2f;
        public Color GhostColor { get; set; } = new Color(220, 225, 240, 100);

        public int MaxCloudPuffs { get; set; } = 12;
        public int CloudPuffLife { get; set; } = 60;
        public float CloudPuffStartSize { get; set; } = 0.4f;
        public float CloudPuffEndSize { get; set; } = 2.5f;
        public float CloudPuffSpawnChance { get; set; } = 0.06f;
        public float CloudPuffExpandSpeed { get; set; } = 1.5f;
        public float CloudPuffRotSpeed { get; set; } = 0.01f;
        public float CloudPuffDriftSpeed { get; set; } = 0.06f;
        public Color CloudPuffColor { get; set; } = new Color(230, 235, 250, 120);

        public int MaxVaporWisps { get; set; } = 15;
        public int VaporWispLife { get; set; } = 40;
        public float VaporWispSize { get; set; } = 0.3f;
        public float VaporWispLength { get; set; } = 20f;
        public float VaporWispSpawnChance { get; set; } = 0.1f;
        public float VaporWispWaveSpeed { get; set; } = 0.12f;
        public float VaporWispWaveAmplitude { get; set; } = 0.3f;
        public float VaporWispDriftSpeed { get; set; } = 0.08f;
        public Color VaporWispColor { get; set; } = new Color(210, 220, 245, 140);

        public int MaxCondensations { get; set; } = 8;
        public int CondensationLife { get; set; } = 50;
        public float CondensationStartSize { get; set; } = 0.3f;
        public float CondensationEndSize { get; set; } = 1.8f;
        public float CondensationSpawnChance { get; set; } = 0.03f;
        public float CondensationExpandSpeed { get; set; } = 2f;
        public float CondensationDriftSpeed { get; set; } = 0.04f;
        public Color CondensationColor { get; set; } = new Color(200, 210, 240, 100);

        public float InertiaFactor { get; set; } = 0.12f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<CloudPuffParticle> cloudPuffs = new();
        private List<VaporWispParticle> vaporWisps = new();
        private List<CondensationParticle> condensations = new();

        private GhostTrail _ghostTrail;

        private Texture2D _puffTex;
        private Texture2D _wispTex;
        private Texture2D _condTex;
        private Texture2D _ghostTex;

        public bool HasContent => cloudPuffs.Count > 0 || vaporWisps.Count > 0 || condensations.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_puffTex != null) return;
            _puffTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/CloudTrail/CloudTrailPuff").Value;
            _wispTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/CloudTrail/CloudTrailWisp").Value;
            _condTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/CloudTrail/CloudTrailCond").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/CloudTrail/CloudTrailGhost").Value;
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
                UseAdditiveBlend = false,
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

            if (cloudPuffs.Count < MaxCloudPuffs && Main.rand.NextFloat() < CloudPuffSpawnChance)
                SpawnCloudPuff(center, velocity, moveDir);

            if (vaporWisps.Count < MaxVaporWisps && Main.rand.NextFloat() < VaporWispSpawnChance)
                SpawnVaporWisp(center, velocity, moveDir);

            if (condensations.Count < MaxCondensations && Main.rand.NextFloat() < CondensationSpawnChance)
                SpawnCondensation(center, velocity, moveDir);

            for (int i = cloudPuffs.Count - 1; i >= 0; i--)
            {
                var c = cloudPuffs[i];
                c.Rotation += c.RotSpeed;
                c.Velocity *= 0.98f;
                c.Position += c.Velocity;
                c.Life--;
                if (c.Life <= 0) cloudPuffs.RemoveAt(i);
            }

            for (int i = vaporWisps.Count - 1; i >= 0; i--)
            {
                var w = vaporWisps[i];
                w.WavePhase += w.WaveSpeed;
                w.Rotation += w.CurrentWave * 0.02f;
                w.Velocity *= 0.97f;
                w.Position += w.Velocity;
                w.Life--;
                if (w.Life <= 0) vaporWisps.RemoveAt(i);
            }

            for (int i = condensations.Count - 1; i >= 0; i--)
            {
                var c = condensations[i];
                c.Velocity *= 0.98f;
                c.Position += c.Velocity;
                c.Life--;
                if (c.Life <= 0) condensations.RemoveAt(i);
            }
        }

        private void SpawnCloudPuff(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-8f, 8f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(5f, 5f);

            Vector2 drift = Main.rand.NextVector2Circular(CloudPuffDriftSpeed, CloudPuffDriftSpeed);
            float startSize = CloudPuffStartSize * Main.rand.NextFloat(0.7f, 1.3f);
            float endSize = CloudPuffEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            float rotSpeed = Main.rand.NextFloat(-CloudPuffRotSpeed, CloudPuffRotSpeed);
            float fluffiness = Main.rand.NextFloat(0.6f, 1f);
            Color color = CloudPuffColor * Main.rand.NextFloat(0.7f, 1f);

            cloudPuffs.Add(new CloudPuffParticle(pos, drift, CloudPuffLife, startSize, endSize, CloudPuffExpandSpeed, rotSpeed, fluffiness, color));
        }

        private void SpawnVaporWisp(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-6f, 6f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(3f, 3f);

            Vector2 drift = Main.rand.NextVector2Circular(VaporWispDriftSpeed, VaporWispDriftSpeed);
            float rotation = velocity.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
            float length = VaporWispLength * Main.rand.NextFloat(0.6f, 1.4f);
            float scale = VaporWispSize * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = VaporWispColor * Main.rand.NextFloat(0.6f, 1f);

            vaporWisps.Add(new VaporWispParticle(pos, drift, VaporWispLife, scale, length, rotation, VaporWispWaveSpeed, VaporWispWaveAmplitude, color));
        }

        private void SpawnCondensation(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 perpDir = new Vector2(-moveDir.Y, moveDir.X);
            float sideOffset = Main.rand.NextFloat(-10f, 10f);
            Vector2 pos = center + SpawnOffset + perpDir * sideOffset + Main.rand.NextVector2Circular(8f, 8f);

            Vector2 drift = Main.rand.NextVector2Circular(CondensationDriftSpeed, CondensationDriftSpeed);
            float startSize = CondensationStartSize * Main.rand.NextFloat(0.8f, 1.2f);
            float endSize = CondensationEndSize * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = CondensationColor * Main.rand.NextFloat(0.6f, 1f);

            condensations.Add(new CondensationParticle(pos, drift, CondensationLife, startSize, endSize, CondensationExpandSpeed, 0.6f, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null)
                _ghostTrail.Draw(sb);

            if (condensations.Count > 0 && _condTex != null)
            {
                Vector2 condOrigin = _condTex.Size() * 0.5f;
                var sorted = condensations.OrderBy(c => c.Life);
                foreach (var c in sorted)
                {
                    Color drawColor = c.Color * c.Alpha;
                    Vector2 pos = c.Position - Main.screenPosition;
                    sb.Draw(_condTex, pos, null, drawColor, 0f,
                        condOrigin, c.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (cloudPuffs.Count > 0 && _puffTex != null)
            {
                Vector2 puffOrigin = _puffTex.Size() * 0.5f;
                var sorted = cloudPuffs.OrderBy(c => c.Life);
                foreach (var c in sorted)
                {
                    Color drawColor = c.Color * c.Alpha;
                    Vector2 pos = c.Position - Main.screenPosition;
                    sb.Draw(_puffTex, pos, null, drawColor, c.Rotation,
                        puffOrigin, c.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (vaporWisps.Count > 0 && _wispTex != null)
            {
                Vector2 wispOrigin = new Vector2(0f, _wispTex.Height * 0.5f);
                var sorted = vaporWisps.OrderBy(w => w.Life);
                foreach (var w in sorted)
                {
                    Color drawColor = w.Color * w.Alpha;
                    Vector2 pos = w.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(w.CurrentLength / _wispTex.Width, w.Scale);
                    sb.Draw(_wispTex, pos, null, drawColor, w.Rotation,
                        wispOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            cloudPuffs.Clear();
            vaporWisps.Clear();
            condensations.Clear();
            _ghostTrail?.Clear();
        }
    }
}
