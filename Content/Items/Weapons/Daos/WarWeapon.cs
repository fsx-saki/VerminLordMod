
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Items.Weapons.Daos
{
    public abstract class WarWeapon : GuWeaponItem
    {
        protected override int moddustType => ModContent.DustType<WarDust>();
    }
}
