using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.Items
{
    public abstract class GuWeaponItem : GuBaseItem
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.useTime = 20; Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true; Item.useTurn = true; Item.noMelee = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2) return false;
            return true;
        }
    }
}
