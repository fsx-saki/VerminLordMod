using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.Abstractions
{
    public interface ISaveable
    {
        void SaveTo(TagCompound tag);
        void LoadFrom(TagCompound tag);
    }

    public interface ISaveableWithKey : ISaveable
    {
        string SaveKey { get; }
    }

    public static class SaveableHelper
    {
        public static void SaveList<T>(TagCompound tag, string key, List<T> items) where T : ISaveable
        {
            var list = new List<TagCompound>();
            foreach (var item in items)
            {
                var subTag = new TagCompound();
                item.SaveTo(subTag);
                list.Add(subTag);
            }
            tag[key] = list;
        }

        public static List<T> LoadList<T>(TagCompound tag, string key) where T : ISaveable, new()
        {
            var result = new List<T>();
            if (tag.TryGet(key, out List<TagCompound> list))
            {
                foreach (var subTag in list)
                {
                    var item = new T();
                    item.LoadFrom(subTag);
                    result.Add(item);
                }
            }
            return result;
        }

        public static void SaveDictionary<T>(TagCompound tag, string key, Dictionary<string, T> dict) where T : ISaveable
        {
            var list = new List<TagCompound>();
            foreach (var kvp in dict)
            {
                var subTag = new TagCompound();
                subTag["dictKey"] = kvp.Key;
                kvp.Value.SaveTo(subTag);
                list.Add(subTag);
            }
            tag[key] = list;
        }

        public static Dictionary<string, T> LoadDictionary<T>(TagCompound tag, string key) where T : ISaveable, new()
        {
            var result = new Dictionary<string, T>();
            if (tag.TryGet(key, out List<TagCompound> list))
            {
                foreach (var subTag in list)
                {
                    string dictKey = subTag.GetString("dictKey");
                    var item = new T();
                    item.LoadFrom(subTag);
                    result[dictKey] = item;
                }
            }
            return result;
        }

        public static void SaveEnum<T>(TagCompound tag, string key, T value) where T : struct, System.Enum
        {
            tag[key] = value.ToString();
        }

        public static T LoadEnum<T>(TagCompound tag, string key, T defaultValue = default) where T : struct, System.Enum
        {
            if (tag.TryGet(key, out string str) && System.Enum.TryParse<T>(str, out var result))
                return result;
            return defaultValue;
        }
    }
}
