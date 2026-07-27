using System.Collections.Generic;

namespace VerminLordMod.Common.QuestSystem.Dialogue
{
    public class DialogueNode
    {
        public string Id { get; init; }
        public List<string> SystemLines { get; init; } = [];
        public string ChoiceText { get; init; } = "";
        public string NextNodeId { get; init; } = "";
        public bool IsEnd => string.IsNullOrEmpty(NextNodeId);
    }
}
