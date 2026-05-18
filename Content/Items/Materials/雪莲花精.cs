using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 仙材 — 雪莲花精
    /// 七转仙材，形似雪莲花，表面覆盖冰层，蕴含生机与寒意，产自雪人部族。
    /// </summary>
    public class 雪莲花精 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 99;
            Item.value = 50000;
        }
    }
}
