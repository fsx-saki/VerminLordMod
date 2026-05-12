using System.Collections.Generic;
using VerminLordMod.Common.DialogueTree;

namespace VerminLordMod.Common.NPCBehaviors
{
    public enum ActionCategory
    {
        Beneficial,
        Harmful,
        Strange,
        Intimate
    }

    public static class FreeActionRegistry
    {
        private static readonly Dictionary<ActionCategory, List<IDialogueAction>> _actions = new()
        {
            { ActionCategory.Beneficial, new List<IDialogueAction>() },
            { ActionCategory.Harmful, new List<IDialogueAction>() },
            { ActionCategory.Strange, new List<IDialogueAction>() },
            { ActionCategory.Intimate, new List<IDialogueAction>() }
        };

        private static bool _initialized;

        public static string LastActionResultText { get; set; }
        public static bool LastActionEndsDialogue { get; set; }
        public static bool LastActionTriggersCombat { get; set; }

        public static void Register(ActionCategory category, IDialogueAction action)
        {
            _actions[category].Add(action);
        }

        public static IReadOnlyList<IDialogueAction> GetActions(ActionCategory category)
        {
            EnsureInitialized();
            return _actions[category];
        }

        public static List<IDialogueAction> GetAvailableActions(ActionCategory category, DialogueActionContext context)
        {
            EnsureInitialized();
            var result = new List<IDialogueAction>();
            foreach (var action in _actions[category])
            {
                if (action.IsAvailable(context))
                    result.Add(action);
            }
            return result;
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            Register(ActionCategory.Beneficial, new DialogueTree.Actions.BribeAction());
            Register(ActionCategory.Beneficial, new DialogueTree.Actions.InspectAction());
            Register(ActionCategory.Beneficial, new DialogueTree.Actions.HealAction());
            Register(ActionCategory.Beneficial, new DialogueTree.Actions.GuideAction());
            Register(ActionCategory.Beneficial, new DialogueTree.Actions.VouchAction());

            Register(ActionCategory.Harmful, new DialogueTree.Actions.StealAction());
            Register(ActionCategory.Harmful, new DialogueTree.Actions.BackstabAction());
            Register(ActionCategory.Harmful, new DialogueTree.Actions.ProvokeAction());
            Register(ActionCategory.Harmful, new DialogueTree.Actions.PoisonAction());
            Register(ActionCategory.Harmful, new DialogueTree.Actions.BlackmailAction());
            Register(ActionCategory.Harmful, new DialogueTree.Actions.FrameAction());
            Register(ActionCategory.Harmful, new DialogueTree.Actions.IntimidateAction());

            Register(ActionCategory.Strange, new DialogueTree.Actions.TouchAction());
            Register(ActionCategory.Strange, new DialogueTree.Actions.SniffAction());
            Register(ActionCategory.Strange, new DialogueTree.Actions.StareAction());
            Register(ActionCategory.Strange, new DialogueTree.Actions.CircleAction());
            Register(ActionCategory.Strange, new DialogueTree.Actions.MimicAction());
            Register(ActionCategory.Strange, new DialogueTree.Actions.CollectAction());
            Register(ActionCategory.Strange, new DialogueTree.Actions.WhisperAction());

            Register(ActionCategory.Intimate, new DialogueTree.Actions.FlirtAction());
            Register(ActionCategory.Intimate, new DialogueTree.Actions.HugAction());
            Register(ActionCategory.Intimate, new DialogueTree.Actions.GiftAction());
            Register(ActionCategory.Intimate, new DialogueTree.Actions.HoldHandAction());
            Register(ActionCategory.Intimate, new DialogueTree.Actions.EarWhisperAction());
            Register(ActionCategory.Intimate, new DialogueTree.Actions.CombHairAction());
            Register(ActionCategory.Intimate, new DialogueTree.Actions.ShareDrinkAction());
        }

        public static void Unload()
        {
            foreach (var list in _actions.Values)
                list.Clear();
            _initialized = false;
            LastActionResultText = null;
        }
    }
}