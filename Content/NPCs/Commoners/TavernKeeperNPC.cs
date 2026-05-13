using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class TavernKeeperNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "酒馆老板";

        protected override bool HasShop => true;
        protected override string ShopName => "TavernKeeperShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "酒馆老板擦着杯子：\"欢迎光临！来一杯麦酒暖暖身子？\"";
            if (talkCount <= 3) return "酒馆老板压低声音：\"最近听到不少有趣的消息...\"";
            return "酒馆老板哈哈大笑：\"再来一杯，算我请的！\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("TavernKeeperShop");
            shop.AddItem(ItemID.Ale, 5);
            shop.AddItem(ItemID.Sake, 5);
            shop.AddItem(ItemID.BowlofSoup, 10);
            shop.AddItem(ItemID.CookedFish, 10);
            shop.AddItem(ItemID.CookedShrimp, 10);
            shop.AddItem(ItemID.BunnyStew, 15);
            shop.AddItem(ItemID.Mug, 2);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.SilverCoin, 10, 30);
            loot.AddItem(ItemID.Ale, 1, 3, 0.5f);
            loot.AddItem(ItemID.BowlofSoup, 1, 2, 0.4f);
            loot.AddItem(ItemID.Mug, 1, 1, 0.3f);
            Behaviors.Add(loot);
        }
    }
}