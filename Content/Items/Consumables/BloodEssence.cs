using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Consumables
{
    public class BloodEssence : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.value = 200;
            Item.maxStack = 999;
            Item.rare = Terraria.ID.ItemRarityID.Green;
            Item.consumable = true;
        }
    }
}