using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class SkyTrailBehavior : IBulletBehavior
    {
        public string Name => "SkyTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public SkyTrail Trail { get; private set; }

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 12;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.2f;
        public float GhostLengthScale { get; set; } = 1.6f;
        public float GhostAlpha { get; set; } = 0.3f;
        public Color GhostColor { get; set; } = new Color(100, 140, 220, 140);

        public int MaxCelestialArcs { get; set; } = 6;
        public int CelestialArcLife { get; set; } = 55;
        public float CelestialArcScale { get; set; } = 0.5f;
        public float CelestialArcSpan { get; set; } = MathHelper.Pi * 0.6f;
        public float CelestialArcSpawnChance { get; set; } = 0.03f;
        public float CelestialArcGrowSpeed { get; set; } = 2f;
        public float CelestialArcDriftSpeed { get; set; } = 0.04f;
        public Color CelestialArcColor { get; set; } = new Color(80, 140, 240, 200);

        public int MaxAuroraBands { get; set; } = 10;
        public int AuroraBandLife { get; set; } = 45;
        public float AuroraBandSize { get; set; } = 0.3f;
        public float AuroraBandLength { get; set; } = 28f;
        public float AuroraBandSpawnChance { get; set; } = 0.07f;
        public float AuroraBandWaveSpeed { get; set; } = 0.1f;
        public float AuroraBandWaveAmplitude { get; set; } = 0.3f;
        public float AuroraBandHueSpeed { get; set; } = 1.5f;
        public float AuroraBandDriftSpeed { get; set; } = 0.06f;
        public Color AuroraBandColor { get; set; } = new Color(60, 180, 220, 200);

        public int MaxZenithMarks { get; set; } = 12;
        public int ZenithMarkLife { get; set; } = 25;
        public float ZenithMarkSize { get; set; } = 0.4f;
        public float ZenithMarkSpawnChance { get; set; } = 0.1f;
        public float ZenithMarkSpinSpeed { get; set; } = 0.04f;
        public float ZenithMarkCrossLength { get; set; } = 14f;
        public float ZenithMarkDriftSpeed { get; set; } = 0.12f;
        public Color ZenithMarkColor { get; set; } = new Color(160, 200, 255, 240);

        public float InertiaFactor { get; set; } = 0.12f;
        public float RandomSpread { get; set; } = 3f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        public bool AutoDraw { get; set; } = true;
        public bool SuppressDefaultDraw { get; set; } = false;

        public SkyTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new SkyTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxCelestialArcs = MaxCelestialArcs,
                CelestialArcLife = CelestialArcLife,
                CelestialArcScale = CelestialArcScale,
                CelestialArcSpan = CelestialArcSpan,
                CelestialArcSpawnChance = CelestialArcSpawnChance,
                CelestialArcGrowSpeed = CelestialArcGrowSpeed,
                CelestialArcDriftSpeed = CelestialArcDriftSpeed,
                CelestialArcColor = CelestialArcColor,

                MaxAuroraBands = MaxAuroraBands,
                AuroraBandLife = AuroraBandLife,
                AuroraBandSize = AuroraBandSize,
                AuroraBandLength = AuroraBandLength,
                AuroraBandSpawnChance = AuroraBandSpawnChance,
                AuroraBandWaveSpeed = AuroraBandWaveSpeed,
                AuroraBandWaveAmplitude = AuroraBandWaveAmplitude,
                AuroraBandHueSpeed = AuroraBandHueSpeed,
                AuroraBandDriftSpeed = AuroraBandDriftSpeed,
                AuroraBandColor = AuroraBandColor,

                MaxZenithMarks = MaxZenithMarks,
                ZenithMarkLife = ZenithMarkLife,
                ZenithMarkSize = ZenithMarkSize,
                ZenithMarkSpawnChance = ZenithMarkSpawnChance,
                ZenithMarkSpinSpeed = ZenithMarkSpinSpeed,
                ZenithMarkCrossLength = ZenithMarkCrossLength,
                ZenithMarkDriftSpeed = ZenithMarkDriftSpeed,
                ZenithMarkColor = ZenithMarkColor,

                InertiaFactor = InertiaFactor,
                RandomSpread = RandomSpread,
                SpawnOffset = SpawnOffset,
            };

            TrailManager.Add(Trail);
        }

        public void Update(Projectile projectile)
        {
            TrailManager.Update(projectile.Center, projectile.velocity);
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            TrailManager.Clear();
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            if (AutoDraw)
                TrailManager.Draw(spriteBatch);
            return !SuppressDefaultDraw;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
