using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 搬拦亭
    /// 七转力道仙蛊屋，形如鸟笼，八根蓝金大柱弯曲。苏家所有，用于深海开采。
    /// </summary>
    public class 搬拦亭 : ModItem
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
