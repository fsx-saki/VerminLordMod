using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class LuckTrail : ITrail
    {
        public class LuckyCloverParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float PulsePhase;
            public float FloatAmplitude;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 4f) * (1f - Progress) * (0.8f + 0.2f * MathF.Sin(PulsePhase)));
            public float CurrentScale => Scale * (1f + 0.15f * MathF.Sin(PulsePhase * 2f));

            public LuckyCloverParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotSpeed, float floatAmplitude, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Scale = scale; Rotation = Main.rand.NextFloat(MathHelper.TwoPi); RotSpeed = rotSpeed;
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi); FloatAmplitude = floatAmplitude; Color = color;
            }
        }

        public class FortuneStarParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float TwinklePhase;
            public float TwinkleSpeed;
            public float DriftAngle;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 6f) * (1f - Progress) * (0.5f + 0.5f * MathF.Abs(MathF.Sin(TwinklePhase))));
            public float CurrentScale => Scale * (0.6f + 0.4f * MathF.Abs(MathF.Sin(TwinklePhase)));

            public FortuneStarParticle(Vector2 pos, Vector2 vel, int life, float scale, float twinkleSpeed, float driftAngle, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Scale = scale; Rotation = Main.rand.NextFloat(MathHelper.TwoPi); TwinklePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                TwinkleSpeed = twinkleSpeed; DriftAngle = driftAngle; Color = color;
            }
        }

        public class FateThreadParticle
        {
            public Vector2 StartPos;
            public Vector2 EndPos;
            public Vector2 StartVel;
            public Vector2 EndVel;
            public float Width;
            public int Life;
            public int MaxLife;
            public float CurlPhase;
            public float CurlAmount;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 3f) * (1f - Progress * Progress) * 0.4f);
            public float CurrentWidth => Width * (1f - Progress * 0.5f);

            public FateThreadParticle(Vector2 start, Vector2 end, Vector2 startVel, Vector2 endVel, int life, float width, float curlAmount, Color color)
            {
                StartPos = start; EndPos = end; StartVel = startVel; EndVel = endVel;
                MaxLife = life; Life = life; Width = width; CurlPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                CurlAmount = curlAmount; Color = color;
            }
        }

        public string Name { get; set; } = "LuckTrail";
        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.15f;
        public float GhostLengthScale { get; set; } = 1.3f;
        public float GhostAlpha { get; set; } = 0.25f;
        public Color GhostColor { get; set; } = new Color(180, 230, 180, 120);

        public int MaxLuckyClovers { get; set; } = 8;
        public int LuckyCloverLife { get; set; } = 40;
        public float LuckyCloverSize { get; set; } = 0.5f;
        public float LuckyCloverSpawnChance { get; set; } = 0.04f;
        public float LuckyCloverRotSpeed { get; set; } = 0.03f;
        public float LuckyCloverFloatAmplitude { get; set; } = 1.5f;
        public float LuckyCloverDriftSpeed { get; set; } = 0.2f;
        public Color LuckyCloverColor { get; set; } = new Color(100, 220, 100, 220);

        public int MaxFortuneStars { get; set; } = 15;
        public int FortuneStarLife { get; set; } = 25;
        public float FortuneStarSize { get; set; } = 0.35f;
        public float FortuneStarSpawnChance { get; set; } = 0.12f;
        public float FortuneStarTwinkleSpeed { get; set; } = 0.15f;
        public float FortuneStarDriftSpeed { get; set; } = 0.3f;
        public Color FortuneStarColor { get; set; } = new Color(255, 220, 80, 230);

        public int MaxFateThreads { get; set; } = 5;
        public int FateThreadLife { get; set; } = 30;
        public float FateThreadWidth { get; set; } = 0.15f;
        public float FateThreadSpawnChance { get; set; } = 0.03f;
        public float FateThreadCurlAmount { get; set; } = 4f;
        public float FateThreadSpread { get; set; } = 20f;
        public Color FateThreadColor { get; set; } = new Color(220, 200, 100, 180);

        public float InertiaFactor { get; set; } = 0.1f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<LuckyCloverParticle> luckyClovers = new();
        private List<FortuneStarParticle> fortuneStars = new();
        private List<FateThreadParticle> fateThreads = new();
        private GhostTrail _ghostTrail;
        private Texture2D _cloverTex, _starTex, _threadTex, _ghostTex;

        public bool HasContent => luckyClovers.Count > 0 || fortuneStars.Count > 0 || fateThreads.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_cloverTex != null) return;
            _cloverTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LuckTrail/LuckTrailClover").Value;
            _starTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LuckTrail/LuckTrailStar").Value;
            _threadTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LuckTrail/LuckTrailThread").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/LuckTrail/LuckTrailGhost").Value;
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

            if (luckyClovers.Count < MaxLuckyClovers && Main.rand.NextFloat() < LuckyCloverSpawnChance)
                SpawnLuckyClover(center, velocity, moveDir);
            if (fortuneStars.Count < MaxFortuneStars && Main.rand.NextFloat() < FortuneStarSpawnChance)
                SpawnFortuneStar(center, velocity, moveDir);
            if (fateThreads.Count < MaxFateThreads && Main.rand.NextFloat() < FateThreadSpawnChance)
                SpawnFateThread(center, velocity, moveDir);

            for (int i = luckyClovers.Count - 1; i >= 0; i--)
            {
                var c = luckyClovers[i];
                c.Rotation += c.RotSpeed; c.PulsePhase += 0.08f;
                c.Velocity *= 0.97f;
                c.Position += c.Velocity + new Vector2(0f, MathF.Sin(c.PulsePhase) * c.FloatAmplitude * 0.05f);
                c.Life--;
                if (c.Life <= 0) luckyClovers.RemoveAt(i);
            }
            for (int i = fortuneStars.Count - 1; i >= 0; i--)
            {
                var s = fortuneStars[i];
                s.TwinklePhase += s.TwinkleSpeed; s.Rotation += 0.02f;
                s.Velocity *= 0.95f; s.Position += s.Velocity; s.Life--;
                if (s.Life <= 0) fortuneStars.RemoveAt(i);
            }
            for (int i = fateThreads.Count - 1; i >= 0; i--)
            {
                var t = fateThreads[i];
                t.CurlPhase += 0.1f;
                t.StartPos += t.StartVel; t.EndPos += t.EndVel;
                t.StartVel *= 0.98f; t.EndVel *= 0.98f;
                t.Life--;
                if (t.Life <= 0) fateThreads.RemoveAt(i);
            }
        }

        private void SpawnLuckyClover(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(2f, 2f);
            Vector2 vel = -velocity * InertiaFactor * 0.3f + Main.rand.NextVector2Circular(LuckyCloverDriftSpeed, LuckyCloverDriftSpeed);
            float scale = Main.rand.NextFloat(0.6f, 1.3f) * LuckyCloverSize;
            float rotSpeed = Main.rand.NextFloat(-LuckyCloverRotSpeed, LuckyCloverRotSpeed);
            float floatAmp = LuckyCloverFloatAmplitude * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = LuckyCloverColor * Main.rand.NextFloat(0.7f, 1f);
            luckyClovers.Add(new LuckyCloverParticle(pos, vel, LuckyCloverLife, scale, rotSpeed, floatAmp, color));
        }

        private void SpawnFortuneStar(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(3f, 3f);
            Vector2 vel = -velocity * InertiaFactor * 0.5f + Main.rand.NextVector2Circular(FortuneStarDriftSpeed, FortuneStarDriftSpeed);
            float scale = Main.rand.NextFloat(0.5f, 1.4f) * FortuneStarSize;
            float twinkleSpeed = FortuneStarTwinkleSpeed * Main.rand.NextFloat(0.7f, 1.5f);
            float driftAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            Color color = FortuneStarColor * Main.rand.NextFloat(0.6f, 1f);
            fortuneStars.Add(new FortuneStarParticle(pos, vel, FortuneStarLife, scale, twinkleSpeed, driftAngle, color));
        }

        private void SpawnFateThread(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 start = center + SpawnOffset + Main.rand.NextVector2Circular(1f, 1f);
            Vector2 end = start + Main.rand.NextVector2Circular(FateThreadSpread, FateThreadSpread);
            Vector2 startVel = -velocity * InertiaFactor * 0.2f + Main.rand.NextVector2Circular(0.3f, 0.3f);
            Vector2 endVel = -velocity * InertiaFactor * 0.2f + Main.rand.NextVector2Circular(0.3f, 0.3f);
            float width = FateThreadWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float curlAmount = FateThreadCurlAmount * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = FateThreadColor * Main.rand.NextFloat(0.6f, 1f);
            fateThreads.Add(new FateThreadParticle(start, end, startVel, endVel, FateThreadLife, width, curlAmount, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null) _ghostTrail.Draw(sb);

            if (fateThreads.Count > 0 && _threadTex != null)
            {
                Vector2 threadOrigin = new Vector2(0f, _threadTex.Height * 0.5f);
                foreach (var t in fateThreads.OrderBy(x => x.Life))
                {
                    Color drawColor = t.Color * t.Alpha;
                    Vector2 dir = t.EndPos - t.StartPos;
                    float length = dir.Length();
                    if (length < 1f) continue;
                    float rotation = dir.ToRotation();
                    Vector2 mid = (t.StartPos + t.EndPos) * 0.5f;
                    Vector2 curlOffset = new Vector2(0f, MathF.Sin(t.CurlPhase) * t.CurlAmount);
                    Vector2 drawStart = t.StartPos - Main.screenPosition + curlOffset * 0.3f;
                    Vector2 drawEnd = t.EndPos - Main.screenPosition + curlOffset * 0.7f;
                    Vector2 drawDir = drawEnd - drawStart;
                    float drawLength = drawDir.Length();
                    if (drawLength < 1f) continue;
                    float drawRot = drawDir.ToRotation();
                    Vector2 scale = new Vector2(drawLength / _threadTex.Width, t.CurrentWidth);
                    sb.Draw(_threadTex, drawStart, null, drawColor, drawRot, threadOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (luckyClovers.Count > 0 && _cloverTex != null)
            {
                Vector2 cloverOrigin = _cloverTex.Size() * 0.5f;
                foreach (var c in luckyClovers.OrderBy(x => x.Life))
                {
                    Color drawColor = c.Color * c.Alpha;
                    Vector2 pos = c.Position - Main.screenPosition;
                    sb.Draw(_cloverTex, pos, null, drawColor, c.Rotation, cloverOrigin, c.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (fortuneStars.Count > 0 && _starTex != null)
            {
                Vector2 starOrigin = _starTex.Size() * 0.5f;
                foreach (var s in fortuneStars.OrderBy(x => x.Life))
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    sb.Draw(_starTex, pos, null, drawColor, s.Rotation, starOrigin, s.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            luckyClovers.Clear(); fortuneStars.Clear(); fateThreads.Clear();
            _ghostTrail?.Clear();
        }
    }
}