using Microsoft.Xna.Framework;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class FireTrailBehavior : ElementalTrailBehavior<FireTrail>
    {
        public override string Name => "FireTrail";

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

        protected override void ConfigureTrail(FireTrail trail)
        {
            trail.EnableGhostTrail = EnableGhostTrail;
            trail.GhostMaxPositions = GhostMaxPositions;
            trail.GhostRecordInterval = GhostRecordInterval;
            trail.GhostWidthScale = GhostWidthScale;
            trail.GhostLengthScale = GhostLengthScale;
            trail.GhostAlpha = GhostAlpha;
            trail.GhostColor = GhostColor;

            trail.MaxTongues = MaxTongues;
            trail.TongueLife = TongueLife;
            trail.TongueScale = TongueScale;
            trail.TongueLength = TongueLength;
            trail.TongueSpawnInterval = TongueSpawnInterval;
            trail.TongueSwaySpeed = TongueSwaySpeed;
            trail.TongueSwayAmp = TongueSwayAmp;
            trail.TongueRiseSpeed = TongueRiseSpeed;
            trail.TongueSpread = TongueSpread;
            trail.TongueColor = TongueColor;

            trail.MaxEmbers = MaxEmbers;
            trail.EmberLife = EmberLife;
            trail.EmberSize = EmberSize;
            trail.EmberSpawnChance = EmberSpawnChance;
            trail.EmberRiseSpeed = EmberRiseSpeed;
            trail.EmberDriftSpeed = EmberDriftSpeed;
            trail.EmberColor = EmberColor;

            trail.MaxAshes = MaxAshes;
            trail.AshLife = AshLife;
            trail.AshSize = AshSize;
            trail.AshSpawnChance = AshSpawnChance;
            trail.AshFallSpeed = AshFallSpeed;
            trail.AshDriftSpeed = AshDriftSpeed;
            trail.AshSpinSpeed = AshSpinSpeed;
            trail.AshColor = AshColor;

            trail.InertiaFactor = InertiaFactor;
            trail.RandomSpread = RandomSpread;
            trail.SpawnOffset = SpawnOffset;
        }
    }
}
