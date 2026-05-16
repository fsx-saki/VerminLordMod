using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Breeding
{
    public class SpiritPoolBox : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = 1500;
            Item.maxStack = 99;
            Item.rare = Terraria.ID.ItemRarityID.Blue;
        }
    }
}