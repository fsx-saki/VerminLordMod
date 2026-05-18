using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 荒兽资源 — 魅蓝电影
    /// 一种由蓝色闪电构成的荒兽，能攻击天地造成福地漏洞。被方源设计驱除出福地。
    /// </summary>
    public class 魅蓝电影 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 10;
            Item.value = 20000;
        }
    }
}
