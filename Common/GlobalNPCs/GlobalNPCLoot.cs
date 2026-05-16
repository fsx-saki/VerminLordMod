using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.One;

namespace VerminLordMod.Common.GlobalNPCs
{
    class GlobalNPCLoot : GlobalNPC
    {
        private static bool _cacheBuilt = false;

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (!_cacheBuilt)
            {
                GuDropRegistry.BuildCache();
                _cacheBuilt = true;
            }

            if (npc.boss)
            {
                foreach (var drop in GuDropRegistry.BossCommonDrops)
                    npcLoot.Add(ItemDropRule.Common(drop.ItemType, drop.ChanceDenominator));
            }

            if (GuDropRegistry.TryGetBossDrops(npc.type, out var bossDrops))
            {
                foreach (var drop in bossDrops)
                    npcLoot.Add(ItemDropRule.Common(drop.ItemType, drop.ChanceDenominator, drop.MinStack, drop.MaxStack));
            }

            if (GuDropRegistry.IsInEarlyBossGroup(npc.type))
            {
                foreach (var group in GuDropRegistry.EarlyBossGroupDrops)
                {
                    foreach (int t in group.NpcTypes)
                    {
                        if (t == npc.type)
                        {
                            foreach (var drop in group.Drops)
                                npcLoot.Add(ItemDropRule.Common(drop.ItemType, drop.ChanceDenominator, drop.MinStack, drop.MaxStack));
                            break;
                        }
                    }
                }
            }

            if (GuDropRegistry.TryGetDrops(npc.type, out List<GuDropEntry> drops))
            {
                foreach (var drop in drops)
                    npcLoot.Add(ItemDropRule.Common(drop.ItemType, drop.ChanceDenominator, drop.MinStack, drop.MaxStack));
            }

            if (npc.townNPC)
                npcLoot.Add(ItemDropRule.Common(ItemID.FleshBlock, 1, 1, 7));

            foreach (var drop in GuDropRegistry.UniversalDrops)
                npcLoot.Add(ItemDropRule.Common(drop.ItemType, drop.ChanceDenominator));
        }
    }
}
