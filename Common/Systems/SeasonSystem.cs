using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.Systems
{
    public enum Season
    {
        Spring = 0,     // 春 — 万物复苏，种植加速，药材品质提升
        Summer = 1,     // 夏 — 灵气充沛，修炼效率提升，兽潮频率增加
        Autumn = 2,     // 秋 — 丰收季节，资源产出翻倍，商队频繁
        Winter = 3,     // 冬 — 万物蛰伏，修炼减速，蛊虫忠诚度消耗增加
    }

    public enum CelestialPhase
    {
        NewMoon,            // 朔月 — 暗属性增强，暗杀/潜行加成
        WaxingCrescent,     // 上弦月 — 成长类效果加成
        FirstQuarter,       // 上半月 — 平衡期
        WaxingGibbous,      // 盈凸月 — 灵气聚集，修炼加成
        FullMoon,           // 满月 — 灵气最盛，蛊虫威力提升，狼潮概率增加
        WaningGibbous,      // 亏凸月 — 收敛期
        LastQuarter,        // 下半月 — 衰减期
        WaningCrescent,     // 下弦月 — 阴属性增强
    }

    public enum WeatherType
    {
        Clear,          // 晴天
        Rain,           // 雨天 — 水属性加成，火属性削弱
        Storm,          // 暴风雨 — 雷属性加成，移动减速
        Snow,           // 雪天 — 冰属性加成，种植停止
        Fog,            // 迷雾 — 感知范围降低，暗杀加成
        SpiritRain,     // 灵雨 — 修炼效率大幅提升（稀有）
        BloodMoon,      // 血月 — 敌意NPC增强，杀戮加成
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

    public class SeasonSystem : ModSystem
    {
        public static SeasonSystem Instance => ModContent.GetInstance<SeasonSystem>();

        public Season CurrentSeason = Season.Spring;
        public CelestialPhase CurrentMoonPhase = CelestialPhase.NewMoon;
        public WeatherType CurrentWeather = WeatherType.Clear;
        public int DayInSeason = 0;
        public int WeatherRemainingTicks = 0;

        public const int DAYS_PER_SEASON = 10;
        public const int TICKS_PER_DAY = 36000;

        private readonly Dictionary<Season, SeasonModifier> _seasonModifiers = new();
        private readonly Dictionary<CelestialPhase, float> _moonCultivationBonus = new();
        private readonly Dictionary<WeatherType, float> _weatherMerchantBonus = new();

        public override void OnWorldLoad()
        {
            CurrentSeason = Season.Spring;
            CurrentMoonPhase = CelestialPhase.NewMoon;
            CurrentWeather = WeatherType.Clear;
            DayInSeason = 0;
            WeatherRemainingTicks = 0;
            InitializeModifiers();
        }

        private void InitializeModifiers()
        {
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

            _moonCultivationBonus[CelestialPhase.NewMoon] = 0.8f;
            _moonCultivationBonus[CelestialPhase.WaxingCrescent] = 0.9f;
            _moonCultivationBonus[CelestialPhase.FirstQuarter] = 1.0f;
            _moonCultivationBonus[CelestialPhase.WaxingGibbous] = 1.1f;
            _moonCultivationBonus[CelestialPhase.FullMoon] = 1.3f;
            _moonCultivationBonus[CelestialPhase.WaningGibbous] = 1.1f;
            _moonCultivationBonus[CelestialPhase.LastQuarter] = 1.0f;
            _moonCultivationBonus[CelestialPhase.WaningCrescent] = 0.9f;

            _weatherMerchantBonus[WeatherType.Clear] = 1.0f;
            _weatherMerchantBonus[WeatherType.Rain] = 0.8f;
            _weatherMerchantBonus[WeatherType.Storm] = 0.5f;
            _weatherMerchantBonus[WeatherType.Snow] = 0.6f;
            _weatherMerchantBonus[WeatherType.Fog] = 0.9f;
            _weatherMerchantBonus[WeatherType.SpiritRain] = 1.2f;
            _weatherMerchantBonus[WeatherType.BloodMoon] = 0.3f;
        }

        public SeasonModifier GetCurrentModifiers()
        {
            if (_seasonModifiers.TryGetValue(CurrentSeason, out var mod))
                return mod;
            return new SeasonModifier();
        }

        public float GetCultivationSpeed()
        {
            float baseSpeed = GetCurrentModifiers().CultivationSpeedMult;
            float moonBonus = _moonCultivationBonus.TryGetValue(CurrentMoonPhase, out var mb) ? mb : 1f;
            float weatherBonus = CurrentWeather == WeatherType.SpiritRain ? 1.5f : 1f;
            return baseSpeed * moonBonus * weatherBonus;
        }

        public float GetPlantGrowthSpeed()
        {
            float baseSpeed = GetCurrentModifiers().PlantGrowthMult;
            if (CurrentWeather == WeatherType.Rain || CurrentWeather == WeatherType.SpiritRain)
                baseSpeed *= 1.3f;
            if (CurrentWeather == WeatherType.Snow)
                baseSpeed *= 0.1f;
            return baseSpeed;
        }

        public float GetBeastWaveChance()
        {
            float baseChance = GetCurrentModifiers().BeastWaveChanceMult;
            if (CurrentMoonPhase == CelestialPhase.FullMoon)
                baseChance *= 2.0f;
            if (CurrentWeather == WeatherType.BloodMoon)
                baseChance *= 3.0f;
            return baseChance;
        }

        public float GetGuLoyaltyDecayRate()
        {
            return GetCurrentModifiers().GuLoyaltyDecayMult;
        }

        public override void PostUpdateWorld()
        {
            // TODO: 季节推进逻辑
            // TODO: 月相更新逻辑
            // TODO: 天气变化逻辑
            // TODO: 天气视觉效果
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["season"] = (int)CurrentSeason;
            tag["moonPhase"] = (int)CurrentMoonPhase;
            tag["weather"] = (int)CurrentWeather;
            tag["dayInSeason"] = DayInSeason;
            tag["weatherTicks"] = WeatherRemainingTicks;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            CurrentSeason = (Season)tag.GetInt("season");
            CurrentMoonPhase = (CelestialPhase)tag.GetInt("moonPhase");
            CurrentWeather = (WeatherType)tag.GetInt("weather");
            DayInSeason = tag.GetInt("dayInSeason");
            WeatherRemainingTicks = tag.GetInt("weatherTicks");
            InitializeModifiers();
        }
    }
}
