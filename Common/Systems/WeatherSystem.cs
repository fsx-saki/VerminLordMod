using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;

namespace VerminLordMod.Common.Systems
{
    public enum WeatherCondition
    {
        Clear,
        Cloudy,
        Rain,
        HeavyRain,
        Thunderstorm,
        Fog,
        Sandstorm,
        Snow,
        Blizzard,
        BloodMoon,
        SpiritTide,
        QiSurge,
        Eclipse
    }

    public enum WeatherIntensity
    {
        Light,
        Moderate,
        Heavy,
        Extreme
    }

    public enum Season
    {
        Spring = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 3,
    }

    public enum CelestialPhase
    {
        NewMoon,
        WaxingCrescent,
        FirstQuarter,
        WaxingGibbous,
        FullMoon,
        WaningGibbous,
        LastQuarter,
        WaningCrescent,
    }

    public class WeatherState
    {
        public WeatherCondition Type;
        public WeatherIntensity Intensity;
        public int DurationTicks;
        public int ElapsedTicks;
        public float TransitionProgress;
        public bool IsTransitioning;
        public WeatherCondition NextType;
        public float WindSpeed;
        public float Temperature;
        public float Humidity;
        public float QiDensity;
    }

    public class SeasonModifier
    {
        public float CultivationSpeedMult = 1f;
        public float PlantGrowthMult = 1f;
        public float ResourceYieldMult = 1f;
        public float GuLoyaltyDecayMult = 1f;
        public float BeastWaveChanceMult = 1f;
        public float MerchantVisitMult = 1f;
        public Dictionary<int, float> ElementDamageBonus = new();
    }

    public class WeatherSystem : ModSystem
    {
        public static WeatherSystem Instance => ModContent.GetInstance<WeatherSystem>();

        public WeatherState CurrentWeather;
        public WeatherState PreviousWeather;
        public int WeatherChangeTimer;
        public const int MIN_WEATHER_DURATION = 18000;
        public const int MAX_WEATHER_DURATION = 72000;
        public const int TRANSITION_DURATION = 300;
        public const int DAYS_PER_SEASON = 10;

        public Season CurrentSeason = Season.Spring;
        public CelestialPhase CurrentMoonPhase = CelestialPhase.NewMoon;
        public int DayInSeason = 0;

        private readonly Dictionary<Season, SeasonModifier> _seasonModifiers = new();
        private readonly Dictionary<CelestialPhase, float> _moonCultivationBonus = new();

        private int _lastDay = -1;

        public override void OnWorldLoad()
        {
            CurrentWeather = new WeatherState
            {
                Type = WeatherCondition.Clear,
                Intensity = WeatherIntensity.Light,
                DurationTicks = Main.rand.Next(MIN_WEATHER_DURATION, MAX_WEATHER_DURATION),
                Temperature = 20f,
                Humidity = 0.5f,
                QiDensity = 0.1f,
            };
            WeatherChangeTimer = CurrentWeather.DurationTicks;
            CurrentSeason = Season.Spring;
            CurrentMoonPhase = CelestialPhase.NewMoon;
            DayInSeason = 0;
            InitializeModifiers();
        }

