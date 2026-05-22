using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 定空仙蛊
    /// 未知（推断七转或八转）
    /// </summary>
    public class DingKongXianGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 1;
            Item.value = 1000000;
        }
    }
}
