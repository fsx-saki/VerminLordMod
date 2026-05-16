using Microsoft.Xna.Framework;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    public class WaterTrailBehavior : ElementalTrailBehavior<WaterTrail>
    {
        public override string Name => "WaterTrail";

        public int MaxFragments { get; set; } = 80;
        public int ParticleLife { get; set; } = 25;
        public int SpawnInterval { get; set; } = 1;
        public float SizeMultiplier { get; set; } = 1.0f;
        public float SpawnChance { get; set; } = 0.8f;
        public float SplashSpeed { get; set; } = 4f;
        public float SplashAngle { get; set; } = 1.5f;
        public float InertiaFactor { get; set; } = 0.4f;
        public float RandomSpread { get; set; } = 2f;
        public Color ColorStart { get; set; } = new Color(30, 100, 200, 200);
        public Color ColorEnd { get; set; } = new Color(60, 150, 255, 0);
        public bool AdaptiveDensity { get; set; } = true;
        public float AdaptiveSpeedThreshold { get; set; } = 3f;
        public float AdaptiveDensityFactor { get; set; } = 4f;
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;
        public float Gravity { get; set; } = 0.12f;
        public float AirResistance { get; set; } = 0.96f;
        public float BubbleChance { get; set; } = 0.15f;
        public float BubbleSizeMultiplier { get; set; } = 1.8f;
        public bool AdaptiveLife { get; set; } = true;
        public float AdaptiveTargetLength { get; set; } = 60f;
        public float SpeedLifeExponent { get; set; } = 0.35f;
        public int MinParticleLife { get; set; } = 8;

        protected override void ConfigureTrail(WaterTrail trail)
        {
            trail.MaxFragments = MaxFragments;
            trail.ParticleLife = ParticleLife;
            trail.SpawnInterval = SpawnInterval;
            trail.SizeMultiplier = SizeMultiplier;
            trail.SpawnChance = SpawnChance;
            trail.SplashSpeed = SplashSpeed;
            trail.SplashAngle = SplashAngle;
            trail.InertiaFactor = InertiaFactor;
            trail.RandomSpread = RandomSpread;
            trail.ColorStart = ColorStart;
            trail.ColorEnd = ColorEnd;
            trail.AdaptiveDensity = AdaptiveDensity;
            trail.AdaptiveSpeedThreshold = AdaptiveSpeedThreshold;
            trail.AdaptiveDensityFactor = AdaptiveDensityFactor;
            trail.SpawnOffset = SpawnOffset;
            trail.Gravity = Gravity;
            trail.AirResistance = AirResistance;
            trail.BubbleChance = BubbleChance;
            trail.BubbleSizeMultiplier = BubbleSizeMultiplier;
            trail.AdaptiveLife = AdaptiveLife;
            trail.AdaptiveTargetLength = AdaptiveTargetLength;
            trail.SpeedLifeExponent = SpeedLifeExponent;
            trail.MinParticleLife = MinParticleLife;
        }
    }
}
