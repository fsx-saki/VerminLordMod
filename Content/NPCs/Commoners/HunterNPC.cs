using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class HunterNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "猎人";

        protected override bool HasShop => true;
        protected override string ShopName => "HunterShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "猎人擦拭着弓箭：\"嘘——别出声，前面有猎物。\"";
            if (talkCount <= 3) return "猎人低声说：\"这片林子里的野狼越来越多了，你小心点。\"";
            return "猎人拍了拍你的肩膀：\"好猎手从不空手而归。\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("HunterShop");
            shop.AddItem(ItemID.WoodenArrow, 1);
            shop.AddItem(ItemID.FlamingArrow, 3);
            shop.AddItem(ItemID.UnholyArrow, 5);
            shop.AddItem(ItemID.JestersArrow, 10);
            shop.AddItem(ItemID.WoodenBow, 50);
            shop.AddItem(ItemID.BorealWoodBow, 75);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.SilverCoin, 10, 30);
            loot.AddItem(ItemID.Leather, 1, 3, 0.6f);
            loot.AddItem(ItemID.Bunny, 1, 1, 0.3f);
            loot.AddItem(ItemID.WoodenArrow, 10, 30, 0.5f);
            Behaviors.Add(loot);
        }
    }
}