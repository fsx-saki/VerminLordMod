using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;
using VerminLordMod.Content.Items.Weapons;

namespace VerminLordMod.Content.Items.Weapons.Daos
{
    public abstract class UnrealWeapon : GuWeaponItem
    {
        protected override int moddustType => ModContent.DustType<UnrealDust>();
    }
}
