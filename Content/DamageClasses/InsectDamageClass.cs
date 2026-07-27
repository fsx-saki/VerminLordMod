using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.DamageClasses
{
    public class InsectDamageClass : DamageClass
    {
        public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
        {
            return StatInheritanceData.Full;
        }

        public override bool GetEffectInheritance(DamageClass damageClass)
        {
            return true;
        }
    }
}
