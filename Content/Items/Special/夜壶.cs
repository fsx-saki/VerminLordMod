using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 夜壶
    /// 萧夜壶手中的暗道仙蛊屋，袖珍，能将蛊仙强行摄入其中，困上一夜化为一滩屎尿。
    /// </summary>
    public class 夜壶 : ModItem
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
