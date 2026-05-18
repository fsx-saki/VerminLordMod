using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 仙材 — 斑斓霸王花
    /// 七转仙材，慧剑蛊的食料，巨大花朵，覆盖方圆千里，需光照、甜气、珍珠土。
    /// </summary>
    public class 斑斓霸王花 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 99;
            Item.value = 50000;
        }
    }
}
