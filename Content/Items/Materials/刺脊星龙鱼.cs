using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 荒兽资源 — 刺脊星龙鱼
    /// 普通龙鱼及六转荒兽龙鱼，肉质应用广泛，可做食料和辅助喂养蛊虫。数量巨大，有近百万条。
    /// </summary>
    public class 刺脊星龙鱼 : ModItem
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
