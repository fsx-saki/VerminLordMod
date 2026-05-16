using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Consumables
{
    public class GuFoodPellet : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.value = 30;
            Item.maxStack = 999;
            Item.rare = Terraria.ID.ItemRarityID.White;
            Item.consumable = true;
        }
    }
}