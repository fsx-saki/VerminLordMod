using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Seeds
{
    public class BoneBanbooSeed : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.value = 90;
            Item.maxStack = 999;
            Item.rare = Terraria.ID.ItemRarityID.Blue;
        }
    }
}