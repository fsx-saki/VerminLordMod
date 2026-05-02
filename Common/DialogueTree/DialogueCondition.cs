// ============================================================
// DialogueCondition - 对话条件系统
// 决定对话选项是否可见/可选
// ============================================================
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.DialogueTree;

/// <summary>
/// 对话条件 — 决定选项是否可见/可选
/// </summary>
public abstract class DialogueCondition
{
    /// <summary> 评估条件是否满足 </summary>
    public abstract bool Evaluate(Player player, NPC npc, BeliefState belief);
}

// ============================================================
// 具体条件类型
// ============================================================

/// <summary> 信念条件 </summary>
public class BeliefCondition : DialogueCondition
{
    public enum BeliefField { RiskThreshold, ConfidenceLevel, EstimatedPower }
    public enum CompareOp { LessThan, LessOrEqual, Equal, GreaterOrEqual, GreaterThan }

    public BeliefField Field;
    public CompareOp Operator;
    public float Value;

    public BeliefCondition() { }

    public BeliefCondition(BeliefField field, CompareOp op, float value)
    {
        Field = field;
        Operator = op;
        Value = value;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        if (belief == null) return false;

        float fieldValue = Field switch
        {
            BeliefField.RiskThreshold => belief.RiskThreshold,
            BeliefField.ConfidenceLevel => belief.ConfidenceLevel,
            BeliefField.EstimatedPower => belief.EstimatedPower,
            _ => 0f
        };

        return Operator switch
        {
            CompareOp.LessThan => fieldValue < Value,
            CompareOp.LessOrEqual => fieldValue <= Value,
            CompareOp.Equal => System.Math.Abs(fieldValue - Value) < 0.01f,
            CompareOp.GreaterOrEqual => fieldValue >= Value,
            CompareOp.GreaterThan => fieldValue > Value,
            _ => false
        };
    }
}

/// <summary> 声望条件 </summary>
public class ReputationCondition : DialogueCondition
{
    public FactionID Faction;
    public int MinReputation;

    public ReputationCondition() { }

    public ReputationCondition(FactionID faction, int minReputation)
    {
        Faction = faction;
        MinReputation = minReputation;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        var worldPlayer = player.GetModPlayer<GuWorldPlayer>();
        if (worldPlayer.FactionRelations.TryGetValue(Faction, out var rel))
        {
            return rel.ReputationPoints >= MinReputation;
        }
        return false;
    }
}

/// <summary> 修为条件 </summary>
public class RealmCondition : DialogueCondition
{
    public int MinGuLevel;

    public RealmCondition() { }

    public RealmCondition(int minGuLevel)
    {
        MinGuLevel = minGuLevel;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        var qiRealm = player.GetModPlayer<QiRealmPlayer>();
        return qiRealm.GuLevel >= MinGuLevel;
    }
}

/// <summary> 物品条件 </summary>
public class HasItemCondition : DialogueCondition
{
    public int ItemType;
    public int MinStack = 1;

    public HasItemCondition() { }

    public HasItemCondition(int itemType, int minStack = 1)
    {
        ItemType = itemType;
        MinStack = minStack;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        return player.HasItem(ItemType) && player.CountItem(ItemType) >= MinStack;
    }
}

/// <summary> 态度条件 </summary>
public class AttitudeCondition : DialogueCondition
{
    public GuAttitude RequiredAttitude;

    public AttitudeCondition() { }

    public AttitudeCondition(GuAttitude requiredAttitude)
    {
        RequiredAttitude = requiredAttitude;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        if (npc.ModNPC is GuMasterBase guMaster)
        {
            return guMaster.CurrentAttitude == RequiredAttitude;
        }
        return false;
    }
}

/// <summary> 对话标记条件 </summary>
public class FlagCondition : DialogueCondition
{
    public string FlagName;

    public FlagCondition() { }

    public FlagCondition(string flagName)
    {
        FlagName = flagName;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        // 标记存储在对话会话中，由 DialogueSession 提供
        return false; // 由外部覆盖
    }
}

// ============================================================
// 逻辑组合条件
// ============================================================

/// <summary> 与条件（所有子条件都满足） </summary>
public class AndCondition : DialogueCondition
{
    public DialogueCondition[] Conditions;

    public AndCondition(params DialogueCondition[] conditions)
    {
        Conditions = conditions;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        foreach (var c in Conditions)
        {
            if (!c.Evaluate(player, npc, belief))
                return false;
        }
        return true;
    }
}

