using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 仙材 — 仙元石
    /// 蛊仙交易货币，天庭量产。可快速补充仙元。目前天庭减少发放，导致五域仙元石紧缺。
    /// </summary>
    public class 仙元石 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 99;
            Item.value = 50000;
        }
    }
}
