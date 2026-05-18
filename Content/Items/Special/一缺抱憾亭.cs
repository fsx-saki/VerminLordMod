using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 一缺抱憾亭
    /// 八转仙蛊屋，位于星驰山顶峰，星宿仙尊与无极魔尊对弈之地。
    /// </summary>
    public class 一缺抱憾亭 : ModItem
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
