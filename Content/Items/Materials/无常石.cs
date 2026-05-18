using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 仙材 — 无常石
    /// 六转仙材，半黑半白，形成于常年征战之地，可用于炼器或修炼。
    /// </summary>
    public class 无常石 : ModItem
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