/// <summary> 或条件（任一子条件满足） </summary>
public class OrCondition : DialogueCondition
{
    public DialogueCondition[] Conditions;

    public OrCondition(params DialogueCondition[] conditions)
    {
        Conditions = conditions;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        foreach (var c in Conditions)
        {
            if (c.Evaluate(player, npc, belief))
                return true;
        }
        return false;
    }
}

/// <summary> 非条件（取反） </summary>
public class NotCondition : DialogueCondition
{
    public DialogueCondition Condition;

    public NotCondition(DialogueCondition condition)
    {
        Condition = condition;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        return !Condition.Evaluate(player, npc, belief);
    }
}

// ============================================================
// 新增条件类型（阶段1扩展）
// ============================================================

/// <summary> 元石数量条件 - 检查玩家持有元石数量 </summary>
public class HasYuanSCondition : DialogueCondition
{
    public int MinYuanS;

    public HasYuanSCondition() { }

    public HasYuanSCondition(int minYuanS)
    {
        MinYuanS = minYuanS;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        int yuanSItemType = ModContent.ItemType<Content.Items.Consumables.YuanS>();
        return player.CountItem(yuanSItemType) >= MinYuanS;
    }
}

/// <summary> 蛊虫条件 - 检查玩家是否拥有特定蛊虫 </summary>
public class HasGuCondition : DialogueCondition
{
    public int GuItemType;
    public int MinCount = 1;

    public HasGuCondition() { }

    public HasGuCondition(int guItemType, int minCount = 1)
    {
        GuItemType = guItemType;
        MinCount = minCount;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        return player.CountItem(GuItemType) >= MinCount;
    }
}

/// <summary> 时间条件 - 检查当前游戏时间 </summary>
public class TimeCondition : DialogueCondition
{
    public enum TimePeriod { Day, Night, Dawn, Dusk, Any }
    public TimePeriod RequiredPeriod;

    public TimeCondition() { }

    public TimeCondition(TimePeriod requiredPeriod)
    {
        RequiredPeriod = requiredPeriod;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        double time = Main.time;
        bool isDay = Main.dayTime;

        return RequiredPeriod switch
        {
            TimePeriod.Day => isDay,
            TimePeriod.Night => !isDay,
            TimePeriod.Dawn => isDay && time < 10000,
            TimePeriod.Dusk => !isDay && time < 10000,
            TimePeriod.Any => true,
            _ => true
        };
    }
}

/// <summary> 修为差距条件 - 检查玩家与NPC的修为差距 </summary>
public class RealmGapCondition : DialogueCondition
{
    public enum GapDirection { PlayerHigher, NPCHigher, WithinRange }
    public GapDirection Direction;
    public int MaxGap = 3; // 最大差距等级数

    public RealmGapCondition() { }

    public RealmGapCondition(GapDirection direction, int maxGap = 3)
    {
        Direction = direction;
        MaxGap = maxGap;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        var qiRealm = player.GetModPlayer<QiRealmPlayer>();
        int playerLevel = qiRealm.GuLevel;

        int npcLevel = 0;
        if (npc.ModNPC is GuMasterBase guMaster)
        {
            npcLevel = (int)guMaster.GetRank();
        }

        int gap = playerLevel - npcLevel;

        return Direction switch
        {
            GapDirection.PlayerHigher => gap >= MaxGap,
            GapDirection.NPCHigher => -gap >= MaxGap,
            GapDirection.WithinRange => System.Math.Abs(gap) <= MaxGap,
            _ => false
        };
    }
}

/// <summary> 任务完成条件 - 检查特定任务是否已完成 </summary>
public class QuestCompletedCondition : DialogueCondition
{
    public string QuestName;

    public QuestCompletedCondition() { }

    public QuestCompletedCondition(string questName)
    {
        QuestName = questName;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        // 任务系统预留 - 后续集成到 QuestSystem
        // 目前返回 false，表示任务未完成
        return false;
    }
}

/// <summary> 性格条件 - 检查NPC的性格类型 </summary>
public class PersonalityCondition : DialogueCondition
{
    public GuPersonality RequiredPersonality;

    public PersonalityCondition() { }

    public PersonalityCondition(GuPersonality requiredPersonality)
    {
        RequiredPersonality = requiredPersonality;
    }

    public override bool Evaluate(Player player, NPC npc, BeliefState belief)
    {
        if (npc.ModNPC is GuMasterBase guMaster)
        {
            return guMaster.GetPersonality() == RequiredPersonality;
        }
        return false;
    }
}
