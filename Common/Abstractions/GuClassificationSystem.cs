using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;

namespace VerminLordMod.Common.Abstractions
{
    public class GuClassificationEntry
    {
        public int ItemType;
        public string InternalName;
        public string DisplayName;
        public int GuLevel;
        public GuCategory Category;
        public GuElement PrimaryElement;
        public GuElement SecondaryElement;
        public ulong DaoHenTagMask;
        public bool IsMainGuCandidate;
        public bool IsConsumable;
        public bool IsAccessory;
    }

    public static class GuClassificationSystem
    {
        private static readonly Dictionary<int, GuClassificationEntry> _entries = new();
        private static readonly Dictionary<GuCategory, List<GuClassificationEntry>> _byCategory = new();
        private static readonly Dictionary<GuElement, List<GuClassificationEntry>> _byElement = new();
        private static readonly Dictionary<int, List<GuClassificationEntry>> _byLevel = new();
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _byCategory.Clear();
            _byElement.Clear();
            _byLevel.Clear();
            _entries.Clear();

            foreach (GuCategory cat in System.Enum.GetValues<GuCategory>())
                _byCategory[cat] = new List<GuClassificationEntry>();
            foreach (GuElement elem in System.Enum.GetValues<GuElement>())
                _byElement[elem] = new List<GuClassificationEntry>();

            RegisterAllGus();
        }

        private static void RegisterAllGus()
        {
            // TODO: 自动扫描所有实现 IGu 的 ModItem 并注册
            // 目前需要手动注册，后续可通过反射自动扫描
        }

        public static void Register(GuClassificationEntry entry)
        {
            if (!_initialized) Initialize();

            _entries[entry.ItemType] = entry;

            if (!_byCategory.ContainsKey(entry.Category))
                _byCategory[entry.Category] = new List<GuClassificationEntry>();
            _byCategory[entry.Category].Add(entry);

            if (!_byElement.ContainsKey(entry.PrimaryElement))
                _byElement[entry.PrimaryElement] = new List<GuClassificationEntry>();
            _byElement[entry.PrimaryElement].Add(entry);

            if (!_byLevel.ContainsKey(entry.GuLevel))
                _byLevel[entry.GuLevel] = new List<GuClassificationEntry>();
            _byLevel[entry.GuLevel].Add(entry);
        }

        public static GuClassificationEntry GetEntry(int itemType)
        {
            if (!_initialized) Initialize();
            _entries.TryGetValue(itemType, out var entry);
            return entry;
        }

        public static GuClassificationEntry GetEntry(Item item)
        {
            return GetEntry(item.type);
        }

        public static IReadOnlyList<GuClassificationEntry> GetByCategory(GuCategory category)
        {
            if (!_initialized) Initialize();
            return _byCategory.TryGetValue(category, out var list) ? list.AsReadOnly() : new List<GuClassificationEntry>().AsReadOnly();
        }

        public static IReadOnlyList<GuClassificationEntry> GetByElement(GuElement element)
        {
            if (!_initialized) Initialize();
            return _byElement.TryGetValue(element, out var list) ? list.AsReadOnly() : new List<GuClassificationEntry>().AsReadOnly();
        }

        public static IReadOnlyList<GuClassificationEntry> GetByLevel(int level)
        {
            if (!_initialized) Initialize();
            return _byLevel.TryGetValue(level, out var list) ? list.AsReadOnly() : new List<GuClassificationEntry>().AsReadOnly();
        }

        public static List<GuClassificationEntry> GetGusForLevel(int playerLevel, int maxResults = 20)
        {
            if (!_initialized) Initialize();
            var result = new List<GuClassificationEntry>();
            for (int lvl = System.Math.Max(1, playerLevel - 1); lvl <= playerLevel + 1; lvl++)
            {
                if (_byLevel.TryGetValue(lvl, out var list))
                    result.AddRange(list);
            }
            return result.GetRange(0, System.Math.Min(result.Count, maxResults));
        }

        public static bool HasElement(Item item, GuElement element)
        {
            var entry = GetEntry(item);
            if (entry == null) return false;
            return entry.PrimaryElement == element || entry.SecondaryElement == element;
        }

        public static bool HasCategory(Item item, GuCategory category)
        {
            var entry = GetEntry(item);
            return entry != null && entry.Category == category;
        }

        public static DaoEffectTags GetDaoEffectTags(Item item)
        {
            var entry = GetEntry(item);
            if (entry == null) return DaoEffectTags.None;
            return (DaoEffectTags)entry.DaoHenTagMask;
        }
    }
}
