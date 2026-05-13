using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class FishermanNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "渔夫";

        protected override bool HasShop => true;
        protected override string ShopName => "FishermanShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "渔夫收起鱼竿：\"今天运气不错，钓到一条大的！\"";
            if (talkCount <= 3) return "渔夫指着水面：\"那边的鱼特别多，我一般不告诉别人。\"";
            return "渔夫递给你一条鱼：\"拿回去炖汤，鲜得很。\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("FishermanShop");
            shop.AddItem(ItemID.WoodFishingPole, 50);
            shop.AddItem(ItemID.ReinforcedFishingPole, 150);
            shop.AddItem(ItemID.ApprenticeBait, 5);
            shop.AddItem(ItemID.JourneymanBait, 10);
            shop.AddItem(ItemID.MasterBait, 20);
            shop.AddItem(ItemID.FishingBobber, 25);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.SilverCoin, 5, 20);
            loot.AddItem(ItemID.Bass, 1, 3, 0.6f);
            loot.AddItem(ItemID.Trout, 1, 2, 0.4f);
            loot.AddItem(ItemID.Salmon, 1, 2, 0.3f);
            loot.AddItem(ItemID.ApprenticeBait, 1, 5, 0.3f);
            Behaviors.Add(loot);
        }
    }
}