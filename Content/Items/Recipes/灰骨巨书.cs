using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Recipes
{
    /// <summary>
    /// 秘方 — 灰骨巨书
    /// 骨制巨书，记载炼蛊秘方、灰骨才子生平及传承考验。
    /// </summary>
    public class 灰骨巨书 : ModItem
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
