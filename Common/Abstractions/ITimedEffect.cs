using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;

namespace VerminLordMod.Common.Abstractions
{
    public enum TimedEffectType
    {
        None = 0,
        DoT,            // 持续伤害
        Slow,           // 减速
        Freeze,         // 冻结
        ArmorShred,     // 碎甲
        Weaken,         // 虚弱
        Fear,           // 恐惧
        Charm,          // 魅惑
        Blind,          // 致盲
        Silence,        // 沉默
        Disarm,         // 缴械
        Mark,           // 标记
        Shield,         // 护盾
        Buff,           // 增益
        Debuff,         // 减益
        Custom,         // 自定义
    }

    public enum EffectStackingRule
    {
        Override,       // 新效果覆盖旧效果
        Refresh,        // 刷新持续时间，取较高强度
        Stack,          // 叠加层数
        Ignore,         // 已有同类效果时忽略新的
    }

    public interface ITimedEffect
    {
        string EffectID { get; }
        TimedEffectType EffectType { get; }
        string DisplayName { get; }
        float Intensity { get; }
        int RemainingTicks { get; }
        int TotalTicks { get; }
        bool IsExpired => RemainingTicks <= 0;
        float Progress => TotalTicks > 0 ? 1f - (float)RemainingTicks / TotalTicks : 0f;
        EffectStackingRule StackingRule { get; }
        int SourcePlayerID { get; }

        void Tick();
        void Apply(Entity target);
        void Remove(Entity target);
        ITimedEffect Clone();
    }

    public interface ITimedEffectContainer
    {
        IReadOnlyList<ITimedEffect> ActiveEffects { get; }
        void AddEffect(ITimedEffect effect);
        void RemoveEffect(string effectID);
        void RemoveAllEffects(TimedEffectType type);
        void RemoveAllEffects();
        bool HasEffect(string effectID);
        bool HasEffect(TimedEffectType type);
        T GetEffect<T>(string effectID) where T : class, ITimedEffect;
        IReadOnlyList<ITimedEffect> GetEffects(TimedEffectType type);
        void TickAll();
    }

    public abstract class BaseTimedEffect : ITimedEffect
    {
        public abstract string EffectID { get; }
        public abstract TimedEffectType EffectType { get; }
        public abstract string DisplayName { get; }
        public abstract float Intensity { get; }
        public abstract EffectStackingRule StackingRule { get; }
        public abstract int SourcePlayerID { get; }

        public int RemainingTicks { get; protected set; }
        public int TotalTicks { get; protected set; }

        protected BaseTimedEffect(int durationTicks)
        {
            TotalTicks = durationTicks;
            RemainingTicks = durationTicks;
        }

        public virtual void Tick()
        {
            if (RemainingTicks > 0)
                RemainingTicks--;
        }

        public abstract void Apply(Entity target);
        public abstract void Remove(Entity target);
        public abstract ITimedEffect Clone();
    }

    public class TimedEffectContainer : ITimedEffectContainer
    {
        private readonly List<ITimedEffect> _effects = new();

        public IReadOnlyList<ITimedEffect> ActiveEffects => _effects.AsReadOnly();

        public void AddEffect(ITimedEffect effect)
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                if (_effects[i].EffectID == effect.EffectID)
                {
                    switch (effect.StackingRule)
                    {
                        case EffectStackingRule.Override:
                            _effects[i] = effect;
                            return;
                        case EffectStackingRule.Refresh:
                            var refreshed = effect.Clone();
                            refreshed.Tick();
                            _effects[i] = refreshed;
                            return;
                        case EffectStackingRule.Ignore:
                            return;
                        case EffectStackingRule.Stack:
                            break;
                    }
                }
            }
            _effects.Add(effect);
        }

        public void RemoveEffect(string effectID)
        {
            _effects.RemoveAll(e => e.EffectID == effectID);
        }

        public void RemoveAllEffects(TimedEffectType type)
        {
            _effects.RemoveAll(e => e.EffectType == type);
        }

        public void RemoveAllEffects()
        {
            _effects.Clear();
        }

        public bool HasEffect(string effectID)
        {
            return _effects.Exists(e => e.EffectID == effectID);
        }

        public bool HasEffect(TimedEffectType type)
        {
            return _effects.Exists(e => e.EffectType == type);
        }

        public T GetEffect<T>(string effectID) where T : class, ITimedEffect
        {
            foreach (var e in _effects)
            {
                if (e.EffectID == effectID && e is T typed)
                    return typed;
            }
            return null;
        }

        public IReadOnlyList<ITimedEffect> GetEffects(TimedEffectType type)
        {
            return _effects.FindAll(e => e.EffectType == type).AsReadOnly();
        }

        public void TickAll()
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                _effects[i].Tick();
                if (_effects[i].IsExpired)
                {
                    _effects.RemoveAt(i);
                }
            }
        }
    }
}
