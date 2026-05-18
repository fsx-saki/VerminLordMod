using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 荒兽资源 — 天晶
    /// 材料，用于催熟上极天鹰，大量消耗。
    /// </summary>
    public class 天晶 : ModItem
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
