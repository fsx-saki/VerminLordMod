using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class TailorNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "裁缝";

        protected override bool HasShop => true;
        protected override string ShopName => "TailorShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "裁缝量着布料：\"欢迎！想做件新衣裳吗？\"";
            if (talkCount <= 3) return "裁缝展示着一匹丝绸：\"这可是上好的料子。\"";
            return "裁缝微笑着说：\"人靠衣装，佛靠金装。\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("TailorShop");
            shop.AddItem(ItemID.Silk, 5);
            shop.AddItem(ItemID.Cobweb, 1);
            shop.AddItem(ItemID.FamiliarWig, 25);
            shop.AddItem(ItemID.FamiliarShirt, 25);
            shop.AddItem(ItemID.FamiliarPants, 25);
            shop.AddItem(ItemID.RedDye, 10);
            shop.AddItem(ItemID.BlueDye, 10);
            shop.AddItem(ItemID.GreenDye, 10);
            shop.AddItem(ItemID.YellowDye, 10);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.SilverCoin, 5, 20);
            loot.AddItem(ItemID.Silk, 2, 5, 0.5f);
            loot.AddItem(ItemID.Cobweb, 5, 15, 0.4f);
            Behaviors.Add(loot);
        }
    }
}