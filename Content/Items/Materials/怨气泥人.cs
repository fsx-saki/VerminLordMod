using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 荒兽资源 — 怨气泥人
    /// 戚灾用怨气、荒兽泥怪尸躯和气道手段制成的泥人怪物，具有荒兽级战力，心智简单。
    /// </summary>
    public class 怨气泥人 : ModItem
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
