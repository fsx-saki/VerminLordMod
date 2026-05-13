using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.NPCs.GuYue;

namespace VerminLordMod.Common.DialogueTree
{
    public class StoryNPCComponent : GlobalNPC
    {
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return entity.ModNPC is GuYueNPCBase || entity.ModNPC is GuYueVillager;
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Player player = Main.LocalPlayer;
            if (player == null || !player.active)
                return;

            BindStoryToNPC(npc, player);
        }

        public override bool? CanChat(NPC npc)
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active)
                return base.CanChat(npc);

            if (!DialogueTreeManager.Instance.HasTree(npc))
            {
                BindStoryToNPC(npc, player);
            }

            return base.CanChat(npc);
        }

        private void BindStoryToNPC(NPC npc, Player player)
        {
            var storyManager = StoryManager.Instance;
            var provider = storyManager.GetStoryForNPC(player, npc);

            if (provider != null)
            {
                provider.BindToNPC(npc);
                var tree = provider.GetDialogueTree(player);
                if (tree != null)
                {
                    DialogueTreeManager.Instance.RegisterTree(tree);
                }
            }
        }
    }
}