using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Events;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// 天气-剧情联动系统 — 根据StoryPhase触发特定天气效果
    ///
    /// 联动规则：
    /// - TianHeAttack: 触发雷暴天气
    /// - BloodSacrifice: 触发血月
    /// - DaTongFeng: 触发极端大风
    /// - DestinyWarBegin: 触发日蚀
    /// - Ascension: 触发灵潮
    /// - SevenTurnBegin: 触发混沌天气
    /// </summary>
    public class StoryWeatherIntegration : ModSystem
    {
        public static StoryWeatherIntegration Instance => ModContent.GetInstance<StoryWeatherIntegration>();

        private bool _subscribed = false;

        /// <summary> 当前剧情天气效果 </summary>
        private string _activeStoryWeather = null;
        private int _weatherDuration = 0;

        /// <summary> 剧情天气定义 </summary>
        private static readonly Dictionary<StoryPhase, StoryWeatherDef> PhaseWeatherMap = new()
        {
            { StoryPhase.TianHeAttack, new StoryWeatherDef("雷暴", 36000, "天鹤来袭——雷鸣电闪！") },
            { StoryPhase.BloodSacrifice, new StoryWeatherDef("血月", 72000, "血色月光笼罩大地……") },
            { StoryPhase.DaTongFeng, new StoryWeatherDef("大风", 144000, "大同风——天地变色！") },
            { StoryPhase.DestinyWarBegin, new StoryWeatherDef("日蚀", 108000, "宿命大战——天日无光！") },
            { StoryPhase.Ascension, new StoryWeatherDef("灵潮", 72000, "天地灵气涌动——升仙之兆！") },
            { StoryPhase.SevenTurnBegin, new StoryWeatherDef("混沌", 144000, "混沌之气弥漫——蛊仙之路开启！") },
        };

        public override void PostUpdateWorld()
        {
            if (!_subscribed)
            {
                EventBus.Subscribe<StoryPhaseAdvancedEvent>(OnPhaseAdvanced);
                _subscribed = true;
            }

            // 更新剧情天气持续时间
            if (_weatherDuration > 0)
            {
                _weatherDuration--;
                if (_weatherDuration <= 0)
                {
                    _activeStoryWeather = null;
                }
            }
        }

        public override void OnWorldUnload()
        {
            _subscribed = false;
            _activeStoryWeather = null;
            _weatherDuration = 0;
        }

        /// <summary> 获取当前剧情天气 </summary>
        public static string GetActiveStoryWeather()
        {
            return Instance._activeStoryWeather;
        }

        /// <summary> 是否处于剧情天气中 </summary>
        public static bool IsInStoryWeather()
        {
            return Instance._activeStoryWeather != null;
        }

        private void OnPhaseAdvanced(StoryPhaseAdvancedEvent e)
        {
            var phase = (StoryPhase)e.NewPhase;
            if (PhaseWeatherMap.TryGetValue(phase, out var def))
            {
                _activeStoryWeather = def.WeatherType;
                _weatherDuration = def.DurationTicks;

                Main.NewText($"[蛊世界] {def.Message}", Color.OrangeRed);

                // 应用对应的Terraria天气效果
                ApplyVanillaWeather(def.WeatherType);

                var uiSystem = ModContent.GetInstance<global::VerminLordMod.Common.UI.StoryUI.StoryUISystem>();
                uiSystem?.ShowToast(def.WeatherType, Color.OrangeRed, 5f);
            }
        }

        private void ApplyVanillaWeather(string weatherType)
        {
            switch (weatherType)
            {
                case "雷暴":
                    Main.StartRain();
                    Main.maxRaining = 1f;
                    break;
                case "血月":
                    Main.bloodMoon = true;
                    break;
                case "大风":
                    Main.windSpeedCurrent = 2f;
                    Main.windSpeedTarget = 2f;
                    break;
                case "日蚀":
                    Main.eclipse = true;
                    break;
                case "灵潮":
                    // 灵潮为自定义天气，不直接映射到原版
                    break;
                case "混沌":
                    // 混沌为自定义天气，不直接映射到原版
                    break;
            }
        }

        // ==================== 内部类 ====================

        private class StoryWeatherDef
        {
            public string WeatherType;
            public int DurationTicks;
            public string Message;

            public StoryWeatherDef(string type, int ticks, string msg)
            {
                WeatherType = type;
                DurationTicks = ticks;
                Message = msg;
            }
        }
    }
}
