using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Trails
{
    public class DevourTrail : ITrail
    {
        public class VortexMawParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float RotSpeed;
            public float SwirlPhase;
            public float SwirlRadius;
            public float SwirlSpeed;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 3f) * (1f - Progress) * (0.6f + 0.4f * MathF.Abs(MathF.Sin(SwirlPhase))));
            public float CurrentScale => Scale * (1f + Progress * 0.5f);

            public VortexMawParticle(Vector2 pos, Vector2 vel, int life, float scale, float rotSpeed, float swirlRadius, float swirlSpeed, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Scale = scale; Rotation = Main.rand.NextFloat(MathHelper.TwoPi); RotSpeed = rotSpeed;
                SwirlPhase = Main.rand.NextFloat(MathHelper.TwoPi); SwirlRadius = swirlRadius; SwirlSpeed = swirlSpeed; Color = color;
            }
        }

        public class AcidDropParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Life;
            public int MaxLife;
            public float DripPhase;
            public float Stretch;
            public float WobblePhase;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 2f) * (1f - Progress) * 0.7f);
            public float CurrentScale => Scale * (1f - Progress * 0.3f);
            public float CurrentStretch => Stretch * (1f + Progress * 0.5f);

            public AcidDropParticle(Vector2 pos, Vector2 vel, int life, float scale, float stretch, Color color)
            {
                Position = pos; Velocity = vel; MaxLife = life; Life = life;
                Scale = scale; DripPhase = Main.rand.NextFloat(MathHelper.TwoPi);
                Stretch = stretch; WobblePhase = Main.rand.NextFloat(MathHelper.TwoPi); Color = color;
            }
        }

        public class AbsorbTendrilParticle
        {
            public Vector2 Position;
            public Vector2 TargetPos;
            public Vector2 Velocity;
            public float Length;
            public float Width;
            public int Life;
            public int MaxLife;
            public float Rotation;
            public float WrigglePhase;
            public float WriggleAmount;
            public float PullStrength;
            public Color Color;

            public float Progress => 1f - (float)Life / MaxLife;
            public float Alpha => MathF.Max(0f, MathF.Min(1f, Progress * 4f) * (1f - Progress) * 0.5f);
            public float CurrentLength => Length * (1f - Progress * 0.4f);
            public float CurrentWidth => Width * (1f - Progress * 0.6f);

            public AbsorbTendrilParticle(Vector2 pos, Vector2 target, Vector2 vel, int life, float length, float width, float wriggleAmount, float pullStrength, Color color)
            {
                Position = pos; TargetPos = target; Velocity = vel; MaxLife = life; Life = life;
                Length = length; Width = width; Rotation = (target - pos).ToRotation();
                WrigglePhase = Main.rand.NextFloat(MathHelper.TwoPi); WriggleAmount = wriggleAmount;
                PullStrength = pullStrength; Color = color;
            }
        }

        public string Name { get; set; } = "DevourTrail";
        public BlendState BlendMode => BlendState.Additive;

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 6;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.18f;
        public float GhostLengthScale { get; set; } = 1.4f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(160, 80, 180, 130);

        public int MaxVortexMaws { get; set; } = 5;
        public int VortexMawLife { get; set; } = 35;
        public float VortexMawSize { get; set; } = 0.6f;
        public float VortexMawSpawnChance { get; set; } = 0.04f;
        public float VortexMawRotSpeed { get; set; } = 0.1f;
        public float VortexMawSwirlRadius { get; set; } = 4f;
        public float VortexMawSwirlSpeed { get; set; } = 0.08f;
        public float VortexMawDriftSpeed { get; set; } = 0.2f;
        public Color VortexMawColor { get; set; } = new Color(180, 80, 200, 220);

        public int MaxAcidDrops { get; set; } = 10;
        public int AcidDropLife { get; set; } = 28;
        public float AcidDropSize { get; set; } = 0.3f;
        public float AcidDropSpawnChance { get; set; } = 0.08f;
        public float AcidDropStretch { get; set; } = 1.5f;
        public float AcidDropGravity { get; set; } = 0.15f;
        public float AcidDropDriftSpeed { get; set; } = 0.4f;
        public Color AcidDropColor { get; set; } = new Color(100, 220, 80, 200);

        public int MaxAbsorbTendrils { get; set; } = 6;
        public int AbsorbTendrilLife { get; set; } = 22;
        public float AbsorbTendrilLength { get; set; } = 30f;
        public float AbsorbTendrilWidth { get; set; } = 0.2f;
        public float AbsorbTendrilSpawnChance { get; set; } = 0.06f;
        public float AbsorbTendrilWriggleAmount { get; set; } = 5f;
        public float AbsorbTendrilPullStrength { get; set; } = 0.05f;
        public float AbsorbTendrilSpread { get; set; } = 12f;
        public Color AbsorbTendrilColor { get; set; } = new Color(200, 120, 220, 180);

        public float InertiaFactor { get; set; } = 0.15f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        private List<VortexMawParticle> vortexMaws = new();
        private List<AcidDropParticle> acidDrops = new();
        private List<AbsorbTendrilParticle> absorbTendrils = new();
        private GhostTrail _ghostTrail;
        private Texture2D _mawTex, _acidTex, _tendrilTex, _ghostTex;

        public bool HasContent => vortexMaws.Count > 0 || acidDrops.Count > 0 || absorbTendrils.Count > 0 || (_ghostTrail?.HasContent ?? false);

        private void EnsureTextures()
        {
            if (_mawTex != null) return;
            _mawTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/DevourTrail/DevourTrailMaw").Value;
            _acidTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/DevourTrail/DevourTrailAcid").Value;
            _tendrilTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/DevourTrail/DevourTrailTendril").Value;
            _ghostTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Trails/DevourTrail/DevourTrailGhost").Value;
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

            if (vortexMaws.Count < MaxVortexMaws && Main.rand.NextFloat() < VortexMawSpawnChance)
                SpawnVortexMaw(center, velocity, moveDir);
            if (acidDrops.Count < MaxAcidDrops && Main.rand.NextFloat() < AcidDropSpawnChance)
                SpawnAcidDrop(center, velocity, moveDir);
            if (absorbTendrils.Count < MaxAbsorbTendrils && Main.rand.NextFloat() < AbsorbTendrilSpawnChance)
                SpawnAbsorbTendril(center, velocity, moveDir);

            for (int i = vortexMaws.Count - 1; i >= 0; i--)
            {
                var v = vortexMaws[i];
                v.Rotation += v.RotSpeed; v.SwirlPhase += v.SwirlSpeed;
                Vector2 swirlOffset = new Vector2(
                    MathF.Cos(v.SwirlPhase) * v.SwirlRadius,
                    MathF.Sin(v.SwirlPhase * 0.7f) * v.SwirlRadius * 0.5f
                );
                v.Velocity *= 0.95f; v.Position += v.Velocity + swirlOffset * 0.05f; v.Life--;
                if (v.Life <= 0) vortexMaws.RemoveAt(i);
            }
            for (int i = acidDrops.Count - 1; i >= 0; i--)
            {
                var a = acidDrops[i];
                a.DripPhase += 0.1f; a.WobblePhase += 0.15f;
                a.Velocity.Y += AcidDropGravity;
                a.Velocity.X += MathF.Sin(a.WobblePhase) * 0.1f;
                a.Velocity *= 0.98f; a.Position += a.Velocity; a.Life--;
                if (a.Life <= 0) acidDrops.RemoveAt(i);
            }
            for (int i = absorbTendrils.Count - 1; i >= 0; i--)
            {
                var t = absorbTendrils[i];
                t.WrigglePhase += 0.12f;
                Vector2 toTarget = t.TargetPos - t.Position;
                float dist = toTarget.Length();
                if (dist > 1f)
                {
                    t.Velocity += Vector2.Normalize(toTarget) * t.PullStrength;
                    t.Rotation = toTarget.ToRotation();
                }
                t.Velocity *= 0.96f; t.Position += t.Velocity; t.Life--;
                if (t.Life <= 0) absorbTendrils.RemoveAt(i);
            }
        }

        private void SpawnVortexMaw(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(3f, 3f);
            Vector2 vel = -velocity * InertiaFactor * 0.4f + Main.rand.NextVector2Circular(VortexMawDriftSpeed, VortexMawDriftSpeed);
            float scale = Main.rand.NextFloat(0.6f, 1.3f) * VortexMawSize;
            float rotSpeed = Main.rand.NextFloat(-VortexMawRotSpeed, VortexMawRotSpeed);
            float swirlRadius = VortexMawSwirlRadius * Main.rand.NextFloat(0.7f, 1.3f);
            float swirlSpeed = VortexMawSwirlSpeed * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = VortexMawColor * Main.rand.NextFloat(0.6f, 1f);
            vortexMaws.Add(new VortexMawParticle(pos, vel, VortexMawLife, scale, rotSpeed, swirlRadius, swirlSpeed, color));
        }

        private void SpawnAcidDrop(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(2f, 2f);
            Vector2 vel = -velocity * InertiaFactor * 0.3f + Main.rand.NextVector2Circular(AcidDropDriftSpeed, AcidDropDriftSpeed);
            float scale = Main.rand.NextFloat(0.5f, 1.4f) * AcidDropSize;
            float stretch = AcidDropStretch * Main.rand.NextFloat(0.7f, 1.3f);
            Color color = AcidDropColor * Main.rand.NextFloat(0.6f, 1f);
            acidDrops.Add(new AcidDropParticle(pos, vel, AcidDropLife, scale, stretch, color));
        }

        private void SpawnAbsorbTendril(Vector2 center, Vector2 velocity, Vector2 moveDir)
        {
            Vector2 pos = center + SpawnOffset + Main.rand.NextVector2Circular(1f, 1f);
            Vector2 target = pos + Main.rand.NextVector2Circular(AbsorbTendrilSpread, AbsorbTendrilSpread);
            Vector2 vel = -velocity * InertiaFactor * 0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            float length = AbsorbTendrilLength * Main.rand.NextFloat(0.7f, 1.3f);
            float width = AbsorbTendrilWidth * Main.rand.NextFloat(0.7f, 1.3f);
            float wriggleAmount = AbsorbTendrilWriggleAmount * Main.rand.NextFloat(0.8f, 1.2f);
            float pullStrength = AbsorbTendrilPullStrength * Main.rand.NextFloat(0.8f, 1.2f);
            Color color = AbsorbTendrilColor * Main.rand.NextFloat(0.6f, 1f);
            absorbTendrils.Add(new AbsorbTendrilParticle(pos, target, vel, AbsorbTendrilLife, length, width, wriggleAmount, pullStrength, color));
        }

        public void Draw(SpriteBatch sb)
        {
            if (_ghostTrail != null) _ghostTrail.Draw(sb);

            if (absorbTendrils.Count > 0 && _tendrilTex != null)
            {
                Vector2 tendrilOrigin = new Vector2(0f, _tendrilTex.Height * 0.5f);
                foreach (var t in absorbTendrils.OrderBy(x => x.Life))
                {
                    Color drawColor = t.Color * t.Alpha;
                    Vector2 pos = t.Position - Main.screenPosition;
                    Vector2 wriggleOffset = new Vector2(0f, MathF.Sin(t.WrigglePhase) * t.WriggleAmount);
                    Vector2 drawPos = pos + wriggleOffset * 0.3f;
                    Vector2 scale = new Vector2(t.CurrentLength / _tendrilTex.Width, t.CurrentWidth);
                    sb.Draw(_tendrilTex, drawPos, null, drawColor, t.Rotation, tendrilOrigin, scale, SpriteEffects.None, 0);
                }
            }

            if (vortexMaws.Count > 0 && _mawTex != null)
            {
                Vector2 mawOrigin = _mawTex.Size() * 0.5f;
                foreach (var v in vortexMaws.OrderBy(x => x.Life))
                {
                    Color drawColor = v.Color * v.Alpha;
                    Vector2 pos = v.Position - Main.screenPosition;
                    sb.Draw(_mawTex, pos, null, drawColor, v.Rotation, mawOrigin, v.CurrentScale, SpriteEffects.None, 0);
                }
            }

            if (acidDrops.Count > 0 && _acidTex != null)
            {
                Vector2 acidOrigin = _acidTex.Size() * 0.5f;
                foreach (var a in acidDrops.OrderBy(x => x.Life))
                {
                    Color drawColor = a.Color * a.Alpha;
                    Vector2 pos = a.Position - Main.screenPosition;
                    Vector2 scale = new Vector2(a.CurrentScale, a.CurrentScale * a.CurrentStretch);
                    sb.Draw(_acidTex, pos, null, drawColor, MathHelper.PiOver2, acidOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public void Clear()
        {
            vortexMaws.Clear(); acidDrops.Clear(); absorbTendrils.Clear();
            _ghostTrail?.Clear();
        }
    }
}