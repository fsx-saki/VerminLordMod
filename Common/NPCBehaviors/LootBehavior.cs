using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.NPCBehaviors
{
    public class LootBehavior : BaseNPCBehavior
    {
        public override string Name => "LootBehavior";

        private readonly List<LootEntry> _lootEntries;
        private readonly int _coinType;
        private readonly int _coinMin;
        private readonly int _coinMax;

        public LootBehavior(int coinType = 0, int coinMin = 0, int coinMax = 0)
        {
            _lootEntries = new List<LootEntry>();
            _coinType = coinType;
            _coinMin = coinMin;
            _coinMax = coinMax;
        }

        public LootBehavior AddItem(int itemType, int minStack = 1, int maxStack = 1, float chance = 1f)
        {
            _lootEntries.Add(new LootEntry { ItemType = itemType, MinStack = minStack, MaxStack = maxStack, Chance = chance });
            return this;
        }

        public override void OnKill(NPC npc)
        {
            if (Main.netMode == NetmodeID.Server) return;

            foreach (var entry in _lootEntries)
            {
                if (Main.rand.NextFloat() <= entry.Chance)
                {
                    int stack = Main.rand.Next(entry.MinStack, entry.MaxStack + 1);
                    Item.NewItem(npc.GetSource_Loot(), npc.getRect(), entry.ItemType, stack);
                }
            }

            if (_coinType > 0 && _coinMax > 0)
            {
                int coins = Main.rand.Next(_coinMin, _coinMax + 1);
                if (coins > 0)
                    Item.NewItem(npc.GetSource_Loot(), npc.getRect(), _coinType, coins);
            }
        }

        private struct LootEntry
        {
            public int ItemType;
            public int MinStack;
            public int MaxStack;
            public float Chance;
        }
    }
}