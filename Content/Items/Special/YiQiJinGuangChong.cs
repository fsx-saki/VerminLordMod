using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 一气金光虫
    /// 一转（推测）
    /// </summary>
    public class YiQiJinGuangChong : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 1;
            Item.value = 1000;
        }
    }
}
