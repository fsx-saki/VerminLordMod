using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class BlacksmithNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "铁匠";

        protected override bool HasShop => true;
        protected override string ShopName => "BlacksmithShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "铁匠挥舞着铁锤：\"叮叮当当——欢迎光临！需要什么武器？\"";
            if (talkCount <= 3) return "铁匠擦了擦汗：\"这把剑刚出炉，还热乎着呢。\"";
            return "铁匠自豪地说：\"我的手艺，方圆百里无人能及！\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("BlacksmithShop");
            shop.AddItem(ItemID.IronBroadsword, 150);
            shop.AddItem(ItemID.IronBow, 120);
            shop.AddItem(ItemID.IronHammer, 100);
            shop.AddItem(ItemID.IronAxe, 100);
            shop.AddItem(ItemID.IronPickaxe, 100);
            shop.AddItem(ItemID.IronHelmet, 80);
            shop.AddItem(ItemID.IronChainmail, 120);
            shop.AddItem(ItemID.IronGreaves, 100);
            shop.AddItem(ItemID.Chain, 10);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.SilverCoin, 20, 50);
            loot.AddItem(ItemID.IronBar, 2, 5, 0.5f);
            loot.AddItem(ItemID.IronOre, 3, 8, 0.4f);
            loot.AddItem(ItemID.Chain, 3, 8, 0.3f);
            Behaviors.Add(loot);
        }
    }
}