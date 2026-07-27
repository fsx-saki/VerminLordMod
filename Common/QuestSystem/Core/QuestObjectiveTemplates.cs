using Terraria;

namespace VerminLordMod.Common.QuestSystem.Core
{
    public enum QuestObjectiveDescriptionStyle { Custom, DefeatNpc, ObtainItem, CollectItem }

    internal static class QuestObjectiveTemplates
    {
        public static string Format(QuestObjective objective)
        {
            switch (objective.DescriptionStyle)
            {
                case QuestObjectiveDescriptionStyle.DefeatNpc:
                    if (objective.TargetNpcID <= 0) return string.Empty;
                    return QuestLog.ObjectiveTemplateDefeatNpc
                        .WithFormatArgs(Lang.GetNPCNameValue(objective.TargetNpcID)).Value;

                case QuestObjectiveDescriptionStyle.ObtainItem:
                    if (objective.TargetItemID <= 0) return string.Empty;
                    return QuestLog.ObjectiveTemplateObtainItem
                        .WithFormatArgs(Lang.GetItemNameValue(objective.TargetItemID)).Value;

                case QuestObjectiveDescriptionStyle.CollectItem:
                    if (objective.TargetItemID <= 0 || objective.RequiredProgress <= 0) return string.Empty;
                    return QuestLog.ObjectiveTemplateCollectItem
                        .WithFormatArgs(objective.RequiredProgress, Lang.GetItemNameValue(objective.TargetItemID)).Value;

                default:
                    return objective.Description?.Value ?? string.Empty;
            }
        }
    }
}
