using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class BeggarNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "乞丐";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "乞丐伸出破碗：\"行行好吧，给点吃的...\"";
            if (talkCount <= 3) return "乞丐叹了口气：\"这世道，连乞丐都不好当啊。\"";
            return "乞丐感激地看着你：\"好人一生平安。\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var loot = new LootBehavior(ItemID.CopperCoin, 1, 10);
            loot.AddItem(ItemID.Mushroom, 1, 2, 0.3f);
            Behaviors.Add(loot);
        }
    }
}