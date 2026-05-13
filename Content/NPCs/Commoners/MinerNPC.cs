using System;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;

namespace VerminLordMod.Content.NPCs.Commoners
{
    [AutoloadHead]
    public class MinerNPC : CommonerNPCBase
    {
        protected override string ProfessionName => "矿工";

        protected override bool HasShop => true;
        protected override string ShopName => "MinerShop";

        protected override Func<int, string> GetDialogueFunc() => (talkCount) =>
        {
            if (talkCount == 1) return "矿工拍了拍身上的灰：\"刚从矿洞里出来，今天挖到好东西了！\"";
            if (talkCount <= 3) return "矿工神秘地说：\"地下深处有闪闪发光的东西...\"";
            return "矿工递给你一块矿石：\"这块成色不错，送你了。\"";
        };

        protected override void RegisterProfessionBehaviors()
        {
            var shop = new ShopBehavior("MinerShop");
            shop.AddItem(ItemID.CopperOre, 5);
            shop.AddItem(ItemID.IronOre, 10);
            shop.AddItem(ItemID.SilverOre, 15);
            shop.AddItem(ItemID.GoldOre, 25);
            shop.AddItem(ItemID.CopperPickaxe, 50);
            shop.AddItem(ItemID.IronPickaxe, 100);
            shop.AddItem(ItemID.Torch, 1);
            shop.AddItem(ItemID.Glowstick, 2);
            shop.AddItem(ItemID.Rope, 1);
            Behaviors.Add(shop);

            var loot = new LootBehavior(ItemID.SilverCoin, 15, 40);
            loot.AddItem(ItemID.IronOre, 3, 8, 0.6f);
            loot.AddItem(ItemID.SilverOre, 2, 5, 0.4f);
            loot.AddItem(ItemID.GoldOre, 1, 3, 0.2f);
            loot.AddItem(ItemID.StoneBlock, 10, 30, 0.5f);
            Behaviors.Add(loot);
        }
    }
}