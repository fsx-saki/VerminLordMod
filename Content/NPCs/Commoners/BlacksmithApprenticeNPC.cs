using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class BlacksmithApprenticeNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "铁匠学徒";

        protected override bool HasShop => true;
        protected override string ShopName => "BlacksmithApprenticeShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "铁匠学徒擦着汗：\"师父让我先打一百个铁钉……才打到第三十个。\"";
            if (talkCount <= 3) return "铁匠学徒举起锤子又放下：\"打铁真是力气活，但师父说这是基本功。\"";
            return "铁匠学徒悄悄说：\"等师父不在，我可以偷偷帮你打点东西……不过可别说是我做的！\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("BlacksmithApprenticeShop");
            shop.AddItem(ItemID.CopperBroadsword, 80);
            shop.AddItem(ItemID.CopperPickaxe, 60);
            shop.AddItem(ItemID.CopperAxe, 60);
            shop.AddItem(ItemID.CopperHammer, 50);
            shop.AddItem(ItemID.IronBar, 30);
            shop.AddItem(ItemID.CopperBar, 15);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.SilverCoin, 10, 25);
            loot.AddItem(ItemID.CopperBar, 2, 5, 0.4f);
            loot.AddItem(ItemID.IronOre, 3, 6, 0.3f);
            loot.AddItem(ItemID.CopperOre, 5, 10, 0.4f);
            Behaviors.Add(loot);
        }
    }
}
