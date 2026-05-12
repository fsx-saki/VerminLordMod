using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace VerminLordMod.Common.NPCBehaviors
{
    public class ShopBehavior : BaseNPCBehavior
    {
        public override string Name => "ShopBehavior";

        private readonly string _shopName;
        private readonly List<ShopItem> _items;
        private bool _registered;

        public ShopBehavior(string shopName)
        {
            _shopName = shopName;
            _items = new List<ShopItem>();
        }

        public ShopBehavior AddItem(int itemType, int? customPrice = null, int? specialCurrency = null)
        {
            _items.Add(new ShopItem { ItemType = itemType, CustomPrice = customPrice, SpecialCurrency = specialCurrency });
            return this;
        }

        public override void OnChatButtonClicked(NPC npc, bool firstButton, ref string shop)
        {
            if (!firstButton)
                shop = _shopName;
        }

        public override void AddShops(NPC npc)
        {
            if (_registered) return;
            _registered = true;

            if (_items.Count == 0)
            {
                var emptyShop = new NPCShop(npc.type, _shopName);
                emptyShop.Register();
                return;
            }

            var shop = new NPCShop(npc.type, _shopName);
            foreach (var item in _items)
            {
                var itemObj = new Item(item.ItemType);
                if (item.CustomPrice.HasValue)
                    itemObj.shopCustomPrice = item.CustomPrice.Value;
                if (item.SpecialCurrency.HasValue)
                    itemObj.shopSpecialCurrency = item.SpecialCurrency.Value;
                shop.Add(itemObj);
            }
            shop.Register();
        }

        private struct ShopItem
        {
            public int ItemType;
            public int? CustomPrice;
            public int? SpecialCurrency;
        }
    }
}