using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 逆命祭炼大阵
    /// 大雪山福地中的阵法，用于炼制鸿运齐天仙蛊，后被子阵影响崩溃。
    /// </summary>
    public class 逆命祭炼大阵 : ModItem
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
