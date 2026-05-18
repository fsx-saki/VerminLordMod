using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Recipes
{
    /// <summary>
    /// 秘方 — 天难真传
    /// 天难老怪留下的炼道真传，被宋家获得并视为核心传承。
    /// </summary>
    public class 天难真传 : ModItem
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