        private void InitializeModifiers()
        {
            _seasonModifiers.Clear();
            _seasonModifiers[Season.Spring] = new SeasonModifier
            {
                PlantGrowthMult = 1.5f,
                CultivationSpeedMult = 1.0f,
                ResourceYieldMult = 1.0f,
                GuLoyaltyDecayMult = 0.8f,
                BeastWaveChanceMult = 0.8f,
                MerchantVisitMult = 1.0f,
            };
            _seasonModifiers[Season.Summer] = new SeasonModifier
            {
                PlantGrowthMult = 1.0f,
                CultivationSpeedMult = 1.3f,
                ResourceYieldMult = 1.0f,
                GuLoyaltyDecayMult = 1.0f,
                BeastWaveChanceMult = 1.5f,
                MerchantVisitMult = 0.8f,
            };
            _seasonModifiers[Season.Autumn] = new SeasonModifier
            {
                PlantGrowthMult = 1.0f,
                CultivationSpeedMult = 1.0f,
                ResourceYieldMult = 2.0f,
                GuLoyaltyDecayMult = 1.0f,
                BeastWaveChanceMult = 1.0f,
                MerchantVisitMult = 1.5f,
            };
            _seasonModifiers[Season.Winter] = new SeasonModifier
            {
                PlantGrowthMult = 0.2f,
                CultivationSpeedMult = 0.7f,
                ResourceYieldMult = 0.5f,
                GuLoyaltyDecayMult = 1.5f,
                BeastWaveChanceMult = 0.5f,
                MerchantVisitMult = 0.6f,
            };

            _moonCultivationBonus.Clear();
            _moonCultivationBonus[CelestialPhase.NewMoon] = 0.8f;
            _moonCultivationBonus[CelestialPhase.WaxingCrescent] = 0.9f;
            _moonCultivationBonus[CelestialPhase.FirstQuarter] = 1.0f;
            _moonCultivationBonus[CelestialPhase.WaxingGibbous] = 1.1f;
            _moonCultivationBonus[CelestialPhase.FullMoon] = 1.3f;
            _moonCultivationBonus[CelestialPhase.WaningGibbous] = 1.1f;
            _moonCultivationBonus[CelestialPhase.LastQuarter] = 1.0f;
            _moonCultivationBonus[CelestialPhase.WaningCrescent] = 0.9f;
        }

        public SeasonModifier GetCurrentSeasonModifiers()
        {
            if (_seasonModifiers.TryGetValue(CurrentSeason, out var mod))
                return mod;
            return new SeasonModifier();
        }

        public override void PostUpdateWorld()
        {
            if (CurrentWeather == null) return;

            if (WorldTimeHelper.IsNewDay(ref _lastDay))
            {
                AdvanceDayCycle();
            }

            CurrentWeather.ElapsedTicks++;
            WeatherChangeTimer--;

            if (CurrentWeather.IsTransitioning)
            {
                CurrentWeather.TransitionProgress += 1f / TRANSITION_DURATION;
                if (CurrentWeather.TransitionProgress >= 1f)
                {
                    PreviousWeather = new WeatherState
                    {
                        Type = CurrentWeather.Type,
                        Intensity = CurrentWeather.Intensity,
                    };
                    CurrentWeather.Type = CurrentWeather.NextType;
                    CurrentWeather.Intensity = RollIntensity(CurrentWeather.NextType);
                    CurrentWeather.IsTransitioning = false;
                    CurrentWeather.TransitionProgress = 0f;
                    CurrentWeather.DurationTicks = Main.rand.Next(MIN_WEATHER_DURATION, MAX_WEATHER_DURATION);
                    CurrentWeather.ElapsedTicks = 0;
                    WeatherChangeTimer = CurrentWeather.DurationTicks;
                    ApplyWeatherEffects(CurrentWeather);
                    EventBus.Publish(new WeatherChangedEvent { NewWeather = CurrentWeather.Type });
                }
            }

            if (WeatherChangeTimer <= 0 && !CurrentWeather.IsTransitioning)
            {
                TriggerWeatherChange();
            }

            UpdateWeatherEffects();
        }

        private void AdvanceDayCycle()
        {
            DayInSeason++;
            if (DayInSeason >= DAYS_PER_SEASON)
            {
                DayInSeason = 0;
                CurrentSeason = (Season)(((int)CurrentSeason + 1) % 4);
                OnSeasonChanged();
            }

            CurrentMoonPhase = (CelestialPhase)(((int)CurrentMoonPhase + 1) % 8);

            ApplyWeatherVisuals();
        }

        private void OnSeasonChanged()
        {
            string seasonName = CurrentSeason switch
            {
                Season.Spring => "春",
                Season.Summer => "夏",
                Season.Autumn => "秋",
                Season.Winter => "冬",
                _ => "?"
            };
            Main.NewText($"季节更替：{seasonName}季来临！", Color.Gold);
        }

        private void ApplyWeatherVisuals()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server || Main.myPlayer < 0) return;

