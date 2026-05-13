using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class AlchemistApprenticeNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "炼药学徒";

        protected override bool HasShop => true;
        protected override string ShopName => "AlchemistApprenticeShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "炼药学徒手忙脚乱地整理药材：\"啊！你、你好！药堂正缺人手呢。\"";
            if (talkCount <= 3) return "炼药学徒小心翼翼地研磨药材：\"师父说，炼药最重要的是耐心。\"";
            return "炼药学徒小声说：\"我偷偷试过炼丹……炸了三次了。\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("AlchemistApprenticeShop");
            shop.AddItem(ItemID.HealingPotion, 100);
            shop.AddItem(ItemID.ManaPotion, 100);
            shop.AddItem(ItemID.LesserHealingPotion, 40);
            shop.AddItem(ItemID.LesserManaPotion, 40);
            shop.AddItem(ItemID.RecallPotion, 80);
            shop.AddItem(ItemID.WormholePotion, 120);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.SilverCoin, 15, 40);
            loot.AddItem(ItemID.Daybloom, 2, 5, 0.4f);
            loot.AddItem(ItemID.Moonglow, 2, 5, 0.4f);
            loot.AddItem(ItemID.BottledWater, 3, 8, 0.3f);
            loot.AddItem(ItemID.Gel, 5, 10, 0.5f);
            Behaviors.Add(loot);
        }
    }
}
