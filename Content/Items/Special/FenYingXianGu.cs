using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 分影仙蛊
    /// 六转（推测）
    /// </summary>
    public class FenYingXianGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 100000;
        }
    }
}
