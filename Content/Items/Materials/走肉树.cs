using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 植物 — 走肉树
    /// 上古荒植，力道植株，枝条如章鱼触手，能攻击
    /// </summary>
    public class 走肉树 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 99;
            Item.value = 3000;
        }
    }
}
