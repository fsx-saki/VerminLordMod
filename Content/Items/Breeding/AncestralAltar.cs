using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Breeding
{
    public class AncestralAltar : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.value = 5000;
            Item.maxStack = 99;
            Item.rare = Terraria.ID.ItemRarityID.Orange;
        }
    }
}