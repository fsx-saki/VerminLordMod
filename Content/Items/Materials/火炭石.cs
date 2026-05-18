using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 矿石 — 火炭石
    /// 火炭山特产矿石，燃烧长久、热量不高、无烟雾，用于商家城燃料。
    /// </summary>
    public class 火炭石 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 999;
            Item.value = 2000;
        }
    }
}
