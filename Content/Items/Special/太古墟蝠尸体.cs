using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 太古墟蝠尸体
    /// 太古荒兽的尸体，曾在太丘战斗中使用，气息能阻挡其他荒兽。
    /// </summary>
    public class 太古墟蝠尸体 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 100000;
        }
    }
}
