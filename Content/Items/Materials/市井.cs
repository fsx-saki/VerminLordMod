using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 材料 — 市井
    /// 天地秘境，方源用于天网恢恢杀招的防御核心。
    /// </summary>
    public class 市井 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 999;
            Item.value = 500;
        }
    }
}
