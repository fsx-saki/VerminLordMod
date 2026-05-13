using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class MerchantNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "商人";

        protected override bool HasShop => true;
        protected override string ShopName => "MerchantShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "商人搓着手：\"欢迎欢迎！本店货品齐全，价格公道！\"";
            if (talkCount <= 3) return "商人压低声音：\"我这有些稀罕玩意儿，要不要看看？\"";
            return "商人笑着说：\"老顾客了，给你打个折。\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("MerchantShop");
            shop.AddItem(ItemID.Torch, 1);
            shop.AddItem(ItemID.Rope, 1);
            shop.AddItem(ItemID.Glowstick, 2);
            shop.AddItem(ItemID.RecallPotion, 15);
            shop.AddItem(ItemID.HealingPotion, 10);
            shop.AddItem(ItemID.ManaPotion, 5);
            shop.AddItem(ItemID.Bomb, 10);
            shop.AddItem(ItemID.BugNet, 25);
            shop.AddItem(ItemID.Umbrella, 50);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.GoldCoin, 1, 3);
            loot.AddItem(ItemID.SilverCoin, 20, 50, 0.8f);
            loot.AddItem(ItemID.HealingPotion, 1, 3, 0.3f);
            Behaviors.Add(loot);
        }
    }
}