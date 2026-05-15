using System.Collections.Generic;

namespace VerminLordMod.Common.Abstractions
{
    public enum GrowthStage
    {
        Seed = 0,           // 种子/初始
        Sprout,             // 萌芽/幼体
        Growing,            // 成长
        Mature,             // 成熟
        Peak,               // 巅峰/完全体
    }

    public interface IGrowable
    {
        string GrowableID { get; }
        GrowthStage CurrentStage { get; }
        float GrowthProgress { get; }
        float GrowthRate { get; set; }
        bool IsFullyGrown => CurrentStage == GrowthStage.Peak;
        bool CanGrow => !IsFullyGrown;

        void TickGrowth(float deltaTime = 1f);
        void ForceAdvance();
        void ResetGrowth();
        float GetTimeToNextStage();
    }

    public interface IGrowableWithEnvironment : IGrowable
    {
        float GetEnvironmentBonus();
        bool IsInOptimalEnvironment();
        void ApplyEnvironmentEffects();
    }

    public interface IGrowableWithYield : IGrowable
    {
        int YieldAmount { get; }
        int YieldItemID { get; }
        bool CanHarvest => CurrentStage >= GrowthStage.Mature;
        int Harvest();
    }

    public class GrowthStageConfig
    {
        public GrowthStage Stage;
        public float RequiredProgress;
        public string DisplayName;
        public Dictionary<string, float> StatMultipliers = new();
    }

    public abstract class BaseGrowable : IGrowable
    {
        public abstract string GrowableID { get; }
        public GrowthStage CurrentStage { get; protected set; } = GrowthStage.Seed;
        public float GrowthProgress { get; protected set; }
        public float GrowthRate { get; set; } = 1f;
        public bool IsFullyGrown => CurrentStage == GrowthStage.Peak;
        public bool CanGrow => !IsFullyGrown;

        protected readonly List<GrowthStageConfig> _stageConfigs = new();

        protected BaseGrowable()
        {
            InitializeStageConfigs();
        }

        protected virtual void InitializeStageConfigs()
        {
            _stageConfigs.Add(new GrowthStageConfig { Stage = GrowthStage.Seed, RequiredProgress = 0f, DisplayName = "种子" });
            _stageConfigs.Add(new GrowthStageConfig { Stage = GrowthStage.Sprout, RequiredProgress = 20f, DisplayName = "萌芽" });
            _stageConfigs.Add(new GrowthStageConfig { Stage = GrowthStage.Growing, RequiredProgress = 50f, DisplayName = "成长" });
            _stageConfigs.Add(new GrowthStageConfig { Stage = GrowthStage.Mature, RequiredProgress = 80f, DisplayName = "成熟" });
            _stageConfigs.Add(new GrowthStageConfig { Stage = GrowthStage.Peak, RequiredProgress = 100f, DisplayName = "巅峰" });
        }

        public virtual void TickGrowth(float deltaTime = 1f)
        {
            if (IsFullyGrown) return;
            GrowthProgress += GrowthRate * deltaTime;
            CheckStageAdvance();
        }

        public virtual void ForceAdvance()
        {
            if (IsFullyGrown) return;
            int nextStage = (int)CurrentStage + 1;
            if (nextStage <= (int)GrowthStage.Peak)
            {
                var nextConfig = _stageConfigs.Find(c => c.Stage == (GrowthStage)nextStage);
                if (nextConfig != null)
                {
                    GrowthProgress = nextConfig.RequiredProgress;
                    CurrentStage = (GrowthStage)nextStage;
                }
            }
        }

        public virtual void ResetGrowth()
        {
            CurrentStage = GrowthStage.Seed;
            GrowthProgress = 0f;
        }

        public virtual float GetTimeToNextStage()
        {
            if (IsFullyGrown) return 0f;
            int nextStage = (int)CurrentStage + 1;
            if (nextStage <= (int)GrowthStage.Peak)
            {
                var nextConfig = _stageConfigs.Find(c => c.Stage == (GrowthStage)nextStage);
                if (nextConfig != null && GrowthRate > 0)
                    return (nextConfig.RequiredProgress - GrowthProgress) / GrowthRate;
            }
            return 0f;
        }

        protected virtual void CheckStageAdvance()
        {
            for (int i = _stageConfigs.Count - 1; i >= 0; i--)
            {
                if (GrowthProgress >= _stageConfigs[i].RequiredProgress)
                {
                    if (CurrentStage != _stageConfigs[i].Stage)
                    {
                        CurrentStage = _stageConfigs[i].Stage;
                        OnStageAdvanced(CurrentStage);
                    }
                    break;
                }
            }
        }

        protected virtual void OnStageAdvanced(GrowthStage newStage) { }
    }
}
