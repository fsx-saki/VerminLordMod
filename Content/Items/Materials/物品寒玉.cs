using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 仙材 — 物品:寒玉
    /// 五转蛊材，含寒玉髓可作六转仙材，是广寒峰的主要资源。
    /// </summary>
    public class 物品寒玉 : ModItem
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
