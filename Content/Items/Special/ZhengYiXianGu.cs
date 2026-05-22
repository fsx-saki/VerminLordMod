using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 正义仙蛊
    /// 六转（理想情况可升为七转）
    /// </summary>
    public class ZhengYiXianGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Lime;
            Item.maxStack = 1;
            Item.value = 500000;
        }
    }
}
