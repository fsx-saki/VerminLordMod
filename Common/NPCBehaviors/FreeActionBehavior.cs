using System.Collections.Generic;
using Terraria;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.UI.DialogueTreeUI;
using VerminLordMod.Content.NPCs.GuMasters;
using DTree = VerminLordMod.Common.DialogueTree.DialogueTree;

namespace VerminLordMod.Common.NPCBehaviors
{
    public class FreeActionBehavior : BaseNPCBehavior
    {
        public override string Name => "FreeActionBehavior";

        private const string TREE_ID_PREFIX = "__free_action_";
        private const string ROOT_NODE = "root";
        private const string RESULT_NODE = "action_result";

        public override void SetChatButtons(NPC npc, ref string button, ref string button2)
        {
            button = "自由行动";
            button2 = "商店";
        }

        public override void OnChatButtonClicked(NPC npc, bool firstButton, ref string shop)
        {
            if (!firstButton) return;

            var player = Main.LocalPlayer;
            if (player == null || !player.active) return;

            var tree = BuildFreeActionTree(npc, player);
            var treeID = TREE_ID_PREFIX + npc.whoAmI;

            var manager = DialogueTreeManager.Instance;
            manager.RegisterTree(tree);
            tree.TreeID = treeID;

            if (manager.StartDialogue(npc, player))
            {
                var options = manager.GetCurrentOptions();
                string npcName = npc.GivenName ?? npc.TypeName;
                DialogueTreeUI.Instance.Open(npcName, npc.type, manager.GetCurrentNPCText(), options);
                Main.npcChatText = "";
            }
        }

        private DTree BuildFreeActionTree(NPC npc, Player player)
        {
            var tree = new DTree
            {
                TreeID = TREE_ID_PREFIX + npc.whoAmI,
                RootNodeID = ROOT_NODE
            };

            var integration = DialogueIntegration.Instance;
            var belief = integration.GetBelief(player, npc);

            var rootContext = new DialogueActionContext(player, npc, belief, null, null, integration);

            var rootNode = new DialogueNode(ROOT_NODE, "你想做什么？");
            tree.Nodes[ROOT_NODE] = rootNode;

            AddCategoryOption(tree, rootNode, "有益行为", ActionCategory.Beneficial, rootContext,
                "[有益] 帮助、交易、治疗等友善行为", DialogueOptionType.Ally);
            AddCategoryOption(tree, rootNode, "有害行为", ActionCategory.Harmful, rootContext,
                "[有害] 偷窃、背刺、下毒等敌对行为", DialogueOptionType.Betray);
            AddCategoryOption(tree, rootNode, "怪异行为", ActionCategory.Strange, rootContext,
                "[怪异] 触摸、嗅探、凝视等奇怪行为", DialogueOptionType.Special);
            AddCategoryOption(tree, rootNode, "亲密行为", ActionCategory.Intimate, rootContext,
                "[亲密] 调戏、拥抱、送礼等亲密行为", DialogueOptionType.Social);

            rootNode.Options.Add(new DialogueOption("离开", null, DialogueOptionType.Exit));

            var resultNode = new DialogueNode(RESULT_NODE, "行动完成。");
            resultNode.Options.Add(new DialogueOption("继续", ROOT_NODE, DialogueOptionType.Normal));
            tree.Nodes[RESULT_NODE] = resultNode;

            return tree;
        }

        private static void AddCategoryOption(DTree tree, DialogueNode rootNode,
            string categoryName, ActionCategory category, DialogueActionContext rootContext,
            string tooltip, DialogueOptionType optionType)
        {
            string categoryNodeID = "cat_" + category.ToString().ToLower();
            var availableActions = FreeActionRegistry.GetAvailableActions(category, rootContext);

            if (availableActions.Count == 0) return;

            var categoryNode = new DialogueNode(categoryNodeID, $"选择一项{categoryName}：");
            tree.Nodes[categoryNodeID] = categoryNode;

            foreach (var action in availableActions)
            {
                string actionNodeID = "act_" + action.ActionID;
                var actionNode = new DialogueNode(actionNodeID, "行动完成。");
                actionNode.Options.Add(new DialogueOption("继续", ROOT_NODE, DialogueOptionType.Normal));
                tree.Nodes[actionNodeID] = actionNode;

                var option = new DialogueOption(action.DisplayName, actionNodeID, action.OptionType)
                {
                    Tooltip = action.Description
                };
                option.Effects.Add(new ExecuteFreeActionEffect(action, rootContext, actionNode));
                categoryNode.Options.Add(option);
            }

            categoryNode.Options.Add(new DialogueOption("返回", ROOT_NODE, DialogueOptionType.Exit));

            var rootOption = new DialogueOption(categoryName, categoryNodeID, optionType)
            {
                Tooltip = tooltip
            };
            rootNode.Options.Add(rootOption);
        }
    }

    internal class ExecuteFreeActionEffect : DialogueEffect
    {
        private readonly IDialogueAction _action;
        private readonly DialogueActionContext _context;
        private readonly DialogueNode _resultNode;

        public ExecuteFreeActionEffect(IDialogueAction action, DialogueActionContext context, DialogueNode resultNode)
        {
            _action = action;
            _context = context;
            _resultNode = resultNode;
        }

        public override void Execute(Player player, NPC npc, BeliefState belief)
        {
            var result = _action.Execute(_context);

            if (_resultNode != null)
            {
                _resultNode.NPCText = result.NPCText ?? "行动完成。";
            }

            if (result.TriggersCombat)
            {
                DialogueTreeUI.Instance.Close();
                DialogueTreeManager.Instance.EndDialogue();
                Main.LocalPlayer.SetTalkNPC(-1);
            }

            if (result.EndsDialogue && !result.TriggersCombat)
            {
                _resultNode.Options.Clear();
                _resultNode.Options.Add(new DialogueOption("离开", null, DialogueOptionType.Exit));
            }
        }
    }
}