
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Items.Weapons.Daos
{
    public abstract class IceSnowWeapon : GuWeaponItem
    {
        protected override int moddustType => ModContent.DustType<IceSnowDust>();
    }
}
