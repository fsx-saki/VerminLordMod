using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 荒兽资源 — 星夜黏涎
    /// 地壳蜗牛分泌的黏涎，采集过程需用星光处理，是方源所需的重要材料。
    /// </summary>
    public class 星夜黏涎 : ModItem
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
