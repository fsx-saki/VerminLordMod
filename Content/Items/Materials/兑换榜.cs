using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 材料 — 兑换榜
    /// 至尊仙窍中的三大榜单之一，列出可用贡献兑换的修行资源，极其丰富。
    /// </summary>
    public class 兑换榜 : ModItem
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
