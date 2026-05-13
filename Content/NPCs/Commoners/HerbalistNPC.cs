using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class HerbalistNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "药师";

        protected override bool HasShop => true;
        protected override string ShopName => "HerbalistShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "药师嗅了嗅手中的草药：\"嗯...这株月光草品质不错。\"";
            if (talkCount <= 3) return "药师递给你一瓶药水：\"这是我新配的方子，试试效果。\"";
            return "药师认真地说：\"是药三分毒，用药需谨慎。\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("HerbalistShop");
            shop.AddItem(ItemID.HealingPotion, 8);
            shop.AddItem(ItemID.ManaPotion, 4);
            shop.AddItem(ItemID.LesserHealingPotion, 3);
            shop.AddItem(ItemID.LesserManaPotion, 2);
            shop.AddItem(ItemID.IronskinPotion, 15);
            shop.AddItem(ItemID.SwiftnessPotion, 15);
            shop.AddItem(ItemID.RegenerationPotion, 15);
            shop.AddItem(ItemID.NightOwlPotion, 10);
            shop.AddItem(ItemID.ShinePotion, 10);
            shop.AddItem(ItemID.HunterPotion, 10);
            shop.AddItem(ItemID.BattlePotion, 20);
            shop.AddItem(ItemID.Bottle, 1);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.SilverCoin, 10, 30);
            loot.AddItem(ItemID.Daybloom, 2, 5, 0.5f);
            loot.AddItem(ItemID.Moonglow, 2, 5, 0.5f);
            loot.AddItem(ItemID.Blinkroot, 2, 5, 0.4f);
            loot.AddItem(ItemID.HealingPotion, 1, 3, 0.3f);
            Behaviors.Add(loot);
        }
    }
}