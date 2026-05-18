using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 材料 — 棺材
    /// 古月一代沉睡的血红大棺材，浮于血湖中央。
    /// </summary>
    public class 棺材 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 999;
            Item.value = 500;
        }
    }
}
