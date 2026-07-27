using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.QuestSystem.Dialogue
{
    public class DialoguePlayer : ModPlayer
    {
        public string CurrentNodeId = "start";
        public List<string> UnlockedNodeIds = [];
        public Stack<string> History = new();
        public Item AnalysisSlot = new Item();
        public bool DialogueCompleted;
        public List<int> VisibleTabs = [0, 1]; // 默认主页(0)和系统(1)

        private DialogueTreeData Tree => DialogueLoader.LoadedTree;

        public DialogueNode CurrentNode =>
            string.IsNullOrEmpty(CurrentNodeId) ? null : Tree?.Get(CurrentNodeId);

        public bool IsDialogueComplete => DialogueCompleted || string.IsNullOrEmpty(CurrentNodeId);

        public bool CanGoBack => History.Count > 0;

        public bool Advance()
        {
            var node = CurrentNode;
            if (node == null) return false;

            if (!UnlockedNodeIds.Contains(CurrentNodeId))
                UnlockedNodeIds.Add(CurrentNodeId);

            History.Push(CurrentNodeId);
            CurrentNodeId = node.NextNodeId;

            if (string.IsNullOrEmpty(CurrentNodeId))
                DialogueCompleted = true;

            return !string.IsNullOrEmpty(CurrentNodeId);
        }

        public bool GoBack()
        {
            if (History.Count == 0) return false;
            CurrentNodeId = History.Pop();
            return true;
        }

        public void JumpToNode(string nodeId)
        {
            if (Tree?.Get(nodeId) != null)
            {
                if (CurrentNodeId != null)
                    History.Push(CurrentNodeId);
                CurrentNodeId = nodeId;
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["Dlg_Current"] = CurrentNodeId ?? "";
            tag["Dlg_Completed"] = DialogueCompleted;
            tag["Dlg_Unlocked"] = UnlockedNodeIds;
            tag["Dlg_VisibleTabs"] = VisibleTabs;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.TryGet("Dlg_Current", out string cur))
                CurrentNodeId = string.IsNullOrEmpty(cur) ? "start" : cur;
            DialogueCompleted = tag.GetBool("Dlg_Completed");
            if (tag.TryGet("Dlg_Unlocked", out List<string> unlocked))
                UnlockedNodeIds = unlocked ?? [];
            if (tag.TryGet("Dlg_VisibleTabs", out List<int> tabs))
                VisibleTabs = tabs ?? [0, 1];
            else
                VisibleTabs = [0, 1];
        }
    }
}
