using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class ScholarNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "书生";

        protected override bool HasShop => true;
        protected override string ShopName => "ScholarShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "书生放下书卷：\"有朋自远方来，不亦乐乎？\"";
            if (talkCount <= 3) return "书生若有所思：\"书中自有黄金屋，书中自有颜如玉。\"";
            return "书生递给你一本书：\"这本《蛊经入门》送你了。\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("ScholarShop");
            shop.AddItem(ItemID.Book, 10);
            shop.AddItem(ItemID.SpellTome, 50);
            shop.AddItem(ItemID.WaterCandle, 25);
            shop.AddItem(ItemID.PeaceCandle, 25);
            shop.AddItem(ItemID.CrystalBall, 50);
            shop.AddItem(ItemID.ManaCrystal, 100);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.SilverCoin, 5, 15);
            loot.AddItem(ItemID.Book, 1, 2, 0.5f);
            loot.AddItem(ItemID.FallenStar, 1, 3, 0.3f);
            Behaviors.Add(loot);
        }
    }
}