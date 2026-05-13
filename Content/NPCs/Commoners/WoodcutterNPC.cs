using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class WoodcutterNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "樵夫";

        protected override bool HasShop => true;
        protected override string ShopName => "WoodcutterShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "樵夫放下斧头：\"呼——今天砍了不少好木头。\"";
            if (talkCount <= 3) return "樵夫擦了擦汗：\"深山里的木材最好，就是路不好走。\"";
            return "樵夫递给你一块木柴：\"拿回去生火用。\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("WoodcutterShop");
            shop.AddItem(ItemID.Wood, 1);
            shop.AddItem(ItemID.BorealWood, 1);
            shop.AddItem(ItemID.RichMahogany, 2);
            shop.AddItem(ItemID.Ebonwood, 2);
            shop.AddItem(ItemID.Shadewood, 2);
            shop.AddItem(ItemID.PalmWood, 2);
            shop.AddItem(ItemID.WoodenSword, 20);
            shop.AddItem(ItemID.WoodenBow, 20);
            shop.AddItem(ItemID.WoodenHammer, 15);
            shop.AddItem(ItemID.Acorn, 1);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.CopperCoin, 50, 100);
            loot.AddItem(ItemID.Wood, 10, 30, 0.8f);
            loot.AddItem(ItemID.BorealWood, 5, 15, 0.5f);
            loot.AddItem(ItemID.Acorn, 3, 8, 0.4f);
            Behaviors.Add(loot);
        }
    }
}