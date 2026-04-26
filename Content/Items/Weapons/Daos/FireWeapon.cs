
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Items.Weapons.Daos
{
    public abstract class FireWeapon : GuWeaponItem
    {
        protected override int moddustType => ModContent.DustType<FireDust>();
    }
}
