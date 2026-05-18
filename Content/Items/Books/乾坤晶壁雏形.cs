using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Books
{
    /// <summary>
    /// 书籍 — 乾坤晶壁雏形
    /// 天地秘境之一，书道阁收藏的一部分，信道底蕴。
    /// </summary>
    public class 乾坤晶壁雏形 : ModItem
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
