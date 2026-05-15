using System.Collections.Generic;

namespace VerminLordMod.Common.Abstractions
{
    public interface IDataDrivenConfig
    {
        string ConfigID { get; }
        void Validate();
    }

    public interface IDataDrivenConfigProvider<T> where T : class, IDataDrivenConfig
    {
        T GetConfig(string id);
        void RegisterConfig(string id, T config);
        IReadOnlyDictionary<string, T> GetAllConfigs();
    }

    public abstract class DataDrivenConfigProvider<T> : IDataDrivenConfigProvider<T> where T : class, IDataDrivenConfig
    {
        protected readonly Dictionary<string, T> _configs = new();

        public virtual T GetConfig(string id)
        {
            _configs.TryGetValue(id, out var config);
            return config;
        }

        public virtual void RegisterConfig(string id, T config)
        {
            config.Validate();
            _configs[id] = config;
        }

        public IReadOnlyDictionary<string, T> GetAllConfigs()
        {
            return _configs;
        }

        protected abstract void InitializeDefaults();
    }

    public abstract class BaseDataDrivenConfig : IDataDrivenConfig
    {
        public abstract string ConfigID { get; }

        public virtual void Validate()
        {
            // 子类重写以添加验证逻辑
        }
    }
}
