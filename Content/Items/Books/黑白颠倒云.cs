using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Books
{
    /// <summary>
    /// 书籍 — 黑白颠倒云
    /// 特殊云朵，连通黑天与白天，可让生物随机传送。
    /// </summary>
    public class 黑白颠倒云 : ModItem
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
