using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Tokens
{
    /// <summary>
    /// 令牌 — 通缉令
    /// 百家发布的追捕方白二人的文书，消息价一千元石，缉杀价五千八百元石。
    /// </summary>
    public class 通缉令 : ModItem
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
