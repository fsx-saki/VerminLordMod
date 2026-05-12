using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Common.NPCBehaviors
{
    public abstract class NPCBehaviorHost : ModNPC
    {
        protected List<INPCBehavior> Behaviors { get; } = new();

        private bool _behaviorsRegistered;

        protected abstract void RegisterBehaviors();

        public sealed override void OnSpawn(IEntitySource source)
        {
            if (!_behaviorsRegistered)
            {
                Behaviors.Insert(0, new FreeActionBehavior());
                RegisterBehaviors();
                _behaviorsRegistered = true;
            }
            foreach (var b in Behaviors) b.OnSpawn(NPC);
            OnSpawned(source);
        }

        public sealed override void AI()
        {
            foreach (var b in Behaviors) b.PreAI(NPC);
            OnAI();
            foreach (var b in Behaviors) b.PostAI(NPC);
        }

        public sealed override bool CanChat()
        {
            foreach (var b in Behaviors)
            {
                var r = b.CanChat(NPC);
                if (r.HasValue) return r.Value;
            }
            return true;
        }

        public sealed override string GetChat()
        {
            foreach (var b in Behaviors)
            {
                var chat = b.GetChat(NPC);
                if (!string.IsNullOrEmpty(chat)) return chat;
            }
            return "...";
        }

        public sealed override void SetChatButtons(ref string button, ref string button2)
        {
            foreach (var b in Behaviors)
                b.SetChatButtons(NPC, ref button, ref button2);
        }

        public sealed override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            foreach (var b in Behaviors)
                b.OnChatButtonClicked(NPC, firstButton, ref shop);
        }

        public sealed override bool CanTownNPCSpawn(int numTownNPCs)
        {
            foreach (var b in Behaviors)
            {
                var r = b.CanTownNPCSpawn(NPC, numTownNPCs);
                if (r.HasValue) return r.Value;
            }
            return true;
        }

        public sealed override void AddShops()
        {
            foreach (var b in Behaviors)
                b.AddShops(NPC);
        }

        public sealed override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            foreach (var b in Behaviors)
                b.TownNPCAttackStrength(NPC, ref damage, ref knockback);
        }

        public sealed override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            foreach (var b in Behaviors)
                b.TownNPCAttackCooldown(NPC, ref cooldown, ref randExtraCooldown);
        }

        public sealed override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            foreach (var b in Behaviors)
                b.TownNPCAttackProj(NPC, ref projType, ref attackDelay);
        }

        public sealed override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            foreach (var b in Behaviors)
                b.TownNPCAttackProjSpeed(NPC, ref multiplier, ref gravityCorrection, ref randomOffset);
        }

        public sealed override void OnKill()
        {
            foreach (var b in Behaviors) b.OnKill(NPC);
            OnKilled();
        }

        public sealed override bool? CanBeHitByProjectile(Projectile projectile)
        {
            bool? result = null;
            foreach (var b in Behaviors)
            {
                var r = b.CanBeHitByProjectile(NPC, projectile);
                if (r.HasValue) result = r.Value;
            }
            return result;
        }

        public sealed override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            foreach (var b in Behaviors)
                b.OnHitByItem(NPC, player, item, modifiers);
        }

        protected virtual void OnSpawned(IEntitySource source) { }
        protected virtual void OnAI() { }
        protected virtual void OnKilled() { }
    }
}