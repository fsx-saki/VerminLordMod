using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Common.Players
{
    public class TieGuiPlayer : ModPlayer
    {
        public bool HasTieGuiEquipped { get; set; }

        public override void ResetEffects()
        {
            HasTieGuiEquipped = false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (HasTieGuiEquipped && Main.rand.NextFloat() < 0.2f)
            {
                modifiers.FinalDamage *= 0f;
                Player.immuneTime = (int)(60);
            }
        }
    }
}
