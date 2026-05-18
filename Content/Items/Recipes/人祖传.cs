using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Recipes
{
    /// <summary>
    /// 秘方 — 人祖传
    /// 蛊师世界第一真传，蕴含人道奥义，多位仙尊魔尊从中获益。
    /// </summary>
    public class 人祖传 : ModItem
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
