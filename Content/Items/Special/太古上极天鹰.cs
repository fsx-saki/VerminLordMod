using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 太古上极天鹰
    /// 太古荒兽，宇道，八转战力，体型巨大，可比巨鲸，可穿透空间进出洞天和太古九天。方源从黑凡真传中获得幼体，通过宙道杀招催熟。
    /// </summary>
    public class 太古上极天鹰 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 100000;
        }
    }
}
