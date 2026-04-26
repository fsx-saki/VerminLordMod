
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Items.Weapons.Daos
{
    public abstract class WindWeapon : GuWeaponItem
    {
        protected override int moddustType => ModContent.DustType<WindDust>();
    }
}
