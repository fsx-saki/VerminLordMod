using System;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.Systems
{
    [Obsolete("SeasonSystem has been merged into WeatherSystem. Use WeatherSystem.Instance instead.")]
    public class SeasonSystem : ModSystem
    {
        public static SeasonSystem Instance => ModContent.GetInstance<SeasonSystem>();

        public Season CurrentSeason => WeatherSystem.Instance?.CurrentSeason ?? Season.Spring;
        public CelestialPhase CurrentMoonPhase => WeatherSystem.Instance?.CurrentMoonPhase ?? CelestialPhase.NewMoon;
        public int DayInSeason => WeatherSystem.Instance?.DayInSeason ?? 0;

        public const int DAYS_PER_SEASON = 10;

        public SeasonModifier GetCurrentModifiers() => WeatherSystem.Instance?.GetCurrentSeasonModifiers() ?? new SeasonModifier();

        public float GetCultivationSpeed() => WeatherSystem.Instance?.GetCultivationBonus() + 1f ?? 1f;

        public float GetPlantGrowthSpeed() => WeatherSystem.Instance?.GetPlantGrowthBonus() + 1f ?? 1f;

        public float GetBeastWaveChance() => WeatherSystem.Instance?.GetBeastWaveChance() ?? 1f;

        public float GetGuLoyaltyDecayRate() => WeatherSystem.Instance?.GetGuLoyaltyDecayRate() ?? 1f;
    }
}
