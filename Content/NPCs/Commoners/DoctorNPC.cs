using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class DoctorNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "郎中";

        protected override bool HasShop => true;
        protected override string ShopName => "DoctorShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "郎中把着脉：\"嗯...脉象平稳，身体不错。\"";
            if (talkCount <= 3) return "郎中翻开药方：\"这味药得用文火慢熬三个时辰。\"";
            return "郎中递给你一包药：\"拿着，以备不时之需。\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("DoctorShop");
            shop.AddItem(ItemID.HealingPotion, 8);
            shop.AddItem(ItemID.GreaterHealingPotion, 20);
            shop.AddItem(ItemID.RegenerationPotion, 15);
            shop.AddItem(ItemID.IronskinPotion, 15);
            shop.AddItem(ItemID.LifeforcePotion, 25);
            shop.AddItem(ItemID.EndurancePotion, 25);
            shop.AddItem(ItemID.HeartreachPotion, 25);
            shop.AddItem(ItemID.BandofRegeneration, 100);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.SilverCoin, 10, 30);
            loot.AddItem(ItemID.HealingPotion, 1, 3, 0.5f);
            loot.AddItem(ItemID.RegenerationPotion, 1, 2, 0.3f);
            loot.AddItem(ItemID.BandofRegeneration, 1, 1, 0.1f);
            Behaviors.Add(loot);
        }
    }
}