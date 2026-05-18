using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 材料 — 星意
    /// 东方长凡死后留下的意志体，拥有本体记忆和部分能力，可操控阵法。
    /// </summary>
    public class 星意 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 999;
            Item.value = 500;
        }
    }
}
