using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Common.NPCBehaviors
{
    public abstract class BaseNPCBehavior : INPCBehavior
    {
        public abstract string Name { get; }

        public virtual void OnSpawn(NPC npc) { }
        public virtual void PreAI(NPC npc) { }
        public virtual void PostAI(NPC npc) { }
        public virtual bool? CanChat(NPC npc) => null;
        public virtual string GetChat(NPC npc) => null;
        public virtual void SetChatButtons(NPC npc, ref string button, ref string button2) { }
        public virtual void OnChatButtonClicked(NPC npc, bool firstButton, ref string shop) { }
        public virtual bool? CanTownNPCSpawn(NPC npc, int numTownNPCs) => null;
        public virtual void AddShops(NPC npc) { }
        public virtual void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback) { }
        public virtual void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown) { }
        public virtual void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay) { }
        public virtual void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset) { }
        public virtual void OnKill(NPC npc) { }
        public virtual bool? CanBeHitByProjectile(NPC npc, Projectile projectile) => null;
        public virtual void OnHitByItem(NPC npc, Player player, Item item, NPC.HitModifiers modifiers) { }
    }
}