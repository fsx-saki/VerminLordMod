using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 仙材 — 流光果
    /// 六转仙材，由浓郁极光凝结而成，颜色多样，需特殊种植。
    /// </summary>
    public class 流光果 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 99;
            Item.value = 50000;
        }
    }
}
