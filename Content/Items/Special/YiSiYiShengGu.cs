using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 一死一生蛊
    /// 未知（可能五转以上）
    /// </summary>
    public class YiSiYiShengGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
            Item.value = 50000;
        }
    }
}
