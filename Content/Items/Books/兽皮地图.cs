using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Books
{
    /// <summary>
    /// 书籍 — 兽皮地图
    /// 王家祖传的白色兽皮地图，详细记录了青茅山的地形、陷阱位置和野兽分布，是猎人的传家宝。
    /// </summary>
    public class 兽皮地图 : ModItem
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
