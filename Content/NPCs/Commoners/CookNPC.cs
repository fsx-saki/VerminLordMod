using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class CookNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "厨师";

        protected override bool HasShop => true;
        protected override string ShopName => "CookShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "厨师热情地说：\"来来来，尝尝我的手艺！\"";
            if (talkCount <= 3) return "厨师搅拌着大锅：\"好汤需要好食材和好火候，缺一不可。\"";
            return "厨师端出一碗汤：\"这碗蛇羹是青茅山的特色，喝了能补真元！\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("CookShop");
            shop.AddItem(ItemID.CookedFish, 50);
            shop.AddItem(ItemID.CookedShrimp, 60);
            shop.AddItem(ItemID.PadThai, 80);
            shop.AddItem(ItemID.CookedMarshmallow, 40);
            shop.AddItem(ItemID.PumpkinPie, 30);
            shop.AddItem(ItemID.BottledHoney, 70);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.SilverCoin, 10, 30);
            loot.AddItem(ItemID.Bowl, 1, 3, 0.3f);
            loot.AddItem(ItemID.Mushroom, 2, 5, 0.4f);
            loot.AddItem(ItemID.BottledHoney, 1, 2, 0.2f);
            Behaviors.Add(loot);
        }
    }
}
