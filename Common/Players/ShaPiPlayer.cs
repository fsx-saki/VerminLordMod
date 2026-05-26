using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Common.Players
{
    public class ShaPiPlayer : ModPlayer
    {
        public bool HasShaPiEquipped { get; set; }

        public override void ResetEffects()
        {
            HasShaPiEquipped = false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (HasShaPiEquipped && modifiers.DamageSource.TryGetCausingEntity(out Entity source) && source is NPC attacker)
            {
                int thornsDamage = (int)(modifiers.FinalDamage.Base * 0.15f);
                if (thornsDamage > 0 && attacker.active)
                {
                    attacker.SimpleStrikeNPC(thornsDamage, 0);
                }
            }
        }
    }
}