            if (CurrentWeather.Type == WeatherCondition.Rain || CurrentWeather.Type == WeatherCondition.HeavyRain || CurrentWeather.Type == WeatherCondition.Thunderstorm)
            {
                Main.rainTime = System.Math.Max(Main.rainTime, 600);
                Main.maxRaining = CurrentWeather.Type == WeatherCondition.Thunderstorm ? 0.8f : 0.4f;
            }
            if (CurrentWeather.Type == WeatherCondition.BloodMoon)
            {
                Main.bloodMoon = true;
            }
        }

        private void TriggerWeatherChange()
        {
            CurrentWeather.IsTransitioning = true;
            CurrentWeather.TransitionProgress = 0f;
            CurrentWeather.NextType = RollNextWeather();
        }

        private WeatherCondition RollNextWeather()
        {
            float roll = Main.rand.NextFloat();
            bool isBloodMoon = Main.bloodMoon;
            bool isEclipse = Main.eclipse;

            if (isBloodMoon) return WeatherCondition.BloodMoon;
            if (isEclipse) return WeatherCondition.Eclipse;

            float qiDensity = CurrentWeather.QiDensity;
            float spiritChance = CurrentSeason == Season.Spring ? 0.08f : 0.03f;
            if (qiDensity > 0.5f && roll < 0.1f) return WeatherCondition.QiSurge;
            if (qiDensity > 0.3f && roll < 0.1f + spiritChance) return WeatherCondition.SpiritTide;

            return CurrentSeason switch
            {
                Season.Spring => roll switch
                {
                    < 0.25f => WeatherCondition.Clear,
                    < 0.40f => WeatherCondition.Cloudy,
                    < 0.60f => WeatherCondition.Rain,
                    < 0.72f => WeatherCondition.HeavyRain,
                    < 0.78f => WeatherCondition.Thunderstorm,
                    < 0.85f => WeatherCondition.Fog,
                    < 0.90f => WeatherCondition.Sandstorm,
                    _ => WeatherCondition.Clear,
                },
                Season.Summer => roll switch
                {
                    < 0.30f => WeatherCondition.Clear,
                    < 0.45f => WeatherCondition.Cloudy,
                    < 0.55f => WeatherCondition.Rain,
                    < 0.65f => WeatherCondition.HeavyRain,
                    < 0.75f => WeatherCondition.Thunderstorm,
                    < 0.80f => WeatherCondition.Fog,
                    < 0.90f => WeatherCondition.Sandstorm,
                    _ => WeatherCondition.Clear,
                },
                Season.Autumn => roll switch
                {
                    < 0.30f => WeatherCondition.Clear,
                    < 0.50f => WeatherCondition.Cloudy,
                    < 0.60f => WeatherCondition.Rain,
                    < 0.68f => WeatherCondition.HeavyRain,
                    < 0.73f => WeatherCondition.Thunderstorm,
                    < 0.83f => WeatherCondition.Fog,
                    < 0.90f => WeatherCondition.Sandstorm,
                    _ => WeatherCondition.Clear,
                },
                Season.Winter => roll switch
                {
                    < 0.20f => WeatherCondition.Clear,
                    < 0.35f => WeatherCondition.Cloudy,
                    < 0.40f => WeatherCondition.Rain,
                    < 0.45f => WeatherCondition.HeavyRain,
                    < 0.50f => WeatherCondition.Thunderstorm,
                    < 0.55f => WeatherCondition.Fog,
                    < 0.60f => WeatherCondition.Sandstorm,
                    < 0.80f => WeatherCondition.Snow,
                    _ => WeatherCondition.Blizzard,
                },
                _ => WeatherCondition.Clear,
            };
        }

        private WeatherIntensity RollIntensity(WeatherCondition type)
        {
            float roll = Main.rand.NextFloat();
            return type switch
            {
                WeatherCondition.Clear or WeatherCondition.Cloudy => WeatherIntensity.Light,
                WeatherCondition.Fog => roll < 0.5f ? WeatherIntensity.Light : WeatherIntensity.Moderate,
                WeatherCondition.Rain => roll < 0.6f ? WeatherIntensity.Light : WeatherIntensity.Moderate,
                WeatherCondition.HeavyRain => roll < 0.5f ? WeatherIntensity.Moderate : WeatherIntensity.Heavy,
                WeatherCondition.Thunderstorm => roll < 0.4f ? WeatherIntensity.Heavy : WeatherIntensity.Extreme,
                WeatherCondition.Sandstorm => roll < 0.5f ? WeatherIntensity.Moderate : WeatherIntensity.Heavy,
                WeatherCondition.Snow => roll < 0.6f ? WeatherIntensity.Light : WeatherIntensity.Moderate,
                WeatherCondition.Blizzard => roll < 0.5f ? WeatherIntensity.Heavy : WeatherIntensity.Extreme,
                WeatherCondition.QiSurge => WeatherIntensity.Extreme,
                WeatherCondition.SpiritTide => WeatherIntensity.Heavy,
                _ => WeatherIntensity.Light,
            };
        }

        private void ApplyWeatherEffects(WeatherState weather)
        {
            switch (weather.Type)
            {
                case WeatherCondition.Rain:
                case WeatherCondition.HeavyRain:
                    CurrentWeather.Humidity = System.Math.Min(1f, CurrentWeather.Humidity + 0.3f);
                    CurrentWeather.Temperature -= 3f;
                    break;
                case WeatherCondition.Thunderstorm:
                    CurrentWeather.Humidity = System.Math.Min(1f, CurrentWeather.Humidity + 0.5f);
                    CurrentWeather.Temperature -= 5f;
                    CurrentWeather.WindSpeed = 0.3f;
                    break;
                case WeatherCondition.Sandstorm:
                    CurrentWeather.Humidity = System.Math.Max(0f, CurrentWeather.Humidity - 0.3f);
                    CurrentWeather.WindSpeed = 0.5f;
                    break;
                case WeatherCondition.Snow:
                case WeatherCondition.Blizzard:
                    CurrentWeather.Temperature -= 10f;
                    CurrentWeather.Humidity += 0.2f;
                    break;
                case WeatherCondition.QiSurge:
                    CurrentWeather.QiDensity = System.Math.Min(1f, CurrentWeather.QiDensity + 0.4f);
                    break;
                case WeatherCondition.SpiritTide:
                    CurrentWeather.QiDensity = System.Math.Min(1f, CurrentWeather.QiDensity + 0.2f);
                    break;
                case WeatherCondition.Clear:
                    CurrentWeather.Temperature += 2f;
                    CurrentWeather.Humidity -= 0.1f;
                    CurrentWeather.WindSpeed = 0.05f;
                    break;
            }

            CurrentWeather.Temperature = MathHelper.Clamp(CurrentWeather.Temperature, -20f, 45f);
            CurrentWeather.Humidity = MathHelper.Clamp(CurrentWeather.Humidity, 0f, 1f);
            CurrentWeather.QiDensity = MathHelper.Clamp(CurrentWeather.QiDensity, 0f, 1f);
        }

        private void UpdateWeatherEffects()
        {
            if (CurrentWeather.Type == WeatherCondition.QiSurge)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    var player = Main.player[i];
                    if (!player.active) continue;
                    var qiResource = player.GetModPlayer<Players.QiResourcePlayer>();
                    float regenBonus = CurrentWeather.QiDensity * 0.5f;
                    qiResource.QiCurrent = System.Math.Min(qiResource.QiMaxCurrent,
                        qiResource.QiCurrent + regenBonus * 0.01f);
                }
            }

            if (CurrentWeather.Type == WeatherCondition.Thunderstorm && CurrentWeather.ElapsedTicks % 180 == 0)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    var player = Main.player[i];
                    if (!player.active || player.dead) continue;
                    if (player.ZoneOverworldHeight && Main.rand.NextFloat() < 0.02f)
                    {
                        int strikeX = (int)player.Center.X + Main.rand.Next(-200, 200);
                        int strikeY = (int)player.Center.Y - 400;
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(),
                            new Vector2(strikeX, strikeY), Vector2.Zero,
                            Terraria.ID.ProjectileID.CultistBossLightningOrbArc,
                            50, 2f, Main.myPlayer);
                    }
                }
            }
        }

        public float GetCultivationBonus()
        {
            float weatherBonus = CurrentWeather.Type switch
            {
                WeatherCondition.QiSurge => 0.5f,
                WeatherCondition.SpiritTide => 0.3f,
                WeatherCondition.Clear => 0.1f,
                WeatherCondition.Cloudy => 0.05f,
                _ => 0f,
            };

            float seasonMult = GetCurrentSeasonModifiers().CultivationSpeedMult;
            float moonBonus = _moonCultivationBonus.TryGetValue(CurrentMoonPhase, out var mb) ? mb : 1f;

            return weatherBonus + (seasonMult * moonBonus - 1f);
        }

        public float GetPlantGrowthBonus()
        {
            float weatherBonus = 0f;
            if (CurrentWeather.Type == WeatherCondition.Rain || CurrentWeather.Type == WeatherCondition.HeavyRain)
                weatherBonus += 0.3f;
            if (CurrentWeather.Type == WeatherCondition.SpiritTide)
                weatherBonus += 0.2f;
            if (CurrentWeather.Type == WeatherCondition.QiSurge)
                weatherBonus += 0.4f;
            if (CurrentWeather.Type == WeatherCondition.Sandstorm || CurrentWeather.Type == WeatherCondition.Blizzard)
                weatherBonus -= 0.3f;

            float seasonMult = GetCurrentSeasonModifiers().PlantGrowthMult;
            return weatherBonus + (seasonMult - 1f);
        }

        public float GetCombatPenalty()
        {
            return CurrentWeather.Type switch
            {
                WeatherCondition.Fog => 0.15f,
                WeatherCondition.Sandstorm => 0.2f,
                WeatherCondition.Blizzard => 0.25f,
                WeatherCondition.Thunderstorm => 0.1f,
                _ => 0f,
            };
        }

        public float GetBeastWaveChance()
        {
            float baseChance = GetCurrentSeasonModifiers().BeastWaveChanceMult;
            if (CurrentMoonPhase == CelestialPhase.FullMoon)
                baseChance *= 2.0f;
            if (CurrentWeather.Type == WeatherCondition.BloodMoon)
                baseChance *= 3.0f;
            return baseChance;
        }

        public float GetGuLoyaltyDecayRate()
        {
            return GetCurrentSeasonModifiers().GuLoyaltyDecayMult;
        }

        public float GetMerchantVisitMult()
        {
            return GetCurrentSeasonModifiers().MerchantVisitMult;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["weatherType"] = (int)CurrentWeather.Type;
            tag["weatherIntensity"] = (int)CurrentWeather.Intensity;
            tag["weatherDuration"] = CurrentWeather.DurationTicks;
            tag["weatherElapsed"] = CurrentWeather.ElapsedTicks;
            tag["weatherTimer"] = WeatherChangeTimer;
            tag["temperature"] = CurrentWeather.Temperature;
            tag["humidity"] = CurrentWeather.Humidity;
            tag["qiDensity"] = CurrentWeather.QiDensity;
            tag["season"] = (int)CurrentSeason;
            tag["moonPhase"] = (int)CurrentMoonPhase;
            tag["dayInSeason"] = DayInSeason;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            CurrentWeather = new WeatherState
            {
                Type = (WeatherCondition)tag.GetInt("weatherType"),
                Intensity = (WeatherIntensity)tag.GetInt("weatherIntensity"),
                DurationTicks = tag.GetInt("weatherDuration"),
                ElapsedTicks = tag.GetInt("weatherElapsed"),
                Temperature = tag.GetFloat("temperature"),
                Humidity = tag.GetFloat("humidity"),
                QiDensity = tag.GetFloat("qiDensity"),
            };
            WeatherChangeTimer = tag.GetInt("weatherTimer");

            CurrentSeason = (Season)tag.GetInt("season");
            CurrentMoonPhase = (CelestialPhase)tag.GetInt("moonPhase");
            DayInSeason = tag.GetInt("dayInSeason");

            InitializeModifiers();
        }
    }

    public class WeatherChangedEvent : GuWorldEvent
    {
        public WeatherCondition NewWeather;
    }
}
