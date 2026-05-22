using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 智慧蛊
    /// 传说级
    /// </summary>
    public class ZhiHuiGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Red;
            Item.maxStack = 1;
            Item.value = 10000000;
        }
    }
}
