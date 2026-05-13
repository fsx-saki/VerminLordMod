using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class FarmerNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "农夫";

        protected override bool HasShop => true;
        protected override string ShopName => "FarmerShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "农夫擦了擦汗：\"欢迎来到我的农场！今年的收成不错。\"";
            if (talkCount <= 3) return "农夫笑着说：\"种地虽然辛苦，但看着庄稼一天天长大，心里踏实。\"";
            return "农夫递给你一个新鲜的萝卜：\"尝尝，刚摘的。\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("FarmerShop");
            shop.AddItem(ItemID.DaybloomSeeds, 5);
            shop.AddItem(ItemID.BlinkrootSeeds, 5);
            shop.AddItem(ItemID.MoonglowSeeds, 5);
            shop.AddItem(ItemID.WaterleafSeeds, 5);
            shop.AddItem(ItemID.DeathweedSeeds, 5);
            shop.AddItem(ItemID.FireblossomSeeds, 5);
            shop.AddItem(ItemID.ShiverthornSeeds, 5);
            shop.AddItem(ItemID.PumpkinSeed, 2);
            shop.AddItem(ItemID.GrassSeeds, 2);
            shop.AddItem(ItemID.Acorn, 1);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.SilverCoin, 5, 20);
            loot.AddItem(ItemID.Pumpkin, 1, 3, 0.5f);
            loot.AddItem(ItemID.Daybloom, 1, 3, 0.4f);
            loot.AddItem(ItemID.Mushroom, 1, 5, 0.3f);
            Behaviors.Add(loot);
        }
    }
}