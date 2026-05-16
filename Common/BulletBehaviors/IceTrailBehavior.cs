using Microsoft.Xna.Framework;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class IceTrailBehavior : ElementalTrailBehavior<IceTrail>
    {
        public override string Name => "IceTrail";

        public bool EnableGhostTrail { get; set; } = true;
        public int GhostMaxPositions { get; set; } = 8;
        public int GhostRecordInterval { get; set; } = 2;
        public float GhostWidthScale { get; set; } = 0.3f;
        public float GhostLengthScale { get; set; } = 1.5f;
        public float GhostAlpha { get; set; } = 0.6f;
        public Color GhostColor { get; set; } = new Color(120, 200, 255, 180);

        public int MaxStars { get; set; } = 40;
        public int StarLife { get; set; } = 30;
        public float StarSize { get; set; } = 0.4f;
        public int StarSpawnInterval { get; set; } = 3;
        public Color StarColor { get; set; } = new Color(160, 220, 255, 220);
        public float StarRotSpeed { get; set; } = 0.01f;
        public float StarSpreadWidth { get; set; } = 8f;

        public int MaxSnowflakes { get; set; } = 120;
        public int SnowflakeLife { get; set; } = 25;
        public float SnowflakeSize { get; set; } = 0.2f;
        public float SnowflakeGravity { get; set; } = 0.1f;
        public float SnowflakeAirResistance { get; set; } = 0.98f;
        public Color SnowflakeColor { get; set; } = new Color(200, 240, 255, 180);
        public int SnowflakeClusterSize { get; set; } = 5;
        public float SnowflakeSplashSpeed { get; set; } = 3f;
        public float SnowflakeSplashAngle { get; set; } = 1.2f;
        public float SnowflakeSpawnChance { get; set; } = 0.7f;
        public float InertiaFactor { get; set; } = 0.3f;
        public float RandomSpread { get; set; } = 6f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        protected override void ConfigureTrail(IceTrail trail)
        {
            trail.EnableGhostTrail = EnableGhostTrail;
            trail.GhostMaxPositions = GhostMaxPositions;
            trail.GhostRecordInterval = GhostRecordInterval;
            trail.GhostWidthScale = GhostWidthScale;
            trail.GhostLengthScale = GhostLengthScale;
            trail.GhostAlpha = GhostAlpha;
            trail.GhostColor = GhostColor;

            trail.MaxStars = MaxStars;
            trail.StarLife = StarLife;
            trail.StarSize = StarSize;
            trail.StarSpawnInterval = StarSpawnInterval;
            trail.StarColor = StarColor;
            trail.StarRotSpeed = StarRotSpeed;
            trail.StarSpreadWidth = StarSpreadWidth;

            trail.MaxSnowflakes = MaxSnowflakes;
            trail.SnowflakeLife = SnowflakeLife;
            trail.SnowflakeSize = SnowflakeSize;
            trail.SnowflakeGravity = SnowflakeGravity;
            trail.SnowflakeAirResistance = SnowflakeAirResistance;
            trail.SnowflakeColor = SnowflakeColor;
            trail.SnowflakeClusterSize = SnowflakeClusterSize;
            trail.SnowflakeSplashSpeed = SnowflakeSplashSpeed;
            trail.SnowflakeSplashAngle = SnowflakeSplashAngle;
            trail.SnowflakeSpawnChance = SnowflakeSpawnChance;
            trail.InertiaFactor = InertiaFactor;
            trail.RandomSpread = RandomSpread;
            trail.SpawnOffset = SpawnOffset;
        }
    }
}
