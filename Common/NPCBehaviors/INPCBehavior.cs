using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Common.NPCBehaviors
{
    public interface INPCBehavior
    {
        string Name { get; }

        void OnSpawn(NPC npc);
        void PreAI(NPC npc);
        void PostAI(NPC npc);
        bool? CanChat(NPC npc);
        string GetChat(NPC npc);
        void SetChatButtons(NPC npc, ref string button, ref string button2);
        void OnChatButtonClicked(NPC npc, bool firstButton, ref string shop);
        bool? CanTownNPCSpawn(NPC npc, int numTownNPCs);
        void AddShops(NPC npc);
        void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback);
        void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown);
        void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay);
        void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset);
        void OnKill(NPC npc);
        bool? CanBeHitByProjectile(NPC npc, Projectile projectile);
        void OnHitByItem(NPC npc, Player player, Item item, NPC.HitModifiers modifiers);
    }
}