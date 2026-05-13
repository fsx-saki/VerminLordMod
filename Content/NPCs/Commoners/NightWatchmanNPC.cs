using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class NightWatchmanNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "更夫";

        protected override bool HasShop => true;
        protected override string ShopName => "NightWatchmanShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "更夫敲着梆子：\"天干物燥，小心火烛——\"";
            if (talkCount <= 3) return "更夫警惕地环顾四周：\"夜里不太平，你最好别在外面乱逛。\"";
            return "更夫递给你一支火把：\"拿着，夜里用得着。\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("NightWatchmanShop");
            shop.AddItem(ItemID.Torch, 1);
            shop.AddItem(ItemID.Glowstick, 2);
            shop.AddItem(ItemID.Flare, 3);
            shop.AddItem(ItemID.Bell, 25);
            shop.AddItem(ItemID.NightOwlPotion, 10);
            shop.AddItem(ItemID.ShinePotion, 10);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.CopperCoin, 30, 80);
            loot.AddItem(ItemID.Torch, 5, 15, 0.6f);
            loot.AddItem(ItemID.Glowstick, 3, 8, 0.4f);
            Behaviors.Add(loot);
        }
    }
}