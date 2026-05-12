using System;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Common.NPCBehaviors
{
    public class DialogueBehavior : BaseNPCBehavior
    {
        public override string Name => "DialogueBehavior";

        private readonly string _shopName;
        private readonly Func<int, string> _getDialogue;
        private readonly bool _hasShop;

        public int NumberOfTimesTalkedTo { get; set; }

        public DialogueBehavior(Func<int, string> getDialogue, bool hasShop = false, string shopName = null)
        {
            _getDialogue = getDialogue;
            _hasShop = hasShop;
            _shopName = shopName;
        }

        public override string GetChat(NPC npc)
        {
            NumberOfTimesTalkedTo++;
            return _getDialogue(NumberOfTimesTalkedTo);
        }

        public override void SetChatButtons(NPC npc, ref string button, ref string button2)
        {
            button = "对话";
            if (_hasShop)
                button2 = "商店";
        }

        public override void OnChatButtonClicked(NPC npc, bool firstButton, ref string shop)
        {
            if (!firstButton && _hasShop && !string.IsNullOrEmpty(_shopName))
            {
                shop = _shopName;
            }
        }
    }
}