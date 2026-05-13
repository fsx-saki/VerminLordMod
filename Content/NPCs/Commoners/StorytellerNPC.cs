using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class StorytellerNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "说书人";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "说书人摇着折扇：\"客官，可曾听过南疆蛊师的传说？\"";
            if (talkCount <= 3) return "说书人清了清嗓子：\"话说当年，古月先祖以一己之力……\"";
            return "说书人神秘兮兮地说：\"你可知道，青茅山深处藏着一个天大的秘密……\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var loot = new LootBehavior(ItemID.SilverCoin, 5, 20);
            loot.AddItem(ItemID.Book, 1, 2, 0.2f);
            loot.AddItem(ItemID.Tombstone, 1, 1, 0.05f);
            Behaviors.Add(loot);
        }
    }
}
