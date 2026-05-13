using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class WeaverNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "织工";

        protected override bool HasShop => true;
        protected override string ShopName => "WeaverShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "织工灵巧地穿梭着丝线：\"好布料需要好蚕丝，好蚕丝需要好桑叶。\"";
            if (talkCount <= 3) return "织工展示着布匹：\"这块绸缎，是给族长做衣裳用的。\"";
            return "织工低声说：\"青茅山的蚕丝品质上乘，外面的人可买不到。\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("WeaverShop");
            shop.AddItem(ItemID.Silk, 30);
            shop.AddItem(ItemID.Cobweb, 5);
            shop.AddItem(ItemID.WhiteString, 50);
            shop.AddItem(ItemID.BlackString, 50);
            shop.AddItem(ItemID.Rope, 20);
            shop.AddItem(ItemID.SilkRope, 80);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.SilverCoin, 10, 30);
            loot.AddItem(ItemID.Silk, 3, 8, 0.5f);
            loot.AddItem(ItemID.Cobweb, 5, 15, 0.6f);
            loot.AddItem(ItemID.WhiteString, 1, 2, 0.2f);
            Behaviors.Add(loot);
        }
    }
}
