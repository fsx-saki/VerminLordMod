using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 煮运锅
    /// 七转仙蛊屋（煮运锅），大如水盆，浑身金黄作色，笼罩一层洁白光晕，用于煮运、改变气运。
    /// </summary>
    public class 煮运锅 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
            Item.value = 100000;
        }
    }
}
