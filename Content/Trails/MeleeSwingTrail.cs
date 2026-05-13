using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class MeleeSwingTrail : ITrail
    {
        public class SwingArcParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float ArcAngle;
            public float ArcLength;
            public float Width;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float CurlPhase;
            public float CurlAmount;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 4f) * (1f - Progress) * 0.8f);
            public float CurrentWidth => Width * (1f - Progress * 0.5f);
            public float CurrentArcLength => ArcLength * (1f - Progress * 0.3f);

            public SwingArcParticle(Vector2 pos, Vector2 vel, int life, float arcAngle, float arcLength, float width, float curlAmount, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                ArcAngle = arcAngle; ArcLength = arcLength; Width = width;
                CurlPhase = Main.rand.NextFloat(MathHelper.TwoPi); CurlAmount = curlAmount;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi); Color = color;
            }
        }

        public class StabImpactParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float SpreadAngle;
            public float Stretch;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 6f) * (1f - Progress) * 0.9f);
            public float CurrentScale => Scale * (1f + Progress * 0.5f);
            public float CurrentStretch => Stretch * (1f - Progress * 0.3f);

            public StabImpactParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotation, float spreadAngle, float stretch, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Scale = scale; Rotation = rotation; SpreadAngle = spreadAngle; Stretch = stretch; Color = color;
            }
        }

        public class SmashRingParticle
        {
            public Vector2 Position;
            public float Radius;
            public float MaxRadius;
            public float Width;
            public int Life;
            public int MaxLife;
            public float ExpandSpeed;
            public float PulsePhase;
            public int CrackCount;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 3f) * (1f - Progress) * 0.6f);
            public float CurrentRadius => Radius + (MaxRadius - Radius) * MathF.Min(1f, Progress * ExpandSpeed);
            public float CurrentWidth => Width * (1f - Progress * 0.4f);

            public SmashRingParticle(Vector2 pos, int life, float radius, float maxRadius, float width, float expandSpeed, int crackCount, Color color)
            {
                Position = pos; MaxLife = life; Life = life; Radius = radius; MaxRadius = maxRadius;
                Width = width; ExpandSpeed = expandSpeed; PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                CrackCount = crackCount; Color = color;
            }
        }

        public string Name { get; set; } = "MeleeSwingTrail";
        public BlendState BlendMode => BlendState.Additive;

        public int MaxSwingArcs { get; set; } = 6;
        public int SwingArcLife { get; set; } = 15;
        public float SwingArcLength { get; set; } = 35f;
        public float SwingArcWidth { get; set; } = 0.3f;
        public float SwingArcSpawnChance { get; set; } = 0.15f;
        public float SwingArcCurlAmount { get; set; } = 3f;
        public Color SwingArcColor { get; set; } = new Color(200, 200, 220, 200);

        public int MaxStabImpacts { get; set; } = 8;
        public int StabImpactLife { get; set; } = 12;
        public float StabImpactSize { get; set; } = 0.5f;
        public float StabImpactSpawnChance { get; set; } = 0.2f;
        public float StabImpactSpread { get; set; } = 0.4f;
        public float StabImpactStretch { get; set; } = 2.5f;
        public Color StabImpactColor { get; set; } = new Color(220, 210, 200, 220);

        public int MaxSmashRings { get; set; } = 3;
        public int SmashRingLife { get; set; } = 30;
        public float SmashRingStartRadius { get; set; } = 2f;
        public float SmashRingEndRadius { get; set; } = 40f;
        public float SmashRingWidth { get; set; } = 0.4f;
        public float SmashRingSpawnChance { get; set; } = 0.02f;
        public float SmashRingExpandSpeed { get; set; } = 1.2f;
        public int SmashRingCrackCount { get; set; } = 6;
        public Color SmashRingColor { get; set; } = new Color(180, 180, 200, 180);

        public float InertiaFactor { get; set; } = 0.1f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<SwingArcParticle> swingArcs = new();
        private List<StabImpactParticle> stabImpacts = new();
        private List<SmashRingParticle> smashRings = new();
        private Texture2D _arcTex, _stabTex, _smashTex;

        public bool HasContent => swingArcs.Count > 0 || stabImpacts.Count > 0 || smashRings.Count > 0;

        private void EnsureTextures()
        {
            if (_arcTex != null) return;
            _arcTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MeleeSwingTrail/MeleeSwingArc").Value;
            _stabTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MeleeSwingTrail/MeleeStabImpact").Value;
            _smashTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/MeleeSwingTrail/MeleeSmashRing").Value;
        }

        public void Update(Vector2 center, Vector2 velocity)
        {
            EnsureTextures();

            Vector2 moveDir = velocity.SafeNormalize(Vector2.UnitX);
            float speed = velocity.Length();

            if (swingArcs.Count < MaxSwingArcs && Main.rand.NextFloat() < SwingArcSpawnChance)
                SpawnSwingArc(center, velocity, moveDir, speed);
            if (stabImpacts.Count < MaxStabImpacts && Main.rand.NextFloat() < StabImpactSpawnChance)
                SpawnStabImpact(center, velocity, moveDir, speed);
            if (smashRings.Count < MaxSmashRings && Main.rand.NextFloat() < SmashRingSpawnChance)
                SpawnSmashRing(center, velocity, moveDir, speed);

            for (int i = swingArcs.Count - 1; i >= 0; i--)
            {
                var s = swingArcs[i];
                s.CurlPhase += 0.15f;
                s.Velocity *= 0.95f; s.Position += s.Velocity; s.Life--;
                if (s.Life <= 0) swingArcs.RemoveAt(i);
            }
            for (int i = stabImpacts.Count - 1; i >= 0; i--)
            {
                var st = stabImpacts[i];
                st.Velocity *= 0.9f; st.Position += st.Velocity; st.Life--;
                if (st.Life <= 0) stabImpacts.RemoveAt(i);
            }
            for (int i = smashRings.Count - 1; i >= 0; i--)
            {
                var sm = smashRings[i];
                sm.PulsePhase += 0.08f;
                sm.Position += (center - sm.Position) * 0.01f; sm.Life--;
                if (sm.Life <= 0) smashRings.RemoveAt(i);
            }
        }

        private void SpawnSwingArc(Vector2 center, Vector2 velocity, Vector2 moveDir, float speed)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(3f, 3f);
            Vector2 vel = -velocity * InertiaFactor * 0.3f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            float arcAngle = Main.rand.NextFloat(-0.8f, 0.8f);
            float arcLength = SwingArcLength * Main.rand.NextFloat(0.7f, 1.3f);
            float width = SwingArcWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float curlAmount = SwingArcCurlAmount * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = SwingArcColor * Main.rand.NextFloat(0.6f, 1f);
            swingArcs.Add(new SwingArcParticle(pos, vel, SwingArcLife, arcAngle, arcLength, width, curlAmount, color));
        }

        private void SpawnStabImpact(Vector2 center, Vector2 velocity, Vector2 moveDir, float speed)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(2f, 2f);
            float angle = velocity.ToRotation() + Main.rand.NextFloat(-StabImpactSpread, StabImpactSpread);
            Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f);
            float scale = Main.rand.NextFloat(0.6f, 1.3f) * StabImpactSize;
            float stretch = StabImpactStretch * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = StabImpactColor * Main.rand.NextFloat(0.6f, 1f);
            stabImpacts.Add(new StabImpactParticle(pos, vel, StabImpactLife, scale, angle, StabImpactSpread, stretch, color));
        }

        private void SpawnSmashRing(Vector2 center, Vector2 velocity, Vector2 moveDir, float speed)
        {
            Vector2 pos = center + SpawnOffset;
            float maxRadius = SmashRingEndRadius * Main.rand.NextFloat(0.7f, 1.3f);
            float width = SmashRingWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float expandSpeed = SmashRingExpandSpeed * Main.rand.NextFloat(0.8f, 1.2f);
            int cracks = SmashRingCrackCount + Main.rand.Next(-1, 2);
            Color color = SmashRingColor * Main.rand.NextFloat(0.5f, 1f);
            smashRings.Add(new SmashRingParticle(pos, SmashRingLife, SmashRingStartRadius, maxRadius, width, expandSpeed, cracks, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (smashRings.Count > 0 && _smashTex != null)
            {
                Vector2 smashOrigin = _smashTex.Size() * 0.5f;
                foreach (var sm in smashRings.OrderBy(x => x.Life))
                {
                    Color drawColor = sm.Color * sm.Alpha;
                    Vector2 pos = sm.Position - Main.screenPosition;
                    float scaleX = sm.CurrentRadius / (_smashTex.Width * 0.5f);
                    float scaleY = sm.CurrentWidth;
                    sb.Draw(_smashTex, pos, null, drawColor, 0f, smashOrigin, new Vector2(scaleX, scaleY), SpriteEffects.None, 0);
                }
            }

            if (stabImpacts.Count > 0 && _stabTex != null)
            {
                Vector2 stabOrigin = new Vector2(0f, _stabTex.Height * 0.5f);
                foreach (var st in stabImpacts.OrderBy(x => x.Life))
                {
                    Color drawColor = st.Color * st.Alpha;
                    Vector2 pos = st.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(st.CurrentScale * st.CurrentStretch, st.CurrentScale);
                    sb.Draw(_stabTex, pos, null, drawColor, st.Rotation, stabOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (swingArcs.Count > 0 && _arcTex != null)
            {
                Vector2 arcOrigin = new Vector2(0f, _arcTex.Height * 0.5f);
                foreach (var s in swingArcs.OrderBy(x => x.Life))
                {
                    Color drawColor = s.Color * s.Alpha;
                    Vector2 pos = s.Position - Main.screenPosition;
                    float curlOffset = MathF.Sin(s.CurlPhase) * s.CurlAmount;
                    Vector2 curlPos = pos + new Vector2(0f, curlOffset) * 0.3f;
                    float drawRotation = s.Rotation + s.ArcAngle;
                    Vector2 scale = new Vector2(s.CurrentArcLength / _arcTex.Width, s.CurrentWidth);
                    sb.Draw(_arcTex, curlPos, null, drawColor, drawRotation, arcOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            swingArcs.Clear(); stabImpacts.Clear(); smashRings.Clear();
        }
    }
}