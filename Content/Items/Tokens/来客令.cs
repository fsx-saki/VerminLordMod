using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Tokens
{
    /// <summary>
    /// 令牌 — 来客令
    /// 进入八十八角真阳楼的令牌，只能使用一次，非黄金血脉者需要此令。
    /// </summary>
    public class 来客令 : ModItem
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
