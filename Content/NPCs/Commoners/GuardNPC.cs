using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    public class GuardNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "守卫";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "守卫挺直身板：\"站住！报上名来……哦，是你啊。\"";
            if (talkCount <= 3) return "守卫警惕地扫视四周：\"最近山外不太平，巡逻要加强。\"";
            return "守卫低声说：\"夜间巡逻时，我总觉得山里有什么东西在窥视我们……\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var loot = new LootBehavior(ItemID.SilverCoin, 15, 40);
            loot.AddItem(ItemID.IronBar, 1, 3, 0.3f);
            loot.AddItem(ItemID.Spear, 1, 1, 0.1f);
            loot.AddItem(ItemID.IronHelmet, 1, 1, 0.05f);
            Behaviors.Add(loot);
        }
    }
}
