using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Seeds
{
    public class SpiritSpringPlantSeed : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.value = 150;
            Item.maxStack = 999;
            Item.rare = Terraria.ID.ItemRarityID.Green;
        }
    }
}