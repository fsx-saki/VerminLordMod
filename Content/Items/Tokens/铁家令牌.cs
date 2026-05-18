using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Tokens
{
    /// <summary>
    /// 令牌 — 铁家令牌
    /// 三王传承中的保命道具，可传送出福地。
    /// </summary>
    public class 铁家令牌 : ModItem
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
