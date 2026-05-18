using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Recipes
{
    /// <summary>
    /// 秘方 — 传承印记
    /// 大能传承的关键凭证，疑似被刘青玉所得，方源曾试图击溃它。
    /// </summary>
    public class 传承印记 : ModItem
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
