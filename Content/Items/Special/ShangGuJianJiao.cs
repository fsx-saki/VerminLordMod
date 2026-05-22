using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 上古剑蛟
    /// 七转（变化形态）
    /// </summary>
    public class ShangGuJianJiao : ModItem
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
