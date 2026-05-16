using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Breeding
{
    public class BattleArena : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.value = 3000;
            Item.maxStack = 99;
            Item.rare = Terraria.ID.ItemRarityID.Green;
        }
    }
}