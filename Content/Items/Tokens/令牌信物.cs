using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Tokens
{
    /// <summary>
    /// 令牌 — 令牌信物
    /// 陈诚与友人的赌约信物，被黑月仙子出示。
    /// </summary>
    public class 令牌信物 : ModItem
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
