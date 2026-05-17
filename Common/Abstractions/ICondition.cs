using System.Collections.Generic;
using Terraria;

namespace VerminLordMod.Common.Abstractions
{
    /// <summary>
    /// 条件类型枚举 — 定义对话/任务/事件的条件分类
    /// </summary>
    public enum ConditionType
    {
        None = 0,
        RealmLevel,         // 境界等级条件
        QiAmount,           // 真元数量条件
        Reputation,         // 声望条件
        QuestCompleted,     // 任务完成条件
        ItemOwned,          // 物品持有条件
        NPCAlive,           // NPC存活条件
        StoryPhase,         // 剧情阶段条件
        TimeOfDay,          // 时间条件
        Biome,              // 生态条件
        FactionRelation,    // 势力关系条件
        GuCount,            // 蛊虫数量条件
        Custom,             // 自定义条件
    }

    /// <summary>
    /// 条件接口 — 定义可评估的游戏条件契约。
    /// 用于对话树分支、任务前置条件、事件触发条件等。
    /// </summary>
    public interface ICondition
    {
        string ConditionID { get; }
        ConditionType Type { get; }
        string Description { get; }
        bool Evaluate(Player player);
        string GetFailureMessage(Player player);
    }

    /// <summary>
    /// 条件评估器接口 — 提供条件组合评估（全部满足/任一满足）
    /// </summary>
    public interface IConditionEvaluator
    {
        bool EvaluateAll(IEnumerable<ICondition> conditions, Player player);
        bool EvaluateAny(IEnumerable<ICondition> conditions, Player player);
        List<ICondition> GetFailedConditions(IEnumerable<ICondition> conditions, Player player);
        string GetCombinedFailureMessage(IEnumerable<ICondition> conditions, Player player);
    }

    /// <summary>
    /// 条件评估器实现 — 提供全部满足/任一满足的组合评估逻辑
    /// </summary>
    public class ConditionEvaluator : IConditionEvaluator
    {
        public bool EvaluateAll(IEnumerable<ICondition> conditions, Player player)
        {
            foreach (var condition in conditions)
            {
                if (!condition.Evaluate(player))
                    return false;
            }
            return true;
        }

        public bool EvaluateAny(IEnumerable<ICondition> conditions, Player player)
        {
            foreach (var condition in conditions)
            {
                if (condition.Evaluate(player))
                    return true;
            }
            return false;
        }

        public List<ICondition> GetFailedConditions(IEnumerable<ICondition> conditions, Player player)
        {
            var failed = new List<ICondition>();
            foreach (var condition in conditions)
            {
                if (!condition.Evaluate(player))
                    failed.Add(condition);
            }
            return failed;
        }

        public string GetCombinedFailureMessage(IEnumerable<ICondition> conditions, Player player)
        {
            var failed = GetFailedConditions(conditions, player);
            if (failed.Count == 0) return "";
            if (failed.Count == 1) return failed[0].GetFailureMessage(player);

            var messages = new List<string>();
            foreach (var c in failed)
                messages.Add(c.GetFailureMessage(player));
            return string.Join("\n", messages);
        }
    }

    /// <summary>
    /// 条件基类 — 提供条件接口的默认实现，子类只需实现核心评估逻辑
    /// </summary>
    public abstract class BaseCondition : ICondition
    {
        public abstract string ConditionID { get; }
        public abstract ConditionType Type { get; }
        public abstract string Description { get; }
        public abstract bool Evaluate(Player player);

        public virtual string GetFailureMessage(Player player)
        {
            return $"条件未满足：{Description}";
        }
    }

    /// <summary>
    /// 组合条件 — 将多个条件通过 AND（全部满足）或 OR（任一满足）组合
    /// </summary>
    public class CompositeCondition : ICondition
    {
        public string ConditionID { get; }
        public ConditionType Type => ConditionType.Custom;
        public string Description { get; }

        private readonly List<ICondition> _conditions;
        private readonly bool _requireAll;

        public CompositeCondition(string id, string description, bool requireAll, params ICondition[] conditions)
        {
            ConditionID = id;
            Description = description;
            _conditions = new List<ICondition>(conditions);
            _requireAll = requireAll;
        }

        public bool Evaluate(Player player)
        {
            if (_requireAll)
            {
                foreach (var c in _conditions)
                    if (!c.Evaluate(player)) return false;
                return true;
            }
            else
            {
                foreach (var c in _conditions)
                    if (c.Evaluate(player)) return true;
                return false;
            }
        }

        public string GetFailureMessage(Player player)
        {
            var evaluator = new ConditionEvaluator();
            if (_requireAll)
                return evaluator.GetCombinedFailureMessage(_conditions, player);
            return $"以下任一条件均未满足：{Description}";
        }
    }

    /// <summary>
    /// 取反条件 — 对内部条件取反，当内部条件不满足时返回 true
    /// </summary>
    public class NegatedCondition : ICondition
    {
        private readonly ICondition _inner;

        public string ConditionID => "NOT_" + _inner.ConditionID;
        public ConditionType Type => _inner.Type;
        public string Description => "非(" + _inner.Description + ")";

        public NegatedCondition(ICondition inner)
        {
            _inner = inner;
        }

        public bool Evaluate(Player player) => !_inner.Evaluate(player);

        public string GetFailureMessage(Player player)
        {
            return _inner.Description + " — 条件不应当满足";
        }
    }
}
