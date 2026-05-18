using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Tokens
{
    /// <summary>
    /// 令牌 — 琉璃楼主令
    /// 八十八角真阳楼的通行令牌，通过墨化可增加角数，提升权限，可进入秘藏阁或吞噬其他楼主令。
    /// </summary>
    public class 琉璃楼主令 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 50000;
        }
    }
}
