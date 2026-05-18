using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Recipes
{
    /// <summary>
    /// 秘方 — 白骨秘方
    /// 一种蛊师传承秘方，包括骨枪蛊、螺旋骨枪蛊等，正常售价约六十万元石。
    /// </summary>
    public class 白骨秘方 : ModItem
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
