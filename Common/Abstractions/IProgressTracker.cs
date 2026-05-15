using System;
using System.Collections.Generic;

namespace VerminLordMod.Common.Abstractions
{
    public enum ProgressType
    {
        None = 0,
        Breakthrough,      // 突破进度
        Refinement,        // 炼化进度
        Quest,             // 任务进度
        Breeding,          // 育种进度
        Growth,            // 生长进度
        Loyalty,           // 忠诚度进度
        Evolution,         // 进化进度
        Crafting,          // 制作进度
        Custom,            // 自定义
    }

    public interface IProgressTracker
    {
        string TrackerID { get; }
        ProgressType Type { get; }
        float Current { get; }
        float Target { get; }
        float Progress => Target > 0 ? Math.Clamp(Current / Target, 0f, 1f) : 0f;
        bool IsComplete => Current >= Target;
        float ProgressPercent => Progress * 100f;

        void AddProgress(float amount);
        void RemoveProgress(float amount);
        void SetProgress(float value);
        void Reset();
    }

    public interface IProgressTrackerWithDecay : IProgressTracker
    {
        float DecayRate { get; }
        bool IsDecaying { get; set; }
        void TickDecay();
    }

    public interface IProgressTrackerWithMilestones : IProgressTracker
    {
        List<float> Milestones { get; }
        int CurrentMilestoneIndex { get; }
        event Action<int> OnMilestoneReached;
        bool HasReachedMilestone(int index);
    }

    public class SimpleProgressTracker : IProgressTracker
    {
        public string TrackerID { get; }
        public ProgressType Type { get; }
        public float Current { get; private set; }
        public float Target { get; }

        public SimpleProgressTracker(string id, ProgressType type, float target, float initial = 0f)
        {
            TrackerID = id;
            Type = type;
            Target = target;
            Current = initial;
        }

        public void AddProgress(float amount)
        {
            Current = Math.Clamp(Current + amount, 0f, Target);
        }

        public void RemoveProgress(float amount)
        {
            Current = Math.Clamp(Current - amount, 0f, Target);
        }

        public void SetProgress(float value)
        {
            Current = Math.Clamp(value, 0f, Target);
        }

        public void Reset()
        {
            Current = 0f;
        }
    }

    public class DecayingProgressTracker : IProgressTrackerWithDecay
    {
        public string TrackerID { get; }
        public ProgressType Type { get; }
        public float Current { get; private set; }
        public float Target { get; }
        public float DecayRate { get; }
        public bool IsDecaying { get; set; } = true;

        public DecayingProgressTracker(string id, ProgressType type, float target, float decayRate, float initial = 0f)
        {
            TrackerID = id;
            Type = type;
            Target = target;
            DecayRate = decayRate;
            Current = initial;
        }

        public void AddProgress(float amount)
        {
            Current = Math.Clamp(Current + amount, 0f, Target);
        }

        public void RemoveProgress(float amount)
        {
            Current = Math.Clamp(Current - amount, 0f, Target);
        }

        public void SetProgress(float value)
        {
            Current = Math.Clamp(value, 0f, Target);
        }

        public void Reset()
        {
            Current = 0f;
        }

        public void TickDecay()
        {
            if (IsDecaying && Current > 0)
            {
                Current = Math.Clamp(Current - DecayRate, 0f, Target);
            }
        }
    }

    public class MilestoneProgressTracker : IProgressTrackerWithMilestones
    {
        public string TrackerID { get; }
        public ProgressType Type { get; }
        public float Current { get; private set; }
        public float Target { get; }
        public List<float> Milestones { get; }
        public int CurrentMilestoneIndex { get; private set; } = -1;

        private readonly Action<int> _onMilestoneReached;
        event Action<int> IProgressTrackerWithMilestones.OnMilestoneReached
        {
            add => _onMilestoneReachedInternal += value;
            remove => _onMilestoneReachedInternal -= value;
        }
        private event Action<int> _onMilestoneReachedInternal;

        public MilestoneProgressTracker(string id, ProgressType type, float target, List<float> milestones, Action<int> onMilestoneReached = null)
        {
            TrackerID = id;
            Type = type;
            Target = target;
            Milestones = milestones;
            _onMilestoneReached = onMilestoneReached;
            _onMilestoneReachedInternal += onMilestoneReached;
        }

        public void AddProgress(float amount)
        {
            float oldProgress = Current;
            Current = Math.Clamp(Current + amount, 0f, Target);
            CheckMilestones(oldProgress, Current);
        }

        public void RemoveProgress(float amount)
        {
            Current = Math.Clamp(Current - amount, 0f, Target);
        }

        public void SetProgress(float value)
        {
            float oldProgress = Current;
            Current = Math.Clamp(value, 0f, Target);
            CheckMilestones(oldProgress, Current);
        }

        public void Reset()
        {
            Current = 0f;
            CurrentMilestoneIndex = -1;
        }

        public bool HasReachedMilestone(int index)
        {
            if (index < 0 || index >= Milestones.Count) return false;
            return Current >= Milestones[index];
        }

        private void CheckMilestones(float oldProgress, float newProgress)
        {
            for (int i = 0; i < Milestones.Count; i++)
            {
                if (oldProgress < Milestones[i] && newProgress >= Milestones[i])
                {
                    CurrentMilestoneIndex = i;
                    _onMilestoneReachedInternal?.Invoke(i);
                }
            }
        }
    }
}
