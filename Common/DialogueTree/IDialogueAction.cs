using System.Collections.Generic;
using Terraria;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.DialogueTree
{
    /// <summary>
    /// 通用动作上下文 — 动作执行时的环境信息。
    /// </summary>
    public class DialogueActionContext
    {
        public Player Player;
        public NPC NPC;
        public BeliefState Belief;
        public DialogueNode CurrentNode;
        public DialogueTree CurrentTree;
        public IDialogueIntegration Integration;

        public DialogueActionContext(Player player, NPC npc, BeliefState belief,
            DialogueNode currentNode, DialogueTree currentTree,
            IDialogueIntegration integration)
        {
            Player = player;
            NPC = npc;
            Belief = belief;
            CurrentNode = currentNode;
            CurrentTree = currentTree;
            Integration = integration;
        }
    }

    /// <summary>
    /// 动作执行结果 — 动作执行后返回给管理器的信息。
    /// </summary>
    public class DialogueActionResult
    {
        /// <summary> 是否成功执行 </summary>
        public bool Success;

        /// <summary> 执行后 NPC 说的文本（null 表示不改变当前文本） </summary>
        public string NPCText;

        /// <summary> 执行后是否结束对话 </summary>
        public bool EndsDialogue;

        /// <summary> 执行后是否触发战斗 </summary>
        public bool TriggersCombat;

        /// <summary> 执行后是否打开商店 </summary>
        public string OpensShop;

        /// <summary> 执行后要显示的消息 </summary>
        public string Message;

        /// <summary> 执行后要添加的临时选项（用于"被发现后"的辩解等） </summary>
        public List<DialogueOption> ExtraOptions;

        public static DialogueActionResult SuccessResult(string npcText = null)
        {
            return new DialogueActionResult { Success = true, NPCText = npcText };
        }

        public static DialogueActionResult FailResult(string npcText = null)
        {
            return new DialogueActionResult { Success = false, NPCText = npcText };
        }

        public static DialogueActionResult CombatResult(string npcText = null)
        {
            return new DialogueActionResult
            {
                Success = true,
                NPCText = npcText,
                TriggersCombat = true,
                EndsDialogue = true
            };
        }
    }

    /// <summary>
    /// 通用动作接口 — 任何对话中都可用的高自由度交互。
    ///
    /// 设计意图：
    /// - 为所有对话注入"偷窃、背刺、贿赂、触摸"等通用动作
    /// - 动作的可用性和效果取决于 NPC 类型、玩家属性、信念状态
    /// - 与 DialogueTree 正交，不依赖具体对话树结构
    ///
    /// 层级位置：Layer 5 — Meta-Action Layer（元动作层）
    /// 上层依赖：DialogueTreeUI（显示动作栏）
    /// 下层依赖：DialogueActionContext, DialogueActionResult
    /// </summary>
    public interface IDialogueAction
    {
        /// <summary> 动作唯一标识 </summary>
        string ActionID { get; }

        /// <summary> 动作显示名称（如"偷窃"） </summary>
        string DisplayName { get; }

        /// <summary> 动作描述（tooltip） </summary>
        string Description { get; }

        /// <summary> 动作类型（决定颜色/图标） </summary>
        DialogueOptionType OptionType { get; }

        /// <summary> 此动作是否在当前上下文中可用 </summary>
        bool IsAvailable(DialogueActionContext context);

        /// <summary> 执行动作，返回结果 </summary>
        DialogueActionResult Execute(DialogueActionContext context);
    }
}